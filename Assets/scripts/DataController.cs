using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DataController : MonoBehaviour
{
    static private DataController _instance;
    static public DataController instance
    {
        get
        {
            return _instance;
        }
    }

    public Dictionary<int, NodeController> nodes;

    void Awake()
    {
        nodes = new Dictionary<int, NodeController>();

        _instance = this;
    }

    private int GenerateNodeID()
    {
        int id;

        while (nodes.ContainsKey(id = Random.Range(int.MinValue, int.MaxValue)) || id == 0) { }

        return id;
    }

    public void RegisterNode(NodeController nodeController)
    {
        int id = GenerateNodeID();

        nodeController.nodeID = id;

        nodes.Add(id, nodeController);
    }

    public void UnregisterNode(int id)
    {
        nodes.Remove(id);
    }
}
