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

        [Test]
        public void TopLevel_WalkAllTheWayUp() {
            _root = new GameObject("R");
            var a = new GameObject("A");
            a.transform.SetParent(_root.transform);
            var b = new GameObject("B");
            b.transform.SetParent(a.transform);

            Assert.AreSame(_root.transform, AvatarUtility.TopLevel(b.transform));
            Assert.AreSame(_root.transform, AvatarUtility.TopLevel(a.transform));
            Assert.AreSame(_root.transform, AvatarUtility.TopLevel(_root.transform));
        }

        [Test]
        public void TopLevel_NullReturnsNull() {
            Assert.IsNull(AvatarUtility.TopLevel(null));
        }

        [Test]
        public void GetSkinnedMeshes_FindsActiveAndInactive() {
            _root = new GameObject("Avatar");
            var a = new GameObject("MeshA");
            a.transform.SetParent(_root.transform);
            a.AddComponent<SkinnedMeshRenderer>();
            var b = new GameObject("MeshB_Inactive");
            b.transform.SetParent(_root.transform);
            b.AddComponent<SkinnedMeshRenderer>();
            b.SetActive(false);

            var all = new System.Collections.Generic.List<SkinnedMeshRenderer>(
                AvatarUtility.GetSkinnedMeshes(_root, includeInactive: true));
            Assert.AreEqual(2, all.Count);

            var activeOnly = new System.Collections.Generic.List<SkinnedMeshRenderer>(
                AvatarUtility.GetSkinnedMeshes(_root, includeInactive: false));
            Assert.AreEqual(1, activeOnly.Count);
        }

        [Test]
        public void GetSkinnedMeshes_NullRoot_ReturnsEmpty() {
            var list = new System.Collections.Generic.List<SkinnedMeshRenderer>(
                AvatarUtility.GetSkinnedMeshes(null));
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void GetAvatarAnimator_ReturnsFirstAnimatorInChildren() {
            _root = new GameObject("Avatar");
            var animatorHost = new GameObject("AnimRoot");
            animatorHost.transform.SetParent(_root.transform);
            var animator = animatorHost.AddComponent<Animator>();

            Assert.AreSame(animator, AvatarUtility.GetAvatarAnimator(_root));
        }

        [Test]
        public void IsAvatarRoot_AnimatorRootWithNoParent_True() {
            _root = new GameObject("Avatar");
            _root.AddComponent<Animator>();
            Assert.IsTrue(AvatarUtility.IsAvatarRoot(_root));
        }

        [Test]
        public void IsAvatarRoot_PlainNonRoot_False() {
            _root = new GameObject("Container");
            var child = new GameObject("Child");
            child.transform.SetParent(_root.transform);
            child.AddComponent<Animator>();
            // Has Animator but is not a root.
            Assert.IsFalse(AvatarUtility.IsAvatarRoot(child));
        }

        [Test]
        public void IsAvatarRoot_NullReturnsFalse() {
            Assert.IsFalse(AvatarUtility.IsAvatarRoot(null));
        }
    }
}
