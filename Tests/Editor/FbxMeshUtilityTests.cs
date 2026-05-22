// FbxMeshUtilityTests.cs
//
// The FBX-clone path requires an actual imported model asset on disk, so
// we test only the passthrough: a runtime-created Mesh is not an
// importer sub-asset and is returned unchanged, no clone, no asset
// creation.

using NUnit.Framework;
using UnityEngine;
using WhyKnot.Core.Utilities;

namespace WhyKnot.Core.Tests {

    public sealed class FbxMeshUtilityTests {

        private GameObject _root;
        private Mesh _runtimeMesh;

        [SetUp]
        public void SetUp() {
            _root = new GameObject("MeshHost");
            _runtimeMesh = new Mesh { name = "TestMesh" };
            _runtimeMesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            _runtimeMesh.triangles = new[] { 0, 1, 2 };
        }

        [TearDown]
        public void TearDown() {
            if (_root != null) Object.DestroyImmediate(_root);
            if (_runtimeMesh != null) Object.DestroyImmediate(_runtimeMesh);
            _root = null;
            _runtimeMesh = null;
        }

        [Test]
        public void RuntimeMesh_PassesThroughUnchanged() {
            var renderer = _root.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = _runtimeMesh;

            var result = FbxMeshUtility.ResolveEditableMesh(
                renderer, _runtimeMesh, "(Test)", "Test undo", "Assets/WkCore Test Generated");

            Assert.IsFalse(result.WasCloned, "Runtime mesh should not be cloned");
            Assert.IsNull(result.ClonedPath);
            Assert.AreSame(_runtimeMesh, result.Mesh);
            Assert.AreSame(_runtimeMesh, renderer.sharedMesh);
        }

        [Test]
        public void NullMesh_PassesThroughAsNull() {
            var renderer = _root.AddComponent<SkinnedMeshRenderer>();
            var result = FbxMeshUtility.ResolveEditableMesh(
                renderer, null, "(Test)", "Test undo", "Assets/WkCore Test Generated");

            Assert.IsFalse(result.WasCloned);
            Assert.IsNull(result.Mesh);
            Assert.IsNull(result.ClonedPath);
        }

        [Test, Ignore("Requires an imported FBX model asset on disk; covered by manual smoke test.")]
        public void FbxSubAsset_ClonesAndRewiresRenderer() {
            // Intentionally left as a documented gap: exercising this path
            // requires constructing an FBX fixture and importing it, which
            // is impractical in this test suite. Manual verification is
            // tracked in the WeightFixer / BoneMergerWindow smoke tests in
            // their respective downstream repos.
        }
    }
}
