// Decompiled with JetBrains decompiler
// Type: FezGame.Components.ISpeechBubbleManager
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Microsoft.Xna.Framework;

#nullable disable
namespace FezGame.Components;

public interface ISpeechBubbleManager
{
  void ChangeText(string toText);

  void Hide();

  Vector3 Origin { set; }

  SpeechFont Font { set; }

  bool Hidden { get; }

  void ForceDrawOrder(int drawOrder);

  void RevertDrawOrder();
}
