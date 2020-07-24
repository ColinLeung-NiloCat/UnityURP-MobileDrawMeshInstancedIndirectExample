using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrassBendingRTPrePass : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        int pid = Shader.PropertyToID("_GrassBendingRT");

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(pid, new RenderTextureDescriptor(64, 64, RenderTextureFormat.R8,0));
            ConfigureTarget(new RenderTargetIdentifier(pid));
            ConfigureClear(ClearFlag.All, Color.white);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            GameObject go = GameObject.Find("Camera-GrassBend");
            if (!go) return;

            CommandBuffer cmd = CommandBufferPool.Get("GrassBendingRT");
            
            //set camera matrix
            Matrix4x4 projectionMatrix = Matrix4x4.Ortho(-50, 50, -50, 50, 1, 100);
            ;
            Matrix4x4 viewMatrix = go.GetComponent<Camera>().worldToCameraMatrix;//Matrix4x4.Translate(new Vector3(0,10,0)) * Matrix4x4.LookAt(new Vector3(0,1,0),new Vector3(0,0,0),new Vector3(1,0,0));
            cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            

            //draw
            var drawSetting = CreateDrawingSettings(new ShaderTagId("GrassBending"), ref renderingData, SortingCriteria.CommonTransparent);
            var filterSetting = new FilteringSettings(RenderQueueRange.all);
            context.DrawRenderers(renderingData.cullResults, ref drawSetting, ref filterSetting);

            //restore camera matrix
            cmd.SetViewProjectionMatrices(renderingData.cameraData.camera.worldToCameraMatrix, renderingData.cameraData.camera.projectionMatrix);
            context.ExecuteCommandBuffer(cmd);
            cmd.SetGlobalTexture(pid, new RenderTargetIdentifier(pid));
            CommandBufferPool.Release(cmd);
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(pid);
        }
    }

    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


