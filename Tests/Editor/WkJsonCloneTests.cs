// WkJsonCloneTests.cs

using System;
using NUnit.Framework;
using UnityEngine;
using WhyKnot.Core.Reflection;

namespace WhyKnot.Core.Tests {

    public sealed class WkJsonCloneTests {

        [Serializable]
        private sealed class Sample {
            public int    age = 7;
            public string name = "x";
            public Vector3 position = new Vector3(1, 2, 3);
        }

        [Test]
        public void Clone_RoundTripsSerializedFields() {
            var src = new Sample { age = 42, name = "Avatar", position = new Vector3(0.5f, 1f, -1f) };
            var copy = WkJsonClone.Clone(src);
            Assert.IsNotNull(copy);
            Assert.AreNotSame(src, copy);
            Assert.AreEqual(src.age, copy.age);
            Assert.AreEqual(src.name, copy.name);
            Assert.AreEqual(src.position, copy.position);
        }

        [Test]
        public void Clone_NullSourceReturnsNull() {
            Assert.IsNull(WkJsonClone.Clone((object) null));
            Assert.IsNull(WkJsonClone.Clone<Sample>(null));
        }

        [Test]
        public void Clone_RuntimeTypeIsPreserved() {
            var src = new Sample();
            var copy = WkJsonClone.Clone(src);
            Assert.AreEqual(typeof(Sample), copy.GetType());
        }
    }
}
