// Decompiled with JetBrains decompiler
// Type: Microsoft.Xna.Framework.Graphics.Localization.WordWrap
// Assembly: XnaWordWrapCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6B8A288C-2178-4AAA-BC9E-71D510EB6454
// Assembly location: E:\GOG Games\Fez\XnaWordWrapCore.dll

#nullable disable
namespace Microsoft.Xna.Framework.Graphics.Localization;

public static class WordWrap
{
  private static Prohibition prohibition = Prohibition.On;
  private static NoHangulWrap nohangul = NoHangulWrap.Off;
  private static WordWrap.CB_GetWidth GetWidth;
  private static WordWrap.CB_Reserved Reserved;
  private static BreakInfo[] BreakArray = new BreakInfo[146]
  {
    new BreakInfo(94U, true, true),
    new BreakInfo(33U, true, false),
    new BreakInfo(36U, false, true),
    new BreakInfo(37U, true, false),
    new BreakInfo(39U, true, true),
    new BreakInfo(40U, false, true),
    new BreakInfo(41U, true, false),
    new BreakInfo(44U, true, false),
    new BreakInfo(46U, true, false),
    new BreakInfo(47U, true, true),
    new BreakInfo(58U, true, false),
    new BreakInfo(59U, true, false),
    new BreakInfo(63U /*0x3F*/, true, false),
    new BreakInfo(91U, false, true),
    new BreakInfo(92U, false, true),
    new BreakInfo(93U, true, false),
    new BreakInfo(123U, false, true),
    new BreakInfo(125U, true, false),
    new BreakInfo(162U, true, false),
    new BreakInfo(163U, false, true),
    new BreakInfo(165U, false, true),
    new BreakInfo(167U, false, true),
    new BreakInfo(168U, true, false),
    new BreakInfo(169U, true, false),
    new BreakInfo(174U, true, false),
    new BreakInfo(176U /*0xB0*/, true, false),
    new BreakInfo(183U, true, true),
    new BreakInfo(711U, true, false),
    new BreakInfo(713U, true, false),
    new BreakInfo(8211U, true, false),
    new BreakInfo(8212U, true, false),
    new BreakInfo(8213U, true, false),
    new BreakInfo(8214U, true, false),
    new BreakInfo(8216U, false, true),
    new BreakInfo(8217U, true, false),
    new BreakInfo(8220U, false, true),
    new BreakInfo(8221U, true, false),
    new BreakInfo(8226U, true, false),
    new BreakInfo(8229U, true, false),
    new BreakInfo(8230U, true, false),
    new BreakInfo(8231U, true, false),
    new BreakInfo(8242U, true, false),
    new BreakInfo(8243U, true, false),
    new BreakInfo(8245U, false, true),
    new BreakInfo(8451U, true, false),
    new BreakInfo(8482U, true, false),
    new BreakInfo(8758U, true, false),
    new BreakInfo(9588U, true, false),
    new BreakInfo(9839U, false, true),
    new BreakInfo(12289U, true, false),
    new BreakInfo(12290U, true, false),
    new BreakInfo(12291U, true, false),
    new BreakInfo(12293U, true, false),
    new BreakInfo(12296U, false, true),
    new BreakInfo(12297U, true, false),
    new BreakInfo(12298U, false, true),
    new BreakInfo(12299U, true, false),
    new BreakInfo(12300U, false, true),
    new BreakInfo(12301U, true, false),
    new BreakInfo(12302U, false, true),
    new BreakInfo(12303U, true, false),
    new BreakInfo(12304U, false, true),
    new BreakInfo(12305U, true, false),
    new BreakInfo(12306U, false, true),
    new BreakInfo(12308U, false, true),
    new BreakInfo(12309U, true, false),
    new BreakInfo(12310U, false, true),
    new BreakInfo(12311U, true, false),
    new BreakInfo(12317U, false, true),
    new BreakInfo(12318U, true, false),
    new BreakInfo(12319U, true, false),
    new BreakInfo(12353U, true, false),
    new BreakInfo(12355U, true, false),
    new BreakInfo(12357U, true, false),
    new BreakInfo(12359U, true, false),
    new BreakInfo(12361U, true, false),
    new BreakInfo(12387U, true, false),
    new BreakInfo(12419U, true, false),
    new BreakInfo(12421U, true, false),
    new BreakInfo(12423U, true, false),
    new BreakInfo(12430U, true, false),
    new BreakInfo(12441U, true, false),
    new BreakInfo(12442U, true, false),
    new BreakInfo(12443U, true, false),
    new BreakInfo(12444U, true, false),
    new BreakInfo(12445U, true, false),
    new BreakInfo(12446U, true, false),
    new BreakInfo(12449U, true, false),
    new BreakInfo(12451U, true, false),
    new BreakInfo(12453U, true, false),
    new BreakInfo(12455U, true, false),
    new BreakInfo(12457U, true, false),
    new BreakInfo(12483U, true, false),
    new BreakInfo(12515U, true, false),
    new BreakInfo(12517U, true, false),
    new BreakInfo(12519U, true, false),
    new BreakInfo(12526U, true, false),
    new BreakInfo(12533U, true, false),
    new BreakInfo(12534U, true, false),
    new BreakInfo(12539U, true, false),
    new BreakInfo(12540U, true, false),
    new BreakInfo(12541U, true, false),
    new BreakInfo(12542U, true, false),
    new BreakInfo(65072U, true, false),
    new BreakInfo(65104U, true, false),
    new BreakInfo(65105U, true, false),
    new BreakInfo(65106U, true, false),
    new BreakInfo(65108U, true, false),
    new BreakInfo(65109U, true, false),
    new BreakInfo(65110U, true, false),
    new BreakInfo(65111U, true, false),
    new BreakInfo(65113U, false, true),
    new BreakInfo(65114U, true, false),
    new BreakInfo(65115U, false, true),
    new BreakInfo(65116U, true, false),
    new BreakInfo(65117U, false, true),
    new BreakInfo(65118U, true, false),
    new BreakInfo(65281U, true, false),
    new BreakInfo(65282U, true, false),
    new BreakInfo(65284U, false, true),
    new BreakInfo(65285U, true, false),
    new BreakInfo(65287U, true, false),
    new BreakInfo(65288U, false, true),
    new BreakInfo(65289U, true, false),
    new BreakInfo(65292U, true, false),
    new BreakInfo(65294U, true, false),
    new BreakInfo(65306U, true, false),
    new BreakInfo(65307U, true, false),
    new BreakInfo(65311U, true, false),
    new BreakInfo(65312U, false, true),
    new BreakInfo(65339U, false, true),
    new BreakInfo(65341U, true, false),
    new BreakInfo(65344U, true, false),
    new BreakInfo(65371U, false, true),
    new BreakInfo(65372U, true, false),
    new BreakInfo(65373U, true, false),
    new BreakInfo(65374U, true, false),
    new BreakInfo(65377U, true, false),
    new BreakInfo(65380U, true, false),
    new BreakInfo(65392U, true, false),
    new BreakInfo(65438U, true, false),
    new BreakInfo(65439U, true, false),
    new BreakInfo(65504U, true, true),
    new BreakInfo(65505U, false, true),
    new BreakInfo(65509U, false, true),
    new BreakInfo(65510U, false, true)
  };

  static WordWrap()
  {
    WordWrap.SetCallback(new WordWrap.CB_GetWidth(WordWrap.MyGetCharWidth), (WordWrap.CB_Reserved) null);
  }

  private static uint MyGetCharWidth(SpriteFont spriteFont, char c)
  {
    return (uint) spriteFont.MeasureString(new string(c, 1)).X;
  }

  public static string Split(string text, SpriteFont font, float maxTextSize)
  {
    string source = text;
    text = "";
    if (source.Length == 0)
      return text;
    do
    {
      string EOL;
      int EOLOffset;
      string nextLine = WordWrap.FindNextLine(font, source, (uint) maxTextSize, out EOL, out EOLOffset);
      int length = EOL != null ? EOLOffset + 1 : 0;
      if (length != 0)
      {
        string str = source.Substring(0, length);
        text = $"{text}{str}\r\n";
      }
      else
        text += "\r\n";
      source = nextLine;
    }
    while (source != null);
    if (text.EndsWith("\r\n"))
      text = text.Substring(0, text.Length - 2);
    else if (text.EndsWith("\n"))
      text = text.Substring(0, text.Length - 1);
    return text;
  }

  public static bool IsNonBeginningChar(char c)
  {
    if (WordWrap.prohibition == Prohibition.Off)
      return false;
    int num1 = 0;
    int num2 = WordWrap.BreakArray.Length;
    while (num1 <= num2)
    {
      int index = (num2 - num1) / 2 + num1;
      if ((int) WordWrap.BreakArray[index].Character == (int) c)
        return WordWrap.BreakArray[index].IsNonBeginningCharacter;
      if ((uint) c < WordWrap.BreakArray[index].Character)
        num2 = index - 1;
      else
        num1 = index + 1;
    }
    return false;
  }

  public static bool IsNonEndingChar(char c)
  {
    if (WordWrap.prohibition == Prohibition.Off)
      return false;
    int num1 = 0;
    int num2 = WordWrap.BreakArray.Length;
    while (num1 <= num2)
    {
      int index = (num2 - num1) / 2 + num1;
      if ((int) WordWrap.BreakArray[index].Character == (int) c)
        return WordWrap.BreakArray[index].IsNonEndingCharacter;
      if ((uint) c < WordWrap.BreakArray[index].Character)
        num2 = index - 1;
      else
        num1 = index + 1;
    }
    return false;
  }

  public static void SetCallback(WordWrap.CB_GetWidth cbGetWidth, WordWrap.CB_Reserved pReserved)
  {
    if (cbGetWidth != null)
      WordWrap.GetWidth = cbGetWidth;
    if (pReserved == null)
      return;
    WordWrap.Reserved = pReserved;
  }

  public static bool IsEastAsianChar(char c)
  {
    if (WordWrap.nohangul == NoHangulWrap.On && ('ᄀ' <= c && c <= 'ᇿ' || '\u3130' <= c && c <= '\u318F' || '가' <= c && c <= '힣'))
      return false;
    if ('ᄀ' <= c && c <= 'ᇿ' || '　' <= c && c <= '\uD7AF' || '豈' <= c && c <= '\uFAFF')
      return true;
    return '\uFF00' <= c && c <= 'ￜ';
  }

  public static bool CanBreakLineAt(string pszStart, int index)
  {
    return index != 0 && (!WordWrap.IsWhiteSpace(pszStart[index]) || !WordWrap.IsNonBeginningChar(pszStart[index + 1])) && (index <= 1 || !WordWrap.IsWhiteSpace(pszStart[index - 2]) || pszStart[index - 1] != '"' || WordWrap.IsWhiteSpace(pszStart[index])) && (WordWrap.IsWhiteSpace(pszStart[index - 1]) || pszStart[index] != '"' || !WordWrap.IsWhiteSpace(pszStart[index + 1])) && (WordWrap.IsWhiteSpace(pszStart[index]) || WordWrap.IsEastAsianChar(pszStart[index]) || WordWrap.IsEastAsianChar(pszStart[index - 1]) || pszStart[index - 1] == '-') && !WordWrap.IsNonBeginningChar(pszStart[index]) && !WordWrap.IsNonEndingChar(pszStart[index - 1]);
  }

  public static string FindNonWhiteSpaceForward(string text)
  {
    int num = 0;
    while (num < text.Length && WordWrap.IsWhiteSpace(text[num]))
      ++num;
    if (num < text.Length && WordWrap.IsLineFeed(text[num]))
      ++num;
    return num >= text.Length ? (string) null : text.Substring(num);
  }

  public static string FindNonWhiteSpaceBackward(string source, int index, out int offset)
  {
    while (index >= 0 && (WordWrap.IsWhiteSpace(source[index]) || WordWrap.IsLineFeed(source[index])))
      --index;
    offset = index;
    return index >= 0 ? source.Substring(index) : (string) null;
  }

  public static bool IsWhiteSpace(char c) => c == '\t' || c == '\r' || c == ' ' || c == '　';

  public static bool IsLineFeed(char c) => c == '\n';

  public static string FindNextLine(
    SpriteFont spriteFont,
    string source,
    uint width,
    out string EOL,
    out int EOLOffset)
  {
    if (WordWrap.GetWidth == null || source == null)
    {
      EOL = (string) null;
      EOLOffset = -1;
      return (string) null;
    }
    int num1 = 0;
    uint num2 = 0;
    for (; num1 < source.Length && !WordWrap.IsLineFeed(source[num1]); ++num1)
    {
      num2 += WordWrap.GetWidth(spriteFont, source[num1]);
      if (num2 > width)
        break;
    }
    if (num1 == 0)
    {
      EOL = WordWrap.FindNonWhiteSpaceBackward(source, num1, out EOLOffset);
      return WordWrap.FindNonWhiteSpaceForward(source.Substring(num1 + 1));
    }
    if (num2 <= width)
    {
      EOL = WordWrap.FindNonWhiteSpaceBackward(source, num1 - 1, out EOLOffset);
      if (num1 - 1 >= 0 && WordWrap.IsLineFeed(source[num1 - 1]))
        return source.Substring(num1);
      return num1 < 0 || num1 >= source.Length ? (string) null : WordWrap.FindNonWhiteSpaceForward(source.Substring(num1));
    }
    int startIndex = num1;
    for (; num1 > 0; --num1)
    {
      if (WordWrap.IsWhiteSpace(source[num1]))
      {
        EOL = WordWrap.FindNonWhiteSpaceBackward(source, num1, out EOLOffset);
        if (EOL != null)
          return WordWrap.FindNonWhiteSpaceForward(source.Substring(num1 + 1));
        num1 = EOLOffset + 1;
      }
      if (WordWrap.CanBreakLineAt(source, num1))
        break;
    }
    if (num1 <= 0)
    {
      EOL = source.Substring(startIndex - 1);
      EOLOffset = startIndex - 1;
      return source.Substring(startIndex);
    }
    EOL = source.Substring(num1 - 1);
    EOLOffset = num1 - 1;
    return WordWrap.FindNonWhiteSpaceForward(source.Substring(num1));
  }

  public static Prohibition ProhibitionSetting
  {
    get => WordWrap.prohibition;
    set => WordWrap.prohibition = value;
  }

  public static NoHangulWrap NoHangulWrapSetting
  {
    get => WordWrap.nohangul;
    set => WordWrap.nohangul = value;
  }

  public delegate uint CB_GetWidth(SpriteFont spriteFont, char c);

  public delegate uint CB_Reserved();
}
