#if (UNITY_EDITOR && !IGNORE_UNITY_EDITOR) || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

#if UNITY_2019_1_OR_NEWER
#define USE_SRP
#endif // UNITY_2019_1_OR_NEWER

#if UNITY_2019_1_OR_NEWER && HAS_URP
#define USING_URP
#endif // UNITY_2019_1_OR_NEWER

using System;
using System.Collections.Generic;
using System.Reflection;
using BeauPools;
using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Debugging;
using FieldDay.Perf;
using UnityEngine;

#if USE_SRP
using UnityEngine.Rendering;
#endif // USE_SRP

#if USING_URP
using UnityEngine.Rendering.Universal;
#endif // USING_URP

namespace FieldDay.Rendering {
    public sealed class ShadingMgr {
        #region Types

        [Serializable]
        internal struct Config {
            public Material DefaultGraphicMaterial;
        }

        #endregion // Types

        #region Events

        internal void Initialize(Config config) {
            if (config.DefaultGraphicMaterial) {
                MaterialUtility.SetDefaultUIGraphicMaterial(config.DefaultGraphicMaterial);
            }
        }

        internal void Shutdown() {

        }

        #endregion // Events
    }
}