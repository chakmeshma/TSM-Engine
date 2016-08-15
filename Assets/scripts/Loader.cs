using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour
{
    static public bool loadedFromFile = false;

    void Start()
    {
        loadedFromFile = DataController.instance.Load();

        if (!loadedFromFile)
            HelpController.instance.helpEnabled = true;
    }

    void OnApplicationQuit()
    {
        DataController.instance.Save("before quit save.xml", true);
    }
}
