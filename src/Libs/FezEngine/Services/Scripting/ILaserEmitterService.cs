// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.Scripting.ILaserEmitterService
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Structure;
using FezEngine.Structure.Scripting;

#nullable disable
namespace FezEngine.Services.Scripting;

[Entity(Model = typeof (ArtObjectInstance), RestrictTo = new ActorType[] {ActorType.LaserEmitter})]
public interface ILaserEmitterService : IScriptingBase
{
  [Description("Starts or stops an emitter")]
  void SetEnabled(int id, bool enabled);
}
