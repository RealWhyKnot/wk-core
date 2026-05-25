// AnimatorControllerUtilityTests.cs

using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;
using WhyKnot.Core.Pipeline;

namespace WhyKnot.Core.Tests {

    public sealed class AnimatorControllerUtilityTests {

        private AnimatorController _controller;

        [SetUp]
        public void SetUp() {
            _controller = new AnimatorController { name = "test" };
            _controller.AddLayer("Base");
            _controller.AddLayer("Outfit");
            _controller.AddLayer("Gestures");
        }

        [TearDown]
        public void TearDown() {
            if (_controller != null) Object.DestroyImmediate(_controller);
            _controller = null;
        }

        [Test]
        public void FindLayer_Hit() {
            var layer = AnimatorControllerUtility.FindLayer(_controller, "Outfit");
            Assert.IsNotNull(layer);
            Assert.AreEqual("Outfit", layer.name);
        }

        [Test]
        public void FindLayer_MissReturnsNull() {
            Assert.IsNull(AnimatorControllerUtility.FindLayer(_controller, "NotThere"));
        }

        [Test]
        public void IndexOfLayer() {
            Assert.AreEqual(0, AnimatorControllerUtility.IndexOfLayer(_controller, "Base"));
            Assert.AreEqual(1, AnimatorControllerUtility.IndexOfLayer(_controller, "Outfit"));
            Assert.AreEqual(2, AnimatorControllerUtility.IndexOfLayer(_controller, "Gestures"));
            Assert.AreEqual(-1, AnimatorControllerUtility.IndexOfLayer(_controller, "NotThere"));
        }

        [Test]
        public void RemoveLayer_DropsTheLayer() {
            AnimatorControllerUtility.RemoveLayer(_controller, "Outfit");
            Assert.AreEqual(-1, AnimatorControllerUtility.IndexOfLayer(_controller, "Outfit"));
            Assert.AreEqual(0,  AnimatorControllerUtility.IndexOfLayer(_controller, "Base"));
            Assert.AreEqual(1,  AnimatorControllerUtility.IndexOfLayer(_controller, "Gestures"));
        }

        [Test]
        public void NullSafety() {
            Assert.IsNull(AnimatorControllerUtility.FindLayer(null, "x"));
            Assert.IsNull(AnimatorControllerUtility.FindLayer(_controller, null));
            Assert.AreEqual(-1, AnimatorControllerUtility.IndexOfLayer(null, "x"));
            Assert.IsNull(AnimatorControllerUtility.FindState(null, "x"));
        }
    }
}
