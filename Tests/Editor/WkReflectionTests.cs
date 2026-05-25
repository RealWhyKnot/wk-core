// WkReflectionTests.cs

using System.Reflection;
using NUnit.Framework;
using WhyKnot.Core.Reflection;

namespace WhyKnot.Core.Tests {

    public sealed class WkReflectionTests {

        private class Base {
            public int basePublic = 7;
            private int basePrivate = 11;
            public int GetBasePrivate() => basePrivate;
        }

        private class Derived : Base {
            public string derivedField = "hello";
            public int[] elems = new[] { 1, 2, 3 };
            public Nested nested = new Nested();
        }

        private class Nested {
            public float depth = 3.14f;
        }

        [Test]
        public void FindField_FindsOnDeclaringType() {
            var f = WkReflection.FindField(typeof(Derived), "derivedField");
            Assert.IsNotNull(f);
            Assert.AreEqual(typeof(string), f.FieldType);
        }

        [Test]
        public void FindField_WalksInheritance_FindsBasePublic() {
            var f = WkReflection.FindField(typeof(Derived), "basePublic");
            Assert.IsNotNull(f);
            Assert.AreEqual(typeof(int), f.FieldType);
        }

        [Test]
        public void FindField_FindsPrivateOnBase() {
            var f = WkReflection.FindField(typeof(Derived), "basePrivate");
            Assert.IsNotNull(f, "Private base field reachable via DeclaredOnly walk");
        }

        [Test]
        public void FindField_Missing_ReturnsNull() {
            Assert.IsNull(WkReflection.FindField(typeof(Derived), "neverNamedThis"));
            Assert.IsNull(WkReflection.FindField(null, "x"));
            Assert.IsNull(WkReflection.FindField(typeof(Derived), null));
        }

        [Test]
        public void GetFieldValue_ReturnsTypedValue() {
            var d = new Derived();
            Assert.AreEqual("hello", WkReflection.GetFieldValue(d, "derivedField", "fallback"));
        }

        [Test]
        public void GetFieldValue_MissingField_ReturnsFallback() {
            var d = new Derived();
            Assert.AreEqual(42, WkReflection.GetFieldValue(d, "nope", 42));
        }

        [Test]
        public void GetFieldValue_TypeMismatch_ReturnsFallback() {
            var d = new Derived();
            Assert.AreEqual(-1, WkReflection.GetFieldValue(d, "derivedField", -1));
        }

        [Test]
        public void GetFieldValue_NullTarget_ReturnsFallback() {
            Assert.AreEqual("x", WkReflection.GetFieldValue((object) null, "any", "x"));
        }

        [Test]
        public void WalkPath_SimpleField() {
            var d = new Derived();
            Assert.AreEqual("hello", WkReflection.WalkPath(d, "derivedField"));
        }

        [Test]
        public void WalkPath_NestedField() {
            var d = new Derived();
            Assert.AreEqual(3.14f, WkReflection.WalkPath(d, "nested.depth"));
        }

        [Test]
        public void WalkPath_ArrayDataSegment() {
            var d = new Derived();
            Assert.AreEqual(2, WkReflection.WalkPath(d, "elems.Array.data[1]"));
        }

        [Test]
        public void WalkPath_BareIndex() {
            var d = new Derived();
            Assert.AreEqual(3, WkReflection.WalkPath(d, "elems[2]"));
        }

        [Test]
        public void WalkPath_MissingSegment_ReturnsNull() {
            var d = new Derived();
            Assert.IsNull(WkReflection.WalkPath(d, "nope"));
            Assert.IsNull(WkReflection.WalkPath(d, "elems[99]"));
        }

        [Test]
        public void TryFindAssembly_FindsUnityEngine() {
            Assert.IsTrue(WkReflection.TryFindAssembly("UnityEngine", out var asm));
            Assert.IsNotNull(asm);
        }

        [Test]
        public void TryFindAssembly_MissingReturnsFalse() {
            Assert.IsFalse(WkReflection.TryFindAssembly("definitely.not.an.assembly", out var asm));
            Assert.IsNull(asm);
        }
    }
}
