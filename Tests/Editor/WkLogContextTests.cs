// WkLogContextTests.cs

using NUnit.Framework;
using UnityEngine;
using WhyKnot.Core.Logging;

namespace WhyKnot.Core.Tests {

    public sealed class WkLogContextTests {

        [Test]
        public void NoScopeActive_FormatsToEmptyPrefix() {
            Assert.AreEqual(0, WkLogContext.CurrentScopes.Count);
            Assert.AreEqual(string.Empty, FormatPrefix());
        }

        [Test]
        public void Scope_PushesAndPopsCleanly() {
            using (WkLogContext.Scope("BoneMerger")) {
                Assert.AreEqual(1, WkLogContext.CurrentScopes.Count);
                Assert.AreEqual("BoneMerger", WkLogContext.CurrentScopes[0]);
                Assert.AreEqual("[BoneMerger] ", FormatPrefix());
            }
            Assert.AreEqual(0, WkLogContext.CurrentScopes.Count);
        }

        [Test]
        public void NestedScopes_Compose() {
            using (WkLogContext.Scope("OuterPass"))
            using (WkLogContext.Scope("Stage")) {
                Assert.AreEqual(2, WkLogContext.CurrentScopes.Count);
                Assert.AreEqual("[OuterPass > Stage] ", FormatPrefix());
            }
            Assert.AreEqual(0, WkLogContext.CurrentScopes.Count);
        }

        [Test]
        public void Scope_NullOrEmpty_IsNoop() {
            using (WkLogContext.Scope(null))
            using (WkLogContext.Scope("")) {
                Assert.AreEqual(0, WkLogContext.CurrentScopes.Count);
            }
        }

        [Test]
        public void WithContextObject_AttachesAndDetaches() {
            var go = new GameObject("CtxObject");
            try {
                Assert.IsNull(WkLogContext.CurrentContextObject);
                using (WkLogContext.WithContextObject(go)) {
                    Assert.AreSame(go, WkLogContext.CurrentContextObject);
                }
                Assert.IsNull(WkLogContext.CurrentContextObject);
            } finally {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void WithContextObject_NullIsNoop() {
            using (WkLogContext.WithContextObject(null)) {
                Assert.IsNull(WkLogContext.CurrentContextObject);
            }
        }

        // FormatScopePrefix is internal but we can hit it via reflection.
        private static string FormatPrefix() {
            var m = typeof(WkLogContext).GetMethod("FormatScopePrefix",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (string) m.Invoke(null, null);
        }
    }
}
