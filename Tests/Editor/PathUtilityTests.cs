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
    }
}
