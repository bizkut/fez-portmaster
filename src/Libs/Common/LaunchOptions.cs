// Decompiled with JetBrains decompiler
// Type: Common.LaunchOptions
// Assembly: Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BC7A950C-D861-40F4-B8D6-28776BD88C9A
// Assembly location: E:\GOG Games\Fez\Common.dll

#nullable disable
namespace Common;

public class LaunchOptions
{
  public bool ForceWindowed { get; set; }

  public bool DisableGamepad { get; set; }

  public bool Force60Hz { get; set; }

  public bool SinglethreadedMode { get; set; }

  public bool DisableLighting { get; set; }

  public bool DisableSteamworks { get; set; }

  public bool DisablePauseOnUnfocus { get; set; }

  public bool PowerSavingMode { get; set; }
}
