using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
[DefaultExecutionOrder(-1000)]
public class GlobalRTCamera : MonoBehaviour
{
    public bool unlockFrameRate;
    public int targetFrameRate = 30;
    public Shader blurShader;
    public string globalTextureName;
    public Shader colorReplacementShader;
    public Shader normalReplacementShader;
    public int msaaSamples;
    public int resolution = 256;
    [Range(0, 4)]
    public int blurIterations = 1;
    public float blurRadius = 0.5f;

    public Camera colorCamera;
    public Camera normalCamera;
    public RenderTexture colorTarget;
    public RenderTexture normalTarget;
    public RenderTexture tempRT;

    Material blurMaterial;

    float frameRateDelta;
    float frameRateTime;

    void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        OnValidate();
    }
    void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        Cleanup();
    }

    void OnEndCameraRendering(ScriptableRenderContext ctx, Camera camera)
    {
        //var keyword = GlobalKeyword.Create("_RENDERING_JELLY_BLUR");

        var colorStartCmd = new CommandBuffer { name = "Jelly Blur Color" };
        //colorStartCmd.SetKeyword(keyword, true);
        //colorStartCmd.SetGlobalInt("_RENDERING_JELLY_BLUR", 1);
        //colorCamera.AddCommandBuffer(CameraEvent.BeforeDepthTexture, colorStartCmd);

        //var colorEndCmd = new CommandBuffer { name = "Jelly Blur Color" };
        
        for (int i = 0; i < blurIterations; i++)
        {
            Shader.SetGlobalVector("_BlurDirection", new Vector2(blurRadius, 0));
            Graphics.Blit(colorTarget, tempRT, blurMaterial, 0);

            Shader.SetGlobalVector("_BlurDirection", new Vector2(0, blurRadius));
            Graphics.Blit(tempRT, colorTarget, blurMaterial, 0);
        }
        Shader.SetGlobalTexture(globalTextureName + "Color", colorTarget);
        //colorStartCmd.SetKeyword(keyword, false);
        //colorStartCmd.SetGlobalInt("_RENDERING_JELLY_BLUR", 0);
        //colorCamera.AddCommandBuffer(CameraEvent.AfterEverything, colorEndCmd);
    }

    void OnValidate()
    {
        if (targetFrameRate > 0)
            frameRateDelta = 1f / targetFrameRate;
        else
            frameRateDelta = 0;

        Cleanup();

        if (!blurMaterial)
           blurMaterial = new Material(blurShader) { hideFlags = HideFlags.HideAndDontSave };

        CreateTargetIfNull(ref tempRT, resolution, false);

        if (colorCamera)
        {
            CreateTargetIfNull(ref colorTarget, resolution, true);
            if (colorReplacementShader)
                colorCamera.SetReplacementShader(colorReplacementShader, "RenderType");
        }
        if (normalCamera)
        {
            CreateTargetIfNull(ref normalTarget, resolution, true);
            if (normalReplacementShader)
                normalCamera.SetReplacementShader(normalReplacementShader, "RenderType");
        }
        if (unlockFrameRate)
            colorCamera.enabled = true;

        colorCamera.targetTexture = colorTarget;
        //AddCommandBuffers();
    }

    public void ClearColorTarget()
    {
        var renderTarget = RenderTexture.active;
        RenderTexture.active = colorTarget;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = renderTarget;
    }
    public void FitCameraToBounds(Bounds bounds, float minOrthoSize, float orthoSizeOffset)
    {
        transform.position = bounds.center + new Vector3(0, 10, 0);

        float aspectRatio = colorCamera.aspect;
        float targetRatio = bounds.size.x / bounds.size.z;

        if (aspectRatio >= targetRatio)
        {
            colorCamera.orthographicSize = Mathf.Max(minOrthoSize, orthoSizeOffset + (bounds.size.z / 2));
        }
        else
        {
            float differenceInSize = targetRatio / aspectRatio;
            colorCamera.orthographicSize = Mathf.Max(minOrthoSize, orthoSizeOffset + (bounds.size.z / 2 * differenceInSize));
        }
    }

    void AddCommandBuffers()
    {
        if (colorCamera)
        {
            colorCamera.targetTexture = colorTarget;

            var keyword = GlobalKeyword.Create("_RENDERING_JELLY_BLUR");

            var colorStartCmd = new CommandBuffer { name = "Jelly Blur Color" };
            colorStartCmd.SetKeyword(keyword, true);
            colorStartCmd.SetGlobalInt("_RENDERING_JELLY_BLUR", 1);
            colorCamera.AddCommandBuffer(CameraEvent.BeforeDepthTexture, colorStartCmd);

            var colorEndCmd = new CommandBuffer { name = "Jelly Blur Color" };
           
            for (int i = 0; i < blurIterations; i++)
            {
                colorEndCmd.SetGlobalVector("_BlurDirection", new Vector2(blurRadius, 0));
                colorEndCmd.Blit(colorTarget, tempRT, blurMaterial, 0);

                colorEndCmd.SetGlobalVector("_BlurDirection", new Vector2(0, blurRadius));
                colorEndCmd.Blit(tempRT, colorTarget, blurMaterial, 0);
            }
            colorEndCmd.SetGlobalTexture(globalTextureName + "Color", colorTarget);
            colorStartCmd.SetKeyword(keyword, false);
            colorStartCmd.SetGlobalInt("_RENDERING_JELLY_BLUR", 0);
            colorCamera.AddCommandBuffer(CameraEvent.AfterEverything, colorEndCmd);
        }

        if (normalCamera)
        {
            var normalCmd = new CommandBuffer { name = "Jelly Blur Normal" };
            normalCamera.targetTexture = normalTarget;

            for (int i = 0; i < blurIterations; i++)
            {
                normalCmd.SetGlobalVector("_BlurDirection", new Vector2(blurRadius, 0));
                normalCmd.Blit(normalTarget, tempRT, blurMaterial, 0);

                normalCmd.SetGlobalVector("_BlurDirection", new Vector2(0, blurRadius));
                normalCmd.Blit(tempRT, normalTarget, blurMaterial, 0);
            }
            normalCmd.SetGlobalTexture(globalTextureName + "Normal", normalTarget);
            normalCamera.AddCommandBuffer(CameraEvent.AfterEverything, normalCmd);
        }
    }

    void Cleanup()
    {
        DestroyImmediate(blurMaterial);
        if (colorCamera)
            colorCamera.RemoveAllCommandBuffers();
        if (normalCamera)
            normalCamera.RemoveAllCommandBuffers();
        
        ReleaseRT(ref colorTarget);
        ReleaseRT(ref normalTarget);
        ReleaseRT(ref tempRT);
    }
    void ReleaseRT(ref RenderTexture rt)
    {
        if (rt != null)
            rt.Release();
    }

    void CreateTargetIfNull(ref RenderTexture rt, int resolution, bool allowMSAA)
    {
        ReleaseRT(ref rt);

        var rtDesc = new RenderTextureDescriptor(resolution, resolution);
        rtDesc.colorFormat = RenderTextureFormat.ARGBHalf;
        rtDesc.depthBufferBits = 16;
        if (allowMSAA)
            rtDesc.msaaSamples = msaaSamples;
            
        rt = new RenderTexture(rtDesc) { hideFlags = HideFlags.HideAndDontSave};
        rt.filterMode = FilterMode.Bilinear;
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.antiAliasing = msaaSamples;
        rt.Create();
    }

    Matrix4x4 GetCameraMatrix(Camera camera)
    {
        //return camera.transform.worldToLocalMatrix;

        var bias = new Matrix4x4() {
            m00 = 0.5f, m01 = 0,    m02 = 0,    m03 = 0.5f,
            m10 = 0,    m11 = 0.5f, m12 = 0,    m13 = 0.5f,
            m20 = 0,    m21 = 0,    m22 = 0.5f, m23 = 0.5f,
            m30 = 0,    m31 = 0,    m32 = 0,    m33 = 1,
        };
        
        Matrix4x4 view = camera.worldToCameraMatrix;
        Matrix4x4 proj = camera.projectionMatrix;
        return bias * proj * view;
    }

    void Update()
    {
        if (frameRateDelta != 0 && !unlockFrameRate)
        {
            if (frameRateTime <= 0)
            {
                frameRateTime = frameRateDelta;
                colorCamera.enabled = true;
            }
            else 
            {
                frameRateTime -= Time.deltaTime;
                colorCamera.enabled = false;
            }
        }
        Shader.SetGlobalMatrix(globalTextureName + "Matrix", GetCameraMatrix(colorCamera));
/*
        colorCmd.Clear();
       
        Graphics.ExecuteCommandBuffer(colorCmd);
        Shader.SetGlobalTexture("_JellyBlurColor", colorTarget);

        colorCmd.Clear();
        colorCmd.name = "Jelly Blur Normal";
        colorCamera.targetTexture = normalTarget;
        colorCamera.SetReplacementShader(normalReplacementShader, "RenderType");
        colorCamera.Render();

        for (int i = 0; i < blurIterations; i++)
        {
            colorCmd.SetGlobalVector("_BlurDirection", new Vector2(blurRadius, 0));
            colorCmd.Blit(normalTarget, tempRT, blurMaterial, 0);

            colorCmd.SetGlobalVector("_BlurDirection", new Vector2(0, blurRadius));
            colorCmd.Blit(tempRT, normalTarget, blurMaterial, 0);
        }
        Graphics.ExecuteCommandBuffer(colorCmd);
        Shader.SetGlobalTexture("_JellyBlurColor", normalTarget);*/
    }
}
