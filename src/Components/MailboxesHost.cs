// Decompiled with JetBrains decompiler
// Type: FezGame.Components.MailboxesHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components;

internal class MailboxesHost(Game game) : GameComponent(game)
{
  private readonly List<MailboxesHost.MailboxState> Mailboxes = new List<MailboxesHost.MailboxState>();

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    this.Mailboxes.Clear();
    foreach (ArtObjectInstance aoInstance in (IEnumerable<ArtObjectInstance>) this.LevelManager.ArtObjects.Values)
    {
      if (aoInstance.ArtObject.ActorType == ActorType.Mailbox && !this.GameState.SaveData.ThisLevel.InactiveArtObjects.Contains(aoInstance.Id) && aoInstance.ActorSettings.TreasureMapName != null)
        this.Mailboxes.Add(new MailboxesHost.MailboxState(aoInstance));
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused || this.GameState.InCutscene || this.GameState.InMap || this.GameState.InMenuCube || this.GameState.InFpsMode)
      return;
    for (int index = this.Mailboxes.Count - 1; index >= 0; --index)
    {
      this.Mailboxes[index].Update();
      if (this.Mailboxes[index].Empty)
        this.Mailboxes.RemoveAt(index);
    }
  }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  private class MailboxState
  {
    public readonly ArtObjectInstance MailboxAo;

    public bool Empty { get; private set; }

    public MailboxState(ArtObjectInstance aoInstance)
    {
      ServiceHelper.InjectServices((object) this);
      this.MailboxAo = aoInstance;
    }

    public void Update()
    {
      Vector3 position = this.MailboxAo.Position;
      Vector3 center = this.PlayerManager.Center;
      Vector3 b1 = this.CameraManager.Viewpoint.SideMask();
      Vector3 b2 = this.CameraManager.Viewpoint.ForwardVector();
      Vector3 vector3 = new Vector3(position.Dot(b1), position.Y, 0.0f);
      BoundingBox boundingBox = new BoundingBox(vector3 - new Vector3(0.5f, 0.75f, 0.5f), vector3 + new Vector3(0.5f, 0.75f, 0.5f));
      Vector3 point = new Vector3(center.Dot(b1), center.Y, 0.0f);
      if ((double) center.Dot(b2) >= (double) this.MailboxAo.Position.Dot(b2) || boundingBox.Contains(point) == ContainmentType.Disjoint || this.InputManager.GrabThrow != FezButtonState.Pressed)
        return;
      this.GameState.SaveData.ThisLevel.InactiveArtObjects.Add(this.MailboxAo.Id);
      ServiceHelper.AddComponent((IGameComponent) new LetterViewer(ServiceHelper.Game, this.MailboxAo.ActorSettings.TreasureMapName));
      this.GomezService.OnReadMail();
      this.Empty = true;
    }

    [ServiceDependency]
    public IGomezService GomezService { private get; set; }

    [ServiceDependency]
    public IGameStateManager GameState { private get; set; }

    [ServiceDependency]
    public IInputManager InputManager { private get; set; }

    [ServiceDependency]
    public IPlayerManager PlayerManager { private get; set; }

    [ServiceDependency]
    public IGameCameraManager CameraManager { private get; set; }
  }
}
