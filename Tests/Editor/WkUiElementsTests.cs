// WkUiElementsTests.cs
//
// Verifies the UI Toolkit factories carry the expected BEM class set
// and that the public class/variable constants stay in lockstep with
// the AllClasses / AllVariables collections.

using NUnit.Framework;
using WhyKnot.Core.Styling;

namespace WhyKnot.Core.Tests {

    public sealed class WkUiElementsTests {

        [Test]
        public void AllClasses_ContainsEveryBemClass() {
            CollectionAssert.Contains(WkUiElements.AllClasses, WkUiElements.ClassNotice);
            CollectionAssert.Contains(WkUiElements.AllClasses, WkUiElements.ClassNoticeDanger);
            CollectionAssert.Contains(WkUiElements.AllClasses, WkUiElements.ClassPill);
            CollectionAssert.Contains(WkUiElements.AllClasses, WkUiElements.ClassDivider);
            CollectionAssert.Contains(WkUiElements.AllClasses, WkUiElements.ClassButtonPrimary);
            CollectionAssert.Contains(WkUiElements.AllClasses, WkUiElements.ClassThemeWhyKnot);
            CollectionAssert.Contains(WkUiElements.AllClasses, WkUiElements.ClassSkinPro);
        }

        [Test]
        public void AllVariables_ContainsEveryColorVar() {
            CollectionAssert.Contains(WkUiElements.AllVariables, "--wk-color-background");
            CollectionAssert.Contains(WkUiElements.AllVariables, "--wk-color-accent");
            CollectionAssert.Contains(WkUiElements.AllVariables, "--wk-color-danger");
            CollectionAssert.Contains(WkUiElements.AllVariables, "--wk-color-divider-subtle");
            CollectionAssert.Contains(WkUiElements.AllVariables, "--wk-color-text-primary");
            CollectionAssert.Contains(WkUiElements.AllVariables, "--wk-color-button-hover");
        }

        [Test]
        public void Notice_CarriesBaseAndKindClasses() {
            var notice = WkUiElements.Notice(NoticeKind.Danger, "test");
            Assert.IsTrue(notice.ClassListContains(WkUiElements.ClassNotice));
            Assert.IsTrue(notice.ClassListContains(WkUiElements.ClassNoticeDanger));
        }

        [Test]
        public void Pill_CarriesBaseAndKindClasses() {
            var pill = WkUiElements.Pill("test", NoticeKind.Success);
            Assert.IsTrue(pill.ClassListContains(WkUiElements.ClassPill));
            Assert.IsTrue(pill.ClassListContains(WkUiElements.ClassPillSuccess));
        }

        [Test]
        public void PrimaryButton_CarriesPrimaryClass() {
            var btn = WkUiElements.PrimaryButton("Apply", () => { });
            Assert.IsTrue(btn.ClassListContains(WkUiElements.ClassButtonPrimary));
        }

        [Test]
        public void DangerButton_CarriesDangerClass() {
            var btn = WkUiElements.DangerButton("Stop", () => { });
            Assert.IsTrue(btn.ClassListContains(WkUiElements.ClassButtonDanger));
        }

        [Test]
        public void Divider_SubtleVariant_CarriesSubtleClass() {
            var d = WkUiElements.Divider(subtle: true);
            Assert.IsTrue(d.ClassListContains(WkUiElements.ClassDivider));
            Assert.IsTrue(d.ClassListContains(WkUiElements.ClassDividerSubtle));
        }
    }
}
