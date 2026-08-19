using UnityEngine;

public class Level1DustDevilSpin : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 90f;
    Transform visualRoot;

    public void SetVisualRoot(Transform root)
    {
        visualRoot = root;
    }

    void Update()
    {
        Transform target = visualRoot != null ? visualRoot : transform;
        target.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }
}
