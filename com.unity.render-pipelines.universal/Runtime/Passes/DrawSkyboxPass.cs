namespace UnityEngine.Rendering.Universal.Internal
{
    /// <summary>
    /// Draw the skybox into the given color buffer using the given depth buffer for depth testing.
    ///
    /// This pass renders the standard Unity skybox.
    /// </summary>
    public class DrawSkyboxPass : ScriptableRenderPass
    {
        public DrawSkyboxPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            // Setup Legacy XR buffer states
            if (renderingData.cameraData.xrPass.hasMultiXrView)
            {
                // Setup legacy skybox stereo buffer
                renderingData.cameraData.camera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, renderingData.cameraData.xrPass.GetProjMatrix(0));
                renderingData.cameraData.camera.SetStereoViewMatrix(Camera.StereoscopicEye.Left, renderingData.cameraData.xrPass.GetViewMatrix(0));
                renderingData.cameraData.camera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, renderingData.cameraData.xrPass.GetProjMatrix(1));
                renderingData.cameraData.camera.SetStereoViewMatrix(Camera.StereoscopicEye.Right, renderingData.cameraData.xrPass.GetViewMatrix(1));

                // Use legacy stereo instancing mode to have legacy XR code path configured
                cmd.SetSinglePassStereo(SinglePassStereoMode.Instancing);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                // Calling into built-in skybox pass
                context.DrawSkybox(renderingData.cameraData.camera);

                // Disable Legacy XR path
                cmd.SetSinglePassStereo(SinglePassStereoMode.None);
                context.ExecuteCommandBuffer(cmd);
            }
            else
            {
                float camFov = renderingData.cameraData.camera.fieldOfView;
                float camAspect = renderingData.cameraData.camera.aspect;

                // Decompose current pass' projection matrix
                Matrix4x4 projection = renderingData.cameraData.xrPass.GetProjMatrix(0);
                float fov = Mathf.Atan(1f / projection[1, 1]) * 2 * Mathf.Rad2Deg;
                float aspect = projection[1, 1] / projection[0, 0];
                // Setup legacy skybox camera data
                renderingData.cameraData.camera.fieldOfView = fov;
                renderingData.cameraData.camera.aspect = aspect;

                // Use legacy stereo none mode for legacy multi pass
                cmd.SetSinglePassStereo(SinglePassStereoMode.None);
                context.ExecuteCommandBuffer(cmd);

                // Calling into built-in skybox pass
                context.DrawSkybox(renderingData.cameraData.camera);
                // Require context flush to get skybox work executed before restoring camera data
                context.Submit();

                // Restore camera data
                renderingData.cameraData.camera.fieldOfView = camFov;
                renderingData.cameraData.camera.aspect = camAspect;
            }
            CommandBufferPool.Release(cmd);
        }
    }
}
