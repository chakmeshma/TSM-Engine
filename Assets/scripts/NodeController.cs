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
    private UnityEngine.UI.Text debugText;
    private UnityEngine.UI.Image imageUIComponent;
    private Color nodeDefaultBackgroundColor;
    private bool _nodeTextMouseOver = false;
    private bool _nodeMouseOver = false;
    private bool _nodeClicked = false;
    private NodePointerState lastPointerState = NodePointerState.Unvalid;
    //private PointerLocation lastPointerLocation = PointerLocation.OffBorder;
    private RectTransform _rectTransform = null;
    private Camera nodesCamera;
    private UnityEngine.UI.InputField inputField;
    private RectTransform inputFieldRectTransform;
    private Vector3 resizingMovingLastMousePosition;
    private NodePointerState resizeStartPointerState;
    private Canvas parentCanvas;
    private float lastCanvasScaleFactor;
    private float lastLeftClickTime = Mathf.NegativeInfinity;
    private List<EdgeController> _inwardEdges = null;
    private List<EdgeController> _outwardEdges = null;
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
                    {
                        bool alreadyConnected = false;

                        foreach (EdgeController edge in connectModeFromNode.outwardEdges)
                        {
                            if(edge.toNode == this)
                            {
                                alreadyConnected = true;
                                break;
                            }
                        }

                        if(!alreadyConnected)
                            potentialConnectModeToNode = this;
                    }
                }
                else
                {
                    if (connectMode && potentialConnectModeToNode == this)
                        potentialConnectModeToNode = null;

                    //if (!connectMode)
                    //    ignoreNextPotentialAddEdgeClick = false;
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
                            case NodePointerState.Move:
                                resizeMoveMode = ResizeMoveMode.Moving;
                                resizingMovingNode = this;
                                resizingMovingLastMousePosition = Input.mousePosition;

                                MakeTopMost();
                                break;
                            case NodePointerState.Off:
                            case NodePointerState.Unvalid:
                                break;
                            default:
                                resizeMoveMode = ResizeMoveMode.Resizing;
                                resizingMovingNode = this;
                                resizingMovingLastMousePosition = Input.mousePosition;
                                resizeStartPointerState = pointerState;

                                MakeTopMost();
                                break;
                        }
                    }
                    else
                    {
                        resizeMoveMode = ResizeMoveMode.No;
                        UnvalidateLastPointerState();
                    }
                }
                else
                {
                    if (potentialConnectModeToNode == this)
                    {
                        NodesParentController.instance.ConfirmEdge();
                        ignoreNextPotentialAddEdgeClick = true;

                        StopCoroutine(IgnoreNextPotentialAddEdgeClickClear());
                        StartCoroutine(IgnoreNextPotentialAddEdgeClickClear());
                    }
                    else
                    {
                        NodesParentController.instance.CancelEdge();
                        ignoreNextPotentialAddEdgeClick = true;

                        StopCoroutine(IgnoreNextPotentialAddEdgeClickClear());
                        StartCoroutine(IgnoreNextPotentialAddEdgeClickClear());
                    }
                }
            }
        }
    }
    public enum NodePointerLocation { OnBorder, OffBorder }
    public enum NodePointerState { Off = -1, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left, Move, Unvalid }
    public enum ResizeMoveMode { No, Moving, Resizing }
    public NodePointerLocation pointerLocation
    {
        get
        {
            if (nodeMouseOver && !nodeTextMouseOver)
                return NodePointerLocation.OnBorder;
            else
                return NodePointerLocation.OffBorder;
        }
    }
    public NodePointerState pointerState
    {
        get
        {
            Vector2 localCursor;
            float cornerThreshold = borderWidth / parentCanvas.scaleFactor;
            float edgeThreshold = borderWidth / parentCanvas.scaleFactor;

            if ((pointerLocation == NodePointerLocation.OnBorder || nodeTextMouseOver) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                return NodePointerState.Move;

            if (pointerLocation == NodePointerLocation.OffBorder)
                return NodePointerState.Off;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, nodesCamera, out localCursor))
                return NodePointerState.Off;

            Vector2 topLeftCorner = rectTransform.sizeDelta / 2.0F;
            topLeftCorner.x = -topLeftCorner.x;
            Vector2 topRightCorner = rectTransform.sizeDelta / 2.0F;
            Vector2 bottomLeftCorner = rectTransform.sizeDelta / 2.0F;
            bottomLeftCorner.x = -bottomLeftCorner.x;
            bottomLeftCorner.y = -bottomLeftCorner.y;
            Vector2 bottomRightCorner = rectTransform.sizeDelta / 2.0F;
            bottomRightCorner.y = -bottomRightCorner.y;

            if ((localCursor - topLeftCorner).magnitude <= cornerThreshold)
                return NodePointerState.TopLeft;
            else if ((localCursor - topRightCorner).magnitude <= cornerThreshold)
                return NodePointerState.TopRight;
            else if ((localCursor - bottomLeftCorner).magnitude <= cornerThreshold)
                return NodePointerState.BottomLeft;
            else if ((localCursor - bottomRightCorner).magnitude <= cornerThreshold)
                return NodePointerState.BottomRight;
            else if (Mathf.Abs(localCursor.x - topRightCorner.x) <= edgeThreshold)
                return NodePointerState.Right;
            else if (Mathf.Abs(localCursor.x - topLeftCorner.x) <= edgeThreshold)
                return NodePointerState.Left;
            else if (Mathf.Abs(localCursor.y - topRightCorner.y) <= edgeThreshold)
                return NodePointerState.Top;
            else if (Mathf.Abs(localCursor.y - bottomRightCorner.y) <= edgeThreshold)
                return NodePointerState.Bottom;

            return NodePointerState.Off;
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
    public List<EdgeController> inwardEdges
    {
        get
        {
            if (_inwardEdges == null)
                _inwardEdges = new List<EdgeController>();

            return _inwardEdges;
        }
    }
    public List<EdgeController> outwardEdges
    {
        get
        {
            if (_outwardEdges == null)
                _outwardEdges = new List<EdgeController>();

            return _outwardEdges;
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

    IEnumerator IgnoreNextPotentialAddEdgeClickClear()
    {
        yield return new WaitForSecondsRealtime(0.1F);
        //yield return new WaitForEndOfFrame();

        ignoreNextPotentialAddEdgeClick = false;
    }

    void MakeTopMost()
    {
        rectTransform.SetAsLastSibling();

        foreach (EdgeController edge in outwardEdges)
            edge.rectTransform.SetAsLastSibling();

        foreach (EdgeController edge in inwardEdges)
            edge.rectTransform.SetAsLastSibling();
    }

    public void UnvalidateLastPointerState(bool unvalidateConnectedEdges = true)
    {
        lastPointerState = NodePointerState.Unvalid;

        if (unvalidateConnectedEdges)
        {
            foreach (EdgeController edge in outwardEdges)
                edge.UnvalidateLastPointerState(false);

            foreach (EdgeController edge in inwardEdges)
                edge.UnvalidateLastPointerState(false);
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
        debugText = GameObject.Find("debug").GetComponent<UnityEngine.UI.Text>();
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
        //debugText.text = "connectMode: " + connectMode.ToString() + "    resizingMovingMode: " + resizeMoveMode.ToString();

        //print(nodeID + ": pointerState: " + pointerState.ToString());
        //print("nodeMouseOver: " + nodeMouseOver.ToString() + " nodeTextMouseOver: " + nodeTextMouseOver.ToString());
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
            if ((resizeMoveMode == ResizeMoveMode.No && !connectMode))
            {
                switch (pointerState)
                {
                    case NodePointerState.Move:
                        CursorController.instance.SetCursor(CursorController.CursorType.Move);
                        break;
                    case NodePointerState.TopLeft:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeBackslash);
                        break;
                    case NodePointerState.Top:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeVertical);
                        break;
                    case NodePointerState.TopRight:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeSlash);
                        break;
                    case NodePointerState.Right:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeHorizontal);
                        break;
                    case NodePointerState.BottomRight:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeBackslash);
                        break;
                    case NodePointerState.Bottom:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeVertical);
                        break;
                    case NodePointerState.BottomLeft:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeSlash);
                        break;
                    case NodePointerState.Left:
                        CursorController.instance.SetCursor(CursorController.CursorType.ResizeHorizontal);
                        break;
                    case NodePointerState.Off:
                        CursorController.instance.ResetCursor();
                        break;
                }

                switch (pointerState)
                {
                    case NodePointerState.Off:
                        imageUIComponent.color = nodeDefaultBackgroundColor;
                        break;
                    case NodePointerState.Move:
                        if (pointerLocation == NodePointerLocation.OnBorder)
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

        if (connectMode && (connectModeFromNode == this || potentialConnectModeToNode == this))
        {
            inputField.interactable = false;

            if (eventSystem.currentSelectedGameObject)
                eventSystem.SetSelectedGameObject(null);

            imageUIComponent.color = backgroundMovingColor;
        }
        else if (resizeMoveMode == ResizeMoveMode.Moving && resizingMovingNode == this)
        {
            inputField.interactable = false;

            if (eventSystem.currentSelectedGameObject)
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

            UpdateConnectedEdgesGeometry();
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
                    case NodePointerState.TopRight:
                        scaleVector = new Vector2(localCurrentCursor.x - localLastCursor.x, localCurrentCursor.y - localLastCursor.y);
                        break;
                    case NodePointerState.BottomRight:
                        scaleVector = new Vector2(localCurrentCursor.x - localLastCursor.x, -(localCurrentCursor.y - localLastCursor.y));
                        break;
                    case NodePointerState.BottomLeft:
                        scaleVector = new Vector2(-(localCurrentCursor.x - localLastCursor.x), -(localCurrentCursor.y - localLastCursor.y));
                        break;
                    case NodePointerState.TopLeft:
                        scaleVector = new Vector2(-(localCurrentCursor.x - localLastCursor.x), localCurrentCursor.y - localLastCursor.y);
                        break;
                    case NodePointerState.Top:
                        scaleVector = new Vector2(0.0F, localCurrentCursor.y - localLastCursor.y);
                        break;
                    case NodePointerState.Right:
                        scaleVector = new Vector2(localCurrentCursor.x - localLastCursor.x, 0.0F);
                        break;
                    case NodePointerState.Bottom:
                        scaleVector = new Vector2(0.0F, -(localCurrentCursor.y - localLastCursor.y));
                        break;
                    case NodePointerState.Left:
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
                    case NodePointerState.TopRight:
                    case NodePointerState.Top:
                    case NodePointerState.Right:
                        translateVector = scaleVector / 2.0F;
                        break;
                    case NodePointerState.BottomRight:
                    case NodePointerState.Bottom:
                        translateVector = scaleVector / 2.0F;
                        translateVector.y = -translateVector.y;
                        break;
                    case NodePointerState.BottomLeft:
                        translateVector = scaleVector / 2.0F;
                        translateVector = -translateVector;
                        break;
                    case NodePointerState.TopLeft:
                    case NodePointerState.Left:
                        translateVector = scaleVector / 2.0F;
                        translateVector.x = -translateVector.x;
                        break;
                }

                rectTransform.sizeDelta += scaleVector;
                rectTransform.anchoredPosition += translateVector;

                resizingMovingLastMousePosition += new Vector3((float)(((double)(Input.mousePosition.x - resizingMovingLastMousePosition.x)) * cleanToDirtyRatioScaleX),
                                                               (float)(((double)(Input.mousePosition.y - resizingMovingLastMousePosition.y)) * cleanToDirtyRatioScaleY),
                                                               (Input.mousePosition.z - resizingMovingLastMousePosition.z));
            }

            UpdateConnectedEdgesGeometry();

            //rectTransform.pivot = originalPivot;
        }
        else if(!connectMode && resizeMoveMode == ResizeMoveMode.No && (IsSelectedConnectedEdgeThroughPointerState() || IsSelectedConnectedEdgeThroughResizingMoving()))
        {
            //if (nodeMouseOver || nodeTextMouseOver)
            //    return;
                        
            //if(eventSystem.currentSelectedGameObject)
            {
                if (inputField.interactable == false)
                    inputField.interactable = true;

                //if (eventSystem.currentSelectedGameObject)
                //    eventSystem.SetSelectedGameObject(null);

                imageUIComponent.color = backgroundSelectedColor;
            }
        }
        else
        {
            if (connectMode)
            {
                if (inputField.interactable == true)
                    inputField.interactable = false;

                imageUIComponent.color = nodeDefaultBackgroundColor;
            }
            else
            {
                if (inputField.interactable == false)
                    inputField.interactable = true;
            }
        }

        //debugText.text += " " + inputField.interactable.ToString();
    }

    bool IsSelectedConnectedEdgeThroughResizingMoving()
    {
        foreach (EdgeController edge in inwardEdges)
        {
            if (edge.IsResizingMovingConnectedNode())
                return true;
        }

        foreach (EdgeController edge in outwardEdges)
        {
            if (edge.IsResizingMovingConnectedNode())
                return true;
        }

        return false;
    }

    bool IsSelectedConnectedEdgeThroughPointerState()
    {
        foreach(EdgeController edge in inwardEdges)
        {
            if ((edge.edgeState == EdgeController.EdgeState.Stable && edge.pointerState == EdgeController.EdgePointerState.On))
                return true;
        }

        foreach (EdgeController edge in outwardEdges)
        {
            if ((edge.edgeState == EdgeController.EdgeState.Stable && edge.pointerState == EdgeController.EdgePointerState.On))
                return true;
        }

        return false;
    }

    void UpdateConnectedEdgesGeometry()
    {
        foreach (EdgeController edge in outwardEdges)
            NodesParentController.instance.UpdateStableEdgeGeometry(edge);

        foreach (EdgeController edge in inwardEdges)
            NodesParentController.instance.UpdateStableEdgeGeometry(edge);
    }

    void DeleteNode()
    {
        resizeMoveMode = ResizeMoveMode.No;
        CursorController.instance.ResetCursor();
        lastPointerState = NodePointerState.Unvalid;
        PreviewController.instance.previewEnabled = false;
        PreviewController.instance.previewNode = null;

        DataController.instance.UnregisterNode(nodeID);

        EdgeController[] connectedEdgesArray = new EdgeController[inwardEdges.Count + outwardEdges.Count];

        int i = 0;

        foreach(EdgeController edge in inwardEdges)
        {
            connectedEdgesArray[i] = edge;
            i++;
        }

        foreach(EdgeController edge in outwardEdges)
        {
            connectedEdgesArray[i] = edge;
            i++;
        }

        if (i != connectedEdgesArray.Length)
            throw new UnityException("DeleteNode: connected edges count mismatch");

        for (i = 0; i < connectedEdgesArray.Length; i++)
            connectedEdgesArray[i].AnnihilateEdge();

        Destroy(this.gameObject);
    }

    public void InitConnectMode()
    {
        CursorController.instance.ResetCursor();
        connectModeFromNode = this;
        connectMode = true;
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
                        InitConnectMode();

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
