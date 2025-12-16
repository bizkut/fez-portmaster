// Decompiled with JetBrains decompiler
// Type: FezGame.Services.GameLevelMaterializer
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezGame.Services;

public class GameLevelMaterializer(Game game) : LevelMaterializer(game)
{
  public override void RebuildTrile(Trile trile)
  {
    TrileMaterializer trileMaterializer = new TrileMaterializer(trile, this.TrilesMesh, false);
    this.trileMaterializers.Add(trile, trileMaterializer);
    trileMaterializer.Geometry = trile.Geometry;
    trileMaterializer.DetermineFlags();
  }
}
