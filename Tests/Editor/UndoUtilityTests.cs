// UndoUtilityTests.cs

using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WhyKnot.Core.Utilities;

namespace WhyKnot.Core.Tests {

    public sealed class UndoUtilityTests {

        private GameObject _host;

        [TearDown]
        public void TearDown() {
            if (_host != null) Object.DestroyImmediate(_host);
            _host = null;
        }

        [Test]
        public void Group_OpensAndCloses_NoThrow() {
            Assert.DoesNotThrow(() => {
                using (UndoUtility.Group("Test action")) {
                    _host = new GameObject("Tmp");
                    Undo.RegisterCreatedObjectUndo(_host, "Test action");
                }
            });
        }

        [Test]
        public void Group_NullLabel_FallsBackToDefault() {
            Assert.DoesNotThrow(() => {
                using (UndoUtility.Group(null)) {
                    _host = new GameObject("Tmp");
                }
            });
        }

        [Test]
        public void RecordPropertyChange_NullTarget_IsNoop() {
            Assert.DoesNotThrow(() => UndoUtility.RecordPropertyChange(null, "x"));
        }

        [Test]
        public void RecordAdd_NullHost_ReturnsNull() {
            Assert.IsNull(UndoUtility.RecordAdd<BoxCollider>(null, "Add"));
        }

        [Test]
        public void RecordAdd_AddsComponentToHost() {
            _host = new GameObject("Tmp");
            var added = UndoUtility.RecordAdd<BoxCollider>(_host, "Add Box");
            Assert.IsNotNull(added);
            Assert.AreSame(_host, added.gameObject);
        }
    }
}
