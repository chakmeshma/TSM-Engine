using UnityEngine;
using System.Collections;

public class NodesParentController : MonoBehaviour
{
    static private NodesParentController _instance;
    static public NodesParentController instance
    {
        get
        {
            return _instance;
        }
    }

    //private UnityEngine.EventSystems.EventSystem eventSystem;
    private bool ignoreNextClick = false;
    private float lastClickTime = Mathf.NegativeInfinity;
    private RectTransform _rectTransform;
    private Camera _nodesCamera;
    private RectTransform edgesParent;
    private EdgeController lastEdgeController;
    private NodeController lastFromNodeController;
    private NodeController lastToNodeController;
    //private NodeController lastPotentialConnectModeToNode = null;

    public Camera nodesCamera
    {
        get
        {
            return _nodesCamera;
        }
    }
    public RectTransform rectTransform
    {
        get
        {
            return _rectTransform;
        }
    }
    public Object nodePrefab;
    public Object edgePrefab;

    void Awake()
    {
        //eventSystem = UnityEngine.EventSystems.EventSystem.current;
        _rectTransform = GetComponent<RectTransform>();
        edgesParent = _rectTransform.Find("Edges").GetComponent<RectTransform>();
        _nodesCamera = Camera.main;

        _instance = this;
    }

    void Update()
    {
        if (NodeController.connectMode)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                CancelEdge();

                return;
            }

            //if(lastPotentialConnectModeToNode != NodeController.potentialConnectModeToNode)
            //{
            //    if (NodeController.potentialConnectModeToNode != null)
            //        lastEdgeController.edgeState = EdgeController.EdgeState.Potential;
            //    else
            //        lastEdgeController.edgeState = EdgeController.EdgeState.Temporary;

            //    lastPotentialConnectModeToNode = NodeController.potentialConnectModeToNode;
            //}

            if (NodeController.potentialConnectModeToNode)
                lastToNodeController = NodeController.potentialConnectModeToNode;

            Vector2 localCursor, targetPoint = Vector2.zero;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, Input.mousePosition, _nodesCamera, out localCursor))
                return;

            if (NodeController.potentialConnectModeToNode != null)
                targetPoint = NodeController.potentialConnectModeToNode.rectTransform.anchoredPosition;
            else
                targetPoint = localCursor;

            Vector2 targetVector = new Vector3(targetPoint.x - lastFromNodeController.rectTransform.anchoredPosition.x, targetPoint.y - lastFromNodeController.rectTransform.anchoredPosition.y);

            float angle = (float)(System.Math.Atan2(targetVector.y, targetVector.x) / System.Math.PI * 180.0);

            if (angle > 360.0F) angle -= 360.0F;
            if (angle < 0.0F) angle += 360.0F;

            lastEdgeController.rectTransform.localRotation = Quaternion.Euler(0.0F, 0.0F, angle);
            lastEdgeController.rectTransform.sizeDelta = new Vector2(Mathf.Clamp(targetVector.magnitude, 0.3F, Mathf.Infinity), lastEdgeController.rectTransform.sizeDelta.y);

            if (NodeController.potentialConnectModeToNode != null)
                lastEdgeController.edgeState = EdgeController.EdgeState.Potential;
            else
                lastEdgeController.edgeState = EdgeController.EdgeState.Temporary;
        }
    }

    GameObject AddNode()
    {
        GameObject node = Instantiate(nodePrefab, transform, false) as GameObject;

        DataController.instance.RegisterNode(node.GetComponent<NodeController>());

        return node;
    }

    public void CancelEdge(bool delete = true)
    {
        NodeController.connectMode = false;
        if (lastEdgeController && delete)
            lastEdgeController.DeleteEdge();
        


        lastEdgeController = null;
        NodeController fromNodeController = lastFromNodeController;
        lastFromNodeController = null;
        NodeController.potentialConnectModeToNode = null;
        NodeController.connectModeFromNode = null;

        fromNodeController.UnvalidateLastPointerState();
        if (lastToNodeController)
            lastToNodeController.UnvalidateLastPointerState(false);
    }

    public void UpdateStableEdgeGeometry(EdgeController edge, int stackDepth = 0)
    {
        if (!edge)
            return;

        if (edge.edgeState != EdgeController.EdgeState.Stable)
            return;

        NodeController fromNode = edge.fromNode;
        NodeController toNode = edge.toNode;
        EdgeController partnerEdge = null;

        bool twoWay = false;

        foreach (EdgeController edge2 in toNode.outwardEdges)
        {
            if (edge2.toNode == fromNode)
            {
                partnerEdge = edge2;
                twoWay = true;
                break;
            }
        }

        Vector2 fromNodeAnchoredPosition = fromNode.rectTransform.anchoredPosition;
        Vector2 toNodeAnchoredPosition = toNode.rectTransform.anchoredPosition;
        Vector2 toNodeSizeDelta = toNode.rectTransform.sizeDelta;

        Vector2 reverseEdgeVector = fromNodeAnchoredPosition - toNodeAnchoredPosition;

        if (twoWay)
        {
            Vector2 twoWayEdgeEndsOffet = Quaternion.Euler(0.0F, 0.0F, 90.0F) * reverseEdgeVector;
            twoWayEdgeEndsOffet *= (float)(((double)edge.headSize) /  ((double)twoWayEdgeEndsOffet.magnitude));

            fromNodeAnchoredPosition += twoWayEdgeEndsOffet;
            toNodeAnchoredPosition += twoWayEdgeEndsOffet;
        }

        edge.fromNodeAnchoredPosition = fromNodeAnchoredPosition;
        edge.toNodeAnchoredPosition = toNodeAnchoredPosition;

        float angle = (float)(System.Math.Atan2(-reverseEdgeVector.y, -reverseEdgeVector.x) / System.Math.PI * 180.0);

        if (angle > 360.0F) angle -= 360.0F;
        if (angle < 0.0F) angle += 360.0F;

        double toNodeRectRadius = System.Math.Sqrt(System.Math.Pow(toNodeSizeDelta.x / 2.0, 2.0) + System.Math.Pow(toNodeSizeDelta.y / 2.0, 2.0));

        double vectorMultiplier = toNodeRectRadius * 2.0 / ((double)reverseEdgeVector.magnitude);
        reverseEdgeVector *= ((float)vectorMultiplier);

        double intersectionX = 0.0;
        double intersectionY = 0.0;

        Rect toNodeRect = new Rect(toNode.rectTransform.anchoredPosition - (toNodeSizeDelta / 2.0F), toNodeSizeDelta);

        if(Utilities.LineRectIntersection(fromNodeAnchoredPosition , toNodeAnchoredPosition, toNodeRect, ref intersectionX, ref intersectionY))
        {
            Vector2 finalEdgeVector = new Vector2(((float)intersectionX) - fromNodeAnchoredPosition.x, ((float)intersectionY) - fromNodeAnchoredPosition.y);

            edge.rectTransform.anchoredPosition = fromNodeAnchoredPosition;
            edge.rectTransform.localRotation = Quaternion.Euler(0.0F, 0.0F, angle);
            edge.rectTransform.sizeDelta = new Vector2(finalEdgeVector.magnitude, edge.rectTransform.sizeDelta.y);
        }
        else
        {
            Vector2 finalEdgeVector = new Vector2(toNodeAnchoredPosition.x - fromNodeAnchoredPosition.x,
                                                  toNodeAnchoredPosition.y - fromNodeAnchoredPosition.y);

            edge.rectTransform.anchoredPosition = fromNodeAnchoredPosition;
            edge.rectTransform.localRotation = Quaternion.Euler(0.0F, 0.0F, angle);
            edge.rectTransform.sizeDelta = new Vector2(finalEdgeVector.magnitude, edge.rectTransform.sizeDelta.y);
        }

        edge.forceUpdateSizes = true;

        StopAllCoroutines();

        if (stackDepth == 0)
        {
            StartCoroutine(UpdatePartnerEdge(partnerEdge, stackDepth + 1));
        }
    }

    IEnumerator UpdatePartnerEdge(EdgeController partnerEdge, int stackDepth)
    {
        yield return new WaitForEndOfFrame();

        UpdateStableEdgeGeometry(partnerEdge, stackDepth);
    }

    public bool ConfirmEdge()
    {
        NodeController toNode = NodeController.potentialConnectModeToNode;
        NodeController fromNode = lastFromNodeController;

        lastEdgeController.ConfirmEdge(fromNode, toNode);
        lastEdgeController.rectTransform.SetParent(edgesParent);

        UpdateStableEdgeGeometry(lastEdgeController);

        fromNode.outwardEdges.Add(lastEdgeController);
        toNode.inwardEdges.Add(lastEdgeController);

        CancelEdge(false);

        return true;
    }

    public GameObject EditEdge(NodeController fromNodeController, EdgeController edgeController)
    {
        GameObject edge = edgeController.gameObject;
        edgeController.rectTransform.SetParent(_rectTransform);
        edgeController.rectTransform.SetAsLastSibling();

        lastEdgeController = edgeController;
        lastFromNodeController = fromNodeController;

        lastEdgeController.rectTransform.anchoredPosition = lastFromNodeController.rectTransform.anchoredPosition;

        return edge;
    }

    public GameObject AddEdge(NodeController fromNodeController)
    {
        GameObject edge = Instantiate(edgePrefab, transform, false) as GameObject;

        lastEdgeController = edge.GetComponent<EdgeController>();
        lastFromNodeController = fromNodeController;

        lastEdgeController.rectTransform.anchoredPosition = lastFromNodeController.rectTransform.anchoredPosition;

        return edge;
        //print(eventSystem.currentSelectedGameObject.GetComponent<NodeController>().nodeText + " >>>>>>>>>>>>>>>>>>> " + toNodeController.nodeText);
    }

    public void PotentialEdgeCancel()
    {
        if (NodeController.connectMode)
        {
            CancelEdge();

            lastClickTime = Mathf.NegativeInfinity;

            ignoreNextClick = true;
        }
    }

    //public void ClearNodeControllerIgnoreFlag()
    //{
    //    NodeController.StaticClearIgnoreFlag();
    //}

    public void PotentialLeftClicked(UnityEngine.EventSystems.BaseEventData eventData)
    {
        if (ignoreNextClick)
        {
            ignoreNextClick = false;
            return;
        }

        UnityEngine.EventSystems.PointerEventData pointerEventData = eventData as UnityEngine.EventSystems.PointerEventData;

        if (pointerEventData.button != UnityEngine.EventSystems.PointerEventData.InputButton.Left || NodeController.connectMode)
        {
            lastClickTime = Mathf.NegativeInfinity;

            return;
        }

        if (Time.realtimeSinceStartup - lastClickTime <= 0.3F)
        {
            Vector2 localCursor;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, Input.mousePosition, _nodesCamera, out localCursor))
            {
                GameObject node = AddNode();

                node.GetComponent<RectTransform>().anchoredPosition = localCursor;
            }

            lastClickTime = Mathf.NegativeInfinity;
        }
        else
            lastClickTime = Time.realtimeSinceStartup;
    }

    //public void ClearNodeControllersIgnoreFlag()
    //{
    //    print("brosk");
    //    //NodeController.clearIgnoreFlag = true;
    //}
}
