// WkGlobalIdTests.cs

using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WhyKnot.Core.Reflection;

namespace WhyKnot.Core.Tests {

    public sealed class WkGlobalIdTests {

        private const string TestKey = "WkGlobalIdTests.Probe";

        [TearDown]
        public void TearDown() {
            EditorPrefs.DeleteKey(TestKey);
        }

        [Test]
        public void Stringify_NullReturnsNull() {
            Assert.IsNull(WkGlobalId.Stringify(null));
        }

        [Test]
        public void TryResolve_MalformedReturnsFalse() {
            Assert.IsFalse(WkGlobalId.TryResolve<GameObject>("not-a-valid-global-id", out var obj));
            Assert.IsNull(obj);
        }

        [Test]
        public void TryResolve_EmptyReturnsFalse() {
            Assert.IsFalse(WkGlobalId.TryResolve<GameObject>("", out _));
            Assert.IsFalse(WkGlobalId.TryResolve<GameObject>(null, out _));
        }

        [Test]
        public void TryRoundTripToPrefs_NullObjectDeletesKey() {
            EditorPrefs.SetString(TestKey, "seed-value");
            Assert.IsTrue(WkGlobalId.TryRoundTripToPrefs(TestKey, null));
            Assert.IsFalse(EditorPrefs.HasKey(TestKey));
        }

        [Test]
        public void RecallFromPrefs_MissingKeyReturnsNull() {
            EditorPrefs.DeleteKey(TestKey);
            Assert.IsNull(WkGlobalId.RecallFromPrefs<GameObject>(TestKey));
        }

        [Test]
        public void RecallFromPrefs_InvalidStoredValueReturnsNull() {
            EditorPrefs.SetString(TestKey, "garbage");
            Assert.IsNull(WkGlobalId.RecallFromPrefs<GameObject>(TestKey));
        }
    }
}
