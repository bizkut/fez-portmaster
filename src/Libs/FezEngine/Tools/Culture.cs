// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.Culture
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;
using System.Globalization;

#nullable disable
namespace FezEngine.Tools;

public static class Culture
{
  public static Language Language = Culture.LanguageFromCurrentCulture();

  public static bool IsCJK => Culture.Language.IsCjk();

  public static string TwoLetterISOLanguageName
  {
    get
    {
      switch (Culture.Language)
      {
        case Language.English:
          return "en";
        case Language.French:
          return "fr";
        case Language.Italian:
          return "it";
        case Language.German:
          return "de";
        case Language.Spanish:
          return "es";
        case Language.Portuguese:
          return "pt";
        case Language.Chinese:
          return "zh";
        case Language.Japanese:
          return "ja";
        case Language.Korean:
          return "ko";
        default:
          throw new InvalidOperationException("Unknown culture");
      }
    }
  }

  public static Language LanguageFromCurrentCulture()
  {
    switch (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
    {
      case "de":
        return Language.German;
      case "es":
        return Language.Spanish;
      case "fr":
        return Language.French;
      case "it":
        return Language.Italian;
      case "ja":
        return Language.Japanese;
      case "ko":
        return Language.Korean;
      case "pt":
        return Language.Portuguese;
      case "zh":
        return Language.Chinese;
      default:
        return Language.English;
    }
  }

  public static bool IsCjk(this Language language)
  {
    return language == Language.Japanese || language == Language.Korean || language == Language.Chinese;
  }
}
