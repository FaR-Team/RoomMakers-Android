using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureInfoSprite : MonoBehaviour
{
    [SerializeField] private Image[] images;

    public void UpdateSprites(Sprite[] sprites)
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].sprite = sprites[i];
        }
    }
}
