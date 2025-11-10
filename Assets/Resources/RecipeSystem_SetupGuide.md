# Recipe System Setup Guide

## Overview
This guide walks you through setting up the new recipe-based honey production system in Unity Editor.

## What Changed
The beehive now operates as a production facility:
- **Before**: Pollen delivered → instant money
- **After**: Pollen stored → recipes consume pollen → honey produced → money earned

## Setup Steps

### 1. Update Hive GameObject

Find the Hive GameObject in your scene (should already have `HiveController` component).

**Add these components:**
1. `RecipeProductionManager` component
2. `HoneyProductionVFX` component

**Configure RecipeProductionManager:**
1. In the Inspector, find "Active Recipes" list
2. Set the size to 5 (or however many recipes you want)
3. Drag recipe assets from `Assets/Resources/Recipes/` into the slots
4. **Order matters!** Top = highest priority, bottom = lowest priority

**Recommended Recipe Priority Order:**
1. Simple Forest Honey (quick income)
2. Wildflower Honey (quick income)
3. Mountain Honey (medium value)
4. Desert Blossom (combo, good value)
5. Premium Blend (ultimate combo, requires all types)

**Configure HoneyProductionVFX:**
- Leave "Create Particles At Runtime" checked (auto-creates particle system)
- Or assign your own particle system if preferred
- Income Popup Prefab is optional (system works without it)

### 2. Verify HiveController Settings

Select the Hive GameObject and check `HiveController` component:

**Storage Settings:**
- `Default Storage Capacity`: Set to 100 (already configured)
- This is the starting capacity per pollen type
- Can be upgraded at runtime via code

### 3. Test in Play Mode

Press Play and observe:

**Expected Behavior:**
1. Bees deliver pollen to hive
2. Pollen count increases in HUD (shows as "ResourceType: X/100")
3. When recipe ingredients are available, production starts automatically
4. After production time elapses:
   - Particle effects play at hive
   - Money increases by recipe value
   - Recipe can start again if ingredients available

**HUD Display:**
- Resources now show as "ForestPollen: 25/100"
- Color coding:
  - White: Normal
  - Yellow: 80%+ full
  - Red: At capacity
- When full, overflow pollen is discarded (warning in console)

### 4. Debug Recipe Production

**Console Messages to Look For:**
- "Started production: [Recipe Name] ($X, Ys)"
- "Completed recipe: [Recipe Name] - Earned $X"
- "Consumed resources for recipe: [Recipe Name]"
- "Storage full for [Resource]! Discarded 1 pollen." (if storage full)

**Common Issues:**

**Problem**: Recipes never start
- **Solution**: Check if pollen is being delivered (watch HUD counts)
- **Solution**: Verify recipes are assigned in RecipeProductionManager
- **Solution**: Check recipe ingredients match available pollen types

**Problem**: No visual effects
- **Solution**: Check HoneyProductionVFX is attached to Hive GameObject
- **Solution**: Look for "Created default particle system" message in console
- **Solution**: Particle system might be hidden behind other objects

**Problem**: Money not increasing
- **Solution**: Verify EconomyManager exists in scene
- **Solution**: Check console for "EconomyManager not found" warnings
- **Solution**: Recipe production might not be completing (check timers)

### 5. Testing Priority System

To test priority-based resource allocation:

1. Set up multiple flower patches (different biomes)
2. Let pollen accumulate
3. Note which recipes run first (should be top of list)
4. When low on resources, only high-priority recipes run

**Example Test:**
- Have 2 Forest pollen, 1 Desert pollen
- Desert Blossom needs: 1 Forest + 1 Desert
- Simple Forest Honey needs: 1 Forest
- If Desert Blossom is higher priority → it consumes 1 Forest + 1 Desert first
- Then Simple Forest Honey can use remaining 1 Forest

### 6. Testing Storage Limits

**To test storage cap:**
1. Set Default Storage Capacity to low value (e.g., 10) in Inspector
2. Play and let bees deliver pollen
3. Watch for overflow warnings when storage fills
4. Notice pollen count stops at cap (e.g., "ForestPollen: 10/10")
5. Additional deliveries are discarded

**To test upgrades (via code):**
```csharp
// In a test script or console command:
HiveController.Instance.UpgradeStorageCapacity(ResourceType.ForestPollen, 50);
// Storage for ForestPollen is now 150 (100 + 50)
```

### 7. Performance Testing

**Target Performance:**
- Recipe checking runs in Update() loop
- Should handle 5-10 recipes without performance impact
- Production timers are lightweight

**Monitor:**
- Unity Profiler → Scripts → RecipeProductionManager.Update()
- Should be < 0.1ms per frame

### 8. Customizing Recipes

**To create new recipes:**
1. Right-click in `Assets/Resources/Recipes/`
2. Create → Game → Honey Recipe
3. Configure:
   - Recipe Name: Display name
   - Ingredients: Add entries, set type and quantity
   - Production Time: Seconds to produce
   - Honey Value: Money earned
   - Honey Color: Visual tint
4. Add to Hive's RecipeProductionManager list

**Design Tips:**
- Simple recipes (1 ingredient): $1-5, 5-10s
- Two-ingredient combos: $10-25, 10-20s
- Three-ingredient combos: $40-60, 20-30s
- Six-ingredient combo: $150+, 30-60s

### 9. Integration with Existing Systems

**DemandManager:**
- Still tracks deliveries
- Still calculates payment multipliers
- However, money comes from recipes now, not direct delivery
- Consider making DemandManager optional or removing it

**EconomyManager:**
- No changes needed
- Receives money from `RecipeProductionManager` instead of `HiveController`

**BeeController:**
- No changes needed
- Still delivers to `HiveController.ReceiveResources()`

### 10. Optional: Create Income Popup Prefab

For better visual feedback:

1. Create new GameObject in scene
2. Add `TextMeshPro - Text` component (3D text)
3. Configure:
   - Font Size: 3
   - Alignment: Center
   - Color: Yellow/Gold
4. Save as Prefab in `Assets/Prefabs/IncomePopup.prefab`
5. Assign to HoneyProductionVFX → Income Popup Prefab

The VFX script will instantiate this, set the text to "+$X", and animate it rising and fading.

## Summary

**Core Flow:**
```
Bee Delivers Pollen
    ↓
HiveController.ReceiveResources()
    ↓
Pollen added to inventory (up to storage cap)
    ↓
RecipeProductionManager.Update() checks recipes
    ↓
Recipe with ingredients available starts production
    ↓
Timer counts down (production time)
    ↓
Recipe completes
    ↓
HoneyProductionVFX plays particles
    ↓
EconomyManager.EarnMoney() called
    ↓
Money increases!
```

**Files Modified:**
- `HiveController.cs` - Storage management, no longer calculates money
- `HUDController.cs` - Shows inventory with capacity

**Files Added:**
- `HoneyRecipe.cs` - Recipe data structure
- `RecipeProductionManager.cs` - Production logic
- `HoneyProductionVFX.cs` - Visual effects
- Recipe assets in `Assets/Resources/Recipes/`

## Next Steps

After confirming basic functionality works:

1. **Balance recipes** - Adjust times and values
2. **Create more recipes** - Explore different combinations
3. **Add upgrade system** - UI for increasing storage capacity
4. **Recipe unlocks** - Lock advanced recipes behind progression
5. **Visual polish** - Custom particles, better income popups
6. **Recipe management UI** - Let players enable/disable recipes
7. **Consider removing DemandManager** - No longer needed for income

Good luck! 🐝🍯
