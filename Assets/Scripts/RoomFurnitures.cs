using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomFurnitures : MonoBehaviour
{
    public Dictionary<Vector2, PlacementData> PlacementDatasInPosition = new();
    public Dictionary<Vector2, KitObject> KitsInPosition = new();
    
    // TODO: que no se tenga que asignar aca la referencia al prefab
    [SerializeField] protected ComboPopUp popUpPrefab;
    [SerializeField] protected ComboPopUp matchPrefab;

    private LayerMask wallsLayerMask;
    private LayerMask unplaceableLayerMask;
    //private LayerMask kitLayerMask;

    private void Start()
    {
        wallsLayerMask = LayerMask.GetMask("Walls", "InnerWalls");
        unplaceableLayerMask = ~(1 << 15); // Everything but kit 
        //kitLayerMask = (1 << 15);
    }

    public bool PlaceFurniture(Vector2 position, FurnitureData furnitureData, bool isItem = false)
    {
        if (furnitureData.instanceID == 0)
        {
            furnitureData.instanceID = System.DateTime.Now.GetHashCode() ^ furnitureData.GetHashCode();
        }
        
        var originalData = furnitureData.originalData;
        
        Debug.Log($"Placing furniture: {originalData.Name}, isStackable: {originalData.isStackable}");
        
        List<Vector2> positionToOccupy = CalculatePositions(position, furnitureData.size);

        bool requiresBase = furnitureData.originalData.requiredBase != null;
        bool canPlace =
            (!positionToOccupy.Any(x => PlacementDatasInPosition.ContainsKey(x) || Physics2D.OverlapCircle(x, 0.2f, isItem ? ~0 : unplaceableLayerMask))) && // If isItem, check for anything
            (!furnitureData.originalData.wallObject || CheckWallsAndRotate(position, furnitureData));
        bool placeOnTop = false;
        bool unboxed = true;
        
        bool isStacking = false;
        BottomFurnitureObject stackReceiver = null;
        
        if (originalData.isStackable)
        {
            foreach (var pos in positionToOccupy)
            {
                if (PlacementDatasInPosition.ContainsKey(pos))
                {
                    var existingData = PlacementDatasInPosition[pos];
                    if (existingData.instantiatedFurniture is BottomFurnitureObject bottomObj && 
                        bottomObj.Data.originalData.isStackReceiver)
                    {
                        bool isSameObject = furnitureData.instanceID != 0 && 
                                           existingData.stackedItems != null && 
                                           existingData.stackedItems.Any(item => item.Data.instanceID == furnitureData.instanceID);
                        
                        if (bottomObj.Data.currentStackLevel < bottomObj.Data.originalData.maxStackLevel || isSameObject)
                        {
                            stackReceiver = bottomObj;
                            isStacking = true;
                            position = pos;
                            canPlace = true;
                            break;
                        }
                    }
                }
            }
        }

        if (canPlace && !isStacking)
        {
            if (!isItem) // Don't even create data for Kits
            {
                PlacementData data = new PlacementData(positionToOccupy, furnitureData);
                foreach (var pos in positionToOccupy)
                {
                    PlacementDatasInPosition[pos] = data;
                }
                
                if (requiresBase)
                {
                    unboxed = false; // Set to false so we check later if any kits are underneath
                    
                    for (int i = 0; i < positionToOccupy.Count; i++)
                    {
                        if (KitsInPosition.TryGetValue(positionToOccupy[i], out KitObject kit) &&
                            furnitureData.originalData.requiredBase == kit.Data.originalData)
                        {
                            unboxed = true;
                            break;
                        }
                    }
                }
            }
            else // If placing a kit
            {
                //if (KitsInPosition.ContainsKey(position)) canPlace = false;
            }
        }
        else if (!isStacking)
        {
            bool check = true;
            Vector2 validBottomPosition = Vector2.zero;
            bool foundValidPosition = false;
         
            // First check if all positions are contained in the dictionary
            foreach (var pos in positionToOccupy)
            {
                if (!PlacementDatasInPosition.ContainsKey(pos)) 
                {
                    check = false;
                    break;
                }
                
                // Find a valid position that we can use for compatibility checks
                if (!foundValidPosition)
                {
                    validBottomPosition = pos;
                    foundValidPosition = true;
                }
            }

            if (check && foundValidPosition)
            {
                // Use the valid position we found for compatibility checks
                canPlace = PlacementDatasInPosition[validBottomPosition].instantiatedFurniture.IsUnpacked &&
                           positionToOccupy.Intersect(PlacementDatasInPosition[validBottomPosition].occupiedPositions).Count() == positionToOccupy.Count()
                           && PlacementDatasInPosition[validBottomPosition].IsCompatibleWith(originalData)
                           && PlacementDatasInPosition[validBottomPosition].HasFreePositions(positionToOccupy); 
                placeOnTop = canPlace;
                
                // If we can place it, update the position to use for instantiation
                if (canPlace)
                {
                    position = validBottomPosition;
                }

                if (requiresBase)
                {
                    unboxed = false;

                    // Check if bottom object is on top of necessary kit
                    foreach (var pos in PlacementDatasInPosition[validBottomPosition].occupiedPositions)
                    {
                        if (KitsInPosition.TryGetValue(pos, out KitObject kit) &&
                            furnitureData.originalData.requiredBase == kit.Data.originalData)
                        {
                            unboxed = true;
                            break;
                        }
                    }
                }
            }
        }

        if (!canPlace && !isStacking) return false;

        var finalPos = GridManager.PositionToCellCenter(position);

        if (isStacking && stackReceiver != null)
        {
            bool isSameObject = false;
            int existingStackIndex = -1;
            
            if (furnitureData.instanceID != 0 && 
                PlacementDatasInPosition[position].stackedItems != null)
            {
                for (int i = 0; i < PlacementDatasInPosition[position].stackedItems.Count; i++)
                {
                    if (PlacementDatasInPosition[position].stackedItems[i].Data.instanceID == furnitureData.instanceID)
                    {
                        isSameObject = true;
                        existingStackIndex = i;
                        break;
                    }
                }
            }
            
            if (!isSameObject)
            {
                if (stackReceiver.Data.currentStackLevel >= stackReceiver.Data.originalData.maxStackLevel)
                {
                    return false;
                }
                
                stackReceiver.Data.currentStackLevel++;
            }
            
            FurnitureObjectBase stackedItem = Instantiate(furnitureData.prefab, finalPos, stackReceiver.transform.rotation).GetComponent<FurnitureObjectBase>();
            
            if (isSameObject && existingStackIndex >= 0)
            {
                furnitureData.currentStackLevel = PlacementDatasInPosition[position].stackedItems[existingStackIndex].Data.currentStackLevel;
            }
            else
            {
                furnitureData.currentStackLevel = stackReceiver.Data.currentStackLevel;
            }
            
            furnitureData.rotationStep = stackReceiver.Data.rotationStep;
            furnitureData.VectorRotation = stackReceiver.Data.VectorRotation;
            
            stackedItem.CopyFurnitureData(furnitureData);
            stackedItem.SetUnpackedState(true);
            
            if (stackedItem is TopFurnitureObject topObj && 
                furnitureData.originalData.stackLevelSprites != null && 
                furnitureData.originalData.stackLevelSprites.Length > stackReceiver.Data.currentStackLevel - 1)
            {
                SpriteRenderer spriteRenderer = topObj.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = furnitureData.originalData.stackLevelSprites[stackReceiver.Data.currentStackLevel - 1];
                    
                    if (spriteRenderer.transform != stackedItem.transform)
                    {
                        spriteRenderer.transform.rotation = stackReceiver.transform.rotation;
                    }
                }
            }
            
            int tagBonus = 0;
            Room currentRoom = GetComponent<Room>();

            RoomTag furnitureTag = furnitureData.originalData.furnitureTag;
            if (furnitureTag != RoomTag.None && currentRoom != null)
            {
                currentRoom.TrySetRoomTagFromFurniture(furnitureTag);
            }

            if (currentRoom != null &&
                furnitureTag != RoomTag.None &&
                furnitureTag == currentRoom.roomTag &&
                !furnitureData.hasReceivedTagBonus)
            {
                tagBonus = furnitureData.originalData.tagMatchBonusPoints;
                furnitureData.hasReceivedTagBonus = true;

                PlayerController.instance.Inventory.UpdateMoney(tagBonus);
                House.instance.UpdateScore(tagBonus);
                ComboPopUp.Create(matchPrefab, tagBonus, finalPos, new Vector2(0f, 1.5f));
            }
            
            int comboPoints = 0;
            
            if (stackReceiver is BottomFurnitureObject bottomObj)
            {
                List<Vector2> positionsForCombo = CalculatePositions(position, furnitureData.size);
                
                comboPoints = bottomObj.MakeCombo(positionsForCombo.ToArray());
                
                if (comboPoints > 0 && stackedItem is TopFurnitureObject topObject)
                {
                    topObject.MakeCombo();
                    
                    PlayerController.instance.Inventory.UpdateMoney(comboPoints);
                    House.instance.UpdateScore(comboPoints);
                    
                    ComboPopUp.Create(popUpPrefab, comboPoints, finalPos, new Vector2(0f, 1.2f));
                    
                    if (tagBonus > 0)
                    {
                        StartCoroutine(ShowMatchPopupDelayed(tagBonus, finalPos));
                    }
                }
            }
            
            if (!furnitureData.firstTimePlaced)
            {
                PlayerController.instance.Inventory.UpdateMoney(furnitureData.originalData.price);
                House.instance.UpdateScore(furnitureData.originalData.price);
                furnitureData.firstTimePlaced = true;
            }
            
            AudioManager.instance.PlaySfx(GlobalSfx.Click);
            
            PlacementData data = PlacementDatasInPosition[position];
            data.stackedItems = data.stackedItems ?? new List<FurnitureObjectBase>();
            
            if (isSameObject && existingStackIndex >= 0)
            {
                data.stackedItems[existingStackIndex] = stackedItem;
            }
            else
            {
                data.stackedItems.Add(stackedItem);
            }
            
            return true;
        }

        FurnitureObjectBase furniturePrefab = Instantiate(furnitureData.prefab, finalPos, Quaternion.Euler(furnitureData.VectorRotation)).GetComponent<FurnitureObjectBase>();
        furniturePrefab.CopyFurnitureData(furnitureData);
        furniturePrefab.SetUnpackedState(unboxed);
        
        if(!isItem) GivePoints(furnitureData, positionToOccupy, placeOnTop, finalPos, furniturePrefab, unboxed);
        else KitsInPosition[finalPos] = furniturePrefab as KitObject;

        return true;
    }

    #region Items
    public bool PlaceItem(Vector2 position, ItemData itemData, FurnitureData furnitureData)
    {
        bool placed;
        Room currentRoom = GetComponent<Room>();
        switch (itemData.type)
        {
            case ItemType.Tagger:
                if (currentRoom.roomTag is RoomTag.Shop){ placed = false; break;}
                TagSelectionUI.instance.ShowTagSelection(currentRoom, () =>
                {
                    Debug.Log("Placed tagger");
                });
                placed = true;
                break;
            case ItemType.Sledgehammer:
                var coll = Physics2D.OverlapCircle(position, 0.2f);
                if (coll != null && coll.TryGetComponent<DoorData>(out DoorData door) && !door.isUnlocked)
                {
                    door.BuyNextRoom(true);
                    placed = true;
                    break;
                }
                placed = false;
                break;
            case ItemType.OutletKit:
                placed = PlaceFurniture(position, furnitureData, true);
                break;
            case ItemType.PipelineKit:
                placed = PlaceFurniture(position, furnitureData, true);
                break;
            default:
                placed = false;
                break;
        }
        
        return placed;
    }
    
    #endregion

    #region Wall Objects
    private bool CheckWallsAndRotate(Vector2 position, FurnitureData data)
    {
        bool isWallUp = Physics2D.Raycast(position, Vector2.up, 1f, wallsLayerMask);
        bool isWallLeft = Physics2D.Raycast(position, Vector2.left, 1f, wallsLayerMask);
        bool isWallBottom = Physics2D.Raycast(position, Vector2.down, 1f, wallsLayerMask);
        bool isWallRight = Physics2D.Raycast(position, Vector2.right, 1f, wallsLayerMask);
        
        if (!(isWallBottom || isWallRight || isWallLeft || isWallUp)) return false;
        
        bool[] availableWalls = {isWallUp, isWallLeft, isWallBottom, isWallRight};
        
        RotateToWall(data, availableWalls);
        return true;
    }

    void RotateToWall(FurnitureData data, bool[] availableWalls)
    {
        bool correctRotation = availableWalls[data.rotationStep];

        if (!correctRotation)
        {
            for (int i = 0; i < availableWalls.Length; i++)
            {
                if (availableWalls[i])
                {
                    data.SetRotationByStep(i);
                    break;
                }
            }
        }
    }
    
    #endregion

    protected virtual void GivePoints(FurnitureData data, List<Vector2> positionToOccupy, bool placeOnTop, Vector2 finalPos, FurnitureObjectBase furnitureObject, bool unboxed)
    {
        if (!data.firstTimePlaced && unboxed) // First time we place data, check if placed unboxed to give points
        {
            PlayerController.instance.Inventory.UpdateMoney(data.originalData.price);
            House.instance.UpdateScore(data.originalData.price);
            data.firstTimePlaced = true;
        }
        
        int tagBonus = 0;
        if (unboxed) // Only do tag stuff if unboxed
        {
            // Get the room component
            Room currentRoom = GetComponent<Room>();


            // Check if this furniture has a tag and the room doesn't have one yet
            RoomTag furnitureTag = data.originalData.furnitureTag;
            if (furnitureTag != RoomTag.None && currentRoom != null)
            {
                // Try to set the room tag from furniture
                currentRoom.TrySetRoomTagFromFurniture(furnitureTag);
            }

            // Calculate bonus points for tag matching - only if not already received
            if (currentRoom != null &&
                furnitureTag != RoomTag.None &&
                furnitureTag == currentRoom.roomTag &&
                !data.hasReceivedTagBonus) // Check using the data field
            {
                tagBonus = data.originalData.tagMatchBonusPoints;
                data.hasReceivedTagBonus = true; // Mark as received in the data

                // Show bonus points popup only if we're actually awarding points and not placing on top
                // (if placing on top, we'll handle it differently below)
                if (tagBonus > 0 && !placeOnTop)
                {
                    PlayerController.instance.Inventory.UpdateMoney(tagBonus);
                    House.instance.UpdateScore(tagBonus);
                    ComboPopUp.Create(matchPrefab, tagBonus, finalPos, new Vector2(0f, 1.5f));
                }
            }
        }

        if (!placeOnTop)
        {
            if (TryGetComponent(out MainRoom room))
            {
                MainRoom.instance.availableTiles -= data.originalData.size.x * data.originalData.size.y;
            }

            if (furnitureObject.originalData is not ItemData) PlacementDatasInPosition[finalPos].instantiatedFurniture = furnitureObject;
            //else PlacementDatasInPosition[finalPos].instantiatedBaseObject = furnitureObject;  // If just placed a kit
        }
        else
        {
            TopFurnitureObject topObject = (TopFurnitureObject)furnitureObject;
            if (unboxed)
            {
                // Si el objeto va encima de otro, lo guardamos en el PlacementData y damos doble score por combo
                int totalCombo = 0;

                BottomFurnitureObject bottomObject = (BottomFurnitureObject)PlacementDatasInPosition[finalPos]
                    .instantiatedFurniture;

                // Check for sprite changes regardless of combo status
                topObject.CheckAndUpdateSprite(bottomObject);

                // Only calculate and award points if combo hasn't been done yet
                if (!topObject.ComboDone)
                {
                    // Calculamos y sumamos los puntos por cada tile en el que se hace combo
                    totalCombo += bottomObject.MakeCombo(positionToOccupy.ToArray());

                    // Si hubo al menos un tile en el que se hizo combo, gastamos el combo tambien en el objeto de arriba
                    if (totalCombo > 0)
                    {
                        topObject.MakeCombo();

                        // Add tag bonus to combo points
                        int comboPoints = totalCombo;
                        if (tagBonus > 0)
                        {
                            comboPoints += tagBonus;
                        }

                        PlayerController.instance.Inventory.UpdateMoney(comboPoints);
                        House.instance.UpdateScore(comboPoints);

                        // Show combo popup immediately
                        ComboPopUp.Create(popUpPrefab, comboPoints, finalPos, new Vector2(0f, 1.2f));

                        // If there's also a tag bonus, show it after a delay
                        if (tagBonus > 0)
                        {
                            StartCoroutine(ShowMatchPopupDelayed(tagBonus, finalPos));
                        }
                    }
                    else if (tagBonus > 0)
                    {
                        // If there's no combo but there is a tag bonus, add it separately
                        PlayerController.instance.Inventory.UpdateMoney(tagBonus);
                        House.instance.UpdateScore(tagBonus);

                        ComboPopUp.Create(matchPrefab, tagBonus, finalPos, new Vector2(0f, 1.2f));
                    }
                }
            }

            // Updateamos la placement data con el objeto que pusimos encima
            PlacementDatasInPosition[finalPos].PlaceObjectOnTop(positionToOccupy, topObject);
        }
    }
    
    private IEnumerator ShowMatchPopupDelayed(int tagBonus, Vector2 position)
    {
        // Wait for half a second
        yield return new WaitForSeconds(0.7f);
        
        // Show the match popup
        ComboPopUp.Create(matchPrefab, tagBonus, position, new Vector2(0f, 1f)); // Slightly higher position
    }
    
    public void RemoveDataInPositions(List<Vector2> positions)
    {
        foreach (var pos in positions)
        {
            PlacementDatasInPosition.Remove(pos);
        }
    }
    
    public void RemoveDataInPosition(Vector2 pos)
    {
        var data = PlacementDatasInPosition[pos];
        Destroy(data.instantiatedFurniture.gameObject);
        foreach (var tile in data.occupiedPositions)
        {
            PlacementDatasInPosition.Remove(tile);
        }
    }

    public void RemoveTopObjectInPositions(List<Vector2> positions)
    {
        foreach (var pos in positions)
        {
            //PlacementDatasInPosition[pos].furnitureOnTopData = null;
            //PlacementDatasInPosition[pos].instantiatedFurnitureOnTop = null;
        }
    }

    public void RemoveTopObjectInPosition(Vector2 pos)
    {
        PlacementDatasInPosition[pos].ClearTopObject(pos);
    }

    public void RemoveKitInPosition(Vector2 pos)
    {
        if (!KitsInPosition.TryGetValue(pos, out KitObject kit)) return;
        Destroy(kit.gameObject);
        KitsInPosition.Remove(pos);
    }

    private List<Vector2> CalculatePositions(Vector2 position, Vector2Int size)
    {
        List<Vector2> returnVal = new();
        for (int x = 0; x < Mathf.Abs(size.x); x++)
        {
            for (int y = 0; y < Mathf.Abs(size.y); y++)
            {
                // Multiplicamos por el signo para que sepa si el size es negativo o positivo
                var newPosition = position + new Vector2(x * Mathf.Sign(size.x), y * Mathf.Sign(size.y));
                returnVal.Add(GridManager.PositionToCellCenter(newPosition));
            }
        }
        return returnVal;
    }

}