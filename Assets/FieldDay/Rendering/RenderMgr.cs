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
    public sealed class RenderMgr : ICameraPreRenderCallback, ICameraPreCullCallback, ICameraPostRenderCallback {
        #region Types

#if DEVELOPMENT

        private struct CameraRestoreData {
            public int CameraId;

            public bool PostProcessing;
            public float FOV;
            public float Ortho;
            public int CullingMask;
            public CameraClearFlags ClearFlags;
            public Color32 BackgroundColor;

#if USING_URP
            public AntialiasingMode AA;
            public AntialiasingQuality AAQuality;
#endif // USING_URP

            public void Apply(Camera camera) {
                camera.cullingMask = CullingMask;
                camera.orthographicSize = Ortho;
                camera.fieldOfView = FOV;
                camera.clearFlags = ClearFlags;
                camera.backgroundColor = BackgroundColor;

#if USING_URP
                var data = camera.GetUniversalAdditionalCameraData();
                if (data) {
                    data.renderPostProcessing = PostProcessing;

                    data.antialiasing = AA;
                    data.antialiasingQuality = AAQuality;
                }
#endif // USING_URP
            }

            public void CreateFrom(Camera camera) {
                CameraId = camera.GetInstanceID();

                CullingMask = camera.cullingMask;
                Ortho = camera.orthographicSize;
                FOV = camera.fieldOfView;

                ClearFlags = camera.clearFlags;
                BackgroundColor = camera.backgroundColor;

#if USING_URP
                var data = camera.GetUniversalAdditionalCameraData();
                if (data) {
                    PostProcessing = data.renderPostProcessing;

                    AA = data.antialiasing;
                    AAQuality = data.antialiasingQuality;
                }
#endif // USING_URP
            }
        }

        private struct DebugCameraAdjustments {
            public enum ClearMode {
                Default,
                DepthClearOnly,
                ColorClear,
            }

            public bool DisablePostProcessing;
            public int DisableLayers;
            public int ForceLayers;
            public float? AdjustFOV;
            public float? AdjustOrthoSize;
            public ClearMode Clear;

#if USING_URP
            public AntialiasingMode? AA;
            public AntialiasingQuality? AAQuality;
#endif // USING_URP

            public bool CachedActive;

            public bool CheckIsActive() {
#if USING_URP
                if (AA.HasValue || AAQuality.HasValue) {
                    return true;
                }
#endif // USING_URP
                return DisablePostProcessing || DisableLayers != 0 || ForceLayers != 0
                    || AdjustFOV.HasValue || AdjustOrthoSize.HasValue || Clear != ClearMode.Default;
            }

            public void Apply(Camera camera) {
                camera.cullingMask = (camera.cullingMask | ForceLayers) & ~DisableLayers;
                if (AdjustFOV.HasValue) {
                    camera.fieldOfView = AdjustFOV.Value;
                }
                if (AdjustOrthoSize.HasValue) {
                    camera.orthographicSize = AdjustOrthoSize.Value;
                }

                switch(Clear) {
                    case ClearMode.DepthClearOnly: {
                        camera.clearFlags = CameraClearFlags.Depth;
                        break;
                    }
                    case ClearMode.ColorClear: {
                        camera.backgroundColor = s_DebugCameraClearColor;
                        camera.clearFlags = CameraClearFlags.SolidColor;
                        break;
                    }
                }

#if USING_URP
                var data = camera.GetUniversalAdditionalCameraData();
                if (DisablePostProcessing) {
                    data.renderPostProcessing = false;
                }

                if (AA.HasValue) {
                    data.antialiasing = AA.Value;
                }
                if (AAQuality.HasValue) {
                    data.antialiasingQuality = AAQuality.Value;
                }
#endif // USING_URP
            }
        }

#endif // DEVELOPMENT

        [Serializable]
        public struct Config {
            public Camera FallbackCamera;
            public Color DebugClearColor;
            public Color LetterboxColor;
            public DisplayConfiguration DisplayConfig;
        }

        public struct CameraChangeData {
            public Camera Previous;
            public Camera New;
        }

        private enum LightProbesState {
            Clean,
            Dirty,
            Tetrahedralizing
        }

        #endregion // Types

        private bool m_LastKnownFullscreen;
        private Resolution m_LastKnownResolution;
        private ScreenDpiType m_LastKnownDpi = (ScreenDpiType) (-1);

        private Camera m_PrimaryCamera;
        private Camera m_FallbackCamera;

        private RingBuffer<CameraClampToVirtualViewport> m_ClampedViewportCameras = new RingBuffer<CameraClampToVirtualViewport>(2, RingBufferMode.Expand);
        private Rect m_VirtualViewport = new Rect(0, 0, 1, 1);

        private DisplayConfiguration.Axis m_ReferenceResolutionAxis;
        private int m_ReferenceResolutionWidth;
        private int m_ReferenceResolutionHeight;
        private int m_ScaledReferenceResolutionWidth;
        private int m_ScaledReferenceResolutionHeight;

        private float m_MinAspect;
        private float m_MaxAspect;
        private bool m_HasLetterboxing;
        private Color m_LetterboxColor;

        private bool m_ShouldCheckFallback = true;
        private bool m_UsingFallback = false;
        private ushort m_LastLetterboxFrameRendered = Frame.InvalidIndex;

        private LightProbesState m_LightProbesState;
        private long m_LightProbesKickTS;

        private uint m_ManualRenderDepth;

#if DEVELOPMENT

        private CameraRestoreData m_DebugPrimaryCameraRestore;
        private DebugCameraAdjustments m_DebugPrimaryCameraAdjustments;
        
        static private Color32 s_DebugCameraClearColor;

        private void CacheDebugCameraAdjustments() {
            m_DebugPrimaryCameraAdjustments.CachedActive = m_DebugPrimaryCameraAdjustments.CheckIsActive();
        }

#endif // DEVELOPMENT

        #region Callbacks

        public readonly CastableEvent<bool> OnFullscreenChanged = new CastableEvent<bool>(2);
        public readonly CastableEvent<Resolution> OnResolutionChanged = new CastableEvent<Resolution>(2);
        public readonly CastableEvent<ScreenDpiType> OnScreenDpiChanged = new CastableEvent<ScreenDpiType>(2);
        public readonly CastableEvent<CameraChangeData> OnPrimaryCameraChanged = new CastableEvent<CameraChangeData>(2);

        #endregion // Callbacks

        #region Events

        internal void Initialize(Config config) {
            GameLoop.OnCanvasPreRender.Register(OnCanvasPreUpdate);
            GameLoop.OnApplicationPreRender.Register(OnApplicationPreRender);
            GameLoop.OnFrameAdvance.Register(OnApplicationPostRender);

#if DEVELOPMENT
            GameLoop.OnDebugUpdate.Register(OnDebugUpdate);
#endif // DEVELOPMENT

            Game.Scenes.OnAnySceneUnloaded.Register(OnSceneLoadUnload);
            Game.Scenes.OnAnySceneEnabled.Register(OnSceneLoadUnload);
            Game.Scenes.OnSceneReady.Register(OnSceneLoadUnload);

            CameraHelper.AddOnPreCull(this);
            CameraHelper.AddOnPreRender(this);
            CameraHelper.AddOnPostRender(this);

            m_FallbackCamera = config.FallbackCamera;
            if (m_FallbackCamera) {
                m_FallbackCamera.gameObject.SetActive(m_UsingFallback);
            }

            if (config.DebugClearColor == Color.clear) {
                config.DebugClearColor = ColorBank.Magenta;
            }
            s_DebugCameraClearColor = config.DebugClearColor;

            if (config.LetterboxColor == Color.clear) {
                m_LetterboxColor = Color.black;
            } else {
                m_LetterboxColor = config.LetterboxColor;
            }

            if (config.DisplayConfig) {
                EnableAspectClamping(config.DisplayConfig.MinimumAspectRatio, config.DisplayConfig.MaximumAspectRatio);
                m_ReferenceResolutionWidth = config.DisplayConfig.ReferenceResolution.x;
                m_ReferenceResolutionHeight = config.DisplayConfig.ReferenceResolution.y;
                m_ReferenceResolutionAxis = config.DisplayConfig.ReferenceAxis;
            } else {
                EnableAspectClamping(new Vector2Int(4, 3), new Vector2Int(16, 9));
                m_ReferenceResolutionAxis = DisplayConfiguration.Axis.Height;
                m_ReferenceResolutionWidth = 1024;
                m_ReferenceResolutionHeight = 768;
            }

            LightProbes.needsRetetrahedralization += OnLightProbesDirty;
            LightProbes.tetrahedralizationCompleted += OnLightProbesFinishedCompute;
        }

        internal void LateInitialize() {
            Game.Gui.OnPrimaryCameraChanged.Register(OnGuiCameraChanged);
            OnGuiCameraChanged(Game.Gui.PrimaryCamera);
        }

        internal void PollScreenSettings() {
            bool fullscreen = ScreenUtility.GetFullscreen();
            if (m_LastKnownFullscreen != fullscreen) {
                m_LastKnownFullscreen = fullscreen;
                OnFullscreenChanged.Invoke(fullscreen);
            }

            Resolution resolution = ScreenUtility.GetResolution();
            if (resolution.width != m_LastKnownResolution.width || resolution.height != m_LastKnownResolution.height
#if UNITY_2022_2_OR_NEWER
                || !resolution.refreshRateRatio.Equals(m_LastKnownResolution.refreshRateRatio)
#else
                || resolution.refreshRate != m_LastKnownResolution.refreshRate
#endif // UNITY_2022_2_OR_NEWER
                ) {
                m_LastKnownResolution = resolution;
                switch(m_ReferenceResolutionAxis) {
                    case DisplayConfiguration.Axis.Width: {
                        m_ScaledReferenceResolutionWidth = m_ReferenceResolutionWidth;
                        m_ScaledReferenceResolutionHeight = m_ReferenceResolutionWidth * resolution.height / resolution.width;
                        break;
                    }
                    case DisplayConfiguration.Axis.Height: {
                        m_ScaledReferenceResolutionHeight = m_ReferenceResolutionHeight;
                        m_ScaledReferenceResolutionWidth = m_ReferenceResolutionHeight * resolution.width / resolution.height;
                        break;
                    }
                }

                ScreenDpiType dpi = GetDpi(resolution);
                bool dpiChanged = dpi != m_LastKnownDpi;
                m_LastKnownDpi = dpi;

                OnResolutionChanged.Invoke(resolution);
                if (dpiChanged) {
                    OnScreenDpiChanged.Invoke(dpi);
                }
            }
        }

        internal void Shutdown() {
            GameLoop.OnCanvasPreRender.Deregister(OnCanvasPreUpdate);
            GameLoop.OnApplicationPreRender.Deregister(OnApplicationPreRender);
            GameLoop.OnFrameAdvance.Deregister(OnApplicationPostRender);

            Game.Scenes.OnAnySceneUnloaded.Deregister(OnSceneLoadUnload);
            Game.Scenes.OnAnySceneEnabled.Deregister(OnSceneLoadUnload);
            Game.Scenes.OnSceneReady.Deregister(OnSceneLoadUnload);

            CameraHelper.RemoveOnPreCull(this);
            CameraHelper.RemoveOnPreRender(this);
            CameraHelper.RemoveOnPostRender(this);

            OnResolutionChanged.Clear();
            OnFullscreenChanged.Clear();
            OnScreenDpiChanged.Clear();
            OnPrimaryCameraChanged.Clear();

            LightProbes.needsRetetrahedralization -= OnLightProbesDirty;
            LightProbes.tetrahedralizationCompleted -= OnLightProbesFinishedCompute;
        }

        #endregion // Events

        #region Dpi

        public ScreenDpiType CurrentDpiType {
            get { return m_LastKnownDpi; }
        }

        static private ScreenDpiType GetDpi(Resolution resolution) {
            // TODO: improve logic? fewer hardcoded values
            if (resolution.height > 2000) {
                return ScreenDpiType.ExtraHigh;
            }
            if (resolution.height > 1200) {
                return ScreenDpiType.High;
            }
            return ScreenDpiType.Normal;
        }

        #endregion // Dpi

        #region World Camera

        public Camera PrimaryCamera {
            get { return m_PrimaryCamera; }
        }

        public void SetPrimaryCamera(Camera camera) {
            if (m_PrimaryCamera != null) {
                Log.Warn("[RenderMgr] Primary world camera already set to '{0}' - make sure to deregister it first", m_PrimaryCamera);
            }
            Camera old = m_PrimaryCamera;
            m_PrimaryCamera = camera;
            m_ShouldCheckFallback = true;
            Log.Msg("[RenderMgr] Assigned primary world camera as '{0}'", camera);

            OnPrimaryCameraChanged.Invoke(new CameraChangeData() {
                Previous = old,
                New = camera
            });

            OnGuiCameraChanged(Game.Gui.PrimaryCamera);
        }

        public void RemovePrimaryCamera(Camera camera) {
            if (camera == null || m_PrimaryCamera != camera) {
                return;
            }

            Camera old = m_PrimaryCamera;
            m_PrimaryCamera = null;
            m_ShouldCheckFallback = true;
            Log.Msg("[RenderMgr] Removed primary world camera");
            
            OnPrimaryCameraChanged.Invoke(new CameraChangeData() {
                Previous = old,
                New = null
            });
        }

        #endregion // World Camera

        #region Clamped Viewport

        public void EnableAspectClamping(int width, int height) {
            m_MinAspect = (float) width / height;
            m_MaxAspect = m_MinAspect;
        }

        public void EnableMinimumAspectClamping(int width, int height) {
            m_MinAspect = (float) width / height;
            m_MaxAspect = float.MaxValue;
        }

        public void EnableAspectClamping(Vector2Int min, Vector2Int max) {
            m_MinAspect = (float) min.x / min.y;
            m_MaxAspect = (float) max.x / max.y;
        }

        public void DisableAspectClamping() {
            m_MinAspect = m_MaxAspect = 0;
            m_VirtualViewport = new Rect(0, 0, 1, 1);
        }

        public Rect VirtualViewport {
            get { return m_VirtualViewport; }
        }

        public void AddClampedViewportCamera(CameraClampToVirtualViewport camera) {
            Assert.NotNull(camera);
            m_ClampedViewportCameras.PushBack(camera);
        }

        public void RemoveClampedViewportCamera(CameraClampToVirtualViewport camera) {
            Assert.NotNull(camera);
            m_ClampedViewportCameras.FastRemove(camera);
        }

        #endregion // Clamped Viewport

        #region Fallback

        public bool HasFallbackCamera() {
            return m_FallbackCamera;
        }

        public void CreateDefaultFallbackCamera() {
            if (m_FallbackCamera) {
                Log.Warn("[RenderMgr] Fallback camera already in place.");
                return;
            }

            GameObject go = new GameObject("[RenderMgr Fallback]");
            Camera camera = go.AddComponent<Camera>();
            GameObject.DontDestroyOnLoad(go);
            go.SetActive(false);

            camera.cullingMask = 0;
            camera.orthographic = true;
            camera.orthographicSize = 0.5f;
            camera.backgroundColor = Color.black;
            camera.clearFlags = CameraClearFlags.SolidColor | CameraClearFlags.Depth;
            camera.depth = -100;

#if USING_URP
            var data = camera.GetUniversalAdditionalCameraData();
            data.renderType = CameraRenderType.Base;
            data.renderShadows = false;
            data.renderPostProcessing = false;
            data.requiresDepthTexture = false;
            data.requiresColorOption = CameraOverrideOption.Off;
            data.requiresColorTexture = false;
            data.stopNaN = false;
            data.dithering = false;
#endif // USING_URP

            m_FallbackCamera = camera;

            Log.Msg("[RenderMgr] Created default fallback camera");

            OnGuiCameraChanged(Game.Gui.PrimaryCamera);
            go.SetActive(m_UsingFallback);
        }

        /// <summary>
        /// Marks the "fallback camera" state as dirty.
        /// This will force it to be reevaluated before the next render.
        /// </summary>
        public void QueueFallbackCameraReevaluate() {
            m_ShouldCheckFallback = true;
        }

        #endregion // Fallback

        #region Lighting

        /// <summary>
        /// Tetrahedralizes light probes, if they need updating.
        /// </summary>
        public void TetrahedralizeLightProbes() {
            if (m_LightProbesState == LightProbesState.Dirty) {
                m_LightProbesState = LightProbesState.Tetrahedralizing;
                m_LightProbesKickTS = Frame.Timestamp();
                LightProbes.TetrahedralizeAsync();
            }
        }

        /// <summary>
        /// Returns if light probes are dirty or currently re-tetrahedralizing.
        /// </summary>
        public bool AreLightProbesDirty() {
            return m_LightProbesState != LightProbesState.Clean;
        }

        #endregion // Lighting

        #region Reference Resolution

        public Vector2 ReferencePixelsToVirtualViewportUnits(Vector2 referencePixels) {
            Vector2 viewport;
            viewport.x = referencePixels.x / m_ScaledReferenceResolutionWidth * m_VirtualViewport.x;
            viewport.y = referencePixels.y / m_ScaledReferenceResolutionHeight * m_VirtualViewport.y;
            return viewport;
        }

        #endregion // Reference Resolution

        #region Handlers

        private void OnLightProbesDirty() {
            if (m_LightProbesState != LightProbesState.Dirty) {
                m_LightProbesState = LightProbesState.Dirty;
                Log.Msg("[RenderMgr] Light probes need retetrahedralizing");
            }
        }

        private void OnLightProbesFinishedCompute() {
            if (m_LightProbesState == LightProbesState.Tetrahedralizing) {
                long ts = Frame.Timestamp() - m_LightProbesKickTS;
                m_LightProbesState = LightProbesState.Clean;
                Log.Msg("[RenderMgr] Light probes finished retetrahedralizing ({0}ms)", Profiling.TicksToMillisecs(ts));
            }
        }

        private void OnGuiCameraChanged(Camera uiCam) {
#if USING_URP
            if (m_FallbackCamera) {
                var data = m_FallbackCamera.GetUniversalAdditionalCameraData();
                if (data) {
                    if (uiCam != null) {
                        if (!data.cameraStack.Contains(uiCam)) {
                            data.cameraStack.Add(uiCam);
                        }
                    } else {
                        data.cameraStack.Clear();
                    }
                }
            }

            if (m_PrimaryCamera && uiCam && uiCam.GetUniversalAdditionalCameraData().renderType == CameraRenderType.Overlay) {
                var data = m_PrimaryCamera.GetUniversalAdditionalCameraData();
                if (data) {
                    if (uiCam != null) {
                        if (!data.cameraStack.Contains(uiCam)) {
                            data.cameraStack.Add(uiCam);
                        }
                    }
                }
            }
#endif // USING_URP
        }

        private void OnSceneLoadUnload() {
            m_ShouldCheckFallback = true;
        }

        private void CheckIfNeedsFallback() {
            if (!m_ShouldCheckFallback) {
                return;
            }

            bool needsFallback = !CameraUtility.AreAnyCamerasDirectlyRendering(m_FallbackCamera);
            if (Ref.Replace(ref m_UsingFallback, needsFallback)) {
                if (m_FallbackCamera) {
                    m_FallbackCamera.gameObject.SetActive(needsFallback);
                }
                Log.Msg("[RenderMgr] Fallback camera switched to {0}", needsFallback ? "ON" : "OFF");
            }
            
            m_ShouldCheckFallback = false;
        }

        private void OnCanvasPreUpdate() {
            if (m_ManualRenderDepth > 0) {
                return;
            }

#if DEVELOPMENT
            if (DebugFlags.IsFlagSet(DebuggingFlags.TraceExecution)) {
                Log.Trace("[RenderMgr] Canvas pre-update");
            }
#endif // DEVELOPMENT

            if (m_MinAspect <= 0 || m_MaxAspect <= 0) {
                m_HasLetterboxing = false;
                return;
            }

            m_VirtualViewport = UpdateAspectRatioClamping(m_LastKnownResolution.width, m_LastKnownResolution.height, m_MinAspect, m_MaxAspect);
            if (DebugFlags.IsFlagSet(DebuggingFlags.TraceExecution)) {
                Log.Trace("[RenderMgr] Virtual viewport is {0}", m_VirtualViewport.ToString());
            }

            for(int i = 0; i < m_ClampedViewportCameras.Count; i++) {
                ref var c = ref m_ClampedViewportCameras[i];
                Rect r = c.Viewport;
                Vector2 minOffset = ReferencePixelsToVirtualViewportUnits(c.MinPadding);
                Vector2 maxOffset = ReferencePixelsToVirtualViewportUnits(c.MaxPadding);
                r.x = m_VirtualViewport.x + (minOffset.x + r.x) * m_VirtualViewport.width;
                r.y = m_VirtualViewport.y + (minOffset.y + r.y) * m_VirtualViewport.height;
                r.width = (r.width - minOffset.x - maxOffset.x) * m_VirtualViewport.width;
                r.height = (r.height - minOffset.y - maxOffset.y) * m_VirtualViewport.height;
                c.Camera.rect = r;
            }

            m_HasLetterboxing = true;
        }

        private void OnApplicationPreRender() {
            if (m_ManualRenderDepth > 0) {
                return;
            }

#if DEVELOPMENT
            if (DebugFlags.IsFlagSet(DebuggingFlags.TraceExecution)) {
                Log.Trace("[RenderMgr] Application pre-render");
            }
#endif // DEVELOPMENT

            CheckIfNeedsFallback();
        }

        private void OnApplicationPostRender() {
            if (m_ManualRenderDepth > 0) {
                return;
            }

#if DEVELOPMENT
            if (DebugFlags.IsFlagSet(DebuggingFlags.TraceExecution)) {
                Log.Trace("[RenderMgr] Application post-render");
            }
#endif // DEVELOPMENT
        }

        static private Rect UpdateAspectRatioClamping(float w, float h, float min, float max) {
            float currentAspect = (float) w / h;
            float finalAspect = Mathf.Clamp(currentAspect, min, max);

            float aspectW = finalAspect;
            float aspectH = 1;

            if (aspectW > currentAspect) {
                aspectH = currentAspect / finalAspect;
                aspectW = aspectH * finalAspect;
            }

            float diffX = 1 - (aspectW / currentAspect),
                diffY = 1 - (aspectH / 1);

            Rect r = default;
            r.x = diffX / 2;
            r.y = diffY / 2;
            r.width = 1 - diffX;
            r.height = 1 - diffY;

            return r;
        }

        #endregion // Handlers

        #region Camera Callbacks

        void ICameraPreCullCallback.OnCameraPreCull(Camera inCamera, CameraCallbackSource inSource) {
            if (m_ManualRenderDepth > 0 || !GameLoop.IsRenderingOrPreparingRendering() || !CameraUtility.IsGameCamera(inCamera)) {
                return;
            }

#if DEVELOPMENT
            if (DebugFlags.IsFlagSet(DebuggingFlags.TraceExecution)) {
                Log.Trace("[RenderMgr] Camera '{0}' pre-cull", inCamera.name);
            }
#endif // DEVELOPMENT

            AttemptRenderLetterboxing(inCamera);

#if DEVELOPMENT
            if (m_DebugPrimaryCameraRestore.CameraId == 0 && m_DebugPrimaryCameraAdjustments.CachedActive && ReferenceEquals(inCamera, m_PrimaryCamera)) {
                if (DebugFlags.IsFlagSet(DebuggingFlags.TraceExecution)) {
                    Log.Trace("[RenderMgr] Applying debug changes to primary camera");
                }
                m_DebugPrimaryCameraRestore.CreateFrom(inCamera);
                m_DebugPrimaryCameraAdjustments.Apply(inCamera);
            }
#endif // DEVELOPMENT
        }

        void ICameraPreRenderCallback.OnCameraPreRender(Camera inCamera, CameraCallbackSource inSource) {
            if (m_ManualRenderDepth > 0 || !GameLoop.IsRendering() || !CameraUtility.IsGameCamera(inCamera)) {
                return;
            }

#if DEVELOPMENT
            if (DebugFlags.IsFlagSet(DebuggingFlags.TraceExecution)) {
                Log.Trace("[RenderMgr] Camera '{0}' pre-render", inCamera.name);
            }
#endif // DEVELOPMENT

#if DEVELOPMENT
            if (m_DebugPrimaryCameraRestore.CameraId == 0 && m_DebugPrimaryCameraAdjustments.CachedActive && ReferenceEquals(inCamera, m_PrimaryCamera)) {
                if (DebugFlags.IsFlagSet(DebuggingFlags.TraceExecution)) {
                    Log.Trace("[RenderMgr] Applying debug changes to primary camera");
                }
                m_DebugPrimaryCameraRestore.CreateFrom(inCamera);
                m_DebugPrimaryCameraAdjustments.Apply(inCamera);
            }
#endif // DEVELOPMENT
        }

        private void AttemptRenderLetterboxing(Camera camera) {
            if (camera.targetTexture != null) {
                return;
            }

            if (m_LastLetterboxFrameRendered != Frame.Index) {
                m_LastLetterboxFrameRendered = Frame.Index;

                RenderTexture prevRenderTarget = null;
                bool switchedRenderTargets = false;

                if (m_UsingFallback && !m_FallbackCamera) {
                    if (DebugFlags.IsFlagSet(DebuggingFlags.TraceExecution)) {
                        Log.Trace("[RenderMgr] Clearing backbuffer as fallback");
                    }

                    if (!switchedRenderTargets) {
                        switchedRenderTargets = true;
                        prevRenderTarget = RenderTexture.active;
                        Graphics.SetRenderTarget(null);
                    }

                    GL.PushMatrix();
                    GL.LoadOrtho();
                    GL.Clear(true, true, Color.black, 1);
                    GL.PopMatrix();
                }

                if (m_HasLetterboxing && m_ClampedViewportCameras.Count > 0) {
                    if (!switchedRenderTargets) {
                        switchedRenderTargets = true;
                        prevRenderTarget = RenderTexture.active;
                        Graphics.SetRenderTarget(null);
                    }

                    GL.PushMatrix();
                    GL.LoadOrtho();
                    GL.Viewport(new Rect(0, 0, m_LastKnownResolution.width, m_LastKnownResolution.height));
                    if (DebugFlags.IsFlagSet(DebuggingFlags.TraceExecution)) {
                        Log.Trace("[RenderMgr] Rendering letterboxing for viewport {0}", m_VirtualViewport.ToString());
                    }
                    CameraHelper.RenderLetterboxing(m_VirtualViewport, m_LetterboxColor);
                    GL.PopMatrix();
                }

                if (DebugFlags.IsFlagSet(DebuggingFlags.VisualizeEntireScreen)) {
                    if (!switchedRenderTargets) {
                        switchedRenderTargets = true;
                        prevRenderTarget = RenderTexture.active;
                        Graphics.SetRenderTarget(null);
                    }

                    GL.PushMatrix();
                    GL.LoadOrtho();
                    GL.Viewport(new Rect(0, 0, m_LastKnownResolution.width, m_LastKnownResolution.height));
                    GL.Clear(true, true, Color.magenta, 1);
                    GL.PopMatrix();

                    using(PooledStringBuilder psb = PooledStringBuilder.Create()) {
                        psb.Builder.Append("Screen Dimensions: ").AppendNoAlloc(m_LastKnownResolution.width)
                            .Append('x').AppendNoAlloc(m_LastKnownResolution.height);

                        if (m_LastKnownFullscreen) {
                            psb.Builder.Append(" (FULLSCREEN)");
                        }
                        if (m_LastKnownDpi == ScreenDpiType.ExtraHigh) {
                            psb.Builder.Append(" (X-HIGH DPI)");
                        } else if (m_LastKnownDpi == ScreenDpiType.High) {
                            psb.Builder.Append(" (HIGH DPI)");
                        }
                        DebugDraw.AddViewportText(new Vector2(0.5f, 1), new Vector2(0, -8), psb, Color.white, 0, TextAnchor.UpperCenter, DebugTextStyle.BackgroundDarkOpaque);
                    }
                }

                if (switchedRenderTargets) {
                    Graphics.SetRenderTarget(prevRenderTarget);
                }
            }
        }

        void ICameraPostRenderCallback.OnCameraPostRender(Camera inCamera, CameraCallbackSource inSource) {
            if (m_ManualRenderDepth > 0 || !GameLoop.IsRendering() || !CameraUtility.IsGameCamera(inCamera)) {
                return;
            }

#if DEVELOPMENT
            if (DebugFlags.IsFlagSet(DebuggingFlags.TraceExecution)) {
                Log.Trace("[RenderMgr] Camera '{0}' post-render", inCamera.name);
            }
#endif // DEVELOPMENT

#if DEVELOPMENT

            if (m_DebugPrimaryCameraRestore.CameraId == inCamera.GetInstanceID()) {
                m_DebugPrimaryCameraRestore.Apply(inCamera);
                m_DebugPrimaryCameraRestore = default;
                if (DebugFlags.IsFlagSet(DebuggingFlags.TraceExecution)) {
                    Log.Trace("[RenderMgr] Undoing debug changes to primary camera");
                }
            }

#endif // DEVELOPMENT
        }

        #endregion // Camera Callbacks

        #region Debug

        private enum DebuggingFlags {
            TraceExecution,
            VisualizeEntireScreen,
            DisplayGPUInfo
        }

        static private float s_ScreenshotScale = 4;

        /// <summary>
        /// Scale of all screenshots.
        /// </summary>
        static public float ScreenshotScale {
            get { return s_ScreenshotScale; }
            set { s_ScreenshotScale = Mathf.Clamp(s_ScreenshotScale, 1, 8); }
        }

#if DEVELOPMENT

        private enum DebugMetricsGroup {
            None,
            Basic,
            Vertex,
            DrawCalls,
            Batches,
            RenderTargets,
            Timings,
        }

        static private string s_CachedGraphicsDeviceName;
        static private string s_CachedGraphicsDeviceVendor;
        static private string s_CachedGraphicsDeviceVersion;
        static private string s_CachedGraphicsDeviceType;
        static private string s_CachedNPOTSupport;

        static private DebugMetricsGroup s_SelectedMetricsGroup;

        private void OnDebugUpdate() {
            if (DebugFlags.IsFlagSet(DebuggingFlags.DisplayGPUInfo)) {
                using (PooledStringBuilder psb = PooledStringBuilder.Create()) {
                    psb.Builder
                        .Append("GPU Type: ").Append(s_CachedGraphicsDeviceType ?? (s_CachedGraphicsDeviceType = SystemInfo.graphicsDeviceType.ToString()))
                        .Append("\nGPU Name: ").Append(s_CachedGraphicsDeviceName ?? (s_CachedGraphicsDeviceName = SystemInfo.graphicsDeviceName))
                        .Append(" (").AppendNoAlloc(SystemInfo.graphicsDeviceID).Append(")")
                        .Append("\nGPU Vendor: ").Append(s_CachedGraphicsDeviceVendor ?? (s_CachedGraphicsDeviceVendor = SystemInfo.graphicsDeviceVendor))
                        .Append(" (").AppendNoAlloc(SystemInfo.graphicsDeviceVendorID).Append(")")
                        .Append("\nGPU Version: ").Append(s_CachedGraphicsDeviceVersion ?? (s_CachedGraphicsDeviceVersion = SystemInfo.graphicsDeviceVersion))
                        .Append("\nGPU Memory Size: ").AppendNoAlloc(SystemInfo.graphicsMemorySize).Append("MiB")
                        .Append("\nShader Level: ").AppendNoAlloc(SystemInfo.graphicsShaderLevel)
                        .Append("\nMax Texture Size: ").AppendNoAlloc(SystemInfo.maxTextureSize)
                        .Append("\nNPOT Support: ").Append(s_CachedNPOTSupport ?? (s_CachedNPOTSupport = SystemInfo.npotSupport.ToString()));

                    DebugDraw.AddLogText(psb, ColorBank.LightGray);
                }
            }

            if (s_SelectedMetricsGroup != DebugMetricsGroup.None) {
                using(PooledStringBuilder psb = PooledStringBuilder.CreateLarge()) {
                    switch(s_SelectedMetricsGroup) {
                        case DebugMetricsGroup.Basic: {
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.VertexCount);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.TriangleCount);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.DrawCallsCount);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.SetPassCallsCount);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.BatchesCount);
                            break;
                        }
                        case DebugMetricsGroup.Vertex: {
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.VertexCount);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.TriangleCount);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.StaticBatchedVerticesCount);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.StaticBatchedTrianglesCount);
                            PerfMetric.WriteMetric(psb, RenderMetrics.VertexBufferUploadCount);
                            psb.Builder.Append(" ("); PerfMetric.WriteMetricValue(psb, RenderMetrics.VertexBufferUploadBytes); psb.Builder.Append(")\n");
                            PerfMetric.WriteMetric(psb, RenderMetrics.IndexBufferUploadCount);
                            psb.Builder.Append(" ("); PerfMetric.WriteMetricValue(psb, RenderMetrics.IndexBufferUploadBytes); psb.Builder.Append(")\n");
                            break;
                        }

                        case DebugMetricsGroup.DrawCalls: {
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.DrawCallsCount);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.SetPassCallsCount);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.MaterialSetPassFast);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.StaticBatchedDrawCallsCount);
                            break;
                        }

                        case DebugMetricsGroup.Batches: {
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.BatchesCount);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.StaticBatchesCount);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.StaticBatchedVerticesCount);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.StaticBatchedTrianglesCount);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.StaticBatchedDrawCallsCount);
                            break;
                        }

                        case DebugMetricsGroup.RenderTargets: {
                            PerfMetric.WriteMetric(psb, RenderMetrics.RenderTexturesCount);
                            psb.Builder.Append(" ("); PerfMetric.WriteMetricValue(psb, RenderMetrics.RenderTexturesBytes); psb.Builder.Append(")\n");
                            break;
                        }

                        case DebugMetricsGroup.Timings: {
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.Culling);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.RenderPrepare);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.Clear);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.RenderOpaqueGeometry);
                            PerfMetric.WriteMetricLine(psb, RenderMetrics.RenderTransparentGeometry);
                            break;
                        }
                    }

                    psb.Builder.TrimEnd(StringUtils.DefaultNewLineChars);
                    DebugDraw.AddViewportText(new Vector2(1, 0), new Vector2(-8, 8), psb, Color.white, 0, TextAnchor.LowerRight, DebugTextStyle.BackgroundDarkOpaque);
                }
            }
        }

        [EngineMenuFactory]
        static private DMInfo CreateRenderDebugMenu() {
            DMInfo info = new DMInfo("Rendering", 16);

            DMInfo screenshots = new DMInfo("Screenshots");
            screenshots.AddSlider("Resolution Scale", () => s_ScreenshotScale, (v) => s_ScreenshotScale = v, 1, 8, 0.5f, (f) => string.Format("{0:0.0}x", f));
            info.AddSubmenu(screenshots);

            info.AddDivider();

            info.AddSelector("Debug Metrics",
                () => (int)s_SelectedMetricsGroup,
                (i) => s_SelectedMetricsGroup = (DebugMetricsGroup)i,
                new string[] { "---", "Basic Stats", "Vertices", "Draw Calls", "Batches", "Render Targets", "Timing" });
            info.AddDivider();

            DMInfo debugOptions = new DMInfo("Debug Options");

            DebugFlags.Menu.AddFlagToggle(debugOptions, "Trace Execution", DebuggingFlags.TraceExecution);
            DebugFlags.Menu.AddSingleFrameFlagButton(debugOptions, "Trace Execution (Frame)", DebuggingFlags.TraceExecution);
            DebugFlags.Menu.AddFlagToggle(debugOptions, "Render Screen Info", DebuggingFlags.VisualizeEntireScreen);
            DebugFlags.Menu.AddFlagToggle(debugOptions, "Display GPU Info", DebuggingFlags.DisplayGPUInfo);

            debugOptions.AddSelector("Clear Mode",
                () => (int)Game.Rendering.m_DebugPrimaryCameraAdjustments.Clear,
                (i) => {
                    Game.Rendering.m_DebugPrimaryCameraAdjustments.Clear = (DebugCameraAdjustments.ClearMode)i;
                    Game.Rendering.CacheDebugCameraAdjustments();
                }, new string[] { "---", "Depth Only", "Debug Color" });

            debugOptions.AddSelector("Post Processing", () => Game.Rendering.m_DebugPrimaryCameraAdjustments.DisablePostProcessing ? 1 : 0,
                (i) => {
                    Game.Rendering.m_DebugPrimaryCameraAdjustments.DisablePostProcessing = i == 1;
                    Game.Rendering.CacheDebugCameraAdjustments();
                }, new string[] { "---", "Suppress" });

            info.AddSubmenu(debugOptions);

            info.AddDivider();

            DMInfo renderLayerMenu = new DMInfo("Rendering Layers");
            renderLayerMenu.MinimumWidth = 250;

            string[] layerSelectorLabels = new string[] { "---", "Disabled", "Always" };

            for (int i = 0; i < 32; i++) {
                string layerName = LayerMask.LayerToName(i);

                if (string.IsNullOrEmpty(layerName)) {
                    continue;
                }

                int idx = i;
                renderLayerMenu.AddSelector(layerName, () => {
                    if (Bits.Contains(Game.Rendering.m_DebugPrimaryCameraAdjustments.ForceLayers, idx)) {
                        return 2;
                    } else if (Bits.Contains(Game.Rendering.m_DebugPrimaryCameraAdjustments.DisableLayers, idx)) {
                        return 1;
                    } else {
                        return 0;
                    }
                }, (f) => {
                    Bits.Set(ref Game.Rendering.m_DebugPrimaryCameraAdjustments.ForceLayers, idx, f == 2);
                    Bits.Set(ref Game.Rendering.m_DebugPrimaryCameraAdjustments.DisableLayers, idx, f == 1);
                    Game.Rendering.CacheDebugCameraAdjustments();
                }, layerSelectorLabels);
            }

            info.AddSubmenu(renderLayerMenu);

            DMInfo qualitySettings = new DMInfo("Quality Settings");

            DMInfo antialiasingSettings = new DMInfo("Antialiasing");
            antialiasingSettings.MinimumWidth = 250;
#if USING_URP
            var renderPipeline = UniversalRenderPipeline.asset;
            antialiasingSettings.AddSlider("Antialiasing", () => {
                var aa = Game.Rendering.m_DebugPrimaryCameraAdjustments.AA;
                if (aa.HasValue) {
                    return 1 + (int) aa.Value;
                } else {
                    return 0;
                }
            }, (f) => {
                int m = (int) f;
                AntialiasingMode? mode = m == 0 ? null : (AntialiasingMode) (m - 1);
                Game.Rendering.m_DebugPrimaryCameraAdjustments.AA = mode;
                Game.Rendering.CacheDebugCameraAdjustments();
            }, 0, 3, 1, (f) => {
                int m = (int) f;
                if (m == 0) {
                    return "---";
                } else {
                    return ((AntialiasingMode) (m - 1)).ToString();
                }
            });

            antialiasingSettings.AddSlider("Quality", () => {
                var quality = Game.Rendering.m_DebugPrimaryCameraAdjustments.AAQuality;
                if (quality.HasValue) {
                    return 1 + (int) quality.Value;
                } else {
                    return 0;
                }
            }, (f) => {
                int q = (int) f;
                AntialiasingQuality? quality = q == 0 ? null : (AntialiasingQuality) (q - 1);
                Game.Rendering.m_DebugPrimaryCameraAdjustments.AAQuality = quality;
                Game.Rendering.CacheDebugCameraAdjustments();
            }, 0, 3, 1, (f) => {
                int m = (int) f;
                if (m == 0) {
                    return "---";
                } else {
                    return ((AntialiasingQuality) (m - 1)).ToString();
                }
            }, null, 1);
#else

#endif // USING_URP

            qualitySettings.AddSubmenu(antialiasingSettings);

            info.AddSubmenu(qualitySettings);

            DMInfo auditMenu = new DMInfo("Audit GPU Support");

            auditMenu.AddButton("Find Unsupported Shaders", () => {
                var allShaders = Resources.FindObjectsOfTypeAll<Shader>();
                using(PooledStringBuilder psb = PooledStringBuilder.Create()) {
                    int totalUnsupported = 0;
                    foreach(var shader in allShaders) {
                        if (!shader.isSupported) {
                            totalUnsupported++;
                            psb.Builder.Append("\nShader '").Append(shader.name).Append("' unsupported!");
                        }
                    }

                    if (totalUnsupported == 0) {
                        psb.Builder.Append("No unsupported shaders found!");
                        DebugDraw.AddLogText(psb, Color.white, 4);
                        Log.Msg(psb.Builder.ToString());
                    } else {
                        psb.Builder.Insert(0, string.Format("{0}/{1} shaders unsupported!", totalUnsupported, allShaders.Length));
                        DebugDraw.AddLogText(psb, Color.red, 8);
                        Log.Warn(psb.Builder.ToString());
                    }
                }
            });
            auditMenu.AddButton("Print Texture Format Support", () => {
                using (PooledStringBuilder psb = PooledStringBuilder.Create()) {
                    var allFormatFields = typeof(TextureFormat).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    int totalUnsupported = 0;
                    int totalFormats = 0;
                    foreach (var field in allFormatFields) {
                        if (field.IsDefined(typeof(HiddenAttribute)) || field.IsDefined(typeof(ObsoleteAttribute))) {
                            continue;
                        }

                        totalFormats++;
                        TextureFormat format = (TextureFormat)field.GetValue(null);

                        if (!SystemInfo.SupportsTextureFormat(format)) {
                            totalUnsupported++;
                            psb.Builder.Append("\nFormat '").Append(field.Name).Append("' unsupported!");
                        } else {
                            psb.Builder.Append("\n<color=#FFFFFF>Format '").Append(field.Name).Append("' is supported!</color>");
                        }
                    }

                    if (totalUnsupported == 0) {
                        psb.Builder.Append("No unsupported texture formats found!");
                        DebugDraw.AddLogText(psb, Color.white, 4);
                        Log.Msg(psb.Builder.ToString());
                    } else {
                        psb.Builder.Insert(0, string.Format("{0}/{1} texture formats unsupported!", totalUnsupported, totalFormats));
                        DebugDraw.AddLogText(psb, Color.red, 8);
                        Log.Warn(psb.Builder.ToString());
                    }
                }
            });

            info.AddSubmenu(auditMenu);

            return info;
        }

#endif // DEVELOPMENT

        #endregion // Debug

        #region Manual Rendering

        public void PushManualRender() {
            m_ManualRenderDepth++;
        }

        public void PopManualRender() {
            Assert.True(m_ManualRenderDepth > 0);
            m_ManualRenderDepth--;
        }

        #endregion // Manual Rendering
    }

    /// <summary>
    /// Type of screen dpi.
    /// </summary>
    public enum ScreenDpiType {
        Normal,
        High,
        ExtraHigh
    }

    /// <summary>
    /// Rendering metrics set.
    /// </summary>
    static public class RenderMetrics {
        static public readonly PerfMetric VertexCount = new PerfMetric(PerfMetric.Categories.Render, "Vertices Count");
        static public readonly PerfMetric TriangleCount = new PerfMetric(PerfMetric.Categories.Render, "Triangles Count");
        static public readonly PerfMetric SetPassCallsCount = new PerfMetric(PerfMetric.Categories.Render, "SetPass Calls Count");
        static public readonly PerfMetric DrawCallsCount = new PerfMetric(PerfMetric.Categories.Render, "Draw Calls Count");
        static public readonly PerfMetric MaterialSetPassFast = new PerfMetric(PerfMetric.Categories.Render, "Material.SetPassFast", "Material SetPass");
        static public readonly PerfMetric BatchesCount = new PerfMetric(PerfMetric.Categories.Render, "Batches Count");

        static public readonly PerfMetric RenderTexturesCount = new PerfMetric(PerfMetric.Categories.Render, "Render Textures Count");
        static public readonly PerfMetric RenderTexturesBytes = new PerfMetric(PerfMetric.Categories.Render, "Render Textures Bytes");

        static public readonly PerfMetric UsedBuffersCount = new PerfMetric(PerfMetric.Categories.Render, "Used Buffers Count");
        static public readonly PerfMetric UsedBuffersBytes = new PerfMetric(PerfMetric.Categories.Render, "Used Buffers Bytes");

        static public readonly PerfMetric UsedTexturesCount = new PerfMetric(PerfMetric.Categories.Render, "Used Textures Count");
        static public readonly PerfMetric UsedTexturesBytes = new PerfMetric(PerfMetric.Categories.Render, "Used Textures Bytes");

        static public readonly PerfMetric VertexBufferUploadCount = new PerfMetric(PerfMetric.Categories.Render, "Vertex Buffer Upload In Frame Count");
        static public readonly PerfMetric VertexBufferUploadBytes = new PerfMetric(PerfMetric.Categories.Render, "Vertex Buffer Upload In Frame Bytes");

        static public readonly PerfMetric IndexBufferUploadCount = new PerfMetric(PerfMetric.Categories.Render, "Index Buffer Upload In Frame Count");
        static public readonly PerfMetric IndexBufferUploadBytes = new PerfMetric(PerfMetric.Categories.Render, "Index Buffer Upload In Frame Bytes");

        static public readonly PerfMetric StaticBatchedVerticesCount = new PerfMetric(PerfMetric.Categories.Render, "Static Batched Vertices Count");
        static public readonly PerfMetric StaticBatchedTrianglesCount = new PerfMetric(PerfMetric.Categories.Render, "Static Batched Triangles Count");
        static public readonly PerfMetric StaticBatchedDrawCallsCount = new PerfMetric(PerfMetric.Categories.Render, "Static Batched Draw Calls Count");
        static public readonly PerfMetric StaticBatchesCount = new PerfMetric(PerfMetric.Categories.Render, "Static Batches Count");

        static public readonly PerfMetric RenderOpaqueGeometry = new PerfMetric(PerfMetric.Categories.Render, "Render.OpaqueGeometry", "Render Opaque Geo");
        static public readonly PerfMetric RenderTransparentGeometry = new PerfMetric(PerfMetric.Categories.Render, "Render.TransparentGeometry", "Render Transparent Geo");
        static public readonly PerfMetric RenderPrepare = new PerfMetric(PerfMetric.Categories.Render, "Render.Prepare", "Render Prepare");
        static public readonly PerfMetric Culling = new PerfMetric(PerfMetric.Categories.Render, "Culling");
        static public readonly PerfMetric Clear = new PerfMetric(PerfMetric.Categories.Render, "Clear");

        static public readonly PerfMetric ShaderParse = new PerfMetric(PerfMetric.Categories.Render, "Shader.ParseMainThread", "Shader Parse (Main)");
        static public readonly PerfMetric ShaderParseThreaded = new PerfMetric(PerfMetric.Categories.Render, "Shader.ParseThreaded", "Shader Parse (Threaded)");
    }
}