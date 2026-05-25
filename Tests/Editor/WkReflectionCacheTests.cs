// WkReflectionCacheTests.cs

using System.Reflection;
using NUnit.Framework;
using WhyKnot.Core.Reflection;

namespace WhyKnot.Core.Tests {

    public sealed class WkReflectionCacheTests {

        private sealed class UnityEngineProbe : WkReflectionCache {
            public System.Type Vector3Type;
            public FieldInfo ZeroField;
            protected override string TargetAssemblyName => "UnityEngine.CoreModule";

            protected override bool TryResolveMembers(Assembly asm, out string error) {
                Vector3Type = asm.GetType("UnityEngine.Vector3");
                if (Vector3Type == null) { error = "Vector3 missing"; return false; }
                ZeroField = Vector3Type.GetField("zero", BindingFlags.Public | BindingFlags.Static);
                if (ZeroField == null) { error = "Vector3.zero missing"; return false; }
                error = null;
                return true;
            }
        }

        private sealed class MissingAssembly : WkReflectionCache {
            protected override string TargetAssemblyName => "Definitely.Not.An.Assembly";
            protected override bool TryResolveMembers(Assembly asm, out string error) {
                error = null;
                return true;
            }
        }

        private sealed class PartialResolve : WkReflectionCache {
            public System.Type FoundType;
            protected override string TargetAssemblyName => "UnityEngine.CoreModule";
            protected override bool TryResolveMembers(Assembly asm, out string error) {
                FoundType = asm.GetType("UnityEngine.Vector3");
                error = "deliberately fails after partial resolve";
                return false;
            }
        }

        [Test]
        public void TryEnsure_ResolvesAndCachesAfterSuccess() {
            var cache = new UnityEngineProbe();
            Assert.IsTrue(cache.TryEnsure(out var error), error);
            Assert.IsTrue(cache.IsResolved);
            Assert.IsNotNull(cache.TargetAssembly);
            Assert.IsNotNull(cache.Vector3Type);

            // Second call must be a no-op (idempotent).
            Assert.IsTrue(cache.TryEnsure(out _));
        }

        [Test]
        public void TryEnsure_MissingAssembly_ReturnsFalseAndStaysUnresolved() {
            var cache = new MissingAssembly();
            Assert.IsFalse(cache.TryEnsure(out var error));
            Assert.IsNotNull(error);
            Assert.IsFalse(cache.IsResolved);
            Assert.IsNull(cache.TargetAssembly);
        }

        [Test]
        public void TryEnsure_PartialResolve_NullResetsBaseState() {
            var cache = new PartialResolve();
            Assert.IsFalse(cache.TryEnsure(out var error));
            Assert.AreEqual("deliberately fails after partial resolve", error);
            Assert.IsFalse(cache.IsResolved);
            Assert.IsNull(cache.TargetAssembly, "Partial resolve should reset TargetAssembly");
        }
    }
}
