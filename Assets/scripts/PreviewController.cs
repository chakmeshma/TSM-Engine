using UnityEngine;
using System.Collections;

public class PreviewController : MonoBehaviour
{
    static private PreviewController _instance;
    static public PreviewController instance
    {
        get
        {
            return _instance;
        }
    }

    private bool _previewEnabled = false;
    private Color originalColor;

    public bool previewEnabled
    {
        get
        {
            return _previewEnabled;
        }
        set
        {
            if (_previewEnabled != value)
            {
                _previewEnabled = value;

                previewBackground.enabled = value;
                previewText.enabled = value;
            }
        }
    }
    public UnityEngine.UI.Image previewBackground;
    public UnityEngine.UI.Text previewText;
    [HideInInspector]
    public NodeController previewNode;
    public Color emptyColor;

    void Awake()
    {
        _instance = this;
        originalColor = previewText.color;
    }

    void Update()
    {
        if (NodeController.resizeMoveMode != NodeController.ResizeMoveMode.No)
        {
            previewBackground.enabled = true;
            previewText.enabled = true;
            previewText.text = NodeController.resizingMovingNode.nodeText.Replace("\n", " ");

            if (previewText.text == "")
            {
                previewText.text = "[Empty]";
                previewText.color = emptyColor;
            }
            else
            {
                previewText.color = originalColor;
            }

            return;
        }

        if (previewNode)
            previewText.text = previewNode.nodeText.Replace("\n", " ");
        else
            previewText.text = "";

        if (previewText.text == "")
        {
            previewText.text = "[Empty]";
            previewText.color = emptyColor;
        }
        else
        {
            previewText.color = originalColor;
        }
    }

}
