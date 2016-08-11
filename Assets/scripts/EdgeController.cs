using UnityEngine;
using System.Collections;

public class EdgeController : MonoBehaviour
{
    private RectTransform _rectTransform = null;
    private EdgeState _edgeState = EdgeState.Unvalid;
    private UnityEngine.UI.Image[] childImages;

    public enum EdgeState { Temporary, Potential, Stable, Unvalid }
    public RectTransform rectTransform
    {
        get
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            return _rectTransform;
        }
    }
    public EdgeState edgeState
    {
        get
        {
            return _edgeState;
        }
        set
        {
            if(_edgeState != value)
            {
                _edgeState = value;

                switch (value)
                {
                    case EdgeState.Temporary:
                        foreach (UnityEngine.UI.Image image in childImages)
                        {
                            image.color = temporaryColor;
                            image.raycastTarget = false;
                        }
                        break;
                    case EdgeState.Potential:
                        foreach (UnityEngine.UI.Image image in childImages)
                        {
                            image.color = potentialColor;
                            image.raycastTarget = false;
                        }
                        break;
                    case EdgeState.Stable:
                        foreach (UnityEngine.UI.Image image in childImages)
                        {
                            image.color = stableColor;
                            image.raycastTarget = true;
                        }
                        break;
                }
            }
        }
    }
    public Color temporaryColor;
    public Color potentialColor;
    public Color stableColor;

    void Awake()
    {
        childImages = gameObject.GetComponentsInChildren<UnityEngine.UI.Image>();
    }

    public void DeleteEdge()
    {
        Destroy(this.gameObject);
    }
}
