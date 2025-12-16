// Decompiled with JetBrains decompiler
// Type: FezGame.Tools.CreditsText
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Tools;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Tools;

public static class CreditsText
{
  private static readonly Dictionary<string, string> Fallback = ServiceHelper.Get<IContentManagerProvider>().Global.Load<Dictionary<string, Dictionary<string, string>>>("Resources/CreditsText")[string.Empty];

  public static string GetString(string tag)
  {
    string str;
    return tag == null || !CreditsText.Fallback.TryGetValue(tag, out str) ? "[MISSING TEXT]" : str;
  }
}
