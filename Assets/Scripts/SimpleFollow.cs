using UnityEngine;

public class SimpleFollow : MonoBehaviour
{
    public Transform target;
    public float cameraZ = -10f;

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = new Vector3(target.position.x, target.position.y, cameraZ);
    }
}
