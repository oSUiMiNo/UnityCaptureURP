using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UnityCaptureURP_RendererFeature : ScriptableRendererFeature
{
    [SerializeField] string WebCamName = "WebCam";
    static GameObject WebCam;
    static RenderTexture RenderTexture;
    static Interface CaptureInterface;

    public enum ECaptureDevice { CaptureDevice1 = 0, CaptureDevice2 = 1, CaptureDevice3 = 2, CaptureDevice4 = 3, CaptureDevice5 = 4, CaptureDevice6 = 5, CaptureDevice7 = 6, CaptureDevice8 = 7, CaptureDevice9 = 8, CaptureDevice10 = 9 }
    public enum EResizeMode { Disabled = 0, LinearResize = 1 }
    public enum EMirrorMode { Disabled = 0, MirrorHorizontally = 1 }
    public enum ECaptureSendResult { SUCCESS = 0, WARNING_FRAMESKIP = 1, WARNING_CAPTUREINACTIVE = 2, ERROR_UNSUPPORTEDGRAPHICSDEVICE = 100, ERROR_PARAMETER = 101, ERROR_TOOLARGERESOLUTION = 102, ERROR_TEXTUREFORMAT = 103, ERROR_READTEXTURE = 104, ERROR_INVALIDCAPTUREINSTANCEPTR = 200 };

    [SerializeField][Tooltip("Capture device index")] public ECaptureDevice CaptureDevice = ECaptureDevice.CaptureDevice1;
    [SerializeField][Tooltip("Scale image if Unity and capture resolution don't match (can introduce frame dropping, not recommended)")] public EResizeMode ResizeMode = EResizeMode.Disabled;
    [SerializeField][Tooltip("How many milliseconds to wait for a new frame until sending is considered to be stopped")] public int Timeout = 1000;
    [SerializeField][Tooltip("Mirror captured output image")] public EMirrorMode MirrorMode = EMirrorMode.Disabled;
    [SerializeField][Tooltip("Introduce a frame of latency in favor of frame rate")] public bool DoubleBuffering = false;
    [SerializeField][Tooltip("Check to enable VSync during capturing")] public bool EnableVSync = false;
    [SerializeField][Tooltip("Set the desired render target frame rate")] public int TargetFrameRate = 60;
    [SerializeField][Tooltip("Check to disable output of warnings")] public static bool HideWarnings = false;



    UnityCaptureURP_RenderPass m_ScriptablePass;
    public override void Create()
    {
        m_ScriptablePass = new UnityCaptureURP_RenderPass
        {
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques
        };

#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying) return;
#endif
        WebCam = GameObject.Find(WebCamName);
        RenderTexture = WebCam.GetComponent<Camera>().targetTexture;
        CaptureInterface = new Interface(CaptureDevice);

        Debug.Log(RenderTexture);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }



    class UnityCaptureURP_RenderPass : ScriptableRenderPass
    {
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying) return;
#endif

            // RenderTexture is flipped left and right.
            // Therefore, by setting [EMirrorMode] to [MirrorHorizontally], it is flipped left and right again and sent.
            switch (CaptureInterface.SendTexture(RenderTexture, 1000, false, EResizeMode.Disabled, EMirrorMode.MirrorHorizontally))
            {
                case ECaptureSendResult.SUCCESS: break;
                case ECaptureSendResult.WARNING_FRAMESKIP: if (!HideWarnings) Debug.LogWarning("[UnityCapture] Capture device did skip a frame read, capture frame rate will not match render frame rate."); break;
                case ECaptureSendResult.WARNING_CAPTUREINACTIVE: if (!HideWarnings) Debug.LogWarning("[UnityCapture] Capture device is inactive"); break;
                case ECaptureSendResult.ERROR_UNSUPPORTEDGRAPHICSDEVICE: Debug.LogError("[UnityCapture] Unsupported graphics device (only D3D11 supported)"); break;
                case ECaptureSendResult.ERROR_PARAMETER: Debug.LogError("[UnityCapture] Input parameter error"); break;
                case ECaptureSendResult.ERROR_TOOLARGERESOLUTION: Debug.LogError("[UnityCapture] Render resolution is too large to send to capture device"); break;
                case ECaptureSendResult.ERROR_TEXTUREFORMAT: Debug.LogError("[UnityCapture] Render texture format is unsupported (only basic non-HDR (ARGB32) and HDR (FP16/ARGB Half) formats are supported)"); break;
                case ECaptureSendResult.ERROR_READTEXTURE: Debug.LogError("[UnityCapture] Error while reading texture image data"); break;
                case ECaptureSendResult.ERROR_INVALIDCAPTUREINSTANCEPTR: Debug.LogError("[UnityCapture] Invalid Capture Instance Pointer"); break;
            }
        }

    }



    public class Interface
    {
        [System.Runtime.InteropServices.DllImport("UnityCapturePlugin")] extern static System.IntPtr CaptureCreateInstance(int CapNum);
        [System.Runtime.InteropServices.DllImport("UnityCapturePlugin")] extern static void CaptureDeleteInstance(System.IntPtr instance);
        [System.Runtime.InteropServices.DllImport("UnityCapturePlugin")] extern static ECaptureSendResult CaptureSendTexture(System.IntPtr instance, System.IntPtr nativetexture, int Timeout, bool UseDoubleBuffering, EResizeMode ResizeMode, EMirrorMode MirrorMode, bool IsLinearColorSpace);
        System.IntPtr CaptureInstance;

        public Interface(ECaptureDevice CaptureDevice)
        {
            CaptureInstance = CaptureCreateInstance((int)CaptureDevice);
        }

        ~Interface()
        {
            Close();
        }

        public void Close()
        {
            if (CaptureInstance != System.IntPtr.Zero) CaptureDeleteInstance(CaptureInstance);
            CaptureInstance = System.IntPtr.Zero;
        }

        public ECaptureSendResult SendTexture(Texture Source, int Timeout = 1000, bool DoubleBuffering = false, EResizeMode ResizeMode = EResizeMode.Disabled, EMirrorMode MirrorMode = EMirrorMode.Disabled)
        {
            if (CaptureInstance == System.IntPtr.Zero) return ECaptureSendResult.ERROR_INVALIDCAPTUREINSTANCEPTR;
            return CaptureSendTexture(CaptureInstance, Source.GetNativeTexturePtr(), Timeout, DoubleBuffering, ResizeMode, MirrorMode, QualitySettings.activeColorSpace == ColorSpace.Linear);
        }
    }
}



