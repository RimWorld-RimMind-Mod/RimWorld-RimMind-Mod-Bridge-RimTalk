namespace RimMind.Bridge.RimTalk.Settings
{
    public partial class BridgeRimTalkSettings
    {
        public bool enableDialogueGate = true;
        public bool skipChitchat = true;
        public bool skipAutoDialogue = true;
        public bool skipPlayerDialogue = true;
        public bool forceRimMindPlayerDialogue;

        public bool enableContextPush = true;
        public bool pushPersonality = true;
        public bool pushStoryteller = true;
        public bool pushMemory;
        public bool pushAdvisorLog = true;
        public bool pushShaping;
        public bool injectPersonaToTraits;
        public bool injectPersonaToMood;

        public bool enableContextPull = true;
        public bool pullRimTalkHistory = true;

        private static BridgeRimTalkSettings? _instance;
        private static bool _dirty;

        public BridgeRimTalkSettings()
        {
            _instance = this;
        }

        public static BridgeRimTalkSettings Get() =>
            _instance ?? new BridgeRimTalkSettings();

        internal static void ResetForTesting()
        {
            _instance = null;
            _dirty = false;
        }

        internal static (
            bool,
            bool,
            bool,
            bool,
            bool,
            bool,
            bool,
            bool,
            bool,
            bool,
            bool,
            bool,
            bool,
            bool,
            bool) CaptureSnapshot()
        {
            BridgeRimTalkSettings settings = Get();
            return (
                settings.enableDialogueGate,
                settings.skipChitchat,
                settings.skipAutoDialogue,
                settings.skipPlayerDialogue,
                settings.forceRimMindPlayerDialogue,
                settings.enableContextPush,
                settings.pushPersonality,
                settings.pushStoryteller,
                settings.pushMemory,
                settings.pushAdvisorLog,
                settings.pushShaping,
                settings.injectPersonaToTraits,
                settings.injectPersonaToMood,
                settings.enableContextPull,
                settings.pullRimTalkHistory);
        }

        internal static void MarkDirtyIfChanged(
            (
                bool,
                bool,
                bool,
                bool,
                bool,
                bool,
                bool,
                bool,
                bool,
                bool,
                bool,
                bool,
                bool,
                bool,
                bool) before)
        {
            if (before != CaptureSnapshot())
                _dirty = true;
        }

        internal static bool IsDirtyForTesting => _dirty;

        internal static void ResetDirtyForTesting() => _dirty = false;
    }
}
