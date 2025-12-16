// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Scripting.RunnableAction
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure.Scripting;
using System;

#nullable disable
namespace FezGame.Components.Scripting;

internal struct RunnableAction
{
  public ScriptAction Action;
  public Func<object> Invocation;
}
