using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrassBendingRTPrePass : ScriptableRendererFeature
{


    class CustomRenderPass : ScriptableRenderPass
    {
        static readonly int _GrassBendingRT_pid = Shader.PropertyToID("_GrassBendingRT");
        static readonly RenderTargetIdentifier _GrassBendingRT_rti = new RenderTargetIdentifier(_GrassBendingRT_pid);
        ShaderTagId GrassBending_stid = new ShaderTagId("GrassBending");

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(_GrassBendingRT_pid, new RenderTextureDescriptor(64, 64, RenderTextureFormat.R8,0));//64*64 is big enough for this demo
            ConfigureTarget(_GrassBendingRT_rti);
            ConfigureClear(ClearFlag.All, Color.white);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!InstancedIndirectGrassRenderer.instance)
            {
                Debug.LogError("InstancedIndirectGrassRenderer not found, abort GrassBendingRTPrePass");
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get("GrassBendingRT");

            //override view & Projection matrix
            Matrix4x4 projectionMatrix = Matrix4x4.Ortho(-50, 50, -50, 50, 1, 100);

            //make a view matrix that is the same as a new camera above grass 50 units and looking to grass(bird view)
            Matrix4x4 viewMatrix = Matrix4x4.TRS(InstancedIndirectGrassRenderer.instance.transform.position + new Vector3(0, 50, 0),Quaternion.LookRotation(-Vector3.up), new Vector3(1,1,-1)).inverse;

            cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            context.ExecuteCommandBuffer(cmd);
            
            //draw all trail renderer using SRP batching
            var drawSetting = CreateDrawingSettings(GrassBending_stid, ref renderingData, SortingCriteria.CommonTransparent);
            var filterSetting = new FilteringSettings(RenderQueueRange.all);
            context.DrawRenderers(renderingData.cullResults, ref drawSetting, ref filterSetting);

            //restore camera matrix
            cmd.Clear();
            cmd.SetViewProjectionMatrices(renderingData.cameraData.camera.worldToCameraMatrix, renderingData.cameraData.camera.projectionMatrix);

            //set global RT
            cmd.SetGlobalTexture(_GrassBendingRT_pid, new RenderTargetIdentifier(_GrassBendingRT_pid));

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_GrassBendingRT_pid);
        }
    }

    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses; //don't do RT switch when rendering _CameraColorTexture, so use AfterRenderingPrePasses
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


