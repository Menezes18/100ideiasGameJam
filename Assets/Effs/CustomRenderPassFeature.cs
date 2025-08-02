using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRenderPassFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        public RenderTargetIdentifier source;
        private Material material;
        private RenderTargetHandle tempRenderTargetHandler;

        public CustomRenderPass(Material material)
        {
            this.material = material;
            tempRenderTargetHandler.Init("_TemporaryColorTexture");
        }

        public void SetMaterial(Material newMaterial)
        {
            material = newMaterial;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            source = renderingData.cameraData.renderer.cameraColorTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null)
            {
                Debug.LogWarning("Material não configurado para o Custom Render Pass.");
                return;
            }

            CommandBuffer commandBuffer = CommandBufferPool.Get("CustomBlitRenderPass");

            commandBuffer.GetTemporaryRT(tempRenderTargetHandler.id, renderingData.cameraData.cameraTargetDescriptor);
            Blit(commandBuffer, source, tempRenderTargetHandler.Identifier(), material);
            Blit(commandBuffer, tempRenderTargetHandler.Identifier(), source);

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempRenderTargetHandler.id);
        }
    }

    [System.Serializable]
    public class Settings
    {
        public Material material = null;
    }
    public Settings settings = new Settings();
    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(settings.material);
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
    
    // Método para atualizar o material do passe de renderização
    public void UpdateMaterial(Material newMaterial)
    {
        if (m_ScriptablePass != null)
        {
            m_ScriptablePass.SetMaterial(newMaterial);
        }
    }
    
    // Método público para habilitar/desabilitar o efeito de escala de cinza
    public void SetGrayscaleEnabled(bool enabled)
    {
        if (settings.material != null)
        {
            settings.material.SetFloat("_Enabled", enabled ? 1.0f : 0.0f);
        }
    }
}