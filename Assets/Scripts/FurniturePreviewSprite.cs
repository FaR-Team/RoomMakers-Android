using System.Linq;
using UnityEngine;

public class FurniturePreviewSprite : MonoBehaviour
{
    private FurniturePreview thisFurniturePreview;
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    private void Awake()
    {
        thisFurniturePreview = GetComponent<FurniturePreview>();
    }
    private void OnEnable()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sprite = thisFurniturePreview.data.sprites[i];
        }
    }
}
