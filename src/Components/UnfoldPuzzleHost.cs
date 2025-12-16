// Decompiled with JetBrains decompiler
// Type: FezGame.Components.UnfoldPuzzleHost
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

internal class UnfoldPuzzleHost(Game game) : GameComponent(game)
{
  private static readonly Vector3 PuzzleCenter = new Vector3(9f, 57.5f, 11f);
  private static readonly Vector3[] Slots1 = new Vector3[6]
  {
    new Vector3(-2f, 1f, 0.0f),
    new Vector3(-1f, 1f, 0.0f),
    new Vector3(0.0f, 1f, 0.0f),
    new Vector3(0.0f, 0.0f, 0.0f),
    new Vector3(1f, 0.0f, 0.0f),
    new Vector3(2f, 0.0f, 0.0f)
  };
  private static readonly Vector3[] Slots2 = new Vector3[6]
  {
    new Vector3(-3f, 1f, 0.0f),
    new Vector3(-2f, 1f, 0.0f),
    new Vector3(-1f, 1f, 0.0f),
    new Vector3(-1f, 0.0f, 0.0f),
    new Vector3(0.0f, 0.0f, 0.0f),
    new Vector3(1f, 0.0f, 0.0f)
  };
  private static readonly Vector3[] Slots3 = new Vector3[6]
  {
    new Vector3(-3f, 1f, 0.0f),
    new Vector3(-2f, 1f, 0.0f),
    new Vector3(-1f, 1f, 0.0f),
    new Vector3(-5f, 0.0f, 0.0f),
    new Vector3(-4f, 0.0f, 0.0f),
    new Vector3(-3f, 0.0f, 0.0f)
  };
  private static readonly Vector3[] Slots4 = new Vector3[6]
  {
    new Vector3(-4f, 1f, 0.0f),
    new Vector3(-3f, 1f, 0.0f),
    new Vector3(-2f, 1f, 0.0f),
    new Vector3(-6f, 0.0f, 0.0f),
    new Vector3(-5f, 0.0f, 0.0f),
    new Vector3(-4f, 0.0f, 0.0f)
  };
  private int RightPositions1;
  private int RightPositions2;
  private int RightPositions3;
  private int RightPositions4;
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
    this.RightPositions1 = this.RightPositions2 = this.RightPositions3 = this.RightPositions4 = 0;
    if (this.LevelManager.Name == "ZU_UNFOLD")
    {
      if (this.GameState.SaveData.ThisLevel.InactiveArtObjects.Contains(0))
      {
        foreach (TrileInstance instance in this.LevelManager.Triles.Values.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.Trile.ActorSettings.Type == ActorType.SinkPickup)).ToArray<TrileInstance>())
        {
          instance.PhysicsState = (InstancePhysicsState) null;
          this.LevelManager.ClearTrile(instance);
        }
        if (!this.GameState.SaveData.ThisLevel.DestroyedTriles.Contains(new TrileEmplacement(UnfoldPuzzleHost.PuzzleCenter + Vector3.UnitY * 2f + Vector3.UnitZ - FezMath.HalfVector)))
        {
          Trile trile = this.LevelManager.ActorTriles(ActorType.SecretCube).FirstOrDefault<Trile>();
          if (trile != null)
          {
            Vector3 position = UnfoldPuzzleHost.PuzzleCenter + Vector3.UnitY * 2f - FezMath.HalfVector + Vector3.UnitZ;
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
    Vector3 vector3_1 = this.CameraManager.Viewpoint.ScreenSpaceMask();
    this.CameraManager.Viewpoint.SideMask();
    if (this.RightPositions1 == 6 || this.RightPositions2 == 6 || this.RightPositions3 == 6 || this.RightPositions4 == 6)
    {
      bool flag = false;
      foreach (TrileInstance block in this.Blocks)
      {
        if (!flag)
        {
          ServiceHelper.AddComponent((IGameComponent) new GlitchyDespawner(this.Game, block, UnfoldPuzzleHost.PuzzleCenter + Vector3.UnitY * 2f + Vector3.UnitZ)
          {
            FlashOnSpawn = true
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
      if (this.CameraManager.Viewpoint.ForwardVector() != -Vector3.UnitZ)
        return;
      this.RightPositions1 = this.RightPositions2 = this.RightPositions3 = this.RightPositions4 = 0;
      foreach (TrileInstance block in this.Blocks)
      {
        if (!block.PhysicsState.Background && block.Enabled && (double) block.Position.Y >= 57.0 && ((double) block.Position.Z > 9.0 || (double) block.Center.X >= 11.0 || (double) block.Center.X <= 6.0 && (double) block.Center.Y > 57.0 || (double) block.Center.X <= 5.0 && (double) block.Center.Y == 56.5))
        {
          if (this.PlayerManager.PushedInstance == block && block.PhysicsState.WallCollision.First.Collided && (double) Math.Abs((block.PhysicsState.WallCollision.First.Destination.Center - block.Center).Dot(Vector3.UnitY)) > 15.0 / 16.0)
          {
            block.PhysicsState.WallCollision = new MultipleHits<CollisionResult>();
            block.PhysicsState.Center += (float) this.PlayerManager.LookingDirection.Sign() * this.CameraManager.Viewpoint.RightVector() * 0.01f;
          }
          if (this.PlayerManager.HeldInstance != block && this.PlayerManager.PushedInstance != block && block.PhysicsState.Grounded && block.PhysicsState.Ground.First != this.PlayerManager.PushedInstance)
          {
            Vector3 vector3_2 = Vector3.Min(Vector3.Max(((block.Center - UnfoldPuzzleHost.PuzzleCenter) / 1f).Round(), new Vector3(-7f, 0.0f, 0.0f)), new Vector3(7f, 1f, 0.0f));
            Vector3 vector3_3 = UnfoldPuzzleHost.PuzzleCenter + vector3_2 * 1f;
            if ((double) vector3_2.Y == 0.0)
              vector3_3 += Vector3.UnitZ;
            Vector3 vector3_4 = (vector3_3 - block.Center) * vector3_1;
            float d = Math.Abs(vector3_4.X);
            float num = (float) Math.Sign(vector3_4.X);
            block.PhysicsState.Velocity += (float) (0.25 * ((double) d < 1.0 ? (double) num * Math.Sqrt((double) d) : (double) num)) * Vector3.UnitX * (float) gameTime.ElapsedGameTime.TotalSeconds;
            if ((double) Math.Abs(vector3_4.X) <= 3.0 / 128.0 && (double) Math.Abs(vector3_4.Y) <= 3.0 / 128.0)
            {
              block.PhysicsState.Velocity *= Vector3.UnitY;
              block.PhysicsState.Center = block.PhysicsState.Center * (Vector3.One - vector3_1) + vector3_3 * vector3_1;
              block.PhysicsState.UpdateInstance();
              this.LevelManager.UpdateInstance(block);
            }
          }
        }
        if ((double) block.Position.Y >= 57.0 && ((double) block.Position.Z > 9.0 || (double) block.Center.X >= 11.0 || (double) block.Center.X <= 6.0 && (double) block.Center.Y > 57.0 || (double) block.Center.X <= 5.0 && (double) block.Center.Y == 56.5))
        {
          Vector3 vector3_5 = (block.Center * 16f).Round() / 16f;
          if (block.PhysicsState.Grounded && block.PhysicsState.Velocity.XZ() == Vector2.Zero)
          {
            for (int index = 0; index < UnfoldPuzzleHost.Slots1.Length; ++index)
            {
              if ((UnfoldPuzzleHost.PuzzleCenter + UnfoldPuzzleHost.Slots1[index]) * vector3_1 == vector3_5 * vector3_1)
              {
                ++this.RightPositions1;
                break;
              }
            }
            for (int index = 0; index < UnfoldPuzzleHost.Slots2.Length; ++index)
            {
              if ((UnfoldPuzzleHost.PuzzleCenter + UnfoldPuzzleHost.Slots2[index]) * vector3_1 == vector3_5 * vector3_1)
              {
                ++this.RightPositions2;
                break;
              }
            }
            for (int index = 0; index < UnfoldPuzzleHost.Slots3.Length; ++index)
            {
              if ((UnfoldPuzzleHost.PuzzleCenter + UnfoldPuzzleHost.Slots3[index]) * vector3_1 == vector3_5 * vector3_1)
              {
                ++this.RightPositions3;
                break;
              }
            }
            for (int index = 0; index < UnfoldPuzzleHost.Slots4.Length; ++index)
            {
              if ((UnfoldPuzzleHost.PuzzleCenter + UnfoldPuzzleHost.Slots4[index]) * vector3_1 == vector3_5 * vector3_1)
              {
                ++this.RightPositions4;
                break;
              }
            }
          }
        }
      }
    }
  }

  [ServiceDependency]
  public IGroupService GroupService { get; set; }

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
