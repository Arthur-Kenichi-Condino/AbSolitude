using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Multiplayer.Tools.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Unity.Multiplayer.Tools.NetVis.Editor.Visualization
{
    class NetVisRenderer : IDisposable
    {
        const string k_ShaderName = "Hidden/NetVisFullscreenPass";
        static readonly Shader k_Shader;

        const string k_ShaderOutlineKeywordName = "NET_SCENE_VIS_OUTLINE";
        static readonly LocalKeyword k_ShaderOutlineKeyword;

        static readonly int k_SceneSaturation = Shader.PropertyToID("_NetworkSceneSaturation");

        static readonly int k_SceneRenderTextureIdentifier    = Shader.PropertyToID("_SceneRenderTex");
        static readonly int k_ObjectIdTextureIdentifier       = Shader.PropertyToID("_ObjectIdTex");
        static readonly int k_ObjectIdToColorBufferIdentifier = Shader.PropertyToID("_ObjectIdToColorBuffer");

        readonly Material m_Material;

        CommandBuffer m_CommandBuffer;

        ObjectIdRequest m_RenderRequest;

        RenderTexture m_ObjectIdTexture;
        RenderTexture m_SceneRenderTexture;

        ComputeBuffer m_ObjectIdToColorBuffer;

        public float SceneSaturation
        {
            set => m_Material.SetFloat(k_SceneSaturation, value);
        }

        public bool Outline
        {
            set => m_Material.SetKeyword(k_ShaderOutlineKeyword, value);
            get => m_Material.IsKeywordEnabled(k_ShaderOutlineKeyword);
        }

        public Camera Camera { get; }

        static NetVisRenderer()
        {
            DebugUtil.TraceMethodName();
            k_Shader = Shader.Find(k_ShaderName);
            k_ShaderOutlineKeyword = new LocalKeyword(k_Shader, k_ShaderOutlineKeywordName);
        }

        public NetVisRenderer(Camera camera)
        {
            DebugUtil.TraceMethodName();
            Camera = camera;
            m_Material = new(k_Shader);

#if UNITY_SRP_CORE_14_OR_HIGHER
            RenderPipelineManager.endCameraRendering += RenderNetSceneVis;
#else
            Camera.onPostRender += RenderNetSceneVis;
#endif
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public void Dispose()
        {
            DebugUtil.TraceMethodName();
            DisposeTextures();
            DisposeCommandBuffer();
            DisposeComputeBuffers();

#if UNITY_SRP_CORE_14_OR_HIGHER
            RenderPipelineManager.endCameraRendering -= RenderNetSceneVis;
#else
            Camera.onPostRender -= RenderNetSceneVis;
#endif
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.ExitingPlayMode)
            {
                Dispose();
            }
        }

#if UNITY_SRP_CORE_14_OR_HIGHER
        void RenderNetSceneVis(ScriptableRenderContext context, Camera camera)
        {
            if (camera != Camera)
            {
                return;
            }

            UpdateCommandBuffer(camera);
            context.ExecuteCommandBuffer(m_CommandBuffer);
            context.Submit();
            ReleaseCommandBuffer();
        }
#else
        void RenderNetSceneVis(Camera camera)
        {
            if (camera != Camera)
            {
                return;
            }

            UpdateCommandBuffer(camera);
            Graphics.ExecuteCommandBuffer(m_CommandBuffer);
        }
#endif

        void UpdateCommandBuffer(Camera camera)
        {
            InitializeCommandBuffer();
            InitializeNewTexturesIfNeeded();
            InitializeRenderRequest();
            
            camera.SubmitRenderRequest(m_RenderRequest);
            UpdateBuffers(m_RenderRequest.result!.idToObjectMapping);
            
            m_CommandBuffer.Clear();
            m_CommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, m_SceneRenderTexture);
            
            m_Material.SetTexture(k_ObjectIdTextureIdentifier, m_ObjectIdTexture);
            m_Material.SetTexture(k_SceneRenderTextureIdentifier, m_SceneRenderTexture);
            m_CommandBuffer.Blit(m_SceneRenderTexture, BuiltinRenderTextureType.CameraTarget, m_Material);
        }

        void InitializeCommandBuffer()
        {
#if UNITY_SRP_CORE_14_OR_HIGHER
            m_CommandBuffer = CommandBufferPool.Get(nameof(NetVisRenderer));
#else
            m_CommandBuffer ??= new CommandBuffer();
#endif
        }

#if UNITY_SRP_CORE_14_OR_HIGHER
        void ReleaseCommandBuffer()
        {
            CommandBufferPool.Release(m_CommandBuffer);
        }
#endif

        void DisposeCommandBuffer()
        {
#if !UNITY_SRP_CORE_14_OR_HIGHER
            m_CommandBuffer?.Dispose();
#endif
            m_CommandBuffer = null;
        }

        /// <summary>
        /// Returns the existing texture if the existing texture meets the requirements,
        /// and returns a new texture otherwise.
        /// </summary>
        static RenderTexture InitializeNewTextureIfNeeded(
            RenderTexture existingTexture,
            Camera camera,
            GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_UNorm,
            GraphicsFormat depthStencilFormat = GraphicsFormat.None)
        {
            if (existingTexture != null &&
                existingTexture.width == camera.pixelWidth &&
                existingTexture.height == camera.pixelHeight &&
                existingTexture.graphicsFormat == colorFormat &&
                existingTexture.depthStencilFormat == depthStencilFormat)
            {
                return existingTexture;
            }
            if (existingTexture != null)
            {
                existingTexture.Release();
            }
            RenderTextureDescriptor desc = new(
                width: camera.pixelWidth,
                height: camera.pixelHeight,
                colorFormat: colorFormat,
                depthStencilFormat: depthStencilFormat);
            return new RenderTexture(desc);
        }

        void InitializeNewTexturesIfNeeded()
        {
            m_ObjectIdTexture = InitializeNewTextureIfNeeded(m_ObjectIdTexture, Camera, depthStencilFormat: GraphicsFormat.D32_SFloat);
            m_SceneRenderTexture = InitializeNewTextureIfNeeded(m_SceneRenderTexture, Camera);
        }

        void DisposeTextures()
        {
            if (m_ObjectIdTexture != null)
            {
                m_ObjectIdTexture.Release();
                m_ObjectIdTexture = null;
            }
            if (m_SceneRenderTexture != null)
            {
                m_SceneRenderTexture.Release();
                m_SceneRenderTexture = null;
            }
        }

        void InitializeRenderRequest()
        {
            m_RenderRequest ??= new ObjectIdRequest(m_ObjectIdTexture);
            
            // Even if a render request already exists, it's possible that the destination texture (m_ObjectIdTexture)
            // has since changed due to resizing of the scene view window (in InitializeNewTextureIfNeeded)
            m_RenderRequest.destination = m_ObjectIdTexture;
        }

        Dictionary<int, Color> m_InstanceIdToColor;

        public void SetColorMapping(Dictionary<int, Color> instanceIdToColor)
        {
            m_InstanceIdToColor = instanceIdToColor;
        }

        public void UpdateBuffers(Object[] idToObjectMapping)
        {
            var objectIdCount = idToObjectMapping.Length;
            var objectIdToColors = new Color[objectIdCount];
            for (var objectId = 0; objectId < objectIdCount; ++objectId)
            {
                var obj = idToObjectMapping[objectId];
                if (obj == null)
                {
                    objectIdToColors[objectId] = Color.clear;
                    continue;
                }
                var instanceId = obj.GetInstanceID();
                if (m_InstanceIdToColor.TryGetValue(instanceId, out var color))
                {
                    objectIdToColors[objectId] = color;
                }
                else
                {
                    objectIdToColors[objectId] = Color.clear;
                }
            }
            SetComputeBuffer(k_ObjectIdToColorBufferIdentifier, objectIdToColors, ref m_ObjectIdToColorBuffer);
        }

        void SetComputeBuffer<T>(int bufferPropertyId, T[] data, ref ComputeBuffer computeBuffer) where T : struct
        {
            if (data.Length == 0)
            {
                return;
            }

            if (computeBuffer == null || computeBuffer.count != data.Length)
            {
                var stride = Marshal.SizeOf(typeof(T));
                computeBuffer?.Release();
                computeBuffer = new ComputeBuffer(data.Length, stride, ComputeBufferType.Default, ComputeBufferMode.Dynamic);
            }

            computeBuffer.SetData(data);
            m_Material.SetBuffer(bufferPropertyId, computeBuffer);
        }

        void DisposeComputeBuffers()
        {
            m_CommandBuffer?.Dispose();
            m_ObjectIdToColorBuffer?.Dispose();
        }
    }
}
