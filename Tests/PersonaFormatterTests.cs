using RimMind.Bridge.RimTalk.Bridge;
using RimMind.Personality.Data;
using Xunit;

namespace RimMind.Bridge.RimTalk.Tests
{
    public class PersonaFormatterTests
    {
        [Fact]
        public void BuildFullProfile_NullProfile_ReturnsEmpty()
        {
            Assert.Equal("", PersonaFormatter.BuildFullProfile(null!));
        }

        [Fact]
        public void BuildFullProfile_EmptyProfile_ReturnsEmpty()
        {
            var profile = new PersonalityProfile();
            Assert.Equal("", PersonaFormatter.BuildFullProfile(profile));
        }

        [Fact]
        public void BuildFullProfile_WithDescription_ReturnsDescription()
        {
            var profile = new PersonalityProfile { description = "Brave" };
            Assert.Equal("Brave", PersonaFormatter.BuildFullProfile(profile));
        }

        [Fact]
        public void BuildFullProfile_WithWorkAndSocial_ReturnsAllSections()
        {
            var profile = new PersonalityProfile
            {
                description = "Brave",
                workTendencies = "Hardworking",
                socialTendencies = "Friendly"
            };
            var result = PersonaFormatter.BuildFullProfile(profile);
            Assert.Contains("Brave", result);
            Assert.Contains("[Work] Hardworking", result);
            Assert.Contains("[Social] Friendly", result);
        }

        [Fact]
        public void BuildFullProfile_WithOnlyWork_ReturnsWorkSection()
        {
            var profile = new PersonalityProfile { workTendencies = "Diligent" };
            var result = PersonaFormatter.BuildFullProfile(profile);
            Assert.Equal("[Work] Diligent", result);
        }

        [Fact]
        public void BuildFullProfile_WithOnlySocial_ReturnsSocialSection()
        {
            var profile = new PersonalityProfile { socialTendencies = "Outgoing" };
            var result = PersonaFormatter.BuildFullProfile(profile);
            Assert.Equal("[Social] Outgoing", result);
        }

        [Fact]
        public void BuildFullProfile_DoesNotIncludeAiNarrative()
        {
            // BuildFullProfile 仅构建 description/work/social 三段，
            // aiNarrative 由调用方自行附加（保持 ContextPushBridge 原有行为）。
            var profile = new PersonalityProfile
            {
                description = "Brave",
                aiNarrative = "Should not appear"
            };
            var result = PersonaFormatter.BuildFullProfile(profile);
            Assert.Contains("Brave", result);
            Assert.DoesNotContain("Should not appear", result);
            Assert.DoesNotContain("[AI]", result);
        }

        [Fact]
        public void BuildFullProfile_NoTrailingNewline()
        {
            var profile = new PersonalityProfile
            {
                description = "Brave",
                workTendencies = "Hardworking"
            };
            var result = PersonaFormatter.BuildFullProfile(profile);
            Assert.False(result.EndsWith("\n"));
            Assert.False(result.EndsWith("\r"));
        }
    }
}
