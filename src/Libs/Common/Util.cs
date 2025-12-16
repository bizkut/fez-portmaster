// Decompiled with JetBrains decompiler
// Type: Common.Util
// Assembly: Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BC7A950C-D861-40F4-B8D6-28776BD88C9A
// Assembly location: E:\GOG Games\Fez\Common.dll

using Microsoft.Xna.Framework;
using SDL2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

#nullable disable
namespace Common;

public static class Util
{
  public static readonly string LocalSaveFolder = Util.GetLocalSaveFolder();
  public static readonly string LocalConfigFolder = Util.GetLocalConfigFolder();

  private static string GetLocalSaveFolder()
  {
    string path;
    switch (SDL.SDL_GetPlatform())
    {
      case "Linux":
        string path1 = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        if (string.IsNullOrEmpty(path1))
        {
          string environmentVariable = Environment.GetEnvironmentVariable("HOME");
          if (string.IsNullOrEmpty(environmentVariable))
            return ".";
          path1 = environmentVariable + "/.local/share";
        }
        path = Path.Combine(path1, "FEZ");
        break;
      case "Mac OS X":
        string environmentVariable1 = Environment.GetEnvironmentVariable("HOME");
        if (string.IsNullOrEmpty(environmentVariable1))
          return ".";
        path = Path.Combine(environmentVariable1 + "/Library/Application Support", "FEZ");
        break;
      case "Windows":
        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FEZ");
        break;
      default:
        throw new NotImplementedException("Unhandled SDL2 platform!");
    }
    if (!Directory.Exists(path))
      Directory.CreateDirectory(path);
    return path;
  }

  private static string GetLocalConfigFolder()
  {
    string path;
    switch (SDL.SDL_GetPlatform())
    {
      case "Linux":
        string path1 = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (string.IsNullOrEmpty(path1))
        {
          string environmentVariable = Environment.GetEnvironmentVariable("HOME");
          if (string.IsNullOrEmpty(environmentVariable))
            return ".";
          path1 = environmentVariable + "/.config";
        }
        path = Path.Combine(path1, "FEZ");
        break;
      case "Mac OS X":
        string environmentVariable1 = Environment.GetEnvironmentVariable("HOME");
        if (string.IsNullOrEmpty(environmentVariable1))
          return ".";
        path = Path.Combine(environmentVariable1 + "/Library/Application Support", "FEZ");
        break;
      case "Windows":
        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FEZ");
        break;
      default:
        throw new NotImplementedException("Unhandled SDL2 platform!");
    }
    if (!Directory.Exists(path))
      Directory.CreateDirectory(path);
    return path;
  }

  private static unsafe void Hash(byte* d, int len, ref uint h)
  {
    for (int index = 0; index < len; ++index)
    {
      h += (uint) d[index];
      h += h << 10;
      h ^= h >> 6;
    }
  }

  public static unsafe void Hash(ref uint h, string s)
  {
    fixed (char* d = s)
      Util.Hash((byte*) d, s.Length * 2, ref h);
  }

  public static unsafe void Hash(ref uint h, int data) => Util.Hash((byte*) &data, 4, ref h);

  public static unsafe void Hash(ref uint h, long data) => Util.Hash((byte*) &data, 8, ref h);

  public static unsafe void Hash(ref uint h, bool data) => Util.Hash((byte*) &data, 1, ref h);

  public static unsafe void Hash(ref uint h, float data) => Util.Hash((byte*) &data, 4, ref h);

  public static int Avalanche(uint h)
  {
    h += h << 3;
    h ^= h >> 11;
    h += h << 15;
    return (int) h;
  }

  public static bool ArrayEquals<T>(T[] a, T[] b) where T : struct
  {
    if (a == null != (b == null))
      return false;
    if (a == null)
      return true;
    if (a.Length != b.Length)
      return false;
    for (int index = 0; index < a.Length; ++index)
    {
      if (!b[index].Equals((object) a[index]))
        return false;
    }
    return true;
  }

  public static bool ContainsIgnoreCase(this string source, string value)
  {
    return source.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) != -1;
  }

  public static int CombineHashCodes(int first, int second, int third, int fourth)
  {
    uint h = 0;
    Util.Hash(ref h, first);
    Util.Hash(ref h, second);
    Util.Hash(ref h, third);
    Util.Hash(ref h, fourth);
    return Util.Avalanche(h);
  }

  public static int CombineHashCodes(int first, int second, int third)
  {
    uint h = 0;
    Util.Hash(ref h, first);
    Util.Hash(ref h, second);
    Util.Hash(ref h, third);
    return Util.Avalanche(h);
  }

  public static int CombineHashCodes(int first, int second)
  {
    uint h = 0;
    Util.Hash(ref h, first);
    Util.Hash(ref h, second);
    return Util.Avalanche(h);
  }

  public static int CombineHashCodes(params object[] keys)
  {
    uint h = 0;
    foreach (object key in keys)
      Util.Hash(ref h, key == null ? 0 : key.GetHashCode());
    return Util.Avalanche(h);
  }

  public static string DeepToString<T>(IEnumerable<T> collection)
  {
    return Util.DeepToString<T>(collection, false);
  }

  public static string DeepToString<T>(IEnumerable<T> collection, bool omitBrackets)
  {
    StringBuilder stringBuilder = new StringBuilder(omitBrackets ? string.Empty : "{");
    foreach (T obj in collection)
    {
      stringBuilder.Append((object) obj == null ? string.Empty : obj.ToString());
      stringBuilder.Append(", ");
    }
    if (stringBuilder.Length >= 2)
      stringBuilder.Remove(stringBuilder.Length - 2, 2);
    if (!omitBrackets)
      stringBuilder.Append("}");
    return stringBuilder.ToString();
  }

  public static string ReflectToString(object obj)
  {
    StringBuilder stringBuilder = new StringBuilder("{");
    MemberInfo[] serializableMembers = ReflectionHelper.GetSerializableMembers(obj.GetType());
    for (int index = 0; index < serializableMembers.Length; ++index)
    {
      MemberInfo member = serializableMembers[index];
      stringBuilder.AppendFormat("{0}:{1}", (object) member.Name, ReflectionHelper.GetValue(member, obj));
      if (index != serializableMembers.Length - 1)
        stringBuilder.Append(", ");
    }
    stringBuilder.Append("}");
    return stringBuilder.ToString();
  }

  public static string CompactToString(this Matrix matrix)
  {
    return $"{matrix.M11:0.##} {matrix.M12:0.##} {matrix.M13:0.##} {matrix.M14:0.##} | {matrix.M21:0.##} {matrix.M22:0.##} {matrix.M23:0.##} {matrix.M24:0.##} | {matrix.M31:0.##} {matrix.M32:0.##} {matrix.M33:0.##} {matrix.M34:0.##} | {matrix.M41:0.##} {matrix.M42:0.##} {matrix.M43:0.##} {matrix.M44:0.##}";
  }

  public static T[] JoinArrays<T>(T[] first, T[] second)
  {
    T[] destinationArray = new T[first.Length + second.Length];
    Array.Copy((Array) first, (Array) destinationArray, first.Length);
    Array.Copy((Array) second, 0, (Array) destinationArray, first.Length, second.Length);
    return destinationArray;
  }

  public static T[] AppendToArray<T>(T[] array, T element)
  {
    T[] destinationArray = new T[array.Length + 1];
    Array.Copy((Array) array, (Array) destinationArray, array.Length);
    destinationArray[array.Length] = element;
    return destinationArray;
  }

  public static string StripExtensions(string path)
  {
    int startIndex = path.LastIndexOf('\\') + 1;
    return path.Substring(0, path.IndexOf('.', startIndex));
  }

  public static string GetFileNameWithoutAnyExtension(string path)
  {
    int startIndex = path.LastIndexOf('\\') + 1;
    return path.Substring(startIndex, path.IndexOf('.', startIndex) - startIndex);
  }

  public static string AllExtensions(this FileInfo file)
  {
    int startIndex = file.FullName.LastIndexOf('\\');
    return file.FullName.IndexOf('.', startIndex) == -1 ? "" : file.FullName.Substring(file.FullName.IndexOf('.', startIndex));
  }

  public static Array GetValues(Type t) => Enum.GetValues(t);

  public static IEnumerable<T> GetValues<T>() => Enum.GetValues(typeof (T)).Cast<T>();

  public static IEnumerable<string> GetNames<T>()
  {
    return (IEnumerable<string>) Enum.GetNames(typeof (T));
  }

  public static string GetName<T>(object value) => Util.GetName(typeof (T), value);

  public static string GetName(Type t, object value) => Enum.GetName(t, value);

  public static bool Implements(this Type type, Type interfaceType)
  {
    return ((IEnumerable<Type>) type.GetInterfaces()).Contains<Type>(interfaceType);
  }

  public static Color FromName(string name)
  {
    Color color = Color.White;
    switch (name.ToUpper(CultureInfo.InvariantCulture))
    {
      case "BLACK":
        color = Color.Black;
        break;
      case "BLUE":
        color = Color.Blue;
        break;
      case "CYAN":
        color = Color.Cyan;
        break;
      case "GREEN":
        color = Color.Green;
        break;
      case "MAGENTA":
        color = Color.Magenta;
        break;
      case "RED":
        color = Color.Red;
        break;
      case "WHITE":
        color = Color.White;
        break;
      case "YELLOW":
        color = Color.Yellow;
        break;
    }
    return color;
  }

  public static void ColorToHSV(
    Color color,
    out double hue,
    out double saturation,
    out double value)
  {
    int num1 = (int) Math.Max(color.R, Math.Max(color.G, color.B));
    int num2 = (int) Math.Min(color.R, Math.Min(color.G, color.B));
    hue = (double) color.GetHue();
    saturation = num1 == 0 ? 0.0 : 1.0 - 1.0 * (double) num2 / (double) num1;
    value = (double) num1 / (double) byte.MaxValue;
  }

  public static float GetHue(this Color color)
  {
    if ((int) color.R == (int) color.G && (int) color.G == (int) color.B)
      return 0.0f;
    float num1 = (float) color.R / (float) byte.MaxValue;
    float num2 = (float) color.G / (float) byte.MaxValue;
    float num3 = (float) color.B / (float) byte.MaxValue;
    float num4 = 0.0f;
    float num5 = num1;
    float num6 = num1;
    if ((double) num2 > (double) num5)
      num5 = num2;
    if ((double) num3 > (double) num5)
      num5 = num3;
    if ((double) num2 < (double) num6)
      num6 = num2;
    if ((double) num3 < (double) num6)
      num6 = num3;
    float num7 = num5 - num6;
    if ((double) num1 == (double) num5)
      num4 = (num2 - num3) / num7;
    else if ((double) num2 == (double) num5)
      num4 = (float) (2.0 + ((double) num3 - (double) num1) / (double) num7);
    else if ((double) num3 == (double) num5)
      num4 = (float) (4.0 + ((double) num1 - (double) num2) / (double) num7);
    float hue = num4 * 60f;
    if ((double) hue < 0.0)
      hue += 360f;
    return hue;
  }

  public static Color ColorFromHSV(double hue, double saturation, double value)
  {
    int num1 = (int) (hue / 60.0) % 6;
    double num2 = hue / 60.0 - (double) (int) (hue / 60.0);
    value *= (double) byte.MaxValue;
    byte num3 = (byte) value;
    byte num4 = (byte) (value * (1.0 - saturation));
    byte num5 = (byte) (value * (1.0 - num2 * saturation));
    byte num6 = (byte) (value * (1.0 - (1.0 - num2) * saturation));
    switch (num1)
    {
      case 0:
        return new Color((int) num3, (int) num6, (int) num4);
      case 1:
        return new Color((int) num5, (int) num3, (int) num4);
      case 2:
        return new Color((int) num4, (int) num3, (int) num6);
      case 3:
        return new Color((int) num4, (int) num5, (int) num3);
      case 4:
        return new Color((int) num6, (int) num4, (int) num3);
      default:
        return new Color((int) num3, (int) num4, (int) num5);
    }
  }

  public static string StripPunctuation(this string s)
  {
    StringBuilder stringBuilder = new StringBuilder();
    foreach (char c in s)
    {
      if (!char.IsPunctuation(c))
        stringBuilder.Append(c);
    }
    return stringBuilder.ToString();
  }

  public static void NullAction()
  {
  }

  public static void NullAction<T>(T t)
  {
  }

  public static void NullAction<T, U>(T t, U u)
  {
  }

  public static void NullAction<T, U, V>(T t, U u, V v)
  {
  }

  public static void NullAction<T, U, V, W>(T t, U u, V v, W w)
  {
  }

  public static TResult NullFunc<TResult>() => default (TResult);

  public static TResult NullFunc<T, TResult>(T t) => default (TResult);

  public static TResult NullFunc<T, U, TResult>(T t, U u) => default (TResult);

  public static TResult NullFunc<T, U, V, TResult>(T t, U u, V v) => default (TResult);

  public static TResult NullFunc<T, U, V, W, TResult>(T t, U u, V v, W w) => default (TResult);
}
