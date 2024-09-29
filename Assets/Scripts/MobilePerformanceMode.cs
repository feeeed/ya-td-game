using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MobilePerformanceMode : MonoBehaviour
{
    public GameObject postFX;

    void Start()
    {
        //bool isWebGLOnDesktop = !Application.isMobilePlatform && Application.platform == RuntimePlatform.WebGLPlayer;
        //bool isWebGLOnMobile = Application.isMobilePlatform && Application.platform == RuntimePlatform.WebGLPlayer;

        if (Application.isMobilePlatform)
        {
            postFX.SetActive(false);
            
            var urp = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;

            urp.renderScale = 0.5f;
            Camera.main.GetComponent<UniversalAdditionalCameraData>().renderShadows = false;
        }
    }
}
