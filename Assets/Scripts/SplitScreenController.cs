using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SplitScreenController : MonoBehaviour
{
    public Transform chuckTarget;
    public Transform barbaraTarget;
    public Camera chuckCam;
    public Camera barbaraCam;
    public float orthoSize = 8f;
    public float cameraZ = -10f;

    void Start()
    {
        if (chuckCam == null) chuckCam = GameObject.Find("TopCamera")?.GetComponent<Camera>();
        if (barbaraCam == null) barbaraCam = GameObject.Find("BottomCamera")?.GetComponent<Camera>();
        if (chuckTarget == null) chuckTarget = GameObject.Find("Chuck")?.transform;
        if (barbaraTarget == null) barbaraTarget = GameObject.Find("BARBARA")?.transform;

        if (chuckCam == null || barbaraCam == null)
        {
            Debug.LogError("Can't find cameras!");
            return;
        }

        chuckCam.gameObject.SetActive(true);
        barbaraCam.gameObject.SetActive(true);

        chuckCam.rect = new Rect(0f, 0.5f, 1f, 0.5f);   // top
        barbaraCam.rect = new Rect(0f, 0f, 1f, 0.5f);   // bottom

        chuckCam.orthographic = true;
        barbaraCam.orthographic = true;
        chuckCam.orthographicSize = orthoSize;
        barbaraCam.orthographicSize = orthoSize;

        chuckCam.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Base;
        barbaraCam.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Base;

        // Both cameras see everything, so each player appears on the other's screen
        chuckCam.cullingMask = ~0;
        barbaraCam.cullingMask = ~0;

        foreach (Camera cam in FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if (cam != chuckCam && cam != barbaraCam)
                cam.gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (chuckTarget != null && chuckCam != null)
            chuckCam.transform.position = new Vector3(chuckTarget.position.x, chuckTarget.position.y, cameraZ);

        if (barbaraTarget != null && barbaraCam != null)
            barbaraCam.transform.position = new Vector3(barbaraTarget.position.x, barbaraTarget.position.y, cameraZ);
    }
}