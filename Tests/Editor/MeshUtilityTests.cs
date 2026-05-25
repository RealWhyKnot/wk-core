// MeshUtilityTests.cs

using NUnit.Framework;
using UnityEngine;
using WhyKnot.Core.Utilities;

namespace WhyKnot.Core.Tests {

    public sealed class MeshUtilityTests {

        private Mesh _mesh;

        [TearDown]
        public void TearDown() {
            if (_mesh != null) Object.DestroyImmediate(_mesh);
            _mesh = null;
        }

        [Test]
        public void CloneInMemory_PreservesVerticesAndCarriesDontSaveFlag() {
            _mesh = new Mesh();
            _mesh.name = "Source";
            _mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            _mesh.triangles = new[] { 0, 1, 2 };

            var clone = MeshUtility.CloneInMemory(_mesh, "preview");
            Assert.IsNotNull(clone);
            try {
                Assert.AreEqual("Source_preview", clone.name);
                Assert.AreEqual(HideFlags.DontSave, clone.hideFlags);
                Assert.AreEqual(_mesh.vertexCount, clone.vertexCount);
                Assert.AreEqual(_mesh.triangles.Length, clone.triangles.Length);
            } finally {
                Object.DestroyImmediate(clone);
            }
        }

        [Test]
        public void CloneInMemory_NullSourceReturnsNull() {
            Assert.IsNull(MeshUtility.CloneInMemory(null));
        }

        [Test]
        public void CloneInMemory_NoSuffix_UsesSourceName() {
            _mesh = new Mesh { name = "X" };
            var clone = MeshUtility.CloneInMemory(_mesh);
            try { Assert.AreEqual("X", clone.name); } finally { Object.DestroyImmediate(clone); }
        }

        [Test]
        public void BoneLocalToWorld_IdentityBindpose_IdentityBone_ReturnsInput() {
            var go = new GameObject("Bone");
            try {
                var bp = Matrix4x4.identity;
                var v  = new Vector3(0.25f, 0.5f, -1f);
                var result = MeshUtility.BoneLocalToWorld(bp, go.transform, v);
                // With identity bindpose and identity bone, world = input.
                Assert.That(result.x, Is.EqualTo(v.x).Within(1e-5f));
                Assert.That(result.y, Is.EqualTo(v.y).Within(1e-5f));
                Assert.That(result.z, Is.EqualTo(v.z).Within(1e-5f));
            } finally {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void BoneLocalToWorld_NullBone_ReturnsInput() {
            var v = new Vector3(1f, 2f, 3f);
            Assert.AreEqual(v, MeshUtility.BoneLocalToWorld(Matrix4x4.identity, null, v));
        }
    }
}
