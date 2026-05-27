// WkStylesTests.cs
//
// EditMode tests for the lazy-init pattern in WkStyles. We don't render
// these styles -- we only assert the backing fields populate on first
// access, return the same instance on the second access (the lazy cache
// is doing its job), and that the palette properties produce finite
// colors.

using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WhyKnot.Core.Styling;

namespace WhyKnot.Core.Tests {

    public sealed class WkStylesTests {

        [Test]
        public void Palette_ReturnsFiniteColors() {
            AssertFiniteColor(WkStyles.ColorAccent);
            AssertFiniteColor(WkStyles.ColorWarning);
            AssertFiniteColor(WkStyles.ColorSuccess);
            AssertFiniteColor(WkStyles.ColorInfo);
            AssertFiniteColor(WkStyles.ColorDivider);
        }

        [Test]
        public void LazyStyles_AreNonNullAndCached() {
            AssertCached(() => WkStyles.SectionTitle);
            AssertCached(() => WkStyles.SubsectionTitle);
            AssertCached(() => WkStyles.Body);
            AssertCached(() => WkStyles.Muted);
            AssertCached(() => WkStyles.Mono);
            AssertCached(() => WkStyles.PrimaryButton);
            AssertCached(() => WkStyles.MiniRowButton);
            AssertCached(() => WkStyles.BadgePillStyle);
            AssertCached(() => WkStyles.CardSelected);
            AssertCached(() => WkStyles.FoldoutHeader);
            AssertCached(() => WkStyles.Caption);
            AssertCached(() => WkStyles.Code);
            AssertCached(() => WkStyles.TitleBarStyle);
            AssertCached(() => WkStyles.RowAlt);
            AssertCached(() => WkStyles.BrandFooterStyle);
        }

        [Test]
        public void NoticeKind_HasExpectedValues() {
            Assert.IsTrue(System.Enum.IsDefined(typeof(NoticeKind), NoticeKind.Info));
            Assert.IsTrue(System.Enum.IsDefined(typeof(NoticeKind), NoticeKind.Warning));
            Assert.IsTrue(System.Enum.IsDefined(typeof(NoticeKind), NoticeKind.Success));
            Assert.IsTrue(System.Enum.IsDefined(typeof(NoticeKind), NoticeKind.Danger));
            Assert.AreEqual(4, System.Enum.GetValues(typeof(NoticeKind)).Length);
        }

        [Test]
        public void ColorForKind_RoutesEachKindThroughTheTheme() {
            using (WkStyles.Scope(WkTheme.WhyKnot)) {
                Assert.AreEqual(WkStyles.ColorInfo,    WkStyles.ColorForKind(NoticeKind.Info));
                Assert.AreEqual(WkStyles.ColorWarning, WkStyles.ColorForKind(NoticeKind.Warning));
                Assert.AreEqual(WkStyles.ColorSuccess, WkStyles.ColorForKind(NoticeKind.Success));
                Assert.AreEqual(WkStyles.ColorDanger,  WkStyles.ColorForKind(NoticeKind.Danger));
            }
        }

        [Test]
        public void ExtendedPalette_ReturnsFiniteColors() {
            AssertFiniteColor(WkStyles.ColorDanger);
            AssertFiniteColor(WkStyles.ColorDividerSubtle);
            AssertFiniteColor(WkStyles.ColorBackground);
            AssertFiniteColor(WkStyles.ColorBackgroundAlt);
            AssertFiniteColor(WkStyles.ColorBackgroundEmphasis);
            AssertFiniteColor(WkStyles.ColorTextPrimary);
            AssertFiniteColor(WkStyles.ColorTextMuted);
            AssertFiniteColor(WkStyles.ColorBorder);
            AssertFiniteColor(WkStyles.ColorButtonHover);
        }

        [Test]
        public void LabelColumn_HasReasonableDefault() {
            // 110 is the historical default; if this changes downstream call
            // sites that pass labelWidth explicitly may need a sanity check.
            Assert.AreEqual(110f, WkStyles.LabelColumn);
        }

        [Test]
        public void BrandFooterText_IdentifiesWhyKnot() {
            StringAssert.Contains("Made with", WkStyles.BrandFooterText);
            StringAssert.Contains("\u2665", WkStyles.BrandFooterText);
            StringAssert.Contains("WhyKnot", WkStyles.BrandFooterText);
            Assert.AreEqual("WhyKnotLogo", WkStyles.BrandLogoAssetName);
        }

        [Test]
        public void TitleContent_UsesBrandTitleAndTooltip() {
            var content = WkStyles.TitleContent("Sample Tool", "Sample tooltip");
            Assert.AreEqual("Sample Tool", content.text);
            Assert.AreEqual("Sample tooltip", content.tooltip);
        }

        [Test]
        public void AnimatedAccentColor_ReturnsFiniteColors() {
            AssertFiniteColor(WkStyles.AnimatedAccentColor(0));
            AssertFiniteColor(WkStyles.AnimatedAccentColor(1.25));
            AssertFiniteColor(WkStyles.AnimatedAccentColor(10));
        }

        [Test]
        public void RepaintAnimatedChrome_AllowsNullWindow() {
            double next = 0;
            WkStyles.RepaintAnimatedChrome(null, ref next);
            Assert.AreEqual(0, next);
        }

        private static void AssertFiniteColor(Color c) {
            Assert.IsFalse(float.IsNaN(c.r) || float.IsNaN(c.g) || float.IsNaN(c.b) || float.IsNaN(c.a),
                $"Color contains NaN component: {c}");
            Assert.GreaterOrEqual(c.a, 0f, $"Alpha negative in {c}");
            Assert.LessOrEqual(c.a, 1f, $"Alpha >1 in {c}");
        }

        private static void AssertCached(System.Func<GUIStyle> get) {
            // EditorStyles isn't initialised in -batchmode -nographics, so
            // `new GUIStyle(EditorStyles.boldLabel)` throws on its native
            // call. Skip the assertion in that environment -- the cache
            // pattern itself is exercised when the user runs the tests
            // interactively (which is the path we actually care about).
            GUIStyle a;
            try {
                a = get();
            } catch (System.NullReferenceException) {
                Assert.Ignore("EditorStyles not initialised in this headless run");
                return;
            }
            Assert.IsNotNull(a, "Lazy style returned null");
            var b = get();
            Assert.AreSame(a, b, "Lazy style not cached -- second call returned a different instance");
        }
    }
}
