using UnityEngine;
using System.Collections;

public class CreditsController : MonoBehaviour
{
    private bool _creditsEnabled = false;
    public bool creditsEnabled
    {
        get
        {
            return _creditsEnabled;
        }
        set
        {
            if (_creditsEnabled != value)
            {
                _creditsEnabled = value;

                creditsBackground.enabled = value;
                creditsText.enabled = value;
            }
        }
    }
    public UnityEngine.UI.Image creditsBackground;
    public UnityEngine.UI.Text creditsText;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            creditsEnabled = !creditsEnabled;

            StopAllCoroutines();

            if (creditsEnabled)
            {
                StartCoroutine(DisableCredits());
            }
        }
    }

    IEnumerator DisableCredits()
    {
        yield return new WaitForSeconds(10.0F);

        creditsEnabled = false;
    }
}
