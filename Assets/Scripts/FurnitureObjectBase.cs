using UnityEngine;

public class FurnitureObjectBase : MonoBehaviour
{
    public FurnitureOriginalData originalData;
    [SerializeField] protected FurnitureData furnitureData;
    [SerializeField] private Animator anim;
    protected SpriteRenderer[] spriteRenderers;
    protected bool unpacked;
    protected int currentSpriteIndex = 0;
    protected bool hasReceivedTagBonus = false;
    
    private GameObject indicatorInstance;

    public FurnitureData Data => furnitureData;
    public bool IsUnpacked => unpacked;
    public bool HasReceivedTagBonus => hasReceivedTagBonus;
    
    public void MarkTagBonusReceived()
    {
        hasReceivedTagBonus = true;
    }
    
    protected virtual void Awake()
    {
        furnitureData = new FurnitureData();
        if (spriteRenderers == null)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public virtual void SetUnpackedState(bool unpacked)
    {
        this.unpacked = unpacked;
        if (!this.unpacked)
        {
            if(anim) anim.enabled = false;
            UpdateSprites(House.instance.GetSpritesBySize(Data.originalData.typeOfSize));
            
            if (Data.originalData.requiredBase != null)
            {
                CreateRequiredBaseIndicator();
            }
        }
        else
        {
            UpdateSprites(originalData.sprites);
            if (indicatorInstance != null)
            {
                Destroy(indicatorInstance);
                indicatorInstance = null;
            }
        }
    }
    
    private void CreateRequiredBaseIndicator()
    {
        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
        }
        
        if (furnitureData.originalData.requiredBase == null) return;
        
        indicatorInstance = Instantiate(House.instance.requiredBaseIndicatorPrefab, transform);
        indicatorInstance.transform.rotation = Quaternion.identity;
        
        Vector3 indicatorPosition = CalculateIndicatorPosition();
        indicatorInstance.transform.localPosition = new Vector3(indicatorPosition.x, indicatorPosition.y, 0.1f);
        
        Sprite kitSprite = null;
        if (furnitureData.originalData.requiredBase.indicatorSprite != null)
        {
            kitSprite = furnitureData.originalData.requiredBase.indicatorSprite;
        }
        
        RequiredBaseIndicator indicator = indicatorInstance.GetComponent<RequiredBaseIndicator>();
        if (indicator != null)
        {
            indicator.Initialize(kitSprite);
        }
    }
    
    private Vector3 CalculateIndicatorPosition()
    {
        Vector3 position = Vector3.zero;
        
        switch (furnitureData.originalData.typeOfSize)
        {
            case TypeOfSize.one_one:
                break;
            case TypeOfSize.two_one:
                position = new Vector3(0.5f, 0, 0);
                break;
            case TypeOfSize.two_two:
                position = new Vector3(0.5f, 0.5f, 0);
                break;
            case TypeOfSize.three_one:
                position = new Vector3(1f, 0, 0);
                break;
        }

        //TODO: añadir rotación al indicador choto este
        return position;
    }
    
    public virtual void CopyFurnitureData(FurnitureData newData)
    {
        if (furnitureData == null)
        {
            furnitureData = new FurnitureData();
        }
        
        furnitureData.size = newData.size;
        furnitureData.prefab = newData.prefab;
        furnitureData.originalData = newData.originalData;
        furnitureData.VectorRotation = newData.VectorRotation;
        furnitureData.rotationStep = newData.rotationStep;
        furnitureData.hasReceivedTagBonus = newData.hasReceivedTagBonus;
        furnitureData.firstTimePlaced = newData.firstTimePlaced;
        
        if (spriteRenderers == null)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            
        if (furnitureData.originalData.requiredBase != null && !unpacked)
        {
            CreateRequiredBaseIndicator();
        }
    }
    
    public virtual void UpdateSprites(Sprite[] sprites)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sprite = sprites[i]; // No agregué null checks porque hay que asegurarnos que nunca sea null alguno
        }
    }
    
    private void OnDestroy()
    {
        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
        }
    }
}