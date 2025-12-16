// Decompiled with JetBrains decompiler
// Type: FezGame.Components.BitDoorState
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components;

internal class BitDoorState
{
  private readonly Vector3[] SixtyFourOffsets = new Vector3[64 /*0x40*/]
  {
    new Vector3(8f, 52f, 0.0f),
    new Vector3(12f, 52f, 0.0f),
    new Vector3(8f, 48f, 0.0f),
    new Vector3(12f, 48f, 0.0f),
    new Vector3(0.0f, 44f, 0.0f),
    new Vector3(4f, 44f, 0.0f),
    new Vector3(8f, 44f, 0.0f),
    new Vector3(12f, 44f, 0.0f),
    new Vector3(16f, 44f, 0.0f),
    new Vector3(20f, 44f, 0.0f),
    new Vector3(0.0f, 40f, 0.0f),
    new Vector3(4f, 40f, 0.0f),
    new Vector3(8f, 40f, 0.0f),
    new Vector3(12f, 40f, 0.0f),
    new Vector3(16f, 40f, 0.0f),
    new Vector3(20f, 40f, 0.0f),
    new Vector3(0.0f, 36f, 0.0f),
    new Vector3(4f, 36f, 0.0f),
    new Vector3(8f, 36f, 0.0f),
    new Vector3(12f, 36f, 0.0f),
    new Vector3(16f, 36f, 0.0f),
    new Vector3(20f, 36f, 0.0f),
    new Vector3(0.0f, 32f, 0.0f),
    new Vector3(4f, 32f, 0.0f),
    new Vector3(8f, 32f, 0.0f),
    new Vector3(12f, 32f, 0.0f),
    new Vector3(16f, 32f, 0.0f),
    new Vector3(20f, 32f, 0.0f),
    new Vector3(0.0f, 28f, 0.0f),
    new Vector3(4f, 28f, 0.0f),
    new Vector3(16f, 28f, 0.0f),
    new Vector3(20f, 28f, 0.0f),
    new Vector3(0.0f, 24f, 0.0f),
    new Vector3(4f, 24f, 0.0f),
    new Vector3(16f, 24f, 0.0f),
    new Vector3(20f, 24f, 0.0f),
    new Vector3(0.0f, 20f, 0.0f),
    new Vector3(4f, 20f, 0.0f),
    new Vector3(8f, 20f, 0.0f),
    new Vector3(12f, 20f, 0.0f),
    new Vector3(16f, 20f, 0.0f),
    new Vector3(20f, 20f, 0.0f),
    new Vector3(0.0f, 16f, 0.0f),
    new Vector3(4f, 16f, 0.0f),
    new Vector3(8f, 16f, 0.0f),
    new Vector3(12f, 16f, 0.0f),
    new Vector3(16f, 16f, 0.0f),
    new Vector3(20f, 16f, 0.0f),
    new Vector3(0.0f, 12f, 0.0f),
    new Vector3(4f, 12f, 0.0f),
    new Vector3(8f, 12f, 0.0f),
    new Vector3(12f, 12f, 0.0f),
    new Vector3(16f, 12f, 0.0f),
    new Vector3(20f, 12f, 0.0f),
    new Vector3(0.0f, 8f, 0.0f),
    new Vector3(4f, 8f, 0.0f),
    new Vector3(8f, 8f, 0.0f),
    new Vector3(12f, 8f, 0.0f),
    new Vector3(16f, 8f, 0.0f),
    new Vector3(20f, 8f, 0.0f),
    new Vector3(8f, 4f, 0.0f),
    new Vector3(12f, 4f, 0.0f),
    new Vector3(8f, 0.0f, 0.0f),
    new Vector3(12f, 0.0f, 0.0f)
  };
  private readonly Vector3[] ThirtyTwoOffsets = new Vector3[32 /*0x20*/]
  {
    new Vector3(0.0f, 2.625f, 0.0f),
    new Vector3(0.375f, 2.625f, 0.0f),
    new Vector3(0.75f, 2.625f, 0.0f),
    new Vector3(1.125f, 2.625f, 0.0f),
    new Vector3(0.0f, 2.25f, 0.0f),
    new Vector3(0.375f, 2.25f, 0.0f),
    new Vector3(0.75f, 2.25f, 0.0f),
    new Vector3(1.125f, 2.25f, 0.0f),
    new Vector3(0.0f, 1.875f, 0.0f),
    new Vector3(0.375f, 1.875f, 0.0f),
    new Vector3(0.75f, 1.875f, 0.0f),
    new Vector3(1.125f, 1.875f, 0.0f),
    new Vector3(0.0f, 1.5f, 0.0f),
    new Vector3(0.375f, 1.5f, 0.0f),
    new Vector3(0.75f, 1.5f, 0.0f),
    new Vector3(1.125f, 1.5f, 0.0f),
    new Vector3(0.0f, 1.125f, 0.0f),
    new Vector3(0.375f, 1.125f, 0.0f),
    new Vector3(0.75f, 1.125f, 0.0f),
    new Vector3(1.125f, 1.125f, 0.0f),
    new Vector3(0.0f, 0.75f, 0.0f),
    new Vector3(0.375f, 0.75f, 0.0f),
    new Vector3(0.75f, 0.75f, 0.0f),
    new Vector3(1.125f, 0.75f, 0.0f),
    new Vector3(0.0f, 0.375f, 0.0f),
    new Vector3(0.375f, 0.375f, 0.0f),
    new Vector3(0.75f, 0.375f, 0.0f),
    new Vector3(1.125f, 0.375f, 0.0f),
    new Vector3(0.0f, 0.0f, 0.0f),
    new Vector3(0.375f, 0.0f, 0.0f),
    new Vector3(0.75f, 0.0f, 0.0f),
    new Vector3(1.125f, 0.0f, 0.0f)
  };
  private readonly Vector3[] SixteenOffsets = new Vector3[16 /*0x10*/]
  {
    new Vector3(0.5f, 3f, 0.0f),
    new Vector3(0.0f, 2.5f, 0.0f),
    new Vector3(0.5f, 2.5f, 0.0f),
    new Vector3(1f, 2.5f, 0.0f),
    new Vector3(0.0f, 2f, 0.0f),
    new Vector3(0.5f, 2f, 0.0f),
    new Vector3(1f, 2f, 0.0f),
    new Vector3(0.0f, 1.5f, 0.0f),
    new Vector3(1f, 1.5f, 0.0f),
    new Vector3(0.0f, 1f, 0.0f),
    new Vector3(0.5f, 1f, 0.0f),
    new Vector3(1f, 1f, 0.0f),
    new Vector3(0.0f, 0.5f, 0.0f),
    new Vector3(0.5f, 0.5f, 0.0f),
    new Vector3(1f, 0.5f, 0.0f),
    new Vector3(0.5f, 0.0f, 0.0f)
  };
  private readonly Vector3[] EightOffsets = new Vector3[8]
  {
    new Vector3(0.5f, 1.5f, 0.0f),
    new Vector3(0.0f, 1f, 0.0f),
    new Vector3(0.5f, 1f, 0.0f),
    new Vector3(1f, 1f, 0.0f),
    new Vector3(0.0f, 0.5f, 0.0f),
    new Vector3(0.5f, 0.5f, 0.0f),
    new Vector3(1f, 0.5f, 0.0f),
    new Vector3(0.5f, 0.0f, 0.0f)
  };
  private readonly Vector3[] FourOffsets = new Vector3[4]
  {
    new Vector3(0.0f, 1.75f, 0.0f),
    new Vector3(0.0f, 1.25f, 0.0f),
    new Vector3(0.0f, 0.75f, 0.0f),
    new Vector3(0.0f, 0.25f, 0.0f)
  };
  private static readonly TimeSpan DoorShakeTime = TimeSpan.FromSeconds(0.5);
  private static readonly TimeSpan DoorOpenTime = TimeSpan.FromSeconds(3.0);
  private readonly Viewpoint ExpectedViewpoint;
  private readonly SoundEffect RumbleSound;
  private readonly SoundEffect sLightUp;
  private readonly SoundEffect sFadeOut;
  private Texture2D BitTexture;
  private Texture2D AntiBitTexture;
  public readonly ArtObjectInstance AoInstance;
  private readonly List<BackgroundPlane> BitPlanes = new List<BackgroundPlane>();
  private bool close;
  private bool opening;
  private TimeSpan sinceClose;
  private TimeSpan sinceMoving;
  private Vector3 doorOrigin;
  private Vector3 doorDestination;
  private SoundEmitter rumbleEmitter;
  private int lastBits;

  public BitDoorState(ArtObjectInstance artObject)
  {
    ServiceHelper.InjectServices((object) this);
    this.AoInstance = artObject;
    switch (artObject.ArtObject.ActorType)
    {
      case ActorType.EightBitDoor:
      case ActorType.OneBitDoor:
      case ActorType.FourBitDoor:
      case ActorType.TwoBitDoor:
      case ActorType.SixteenBitDoor:
        DrawActionScheduler.Schedule((Action) (() =>
        {
          this.BitTexture = this.CMProvider.Global.Load<Texture2D>("Other Textures/glow/GLOWBIT");
          this.AntiBitTexture = this.CMProvider.Global.Load<Texture2D>("Other Textures/glow/GLOWBIT_anti");
        }));
        break;
      case ActorType.ThirtyTwoBitDoor:
        DrawActionScheduler.Schedule((Action) (() =>
        {
          this.BitTexture = this.CMProvider.Global.Load<Texture2D>("Other Textures/glow/small_glowbit");
          this.AntiBitTexture = this.CMProvider.Global.Load<Texture2D>("Other Textures/glow/small_glowbit_anti");
        }));
        break;
      default:
        DrawActionScheduler.Schedule((Action) (() =>
        {
          this.BitTexture = this.CMProvider.Global.Load<Texture2D>("Other Textures/glow/code_machine_glowbit");
          this.AntiBitTexture = this.CMProvider.Global.Load<Texture2D>("Other Textures/glow/code_machine_glowbit_anti");
        }));
        for (int index = 0; index < 64 /*0x40*/; ++index)
          this.SixtyFourOffsets[index] /= 16f;
        break;
    }
    this.RumbleSound = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/MiscActors/Rumble");
    this.sLightUp = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Zu/DoorBitLightUp");
    this.sFadeOut = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Zu/DoorBitFadeOut");
    this.ExpectedViewpoint = FezMath.OrientationFromDirection(Vector3.Transform(Vector3.UnitZ, this.AoInstance.Rotation).MaxClamp()).AsViewpoint();
    this.lastBits = -1;
  }

  private void InitBitPlanes()
  {
    if (this.lastBits == this.GameState.SaveData.CubeShards + this.GameState.SaveData.SecretCubes)
      return;
    foreach (BackgroundPlane bitPlane in this.BitPlanes)
      this.LevelManager.RemovePlane(bitPlane);
    this.BitPlanes.Clear();
    if (this.AoInstance.Rotation == new Quaternion(0.0f, 0.0f, 0.0f, -1f))
      this.AoInstance.Rotation = Quaternion.Identity;
    int bitCount = this.AoInstance.ArtObject.ActorType.GetBitCount();
    for (int index = 0; index < bitCount; ++index)
    {
      BackgroundPlane plane = new BackgroundPlane(this.LevelMaterializer.StaticPlanesMesh, index < this.GameState.SaveData.CubeShards ? (Texture) this.BitTexture : (Texture) this.AntiBitTexture)
      {
        Rotation = this.AoInstance.Rotation,
        Opacity = 0.0f,
        Fullbright = true
      };
      this.BitPlanes.Add(plane);
      this.LevelManager.AddPlane(plane);
    }
    this.lastBits = this.GameState.SaveData.CubeShards + this.GameState.SaveData.SecretCubes;
  }

  public void Update(TimeSpan elapsed)
  {
    if (this.lastBits == -1)
      this.InitBitPlanes();
    this.DetermineIsClose();
    if (this.AoInstance.ActorSettings.Inactive)
      return;
    if (!this.opening && !this.close && this.sinceClose.TotalSeconds > 0.0)
      this.sinceClose -= elapsed;
    else if (this.close && this.sinceClose.TotalSeconds < 3.0)
      this.sinceClose += elapsed;
    this.FadeBits();
    if (this.GameState.SaveData.CubeShards + this.GameState.SaveData.SecretCubes < this.AoInstance.ArtObject.ActorType.GetBitCount() || this.sinceClose.TotalSeconds <= 0.5)
      return;
    this.OpenDoor(elapsed);
  }

  private void DetermineIsClose()
  {
    this.close = false;
    if (!this.AoInstance.Visible || this.AoInstance.ActorSettings.Inactive || this.CameraManager.Viewpoint != this.ExpectedViewpoint || this.PlayerManager.Background)
      return;
    Vector3 vector3_1 = this.AoInstance.Position + Vector3.Transform(Vector3.Transform(new Vector3(0.0f, 0.0f, 1f), this.AoInstance.Rotation), this.AoInstance.Rotation);
    Vector3 vector3_2 = (vector3_1 - this.PlayerManager.Position).Abs() * this.CameraManager.Viewpoint.ScreenSpaceMask();
    this.close = (double) vector3_2.X + (double) vector3_2.Z < 2.0 && (double) vector3_2.Y < 2.0 && (double) (vector3_1 - this.PlayerManager.Position).Dot(this.CameraManager.Viewpoint.ForwardVector()) >= 0.0;
  }

  public Vector3 GetOpenOffset()
  {
    switch (this.AoInstance.ArtObject.ActorType.GetBitCount())
    {
      case 1:
      case 4:
      case 8:
      case 16 /*0x10*/:
        return new Vector3(0.0f, 4f, 0.0f) - Vector3.Transform(new Vector3(0.0f, 0.0f, 0.125f), this.AoInstance.Rotation);
      case 2:
        return new Vector3(0.0f, 4f, 0.0f);
      case 32 /*0x20*/:
        return new Vector3(0.0f, -4f, 0.0f) - Vector3.Transform(new Vector3(0.0f, 0.0f, 3f / 16f), this.AoInstance.Rotation);
      case 64 /*0x40*/:
        return new Vector3(0.0f, -4f, 0.0f) - Vector3.Transform(new Vector3(0.0f, 0.0f, 0.125f), this.AoInstance.Rotation);
      default:
        throw new InvalidOperationException();
    }
  }

  private void OpenDoor(TimeSpan elapsed)
  {
    if (!this.opening)
    {
      this.doorOrigin = this.AoInstance.Position + this.GetOpenOffset() * FezMath.XZMask;
      this.doorDestination = this.AoInstance.Position + this.GetOpenOffset();
      this.opening = true;
      this.rumbleEmitter = this.RumbleSound.EmitAt(this.doorOrigin, true);
      this.LevelService.ResolvePuzzle();
    }
    this.sinceMoving += elapsed;
    Vector3 vector3;
    if (this.sinceMoving > BitDoorState.DoorShakeTime)
    {
      long ticks1 = this.sinceMoving.Ticks;
      TimeSpan timeSpan = BitDoorState.DoorShakeTime;
      long ticks2 = timeSpan.Ticks;
      double num = (double) (ticks1 - ticks2);
      timeSpan = BitDoorState.DoorOpenTime;
      double ticks3 = (double) timeSpan.Ticks;
      vector3 = Vector3.Lerp(this.doorOrigin, this.doorDestination, FezMath.Saturate(Easing.EaseInOut(num / ticks3, EasingType.Sine)));
    }
    else
      vector3 = this.doorOrigin;
    this.AoInstance.Position = vector3 + new Vector3(RandomHelper.Centered(0.014999999664723873), RandomHelper.Centered(0.014999999664723873), RandomHelper.Centered(0.014999999664723873)) * this.CameraManager.Viewpoint.ScreenSpaceMask();
    if (!(this.sinceMoving > BitDoorState.DoorOpenTime + BitDoorState.DoorShakeTime))
      return;
    this.rumbleEmitter.FadeOutAndDie(0.25f);
    this.BitDoorService.OnOpen(this.AoInstance.Id);
    this.AoInstance.ActorSettings.Inactive = true;
    this.GameState.SaveData.ThisLevel.InactiveArtObjects.Add(this.AoInstance.Id);
    this.GameState.Save();
    this.opening = false;
  }

  private void FadeBits()
  {
    if (this.sinceClose.Ticks == 0L)
      return;
    this.InitBitPlanes();
    Vector3 position = this.AoInstance.Position;
    Vector3 vector3_1 = new Vector3(0.0f, 0.0f, 1f);
    for (int index = 0; index < this.BitPlanes.Count; ++index)
    {
      Vector3 vector3_2 = vector3_1;
      switch (this.BitPlanes.Count)
      {
        case 2:
          vector3_2 += new Vector3(0.0f, (float) (0.25 - (double) index * 0.5), 0.0f);
          break;
        case 4:
          vector3_2 += new Vector3(0.0f, -1f, 0.0f) + this.FourOffsets[index];
          break;
        case 8:
          vector3_2 += new Vector3(-0.5f, -0.75f, 0.0f) + this.EightOffsets[index];
          break;
        case 16 /*0x10*/:
          vector3_2 += new Vector3(-0.5f, -1.5f, 0.0f) + this.SixteenOffsets[index];
          break;
        case 32 /*0x20*/:
          vector3_2 += new Vector3(-9f / 16f, -21f / 16f, 0.0f) + this.ThirtyTwoOffsets[index];
          break;
        case 64 /*0x40*/:
          vector3_2 += new Vector3(-0.625f, -1.625f, 0.0f) + this.SixtyFourOffsets[index];
          break;
      }
      int num = this.GameState.SaveData.CubeShards + this.GameState.SaveData.SecretCubes;
      this.BitPlanes[index].Position = position + Vector3.Transform(vector3_2, this.AoInstance.Rotation);
      float opacity = this.BitPlanes[index].Opacity;
      this.BitPlanes[index].Opacity = (float) (num > index).AsNumeric() * Easing.EaseIn(FezMath.Saturate(this.sinceClose.TotalSeconds * 2.0 - (double) index / ((double) this.BitPlanes.Count * 0.66600000858306885 + 3.9960000514984131)), EasingType.Sine);
      if ((double) this.BitPlanes[index].Opacity > (double) opacity && (double) opacity > 0.10000000149011612 && this.BitPlanes[index].Loop)
      {
        this.sLightUp.EmitAt(position);
        this.BitPlanes[index].Loop = false;
      }
      else if ((double) this.BitPlanes[index].Opacity < (double) opacity && !this.BitPlanes[index].Loop)
      {
        this.sFadeOut.EmitAt(position);
        this.BitPlanes[index].Loop = true;
      }
    }
  }

  [ServiceDependency]
  public ILevelService LevelService { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  [ServiceDependency]
  public IBitDoorService BitDoorService { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }
}
