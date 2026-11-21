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

        public BridgeRimTalkSettings()
        {
            _instance = this;
        }

        public static BridgeRimTalkSettings Get() =>
            _instance ?? new BridgeRimTalkSettings();

        internal static void ResetForTesting() => _instance = null;

        public void ApplyDefaults()
        {
            enableDialogueGate = true;
            skipChitchat = true;
            skipAutoDialogue = true;
            skipPlayerDialogue = true;
            forceRimMindPlayerDialogue = false;

            enableContextPush = true;
            pushPersonality = true;
            pushStoryteller = true;
            pushMemory = false;
            pushAdvisorLog = true;
            pushShaping = false;
            injectPersonaToTraits = false;
            injectPersonaToMood = false;

            enableContextPull = true;
            pullRimTalkHistory = true;
        }
    }
}
