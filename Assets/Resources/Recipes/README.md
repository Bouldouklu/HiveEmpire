# Honey Recipe System

## Overview
This folder contains HoneyRecipe ScriptableObjects that define recipes for honey production at the beehive.

## Recipe Configuration

Each recipe defines:
- **Recipe Name**: Display name
- **Description**: Flavor text
- **Ingredients**: List of pollen types and quantities required
- **Production Time**: Seconds to produce honey
- **Honey Value**: Income earned ($) when completed
- **Honey Color**: Visual tint for particles and UI

## Pollen Types (ResourceType enum)
- `0` = ForestPollen (Forest biome)
- `1` = PlainsPollen (Plains biome)
- `2` = MountainPollen (Mountain biome)
- `3` = DesertPollen (Desert biome)
- `4` = CoastalPollen (Coastal biome)
- `5` = TundraPollen (Tundra biome)

## Sample Recipes

### Simple Recipes (Quick, Low Value)
- **Simple Forest Honey**: 1 Forest pollen → $2 in 5s
- **Wildflower Honey**: 1 Plains pollen → $2 in 5s

### Medium Recipes
- **Mountain Honey**: 2 Mountain pollen → $10 in 10s

### Advanced Recipes (Combos)
- **Desert Blossom**: 1 Forest + 1 Desert → $25 in 15s
- **Premium Blend**: 1 of each type → $150 in 30s

## Creating New Recipes

1. Right-click in this folder
2. Create → Game → Honey Recipe
3. Configure ingredients, time, and value
4. Add recipe to Hive's RecipeProductionManager in Inspector

## Priority System

Recipes are assigned to the RecipeProductionManager in priority order:
- **Top of list** = Highest priority (gets ingredients first)
- **Bottom of list** = Lowest priority (runs only if resources remain)
- Multiple recipes run simultaneously if ingredients are available
- Higher priority recipes consume resources first when inventory is limited

## Storage System

- Hive has storage cap per pollen type (default: 100)
- Storage is upgradeable via `HiveController.UpgradeStorageCapacity()`
- Overflow pollen is discarded when storage is full
- Recipes automatically consume from inventory when ingredients available

## Integration

The recipe system integrates with:
- `HiveController.cs` - Pollen inventory and storage
- `RecipeProductionManager.cs` - Recipe production logic
- `HoneyProductionVFX.cs` - Visual effects and income popups
- `EconomyManager.cs` - Income generation
