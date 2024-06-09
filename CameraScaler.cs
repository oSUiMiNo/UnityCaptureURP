using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    public Vector3 scale;
    private Camera camera;

    void Start()
    {
        Debug.Log(0);
        camera = GetComponent<Camera>();
    }

    void OnPreCull()
    {
        Debug.Log(1);
        camera.ResetWorldToCameraMatrix();
        camera.ResetProjectionMatrix();
        camera.projectionMatrix = camera.projectionMatrix * Matrix4x4.Scale(scale);
    }

    void OnPreRender()
    {
        Debug.Log(2);
        if (scale.x * scale.y * scale.z < 0)
        {
            GL.invertCulling = true;
        }
    }

    void OnPostRender()
    {
        Debug.Log(3);
        GL.invertCulling = false;
    }
}