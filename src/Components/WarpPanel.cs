// Decompiled with JetBrains decompiler
// Type: FezGame.Components.WarpPanel
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Structure;
using System;

#nullable disable
namespace FezGame.Components;

public class WarpPanel
{
  public string Destination;
  public Mesh PanelMask;
  public Mesh Layers;
  public FaceOrientation Face;
  public TimeSpan Timer;
  public bool Enabled;
}
