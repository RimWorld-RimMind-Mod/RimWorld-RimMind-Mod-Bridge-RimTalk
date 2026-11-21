using RimMind.Bridge.RimTalk.Settings;
using RimMind.Presentation.Api;
using RimMind.Presentation.Settings;
using UnityEngine;

namespace RimMind.Bridge.RimTalk
{
    internal sealed class RimTalkSettingsTab : ISettingsTab
    {
        public string Id => "bridge_rimtalk";
        public string OwnerModId => "RimMindBridgeRimTalk";
        public string Label => RimMindAPI.Text.TrOr("RimMind.BridgeRimTalk.Settings.TabLabel", "RimTalk Bridge");
        public void Draw(Rect rect) => BridgeRimTalkSettings.DrawSettingsContent(rect);
    }
}
