// FolderUtilityTests.cs
//
// Each test runs against a unique Assets/WhyKnotTests/<guid>/ root and
// removes it on TearDown so parallel runs and a previous-failure don't
// leave fixtures behind.

using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WhyKnot.Core.Utilities;

namespace WhyKnot.Core.Tests {

    public sealed class FolderUtilityTests {

        private string _testRoot;

        [SetUp]
        public void SetUp() {
            _testRoot = "Assets/WhyKnotTests_" + System.Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        [TearDown]
        public void TearDown() {
            if (!string.IsNullOrEmpty(_testRoot) && AssetDatabase.IsValidFolder(_testRoot)) {
                AssetDatabase.DeleteAsset(_testRoot);
            }
            _testRoot = null;
        }

        [Test]
        public void EnsureFolder_CreatesMissingSegments() {
            var path = _testRoot + "/A/B/C";
            FolderUtility.EnsureFolder(path);
            Assert.IsTrue(AssetDatabase.IsValidFolder(path), "Deepest folder should exist");
            Assert.IsTrue(AssetDatabase.IsValidFolder(_testRoot + "/A/B"));
            Assert.IsTrue(AssetDatabase.IsValidFolder(_testRoot + "/A"));
        }

        [Test]
        public void EnsureFolder_OnExistingPath_IsNoop() {
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(_testRoot));
            Assert.IsTrue(AssetDatabase.IsValidFolder(_testRoot));
            Assert.DoesNotThrow(() => FolderUtility.EnsureFolder(_testRoot));
        }

        [Test]
        public void EnsureFolder_NormalisesBackslashes() {
            var path = _testRoot + "\\A";
            FolderUtility.EnsureFolder(path);
            Assert.IsTrue(AssetDatabase.IsValidFolder(_testRoot + "/A"));
        }

        [Test]
        public void EnsureFolder_NullOrEmpty_ReturnsArgUnchanged() {
            Assert.IsNull(FolderUtility.EnsureFolder(null));
            Assert.AreEqual("", FolderUtility.EnsureFolder(""));
        }

        [Test]
        public void EnsureFolder_NonAssetsPath_ReturnsUnchangedNoSideEffect() {
            var result = FolderUtility.EnsureFolder("OutsideAssets/Whatever");
            Assert.AreEqual("OutsideAssets/Whatever", result);
            Assert.IsFalse(AssetDatabase.IsValidFolder("OutsideAssets/Whatever"));
        }
    }
}
