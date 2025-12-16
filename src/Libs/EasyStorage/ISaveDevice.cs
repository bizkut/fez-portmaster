// Decompiled with JetBrains decompiler
// Type: EasyStorage.ISaveDevice
// Assembly: EasyStorage, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FEE2FDB6-A226-4BC3-AEBF-7DC7EDE4694E
// Assembly location: E:\GOG Games\Fez\EasyStorage.dll

#nullable disable
namespace EasyStorage;

public interface ISaveDevice
{
  bool Save(string fileName, SaveAction saveAction);

  bool Load(string fileName, LoadAction loadAction);

  bool Delete(string fileName);

  bool FileExists(string fileName);
}
