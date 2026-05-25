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
// Version resolves from UnityEditor.PackageManager.PackageInfo.FindForAssembly
// so package.json stays the single source of truth -- nothing else to
// bump on release. CI also enforces this via .github/workflows/version-guard.yml.

using UnityEditor;
using UnityEditor.PackageManager;

namespace WhyKnot.Core.Logging {

    [InitializeOnLoad]
    public static class WkCoreLogger {

        public const string PackageId = "dev.whyknot.core";
        public const string DisplayName = "WK Core";

        public static readonly string Version = ResolveVersion();

        public static readonly WkLogger Instance = new WkLogger(PackageId, DisplayName, Version);

        static WkCoreLogger() {
            // Field initializers above already created and registered the
            // logger. The [InitializeOnLoad] anchor on this static ctor
            // tells Unity to force-load the type at Editor startup.
        }

        private static string ResolveVersion() {
            // FindForAssembly returns null when the package is dropped loose
            // under Assets/ instead of installed via VPM. Fall back to a
            // sentinel rather than throwing -- a missing version label in
            // the log header is recoverable; an Editor-init exception is not.
            var info = PackageInfo.FindForAssembly(typeof(WkCoreLogger).Assembly);
            return info != null && !string.IsNullOrEmpty(info.version) ? info.version : "unknown";
        }
    }
}
