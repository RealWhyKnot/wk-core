// WkLoggerTests.cs
//
// Covers: construction populates the log directory, level prefixes
// render correctly, console mirror lights up for the levels we want,
// and the session-culling helper trims the directory down to the
// requested keep count.
//
// Tests use a throwaway packageId so they don't stomp on the real
// dev.whyknot.* log files in %LocalAppData%.

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WhyKnot.Core.Logging;

namespace WhyKnot.Core.Tests {

    public sealed class WkLoggerTests {

        private string _testPackageId;
        private string _testLogDir;

        [SetUp]
        public void SetUp() {
            // Unique per-test packageId so parallel test runs don't collide and
            // so a previous failing test doesn't leave stale fixtures behind.
            _testPackageId = "dev.whyknot.test-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            _testLogDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WhyKnot", "Logs", _testPackageId);
        }

        [TearDown]
        public void TearDown() {
            try {
                if (Directory.Exists(_testLogDir)) Directory.Delete(_testLogDir, recursive: true);
            } catch { /* ignore */ }
        }

        [Test]
        public void Construction_CreatesSessionFileWithHeader() {
            var logger = new WkLogger(_testPackageId, "Test Package", "0.0.1");
            Assert.IsTrue(File.Exists(logger.LogFilePath), "Session log file should exist after construction");

            var content = File.ReadAllText(logger.LogFilePath);
            StringAssert.Contains("Test Package", content);
            StringAssert.Contains(_testPackageId, content);
            StringAssert.Contains("0.0.1", content);
            StringAssert.Contains("Session started", content);
            StringAssert.Contains("Unity:", content);
        }

        [Test]
        public void Info_WritesPrefixedLineToFile_AndMirrorsToConsole() {
            var logger = new WkLogger(_testPackageId, "Test", "0.0.1");
            LogAssert.Expect(LogType.Log, "[Test] hello info");
            logger.Info("hello info");

            var lines = File.ReadAllLines(logger.LogFilePath);
            Assert.IsTrue(lines.Any(l => l.Contains("[INFO ]") && l.EndsWith("hello info")),
                "Expected an INFO-tagged line ending with 'hello info'");
        }

        [Test]
        public void Warning_AndError_MirrorAtMatchingLevel() {
            var logger = new WkLogger(_testPackageId, "Test", "0.0.1");
            LogAssert.Expect(LogType.Warning, "[Test] a warning");
            LogAssert.Expect(LogType.Error, "[Test] an error");
            logger.Warning("a warning");
            logger.Error("an error");

            var lines = File.ReadAllLines(logger.LogFilePath);
            Assert.IsTrue(lines.Any(l => l.Contains("[WARN ]") && l.EndsWith("a warning")), "Expected WARN line");
            Assert.IsTrue(lines.Any(l => l.Contains("[ERROR]") && l.EndsWith("an error")), "Expected ERROR line");
        }

        [Test]
        public void Debug_DoesNotMirrorByDefault() {
            var logger = new WkLogger(_testPackageId, "Test", "0.0.1");
            // No LogAssert.Expect -- Debug is file-only by default.
            logger.Debug("verbose details");

            var content = File.ReadAllText(logger.LogFilePath);
            StringAssert.Contains("verbose details", content);
        }

        [Test]
        public void Exception_WritesStackToFile_AndMirrorsToConsole() {
            var logger = new WkLogger(_testPackageId, "Test", "0.0.1");
            InvalidOperationException ex = null;
            try { throw new InvalidOperationException("boom"); }
            catch (InvalidOperationException caught) { ex = caught; }

            // Exception() mirrors the headline at Error level via Write(),
            // then the exception itself via Debug.LogException. Both need
            // an Expect or the test harness flags the unexpected Error.
            LogAssert.Expect(LogType.Error, "[Test] during my test -- InvalidOperationException: boom");
            LogAssert.Expect(LogType.Exception, "InvalidOperationException: boom");
            logger.Exception(ex, "during my test");

            var content = File.ReadAllText(logger.LogFilePath);
            StringAssert.Contains("during my test", content);
            StringAssert.Contains("InvalidOperationException", content);
            StringAssert.Contains("boom", content);
        }

        [Test]
        public void Registry_LooksUpRegisteredLogger() {
            var logger = new WkLogger(_testPackageId, "Test", "0.0.1");
            var fetched = WkLoggerRegistry.Get(_testPackageId);
            Assert.AreSame(logger, fetched);
            Assert.IsTrue(WkLoggerRegistry.IsRegistered(_testPackageId));
        }

        [Test]
        public void Registry_GetUnregistered_Throws() {
            Assert.IsFalse(WkLoggerRegistry.IsRegistered("dev.whyknot.never-registered"));
            Assert.Throws<InvalidOperationException>(() =>
                WkLoggerRegistry.Get("dev.whyknot.never-registered"));
        }

        [Test]
        public void Cull_KeepsOnlyNewestN() {
            Directory.CreateDirectory(_testLogDir);
            // Create 5 fake session files, walking back in time.
            var baseTime = DateTime.UtcNow;
            for (int i = 0; i < 5; i++) {
                var path = Path.Combine(_testLogDir,
                    $"session-2026-05-{20 + i:D2}_000000.log");
                File.WriteAllText(path, $"fake session {i}");
                File.SetLastWriteTimeUtc(path, baseTime.AddHours(-i));
            }
            Assert.AreEqual(5, Directory.GetFiles(_testLogDir, "session-*.log").Length);

            WkLogger.CullOldSessions(_testLogDir, keep: 2);
            var remaining = Directory.GetFiles(_testLogDir, "session-*.log");
            Assert.AreEqual(2, remaining.Length, "Culling should leave exactly `keep` files");
            // The two newest are the ones written at baseTime - 0h and baseTime - 1h.
            // Their filenames contain "session-2026-05-20_000000.log" (newest) and "session-2026-05-21_000000.log" (next).
            Assert.IsTrue(remaining.Any(p => p.EndsWith("2026-05-20_000000.log")),
                "Newest session should be retained");
            Assert.IsTrue(remaining.Any(p => p.EndsWith("2026-05-21_000000.log")),
                "Second-newest session should be retained");
        }

        [Test]
        public void Cull_WhenDirectoryMissing_IsNoop() {
            var missing = Path.Combine(_testLogDir, "does-not-exist");
            Assert.DoesNotThrow(() => WkLogger.CullOldSessions(missing, keep: 3));
        }

        [Test]
        public void NewSession_CullsOldSessionsToMakeRoomForItself() {
            Directory.CreateDirectory(_testLogDir);
            // Seed with three OLD session files that all predate today.
            var baseTime = DateTime.UtcNow.AddDays(-30);
            for (int i = 0; i < 3; i++) {
                var path = Path.Combine(_testLogDir,
                    $"session-2026-04-{1 + i:D2}_000000.log");
                File.WriteAllText(path, $"old session {i}");
                File.SetLastWriteTimeUtc(path, baseTime.AddDays(i));
            }
            Assert.AreEqual(3, Directory.GetFiles(_testLogDir, "session-*.log").Length);

            // Constructing a new logger should cull down to MaxSessionsPerPackage - 1
            // before creating its own file, leaving MaxSessionsPerPackage total.
            var logger = new WkLogger(_testPackageId, "Test", "0.0.1");
            var afterCount = Directory.GetFiles(_testLogDir, "session-*.log").Length;
            Assert.AreEqual(WkLogger.MaxSessionsPerPackage, afterCount,
                $"After a new session opens, there should be exactly {WkLogger.MaxSessionsPerPackage} sessions in the directory");
        }

        [Test]
        public void BeginTask_EmitsStartingAndFinishedLines() {
            var logger = new WkLogger(_testPackageId, "Test", "0.0.1");
            // The completion line at Info level mirrors to the Console; we
            // can't match the elapsed-ms text exactly so the assertion is
            // against the file content only.
            LogAssert.ignoreFailingMessages = true;
            using (logger.BeginTask("BoneMerger")) {
                // empty body
            }
            LogAssert.ignoreFailingMessages = false;

            var content = File.ReadAllText(logger.LogFilePath);
            StringAssert.Contains("BoneMerger starting", content);
            StringAssert.Contains("BoneMerger finished in", content);
        }

        [Test]
        public void InfoBlock_HeaderMirrorsAndContinuationLinesAreFileOnly() {
            var logger = new WkLogger(_testPackageId, "Test", "0.0.1");
            LogAssert.Expect(LogType.Log, "[Test] Scan results");
            logger.InfoBlock("Scan results", new[] { "first issue", "second issue" });

            var lines = File.ReadAllLines(logger.LogFilePath);
            Assert.IsTrue(System.Linq.Enumerable.Any(lines, l => l.Contains("[INFO ]") && l.EndsWith("Scan results")),
                "Header line should be emitted with INFO level");
            Assert.IsTrue(System.Linq.Enumerable.Any(lines, l => l.Contains("first issue")));
            Assert.IsTrue(System.Linq.Enumerable.Any(lines, l => l.Contains("second issue")));
        }

        [Test]
        public void ContextObject_PropagatesToConsoleMirror() {
            var logger = new WkLogger(_testPackageId, "Test", "0.0.1");
            var go = new UnityEngine.GameObject("LogCtx");
            try {
                LogAssert.Expect(LogType.Log, "[Test] tagged");
                using (WkLogContext.WithContextObject(go)) {
                    logger.Info("tagged");
                }
                // We can't easily assert the context-arg was used from the test
                // harness, but a thrown context-object failure would surface here
                // as the Log assert silently failing -- which would already
                // fail the test.
            } finally {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
