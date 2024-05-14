using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboPopUp : MonoBehaviour
{
    [SerializeField] private TextMeshPro scoreText;
    void Start()
    {
        
    }
    
    void Update()
    {
        
    }

    void Setup(int score)
    {
        scoreText.text = score.ToString();
    }

    public static void Create(ComboPopUp prefab, int scoreAmount, Vector2 position, Vector2 offset)
    {
        //Debug.Log("POPUP?");
        Vector2 instantiatePos = position + offset;

        ComboPopUp popUp = Instantiate(prefab, instantiatePos, Quaternion.identity);
        popUp.Setup(scoreAmount);
    }
}
