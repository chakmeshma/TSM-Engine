using UnityEngine;
using System.Collections;

public class EdgeController : MonoBehaviour
{
    private RectTransform _rectTransform = null;
    private EdgeState _edgeState = EdgeState.Unvalid;
    private UnityEngine.UI.Image[] childImages;
    private NodeController _fromNode;
    private NodeController _toNode;
    private float lastCanvasScaleFactor;
    private Canvas parentCanvas;
    private RectTransform headRectTransform;
    private RectTransform tailRectTransform;
    private RectTransform tailAbdomenRectTransform;
    private RectTransform tailBackRectTransform;
    private double toNodeSizeFactor = 236.0;
    //private double lastToNodeSizeFactor = 0.0;
    private bool _headMouseOver;
    private bool _tailAbdomenMouseOver;
    private bool _tailBackMouseOver;
    private EdgePointerState lastPointerState = EdgePointerState.Unvalid;
    private bool _edgeClicked = false;

    [HideInInspector]
    public bool forceUpdateSizes = false;
    [HideInInspector]
    public Vector2 fromNodeAnchoredPosition = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
    [HideInInspector]
    public Vector2 toNodeAnchoredPosition = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
    public float edgeWidth = 10.0F;
    public float headSize = 100.0F;
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
                }
            }
        }
    }
    public Color temporaryColor;
    public Color potentialColor;
    public Color stableColor;
    public Color selectedColor;
    public NodeController fromNode
    {
        get
        {
            return _fromNode;
        }
    }
    public NodeController toNode
    {
        get
        {
            return _toNode;
        }
    }
    public bool tailAbdomenMouseOver
    {
        get
        {
            return _tailAbdomenMouseOver;
        }
        set
        {
            _tailAbdomenMouseOver = value;
        }
    }
    public bool tailBackMouseOver
    {
        get
        {
            return _tailBackMouseOver;
        }
        set
        {
            _tailBackMouseOver = value;
        }
    }
    public bool headMouseOver
    {
        get
        {
            return _headMouseOver;
        }
        set
        {
            _headMouseOver = value;
        }
    }
    public bool tailMouseOver
    {
        get
        {
            return (_tailAbdomenMouseOver || _tailBackMouseOver);
        }
    }
    public bool edgeMouseOver
    {
        get
        {
            if (edgeState == EdgeState.Stable)
                return (headMouseOver || tailMouseOver);
            else
                return false;
        }
    }
    public bool edgeOrConnectedNodesMouseOver
    {
        get
        {
            if(edgeState == EdgeState.Stable)
                return (headMouseOver || tailMouseOver || fromNode.nodeMouseOver || toNode.nodeMouseOver);
            else
                return false;
        }
    }
    public enum EdgePointerState { Off, On, Unvalid }
    public EdgePointerState pointerState
    {
        get
        {
            if (edgeOrConnectedNodesMouseOver)
                return EdgePointerState.On;
            else
                return EdgePointerState.Off;
        }
    }
    public bool edgeClicked
    {
        get
        {
            return _edgeClicked;
        }

        set
        {
            if(_edgeClicked != value)
            {
                _edgeClicked = value;

                if (value)
                {

                }
                else
                {

                }
            }
        }
    }

    void UpdateEdgeTailBackSize()
    {
        double tailBottomSize = 0.0F;
        double intersectionX = 0.0;
        double intersectionY = 0.0;

        NodeController edgeOrigin = null;
        NodeController edgeHead = null;
        Vector2 edgeOriginCoords = Vector2.zero;
        Vector2 edgeHeadCoords = Vector2.zero;

        if (fromNode)
        {
            edgeOrigin = fromNode;
            edgeOriginCoords = edgeOrigin.rectTransform.anchoredPosition;

            if (fromNodeAnchoredPosition.x != float.NegativeInfinity && fromNodeAnchoredPosition.y != float.NegativeInfinity)
                edgeOriginCoords = fromNodeAnchoredPosition;
        }
        else
        {
            edgeOrigin = NodeController.connectModeFromNode;
            edgeOriginCoords = edgeOrigin.rectTransform.anchoredPosition;
        }

        if (!edgeOrigin)
            throw new UnityException("UpdateEdgeSize: Couldn't determine the edge origin.");

        if (toNode)
        {
            edgeHead = toNode;
            edgeHeadCoords = edgeHead.rectTransform.anchoredPosition;

            if (toNodeAnchoredPosition.x != float.NegativeInfinity && toNodeAnchoredPosition.y != float.NegativeInfinity)
                edgeHeadCoords = toNodeAnchoredPosition;
        }
        else if (NodeController.potentialConnectModeToNode)
        {
            edgeHead = NodeController.potentialConnectModeToNode;
            edgeHeadCoords = edgeHead.rectTransform.anchoredPosition;
        }
        else
        {
            Vector2 localCursor = Vector2.zero;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(NodesParentController.instance.rectTransform, Input.mousePosition, NodesParentController.instance.nodesCamera, out localCursor))
                edgeHeadCoords = localCursor;
            else
                throw new UnityException("UpdateEdgeSize: Couldn't determine the edge head coordinates.");
        }

        Rect originNodeRect = new Rect(edgeOrigin.rectTransform.anchoredPosition - (edgeOrigin.rectTransform.sizeDelta / 2.0F), edgeOrigin.rectTransform.sizeDelta);

        if (Utilities.LineRectIntersection(edgeOriginCoords, edgeHeadCoords, originNodeRect, ref intersectionX, ref intersectionY))
        {
            tailBottomSize = System.Math.Sqrt(System.Math.Pow(intersectionX - edgeOriginCoords.x, 2.0) + System.Math.Pow(intersectionY - edgeOriginCoords.y, 2.0));// + (30.0 / parentCanvas.scaleFactor);
        }

        //print("edgeOriginCoords: " + edgeOriginCoords + " edgeHeadCoords: " + edgeHeadCoords + " tailBottomSize: " + tailBottomSize);

        if (((float)tailBottomSize) > tailRectTransform.rect.width || ((float)tailBottomSize) <= 0.0F)
        {
            tailBottomSize = tailRectTransform.rect.width;
        }

        tailAbdomenRectTransform.offsetMin = new Vector2((float)tailBottomSize, tailAbdomenRectTransform.offsetMin.y);
        tailBackRectTransform.sizeDelta = new Vector2((float)tailBottomSize, tailBackRectTransform.sizeDelta.y);
    }

    void UpdateEdgeSizes()
    {
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, edgeWidth / parentCanvas.scaleFactor);

        float finalHeadSize = headSize;

        if (toNodeSizeFactor != 0.0)
        {
            //float dynamicHeadSize = (float)(System.Math.Pow(toNodeSizeFactor * 0.1F, 1.0 / 1.0) + headSize - headSize);
            double nodeRelativeHeadSize = (toNodeSizeFactor * parentCanvas.scaleFactor) / 2.0;
            float edgeRelativeHeadSize = (edgeWidth) * 2.0F;

            float minHeadSize = Mathf.Min((float)nodeRelativeHeadSize, edgeRelativeHeadSize);
            float maxHeadSize = Mathf.Max((float)nodeRelativeHeadSize, edgeRelativeHeadSize);

            finalHeadSize = Mathf.Clamp(headSize, minHeadSize, maxHeadSize);
        }

        headRectTransform.sizeDelta = new Vector2(finalHeadSize / parentCanvas.scaleFactor, finalHeadSize / parentCanvas.scaleFactor);
        tailRectTransform.offsetMax = new Vector2((-finalHeadSize + 2.0F) / parentCanvas.scaleFactor, tailRectTransform.offsetMax.y);

        UpdateEdgeTailBackSize();
    }

    void Start()
    {
        UpdateEdgeSizes();

        lastCanvasScaleFactor = parentCanvas.scaleFactor;
    }

    public bool IsResizingMovingConnectedNode()
    {
        switch(NodeController.resizeMoveMode)
        {
            case NodeController.ResizeMoveMode.No:
                return false;
            case NodeController.ResizeMoveMode.Resizing:
            case NodeController.ResizeMoveMode.Moving:
                return (NodeController.resizingMovingNode == fromNode || NodeController.resizingMovingNode == toNode);
        }

        return false;
    }

    void Update()
    {
        //if (toNode != null)
        //{
        //    toNodeSizeFactor = System.Math.Sqrt(toNode.rectTransform.sizeDelta.x * toNode.rectTransform.sizeDelta.y);
        //}
        //else
        //{
        //    if (NodeController.connectMode && NodeController.potentialConnectModeToNode)
        //    {
        //        toNodeSizeFactor = System.Math.Sqrt(NodeController.potentialConnectModeToNode.rectTransform.sizeDelta.x * NodeController.potentialConnectModeToNode.rectTransform.sizeDelta.y);
        //    }
        //    else
        //        toNodeSizeFactor = 0.0;
        //}

        if (forceUpdateSizes || lastCanvasScaleFactor != parentCanvas.scaleFactor/* || lastToNodeSizeFactor != toNodeSizeFactor*/)
        {
            UpdateEdgeSizes();

            lastCanvasScaleFactor = parentCanvas.scaleFactor;

            if (forceUpdateSizes)
                forceUpdateSizes = false;
            //lastToNodeSizeFactor = toNodeSizeFactor;
        }
        else
        {
            UpdateEdgeTailBackSize();
        }

        if(lastPointerState != pointerState)
        {
            if(edgeState == EdgeState.Stable)
            {
                if (!NodeController.connectMode && NodeController.resizeMoveMode == NodeController.ResizeMoveMode.No)
                {
                    switch (pointerState)
                    {
                        case EdgePointerState.On:
                            foreach (UnityEngine.UI.Image image in childImages)
                                image.color = selectedColor;
                            break;
                        case EdgePointerState.Off:
                            foreach (UnityEngine.UI.Image image in childImages)
                                image.color = stableColor;

                            UnvalidateLastPointerState(true);
                            break;
                    }
                }
                else if(IsResizingMovingConnectedNode())
                {
                    foreach (UnityEngine.UI.Image image in childImages)
                        image.color = selectedColor;
                }
                else
                {
                    foreach (UnityEngine.UI.Image image in childImages)
                        image.color = stableColor;

                    UnvalidateLastPointerState(true);
                }
            }

            lastPointerState = pointerState;
        }
    }

    public void UnvalidateLastPointerState(bool unvalidateConnectedNodes = true)
    {
        if (edgeState != EdgeState.Stable)
            return;

        lastPointerState = EdgePointerState.Unvalid;

        if (unvalidateConnectedNodes)
        {
            fromNode.UnvalidateLastPointerState(false);
            toNode.UnvalidateLastPointerState(false);
        }
    }

    void Awake()
    {
        childImages = gameObject.GetComponentsInChildren<UnityEngine.UI.Image>();
        parentCanvas = rectTransform.GetComponentInParent<Canvas>();
        headRectTransform = rectTransform.Find("Head").GetComponent<RectTransform>();
        tailRectTransform = rectTransform.Find("Tail").GetComponent<RectTransform>();
        tailAbdomenRectTransform = tailRectTransform.Find("Abdomen").GetComponent<RectTransform>();
        tailBackRectTransform = tailRectTransform.Find("Back").GetComponent<RectTransform>();
    }

    public void DisconnectEdge()
    {
        edgeState = EdgeState.Unvalid;

        if (fromNode.outwardEdges.Contains(this))
            fromNode.outwardEdges.Remove(this);

        if (fromNode.inwardEdges.Contains(this))
            fromNode.inwardEdges.Remove(this);

        _fromNode = null;

        if (toNode.inwardEdges.Contains(this))
            toNode.inwardEdges.Remove(this);

        if (toNode.outwardEdges.Contains(this))
            toNode.outwardEdges.Remove(this);

        _toNode = null;
    }

    public void DeleteEdge()
    {
        Destroy(this.gameObject);
    }

    public void ConfirmEdge(NodeController fromNode, NodeController toNode)
    {
        _fromNode = fromNode;
        _toNode = toNode;

        _edgeState = EdgeState.Stable;

        foreach (UnityEngine.UI.Image image in childImages)
        {
            image.color = stableColor;
            image.raycastTarget = true;
        }
    }

    public void PotentialLeftClicked(UnityEngine.EventSystems.BaseEventData eventData)
    {
        UnityEngine.EventSystems.PointerEventData pointerEventData = eventData as UnityEngine.EventSystems.PointerEventData;

        if (pointerEventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Left)
        {
            if (edgeMouseOver && !NodeController.connectMode && NodeController.resizeMoveMode == NodeController.ResizeMoveMode.No)
            {
                NodeController lastFromNode = fromNode;
                //NodeController lastToNode = toNode;
                DisconnectEdge();
                lastFromNode.InitConnectMode();
                NodesParentController.instance.EditEdge(lastFromNode, this);

                //lastToNode.UnvalidateLastPointerState(false);
            }
        }
    }

    public void AnnihilateEdge()
    {
        NodeController lastFromNode = fromNode;
        NodeController lastToNode = toNode;
        DisconnectEdge();
        DeleteEdge();

        lastFromNode.UnvalidateLastPointerState(false);
        lastToNode.UnvalidateLastPointerState(false);
    }

    public void PotentialRightClicked(UnityEngine.EventSystems.BaseEventData eventData)
    {
        UnityEngine.EventSystems.PointerEventData pointerEventData = eventData as UnityEngine.EventSystems.PointerEventData;

        if (pointerEventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Right)
        {
            if (edgeMouseOver && !NodeController.connectMode && NodeController.resizeMoveMode == NodeController.ResizeMoveMode.No)
            {
                //NodeController lastFromNode = fromNode;
                //NodeController lastToNode = toNode;
                //DisconnectEdge();
                //DeleteEdge();

                //lastFromNode.UnvalidateLastPointerState(false);
                //lastToNode.UnvalidateLastPointerState(false);
                AnnihilateEdge();
            }
        }
    }
}
