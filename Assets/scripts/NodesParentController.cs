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
    private RectTransform rectTransform;
    private Camera nodesCamera;
    private RectTransform edgesParent;
    private EdgeController lastEdgeController;
    private NodeController lastFromNodeController;
    //private NodeController lastPotentialConnectModeToNode = null;
    public Object nodePrefab;
    public Object edgePrefab;
    

    void Awake()
    {
        //eventSystem = UnityEngine.EventSystems.EventSystem.current;
        rectTransform = GetComponent<RectTransform>();
        edgesParent = rectTransform.Find("Edges").GetComponent<RectTransform>();
        nodesCamera = Camera.main;

        _instance = this;
    }

    void Update()
    {
        if (NodeController.connectMode)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                CancelEdge();
            }

            //if(lastPotentialConnectModeToNode != NodeController.potentialConnectModeToNode)
            //{
            //    if (NodeController.potentialConnectModeToNode != null)
            //        lastEdgeController.edgeState = EdgeController.EdgeState.Potential;
            //    else
            //        lastEdgeController.edgeState = EdgeController.EdgeState.Temporary;

            //    lastPotentialConnectModeToNode = NodeController.potentialConnectModeToNode;
            //}

            Vector2 localCursor, targetPoint = Vector2.zero;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, nodesCamera, out localCursor))
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

    public void CancelEdge()
    {
        NodeController.connectMode = false;
        if (lastEdgeController)
            lastEdgeController.DeleteEdge();

        lastEdgeController = null;
        lastFromNodeController = null;
        NodeController.potentialConnectModeToNode = null;
        NodeController.connectModeFromNode = null;
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

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, nodesCamera, out localCursor))
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
