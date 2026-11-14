using UnityEngine;
/// <summary>
/// This SimpleFollow script allows the camera to follow whichever character the target transform is on throughout the scene.
/// It's either BARBARA or Chuck in this case.
/// </summary>
public class SimpleFollow : MonoBehaviour
{
    //Instantiates the Transform target
    public Transform target;
    public float cameraZ = -10f;

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = new Vector3(target.position.x, target.position.y, cameraZ);
    }
}
