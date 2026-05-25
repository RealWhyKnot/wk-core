// WkThemeTests.cs

using NUnit.Framework;
using UnityEngine;
using WhyKnot.Core.Styling;

namespace WhyKnot.Core.Tests {

    public sealed class WkThemeTests {

        [Test]
        public void Presets_HaveFiniteColors_InBothVariants() {
            AssertVariantFinite(WkTheme.WhyKnot.Pro, "WhyKnot.Pro");
            AssertVariantFinite(WkTheme.WhyKnot.Personal, "WhyKnot.Personal");
            AssertVariantFinite(WkTheme.VRCFury.Pro, "VRCFury.Pro");
            AssertVariantFinite(WkTheme.VRCFury.Personal, "VRCFury.Personal");
        }

        [Test]
        public void Scope_PushesAndPopsTheme() {
            var initial = WkStyles.CurrentTheme;
            using (WkStyles.Scope(WkTheme.VRCFury)) {
                Assert.AreSame(WkTheme.VRCFury, WkStyles.CurrentTheme);
            }
            Assert.AreSame(initial, WkStyles.CurrentTheme, "Theme should restore after scope ends");
        }

        [Test]
        public void Scope_NestsCorrectly() {
            using (WkStyles.Scope(WkTheme.WhyKnot)) {
                using (WkStyles.Scope(WkTheme.VRCFury)) {
                    Assert.AreSame(WkTheme.VRCFury, WkStyles.CurrentTheme);
                }
                Assert.AreSame(WkTheme.WhyKnot, WkStyles.CurrentTheme,
                    "Theme should pop back to the outer scope, not the global default");
            }
        }

        [Test]
        public void Scope_NullThemeIsNoop() {
            var initial = WkStyles.CurrentTheme;
            using (WkStyles.Scope(null)) {
                Assert.AreSame(initial, WkStyles.CurrentTheme,
                    "A null theme scope should not alter the current theme");
            }
            Assert.AreSame(initial, WkStyles.CurrentTheme);
        }

        [Test]
        public void DefaultTheme_IsWhyKnot() {
            // The default theme is configurable via WkStyles.DefaultTheme; the
            // out-of-the-box value is the brand palette.
            Assert.AreSame(WkTheme.WhyKnot, WkStyles.DefaultTheme);
        }

        [Test]
        public void Palette_ResolvesThroughCurrentTheme() {
            // ColorDanger reads from CurrentTheme.Current.Danger. The two
            // presets have different danger colors, so swapping the scope
            // must surface a different value.
            Color whyknotDanger;
            Color vrcfuryDanger;
            using (WkStyles.Scope(WkTheme.WhyKnot)) {
                whyknotDanger = WkStyles.ColorDanger;
            }
            using (WkStyles.Scope(WkTheme.VRCFury)) {
                vrcfuryDanger = WkStyles.ColorDanger;
            }
            Assert.AreNotEqual(whyknotDanger, vrcfuryDanger,
                "Different themes must surface different palette values");
        }

        private static void AssertVariantFinite(WkTheme.Variant v, string label) {
            AssertFiniteColor(v.Background,         label + ".Background");
            AssertFiniteColor(v.BackgroundAlt,      label + ".BackgroundAlt");
            AssertFiniteColor(v.BackgroundEmphasis, label + ".BackgroundEmphasis");
            AssertFiniteColor(v.Accent,             label + ".Accent");
            AssertFiniteColor(v.Warning,            label + ".Warning");
            AssertFiniteColor(v.Success,            label + ".Success");
            AssertFiniteColor(v.Info,               label + ".Info");
            AssertFiniteColor(v.Danger,             label + ".Danger");
            AssertFiniteColor(v.Divider,            label + ".Divider");
            AssertFiniteColor(v.DividerSubtle,      label + ".DividerSubtle");
            AssertFiniteColor(v.TextPrimary,        label + ".TextPrimary");
            AssertFiniteColor(v.TextMuted,          label + ".TextMuted");
            AssertFiniteColor(v.Border,             label + ".Border");
            AssertFiniteColor(v.ButtonHover,        label + ".ButtonHover");
        }

        private static void AssertFiniteColor(Color c, string label) {
            Assert.IsFalse(float.IsNaN(c.r) || float.IsNaN(c.g) || float.IsNaN(c.b) || float.IsNaN(c.a),
                $"{label} contains NaN: {c}");
        }
    }
}
