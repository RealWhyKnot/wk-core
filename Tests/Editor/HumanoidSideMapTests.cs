// HumanoidSideMapTests.cs
//
// HumanoidSideMap's interesting path requires a bound humanoid Avatar
// asset, which is impractical to construct in a test runner without
// shipping a fixture FBX. We cover the degenerate paths only.

using NUnit.Framework;
using UnityEngine;
using WhyKnot.Core.Utilities;

namespace WhyKnot.Core.Tests {

    public sealed class HumanoidSideMapTests {

        private GameObject _root;

        [TearDown]
        public void TearDown() {
            if (_root != null) Object.DestroyImmediate(_root);
            _root = null;
        }

        [Test]
        public void NullAnimator_IsInvalid() {
            var map = new HumanoidSideMap(null);
            Assert.IsFalse(map.IsValid);
            Assert.IsNull(map.Hips);
            Assert.AreEqual(BoneSide.Unknown, map.GetSide(null));
        }

        [Test]
        public void NonHumanoidAnimator_IsInvalid() {
            _root = new GameObject("Generic");
            var animator = _root.AddComponent<Animator>();
            // Default Animator has no Avatar bound -> isHuman==false.
            var map = new HumanoidSideMap(animator);
            Assert.IsFalse(map.IsValid);
        }

        [Test]
        public void GetSide_OnArbitraryTransform_ReturnsUnknown() {
            _root = new GameObject("Generic");
            var animator = _root.AddComponent<Animator>();
            var bone = new GameObject("LooseBone");
            bone.transform.SetParent(_root.transform);

            var map = new HumanoidSideMap(animator);
            Assert.AreEqual(BoneSide.Unknown, map.GetSide(bone.transform));
        }

        [Test]
        public void ClassifyWorldPosition_WithoutHips_ReturnsUnknown() {
            var map = new HumanoidSideMap(null);
            Assert.AreEqual(BoneSide.Unknown, map.ClassifyWorldPosition(Vector3.zero, 0.05f));
        }

        [Test]
        public void LeftSign_DefaultsToPositiveOne_WhenInvalid() {
            var map = new HumanoidSideMap(null);
            Assert.AreEqual(1f, map.LeftSignInHipsLocal);
        }
    }
}
