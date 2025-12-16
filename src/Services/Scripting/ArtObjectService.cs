// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.ArtObjectService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Components.Scripting;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Components;
using FezGame.Structure;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Services.Scripting;

public class ArtObjectService : IArtObjectService, IScriptingBase
{
  public event Action<int> TreasureOpened = new Action<int>(Util.NullAction<int>);

  public void ResetEvents() => this.TreasureOpened = new Action<int>(Util.NullAction<int>);

  public void OnTreasureOpened(int id) => this.TreasureOpened(id);

  public void SetRotation(int id, float x, float y, float z)
  {
    this.LevelManager.ArtObjects[id].Rotation = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(y), MathHelper.ToRadians(x), MathHelper.ToRadians(z));
  }

  public void GlitchOut(int id, bool permanent, string spawnedActor)
  {
    if (!this.LevelManager.ArtObjects.ContainsKey(id))
      return;
    if (string.IsNullOrEmpty(spawnedActor))
      ServiceHelper.AddComponent((IGameComponent) new GlitchyDespawner(ServiceHelper.Game, this.LevelManager.ArtObjects[id]));
    else
      ServiceHelper.AddComponent((IGameComponent) new GlitchyDespawner(ServiceHelper.Game, this.LevelManager.ArtObjects[id], this.LevelManager.ArtObjects[id].Position)
      {
        ActorToSpawn = (ActorType) Enum.Parse(typeof (ActorType), spawnedActor, true)
      });
    if (!permanent)
      return;
    this.GameState.SaveData.ThisLevel.InactiveArtObjects.Add(-id - 1);
  }

  public LongRunningAction Move(
    int id,
    float dX,
    float dY,
    float dZ,
    float easeInFor,
    float easeOutAfter,
    float easeOutFor)
  {
    TrileGroup group = (TrileGroup) null;
    int? attachedGroup = this.LevelManager.ArtObjects[id].ActorSettings.AttachedGroup;
    if (attachedGroup.HasValue && this.LevelManager.Groups.ContainsKey(attachedGroup.Value))
      group = this.LevelManager.Groups[attachedGroup.Value];
    return new LongRunningAction((Func<float, float, bool>) ((elapsedSeconds, totalSeconds) =>
    {
      ArtObjectInstance artObjectInstance;
      if (!this.LevelManager.ArtObjects.TryGetValue(id, out artObjectInstance))
        return true;
      if ((double) totalSeconds < (double) easeInFor)
        elapsedSeconds *= Easing.EaseIn((double) totalSeconds / (double) easeInFor, EasingType.Quadratic);
      if ((double) easeOutFor != 0.0 && (double) totalSeconds > (double) easeOutAfter)
        elapsedSeconds *= Easing.EaseOut(1.0 - ((double) totalSeconds - (double) easeOutAfter) / (double) easeOutFor, EasingType.Quadratic);
      Vector3 vector3 = new Vector3(dX, dY, dZ) * elapsedSeconds;
      if (group != null)
      {
        foreach (TrileInstance trile in group.Triles)
        {
          trile.Position += vector3;
          this.LevelManager.UpdateInstance(trile);
        }
      }
      artObjectInstance.Position += vector3;
      return false;
    }));
  }

  public LongRunningAction TiltOnVertex(int id, float durationSeconds)
  {
    return new LongRunningAction((Func<float, float, bool>) ((_, totalSeconds) =>
    {
      ArtObjectInstance artObjectInstance;
      if (!this.LevelManager.ArtObjects.TryGetValue(id, out artObjectInstance))
        return true;
      float num = Easing.EaseInOut((double) totalSeconds / (double) durationSeconds, EasingType.Sine);
      artObjectInstance.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Right, (float) Math.Asin(Math.Sqrt(2.0) / Math.Sqrt(3.0)) * FezMath.Saturate(num)) * Quaternion.CreateFromAxisAngle(Vector3.Up, 0.7853982f * FezMath.Saturate(num));
      return false;
    }));
  }

  public LongRunningAction Rotate(int id, float dX, float dY, float dZ)
  {
    return new LongRunningAction((Func<float, float, bool>) ((elapsedSeconds, _) =>
    {
      ArtObjectInstance artObjectInstance;
      if (!this.LevelManager.ArtObjects.TryGetValue(id, out artObjectInstance))
        return true;
      artObjectInstance.Rotation = Quaternion.CreateFromYawPitchRoll(dY * 6.28318548f * elapsedSeconds, dX * 6.28318548f * elapsedSeconds, dZ * 6.28318548f * elapsedSeconds) * artObjectInstance.Rotation;
      return false;
    }));
  }

  public LongRunningAction RotateIncrementally(
    int id,
    float initPitch,
    float initYaw,
    float initRoll,
    float secondsUntilDouble)
  {
    return new LongRunningAction((Func<float, float, bool>) ((elapsedSeconds, _) =>
    {
      ArtObjectInstance artObjectInstance;
      if (!this.LevelManager.ArtObjects.TryGetValue(id, out artObjectInstance))
        return true;
      initYaw = FezMath.DoubleIter(initYaw, elapsedSeconds, secondsUntilDouble);
      initPitch = FezMath.DoubleIter(initPitch, elapsedSeconds, secondsUntilDouble);
      initRoll = FezMath.DoubleIter(initRoll, elapsedSeconds, secondsUntilDouble);
      artObjectInstance.Rotation = Quaternion.CreateFromYawPitchRoll(initYaw, initPitch, initRoll) * artObjectInstance.Rotation;
      return false;
    }));
  }

  public LongRunningAction HoverFloat(int id, float height, float cyclesPerSecond)
  {
    float lastDelta = 0.0f;
    return new LongRunningAction((Func<float, float, bool>) ((_, sinceStarted) =>
    {
      ArtObjectInstance artObjectInstance;
      if (!this.LevelManager.ArtObjects.TryGetValue(id, out artObjectInstance))
        return true;
      float num = (float) Math.Sin((double) sinceStarted * 6.2831854820251465 * (double) cyclesPerSecond) * height;
      artObjectInstance.Position = new Vector3(artObjectInstance.Position.X, artObjectInstance.Position.Y - lastDelta + num, artObjectInstance.Position.Z);
      lastDelta = num;
      return false;
    }));
  }

  public LongRunningAction BeamGomez(int id) => throw new InvalidOperationException();

  public LongRunningAction Pulse(int id, string textureName)
  {
    BackgroundPlane lightPlane = new BackgroundPlane(this.LevelMaterializer.StaticPlanesMesh, textureName, false)
    {
      Position = this.LevelManager.ArtObjects[id].Position - this.CameraManager.Viewpoint.ForwardVector() * 10f,
      Rotation = this.CameraManager.Rotation,
      AllowOverbrightness = true,
      LightMap = true,
      AlwaysOnTop = true,
      PixelatedLightmap = true
    };
    this.LevelManager.AddPlane(lightPlane);
    return new LongRunningAction((Func<float, float, bool>) ((_, sinceStarted) =>
    {
      double num1 = (double) FezMath.Saturate(sinceStarted / 2f);
      float num2 = Easing.EaseOut((double) FezMath.Saturate((float) num1), EasingType.Quadratic);
      lightPlane.Filter = new Color(1f - num2, 1f - num2, 1f - num2);
      float num3 = Easing.EaseOut((double) FezMath.Saturate((float) num1), EasingType.Quartic);
      lightPlane.Scale = new Vector3((float) (1.0 + (double) num3 * 10.0));
      if ((double) num3 == 1.0)
        this.LevelManager.RemovePlane(lightPlane);
      return (double) num3 == 1.0;
    }));
  }

  public LongRunningAction Say(int id, string text, bool zuish)
  {
    this.SpeechBubble.Font = zuish ? SpeechFont.Zuish : SpeechFont.Pixel;
    this.SpeechBubble.ChangeText(zuish ? text : GameText.GetString(text));
    IPlayerManager playerManager = this.PlayerManager;
    playerManager.Velocity = playerManager.Velocity * Vector3.UnitY;
    this.PlayerManager.Action = ActionType.ReadingSign;
    return new LongRunningAction((Func<float, float, bool>) ((_, __) =>
    {
      ArtObjectInstance artObjectInstance;
      if (!this.LevelManager.ArtObjects.TryGetValue(id, out artObjectInstance))
        return true;
      this.SpeechBubble.Origin = artObjectInstance.Position + this.CameraManager.Viewpoint.RightVector() * artObjectInstance.ArtObject.Size * 0.75f - Vector3.UnitY;
      return this.SpeechBubble.Hidden;
    }));
  }

  public LongRunningAction StartEldersSequence(int id)
  {
    ServiceHelper.AddComponent((IGameComponent) new EldersHexahedron(ServiceHelper.Game, this.LevelManager.ArtObjects[id]));
    return new LongRunningAction((Func<float, float, bool>) ((_, __) => false));
  }

  public void MoveNutToEnd(int id)
  {
    this.LevelManager.ArtObjects[id].ActorSettings.ShouldMoveToEnd = true;
  }

  public void MoveNutToHeight(int id, float height)
  {
    this.LevelManager.ArtObjects[id].ActorSettings.ShouldMoveToHeight = new float?(height);
  }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public ISpeechBubbleManager SpeechBubble { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }
}
