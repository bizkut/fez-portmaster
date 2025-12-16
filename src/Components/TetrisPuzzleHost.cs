// Decompiled with JetBrains decompiler
// Type: FezGame.Components.TetrisPuzzleHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class TetrisPuzzleHost(Game game) : GameComponent(game)
{
  private static readonly int[] SpiralTraversal = new int[5]
  {
    0,
    1,
    -1,
    2,
    -2
  };
  private static readonly Vector3 PuzzleCenter = new Vector3(14.5f, 19.5f, 13.5f);
  private static readonly Vector3 TwoHigh = new Vector3(-1f, 0.0f, 0.0f);
  private static readonly Vector3 Interchangeable1_1 = new Vector3(0.0f, 0.0f, -1f);
  private static readonly Vector3 Interchangeable1_2 = new Vector3(1f, 0.0f, 1f);
  private static readonly Vector3 Interchangeable2_1 = new Vector3(0.0f, 0.0f, 1f);
  private static readonly Vector3 Interchangeable2_2 = new Vector3(1f, 0.0f, -1f);
  private int RightPositions;
  private List<TrileInstance> Blocks;
  private float SinceSolved;
  private float SinceStarted;

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    this.Blocks = (List<TrileInstance>) null;
    this.RightPositions = 0;
    if (this.LevelManager.Name == "ZU_TETRIS")
    {
      if (this.GameState.SaveData.ThisLevel.InactiveArtObjects.Contains(0))
      {
        foreach (TrileInstance instance in this.LevelManager.Triles.Values.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.Trile.ActorSettings.Type == ActorType.SinkPickup)).ToArray<TrileInstance>())
        {
          instance.PhysicsState = (InstancePhysicsState) null;
          this.LevelManager.ClearTrile(instance);
        }
        if (!this.GameState.SaveData.ThisLevel.DestroyedTriles.Contains(new TrileEmplacement(TetrisPuzzleHost.PuzzleCenter + Vector3.UnitY - FezMath.HalfVector)))
        {
          Trile trile = this.LevelManager.ActorTriles(ActorType.SecretCube).FirstOrDefault<Trile>();
          if (trile != null)
          {
            Vector3 position = TetrisPuzzleHost.PuzzleCenter + Vector3.UnitY - FezMath.HalfVector;
            this.LevelManager.ClearTrile(new TrileEmplacement(position));
            IGameLevelManager levelManager = this.LevelManager;
            TrileInstance instance = new TrileInstance(position, trile.Id);
            instance.OriginalEmplacement = new TrileEmplacement(position);
            TrileInstance toAdd = instance;
            levelManager.RestoreTrile(instance);
            if (toAdd.InstanceId == -1)
              this.LevelMaterializer.CullInstanceIn(toAdd);
          }
        }
        this.Enabled = false;
      }
      else
        this.Enabled = true;
    }
    else
      this.Enabled = false;
    if (!this.Enabled)
      return;
    this.Blocks = new List<TrileInstance>();
    this.Blocks.AddRange(this.LevelManager.Triles.Values.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.Trile.ActorSettings.Type == ActorType.SinkPickup)));
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.InMap || !this.CameraManager.ActionRunning || !this.CameraManager.Viewpoint.IsOrthographic() || this.GameState.Loading)
      return;
    Vector3 depthMask = this.CameraManager.Viewpoint.DepthMask();
    Vector3 vector3_1 = this.CameraManager.Viewpoint.ScreenSpaceMask();
    if (this.RightPositions == 4)
    {
      this.SinceSolved += (float) gameTime.ElapsedGameTime.TotalSeconds;
      float linearStep = FezMath.Saturate(this.SinceSolved);
      float amount = Easing.EaseIn((double) linearStep, EasingType.Cubic);
      foreach (TrileInstance block in this.Blocks)
      {
        Vector3 v = block.Center - TetrisPuzzleHost.PuzzleCenter;
        if ((double) v.Length() > 0.5)
        {
          Vector3 vector3_2 = FezMath.AlmostClamp(v, 0.1f).Sign();
          Vector3 vector3_3 = (TetrisPuzzleHost.PuzzleCenter + vector3_2 * 1.75f) * FezMath.XZMask + Vector3.UnitY * block.Center;
          Vector3 vector3_4 = (TetrisPuzzleHost.PuzzleCenter + vector3_2) * FezMath.XZMask + Vector3.UnitY * block.Center;
          block.PhysicsState.Center = Vector3.Lerp(vector3_3, vector3_4, amount);
          block.PhysicsState.UpdateInstance();
          this.LevelManager.UpdateInstance(block);
        }
      }
      if ((double) linearStep != 1.0)
        return;
      bool flag = false;
      foreach (TrileInstance block in this.Blocks)
      {
        if (!flag)
        {
          ServiceHelper.AddComponent((IGameComponent) new GlitchyDespawner(this.Game, block, TetrisPuzzleHost.PuzzleCenter + Vector3.UnitY));
          flag = true;
        }
        else
          ServiceHelper.AddComponent((IGameComponent) new GlitchyDespawner(this.Game, block));
      }
      this.GameState.SaveData.ThisLevel.InactiveArtObjects.Add(0);
      foreach (Volume volume in this.LevelManager.Volumes.Values.Where<Volume>((Func<Volume, bool>) (x => x.ActorSettings != null && x.ActorSettings.IsPointOfInterest && x.Enabled)))
      {
        volume.Enabled = false;
        this.GameState.SaveData.ThisLevel.InactiveVolumes.Add(volume.Id);
      }
      this.LevelService.ResolvePuzzle();
      this.Enabled = false;
    }
    else
    {
      this.RightPositions = 0;
      int num1 = 0;
      int num2 = 0;
      this.Blocks.Sort((Comparison<TrileInstance>) ((a, b) => b.LastTreasureSin.CompareTo(a.LastTreasureSin)));
      foreach (TrileInstance block in this.Blocks)
      {
        TrileInstance instance = block;
        if (!instance.PhysicsState.Grounded)
          instance.LastTreasureSin = this.SinceStarted;
        if ((double) instance.Position.Y >= 19.0 && (double) instance.Position.Y <= 20.5 && this.PlayerManager.HeldInstance != instance && this.PlayerManager.PushedInstance != instance && instance.PhysicsState.Grounded)
        {
          MultipleHits<TrileInstance> ground = instance.PhysicsState.Ground;
          if (ground.First != this.PlayerManager.PushedInstance)
          {
            Vector3 vector3_5 = Vector3.Min(Vector3.Max(((instance.Center - TetrisPuzzleHost.PuzzleCenter) / 1.75f).Round(), new Vector3(-3f, 0.0f, -3f)), new Vector3(3f, 1f, 3f));
            Vector3 vector3_6 = (TetrisPuzzleHost.PuzzleCenter + vector3_5 * 1.75f - instance.Center) * FezMath.XZMask;
            float num3 = Math.Max(vector3_6.Length(), 0.1f);
            instance.PhysicsState.Velocity += 0.25f * (vector3_6 / num3) * (float) gameTime.ElapsedGameTime.TotalSeconds;
            if ((double) num3 <= 0.10000000149011612 && (double) vector3_5.Y == 0.0)
            {
              TrileInstance trileInstance = instance;
              ground = this.PlayerManager.Ground;
              TrileInstance first = ground.First;
              if (trileInstance != first && !this.Blocks.Any<TrileInstance>((Func<TrileInstance, bool>) (x => x.PhysicsState.Ground.First == instance)) && this.Blocks.Any<TrileInstance>((Func<TrileInstance, bool>) (b => b != instance && FezMath.AlmostEqual(b.Position.Y, instance.Position.Y) && (double) Math.Abs((b.Center - instance.Center).Dot(depthMask)) < 1.75)))
              {
                int num4 = Math.Sign((instance.Center - TetrisPuzzleHost.PuzzleCenter).Dot(depthMask));
                if (num4 == 0)
                  num4 = 1;
                for (int index = -2; index <= 2; ++index)
                {
                  int num5 = TetrisPuzzleHost.SpiralTraversal[index + 2] * num4;
                  Vector3 tetativePosition = vector3_1 * instance.PhysicsState.Center + TetrisPuzzleHost.PuzzleCenter * depthMask + depthMask * (float) num5 * 1.75f;
                  if (this.Blocks.All<TrileInstance>((Func<TrileInstance, bool>) (b => b == instance || !FezMath.AlmostEqual(b.Position.Y, instance.Position.Y) || (double) Math.Abs((b.Center - tetativePosition).Dot(depthMask)) >= 1.5749999284744263)))
                  {
                    instance.PhysicsState.Center = tetativePosition;
                    break;
                  }
                }
              }
            }
            if (this.RightPositions < 4 && (double) num3 <= 0.10000000149011612 && instance.PhysicsState.Grounded)
            {
              if ((double) instance.Position.Y == 20.0)
              {
                if ((double) vector3_5.X == (double) TetrisPuzzleHost.TwoHigh.X && (double) vector3_5.Z == (double) TetrisPuzzleHost.TwoHigh.Z)
                  this.RightPositions += 2;
              }
              else
              {
                if ((double) vector3_5.X == (double) TetrisPuzzleHost.Interchangeable1_1.X && (double) vector3_5.Z == (double) TetrisPuzzleHost.Interchangeable1_1.Z || (double) vector3_5.X == (double) TetrisPuzzleHost.Interchangeable1_2.X && (double) vector3_5.Z == (double) TetrisPuzzleHost.Interchangeable1_2.Z)
                  ++num1;
                if ((double) vector3_5.X == (double) TetrisPuzzleHost.Interchangeable2_1.X && (double) vector3_5.Z == (double) TetrisPuzzleHost.Interchangeable2_1.Z || (double) vector3_5.X == (double) TetrisPuzzleHost.Interchangeable2_2.X && (double) vector3_5.Z == (double) TetrisPuzzleHost.Interchangeable2_2.Z)
                  ++num2;
              }
            }
          }
        }
      }
      if (this.RightPositions >= 4 || num1 != 2 && num2 != 2)
        return;
      this.RightPositions += 2;
    }
  }

  [ServiceDependency]
  public ILevelService LevelService { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { get; set; }
}
