using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteRaycastSetup : MonoBehaviour
{
    void Start()
    {
        var img = GetComponent<Image>();
        img.alphaHitTestMinimumThreshold = 0.1f;
    }
}
