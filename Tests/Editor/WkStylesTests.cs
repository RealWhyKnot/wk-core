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

        private static void AssertFiniteColor(Color c) {
            Assert.IsFalse(float.IsNaN(c.r) || float.IsNaN(c.g) || float.IsNaN(c.b) || float.IsNaN(c.a),
                $"Color contains NaN component: {c}");
            Assert.GreaterOrEqual(c.a, 0f, $"Alpha negative in {c}");
            Assert.LessOrEqual(c.a, 1f, $"Alpha >1 in {c}");
        }

        private static void AssertCached(System.Func<GUIStyle> get) {
            // EditorStyles may not be initialised in headless test runs; in
            // that case the lazy getter throws and Unity surfaces the
            // underlying issue. We still want to confirm the cache by
            // calling twice and comparing instances.
            var a = get();
            Assert.IsNotNull(a, "Lazy style returned null");
            var b = get();
            Assert.AreSame(a, b, "Lazy style not cached -- second call returned a different instance");
        }
    }
}
