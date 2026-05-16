using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Presentation.Settings;
using RimMind.Bridge.RimTalk.Settings;
using UnityEngine;
using Verse;

namespace RimMind.Bridge.RimTalk
{
    internal sealed class RimTalkSettingsTab : ISettingsTab
    {
        public string Id => "bridge_rimtalk";
        public string Label => "RimMind.BridgeRimTalk.Settings.TabLabel".Translate();
        public void Draw(Rect rect) => BridgeRimTalkSettings.DrawSettingsContent(rect);
    }
}
