// PathUtilityTests.cs

using NUnit.Framework;
using UnityEngine;
using WhyKnot.Core.Utilities;

namespace WhyKnot.Core.Tests {

    public sealed class PathUtilityTests {

        private GameObject _root;

        [TearDown]
        public void TearDown() {
            if (_root != null) Object.DestroyImmediate(_root);
            _root = null;
        }

        [Test]
        public void NullInput_ReturnsNullSentinel() {
            Assert.AreEqual("(null)", PathUtility.GetGameObjectPath(null));
        }

        [Test]
        public void SingleObject_ReturnsName() {
            _root = new GameObject("Solo");
            Assert.AreEqual("Solo", PathUtility.GetGameObjectPath(_root));
        }

        [Test]
        public void NestedHierarchy_ReturnsSlashJoinedPath() {
            _root = new GameObject("Root");
            var mid = new GameObject("Parent");
            mid.transform.SetParent(_root.transform);
            var leaf = new GameObject("Child");
            leaf.transform.SetParent(mid.transform);

            Assert.AreEqual("Root/Parent/Child", PathUtility.GetGameObjectPath(leaf));
        }

        [Test]
        public void HierarchyWithSpacesInNames_PreservesSpaces() {
            _root = new GameObject("My Avatar");
            var child = new GameObject("Left Hand");
            child.transform.SetParent(_root.transform);

            Assert.AreEqual("My Avatar/Left Hand", PathUtility.GetGameObjectPath(child));
        }

        [Test]
        public void GetRelativePath_DirectChild() {
            _root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(_root.transform);

            Assert.AreEqual("Child", PathUtility.GetRelativePath(_root.transform, child.transform));
        }

        [Test]
        public void GetRelativePath_DeepDescendant() {
            _root = new GameObject("Root");
            var a = new GameObject("A");
            a.transform.SetParent(_root.transform);
            var b = new GameObject("B");
            b.transform.SetParent(a.transform);
            var c = new GameObject("C");
            c.transform.SetParent(b.transform);

            Assert.AreEqual("A/B/C", PathUtility.GetRelativePath(_root.transform, c.transform));
        }

        [Test]
        public void GetRelativePath_NotADescendant_ReturnsNull() {
            _root = new GameObject("Root");
            var sibling = new GameObject("Outside");

            try {
                Assert.IsNull(PathUtility.GetRelativePath(_root.transform, sibling.transform));
            } finally {
                Object.DestroyImmediate(sibling);
            }
        }

        [Test]
        public void GetRelativePath_SameTransform_ReturnsEmpty() {
            _root = new GameObject("Root");
            Assert.AreEqual(string.Empty, PathUtility.GetRelativePath(_root.transform, _root.transform));
        }

        [Test]
        public void GetRelativePath_NullArgs_ReturnsNull() {
            _root = new GameObject("Root");
            Assert.IsNull(PathUtility.GetRelativePath((Transform) null, _root.transform));
            Assert.IsNull(PathUtility.GetRelativePath(_root.transform, (Transform) null));
        }

        [Test]
        public void GetRelativePath_GameObjectOverload_WorksToo() {
            _root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(_root.transform);

            Assert.AreEqual("Child", PathUtility.GetRelativePath(_root, child));
        }
    }
}
