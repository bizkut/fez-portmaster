// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Standing
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

internal class Standing(Game game) : PlayerAction(game)
{
  private SoundEffect sBlink;
  private int lastFrame;

  public override void Initialize()
  {
    base.Initialize();
    this.sBlink = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/Blink");
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.Action == ActionType.StandWinking)
    {
      int frame = this.PlayerManager.Animation.Timing.Frame;
      if (this.lastFrame != frame && (frame == 1 || frame == 13))
        this.sBlink.EmitAt(this.PlayerManager.Position);
      this.lastFrame = frame;
    }
    return this.PlayerManager.Action == ActionType.StandWinking;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.Standing || type == ActionType.StandWinking;
  }
}
