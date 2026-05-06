using RimMind.Contracts.Extension;
using RimMind.Bridge.RimTalk.Settings;
using UnityEngine;

namespace RimMind.Bridge.RimTalk
{
    internal sealed class RimTalkSettingsTab : ISettingsTab
    {
        public string Id => "bridge_rimtalk";
        public string Label => "RimMind.BridgeRimTalk.Settings.TabLabel".Translate();
        public void Draw(Rect rect) => BridgeRimTalkSettings.DrawSettingsContent(rect);
    }
}
