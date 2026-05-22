// EditorLogger.cs
//
// Thin Debug.Log wrapper that prepends a stable prefix to every line.
// Each downstream tool constructs its own singleton -- e.g. the package
// initialiser builds a "[Avatar QoL]" or "[VRCF QoL]" logger once and
// every call site uses it -- which keeps Editor.log grep targets
// predictable across releases.

using System;
using UnityEngine;

namespace WhyKnot.Core.Utilities {

    public sealed class EditorLogger {

        private readonly string _prefix;

        public EditorLogger(string prefix) {
            _prefix = string.IsNullOrEmpty(prefix) ? "" : prefix + " ";
        }

        public void Log(string message) => Debug.Log(_prefix + message);
        public void LogWarning(string message) => Debug.LogWarning(_prefix + message);
        public void LogError(string message) => Debug.LogError(_prefix + message);

        public void LogException(Exception ex) {
            if (ex == null) return;
            Debug.LogError(_prefix + ex.GetType().Name + ": " + ex.Message);
            Debug.LogException(ex);
        }
    }
}
