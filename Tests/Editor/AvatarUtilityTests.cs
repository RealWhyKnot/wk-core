// AvatarUtilityTests.cs

using NUnit.Framework;
using UnityEngine;
using WhyKnot.Core.Utilities;

namespace WhyKnot.Core.Tests {

    public sealed class AvatarUtilityTests {

        private GameObject _root;

        [TearDown]
        public void TearDown() {
            if (_root != null) Object.DestroyImmediate(_root);
            _root = null;
        }

        [Test]
        public void NullComponent_ReturnsNull() {
            Assert.IsNull(AvatarUtility.FindAvatarRoot(null));
        }

        [Test]
        public void AnimatorPresent_ReturnsAnimatorsGameObject() {
            _root = new GameObject("AvatarRoot");
            var animator = _root.AddComponent<Animator>();
            var child = new GameObject("Child");
            child.transform.SetParent(_root.transform);
            var leaf = new GameObject("Leaf");
            leaf.transform.SetParent(child.transform);
            var marker = leaf.AddComponent<BoxCollider>();

            Assert.AreSame(animator.gameObject, AvatarUtility.FindAvatarRoot(marker));
        }

        [Test]
        public void NoAnimator_ReturnsSceneRoot() {
            _root = new GameObject("PlainRoot");
            var child = new GameObject("Child");
            child.transform.SetParent(_root.transform);
            var marker = child.AddComponent<BoxCollider>();

            Assert.AreSame(_root, AvatarUtility.FindAvatarRoot(marker));
        }

        [Test]
        public void ComponentItselfIsAvatarRoot_ReturnsItsGameObject() {
            _root = new GameObject("AvatarRoot");
            var animator = _root.AddComponent<Animator>();

            Assert.AreSame(_root, AvatarUtility.FindAvatarRoot(animator));
        }
    }
}
