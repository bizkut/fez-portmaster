// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.IWalkToService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public interface IWalkToService
{
  Func<Vector3> Destination { get; set; }

  ActionType NextAction { get; set; }
}
