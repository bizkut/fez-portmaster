// Decompiled with JetBrains decompiler
// Type: FezGame.Services.MockUser
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

#nullable disable
namespace FezGame.Services;

public class MockUser
{
  public static readonly MockUser Default = new MockUser()
  {
    PersonaName = "DefaultUser"
  };

  public string PersonaName { get; set; }
}
