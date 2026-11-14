using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Sets up the two-player split screen: Chuck's camera fills the top half and
/// Barbara's fills the bottom half. Each camera follows its own character, and both
/// cameras render everything, so each player can see the other on their screen.
/// </summary>
public class SplitScreenController : MonoBehaviour
{
    // This is who the top camera follows, Chuck
    public Transform chuckTarget;
    // This is who the bottom camera follows, BARBARA
    public Transform barbaraTarget;    
    //This is the top camera for Chuck
    public Camera chuckCam;
    // This is the bottom camera for BARBARA
    public Camera barbaraCam;
    // This is how much of the world each camera shows
    public float orthoSize = 8f;
    // camera depth; cameras sit in front of the 2D scene
    public float cameraZ = -10f;        

    void Start()
    {
        // Finds each of the different components, TopCamera, BottomCamera, Chuck, and BARBARA
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

        // This splits the screen: Rect is (x, y, width, height) as fractions of the screen
        //This is for the top half of the screen
        chuckCam.rect = new Rect(0f, 0.5f, 1f, 0.5f);
        //This is for the bottom half of the screen
        barbaraCam.rect = new Rect(0f, 0f, 1f, 0.5f);     

        chuckCam.orthographic = true;
        barbaraCam.orthographic = true;
        chuckCam.orthographicSize = orthoSize;
        barbaraCam.orthographicSize = orthoSize;

        // Both cameras are "Base" cameras so each draws its own half independently
        chuckCam.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Base;
        barbaraCam.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Base;

        // ~0 means "every layer", so each camera can see both characters on the screen at once.
        chuckCam.cullingMask = ~0;
        barbaraCam.cullingMask = ~0;

        // This turns off any other camera (such as the Main Camera) so it doesn't draw over the split screen.
        foreach (Camera cam in FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if (cam != chuckCam && cam != barbaraCam)
                cam.gameObject.SetActive(false);
        }
    }

    // This runs after everything has moved, so the cameras follow smoothly without jitter
    void LateUpdate()
    {
        if (chuckTarget != null && chuckCam != null)
            chuckCam.transform.position = new Vector3(chuckTarget.position.x, chuckTarget.position.y, cameraZ);

        if (barbaraTarget != null && barbaraCam != null)
            barbaraCam.transform.position = new Vector3(barbaraTarget.position.x, barbaraTarget.position.y, cameraZ);
    }
}