// Decompiled with JetBrains decompiler
// Type: FezGame.Components.GameLightingPostProcess
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine;
using FezEngine.Components;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components;

public class GameLightingPostProcess(Game game) : LightingPostProcess(game)
{
  private bool hasTested;

  protected override void DrawLightOccluders(GameTime gameTime)
  {
    if (this.PlayerManager.Hidden || this.GameState.InFpsMode)
      return;
    this.PlayerManager.MeshHost.InterpolatePosition(gameTime);
    this.PlayerManager.MeshHost.PlayerMesh.Draw();
  }

  protected override void DoSetup()
  {
    if (!this.PlayerManager.Hidden && !this.GameState.InFpsMode)
    {
      this.PlayerManager.MeshHost.PlayerMesh.Rotation = this.CameraManager.Viewpoint.IsOrthographic() || this.CameraManager.LastViewpoint == Viewpoint.None ? this.CameraManager.Rotation : Quaternion.CreateFromAxisAngle(Vector3.UnitY, this.CameraManager.LastViewpoint.ToPhi());
      if (this.PlayerManager.LookingDirection == HorizontalDirection.Left)
        this.PlayerManager.MeshHost.PlayerMesh.Rotation *= FezMath.QuaternionFromPhi(3.14159274f);
    }
    if (this.hasTested)
      return;
    try
    {
      this.GraphicsDevice.SetRenderTarget(this.lightMapsRth.Target);
      this.GraphicsDevice.SetRenderTarget((RenderTarget2D) null);
    }
    catch (InvalidOperationException ex)
    {
      Logger.LogError((Exception) ex);
      using (ErrorDialog errorDialog = new ErrorDialog())
      {
        int num = (int) errorDialog.ShowDialog();
        this.Game.Exit();
        return;
      }
    }
    this.hasTested = true;
  }

  public override void Update(GameTime gameTime)
  {
    base.Update(gameTime);
    if (this.GameState.Loading || !this.LevelManager.BackgroundPlanes.ContainsKey(-1))
      return;
    Vector3 vector3_1 = this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign();
    Vector3 vector3_2 = this.PlayerManager.Action == ActionType.PullUpCornerLedge ? this.PlayerManager.Position + this.PlayerManager.Size * (vector3_1 + Vector3.UnitY) * 0.5f * Easing.EaseOut((double) this.PlayerManager.Animation.Timing.NormalizedStep, EasingType.Quadratic) : (this.PlayerManager.Action == ActionType.LowerToCornerLedge ? this.PlayerManager.Position + this.PlayerManager.Size * (-vector3_1 + Vector3.UnitY) * 0.5f * (1f - Easing.EaseOut((double) this.PlayerManager.Animation.Timing.NormalizedStep, EasingType.Quadratic)) : this.PlayerManager.Position);
    if (this.GameState.InFpsMode)
      vector3_2 += this.CameraManager.InverseView.Forward;
    this.LevelManager.BackgroundPlanes[-1].Position = this.LevelManager.HaloFiltering ? vector3_2 : (vector3_2 * 16f).Round() / 16f;
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.SkipLighting)
      return;
    base.Draw(gameTime);
  }

  protected override bool SkipLighting
  {
    get
    {
      if (SettingsManager.Settings.Lighting)
        return false;
      return this.LevelManager.Sky == null || !(this.LevelManager.Sky.Name == "SEWER");
    }
  }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }
}
