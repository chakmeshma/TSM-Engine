using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 1000.0F;
    public float zoomMultiplier = 2.0F;
    //public float zoomMin = 500.0F;
    //public float zoomMax = 10000.0F;
    public float panSpeed = 10.0F;
    public float panMultiplier = 2.0F;
    public AnimationCurve zoomCurve;
    public Canvas referenceCanvas;

    private UnityEngine.UI.CanvasScaler canvasScaler;
    private RectTransform referencePanel;
    private float zoomFactor = 0.2F;
    private float calculatedZoomFactor;

    void Awake()
    {
        canvasScaler = referenceCanvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        referencePanel = referenceCanvas.GetComponent<RectTransform>().Find("Reference Panel").GetComponent<RectTransform>();
    }

	void Update ()
    {
        float calculatedZoomSpeed = zoomSpeed;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            calculatedZoomSpeed *= zoomMultiplier;

        zoomFactor = Mathf.Clamp(zoomFactor - Input.GetAxis("Mouse ScrollWheel") * calculatedZoomSpeed, 0.0F, 1.0F);
        calculatedZoomFactor = zoomCurve.Evaluate(zoomFactor);

        canvasScaler.matchWidthOrHeight = calculatedZoomFactor;

        //referenceCanvas.GetComponent
        //thisCamera.orthographicSize = zoomMin + ((zoomMax - zoomMin) * );

        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            //lastCursorPosition = Input.mouse
        }

        if (Input.GetKeyUp(KeyCode.Mouse2))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (Input.GetKey(KeyCode.Mouse2))
        {
            float calculatedPanSpeed = panSpeed;

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                calculatedPanSpeed *= panMultiplier;

            referencePanel.anchoredPosition += new Vector2(Input.GetAxis("Mouse X") * calculatedPanSpeed, Input.GetAxis("Mouse Y") * calculatedPanSpeed);
            //thisCamera.transform.localPosition = new Vector3(thisCamera.transform.localPosition.x - , thisCamera.transform.localPosition.y - , thisCamera.transform.localPosition.z);
        }
	}
}
