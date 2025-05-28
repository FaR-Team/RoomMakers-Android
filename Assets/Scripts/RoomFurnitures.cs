using System;
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
    private LayerMask kitLayerMask;
    public static event Action<FurnitureOriginalData> OnPlaceFurniture;
    public static event Action<FurnitureOriginalData> OnPlaceOnTopEvent;
    public static event Action<int> OnComboDone;
    public static event Action<ItemType> OnItemUse;

    private struct PlacementDetails
    {
        public bool CanPlace;
        public Vector2 FinalWorldPosition;
        public bool Unboxed;
        public bool PlaceOnTop;
    }

    private void Start()
    {
        wallsLayerMask = LayerMask.GetMask("Walls", "InnerWalls");
        unplaceableLayerMask = ~(1 << 15); // Everything but kit 
        kitLayerMask = ~(1 << 8); // Everything but furnitures
    }

    public bool PlaceFurniture(Vector2 worldPosition, FurnitureData furnitureData, bool isItem = false)
    {
        // 1. Initial Setup
        if (furnitureData.instanceID == 0)
            furnitureData.instanceID = System.DateTime.Now.GetHashCode() ^ furnitureData.GetHashCode();

        var originalData = furnitureData.originalData;

        List<Vector2> potentialOccupiedCells = CalculatePositions(worldPosition, furnitureData.size);
        Vector2 currentTargetWorldPosition = worldPosition;

        // 2. Try Stacking Placement
        if (originalData.isStackable)
        {
            if (TryProcessStackedPlacement(ref currentTargetWorldPosition, furnitureData, originalData, potentialOccupiedCells))
            {
                return true;
            }
        }

        // 3. Handle Wall Object Placement (Rotation)
        if (originalData.wallObject && !CheckWallsAndRotate(currentTargetWorldPosition, furnitureData))
        {
            return false;
        }

        // 4. Determine Non-Stacked Placement Details
        PlacementDetails placementDetails = DetermineNonStackedPlacementDetails(currentTargetWorldPosition, furnitureData, originalData, potentialOccupiedCells, isItem);

        if (!placementDetails.CanPlace)
        {
            return false;
        }

        // 5. Finalize and Instantiate Non-Stacked Placement
        FinalizeAndInstantiateNonStacked(
            placementDetails.FinalWorldPosition,
            furnitureData,
            originalData,
            potentialOccupiedCells,
            isItem,
            placementDetails.Unboxed,
            placementDetails.PlaceOnTop
        );

        return true;
    }

    private bool TryProcessStackedPlacement(ref Vector2 positionForStacking, FurnitureData furnitureData, FurnitureOriginalData originalData, List<Vector2> potentialOccupiedCells)
    {
        BottomFurnitureObject stackReceiver = null;
        Vector2 actualStackingCell = Vector2.zero;

        foreach (var cell in potentialOccupiedCells)
        {
            if (PlacementDatasInPosition.TryGetValue(cell, out var existingData) &&
                existingData.instantiatedFurniture is BottomFurnitureObject bottomObj &&
                bottomObj.Data.originalData.isStackReceiver)
            {
                bool isSameObjectBeingRestacked = furnitureData.instanceID != 0 &&
                                   existingData.stackedItems != null &&
                                   existingData.stackedItems.Any(item => item.Data.instanceID == furnitureData.instanceID);

                if (bottomObj.Data.currentStackLevel < bottomObj.Data.originalData.maxStackLevel || isSameObjectBeingRestacked)
                {
                    stackReceiver = bottomObj;
                    actualStackingCell = cell;
                    positionForStacking = cell;
                    break;
                }
            }
        }

        if (stackReceiver == null) return false;

        Vector2 finalInstantiatePos = GridManager.PositionToCellCenter(actualStackingCell);
        PlacementData basePlacementData = PlacementDatasInPosition[actualStackingCell];

        bool isSameObject = false;
        int existingStackIndex = -1;
        if (furnitureData.instanceID != 0 && basePlacementData.stackedItems != null)
        {
            for (int i = 0; i < basePlacementData.stackedItems.Count; i++)
            {
                if (basePlacementData.stackedItems[i].Data.instanceID == furnitureData.instanceID)
                {
                    isSameObject = true;
                    existingStackIndex = i;
                    break;
                }
            }
        }

        if (!isSameObject && stackReceiver.Data.currentStackLevel >= stackReceiver.Data.originalData.maxStackLevel)
        {
            return false;
        }

        if (!isSameObject) stackReceiver.Data.currentStackLevel++;

        FurnitureObjectBase stackedItemInstance = Instantiate(furnitureData.prefab, finalInstantiatePos, stackReceiver.transform.rotation).GetComponent<FurnitureObjectBase>();

        furnitureData.currentStackLevel = isSameObject && existingStackIndex >= 0 ?
            basePlacementData.stackedItems[existingStackIndex].Data.currentStackLevel :
            stackReceiver.Data.currentStackLevel;

        furnitureData.rotationStep = stackReceiver.Data.rotationStep;
        furnitureData.VectorRotation = stackReceiver.Data.VectorRotation;

        stackedItemInstance.CopyFurnitureData(furnitureData);
        stackedItemInstance.SetUnpackedState(true);

        if (stackedItemInstance is TopFurnitureObject topObj && originalData.stackLevelSprites != null &&
            originalData.stackLevelSprites.Length > furnitureData.currentStackLevel - 1)
        {
            SpriteRenderer spriteRenderer = topObj.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = originalData.stackLevelSprites[furnitureData.currentStackLevel - 1];
                if (spriteRenderer.transform != stackedItemInstance.transform)
                    spriteRenderer.transform.rotation = stackReceiver.transform.rotation;
            }
        }

        int tagBonus = HandleStackedTagBonus(furnitureData, originalData, finalInstantiatePos);
        HandleStackedComboPoints(stackReceiver, potentialOccupiedCells, stackedItemInstance, finalInstantiatePos, tagBonus);
        ApplyFirstTimePlacementBonus(furnitureData, originalData, true);

        AudioManager.instance.PlaySfx(GlobalSfx.Click);

        basePlacementData.stackedItems = basePlacementData.stackedItems ?? new List<FurnitureObjectBase>();
        if (isSameObject && existingStackIndex >= 0)
        {
            if (basePlacementData.stackedItems[existingStackIndex] != null && basePlacementData.stackedItems[existingStackIndex] != stackedItemInstance)
                Destroy(basePlacementData.stackedItems[existingStackIndex].gameObject);
            basePlacementData.stackedItems[existingStackIndex] = stackedItemInstance;
        }
        else
        {
            basePlacementData.stackedItems.Add(stackedItemInstance);
        }

        OnPlaceFurniture?.Invoke(furnitureData.originalData);
        return true;
    }

    private PlacementDetails DetermineNonStackedPlacementDetails(Vector2 worldPosition, FurnitureData furnitureData, FurnitureOriginalData originalData, List<Vector2> cellsToOccupy, bool isItem)
    {
        PlacementDetails details = new PlacementDetails { CanPlace = false, FinalWorldPosition = worldPosition, Unboxed = true, PlaceOnTop = false };
        bool requiresBase = originalData.requiredBase != null;

        bool isValidInitialSpot = !cellsToOccupy.Any(cell =>
            (PlacementDatasInPosition.ContainsKey(cell) && !isItem) ||
            Physics2D.OverlapCircle(cell, 0.2f, isItem ? kitLayerMask : unplaceableLayerMask));

        if (isValidInitialSpot)
        {
            details.CanPlace = true;
            if (requiresBase)
                details.Unboxed = CheckKitRequirement(cellsToOccupy, originalData);
            return details;
        }

        if (!isItem)
        {
            Vector2 validBottomCell = Vector2.zero;
            PlacementData basePlacementData = null;
            bool firstCell = true;

            foreach (var cell in cellsToOccupy)
            {
                if (!PlacementDatasInPosition.TryGetValue(cell, out var currentCellData) || currentCellData.instantiatedFurniture == null)
                    return details;

                if (firstCell)
                {
                    validBottomCell = cell;
                    basePlacementData = currentCellData;
                    firstCell = false;
                }
                else if (currentCellData.instantiatedFurniture != basePlacementData.instantiatedFurniture)
                    return details;
            }

            if (basePlacementData != null && basePlacementData.instantiatedFurniture.IsUnpacked &&
                cellsToOccupy.All(c => basePlacementData.occupiedPositions.Contains(c)) &&
                basePlacementData.IsCompatibleWith(originalData) &&
                basePlacementData.HasFreePositions(cellsToOccupy))
            {
                details.CanPlace = true;
                details.PlaceOnTop = true;
                details.FinalWorldPosition = validBottomCell;
                if (requiresBase)
                    details.Unboxed = CheckKitRequirementForUnderlyingObject(validBottomCell, originalData);
            }
        }
        return details;
    }

    private void FinalizeAndInstantiateNonStacked(Vector2 finalWorldPosForInstantiation, FurnitureData furnitureData, FurnitureOriginalData originalData, List<Vector2> cellsEffectivelyOccupiedByNewItem, bool isItem, bool unboxed, bool placeOnTop)
    {
        Vector2 instantiationGridPos = GridManager.PositionToCellCenter(finalWorldPosForInstantiation);

        if (!placeOnTop && !isItem)
        {
            PlacementData newPlacementEntry = new PlacementData(cellsEffectivelyOccupiedByNewItem, furnitureData);
            foreach (var cell in cellsEffectivelyOccupiedByNewItem)
            {
                PlacementDatasInPosition[cell] = newPlacementEntry;
            }
        }

        FurnitureObjectBase furnitureInstance = Instantiate(furnitureData.prefab, instantiationGridPos, Quaternion.Euler(furnitureData.VectorRotation)).GetComponent<FurnitureObjectBase>();
        furnitureInstance.CopyFurnitureData(furnitureData);
        furnitureInstance.SetUnpackedState(unboxed);

        OnPlaceFurniture?.Invoke(originalData);
        if (placeOnTop) OnPlaceOnTopEvent?.Invoke(originalData);

        if (!isItem)
        {
            GivePoints(furnitureData, cellsEffectivelyOccupiedByNewItem, placeOnTop, instantiationGridPos, furnitureInstance, unboxed);
        }
        else
        {
            KitsInPosition[instantiationGridPos] = furnitureInstance as KitObject;
            if (furnitureInstance is KitObject placedKit)
            {
                UpdateOverlyingFurnitureOnKitPlacement(instantiationGridPos, placedKit);
            }
        }
    }

    private bool CheckKitRequirement(List<Vector2> cellsToCheck, FurnitureOriginalData originalData)
    {
        if (originalData.requiredBase == null) return true;
        return cellsToCheck.Any(cell => KitsInPosition.TryGetValue(cell, out KitObject kit) && originalData.requiredBase == kit.Data.originalData);
    }

    private bool CheckKitRequirementForUnderlyingObject(Vector2 baseFurniturePositionKey, FurnitureOriginalData itemToPlaceOriginalData)
    {
        if (itemToPlaceOriginalData.requiredBase == null) return true;
        if (!PlacementDatasInPosition.TryGetValue(baseFurniturePositionKey, out var basePlacementData)) return false;
        return basePlacementData.occupiedPositions.Any(cellOfBase => KitsInPosition.TryGetValue(cellOfBase, out KitObject kit) && itemToPlaceOriginalData.requiredBase == kit.Data.originalData);
    }

    #region Items
    public bool PlaceItem(Vector2 position, ItemData itemData, FurnitureData furnitureData)
    {
        bool placed;
        Room currentRoom = GetComponent<Room>();
        switch (itemData.type)
        {
            case ItemType.Tagger:
                if (currentRoom.roomTag is RoomTag.Shop) { placed = false; break; }
                TagSelectionUI.instance.ShowTagSelection(currentRoom, ChangedTagCallback);
                placed = true;
                break;
            case ItemType.Sledgehammer:
                var coll = Physics2D.OverlapCircle(position, 0.2f);
                if (coll != null && coll.TryGetComponent<DoorData>(out DoorData door) && !door.isUnlocked)
                {
                    door.BuyNextRoom(true);
                    OnItemUse?.Invoke(ItemType.Sledgehammer);
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

        bool[] availableWalls = { isWallUp, isWallLeft, isWallBottom, isWallRight };

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

    private void ApplyFirstTimePlacementBonus(FurnitureData furnitureData, FurnitureOriginalData originalData, bool isUnboxed)
    {
        if (!furnitureData.firstTimePlaced && isUnboxed)
        {
            PlayerController.instance.Inventory.UpdateMoney(originalData.price);
            House.instance.UpdateScore(originalData.price);
            furnitureData.firstTimePlaced = true;
        }
    }

    private int HandleStackedTagBonus(FurnitureData furnitureData, FurnitureOriginalData originalData, Vector2 popupPosition)
    {
        int tagBonus = 0;
        Room currentRoom = GetComponent<Room>();
        RoomTag furnitureTag = originalData.furnitureTag;

        if (furnitureTag != RoomTag.None && currentRoom != null)
        {
            currentRoom.TrySetRoomTagFromFurniture(furnitureTag);
        }

        if (currentRoom != null && furnitureTag != RoomTag.None &&
            furnitureTag == currentRoom.roomTag && !furnitureData.hasReceivedTagBonus)
        {
            tagBonus = originalData.tagMatchBonusPoints;
            furnitureData.hasReceivedTagBonus = true;
            PlayerController.instance.Inventory.UpdateMoney(tagBonus);
            House.instance.UpdateScore(tagBonus);
            ComboPopUp.Create(matchPrefab, tagBonus, popupPosition, new Vector2(0f, 1.5f));
        }
        return tagBonus;
    }

    private void HandleStackedComboPoints(BottomFurnitureObject stackReceiver, List<Vector2> positionsForComboCheck, FurnitureObjectBase newStackedItem, Vector2 popupPosition, int existingTagBonus)
    {
        int comboPoints = stackReceiver.MakeCombo(positionsForComboCheck.ToArray());

        if (comboPoints > 0 && newStackedItem is TopFurnitureObject topObject)
        {
            topObject.MakeCombo();
            PlayerController.instance.Inventory.UpdateMoney(comboPoints);
            House.instance.UpdateScore(comboPoints);
            ComboPopUp.Create(popUpPrefab, comboPoints, popupPosition, new Vector2(0f, 1.2f));
            OnComboDone?.Invoke(comboPoints);

            if (existingTagBonus > 0)
            {
                StartCoroutine(ShowMatchPopupDelayed(existingTagBonus, popupPosition));
            }

        }
    }

    private int ProcessTagLogicForNonStacked(FurnitureData data, FurnitureOriginalData originalData, Room currentRoom, Vector2 finalPos, bool placeOnTop)
    {
        int tagBonus = 0;
        RoomTag furnitureTag = originalData.furnitureTag;

        if (furnitureTag != RoomTag.None && currentRoom != null)
        {
            currentRoom.TrySetRoomTagFromFurniture(furnitureTag);
        }

        if (currentRoom != null && furnitureTag != RoomTag.None &&
            furnitureTag == currentRoom.roomTag && !data.hasReceivedTagBonus)
        {
            tagBonus = originalData.tagMatchBonusPoints;
            data.hasReceivedTagBonus = true;

            if (tagBonus > 0 && !placeOnTop)
            {
                PlayerController.instance.Inventory.UpdateMoney(tagBonus);
                House.instance.UpdateScore(tagBonus);
                ComboPopUp.Create(matchPrefab, tagBonus, finalPos, new Vector2(0f, 1.5f));
            }
        }
        return tagBonus;
    }

    private void ProcessComboForPlaceOnTop(FurnitureData data, List<Vector2> positionToOccupy, Vector2 finalPos, FurnitureObjectBase furnitureObject, int tagBonusIfAny)
    {
        TopFurnitureObject topObject = (TopFurnitureObject)furnitureObject;
        BottomFurnitureObject bottomObject = (BottomFurnitureObject)PlacementDatasInPosition[finalPos].instantiatedFurniture;

        topObject.CheckAndUpdateSprite(bottomObject);

        if (!topObject.ComboDone)
        {
            int comboPointsFromBase = bottomObject.MakeCombo(positionToOccupy.ToArray());
            if (comboPointsFromBase > 0)
            {
                topObject.MakeCombo();
                int totalPointsAwarded = comboPointsFromBase + tagBonusIfAny;

                PlayerController.instance.Inventory.UpdateMoney(totalPointsAwarded);
                House.instance.UpdateScore(totalPointsAwarded);

                ComboPopUp.Create(popUpPrefab, totalPointsAwarded, finalPos, new Vector2(0f, 1.2f));
                OnComboDone?.Invoke(comboPointsFromBase);

                if (tagBonusIfAny > 0 && comboPointsFromBase > 0)
                {
                    StartCoroutine(ShowMatchPopupDelayed(tagBonusIfAny, finalPos));
                }
            }
            else if (tagBonusIfAny > 0)
            {
                PlayerController.instance.Inventory.UpdateMoney(tagBonusIfAny);
                House.instance.UpdateScore(tagBonusIfAny);
                ComboPopUp.Create(matchPrefab, tagBonusIfAny, finalPos, new Vector2(0f, 1.2f));
            }
        }
    }

    protected virtual void GivePoints(FurnitureData data, List<Vector2> positionToOccupy, bool placeOnTop, Vector2 finalPos, FurnitureObjectBase furnitureObject, bool unboxed)
    {
        ApplyFirstTimePlacementBonus(data, data.originalData, unboxed);
        Room currentRoom = GetComponent<Room>();
        int tagBonus = 0;

        if (unboxed)
        {
            tagBonus = ProcessTagLogicForNonStacked(data, data.originalData, currentRoom, finalPos, placeOnTop);
        }

        if (!placeOnTop)
        {
            if (TryGetComponent(out MainRoom room))
            {
                MainRoom.instance.availableTiles -= data.originalData.size.x * data.originalData.size.y;
            }

            if (furnitureObject.originalData is not ItemData) PlacementDatasInPosition[finalPos].instantiatedFurniture = furnitureObject;
        }
        else
        {
            TopFurnitureObject topObject = (TopFurnitureObject)furnitureObject;
            if (unboxed)
            {
                ProcessComboForPlaceOnTop(data, positionToOccupy, finalPos, topObject, tagBonus);
            }

            // Updateamos la placement data con el objeto que pusimos encima
            PlacementDatasInPosition[finalPos].PlaceObjectOnTop(positionToOccupy, topObject);
        }
    }

    private IEnumerator ShowMatchPopupDelayed(int tagBonus, Vector2 position)
    {
        yield return new WaitForSeconds(0.7f);

        ComboPopUp.Create(matchPrefab, tagBonus, position, new Vector2(0f, 1f));
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

    public void ChangedTagCallback()
    {
        OnItemUse?.Invoke(ItemType.Tagger);
        Debug.Log("Changed tag");
    }

    public void RemoveTopObjectInPosition(Vector2 pos)
    {
        PlacementDatasInPosition[pos].ClearTopObject(pos);
    }

    public void RemoveKitInPosition(Vector2 pos)
    {
        if (!KitsInPosition.TryGetValue(pos, out KitObject kitToRemove)) return;

        FurnitureOriginalData removedKitOriginalData = kitToRemove.Data.originalData; 

        Destroy(kitToRemove.gameObject);
        KitsInPosition.Remove(pos);
        UpdateOverlyingFurnitureOnKitRemoval(pos, removedKitOriginalData);
    }

    private void UpdateOverlyingFurnitureOnKitPlacement(Vector2 placedKitCell, KitObject placedKit)
    {
        if (PlacementDatasInPosition.TryGetValue(placedKitCell, out PlacementData affectedData) &&
            affectedData.instantiatedFurniture != null)
        {
            var topFurniture = affectedData.GetTopFurnitureTopData(placedKitCell);
            FurnitureObjectBase furniture = (topFurniture != null && topFurniture.furnitureOnTopData.originalData.requiredBase) ? topFurniture.instantiatedFurnitureOnTop : affectedData.instantiatedFurniture;
            FurnitureData furnitureData = furniture.Data;
            FurnitureOriginalData originalFurnitureData = furnitureData.originalData;

            if (!furniture.IsUnpacked &&
                originalFurnitureData.requiredBase != null &&
                originalFurnitureData.requiredBase == placedKit.Data.originalData)
            {
                bool nowUnboxed = CheckKitRequirement(affectedData.occupiedPositions, originalFurnitureData);

                if (nowUnboxed)
                {
                    Debug.Log($"Kit placed under {originalFurnitureData.Name}. Updating its state to unboxed.");
                    furniture.SetUnpackedState(true);

                    ApplyFirstTimePlacementBonus(furnitureData, originalFurnitureData, true);

                    Room currentRoom = GetComponent<Room>();
                    Vector2 furnitureAnchorPos = GridManager.PositionToCellCenter(affectedData.occupiedPositions.First());

                    bool placeOnTop = topFurniture != null && furnitureData == topFurniture.furnitureOnTopData;
                    int tagBonus = ProcessTagLogicForNonStacked(furnitureData, originalFurnitureData, currentRoom, furnitureAnchorPos, placeOnTop);
                    if (placeOnTop) ProcessComboForPlaceOnTop(furnitureData, affectedData.occupiedPositions, furnitureAnchorPos, furniture, tagBonus);
                }
            }
        }
    }

    private void UpdateOverlyingFurnitureOnKitRemoval(Vector2 removedKitCell, FurnitureOriginalData removedKitType)
    {
        if (PlacementDatasInPosition.TryGetValue(removedKitCell, out PlacementData affectedData) &&
            affectedData.instantiatedFurniture != null)
        {
            FurnitureObjectBase furniture = affectedData.instantiatedFurniture;
            FurnitureData furnitureData = furniture.Data;
            FurnitureOriginalData originalFurnitureData = furnitureData.originalData;

            if (furniture.IsUnpacked && 
                originalFurnitureData.requiredBase != null &&
                originalFurnitureData.requiredBase == removedKitType) 
            {
                bool stillUnboxed = CheckKitRequirement(affectedData.occupiedPositions, originalFurnitureData);

                if (!stillUnboxed)
                {
                    Debug.Log($"Kit removed from under {originalFurnitureData.Name}. Updating its state to boxed.");
                    furniture.SetUnpackedState(false);
                }
            }
        }
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