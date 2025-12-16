// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.NpcService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components.Scripting;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Services.Scripting;

internal class NpcService : INpcService, IScriptingBase
{
  public LongRunningAction Say(int id, string line, string customSound, string customAnimation)
  {
    SpeechLine speechLine = new SpeechLine() { Text = line };
    NpcInstance npc = this.LevelManager.NonPlayerCharacters[id];
    if (!string.IsNullOrEmpty(customSound))
    {
      if (speechLine.OverrideContent == null)
        speechLine.OverrideContent = new NpcActionContent();
      speechLine.OverrideContent.Sound = this.LoadSound(customSound);
    }
    if (!string.IsNullOrEmpty(customAnimation))
    {
      if (speechLine.OverrideContent == null)
        speechLine.OverrideContent = new NpcActionContent();
      speechLine.OverrideContent.Animation = this.LoadAnimation(npc, customAnimation);
    }
    npc.CustomSpeechLine = speechLine;
    return new LongRunningAction((Func<float, float, bool>) ((_, __) => npc.CustomSpeechLine == null));
  }

  public void CarryGeezerLetter(int id)
  {
    ServiceHelper.AddComponent((IGameComponent) new GeezerLetterSender(ServiceHelper.Game, id));
  }

  private AnimatedTexture LoadAnimation(NpcInstance npc, string name)
  {
    return this.CMProvider.CurrentLevel.Load<AnimatedTexture>($"Character Animations/{npc.Name}/{name}");
  }

  private SoundEffect LoadSound(string name)
  {
    return this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Npc/" + name);
  }

  public void ResetEvents()
  {
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }
}
