// WkAvatarPipelineTests.cs

using NUnit.Framework;
using WhyKnot.Core.Pipeline;

namespace WhyKnot.Core.Tests {

    public sealed class WkAvatarPipelineTests {

        private sealed class StubSession { public int Executes; public bool Disposed; }

        private sealed class StubPass : WkAvatarPass<StubSession> {
            public static int CreateCount;
            public static int RunCount;
            public static int DisposeCount;

            public override string DisplayName => "Stub";
            public override WkBuildPhase Phase => WkBuildPhase.Transforming;

            protected override StubSession CreateSession(WkBuildContext ctx) {
                CreateCount++;
                return new StubSession();
            }

            protected override void RunOnAvatar(WkBuildContext ctx, StubSession session) {
                RunCount++;
                session.Executes++;
            }

            protected override void DisposeSession(StubSession session) {
                DisposeCount++;
                session.Disposed = true;
            }
        }

        [SetUp]
        public void SetUp() {
            StubPass.CreateCount = 0;
            StubPass.RunCount = 0;
            StubPass.DisposeCount = 0;
        }

        [Test]
        public void Execute_RunsCreateRunDisposeInOrder() {
            var pass = new StubPass();
            var ctx = new WkBuildContext(null, WkBuildPhase.Transforming, WkBuildMode.Preview);
            pass.Execute(ctx);
            Assert.AreEqual(1, StubPass.CreateCount);
            Assert.AreEqual(1, StubPass.RunCount);
            Assert.AreEqual(1, StubPass.DisposeCount);
        }

        [Test]
        public void Execute_DisposesEvenWhenRunThrows() {
            var throwing = new ThrowingPass();
            var ctx = new WkBuildContext(null, WkBuildPhase.Transforming, WkBuildMode.Preview);
            Assert.Throws<System.InvalidOperationException>(() => throwing.Execute(ctx));
            Assert.AreEqual(1, ThrowingPass.DisposeCount, "Dispose must run on throw");
        }

        [Test]
        public void FallbackCallbackOrder_DefaultsToPhaseShifted() {
            Assert.AreEqual(-10000, new ResolvingPass().FallbackCallbackOrder);
            Assert.AreEqual(-5000,  new GeneratingPass().FallbackCallbackOrder);
            Assert.AreEqual(0,      new TransformingPass().FallbackCallbackOrder);
            Assert.AreEqual(5000,   new OptimizingPass().FallbackCallbackOrder);
        }

        private sealed class ThrowingPass : WkAvatarPass<StubSession> {
            public static int DisposeCount;
            public override string DisplayName => "Throwing";
            public override WkBuildPhase Phase => WkBuildPhase.Transforming;
            protected override StubSession CreateSession(WkBuildContext ctx) => new StubSession();
            protected override void RunOnAvatar(WkBuildContext ctx, StubSession s) => throw new System.InvalidOperationException("boom");
            protected override void DisposeSession(StubSession s) { DisposeCount++; }
        }

        private sealed class ResolvingPass    : WkAvatarPass<StubSession> { public override string DisplayName => "R"; public override WkBuildPhase Phase => WkBuildPhase.Resolving;    protected override StubSession CreateSession(WkBuildContext c) => null; protected override void RunOnAvatar(WkBuildContext c, StubSession s) { } }
        private sealed class GeneratingPass   : WkAvatarPass<StubSession> { public override string DisplayName => "G"; public override WkBuildPhase Phase => WkBuildPhase.Generating;   protected override StubSession CreateSession(WkBuildContext c) => null; protected override void RunOnAvatar(WkBuildContext c, StubSession s) { } }
        private sealed class TransformingPass : WkAvatarPass<StubSession> { public override string DisplayName => "T"; public override WkBuildPhase Phase => WkBuildPhase.Transforming; protected override StubSession CreateSession(WkBuildContext c) => null; protected override void RunOnAvatar(WkBuildContext c, StubSession s) { } }
        private sealed class OptimizingPass   : WkAvatarPass<StubSession> { public override string DisplayName => "O"; public override WkBuildPhase Phase => WkBuildPhase.Optimizing;   protected override StubSession CreateSession(WkBuildContext c) => null; protected override void RunOnAvatar(WkBuildContext c, StubSession s) { } }
    }
}
