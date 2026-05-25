// WkEditorTickerTests.cs

using NUnit.Framework;
using WhyKnot.Core.Utilities;

namespace WhyKnot.Core.Tests {

    public sealed class WkEditorTickerTests {

        [Test]
        public void Construction_NullActionThrows() {
            Assert.Throws<System.ArgumentNullException>(() => new WkEditorTicker(0.1, null));
        }

        [Test]
        public void RunNow_InvokesTickBody() {
            int count = 0;
            var ticker = new WkEditorTicker(60, () => count++);
            ticker.RunNow();
            Assert.AreEqual(1, count);
            ticker.RunNow();
            Assert.AreEqual(2, count);
        }

        [Test]
        public void StartAndStop_AreIdempotent() {
            var ticker = new WkEditorTicker(60, () => { });
            Assert.IsFalse(ticker.IsRunning);
            ticker.Start();
            Assert.IsTrue(ticker.IsRunning);
            ticker.Start();
            Assert.IsTrue(ticker.IsRunning, "Second Start should be a no-op, not toggle off");
            ticker.Stop();
            Assert.IsFalse(ticker.IsRunning);
            ticker.Stop();
            Assert.IsFalse(ticker.IsRunning, "Second Stop should be a no-op");
        }

        [Test]
        public void RunNow_RecordsLastRunTime() {
            var ticker = new WkEditorTicker(60, () => { });
            ticker.RunNow();
            Assert.Greater(ticker.LastRunUnscaledTime, 0.0);
        }

        [Test]
        public void RunNow_TickBodyException_DoesNotPropagate() {
            var ticker = new WkEditorTicker(60, () => throw new System.InvalidOperationException("boom"));
            Assert.DoesNotThrow(() => ticker.RunNow());
        }
    }
}
