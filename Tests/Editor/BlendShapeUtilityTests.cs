// BlendShapeUtilityTests.cs

using NUnit.Framework;
using UnityEngine;
using WhyKnot.Core.Utilities;

namespace WhyKnot.Core.Tests {

    public sealed class BlendShapeUtilityTests {

        private Mesh _mesh;

        [SetUp]
        public void SetUp() {
            _mesh = new Mesh();
            _mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up };
            _mesh.triangles = new[] { 0, 1, 2 };
        }

        [TearDown]
        public void TearDown() {
            if (_mesh != null) Object.DestroyImmediate(_mesh);
            _mesh = null;
        }

        [Test]
        public void AddOrReplace_NewShape_Adds() {
            var deltas = new[] { Vector3.right, Vector3.zero, Vector3.zero };
            Assert.IsTrue(BlendShapeUtility.AddOrReplace(_mesh, "Wide", deltas));
            Assert.AreEqual(1, _mesh.blendShapeCount);
            Assert.AreEqual("Wide", _mesh.GetBlendShapeName(0));
        }

        [Test]
        public void AddOrReplace_ReplacingExisting_KeepsOtherShapesByName() {
            _mesh.AddBlendShapeFrame("Smile", 100f, new[] { Vector3.up, Vector3.up, Vector3.up }, null, null);
            _mesh.AddBlendShapeFrame("Blink", 100f, new[] { Vector3.right, Vector3.right, Vector3.right }, null, null);

            var deltas = new[] { Vector3.zero, Vector3.zero, Vector3.zero };
            Assert.IsTrue(BlendShapeUtility.AddOrReplace(_mesh, "Smile", deltas));

            Assert.AreEqual(2, _mesh.blendShapeCount);
            Assert.AreEqual("Blink", _mesh.GetBlendShapeName(0), "Other shape preserved");
            Assert.AreEqual("Smile", _mesh.GetBlendShapeName(1), "Replaced shape re-added at the end");
        }

        [Test]
        public void AddOrReplace_MismatchedVertexCount_ReturnsFalse() {
            Assert.IsFalse(BlendShapeUtility.AddOrReplace(_mesh, "Bad", new[] { Vector3.zero, Vector3.zero }));
        }

        [Test]
        public void AddOrReplace_NullInputs_ReturnsFalse() {
            Assert.IsFalse(BlendShapeUtility.AddOrReplace(null, "X", new[] { Vector3.zero, Vector3.zero, Vector3.zero }));
            Assert.IsFalse(BlendShapeUtility.AddOrReplace(_mesh, null, new[] { Vector3.zero, Vector3.zero, Vector3.zero }));
            Assert.IsFalse(BlendShapeUtility.AddOrReplace(_mesh, "X", null));
        }

        [Test]
        public void FindIndex_ReturnsNegativeOneOnMissing() {
            Assert.AreEqual(-1, BlendShapeUtility.FindIndex(_mesh, "NotThere"));
        }

        [Test]
        public void FindIndex_ReturnsCorrectIndexOnHit() {
            _mesh.AddBlendShapeFrame("Smile", 100f, new[] { Vector3.up, Vector3.up, Vector3.up }, null, null);
            Assert.AreEqual(0, BlendShapeUtility.FindIndex(_mesh, "Smile"));
        }

        [Test]
        public void FindIndex_NullSafety() {
            Assert.AreEqual(-1, BlendShapeUtility.FindIndex(null, "X"));
            Assert.AreEqual(-1, BlendShapeUtility.FindIndex(_mesh, null));
        }
    }
}
