// WkAac.cs
//
// Public facade for the animator-builder. Both backends (AAC-backed
// when WK_AAC is set, self-built otherwise) implement IWkAnimatorBuilder.
// Callers don't know or care which path they're on, but they do hand
// in a WkGeneratedAssetScope so the backend has a folder to write
// clips / blend trees / sub-assets into.
//
// Example usage (downstream code, no #if needed):
//   using var scope = new WkGeneratedAssetScope(WkGeneratedAssetTier.Temporary, "dev.whyknot.avatar-qol", "Loom");
//   var b = WkAac.For("Loom", controller, scope);
//   b.NewLayer("Outfit/Shirt").DefaultState("Off").State("On", offClip).Build();
//   b.Build();

using System;
using UnityEditor.Animations;
using UnityEngine;
using WhyKnot.Core.Pipeline;

namespace WhyKnot.Core.Animator {

    public static class WkAac {

        public static IWkAnimatorBuilder For(string systemName, AnimatorController controller, WkGeneratedAssetScope scope) {
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            if (scope == null)      throw new ArgumentNullException(nameof(scope));
#if WK_AAC
            return new WkAacAdapter(systemName, controller, scope);
#else
            return new WkAacFallback(systemName, controller, scope);
#endif
        }

        /// <summary>
        /// Convenience: construct a fresh AnimatorController under the
        /// scope's asset folder, then return a builder over it. The
        /// caller saves the controller (it's already in AssetDatabase)
        /// and references it from the avatar.
        /// </summary>
        public static IWkAnimatorBuilder NewController(string systemName, WkGeneratedAssetScope scope) {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            var controller = new AnimatorController { name = systemName };
            var path = scope.SaveAsset(controller, systemName);
            // Reload through AssetDatabase so subsequent SaveAssets-as-AddObjectToAsset work.
            var loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            return For(systemName, loaded ?? controller, scope);
        }
    }
}
