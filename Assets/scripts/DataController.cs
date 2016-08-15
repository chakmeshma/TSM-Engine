using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

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
    
    private string fileName = "Geschichte.xml";

    [HideInInspector]
    public bool loading = false;
    public struct DataVector
    {
        public float x;
        public float y;
    }
    public struct NodeData
    {
        public int id;
        public DataVector position;
        public DataVector size;
        public string text;
        public int[] toNodes;
        //public NodeData[] fromNodes;
    }
    public struct Settings
    {
        public DataVector pan;
        public float zoom;
    }
    public struct Data
    {
        public NodeData[] nodes;
        public Settings settings;
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

    public void RegisterNode(NodeController nodeController, int forceID = 0)
    {
        int id = (forceID == 0) ? (GenerateNodeID()) : (forceID);

        nodeController.nodeID = id;

        nodes.Add(id, nodeController);
    }

    public void UnregisterNode(int id)
    {
        nodes.Remove(id);
    }

    string SerializeDataIntoXML()
    {
        string dataSTR;

        Data data = new Data();

        data.nodes = new NodeData[nodes.Count];
        data.settings = new Settings();

        int i = 0;

        foreach(var node in nodes)
        {
            data.nodes[i] = new NodeData();

            data.nodes[i].id = node.Key;

            data.nodes[i].position = new DataVector();
            data.nodes[i].size = new DataVector();

            data.nodes[i].position.x = node.Value.rectTransform.anchoredPosition.x;
            data.nodes[i].position.y = node.Value.rectTransform.anchoredPosition.y;

            data.nodes[i].size.x = node.Value.rectTransform.sizeDelta.x;
            data.nodes[i].size.y = node.Value.rectTransform.sizeDelta.y;

            data.nodes[i].text = node.Value.nodeText;

            data.nodes[i].toNodes = new int[node.Value.outwardEdges.Count];

            int ii = 0;

            foreach(var edge in node.Value.outwardEdges)
            {
                data.nodes[i].toNodes[ii] = edge.toNode.nodeID;

                ii++;
            }
            
            i++;
        }

        CameraController cameraController = FindObjectOfType<CameraController>();

        data.settings.pan.x = cameraController.pan.x;
        data.settings.pan.y = cameraController.pan.y;
        data.settings.zoom = cameraController.zoom;

        System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(data.GetType());

        using(System.IO.StringWriter textWriter = new System.IO.StringWriter())
        {
            xmlSerializer.Serialize(textWriter, data);
            dataSTR = textWriter.ToString();
        }

        return dataSTR;
    }

    public void Save(string forceFileName = "", bool makeHidden = false)
    {
        string fileName = (forceFileName == "") ? (this.fileName) : (forceFileName);

        System.IO.File.WriteAllText(fileName, SerializeDataIntoXML());

        if(makeHidden)
            System.IO.File.SetAttributes(fileName, System.IO.FileAttributes.Hidden);
    }

    public bool Load()
    {
        if (!System.IO.File.Exists(fileName))
            return false;

        loading = true;

        //if (NodeController.resizeMoveMode != NodeController.ResizeMoveMode.No || NodeController.connectMode)
        //    return;        

        System.IO.StreamReader streamReader = new System.IO.StreamReader(fileName);

        Data data = new Data();

        XmlSerializer xmlSerializer = new XmlSerializer(data.GetType());

        data = (Data)xmlSerializer.Deserialize(streamReader);

        streamReader.Close();

        //print(data.nodes.Length);

        //foreach(NodeData node in data.nodes)
        //{
        //    print("\tid:\t"+ node.id);
        //    print("\ttext:\t" + node.text);
        //    print("\tposX:\t" + node.position.x);
        //    print("\tposY:\t" + node.position.y);
        //    print("\tsizeX:\t" + node.size.x);
        //    print("\tsizeY:\t" + node.size.y);
        //    print("\t" + node.toNodes.Length + " toNodes:");

        //    foreach(int id in node.toNodes)
        //    {
        //        print("\t\tid:\t" + id);
        //    }
        //}

        Dictionary<int, NodeController> nodes = new Dictionary<int, NodeController>();

        foreach(NodeData node in data.nodes)
        {
            GameObject nodeGameObject = NodesParentController.instance.AddNode(node.id);

            nodeGameObject.GetComponent<NodeController>().nodeText = node.text;
            nodeGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(node.position.x, node.position.y);
            nodeGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(node.size.x, node.size.y);

            nodes.Add(node.id, nodeGameObject.GetComponent<NodeController>());
        }

        foreach(NodeData node in data.nodes)
        {
            NodeController fromNode = nodes[node.id];

            foreach(int toNodeID in node.toNodes)
            {
                NodeController toNode = nodes[toNodeID];

                fromNode.InitConnectMode();
                NodesParentController.instance.AddEdge(fromNode);
                toNode.ConnectEdge();
            }
        }


        CameraController cameraController = FindObjectOfType<CameraController>();

        cameraController.pan = new Vector2(data.settings.pan.x, data.settings.pan.y);
        cameraController.zoom = data.settings.zoom;        

        loading = false;

        return true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            try
            {
                Save("before reload save.xml", true);
            }
            catch(System.Exception e)
            {
                print(e.Message);
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene("main", UnityEngine.SceneManagement.LoadSceneMode.Single);
            //Load();
        }
    }
}
