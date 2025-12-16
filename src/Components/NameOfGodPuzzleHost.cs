// Decompiled with JetBrains decompiler
// Type: FezGame.Components.NameOfGodPuzzleHost
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
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class NameOfGodPuzzleHost(Game game) : GameComponent(game)
{
  private static readonly int[] SpiralTraversal = new int[8]
  {
    -1,
    1,
    -3,
    3,
    -5,
    5,
    -7,
    7
  };
  private static readonly Vector3 PuzzleCenter = new Vector3(13.5f, 57.5f, 14.5f);
  private static readonly NameOfGodPuzzleHost.ZuishSlot[] Slots = new NameOfGodPuzzleHost.ZuishSlot[8]
  {
    new NameOfGodPuzzleHost.ZuishSlot("ZUISH_BLOCKS_0", FaceOrientation.Back),
    new NameOfGodPuzzleHost.ZuishSlot("ZUISH_BLOCKS_4", FaceOrientation.Front),
    new NameOfGodPuzzleHost.ZuishSlot("ZUISH_BLOCKS_1", FaceOrientation.Right),
    new NameOfGodPuzzleHost.ZuishSlot("ZUISH_BLOCKS_0", FaceOrientation.Front),
    new NameOfGodPuzzleHost.ZuishSlot("ZUISH_BLOCKS_1", FaceOrientation.Right),
    new NameOfGodPuzzleHost.ZuishSlot("ZUISH_BLOCKS_5", FaceOrientation.Back),
    new NameOfGodPuzzleHost.ZuishSlot("ZUISH_BLOCKS_2", FaceOrientation.Back),
    new NameOfGodPuzzleHost.ZuishSlot("ZUISH_BLOCKS_1", FaceOrientation.Back)
  };
  private int RightPositions;
  private List<TrileInstance> Blocks;

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
    if (this.LevelManager.Name == "ZU_ZUISH")
    {
      if (this.GameState.SaveData.ThisLevel.InactiveArtObjects.Contains(0))
      {
        foreach (TrileInstance instance in this.LevelManager.Triles.Values.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.Trile.ActorSettings.Type == ActorType.PickUp)).ToArray<TrileInstance>())
        {
          instance.PhysicsState = (InstancePhysicsState) null;
          this.LevelManager.ClearTrile(instance);
        }
        if (!this.GameState.SaveData.ThisLevel.DestroyedTriles.Contains(new TrileEmplacement(NameOfGodPuzzleHost.PuzzleCenter + Vector3.UnitY * 2f + Vector3.UnitZ - FezMath.HalfVector)))
        {
          Trile trile = this.LevelManager.ActorTriles(ActorType.PieceOfHeart).FirstOrDefault<Trile>();
          if (trile != null)
          {
            Vector3 position = NameOfGodPuzzleHost.PuzzleCenter + Vector3.UnitY * 2f - FezMath.HalfVector + Vector3.UnitZ;
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
    this.Blocks.AddRange(this.LevelManager.Triles.Values.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.Trile.ActorSettings.Type == ActorType.PickUp)));
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.InMap || !this.CameraManager.ActionRunning || !this.CameraManager.Viewpoint.IsOrthographic() || this.GameState.Loading)
      return;
    Vector3 b1 = this.CameraManager.Viewpoint.SideMask();
    Vector3 depthMask = this.CameraManager.Viewpoint.DepthMask();
    Vector3 vector3_1 = this.CameraManager.Viewpoint.ScreenSpaceMask();
    if (this.RightPositions == 8)
    {
      bool flag = false;
      foreach (TrileInstance block in this.Blocks)
      {
        if (!flag)
        {
          ServiceHelper.AddComponent((IGameComponent) new GlitchyDespawner(this.Game, block, NameOfGodPuzzleHost.PuzzleCenter + Vector3.UnitY * 2f + Vector3.UnitZ)
          {
            FlashOnSpawn = true,
            ActorToSpawn = ActorType.PieceOfHeart
          });
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
      Vector3 b2 = this.CameraManager.Viewpoint.RightVector();
      foreach (TrileInstance block1 in this.Blocks)
      {
        float num = block1.Center.Dot(b2);
        block1.LastTreasureSin = 0.0f;
        foreach (TrileInstance block2 in this.Blocks)
        {
          if (block2 != block1 && (double) num > (double) block2.Center.Dot(b2))
            ++block1.LastTreasureSin;
        }
      }
      foreach (TrileInstance block in this.Blocks)
      {
        TrileInstance instance = block;
        if (instance.Enabled && (double) instance.Position.Y >= 57.0 && (double) instance.Position.Y < 57.75 && this.PlayerManager.HeldInstance != instance && this.PlayerManager.PushedInstance != instance && instance.PhysicsState.Grounded)
        {
          MultipleHits<TrileInstance> multipleHits1 = instance.PhysicsState.Ground;
          if (multipleHits1.First != this.PlayerManager.PushedInstance)
          {
            if (!instance.PhysicsState.Background)
            {
              Vector3 vector3_2 = Vector3.Min(Vector3.Max(((instance.Center - NameOfGodPuzzleHost.PuzzleCenter) / 1f).Round(), new Vector3(-8f, 0.0f, -8f)), new Vector3(8f, 1f, 8f));
              Vector3 vector3_3 = NameOfGodPuzzleHost.PuzzleCenter + vector3_2 * 1f;
              Vector3 vector3_4 = (vector3_3 - instance.Center) * FezMath.XZMask;
              float num1 = Math.Max(vector3_4.Length(), 0.1f);
              instance.PhysicsState.Velocity += 0.25f * (vector3_4 / num1) * (float) gameTime.ElapsedGameTime.TotalSeconds;
              if ((double) num1 <= 0.10000000149011612 && (double) vector3_2.Y == 0.0)
              {
                TrileInstance trileInstance = instance;
                multipleHits1 = this.PlayerManager.Ground;
                TrileInstance first = multipleHits1.First;
                if (trileInstance != first && !this.Blocks.Any<TrileInstance>((Func<TrileInstance, bool>) (x => x.PhysicsState.Ground.First == instance)) && this.Blocks.Any<TrileInstance>((Func<TrileInstance, bool>) (b => b != instance && FezMath.AlmostEqual(b.Position.Y, instance.Position.Y) && (double) Math.Abs((b.Center - instance.Center).Dot(depthMask)) < 1.0)))
                {
                  instance.Enabled = false;
                  for (int index = 0; index < NameOfGodPuzzleHost.SpiralTraversal.Length; ++index)
                  {
                    int num2 = NameOfGodPuzzleHost.SpiralTraversal[index];
                    NearestTriles nearestTriles = this.LevelManager.NearestTrile(NameOfGodPuzzleHost.PuzzleCenter + (float) num2 * 1f * depthMask, QueryOptions.None, new Viewpoint?(this.CameraManager.Viewpoint.GetRotatedView(1)));
                    if (nearestTriles.Deep == null)
                      nearestTriles.Deep = this.LevelManager.NearestTrile(NameOfGodPuzzleHost.PuzzleCenter + (float) num2 * 1f * depthMask - depthMask * 0.5f, QueryOptions.None, new Viewpoint?(this.CameraManager.Viewpoint.GetRotatedView(1))).Deep;
                    if (nearestTriles.Deep == null)
                      nearestTriles.Deep = this.LevelManager.NearestTrile(NameOfGodPuzzleHost.PuzzleCenter + (float) num2 * 1f * depthMask + depthMask * 0.5f, QueryOptions.None, new Viewpoint?(this.CameraManager.Viewpoint.GetRotatedView(1))).Deep;
                    if (nearestTriles.Deep == null)
                    {
                      vector3_3 = instance.PhysicsState.Center = vector3_1 * instance.PhysicsState.Center + NameOfGodPuzzleHost.PuzzleCenter * depthMask + depthMask * (float) num2 * 1f;
                      break;
                    }
                  }
                  instance.Enabled = true;
                }
              }
              if ((double) Math.Abs(vector3_4.X) <= 1.0 / 64.0 && (double) Math.Abs(vector3_4.Y) <= 1.0 / 64.0)
              {
                instance.PhysicsState.Velocity = Vector3.Zero;
                instance.PhysicsState.Center = vector3_3;
                instance.PhysicsState.UpdateInstance();
                this.LevelManager.UpdateInstance(instance);
              }
              if ((instance.PhysicsState.Ground.NearLow == null || instance.PhysicsState.Ground.NearLow.PhysicsState != null && (double) Math.Abs((instance.PhysicsState.Ground.NearLow.Center - instance.Center).Dot(b1)) > 0.875) && (instance.PhysicsState.Ground.FarHigh == null || instance.PhysicsState.Ground.FarHigh.PhysicsState != null && (double) Math.Abs((instance.PhysicsState.Ground.FarHigh.Center - instance.Center).Dot(b1)) > 0.875))
              {
                InstancePhysicsState physicsState = instance.PhysicsState;
                multipleHits1 = new MultipleHits<TrileInstance>();
                MultipleHits<TrileInstance> multipleHits2 = multipleHits1;
                physicsState.Ground = multipleHits2;
                instance.PhysicsState.Center += Vector3.Down * 0.1f;
              }
            }
            if (instance.PhysicsState.Grounded && instance.PhysicsState.Velocity == Vector3.Zero)
            {
              string cubemapPath = instance.Trile.CubemapPath;
              FaceOrientation o = FezMath.OrientationFromPhi(this.CameraManager.Viewpoint.ToPhi() + instance.Phi);
              switch (o)
              {
                case FaceOrientation.Left:
                case FaceOrientation.Right:
                  o = o.GetOpposite();
                  break;
              }
              int index = (int) MathHelper.Clamp(instance.LastTreasureSin, 0.0f, 7f);
              if (NameOfGodPuzzleHost.Slots[index].Face == o && NameOfGodPuzzleHost.Slots[index].TrileName == cubemapPath)
                ++this.RightPositions;
            }
          }
        }
      }
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

  private struct ZuishSlot(string trileName, FaceOrientation face)
  {
    public readonly string TrileName = trileName;
    public readonly FaceOrientation Face = face;
  }
}
