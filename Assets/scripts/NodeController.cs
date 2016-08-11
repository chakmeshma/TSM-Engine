using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeController : MonoBehaviour
{
    static public ResizeMoveMode resizeMoveMode = ResizeMoveMode.No;
    static public NodeController resizingMovingNode = null;
    static public NodeController connectModeFromNode = null;
    static public bool connectMode = false;
    static public NodeController potentialConnectModeToNode = null;
    //static public bool clearIgnoreFlag = false;

    private int _nodeID = 0;
    private UnityEngine.EventSystems.EventSystem eventSystem;
    //private UnityEngine.UI.Text debugText;
    private UnityEngine.UI.Image imageUIComponent;
    private Color nodeDefaultBackgroundColor;
    private bool _nodeTextMouseOver = false;
    private bool _nodeMouseOver = false;
    private bool _nodeClicked = false;
    private PointerState lastPointerState = PointerState.Off;
    //private PointerLocation lastPointerLocation = PointerLocation.OffBorder;
    private RectTransform _rectTransform = null;
    private Camera nodesCamera;
    private UnityEngine.UI.InputField inputField;
    private RectTransform inputFieldRectTransform;
    private Vector3 resizingMovingLastMousePosition;
    private PointerState resizeStartPointerState;
    private Canvas parentCanvas;
    private float lastCanvasScaleFactor;
    private float lastLeftClickTime = Mathf.NegativeInfinity;
    private Dictionary<int, NodeController> _toNodes = null;
    private Dictionary<int, NodeController> _fromNodes = null;
    private bool ignoreNextPotentialAddEdgeClick = false;

    public int nodeID
    {
        set
        {
            if (_nodeID == 0)
                _nodeID = value;
            else
                throw new UnityEngine.UnityException("NodeID already initialized");
        }

        get
        {
            return _nodeID;
        }
    }
    public float borderWidth = 10.0F;
    public bool nodeTextMouseOver
    {
        get
        {
            return _nodeTextMouseOver;
        }

        set
        {
            _nodeTextMouseOver = value;
        }
    }
    public bool nodeMouseOver
    {
        get
        {
            return _nodeMouseOver;
        }

        set
        {
            if (_nodeMouseOver != value)
            {
                _nodeMouseOver = value;

                if (value)
                    PreviewController.instance.previewNode = this;

                PreviewController.instance.previewEnabled = value;

                if (value)
                {
                    if (connectMode && connectModeFromNode != this && connectModeFromNode != null)
                        potentialConnectModeToNode = this;
                }
                else
                {
                    if (connectMode && potentialConnectModeToNode == this)
                        potentialConnectModeToNode = null;

                    if (!connectMode)
                        ignoreNextPotentialAddEdgeClick = false;
                }
            }
        }
    }
    public bool nodeClicked
    {
        get
        {
            return _nodeClicked;
        }
        set
        {
            if (_nodeClicked != value)
            {
                _nodeClicked = value;

                if (!connectMode)
                {
                    if (value)
                    {
                        switch (pointerState)
                        {
                            case PointerState.Move:
                                resizeMoveMode = ResizeMoveMode.Moving;
                                resizingMovingNode = this;
                                resizingMovingLastMousePosition = Input.mousePosition;

                                rectTransform.SetAsLastSibling();
                                break;
                            case PointerState.Off:
                            case PointerState.Unvalid:
                                break;
                            default:
                                resizeMoveMode = ResizeMoveMode.Resizing;
                                resizingMovingNode = this;
                                resizingMovingLastMousePosition = Input.mousePosition;
                                resizeStartPointerState = pointerState;

                                rectTransform.SetAsLastSibling();
                                break;
                        }
                    }
                    else
                    {
                        resizeMoveMode = ResizeMoveMode.No;
                        lastPointerState = PointerState.Unvalid;
                    }
                }
                else
                {
                    if (potentialConnectModeToNode == this)
                    {
                        NodesParentController.instance.CancelEdge();
                        ignoreNextPotentialAddEdgeClick = true;
                    }
                    else
                    {
                        NodesParentController.instance.CancelEdge();
                        ignoreNextPotentialAddEdgeClick = true;
                    }
                }
            }
        }
    }
    public enum PointerLocation { OnBorder, OffBorder }
    public enum PointerState { Off = -1, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left, Move, Unvalid}
    public enum ResizeMoveMode { No, Moving, Resizing }
    public PointerLocation pointerLocation
    {
        get
        {
            if (nodeMouseOver && !nodeTextMouseOver)
                return PointerLocation.OnBorder;
            else
                return PointerLocation.OffBorder;
        }
    }
    public PointerState pointerState
    {
        get
        {
            Vector2 localCursor;
            float cornerThreshold = borderWidth / parentCanvas.scaleFactor;
            float edgeThreshold = borderWidth / parentCanvas.scaleFactor;

            if ((pointerLocation == PointerLocation.OnBorder || nodeTextMouseOver) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                return PointerState.Move;

            if (pointerLocation == PointerLocation.OffBorder)
                return PointerState.Off;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, nodesCamera, out localCursor))
                return PointerState.Off;

            Vector2 topLeftCorner = rectTransform.sizeDelta / 2.0F;
            topLeftCorner.x = -topLeftCorner.x;
            Vector2 topRightCorner = rectTransform.sizeDelta / 2.0F;
            Vector2 bottomLeftCorner = rectTransform.sizeDelta / 2.0F;
            bottomLeftCorner.x = -bottomLeftCorner.x;
            bottomLeftCorner.y = -bottomLeftCorner.y;
            Vector2 bottomRightCorner = rectTransform.sizeDelta / 2.0F;
            bottomRightCorner.y = -bottomRightCorner.y;

            if ((localCursor - topLeftCorner).magnitude <= cornerThreshold)
                return PointerState.TopLeft;
            else if ((localCursor - topRightCorner).magnitude <= cornerThreshold)
                return PointerState.TopRight;
            else if ((localCursor - bottomLeftCorner).magnitude <= cornerThreshold)
                return PointerState.BottomLeft;
            else if ((localCursor - bottomRightCorner).magnitude <= cornerThreshold)
                return PointerState.BottomRight;
            else if (Mathf.Abs(localCursor.x - topRightCorner.x) <= edgeThreshold)
                return PointerState.Right;
            else if (Mathf.Abs(localCursor.x - topLeftCorner.x) <= edgeThreshold)
                return PointerState.Left;
            else if (Mathf.Abs(localCursor.y - topRightCorner.y) <= edgeThreshold)
                return PointerState.Top;
            else if (Mathf.Abs(localCursor.y - bottomRightCorner.y) <= edgeThreshold)
                return PointerState.Bottom;

            return PointerState.Off;
        }
    }
    public Color backgroundSelectedColor;
    public Color backgroundMovingColor;
    public string nodeText
    {
        get
        {
            return inputField.text;
        }
        set
        {
            inputField.text = value;
        }
    }
    public Dictionary<int, NodeController> toNodes
    {
        get
        {
            if (_toNodes == null)
                _toNodes = new Dictionary<int, NodeController>();

            return _toNodes;
        }
    }
    public Dictionary<int, NodeController> fromNodes
    {
        get
        {
            if (_fromNodes == null)
                _fromNodes = new Dictionary<int, NodeController>();

            return _fromNodes;
        }
    }
    public RectTransform rectTransform
    {
        get
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            return _rectTransform;
        }
    }

    void Awake()
    {
        eventSystem = UnityEngine.EventSystems.EventSystem.current;
        parentCanvas = rectTransform.GetComponentInParent<Canvas>();
        inputField = GetComponentInChildren<UnityEngine.UI.InputField>();
        inputFieldRectTransform = inputField.GetComponent<RectTransform>();
        nodesCamera = Camera.main;
        imageUIComponent = GetComponent<UnityEngine.UI.Image>();
        nodeDefaultBackgroundColor = imageUIComponent.color;
        //debugText = GameObject.Find("debug").GetComponent<UnityEngine.UI.Text>();
    }

    void Start()
    {
        //CursorController.instance.ResetCursor();
        //lastPointerLocation = PointerLocation.OffBorder;
        inputFieldRectTransform.sizeDelta = new Vector2(-borderWidth, -borderWidth) / parentCanvas.scaleFactor;

        lastCanvasScaleFactor = parentCanvas.scaleFactor;
    }

    void Update()
    {
        //if (clearIgnoreFlag)
        //{
        //    ignoreNextPotentialAddEdgeClick = false;
        //    clearIgnoreFlag = false;
        //}
        //if (Input.GetMouseButtonDown(0))
        //{
        //    ignoreNextPotentialAddEdgeClick = false;
        //}

        //if (lastPointerLocation != pointerLocation)
        //{
        //    switch (pointerLocation)
        //    {
        //        case PointerLocation.OnBorder:
        //            imageUIComponent.color = backgroundSelectedColor;
        //            break;
        //        case PointerLocation.OffBorder:
        //            imageUIComponent.color = nodeDefaultBackgroundColor;
        //            break;
        //    }

        //    lastPointerLocation = pointerLocation;
        //}

        //if (nodeClicked)
        //    debugText.text = "clicked";
        //else
        //    debugText.text = "";
        if (lastCanvasScaleFactor != parentCanvas.scaleFactor)
        {
            inputFieldRectTransform.sizeDelta = new Vector2(-borderWidth, -borderWidth) / parentCanvas.scaleFactor;

            lastCanvasScaleFactor = parentCanvas.scaleFactor;
        }

        if (lastPointerState != pointerState)
        { 
            if (resizeMoveMode == ResizeMoveMode.No && !connectMode)
            {
                switch (pointerState)
                {
                    case PointerState.Move:
                        CursorController.instance.SetCursor(CursorController.CursorType.Move);
                        break;
                    case PointerState.TopLeft:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeBackslash);
                        break;
                    case PointerState.Top:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeVertical);
                        break;
                    case PointerState.TopRight:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeSlash);
                        break;
                    case PointerState.Right:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeHorizontal);
                        break;
                    case PointerState.BottomRight:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeBackslash);
                        break;
                    case PointerState.Bottom:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeVertical);
                        break;
                    case PointerState.BottomLeft:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeSlash);
                        break;
                    case PointerState.Left:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeHorizontal);
                        break;
                    case PointerState.Off:
                        CursorController.instance.ResetCursor();
                        break;
                }

                switch (pointerState)
                {
                    case PointerState.Off:
                        imageUIComponent.color = nodeDefaultBackgroundColor;
                        break;
                    case PointerState.Move:
                        if(pointerLocation == PointerLocation.OnBorder)
                            imageUIComponent.color = backgroundSelectedColor;
                        else
                            imageUIComponent.color = nodeDefaultBackgroundColor;
                        break;
                    default:
                        imageUIComponent.color = backgroundSelectedColor;
                        break;
                }
            }

            //switch (point erState)
            //{
            //    case PointerState.Move:
            //        inputField.interactable = false;
            //        break;
            //    default:
            //        inputField.interactable = true;
            //        break;
            //}

            lastPointerState = pointerState;
        }

        //debugText.text = (resizeMoveMode == ResizeMoveMode.Moving && resizingMovingNode == this).ToString();

        if(connectMode && (connectModeFromNode == this || potentialConnectModeToNode == this))
        {
            inputField.interactable = false;

            if (eventSystem.currentSelectedGameObject)
                eventSystem.SetSelectedGameObject(null);

            imageUIComponent.color = backgroundMovingColor;
        }
        else if(resizeMoveMode == ResizeMoveMode.Moving && resizingMovingNode == this)
        {
            inputField.interactable = false;

            if(eventSystem.currentSelectedGameObject)
                eventSystem.SetSelectedGameObject(null);

            imageUIComponent.color = backgroundMovingColor;

            Vector2 localCurrentCursor;
            Vector2 localLastCursor;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, nodesCamera, out localCurrentCursor) &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, resizingMovingLastMousePosition, nodesCamera, out localLastCursor))
            {
                rectTransform.anchoredPosition += new Vector2(localCurrentCursor.x - localLastCursor.x, localCurrentCursor.y - localLastCursor.y);

                resizingMovingLastMousePosition = Input.mousePosition;
            }
        }
        else if (resizeMoveMode == ResizeMoveMode.Resizing && resizingMovingNode == this)
        {
            inputField.interactable = false;

            if (eventSystem.currentSelectedGameObject)
                eventSystem.SetSelectedGameObject(null);

            imageUIComponent.color = backgroundMovingColor;

            Vector2 localCurrentCursor;
            Vector2 localLastCursor;

            //Vector2 originalPivot = rectTransform.pivot;

            //rectTransform.setd

            //rectTransform.pivot = new Vector2(0.0F, 0.0F);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, nodesCamera, out localCurrentCursor) &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, resizingMovingLastMousePosition, nodesCamera, out localLastCursor))
            {

                Vector2 scaleVector = Vector2.zero;
                Vector2 translateVector = Vector2.zero;
                Vector2 minSizeDelta = new Vector2(430.0F, 130.0F);
                Vector2 maxSizeDelta = new Vector2(3000.0F, 3000.0F);

                switch (resizeStartPointerState)
                {
                    case PointerState.TopRight:
                        scaleVector = new Vector2(localCurrentCursor.x - localLastCursor.x, localCurrentCursor.y - localLastCursor.y);
                        break;
                    case PointerState.BottomRight:
                        scaleVector = new Vector2(localCurrentCursor.x - localLastCursor.x, -(localCurrentCursor.y - localLastCursor.y));
                        break;
                    case PointerState.BottomLeft:
                        scaleVector = new Vector2(-(localCurrentCursor.x - localLastCursor.x), -(localCurrentCursor.y - localLastCursor.y));
                        break;
                    case PointerState.TopLeft:
                        scaleVector = new Vector2(-(localCurrentCursor.x - localLastCursor.x), localCurrentCursor.y - localLastCursor.y);
                        break;
                    case PointerState.Top:
                        scaleVector = new Vector2(0.0F, localCurrentCursor.y - localLastCursor.y);
                        break;
                    case PointerState.Right:
                        scaleVector = new Vector2(localCurrentCursor.x - localLastCursor.x, 0.0F);
                        break;
                    case PointerState.Bottom:
                        scaleVector = new Vector2(0.0F, -(localCurrentCursor.y - localLastCursor.y));
                        break;
                    case PointerState.Left:
                        scaleVector = new Vector2(-(localCurrentCursor.x - localLastCursor.x), 0.0F);
                        break;
                }

                Vector2 potentialNewSizeDelta = rectTransform.sizeDelta + scaleVector;
                Vector2 clampedNewSizeDelta = new Vector2(Mathf.Clamp(rectTransform.sizeDelta.x + scaleVector.x, minSizeDelta.x, maxSizeDelta.x),
                                                          Mathf.Clamp(rectTransform.sizeDelta.y + scaleVector.y, minSizeDelta.y, maxSizeDelta.y));

                Vector2 dirtyScaleVector = scaleVector;
                scaleVector += (clampedNewSizeDelta - potentialNewSizeDelta);

                double cleanToDirtyRatioScaleX = 1.0;
                double cleanToDirtyRatioScaleY = 1.0;

                if (dirtyScaleVector.x != 0.0F)
                    cleanToDirtyRatioScaleX = scaleVector.x / dirtyScaleVector.x;
                if (dirtyScaleVector.y != 0.0F)
                    cleanToDirtyRatioScaleY = scaleVector.y / dirtyScaleVector.y;


                switch (resizeStartPointerState)
                {
                    case PointerState.TopRight:
                    case PointerState.Top:
                    case PointerState.Right:
                        translateVector = scaleVector / 2.0F;
                        break;
                    case PointerState.BottomRight:
                    case PointerState.Bottom:
                        translateVector = scaleVector / 2.0F;
                        translateVector.y = -translateVector.y;
                        break;
                    case PointerState.BottomLeft:
                        translateVector = scaleVector / 2.0F;
                        translateVector = -translateVector;
                        break;
                    case PointerState.TopLeft:
                    case PointerState.Left:
                        translateVector = scaleVector / 2.0F;
                        translateVector.x = -translateVector.x;
                        break;
                }

                rectTransform.sizeDelta += scaleVector;
                rectTransform.anchoredPosition += translateVector;

                resizingMovingLastMousePosition += new Vector3((float)(((double)(Input.mousePosition.x - resizingMovingLastMousePosition.x)) * cleanToDirtyRatioScaleX) ,
                                                               (float)(((double)(Input.mousePosition.y - resizingMovingLastMousePosition.y)) * cleanToDirtyRatioScaleY) ,
                                                               (Input.mousePosition.z - resizingMovingLastMousePosition.z));
            }

            //rectTransform.pivot = originalPivot;
        }
        else
        {
            inputField.interactable = true;
        }

        //debugText.text += " " + inputField.interactable.ToString();
    }

    void DeleteNode()
    {
        resizeMoveMode = ResizeMoveMode.No;
        CursorController.instance.ResetCursor();
        lastPointerState = PointerState.Unvalid;
        PreviewController.instance.previewEnabled = false;
        PreviewController.instance.previewNode = null;

        DataController.instance.UnregisterNode(nodeID);

        Destroy(this.gameObject);
    }

    public void PotentialLeftClicked(UnityEngine.EventSystems.BaseEventData eventData)
    {
        UnityEngine.EventSystems.PointerEventData pointerEventData = eventData as UnityEngine.EventSystems.PointerEventData;

        if(pointerEventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Left)
        {
            if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if(resizeMoveMode == ResizeMoveMode.No && !connectMode)
                {
                    if (ignoreNextPotentialAddEdgeClick)
                    {
                        ignoreNextPotentialAddEdgeClick = false;
                    }
                    else
                    {
                        CursorController.instance.ResetCursor();
                        connectModeFromNode = this;
                        connectMode = true;

                        NodesParentController.instance.AddEdge(this);
                    }
                }
            }

            lastLeftClickTime = Mathf.NegativeInfinity;
        }
    }

    public void PotentialRightClicked(UnityEngine.EventSystems.BaseEventData eventData)
    {
        UnityEngine.EventSystems.PointerEventData pointerEventData = eventData as UnityEngine.EventSystems.PointerEventData;

        if (pointerEventData.button != UnityEngine.EventSystems.PointerEventData.InputButton.Right || connectMode)
        {
            lastLeftClickTime = Mathf.NegativeInfinity;

            return;
        }

        if (Time.realtimeSinceStartup - lastLeftClickTime <= 0.3F)
        {
            DeleteNode();

            lastLeftClickTime = Mathf.NegativeInfinity;
        }
        else
            lastLeftClickTime = Time.realtimeSinceStartup;
    }

    //public void ClearIgnoreFlag()
    //{
    //    StaticClearIgnoreFlag();
    //}

    //static public void StaticClearIgnoreFlag()
    //{
    //    if (ignoreNextPotentialAddEdgeClick)
    //        ignoreNextPotentialAddEdgeClick = false;
    //}

}
