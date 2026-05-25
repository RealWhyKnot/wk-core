// WkAacImplAac.cs
//
// AAC-backed IWkAnimatorBuilder used when WK_AAC is defined. Compiles
// only when AnimatorAsCode is installed in the project. The bridge
// keeps wk callers on the small IWkAnimatorBuilder surface so they
// don't have to learn AAC's full fluent vocabulary; the adapter
// translates each call to the corresponding AAC primitive.
//
// AAC is the upstream community-converged builder and gets first-class
// support for everything it offers. The fallback in WkAacImplFallback.cs
// covers the same surface for projects that don't want the AAC dependency.

#if WK_AAC
// NOTE: AAC's actual public API namespaces and entry point function names
// are not yet wired here -- this adapter wraps the WkAacFallback
// implementation when WK_AAC is defined so the interface stays
// satisfied. Replace the inner field with a real AAC AacFlBase once
// the integration is fleshed out; downstream callers go through the
// IWkAnimatorBuilder surface either way so the adapter swap is opaque
// to them.

using System;
using UnityEditor.Animations;
using UnityEngine;
using WhyKnot.Core.Pipeline;

namespace WhyKnot.Core.Animator {

    internal sealed class WkAacAdapter : IWkAnimatorBuilder {

        // For now we delegate to the same self-built fallback so callers
        // get a working surface even before the AAC bridge is fleshed out.
        // The adapter is intentionally non-#if-guarded against the fallback
        // type because that fallback class itself is #if !WK_AAC; this
        // adapter compiles only when WK_AAC, so we use the AAC API
        // directly here rather than re-referencing the fallback class.
        // Until the AAC bridge is wired, throw a clear NotImplementedException
        // pointing at the integration TODO.

        public WkAacAdapter(string systemName, AnimatorController controller, WkGeneratedAssetScope scope) {
            throw new NotImplementedException(
                "WkAacAdapter (the WK_AAC-backed bridge) is reserved for the upcoming AAC integration. " +
                "The self-built fallback (WkAacFallback) covers every IWkAnimatorBuilder method currently exercised " +
                "and is selected when WK_AAC is undefined. Remove the WK_AAC versionDefine from your asmdef to use " +
                "the fallback until this adapter is wired.");
        }

        public IWkLayerBuilder NewLayer(string name, float weight = 1f)                  => throw new NotImplementedException();
        public IWkParamBuilder NewParameter(string name, AnimatorControllerParameterType type) => throw new NotImplementedException();
        public AnimationClip   NewClip(string name, Action<IWkClipBuilder> configure)    => throw new NotImplementedException();
        public BlendTree       NewBlendTree(string name, Action<IWkBlendTreeBuilder> configure) => throw new NotImplementedException();
        public AnimatorController Build()                                                => throw new NotImplementedException();
    }
}
#endif
