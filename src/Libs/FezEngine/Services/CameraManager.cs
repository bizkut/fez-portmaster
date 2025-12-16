// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.CameraManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Services;

public abstract class CameraManager : GameComponent, ICameraProvider
{
  protected Matrix view = Matrix.Identity;
  protected Matrix projection = Matrix.Identity;

  public event Action ViewChanged = new Action(Util.NullAction);

  public event Action ProjectionChanged = new Action(Util.NullAction);

  protected CameraManager(Game game)
    : base(game)
  {
    this.UpdateOrder = 10;
  }

  protected virtual void OnViewChanged() => this.ViewChanged();

  protected virtual void OnProjectionChanged() => this.ProjectionChanged();

  public Matrix View => this.view;

  public Matrix Projection => this.projection;
}
