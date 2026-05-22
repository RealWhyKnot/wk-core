// WkCoreLogger.cs
//
// wk-core's own registered WkLogger instance. Anything inside the
// shared package that wants to log routes through WkCoreLogger.Instance.
// Downstream WhyKnot packages each have their own analogous holder
// (VrcfQolLogger.Instance, AvatarQolLogger.Instance, ...).
//
// The [InitializeOnLoad] attribute guarantees the static ctor runs when
// the Editor assembly loads. The WkLogger ctor self-registers with
// WkLoggerRegistry, so by the time any other code in this package runs,
// the wk-core entry is available via WkLoggerRegistry.Get(PackageId).
//
// When wk-core's package.json version changes, bump the Version constant
// below so the session header in the log file reports the right value.

using UnityEditor;

namespace WhyKnot.Core.Logging {

    [InitializeOnLoad]
    public static class WkCoreLogger {

        public const string PackageId = "dev.whyknot.core";
        public const string DisplayName = "WK Core";
        public const string Version = "1.1.2";

        public static readonly WkLogger Instance = new WkLogger(PackageId, DisplayName, Version);

        static WkCoreLogger() {
            // The field initializer above already created and registered
            // the logger. This static ctor exists so [InitializeOnLoad]
            // has something to anchor on: the attribute fires the type
            // initialiser, which runs the field initialiser, which builds
            // the WkLogger and registers it.
        }
    }
}
