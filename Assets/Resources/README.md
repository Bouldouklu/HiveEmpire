# FlowerPatchData Setup Instructions

## Refactoring Complete! ✅

The flower patch system has been successfully refactored to use ScriptableObjects for configuration instead of dynamic cost scaling.

## What Changed

### Code Changes (Already Done)
- ✅ Created `FlowerPatchData.cs` ScriptableObject class
- ✅ Updated `FlowerPatchPlaceholder.cs` to use FlowerPatchData
- ✅ Updated `EconomyManager.cs` - removed cost scaling logic
- ✅ Updated `FlowerPatchController.cs` - added InitializeFromData() method
- ✅ Added `GetRegisteredFlowerPatchCount()` to BeeFleetManager
- ✅ Fixed `YearStatsTracker.cs` to use new flower patch counting method
- ✅ Created `FlowerPatchDataGenerator.cs` editor script

### What This Achieves
- 🎯 **Fixed Costs**: Each flower patch has a fixed, designer-defined cost (no more scaling)
- 🎯 **Flexibility**: Can have multiple patches of same biome type with different costs
- 🎯 **Centralized Config**: All flower patch properties in one ScriptableObject
- 🎯 **Designer-Friendly**: Easy to tweak costs and properties in Inspector

## Next Steps (Manual - Must Do in Unity Editor)

### Step 1: Generate FlowerPatchData Assets
1. Open the project in Unity Editor
2. In the menu bar, click: **Assets > Hive Empire > Generate FlowerPatchData Assets**
3. A dialog will appear confirming that 6 assets were created
4. Verify assets exist at: `Assets/Resources/FlowerPatchData/`
   - ForestFlowerPatchData.asset (Cost: $10)
   - PlainsFlowerPatchData.asset (Cost: $10)
   - MountainFlowerPatchData.asset (Cost: $20)
   - CoastalFlowerPatchData.asset (Cost: $20)
   - DesertFlowerPatchData.asset (Cost: $30)
   - TundraFlowerPatchData.asset (Cost: $30)

### Step 2: Update Placeholder Prefabs
Update each of the 6 placeholder prefabs to reference their FlowerPatchData:

**Location**: `Assets/Prefabs/BiomePlaceholders/`

For each placeholder prefab:
1. Open the prefab in the Inspector
2. Find the **FlowerPatchPlaceholder** component
3. **Remove** the old fields (if visible):
   - ~~Biome Type~~
   - ~~Flower Patch Prefab~~
4. **Assign** the new field:
   - **Flower Patch Data** → Drag the corresponding FlowerPatchData asset from `Assets/Resources/FlowerPatchData/`

**Mapping**:
- `ForestFlowerPatchPlaceholder.prefab` → `ForestFlowerPatchData.asset`
- `PlainsFlowerPatchPlaceholder.prefab` → `PlainsFlowerPatchData.asset`
- `MountainFlowerPatchPlaceholder.prefab` → `MountainFlowerPatchData.asset`
- `DesertFlowerPatchPlaceholder.prefab` → `DesertFlowerPatchData.asset`
- `CoastalFlowerPatchPlaceholder.prefab` → `CoastalFlowerPatchData.asset`
- `TundraFlowerPatchPlaceholder.prefab` → `TundraFlowerPatchData.asset`

5. Keep the **Bee Prefab** field unchanged (still needed)
6. Save the prefab

### Step 3: Test in Play Mode
1. Open `GameScene.unity`
2. Enter Play Mode
3. Test each biome type:
   - Hover over placeholder → Should show green (affordable) or red (unaffordable)
   - Click to place → Should deduct correct cost ($10/$20/$30)
   - Verify flower patch spawns correctly
   - Check upgrade costs work (Tier 1: $50, Tier 2: $150, Tier 3: $400)

## Expected Behavior

### Placement Costs (Fixed, No Scaling)
- **Common Biomes** (Forest, Plains): $10
- **Medium Biomes** (Mountain, Coastal): $20
- **Rare Biomes** (Desert, Tundra): $30

### Upgrade Costs (From FlowerPatchData)
- **Nectar Flow Tier 1**: $50 (+2 bees to global pool)
- **Nectar Flow Tier 2**: $150 (+2 bees to global pool)
- **Nectar Flow Tier 3**: $400 (+2 bees to global pool)
- **Capacity Upgrade**: $100 (+5 bonus capacity)

## Creating Additional Flower Patches

Want to add a new flower patch variant? (e.g., "Rare Forest Patch" with higher cost)

1. **Create New ScriptableObject**:
   - Right-click in Project → Create → Hive Empire → Flower Patch Data
   - Name it descriptively (e.g., `ForestFlowerPatch_Rare`)

2. **Configure Properties**:
   - Set `biomeType` to Forest
   - Set `displayName` to "Rare Forest Patch"
   - Set `placementCost` to your desired cost (e.g., 50)
   - Assign `flowerPatchPrefab` to `ForestFlowerPatch.prefab`
   - Configure upgrade costs as needed

3. **Create/Update Placeholder**:
   - Duplicate existing placeholder prefab
   - Assign your new FlowerPatchData asset
   - Place in scene

## Troubleshooting

### Issue: "Cannot afford" message shows wrong cost
- **Fix**: Make sure placeholder prefab has correct FlowerPatchData assigned

### Issue: Upgrades cost wrong amount
- **Fix**: Check the `upgradeCosts` array in the FlowerPatchData asset

### Issue: Flower patch doesn't spawn
- **Fix**: Verify `flowerPatchPrefab` is assigned in FlowerPatchData asset

### Issue: Missing FlowerPatchData field in placeholder
- **Fix**: Re-open Unity to recompile scripts, or reimport FlowerPatchPlaceholder.cs

## Support

If you encounter issues after following these steps:
1. Check Unity Console for error messages
2. Verify all placeholder prefabs have FlowerPatchData assigned
3. Ensure the 6 FlowerPatchData assets were generated correctly
