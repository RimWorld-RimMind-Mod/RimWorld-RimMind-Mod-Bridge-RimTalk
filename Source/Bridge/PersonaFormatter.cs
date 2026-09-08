using System.Text;

namespace RimMind.Bridge.RimTalk.Bridge
{
    /// <summary>
    /// 统一构建人格的对外展示文本（description + [Work] + [Social]）。
    /// 供 ContextPushBridge 与 PersonaPushBridge 复用，避免重复 StringBuilder 逻辑。
    /// 注意：AI narrative 段不在此处拼接，由调用方按需附加。
    /// </summary>
    public static class PersonaFormatter
    {
        public static string BuildFullProfile(
            string? description,
            string? workTendencies,
            string? socialTendencies)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(description))
                sb.AppendLine(description);
            if (!string.IsNullOrEmpty(workTendencies))
                sb.AppendLine($"[Work] {workTendencies}");
            if (!string.IsNullOrEmpty(socialTendencies))
                sb.AppendLine($"[Social] {socialTendencies}");
            return sb.ToString().TrimEnd();
        }
    }
}
