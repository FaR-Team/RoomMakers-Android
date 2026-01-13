using UnityEngine;

public class OrientationManager : MonoBehaviour
{
    [Header("UI Layout Containers")]
    public GameObject portraitRoot;
    public GameObject landscapeRoot;

    [Header("Transform References")]
    public Transform scaler;

    private Vector3 portScalerPos = new Vector3(0f, -13f, 5f);
    private Vector3 portScalerScale = new Vector3(19.6000004f, 17.7999992f, 1f);

    private Vector3 landScalerPos = new Vector3(0f, -25.4400005f, 5f);
    private Vector3 landScalerScale = new Vector3(21.3999996f, 19.2999992f, 1f);

    private void Update()
    {
        CheckOrientation();
    }

    void CheckOrientation()
    {
        if (Screen.width > Screen.height)
        {
            if (!landscapeRoot.activeSelf) 
            {
                ApplyLandscape();
            }
        }
        else
        {
            if (!portraitRoot.activeSelf)
            {
                ApplyPortrait();
            }
        }
    }

    void ApplyLandscape()
    {
        portraitRoot.SetActive(false);
        landscapeRoot.SetActive(true);

        if (scaler) 
        {
            scaler.localPosition = landScalerPos;
            scaler.localScale = landScalerScale;
        }
    }

    void ApplyPortrait()
    {
        portraitRoot.SetActive(true);
        landscapeRoot.SetActive(false);

        if (scaler) 
        {
            scaler.localPosition = portScalerPos;
            scaler.localScale = portScalerScale;
        }
    }
}