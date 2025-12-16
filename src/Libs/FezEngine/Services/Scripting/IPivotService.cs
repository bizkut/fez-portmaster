// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.Scripting.IPivotService
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Structure;
using FezEngine.Structure.Scripting;
using System;

#nullable disable
namespace FezEngine.Services.Scripting;

[Entity(Model = typeof (ArtObjectInstance), RestrictTo = new ActorType[] {ActorType.PivotHandle})]
public interface IPivotService : IScriptingBase
{
  [Description("When it's been rotated right")]
  event Action<int> RotatedRight;

  void OnRotateRight(int id);

  [Description("When it's been rotated left")]
  event Action<int> RotatedLeft;

  void OnRotateLeft(int id);

  [Description("Gets the number of turns it's relative to the original state")]
  int get_Turns(int id);

  [Description("Enables or disables a pivot handle's rotatability")]
  void SetEnabled(int id, bool enabled);

  [Description("Enables or disables a pivot handle's rotatability")]
  void RotateTo(int id, int turns);
}
