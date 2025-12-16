// Decompiled with JetBrains decompiler
// Type: FezGame.Components.FlickeringNeon
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components;

public class FlickeringNeon(Game game) : GameComponent(game)
{
  private readonly List<FlickeringNeon.NeonState> NeonPlanes = new List<FlickeringNeon.NeonState>();
  private readonly List<SoundEffect> Glitches = new List<SoundEffect>();

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.TrackNeons);
    this.Enabled = false;
  }

  private void TrackNeons()
  {
    this.NeonPlanes.Clear();
    this.Glitches.Clear();
    this.Enabled = false;
    foreach (BackgroundPlane backgroundPlane in (IEnumerable<BackgroundPlane>) this.LevelManager.BackgroundPlanes.Values)
    {
      if (backgroundPlane.TextureName != null && backgroundPlane.TextureName.EndsWith("_GLOW") && backgroundPlane.TextureName.Contains("NEON"))
        this.NeonPlanes.Add(new FlickeringNeon.NeonState()
        {
          Neon = backgroundPlane,
          Time = RandomHelper.Between(2.0, 4.0)
        });
    }
    this.Enabled = this.NeonPlanes.Count > 0;
    if (!this.Enabled)
      return;
    this.Glitches.Add(this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/Glitches/Glitch1"));
    this.Glitches.Add(this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/Glitches/Glitch2"));
    this.Glitches.Add(this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/Glitches/Glitch3"));
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.InMap || this.GameState.InMenuCube || this.GameState.Paused)
      return;
    bool flag = !this.CameraManager.Viewpoint.IsOrthographic();
    Vector3 forward = this.CameraManager.InverseView.Forward;
    BoundingFrustum frustum = this.CameraManager.Frustum;
    foreach (FlickeringNeon.NeonState neonPlane in this.NeonPlanes)
    {
      neonPlane.Time -= (float) gameTime.ElapsedGameTime.TotalSeconds;
      if ((double) neonPlane.Time <= 0.0)
      {
        if (neonPlane.FlickersLeft == 0)
          neonPlane.FlickersLeft = RandomHelper.Random.Next(4, 18);
        BackgroundPlane neon = neonPlane.Neon;
        int num = (neon.Visible = !neon.Hidden && (flag || neon.Doublesided || neon.Crosshatch || neon.Billboard || (double) forward.Dot(neon.Forward) > 0.0) && frustum.Contains(neon.Bounds) != 0) ? 1 : 0;
        neonPlane.Enabled = !neonPlane.Enabled;
        neonPlane.Neon.Hidden = neonPlane.Enabled;
        neonPlane.Neon.Visible = !neonPlane.Neon.Hidden;
        neonPlane.Neon.Update();
        if (num != 0 && RandomHelper.Probability(0.5))
          RandomHelper.InList<SoundEffect>(this.Glitches).EmitAt(neonPlane.Neon.Position, false, RandomHelper.Centered(0.10000000149011612), RandomHelper.Between(0.0, 1.0), false);
        neonPlane.Time = Easing.EaseIn((double) RandomHelper.Between(0.0, 0.44999998807907104), EasingType.Quadratic);
        --neonPlane.FlickersLeft;
        if (neonPlane.FlickersLeft == 0)
        {
          neonPlane.Enabled = true;
          neonPlane.Neon.Hidden = false;
          neonPlane.Neon.Visible = true;
          neonPlane.Neon.Update();
          neonPlane.Time = RandomHelper.Between(3.0, 8.0);
        }
      }
    }
  }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  private class NeonState
  {
    public BackgroundPlane Neon;
    public float Time;
    public bool Enabled;
    public int FlickersLeft;
  }
}
