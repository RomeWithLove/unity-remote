using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class RoadScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 0.5f;
    private Material targetMaterial;
    private Vector2 offset = Vector2.zero;

    private void Awake()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
        {
            targetMaterial = mr.material;
        }
    }

    private void Update()
    {
        if (targetMaterial == null) return;

        offset.y += scrollSpeed * Time.deltaTime;
        targetMaterial.mainTextureOffset = offset;
    }
}