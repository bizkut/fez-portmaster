// Decompiled with JetBrains decompiler
// Type: FezGame.Components.IDotManager
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezGame.Components;

public interface IDotManager
{
  void Reset();

  void Burrow();

  void ComeOut();

  void MoveWithCamera(Vector3 target, bool burrowAfter);

  void SpiralAround(Volume volume, Vector3 center, bool hideDot);

  void ForceDrawOrder(int drawOrder);

  void RevertDrawOrder();

  bool DrawRays { get; set; }

  bool Hidden { get; set; }

  DotHost.BehaviourType Behaviour { get; set; }

  Vector3 Target { get; set; }

  string[] Dialog { get; set; }

  float TimeToWait { get; set; }

  Volume RoamingVolume { get; set; }

  float ScaleFactor { get; set; }

  float Opacity { get; set; }

  float ScalePulsing { get; set; }

  float RotationSpeed { get; set; }

  bool AlwaysShowLines { get; set; }

  float InnerScale { get; set; }

  Vector3 Position { get; }

  bool PreventPoI { get; set; }

  bool Burrowing { get; }

  bool ComingOut { get; }

  void Hey();

  object Owner { get; set; }

  DotFaceButton FaceButton { get; set; }

  Texture2D DestinationVignette { get; set; }

  Texture2D DestinationVignetteSony { get; set; }
}
