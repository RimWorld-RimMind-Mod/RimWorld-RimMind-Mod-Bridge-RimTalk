using System.Text;
using RimMind.Personality.Data;

namespace RimMind.Bridge.RimTalk.Bridge
{
    /// <summary>
    /// 统一构建 PersonalityProfile 的对外展示文本（description + [Work] + [Social]）。
    /// 供 ContextPushBridge 与 PersonaPushBridge 复用，避免重复 StringBuilder 逻辑。
    /// 注意：aiNarrative 段不在此处拼接，由调用方按需附加（保持原有行为契约）。
    /// </summary>
    public static class PersonaFormatter
    {
        public static string BuildFullProfile(PersonalityProfile profile)
        {
            if (profile == null || profile.IsEmpty) return "";

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(profile.description))
                sb.AppendLine(profile.description);
            if (!string.IsNullOrEmpty(profile.workTendencies))
                sb.AppendLine($"[Work] {profile.workTendencies}");
            if (!string.IsNullOrEmpty(profile.socialTendencies))
                sb.AppendLine($"[Social] {profile.socialTendencies}");
            return sb.ToString().TrimEnd();
        }
    }
}
