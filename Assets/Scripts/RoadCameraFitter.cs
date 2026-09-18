using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class RoadCameraFitter : MonoBehaviour
{
    private Camera targetCamera;

    private void Start()
    {
        targetCamera = Camera.main;
        FitQuadToScreen();
    }

    public void FitQuadToScreen()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;

        float distance = Mathf.Abs(targetCamera.transform.position.y - transform.position.y);
        float frustumHeight = 2.0f * distance * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * targetCamera.aspect;

        transform.localScale = new Vector3(frustumWidth * 1.2f, frustumHeight * 3.0f, 1.0f);
    }
}