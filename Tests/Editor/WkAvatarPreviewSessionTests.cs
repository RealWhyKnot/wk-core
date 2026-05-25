// WkAvatarPreviewSessionTests.cs

using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WhyKnot.Core.Pipeline;

namespace WhyKnot.Core.Tests {

    public sealed class WkAvatarPreviewSessionTests {

        private const string OwnerPackageId = "dev.whyknot.tests-preview";
        private GameObject _source;

        [SetUp]
        public void SetUp() {
            _source = new GameObject("PreviewSource");
            var child = new GameObject("ChildA");
            child.transform.SetParent(_source.transform);
            var grand = new GameObject("Grand");
            grand.transform.SetParent(child.transform);
            grand.AddComponent<BoxCollider>();
        }

        [TearDown]
        public void TearDown() {
            if (_source != null) Object.DestroyImmediate(_source);
            _source = null;
            EditorPrefs.DeleteKey("WhyKnot.Core.PreviewSession.SourceGlobalId." + OwnerPackageId);
            EditorPrefs.DeleteKey("WhyKnot.Core.PreviewSession.SourceWasHidden." + OwnerPackageId);
        }

        [Test]
        public void Construct_BuildsProxyWithDontSave() {
            using (var session = new WkAvatarPreviewSession(_source, OwnerPackageId)) {
                Assert.IsNotNull(session.Proxy);
                Assert.IsTrue((session.Proxy.hideFlags & HideFlags.DontSave) != 0,
                    "Proxy must carry HideFlags.DontSave so it does not save to the scene.");
                Assert.IsTrue((session.Proxy.hideFlags & HideFlags.HideInHierarchy) != 0,
                    "Proxy should also be hidden from the Hierarchy panel.");
                Assert.AreNotSame(_source, session.Proxy);
            }
        }

        [Test]
        public void Construct_HidesSourceByDefault() {
            _source.SetActive(true);
            using (var session = new WkAvatarPreviewSession(_source, OwnerPackageId)) {
                Assert.IsFalse(_source.activeSelf, "Source should be hidden during preview.");
            }
            Assert.IsTrue(_source.activeSelf, "Source should be restored to its prior active state on Dispose.");
        }

        [Test]
        public void Construct_DoesNotHideSourceWhenAsked() {
            _source.SetActive(true);
            using (var session = new WkAvatarPreviewSession(_source, OwnerPackageId, hideSource: false)) {
                Assert.IsTrue(_source.activeSelf, "hideSource=false should leave source visible.");
            }
        }

        [Test]
        public void GetProxyFor_MapsRootAndChildren() {
            using (var session = new WkAvatarPreviewSession(_source, OwnerPackageId)) {
                var proxyRoot = session.GetProxyFor(_source);
                Assert.IsNotNull(proxyRoot);
                Assert.AreSame(session.Proxy, proxyRoot);

                var sourceChild = _source.transform.GetChild(0).gameObject;
                var proxyChild = session.GetProxyFor(sourceChild);
                Assert.IsNotNull(proxyChild);
                Assert.AreEqual("ChildA", proxyChild.name);
            }
        }

        [Test]
        public void GetOriginalFor_ReversesMapping() {
            using (var session = new WkAvatarPreviewSession(_source, OwnerPackageId)) {
                var proxyChild = session.Proxy.transform.GetChild(0).gameObject;
                var originalChild = session.GetOriginalFor(proxyChild);
                Assert.IsNotNull(originalChild);
                Assert.AreEqual("ChildA", originalChild.name);
                Assert.AreNotSame(proxyChild, originalChild);
            }
        }

        [Test]
        public void GetProxyFor_Component_MapsByTypeAndIndex() {
            using (var session = new WkAvatarPreviewSession(_source, OwnerPackageId)) {
                var sourceBox = _source.GetComponentInChildren<BoxCollider>();
                var proxyBox  = session.GetProxyFor(sourceBox);
                Assert.IsNotNull(proxyBox);
                Assert.AreNotSame(sourceBox, proxyBox);
                Assert.AreEqual(sourceBox.GetType(), proxyBox.GetType());
            }
        }

        [Test]
        public void Dispose_DestroysProxy_AndClearsMapping() {
            WkAvatarPreviewSession session;
            using (session = new WkAvatarPreviewSession(_source, OwnerPackageId)) {
                Assert.IsNotNull(session.Proxy);
            }
            Assert.IsNull(session.Proxy);
            Assert.IsNull(session.GetProxyFor(_source));
        }

        [Test]
        public void ForceReset_RebuildsProxy() {
            using (var session = new WkAvatarPreviewSession(_source, OwnerPackageId)) {
                var firstProxy = session.Proxy;
                session.ForceReset();
                Assert.IsNotNull(session.Proxy);
                Assert.AreNotSame(firstProxy, session.Proxy);
            }
        }

        [Test]
        public void NullSourceThrows() {
            Assert.Throws<System.ArgumentNullException>(() => new WkAvatarPreviewSession(null, OwnerPackageId));
        }

        [Test]
        public void NullOwnerPackageIdThrows() {
            Assert.Throws<System.ArgumentException>(() => new WkAvatarPreviewSession(_source, ""));
        }
    }
}
