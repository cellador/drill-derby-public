using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera _camera;
    
    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    /// <summary>
    /// Fits camera to show the complete map regardless of screen size.
    /// </summary>
    /// <param name="pos">Position the camera will be moved to.</param>
    /// <param name="worldSize">Size of the world to set the FOV accordingly.</param>
    public void FitCamera(Vector3 pos, Vector2 worldSize)
    {
        transform.position = pos;
        float screenAspectRatio = Screen.height / ((float)Screen.width);
        // If width-bound
        if(screenAspectRatio > worldSize.y / worldSize.x)
        {
            _camera.orthographicSize = 0.5f * worldSize.x * screenAspectRatio;
        }
        else
        {
            _camera.orthographicSize = 0.5f * worldSize.y;
        }
    }
}