// WkEditorPrefsTests.cs

using NUnit.Framework;
using UnityEditor;
using WhyKnot.Core.Utilities;

namespace WhyKnot.Core.Tests {

    public sealed class WkEditorPrefsTests {

        private const string TestPackage = "dev.whyknot.tests-editor-prefs";

        [TearDown]
        public void TearDown() {
            // Delete every test key we might have touched. EditorPrefs has no
            // bulk-delete, so we enumerate the ones we know about.
            foreach (var k in new[] { "alpha", "beta", "absent" }) {
                EditorPrefs.DeleteKey(TestPackage + "." + k);
            }
        }

        [Test]
        public void SetAndGet_RoundTrip() {
            WkEditorPrefs.SetBool(TestPackage, "alpha", true);
            Assert.IsTrue(WkEditorPrefs.GetBool(TestPackage, "alpha"));

            WkEditorPrefs.SetInt(TestPackage, "alpha", 42);
            Assert.AreEqual(42, WkEditorPrefs.GetInt(TestPackage, "alpha"));

            WkEditorPrefs.SetString(TestPackage, "alpha", "hello");
            Assert.AreEqual("hello", WkEditorPrefs.GetString(TestPackage, "alpha"));

            WkEditorPrefs.SetFloat(TestPackage, "alpha", 1.5f);
            Assert.AreEqual(1.5f, WkEditorPrefs.GetFloat(TestPackage, "alpha"));
        }

        [Test]
        public void Get_AbsentKey_ReturnsFallback() {
            Assert.IsFalse(WkEditorPrefs.GetBool(TestPackage, "absent", false));
            Assert.IsTrue(WkEditorPrefs.GetBool(TestPackage, "absent", true));
            Assert.AreEqual(99, WkEditorPrefs.GetInt(TestPackage, "absent", 99));
            Assert.AreEqual("def", WkEditorPrefs.GetString(TestPackage, "absent", "def"));
        }

        [Test]
        public void Namespacing_PreventsCollisionAcrossPackages() {
            WkEditorPrefs.SetInt("pkg.a", "shared-key", 1);
            WkEditorPrefs.SetInt("pkg.b", "shared-key", 2);
            Assert.AreEqual(1, WkEditorPrefs.GetInt("pkg.a", "shared-key"));
            Assert.AreEqual(2, WkEditorPrefs.GetInt("pkg.b", "shared-key"));
            EditorPrefs.DeleteKey("pkg.a.shared-key");
            EditorPrefs.DeleteKey("pkg.b.shared-key");
        }

        [Test]
        public void Delete_AndHas() {
            WkEditorPrefs.SetBool(TestPackage, "beta", true);
            Assert.IsTrue(WkEditorPrefs.Has(TestPackage, "beta"));
            WkEditorPrefs.Delete(TestPackage, "beta");
            Assert.IsFalse(WkEditorPrefs.Has(TestPackage, "beta"));
        }

        [Test]
        public void Foldout_DefaultOpen() {
            // SessionState is editor-session-scoped, no real cleanup needed
            // beyond the test methods running in sequence.
            const string key = "WkEditorPrefsTests.foldout.a";
            Assert.IsTrue(WkSessionState.Foldout(key, defaultOpen: true));
            WkSessionState.SetFoldout(key, false);
            Assert.IsFalse(WkSessionState.Foldout(key, defaultOpen: true));
        }
    }
}
