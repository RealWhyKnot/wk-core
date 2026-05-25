// WkGeneratedAssetScopeTests.cs

using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WhyKnot.Core.Pipeline;

namespace WhyKnot.Core.Tests {

    public sealed class WkGeneratedAssetScopeTests {

        private const string TestPackageId = "dev.whyknot.tests-asset-scope";
        private string _testKey;

        [SetUp]
        public void SetUp() {
            _testKey = "scope_" + System.Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        [TearDown]
        public void TearDown() {
            // Sweep all three tier folders for the test container key.
            foreach (var tier in System.Enum.GetValues(typeof(WkGeneratedAssetTier))) {
                var folder = ExpectedFolder((WkGeneratedAssetTier) tier, TestPackageId, _testKey);
                if (AssetDatabase.IsValidFolder(folder)) AssetDatabase.DeleteAsset(folder);
            }
        }

        [Test]
        public void Temporary_FolderUnderPackagesUnderscoreGenerated() {
            using (var scope = new WkGeneratedAssetScope(WkGeneratedAssetTier.Temporary, TestPackageId, _testKey)) {
                Assert.AreEqual($"Packages/{TestPackageId}/__Generated/{_testKey}", scope.AssetFolder);
            }
        }

        [Test]
        public void Session_FolderUnderAssetsWhyKnotSession() {
            using (var scope = new WkGeneratedAssetScope(WkGeneratedAssetTier.Session, TestPackageId, _testKey)) {
                Assert.AreEqual($"Assets/WhyKnot/__Session/{TestPackageId}/{_testKey}", scope.AssetFolder);
            }
        }

        [Test]
        public void Persistent_FolderUnderAssetsWhyKnotGenerated() {
            using (var scope = new WkGeneratedAssetScope(WkGeneratedAssetTier.Persistent, TestPackageId, _testKey)) {
                Assert.AreEqual($"Assets/WhyKnot/Generated/{TestPackageId}/{_testKey}", scope.AssetFolder);
            }
        }

        [Test]
        public void Session_DisposeWipesContents() {
            var folder = "";
            using (var scope = new WkGeneratedAssetScope(WkGeneratedAssetTier.Session, TestPackageId, _testKey)) {
                folder = scope.AssetFolder;
                var asset = ScriptableObject.CreateInstance<TestScriptable>();
                scope.SaveAsset(asset, "probe");
                Assert.IsTrue(AssetDatabase.IsValidFolder(folder));
            }
            Assert.IsFalse(AssetDatabase.IsValidFolder(folder),
                "Session scope Dispose should remove its folder + contents.");
        }

        [Test]
        public void Persistent_DisposeKeepsContents() {
            var folder = "";
            using (var scope = new WkGeneratedAssetScope(WkGeneratedAssetTier.Persistent, TestPackageId, _testKey)) {
                folder = scope.AssetFolder;
                var asset = ScriptableObject.CreateInstance<TestScriptable>();
                scope.SaveAsset(asset, "probe");
            }
            Assert.IsTrue(AssetDatabase.IsValidFolder(folder),
                "Persistent scope Dispose should leave its folder in place.");
        }

        [Test]
        public void NullPackageId_Throws() {
            Assert.Throws<System.ArgumentException>(() =>
                new WkGeneratedAssetScope(WkGeneratedAssetTier.Persistent, null, _testKey));
        }

        [Test]
        public void NullContainerKey_Throws() {
            Assert.Throws<System.ArgumentException>(() =>
                new WkGeneratedAssetScope(WkGeneratedAssetTier.Persistent, TestPackageId, null));
        }

        private static string ExpectedFolder(WkGeneratedAssetTier tier, string packageId, string key) {
            switch (tier) {
                case WkGeneratedAssetTier.Temporary:  return $"Packages/{packageId}/__Generated/{key}";
                case WkGeneratedAssetTier.Session:    return $"Assets/WhyKnot/__Session/{packageId}/{key}";
                case WkGeneratedAssetTier.Persistent: return $"Assets/WhyKnot/Generated/{packageId}/{key}";
                default: return "";
            }
        }

        private sealed class TestScriptable : ScriptableObject { }
    }
}
