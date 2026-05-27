// WkHotReloadStatus.cs
//
// EditorWindow exposing the live state of EditorHotReload: refresh
// counter, last compile result, last 50 file events, log file path,
// and an "Open in Explorer" jump. Built in IMGUI deliberately -- the
// HotReload assembly has no reference to wk-core's main Editor
// assembly (that isolation is the whole point of the separate
// asmdef), so we can't reach for WkStyles primitives here. Keep this
// view dependency-free.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WhyKnot.Core.HotReload {

    public sealed class WkHotReloadStatus : EditorWindow {

        // No [MenuItem] here -- the downstream wires its own menu path
        // (Window/WhyKnot/<DisplayName>/Hot Reload Status) from its
        // non-synced code so two synced copies of this file don't race
        // for the same menu path when both downstreams are installed.
        public static void Open() {
            var w = GetWindow<WkHotReloadStatus>(false, "Hot Reload Status");
            w.titleContent = new GUIContent("Hot Reload Status", BrandLogoTexture, "Hot Reload Status");
            w.minSize = new Vector2(460, 360);
            w.Show();
        }

        private Vector2 _scroll;
        private Vector2 _bodyScroll;
        private static GUIStyle _brandFooterStyle;
        private static Texture2D _brandLogo;
        private static bool _brandLogoLoaded;

        private static readonly string[] BrandLogoAssetPaths = {
            "Packages/dev.whyknot.core/Editor/Assets/WhyKnotLogo.png",
            "Packages/dev.whyknot.wk-vrc-qol/Editor/Internal/Assets/WhyKnotLogo.png",
            "Packages/dev.whyknot.wk-vrcfury-qol/Editor/Internal/Assets/WhyKnotLogo.png",
        };

        private void OnEnable() {
            // Keep the view fresh while it's visible. Hot-reload events
            // arrive on background-thread FSW callbacks; the static state
            // in EditorHotReload is updated under a lock, so a polling
            // repaint at 4 Hz is enough to feel live.
            EditorApplication.update += BumpRepaint;
        }

        private void OnDisable() {
            EditorApplication.update -= BumpRepaint;
        }

        private double _nextRepaint;
        private void BumpRepaint() {
            if (EditorApplication.timeSinceStartup < _nextRepaint) return;
            _nextRepaint = EditorApplication.timeSinceStartup + 0.25;
            Repaint();
        }

        private void OnGUI() {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
                EditorGUILayout.LabelField("Hot Reload Status", EditorStyles.boldLabel);
                DrawAnimatedAccentLine();

                using (var s = new EditorGUILayout.ScrollViewScope(
                        _bodyScroll, false, false,
                        GUILayout.ExpandWidth(true),
                        GUILayout.ExpandHeight(true))) {
                    _bodyScroll = s.scrollPosition;
                    DrawSummary();
                    EditorGUILayout.Space();
                    DrawRecentEvents();
                }

                DrawDivider();
                DrawFooter();
                DrawBrandFooter();
            }
        }

        private void DrawSummary() {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                EditorGUILayout.LabelField("Refresh count",     EditorHotReload.RefreshCount.ToString());
                EditorGUILayout.LabelField("Last compile",      EditorHotReload.LastCompileResult);
                EditorGUILayout.LabelField("Log file",          EditorHotReload.LogFilePath ?? "(not initialised)");
            }
        }

        private void DrawRecentEvents() {
            EditorGUILayout.LabelField("Recent file events (newest first)", EditorStyles.boldLabel);
            using (var s = new EditorGUILayout.ScrollViewScope(_scroll, GUILayout.MinHeight(160), GUILayout.MaxHeight(220))) {
                _scroll = s.scrollPosition;
                var events = EditorHotReload.RecentEvents;
                if (events == null || events.Count == 0) {
                    EditorGUILayout.LabelField("(no events yet)", EditorStyles.miniLabel);
                    return;
                }
                // Render newest-first.
                for (int i = events.Count - 1; i >= 0; i--) {
                    var e = events[i];
                    EditorGUILayout.LabelField($"{e.When:HH:mm:ss.fff}  {e.Kind,-9}  {e.Path}", EditorStyles.miniLabel);
                }
            }
        }

        private void DrawFooter() {
            using (new EditorGUILayout.HorizontalScope()) {
                var path = EditorHotReload.LogFilePath;
                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(path))) {
                    if (GUILayout.Button(
                            new GUIContent("Open Log in Explorer", "Reveal the current hot-reload log file."),
                            GUILayout.Height(22))) {
                        EditorUtility.RevealInFinder(path);
                    }
                }
                GUILayout.FlexibleSpace();
                var enabled = EditorPrefs.GetBool("dev.whyknot.core.settings.hot-reload-enabled", true);
                var next = GUILayout.Toggle(enabled, "Watcher enabled (next launch)", GUILayout.Height(22));
                if (next != enabled) {
                    EditorPrefs.SetBool("dev.whyknot.core.settings.hot-reload-enabled", next);
                }
            }
        }

        private static GUIStyle BrandFooterStyle =>
            _brandFooterStyle ??= new GUIStyle(EditorStyles.centeredGreyMiniLabel) {
                fontSize = 10,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter,
                richText = true,
                wordWrap = true,
                padding = new RectOffset(4, 4, 2, 2),
            };

        private static Color AnimatedAccentColor() {
            var pulse = 0.5f + 0.5f * Mathf.Sin((float)EditorApplication.timeSinceStartup * 2.4f);
            return Color.Lerp(new Color(0.23f, 0.56f, 0.78f), new Color(0.42f, 0.82f, 1f), pulse);
        }

        private static void DrawAnimatedAccentLine() {
            var rect = EditorGUILayout.GetControlRect(false, 2f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.23f, 0.56f, 0.78f, 0.35f));
            if (rect.width <= 1f) return;

            var cycle = Mathf.Repeat((float)EditorApplication.timeSinceStartup, 2.8f) / 2.8f;
            var width = Mathf.Clamp(rect.width * 0.28f, 48f, 180f);
            var x = Mathf.Lerp(rect.x - width, rect.xMax, cycle);
            EditorGUI.DrawRect(new Rect(x, rect.y, width, rect.height), AnimatedAccentColor());
        }

        private static void DrawDivider() {
            var rect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.25f));
        }

        private static void DrawBrandFooter() {
            var heartColor = ColorUtility.ToHtmlStringRGB(AnimatedAccentColor());
            var text = "Made with <color=#" + heartColor + ">\u2665</color> by WhyKnot";
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.MinHeight(22))) {
                if (BrandLogoTexture != null) {
                    var rect = GUILayoutUtility.GetRect(
                        38f,
                        20f,
                        GUILayout.Width(38f),
                        GUILayout.Height(20f),
                        GUILayout.ExpandWidth(false));
                    var previous = GUI.color;
                    GUI.color = new Color(1f, 1f, 1f, previous.a * 0.82f);
                    GUI.DrawTexture(rect, BrandLogoTexture, ScaleMode.ScaleToFit, true);
                    GUI.color = previous;
                }

                EditorGUILayout.LabelField(
                    new GUIContent(text, "Made with care by WhyKnot."),
                    BrandFooterStyle,
                    GUILayout.ExpandWidth(true),
                    GUILayout.MinHeight(20));
            }
        }

        private static Texture2D BrandLogoTexture {
            get {
                if (!_brandLogoLoaded) {
                    _brandLogo = LoadBrandLogoTexture();
                    _brandLogoLoaded = true;
                }
                return _brandLogo;
            }
        }

        private static Texture2D LoadBrandLogoTexture() {
            foreach (var path in BrandLogoAssetPaths) {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null) return texture;
            }

            var guids = AssetDatabase.FindAssets("WhyKnotLogo t:Texture2D");
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || !path.EndsWith("/WhyKnotLogo.png", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null) return texture;
            }

            return null;
        }
    }
}
