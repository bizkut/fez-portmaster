// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.Scripting.IRotatingGroupService
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using FezEngine.Structure.Scripting;

#nullable disable
namespace FezEngine.Services.Scripting;

[Entity(Model = typeof (TrileGroup), RestrictTo = new ActorType[] {ActorType.RotatingGroup})]
public interface IRotatingGroupService : IScriptingBase
{
  void Rotate(int id, bool clockwise, int turns);

  void SetEnabled(int id, bool enabled);
}
