// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.IShaderInstantiatableEffect`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

#nullable disable
namespace FezEngine.Effects;

public interface IShaderInstantiatableEffect<T>
{
  void SetInstanceData(T[] instances, int start, int batchInstances);
}
