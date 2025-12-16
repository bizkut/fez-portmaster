// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.OggStream
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Services;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

#nullable disable
namespace FezEngine.Structure;

public class OggStream : IDisposable
{
  private const int MAX_SAMPLES = 192000;
  private static byte[] vorbisBuffer = new byte[192000];
  private static GCHandle bufferHandle = GCHandle.Alloc((object) OggStream.vorbisBuffer, GCHandleType.Pinned);
  private static IntPtr bufferPtr = OggStream.bufferHandle.AddrOfPinnedObject();
  private static readonly Dictionary<int, OggStream> Streams = new Dictionary<int, OggStream>();
  private static int NextStreamId = 1;
  private static Vorbisfile.ov_callbacks VorbisCallbacks = new Vorbisfile.ov_callbacks()
  {
    read_func = new Vorbisfile.read_func(OggStream.ReadCallback),
    close_func = new Vorbisfile.close_func(OggStream.CloseCallback),
    tell_func = new Vorbisfile.tell_func(OggStream.TellCallback),
    seek_func = new Vorbisfile.seek_func(OggStream.SeekCallback)
  };
  private DynamicSoundEffectInstance soundEffect;
  private int streamId;
  private MemoryStream memoryStream;
  private long streamOffset;
  private GCHandle streamHandle;
  private IntPtr vorbisFile;
  private readonly object PrecacheLock = new object();
  private static readonly ConcurrentQueue<OggStream> ToPrecache = new ConcurrentQueue<OggStream>();
  private static readonly AutoResetEvent WakeUpPrecacher = new AutoResetEvent(false);
  private static Thread ThreadedPrecacher;
  private static bool PrecacherAborted;
  private bool hitEof;
  private float volume;
  private float globalVolume = 1f;
  private string category;

  public string Name { get; private set; }

  public string RealName { get; set; }

  private OggStream()
  {
    ServiceHelper.InjectServices((object) this);
    this.streamId = OggStream.NextStreamId;
    OggStream.Streams.Add(this.streamId, this);
    if (OggStream.NextStreamId == int.MaxValue)
      OggStream.NextStreamId = 0;
    else
      ++OggStream.NextStreamId;
    this.LowPass = true;
  }

  public OggStream(MemoryStream stream)
    : this()
  {
    this.memoryStream = stream;
    this.streamHandle = GCHandle.Alloc((object) stream.GetBuffer(), GCHandleType.Pinned);
    Vorbisfile.ov_open_callbacks(new IntPtr(this.streamId), out this.vorbisFile, IntPtr.Zero, IntPtr.Zero, OggStream.VorbisCallbacks);
    this.Initialize();
  }

  public OggStream(string filename)
    : this()
  {
    this.Name = filename;
    Vorbisfile.ov_fopen(filename, out this.vorbisFile);
    this.Initialize();
  }

  private void Initialize()
  {
    Vorbisfile.vorbis_info vorbisInfo = Vorbisfile.ov_info(this.vorbisFile, -1);
    this.soundEffect = new DynamicSoundEffectInstance((int) vorbisInfo.rate, vorbisInfo.channels == 1 ? AudioChannels.Mono : AudioChannels.Stereo);
    this.Volume = 1f;
    OggStream.ToPrecache.Enqueue(this);
    if (OggStream.ThreadedPrecacher == null)
    {
      OggStream.ThreadedPrecacher = new Thread(new ThreadStart(OggStream.PrecacheStreams))
      {
        Priority = ThreadPriority.Lowest
      };
      OggStream.ThreadedPrecacher.Start();
    }
    OggStream.WakeUpPrecacher.Set();
  }

  public static void AbortPrecacher()
  {
    OggStream.PrecacherAborted = true;
    OggStream.WakeUpPrecacher.Set();
  }

  private static void PrecacheStreams()
  {
    while (!OggStream.PrecacherAborted)
    {
      OggStream.Precache();
      OggStream.WakeUpPrecacher.WaitOne();
    }
  }

  private static void Precache()
  {
label_13:
    OggStream result;
    while (OggStream.ToPrecache.TryDequeue(out result))
    {
      lock (result.PrecacheLock)
      {
        while (true)
        {
          if (result.soundEffect != null)
          {
            if (!result.soundEffect.IsDisposed)
            {
              if (result.vorbisFile != IntPtr.Zero)
              {
                if (result.QueuedBuffers < 3)
                {
                  if (!result.hitEof)
                    result.QueueBuffer((object) null, EventArgs.Empty);
                  else
                    goto label_13;
                }
                else
                  goto label_13;
              }
              else
                goto label_13;
            }
            else
              goto label_13;
          }
          else
            goto label_13;
        }
      }
    }
  }

  private static unsafe IntPtr ReadCallback(
    IntPtr ptr,
    IntPtr size,
    IntPtr nmemb,
    IntPtr datasource)
  {
    OggStream oggStream;
    if (!OggStream.Streams.TryGetValue(datasource.ToInt32(), out oggStream))
      return new IntPtr(0);
    byte* source = (byte*) ((IntPtr) oggStream.streamHandle.AddrOfPinnedObject().ToPointer() + (IntPtr) oggStream.streamOffset);
    byte* pointer = (byte*) ptr.ToPointer();
    int val2 = nmemb.ToInt32() * size.ToInt32();
    int num = Math.Min((int) (oggStream.memoryStream.Length - oggStream.streamOffset), val2);
    OggStream.memcpy(pointer, source, (IntPtr) num);
    oggStream.streamOffset += (long) num;
    return new IntPtr(num);
  }

  [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
  private static extern unsafe IntPtr memcpy(byte* destination, byte* source, IntPtr num);

  private static int SeekCallback(IntPtr datasource, long offset, Vorbisfile.SeekWhence whence)
  {
    OggStream oggStream;
    if (!OggStream.Streams.TryGetValue(datasource.ToInt32(), out oggStream))
      return -1;
    switch (whence)
    {
      case Vorbisfile.SeekWhence.SEEK_SET:
        oggStream.streamOffset = offset;
        break;
      case Vorbisfile.SeekWhence.SEEK_CUR:
        oggStream.streamOffset += offset;
        break;
      case Vorbisfile.SeekWhence.SEEK_END:
        oggStream.streamOffset = oggStream.memoryStream.Length;
        break;
    }
    return 0;
  }

  private static long TellCallback(IntPtr datasource)
  {
    OggStream oggStream;
    return !OggStream.Streams.TryGetValue(datasource.ToInt32(), out oggStream) ? 0L : oggStream.streamOffset;
  }

  private static int CloseCallback(IntPtr datasource)
  {
    OggStream oggStream;
    if (!OggStream.Streams.TryGetValue(datasource.ToInt32(), out oggStream))
      return 0;
    oggStream.streamOffset = 0L;
    return 0;
  }

  ~OggStream() => this.Dispose(true);

  public bool IsDisposed { get; private set; }

  public void Dispose()
  {
    this.Dispose(true);
    GC.SuppressFinalize((object) this);
  }

  private void Dispose(bool disposing)
  {
    if (disposing)
    {
      this.Stop();
      lock (this.PrecacheLock)
      {
        if (this.soundEffect != null)
        {
          this.soundEffect.Dispose();
          this.soundEffect = (DynamicSoundEffectInstance) null;
        }
        Vorbisfile.ov_clear(ref this.vorbisFile);
      }
      if (this.memoryStream != null)
      {
        this.streamHandle.Free();
        this.memoryStream = (MemoryStream) null;
      }
      OggStream.Streams.Remove(this.streamId);
    }
    this.IsDisposed = true;
  }

  public bool LowPass { get; set; }

  public void Play()
  {
    this.soundEffect.BufferNeeded += new EventHandler<EventArgs>(this.OnBufferNeeded);
    while (this.soundEffect.PendingBufferCount == 0)
      Thread.Yield();
    if (this.soundEffect.State == SoundState.Paused)
    {
      this.soundEffect.Resume();
    }
    else
    {
      this.soundEffect.Play();
      (this.SoundManager as FezEngine.Services.SoundManager).RegisterLowPass((SoundEffectInstance) this.soundEffect);
    }
  }

  private void OnBufferNeeded(object sender, EventArgs e)
  {
    OggStream.ToPrecache.Enqueue(this);
    OggStream.WakeUpPrecacher.Set();
  }

  private void QueueBuffer(object source, EventArgs ea)
  {
    int count = 0;
    int num;
    do
    {
      num = (int) Vorbisfile.ov_read(this.vorbisFile, OggStream.bufferPtr + count, 4096 /*0x1000*/, 0, 2, 1, out int _);
      count += num;
    }
    while (num > 0 && count < 187904);
    if (count == 0)
    {
      if (this.IsLooped)
      {
        Vorbisfile.ov_time_seek(this.vorbisFile, 0.0);
        this.QueueBuffer(source, ea);
      }
      else
      {
        this.hitEof = true;
        this.soundEffect.BufferNeeded -= new EventHandler<EventArgs>(this.OnBufferNeeded);
      }
    }
    else
      this.soundEffect.SubmitBuffer(OggStream.vorbisBuffer, 0, count);
  }

  public void Pause() => this.soundEffect.Pause();

  public void Resume() => this.soundEffect.Resume();

  public void Stop()
  {
    if (this.soundEffect == null)
      return;
    this.soundEffect.Stop();
    this.soundEffect.BufferNeeded -= new EventHandler<EventArgs>(this.OnBufferNeeded);
  }

  public float Volume
  {
    get => this.volume;
    set
    {
      float num = (double) SoundEffect.MasterVolume == 0.0 ? 1f : 1f / SoundEffect.MasterVolume;
      this.soundEffect.Volume = MathHelper.Clamp((this.volume = value) * this.globalVolume, 0.0f, 1f) * num;
    }
  }

  public float GlobalVolume
  {
    set
    {
      this.globalVolume = value;
      this.Volume = this.volume;
    }
  }

  public string Category
  {
    get => this.category;
    set
    {
      this.category = value;
      this.SyncVolume();
    }
  }

  private void SyncVolume()
  {
    if (this.category == "Ambience")
      this.GlobalVolume = SoundEffect.MasterVolume * this.SoundManager.GlobalVolumeFactor * this.SoundManager.MusicVolumeFactor;
    else
      this.GlobalVolume = FezMath.Saturate(Easing.EaseIn((double) this.SoundManager.MusicVolume * (double) this.SoundManager.GlobalVolumeFactor, EasingType.Quadratic) * this.SoundManager.MusicVolumeFactor);
  }

  public static void SyncAllVolume()
  {
    foreach (OggStream oggStream in OggStream.Streams.Values)
      oggStream.SyncVolume();
  }

  public static void SyncAllFilter(MethodInfo applyFilter, object[] gainContainer)
  {
    foreach (OggStream oggStream in OggStream.Streams.Values)
    {
      if (oggStream.LowPass)
        applyFilter.Invoke((object) oggStream.soundEffect, gainContainer);
    }
  }

  public bool IsLooped { get; set; }

  public bool IsStopped
  {
    get => this.soundEffect.State == SoundState.Stopped || this.soundEffect.PendingBufferCount == 0;
  }

  public bool IsPlaying => this.soundEffect.State == SoundState.Playing;

  public int QueuedBuffers => this.soundEffect.PendingBufferCount;

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }
}
