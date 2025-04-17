using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBlink : MonoBehaviour
{
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private float blinkSpeed = 1.0f;
    
    private Color whiteColor = Color.white;
    private Color blackColor = Color.black;
    private float t = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        // If no renderer is assigned, try to get one from this object
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Ensure we have a sprite renderer to work with
        if (targetRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found for FollowBlink script!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (targetRenderer != null)
        {
            // Calculate the interpolation value using a sine wave for smooth transition
            t += Time.deltaTime * blinkSpeed;
            float pingPong = Mathf.PingPong(t, 1.0f);
            
            targetRenderer.color = Color.Lerp(whiteColor, blackColor, pingPong);
        }
    }
}