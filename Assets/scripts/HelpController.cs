using UnityEngine;
using System.Collections;

public class HelpController : MonoBehaviour
{
    private bool _helpEnabled = true;
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            helpEnabled = !helpEnabled;
        }
    }
}
