// EditorLoggerTests.cs
//
// LogAssert verifies that the prefix is prepended to each emitted line.

using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WhyKnot.Core.Utilities;

namespace WhyKnot.Core.Tests {

    public sealed class EditorLoggerTests {

        [Test]
        public void Log_PrependsPrefix() {
            var logger = new EditorLogger("[TestPrefix]");
            LogAssert.Expect(LogType.Log, new Regex(@"^\[TestPrefix\] hello world$"));
            logger.Log("hello world");
        }

        [Test]
        public void LogWarning_PrependsPrefix() {
            var logger = new EditorLogger("[TestPrefix]");
            LogAssert.Expect(LogType.Warning, new Regex(@"^\[TestPrefix\] a warning$"));
            logger.LogWarning("a warning");
        }

        [Test]
        public void LogError_PrependsPrefix() {
            var logger = new EditorLogger("[TestPrefix]");
            LogAssert.Expect(LogType.Error, new Regex(@"^\[TestPrefix\] an error$"));
            logger.LogError("an error");
        }

        [Test]
        public void EmptyPrefix_StillLogs() {
            var logger = new EditorLogger("");
            LogAssert.Expect(LogType.Log, new Regex(@"^bare message$"));
            logger.Log("bare message");
        }

        [Test]
        public void NullException_DoesNothing() {
            var logger = new EditorLogger("[TestPrefix]");
            // No LogAssert.Expect -- no log expected.
            Assert.DoesNotThrow(() => logger.LogException(null));
        }
    }
}
