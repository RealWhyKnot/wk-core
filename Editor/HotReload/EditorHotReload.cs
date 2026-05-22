// EditorHotReload.cs
//
// Makes Unity pick up script changes while its window is open but not focused.
// All diagnostic output routes through WkCoreLogger so the session file at
// %LocalAppData%/WhyKnot/Logs/dev.whyknot.core/session-*.log captures every
// refresh tick, compile result, and assembly-reload event. Errors mirror to
// the Unity console; verbose per-file watcher chatter stays in the log file.
//
// Behaviour:
//   * FileSystemWatcher on Application.dataPath (recursive, *.cs) flips a flag
//     whenever a script is written, created, renamed or deleted.
//   * EditorApplication.update debounces the flag (~0.4 s) and then calls
//     AssetDatabase.Refresh(). Unity happily refreshes from scripted call even
//     when the editor window isn't focused, which is the whole point.
//   * CompilationPipeline.assemblyCompilationFinished records a one-line
//     summary per assembly plus one line per compile error. Errors surface
//     in the Unity Console; per-assembly summaries stay file-only.
//
// Zero-config: Unity loads it via [InitializeOnLoad]. If the FileSystemWatcher
// fails to start (permission error, Mono sandbox weirdness), the failure is
// logged and Unity's focus-based refresh continues to work.

using System;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using WhyKnot.Core.Logging;

namespace WhyKnot.Core.HotReload {

    [InitializeOnLoad]
    internal static class EditorHotReload {
        private const double DebounceSeconds = 0.4;

        private static FileSystemWatcher _watcher;
        private static volatile bool _pendingRefresh;
        private static double _refreshDueAt;
        private static int _refreshCounter;

        // Accessing WkCoreLogger.Instance here triggers WkCoreLogger's static
        // ctor (the [InitializeOnLoad] anchor), which builds and registers the
        // wk-core WkLogger before any callbacks below try to use it.
        private static WkLogger Log => WkCoreLogger.Instance;

        static EditorHotReload() {
            Log.Info("EditorHotReload starting");

            string dataPath;
            try {
                dataPath = Application.dataPath;
            } catch (Exception ex) {
                Log.Exception(ex, "Could not resolve Application.dataPath");
                return;
            }
            if (string.IsNullOrEmpty(dataPath)) {
                Log.Error("Application.dataPath was empty; watcher not started");
                return;
            }

            try {
                _watcher = new FileSystemWatcher(dataPath) {
                    IncludeSubdirectories = true,
                    Filter = "*.cs",
                    NotifyFilter = NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.Size
                                 | NotifyFilters.CreationTime,
                    EnableRaisingEvents = true,
                };
                _watcher.Changed += OnChange;
                _watcher.Created += OnChange;
                _watcher.Deleted += OnChange;
                _watcher.Renamed += OnRename;
                _watcher.Error   += (s, e) => Log.Warning($"[Watcher] Error: {e.GetException()?.Message}");
                Log.Info($"[Watcher] Active on {dataPath}");
            } catch (Exception ex) {
                Log.Exception(ex, "[Watcher] Failed to start");
            }

            EditorApplication.update += Tick;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompiled;
            CompilationPipeline.compilationStarted += OnCompileStarted;
            CompilationPipeline.compilationFinished += OnCompileFinished;
            AssemblyReloadEvents.beforeAssemblyReload += () => Log.Debug("[Reload] beforeAssemblyReload");
            AssemblyReloadEvents.afterAssemblyReload  += () => Log.Debug("[Reload] afterAssemblyReload");
        }

        // ----- change detection -----

        private static void OnChange(object s, FileSystemEventArgs e) {
            if (ShouldIgnore(e.FullPath)) return;
            _pendingRefresh = true;
            _refreshDueAt = EditorApplication.timeSinceStartup + DebounceSeconds;
            Log.Debug($"[Watcher] {e.ChangeType} {TrimPath(e.FullPath)}");
        }

        private static void OnRename(object s, RenamedEventArgs e) {
            if (ShouldIgnore(e.FullPath) && ShouldIgnore(e.OldFullPath)) return;
            _pendingRefresh = true;
            _refreshDueAt = EditorApplication.timeSinceStartup + DebounceSeconds;
            Log.Debug($"[Watcher] Renamed {TrimPath(e.OldFullPath)} -> {TrimPath(e.FullPath)}");
        }

        private static bool ShouldIgnore(string path) {
            if (string.IsNullOrEmpty(path)) return true;
            var name = Path.GetFileName(path);
            // Unity's own temp artefacts.
            if (name.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.EndsWith(".TMP", StringComparison.OrdinalIgnoreCase)) return true;
            if (name.StartsWith("~", StringComparison.Ordinal)) return true;
            return false;
        }

        private static void Tick() {
            if (!_pendingRefresh) return;
            if (EditorApplication.timeSinceStartup < _refreshDueAt) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            _pendingRefresh = false;
            _refreshCounter++;
            Log.Debug($"[Refresh] AssetDatabase.Refresh() #{_refreshCounter}");
            try { AssetDatabase.Refresh(); }
            catch (Exception ex) { Log.Exception(ex, "[Refresh] AssetDatabase.Refresh() failed"); }
        }

        // ----- compile result logging -----

        private static void OnCompileStarted(object ctx) {
            Log.Debug("[Compile] Started");
        }

        private static void OnCompileFinished(object ctx) {
            Log.Debug("[Compile] Finished");
        }

        private static void OnAssemblyCompiled(string asmPath, CompilerMessage[] messages) {
            int errors = 0, warnings = 0;
            if (messages != null) {
                for (int i = 0; i < messages.Length; i++) {
                    if (messages[i].type == CompilerMessageType.Error) errors++;
                    else if (messages[i].type == CompilerMessageType.Warning) warnings++;
                }
            }
            var asmName = Path.GetFileName(asmPath ?? "(unknown)");
            if (errors > 0) {
                Log.Warning($"[Asm] {asmName} errors={errors} warnings={warnings}");
            } else {
                Log.Debug($"[Asm] {asmName} errors={errors} warnings={warnings}");
            }

            if (messages == null) return;
            foreach (var m in messages) {
                if (m.type != CompilerMessageType.Error) continue;
                var where = string.IsNullOrEmpty(m.file) ? "" : $" {TrimPath(m.file)}({m.line},{m.column})";
                Log.Error($"[Compile]{where}: {m.message}");
            }
            // Warnings stay off by default; flip this on if it becomes useful.
            // foreach (var m in messages) {
            //     if (m.type != CompilerMessageType.Warning) continue;
            //     var where = string.IsNullOrEmpty(m.file) ? "" : $" {TrimPath(m.file)}({m.line},{m.column})";
            //     Log.Warning($"[Compile]{where}: {m.message}");
            // }
        }

        private static string TrimPath(string p) {
            if (string.IsNullOrEmpty(p)) return p;
            try {
                var root = Directory.GetParent(Application.dataPath)?.FullName;
                if (!string.IsNullOrEmpty(root) && p.StartsWith(root, StringComparison.OrdinalIgnoreCase)) {
                    return p.Substring(root.Length).TrimStart('/', '\\');
                }
            } catch { /* ignore */ }
            return p;
        }
    }
}
