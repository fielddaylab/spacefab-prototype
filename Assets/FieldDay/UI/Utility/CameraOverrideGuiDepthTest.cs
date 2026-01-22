using BeauUtil;
using BeauUtil.Debugger;
using FieldDay.Rendering;
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace FieldDay.UI {
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public sealed class CameraOverrideGuiDepthTest : MonoBehaviour, ICameraPreRenderCallback, ICameraPostRenderCallback {
        public CompareFunction DepthTest = CompareFunction.Always;

        [NonSerialized] private Camera m_Camera;
        
        private void OnEnable() {
            if (s_GuiZTestPropertyId == 0) {
                s_GuiZTestPropertyId = Shader.PropertyToID("unity_GUIZTestMode");
            }

            if (!m_Camera) {
                m_Camera = GetComponent<Camera>();
                Assert.NotNullOrDestroyed(m_Camera);
                CameraHelper.AddOnPreRender(m_Camera, this, -1000);
                CameraHelper.AddOnPostRender(m_Camera, this, -1000);
            }
        }

        private void OnDisable() {
            if (m_Camera) {
                CameraHelper.RemoveOnPostRender(m_Camera, this);
                CameraHelper.RemoveOnPreRender(m_Camera, this);
                m_Camera = null;
            }
        }

        void ICameraPreRenderCallback.OnCameraPreRender(Camera inCamera, CameraCallbackSource inSource) {
            Shader.SetGlobalFloat(s_GuiZTestPropertyId, (float)DepthTest);
        }

        void ICameraPostRenderCallback.OnCameraPostRender(Camera inCamera, CameraCallbackSource inSource) {
            Shader.SetGlobalFloat(s_GuiZTestPropertyId, (int)CompareFunction.LessEqual);
        }

        static private int s_GuiZTestPropertyId = 0;
    }
}