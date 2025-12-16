// Decompiled with JetBrains decompiler
// Type: FezGame.Components.SaveIndicator
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Effects;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Components;

public class SaveIndicator : DrawableGameComponent
{
  private const float FadeInTime = 0.1f;
  private const float FadeOutTime = 0.1f;
  private const float LongShowTime = 0.5f;
  private const float ShortShowTime = 0.5f;
  private Mesh mesh;
  private float sinceLastSaveStarted = 4f;
  private float planeOpacity;
  private bool wasSaving;
  private float sinceLoadingVisible;
  private float currentShowTime;

  public SaveIndicator(Game game)
    : base(game)
  {
    this.DrawOrder = 2101;
  }

  protected override void LoadContent()
  {
    this.mesh = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      AlwaysOnTop = true,
      DepthWrites = false
    };
    this.mesh.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Front, Color.Red, true);
    DrawActionScheduler.Schedule((Action) (() =>
    {
      Mesh mesh = this.mesh;
      mesh.Effect = (BaseEffect) new DefaultEffect.VertexColored()
      {
        ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 10f), Vector3.Zero, Vector3.Up))
      };
    }));
  }

  public override void Update(GameTime gameTime)
  {
    float totalSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;
    this.sinceLoadingVisible = this.GameState.LoadingVisible ? FezMath.Saturate(this.sinceLoadingVisible + totalSeconds * 2f) : FezMath.Saturate(this.sinceLoadingVisible - totalSeconds * 3f);
    if (this.GameState.Saving || this.GameState.IsAchievementSave)
    {
      if (!this.wasSaving)
      {
        this.wasSaving = true;
        this.sinceLastSaveStarted = 0.0f;
        this.currentShowTime = this.GameState.IsAchievementSave ? 0.5f : 0.5f;
        this.GameState.IsAchievementSave = false;
      }
    }
    else if (this.wasSaving)
      this.wasSaving = false;
    if ((double) this.sinceLastSaveStarted < (double) this.currentShowTime + 0.20000000298023224)
      this.sinceLastSaveStarted += totalSeconds;
    this.planeOpacity = FezMath.Saturate(this.sinceLastSaveStarted / 0.1f) * FezMath.Saturate((float) (((double) this.currentShowTime - (double) this.sinceLastSaveStarted + 0.20000000298023224) / 0.10000000149011612));
  }

  public override void Draw(GameTime gameTime)
  {
    if ((double) this.planeOpacity == 0.0 || Fez.LongScreenshot)
      return;
    float aspectRatio = this.GraphicsDevice.Viewport.AspectRatio;
    this.mesh.Position = new Vector3(5.5f * aspectRatio, (float) (1.3999999761581421 * (double) aspectRatio - 7.0), 0.0f);
    this.mesh.Effect.ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic(14f * aspectRatio, 14f, 0.1f, 100f));
    this.mesh.Material.Opacity = this.planeOpacity;
    this.mesh.FirstGroup.Position = new Vector3(0.0f, (float) (1.75 * (double) Easing.EaseIn((double) this.sinceLoadingVisible, EasingType.Quadratic) * (this.GameState.DotLoading ? 0.0 : 1.0)), 0.0f);
    this.mesh.FirstGroup.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float) (-gameTime.ElapsedGameTime.TotalSeconds * 3.0)) * this.mesh.FirstGroup.Rotation;
    this.mesh.Draw();
  }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }
}
