using UnityEngine;
using System.Collections;

public class HelpController : MonoBehaviour
{
    static private HelpController _instance;
    static public HelpController instance
    {
        get
        {
            return _instance;
        }
    }
    private bool _helpEnabled = false;
    public bool helpEnabled
    {
        get
        {
            return _helpEnabled;
        }
        set
        {
            if (_helpEnabled != value)
            {
                _helpEnabled = value;

                helpBackground.enabled = value;
                helpText.enabled = value;
            }
        }
    }
    public UnityEngine.UI.Image helpBackground;
    public UnityEngine.UI.Text helpText;

    void Awake()
    {
        _instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            helpEnabled = !helpEnabled;
        }
    }
}
