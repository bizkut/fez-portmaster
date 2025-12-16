// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.ActorTypeExtensions
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Structure;

public static class ActorTypeExtensions
{
  public static string GetArtObjectName(this ActorType type)
  {
    switch (type)
    {
      case ActorType.NumberCube:
        return "NUMBER_CUBEAO";
      case ActorType.LetterCube:
        return "LETTER_CUBEAO";
      case ActorType.TriSkull:
        return "TRI_SKULLAO";
      case ActorType.Tome:
        return "TOMEAO";
      default:
        return (string) null;
    }
  }

  public static Vector2 GetArtifactOffset(this ActorType type)
  {
    switch (type)
    {
      case ActorType.NumberCube:
      case ActorType.LetterCube:
      case ActorType.Tome:
        return new Vector2(6.5f);
      case ActorType.TriSkull:
        return new Vector2(4f, 3f);
      default:
        return Vector2.Zero;
    }
  }

  public static bool IsTreasure(this ActorType type)
  {
    switch (type)
    {
      case ActorType.CubeShard:
      case ActorType.SkeletonKey:
      case ActorType.NumberCube:
      case ActorType.LetterCube:
      case ActorType.TriSkull:
      case ActorType.Tome:
      case ActorType.SecretCube:
      case ActorType.TreasureMap:
      case ActorType.Mail:
      case ActorType.PieceOfHeart:
        return true;
      default:
        return false;
    }
  }

  public static bool IsCollectible(this ActorType type) => type == ActorType.GoldenCube;

  public static bool IsPickable(this ActorType type)
  {
    switch (type)
    {
      case ActorType.PickUp:
      case ActorType.Bomb:
      case ActorType.Vase:
      case ActorType.BigBomb:
      case ActorType.TntPickup:
      case ActorType.Couch:
      case ActorType.SinkPickup:
        return true;
      default:
        return false;
    }
  }

  public static bool IsBomb(this ActorType type)
  {
    return type == ActorType.Bomb || type == ActorType.BigBomb;
  }

  public static bool IsDestructible(this ActorType type)
  {
    switch (type)
    {
      case ActorType.Destructible:
      case ActorType.DestructiblePermanent:
      case ActorType.Vase:
        return true;
      default:
        return false;
    }
  }

  public static bool IsChainsploding(this ActorType type)
  {
    switch (type)
    {
      case ActorType.Bomb:
      case ActorType.BigBomb:
      case ActorType.TntBlock:
      case ActorType.TntPickup:
        return true;
      default:
        return false;
    }
  }

  public static bool IsClimbable(this ActorType type)
  {
    return type == ActorType.Ladder || type == ActorType.Vine;
  }

  public static bool IsFragile(this ActorType type) => type == ActorType.Vase;

  public static bool IsFaceDependant(this ActorType type)
  {
    switch (type)
    {
      case ActorType.Ladder:
      case ActorType.Sign:
      case ActorType.Door:
      case ActorType.Vine:
      case ActorType.Tombstone:
      case ActorType.UnlockedDoor:
      case ActorType.Couch:
      case ActorType.Rumbler:
        return true;
      default:
        return false;
    }
  }

  public static bool IsSafe(this ActorType type)
  {
    if (type <= ActorType.PickUp)
    {
      if (type != ActorType.Bouncer && type != ActorType.PickUp)
        goto label_4;
    }
    else if (type != ActorType.Crystal && type != ActorType.Hurt && type != ActorType.LightningPlatform)
      goto label_4;
    return false;
label_4:
    return true;
  }

  public static bool SupportsPlanes(this ActorType type)
  {
    switch (type)
    {
      case ActorType.None:
      case ActorType.Waterfall:
      case ActorType.Trickle:
      case ActorType.Drips:
      case ActorType.BigWaterfall:
        return true;
      default:
        return false;
    }
  }

  public static bool SupportsArtObjects(this ActorType type)
  {
    switch (type)
    {
      case ActorType.None:
      case ActorType.Checkpoint:
      case ActorType.TreasureChest:
      case ActorType.EightBitDoor:
      case ActorType.WarpGate:
      case ActorType.OneBitDoor:
      case ActorType.SpinBlock:
      case ActorType.PivotHandle:
      case ActorType.FourBitDoor:
      case ActorType.Tombstone:
      case ActorType.SplitUpCube:
      case ActorType.Valve:
      case ActorType.Rumbler:
      case ActorType.ConnectiveRail:
      case ActorType.BoltHandle:
      case ActorType.BoltNutBottom:
      case ActorType.BoltNutTop:
      case ActorType.CodeMachine:
      case ActorType.NumberCube:
      case ActorType.LetterCube:
      case ActorType.TriSkull:
      case ActorType.Tome:
      case ActorType.LesserGate:
      case ActorType.LaserEmitter:
      case ActorType.LaserBender:
      case ActorType.LaserReceiver:
      case ActorType.RebuildingHexahedron:
      case ActorType.TreasureMap:
      case ActorType.Timeswitch:
      case ActorType.TimeswitchMovingPart:
      case ActorType.Mailbox:
      case ActorType.Bookcase:
      case ActorType.TwoBitDoor:
      case ActorType.SixteenBitDoor:
      case ActorType.ThirtyTwoBitDoor:
      case ActorType.SixtyFourBitDoor:
      case ActorType.Bell:
      case ActorType.Telescope:
      case ActorType.QrCode:
      case ActorType.FpsPost:
      case ActorType.SecretPassage:
        return true;
      default:
        return false;
    }
  }

  public static bool SupportsGroups(this ActorType type)
  {
    switch (type)
    {
      case ActorType.None:
      case ActorType.ExploSwitch:
      case ActorType.PushSwitch:
      case ActorType.PushSwitchSticky:
      case ActorType.PushSwitchPermanent:
      case ActorType.SuckBlock:
      case ActorType.Geyser:
      case ActorType.RotatingGroup:
      case ActorType.Piston:
        return true;
      default:
        return false;
    }
  }

  public static bool SupportsNPCs(this ActorType type)
  {
    return type == ActorType.None || type == ActorType.LightningGhost || type == ActorType.Owl;
  }

  public static bool SupportsTriles(this ActorType type)
  {
    if (type == ActorType.None)
      return true;
    return !type.SupportsArtObjects() && !type.SupportsGroups() && !type.SupportsNPCs() & !type.SupportsPlanes();
  }

  public static bool IsLight(this ActorType type)
  {
    return type == ActorType.Bomb || type == ActorType.Vase;
  }

  public static bool IsPushSwitch(this ActorType type)
  {
    switch (type)
    {
      case ActorType.PushSwitch:
      case ActorType.PushSwitchSticky:
      case ActorType.PushSwitchPermanent:
        return true;
      default:
        return false;
    }
  }

  public static bool IsBitDoor(this ActorType type)
  {
    switch (type)
    {
      case ActorType.EightBitDoor:
      case ActorType.OneBitDoor:
      case ActorType.FourBitDoor:
      case ActorType.TwoBitDoor:
      case ActorType.SixteenBitDoor:
      case ActorType.ThirtyTwoBitDoor:
      case ActorType.SixtyFourBitDoor:
        return true;
      default:
        return false;
    }
  }

  public static int GetBitCount(this ActorType type)
  {
    switch (type)
    {
      case ActorType.EightBitDoor:
        return 8;
      case ActorType.OneBitDoor:
        return 1;
      case ActorType.FourBitDoor:
        return 4;
      case ActorType.TwoBitDoor:
        return 2;
      case ActorType.SixteenBitDoor:
        return 16 /*0x10*/;
      case ActorType.ThirtyTwoBitDoor:
        return 32 /*0x20*/;
      case ActorType.SixtyFourBitDoor:
        return 64 /*0x40*/;
      default:
        return 0;
    }
  }

  public static bool IsHeavy(this ActorType type) => type.IsPickable() && !type.IsLight();

  public static bool IsBuoyant(this ActorType type) => type == ActorType.PickUp;

  public static bool IsDoor(this ActorType type)
  {
    return type == ActorType.Door || type == ActorType.UnlockedDoor;
  }

  public static bool IsCubeShard(this ActorType type)
  {
    return type == ActorType.CubeShard || type == ActorType.SecretCube || type == ActorType.PieceOfHeart;
  }

  public static bool UsesLasers(this ActorType type)
  {
    switch (type)
    {
      case ActorType.LaserEmitter:
      case ActorType.LaserBender:
      case ActorType.LaserReceiver:
        return true;
      default:
        return false;
    }
  }
}
