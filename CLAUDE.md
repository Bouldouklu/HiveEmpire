# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Hive Empire** - An incremental strategy resource management game where players build automated pollen networks connecting resource-producing flower patches to a central hive. Players optimize production chains, manage a global bee fleet, and discover resource synergies for exponential growth across seasonal campaigns.

- **Genre:** Incremental Strategy + Resource Management
- **Style:** Minimalist 3D, top-down orthographic view (Mini Metro aesthetic)
- **Target Platform:** WebGL (browser-based, itch.io)
- **Unity Version:** 6000.2.10f1 (Unity 6)
- **Render Pipeline:** URP (Universal Render Pipeline)

## Development Commands

### Unity Editor Testing
```
1. Open Assets/Scenes/GameScene.unity (main) or GameSceneTest.unity (testing)
2. Press Play button in Unity Editor
3. Test gameplay mechanics in Game view
4. Use keyboard shortcuts for time control:
   - 1: Normal speed (1x)
   - 2: Double speed (2x)
   - 3: Fast testing (5x)
```

### Building for WebGL
```
File → Build Settings → WebGL → Build
```
Target 60 FPS with 100+ bees, 50+ FPS with 200+ bees. Use object pooling if needed.

### Version Control
- `.meta` files ARE tracked - never exclude them
- Unity Library folder is gitignored
- Scene changes should be committed carefully to avoid merge conflicts

## Architecture Overview

### Core Systems & Managers (Singletons)

The game uses a **manager-driven architecture** with singleton managers orchestrating different systems:

**GameManager** (`GameManager.cs`)
- Central coordinator for all game systems
- Manages game speed (Time.timeScale) for testing
- Handles year reset flow (ResetYear method)
- Tracks global statistics (bee count, elapsed time)
- Coordinates all manager resets in proper order

**EconomyManager** (`EconomyManager.cs`)
- Manages player money (earning/spending)
- Fires OnMoneyChanged events for UI updates
- Handles transaction validation and logging

**BeeFleetManager** (`BeeFleetManager.cs`)
- Global bee pool system (core strategic constraint)
- Tracks total bees owned vs allocated per flower patch
- Enforces capacity limits per flower patch
- Fires allocation events for UI synchronization

**RecipeProductionManager** (`RecipeProductionManager.cs`)
- Priority-based recipe production system
- Consumes pollen from hive inventory to produce honey
- Handles production timers with seasonal modifiers
- Generates income when recipes complete

**SeasonManager** (`SeasonManager.cs`)
- Campaign progression (Spring → Summer → Autumn → Winter)
- Applies global modifiers (income, bee speed, production time, storage)
- Triggers end-of-year summary at Week 25 (Winter arrival)
- Week-based time progression (default: 60 seconds = 1 week)

**YearStatsTracker** (`YearStatsTracker.cs`)
- Event-driven statistics tracking across the year
- Monitors money earned, recipes completed, resources delivered
- Tracks per-season breakdowns
- Provides data for end-of-year summary panel

**AchievementManager** / **HighScoreManager**
- Achievement unlocking system
- High score persistence across playthroughs

**HiveController** (`HiveController.cs`)
- Central hub receiving pollen deliveries
- Manages pollen inventory with capacity limits
- Provides resources to RecipeProductionManager
- Singleton instance for global access

### Data-Driven Architecture (ScriptableObjects)

**FlowerPatchData** (`Assets/Resources/FlowerPatchData/*.asset`)
- Defines biome type, placement cost, prefab reference
- Upgrade costs for nectar flow tiers (3 tiers per patch)
- Capacity upgrade costs and bonus capacity per upgrade
- Base capacity and bees granted per upgrade
- Located: `Assets/Resources/FlowerPatchData/`

**HoneyRecipe** (`Assets/Resources/Recipes/*.asset`)
- Defines ingredients (pollen type + quantity)
- Production time and honey value (money output)
- CanProduce() checks inventory availability
- Located: `Assets/Resources/Recipes/`

**SeasonData** (`Assets/Resources/Seasons/*.asset`)
- Seasonal modifiers (income, bee speed, production time, storage)
- Applied globally by SeasonManager

**BiomeMaterialMapper** (`Assets/Resources/BiomeMaterialMapper.asset`)
- Maps BiomeType enum to Unity Materials for visual consistency

### Game Entities

**FlowerPatchController** (`FlowerPatchController.cs`)
- Generates pollen based on biome type
- Spawns BeeController instances for deliveries
- Tracks upgrade state (nectar flow tier, capacity upgrades)
- Linked to FlowerPatchData ScriptableObject for configuration

**BeeController** (`BeeController.cs`)
- Automated pathfinding from flower patch → hive
- Carries colored pollen cubes (visual feedback)
- Returns to flower patch after delivery
- Fully automated (no player control)

**RouteController** (`RouteController.cs`)
- Visualizes pollen delivery routes between patches and hive
- Color-coded by biome type

### UI System

**HUDController** (`Assets/Scripts/UI/HUDController.cs`)
- Main game HUD (money, bee fleet status, inventory)
- Subscribes to manager events for real-time updates

**FlowerPatchUpgradePanel** / **FleetManagementPanel**
- Upgrade interfaces for flower patches
- Bee allocation controls

**RecipeDisplayPanel** (`Assets/Scripts/UI/RecipeDisplayPanel.cs`)
- Shows active recipes, ingredients, production progress

**EndOfYearPanel** (`Assets/Scripts/UI/EndOfYearPanel.cs`)
- End-of-campaign summary (hero stats, seasonal breakdown, achievements)
- High score comparison
- "Play Again" restart flow

**SeasonUI** (`Assets/Scripts/UI/SeasonUI.cs`)
- Current season display and week counter

### Key Architectural Patterns

**Event-Driven Communication:**
- UnityEvents for manager-to-UI communication
- Example: `EconomyManager.OnMoneyChanged` → HUD updates
- Decouples systems for maintainability

**ScriptableObject Data Design:**
- All configuration data externalized (flower patches, recipes, seasons)
- No hardcoded balance values in scripts
- Create new content via Unity menu: `Game/Flower Patch Data`, `Game/Honey Recipe`

**Global Bee Pool System:**
- Strategic constraint forcing meaningful choices
- BeeFleetManager enforces allocation limits
- Flower patches compete for limited bee capacity
- Capacity can be upgraded per patch

**Priority-Based Recipe Production:**
- RecipeProductionManager checks recipes in list order
- Automatically consumes ingredients when available
- Multiple recipes can produce simultaneously

**Seasonal Campaign Structure:**
- Fixed 24-week campaign (3 seasons × 8 weeks)
- Modifiers force strategy adaptation each season
- End-of-year summary provides closure and replayability

## Important Files & Locations

### Core Scripts
- **Managers:** `Assets/Scripts/GameManager.cs`, `EconomyManager.cs`, `BeeFleetManager.cs`, `RecipeProductionManager.cs`, `SeasonManager.cs`
- **Entities:** `Assets/Scripts/FlowerPatchController.cs`, `BeeController.cs`, `HiveController.cs`
- **Data Types:** `Assets/Scripts/GameTypes.cs` (enums: BiomeType, ResourceType)

### ScriptableObject Data
- **Flower Patches:** `Assets/Resources/FlowerPatchData/*.asset` (6 biomes: Forest, Plains, Mountain, Desert, Coastal, Tundra)
- **Recipes:** `Assets/Resources/Recipes/*.asset` (ForestHoney, WildflowerHoney, MountainHoney, DesertBlossom, PremiumBlend)
- **Seasons:** `Assets/Resources/Seasons/*.asset` (Spring, Summer, Autumn modifiers)
- **Achievements:** `Assets/Resources/Achievements/*.asset`

### Design Documentation
- **Game Concept:** `Assets/Readme.md` (complete game design, mechanics, progression)
- **Todo List:** `Assets/Resources/ToDo.md` (active development tasks and bugs)

### Scenes
- **Main Game:** `Assets/Scenes/GameScene.unity`
- **Testing:** `Assets/Scenes/GameSceneTest.unity`

### Prefabs & Assets
- **Prefabs:** `Assets/Prefabs/` (Bee prefab, flower patch prefabs per biome)
- **Materials:** `Assets/Material/` (biome-specific materials, UI materials)
- **VFX:** `Assets/FX/PolygonParticleFX/` (third-party particle effects)

## Common Development Tasks

### Adding a New Biome
1. Add enum value to `BiomeType` in `GameTypes.cs`
2. Add enum value to `ResourceType` in `GameTypes.cs`
3. Create material in `Assets/Material/` for visual consistency
4. Create ScriptableObject: Right-click → `Game/Flower Patch Data`
   - Set biome type, costs, prefab reference
5. Add to `BiomeMaterialMapper` asset for automatic material assignment
6. Create recipe(s) using the new pollen type
7. Test placement, resource generation, and recipe production

### Adding a New Recipe
1. Create ScriptableObject: Right-click → `Game/Honey Recipe`
2. Define ingredients (pollen types + quantities)
3. Set production time and honey value
4. Add to `RecipeProductionManager.activeRecipes` list in scene
5. Order in list determines production priority
6. Test ingredient consumption and income generation

### Modifying Flower Patch Upgrade Costs
1. Open FlowerPatchData asset in `Assets/Resources/FlowerPatchData/`
2. Modify `upgradeCosts` array (3 values for 3 tiers)
3. Modify `beesPerUpgrade` (bees added to pool per tier)
4. Modify `capacityUpgradeCost` and `bonusCapacityPerUpgrade`
5. Changes apply immediately to all patches of that type

### Adjusting Seasonal Modifiers
1. Open SeasonData asset in `Assets/Resources/Seasons/`
2. Modify multipliers:
   - `incomeModifier`: Affects honey value
   - `beeSpeedModifier`: Affects delivery speed
   - `productionTimeModifier`: Affects recipe production time
   - `storageCapacityModifier`: Affects hive inventory capacity
3. Changes apply immediately when season is active

### Debugging Production Issues
1. Check `RecipeProductionManager` active recipes list order (priority matters)
2. Verify hive inventory has sufficient pollen (use HUD display)
3. Check seasonal modifiers affecting production time
4. Enable verbose logging in `RecipeProductionManager` if needed
5. Use game speed hotkeys (1/2/3) to speed up testing

### Restarting Year Campaign
- `GameManager.ResetYear()` handles full game reset
- Resets all managers in proper order
- Destroys all flower patch GameObjects
- Resets season to Spring Week 1
- Called by EndOfYearPanel "Play Again" button

## Performance Considerations

### Target Performance
- 60 FPS with 100+ bees
- 50+ FPS with 200 bees
- WebGL optimized (URP pipeline)

### Optimization Strategies
- Object pooling for bees if performance degrades
- Batch material instances where possible
- Minimize Update() loops - prefer event-driven architecture
- Use Time.timeScale for controlled testing (not gameplay speed feature)

### Known Performance Notes
- BeeController uses simple pathfinding (straight lines)
- RouteController uses LineRenderer (watch for overdraw)
- Seasonal modifiers applied globally (no per-entity calculations)

## Code Style & Standards

### Unity 6 API Usage
- Use `FindFirstObjectByType<T>()` instead of deprecated `FindObjectOfType<T>()`
- Use `FindAnyObjectByType<T>()` when any instance is acceptable
- Example: `GameManager.ResetYear()` uses `FindObjectsByType<FlowerPatchController>(FindObjectsSortMode.None)`

### Singleton Pattern
- All managers use singleton pattern with `Instance` property
- Awake() enforces single instance, destroys duplicates
- OnDestroy() cleans up `Instance` reference

### Event Communication
- Use UnityEvents for decoupled communication
- Managers fire events, UI subscribes
- Example: `BeeFleetManager.OnBeeAllocationChanged` → `FleetManagementPanel` updates

### ScriptableObject Configuration
- Never hardcode balance values in C# scripts
- All costs, timers, and multipliers in ScriptableObject assets
- Use OnValidate() for editor-time validation

### Naming Conventions
- PascalCase for classes, methods, public properties
- camelCase for private fields, parameters
- Prefix private fields with underscore NOT used in this codebase
- Descriptive names over abbreviations

### Documentation Standards
- XML comments (`///`) for public APIs
- Tooltip attributes for serialized fields
- Explain "why" in comments, not "what" (code should be self-documenting)

## Design Philosophy

### Critical Design Pillars
1. **Satisfying Automation** - Watch network operate without micromanagement
2. **Exponential Growth** - Income scales dramatically through synergies
3. **Meaningful Choices** - Global bee pool and upgrades create strategy
4. **Visible Optimization** - Bottlenecks are clear, solutions are multiple
5. **Rapid Prototyping** - Validate core mechanics quickly, expand if successful

### Strategic Depth
- **Global Bee Pool:** Limited bees force allocation decisions
- **Recipe Priority:** List order determines production priority
- **Upgrade Paths:** Nectar flow (more bees) vs Capacity (more slots)
- **Seasonal Modifiers:** Force strategy adaptation every 8 weeks
- **Resource Synergies:** Multi-pollen recipes exponentially more valuable

### Replayability
- 20-30 minute campaign length encourages multiple runs
- High score tracking and achievements
- Seasonal variety changes optimal strategies
- No single "correct" build path

## Known Issues & Todo

See `Assets/Resources/ToDo.md` for current development priorities and bug tracking.

## Third-Party Assets

### Editor Plugins
- **AssetInventory** - Asset management tool
- **TableForge** - Data table editor
- **vFolders** - Visual folder organization
- **vHierarchy** - Enhanced hierarchy view
- **HotReload** - Code hot reload for faster iteration

### Runtime Assets
- **PolygonParticleFX** - Particle effects (`Assets/FX/PolygonParticleFX/`)

## MCP Server Integration

This project uses **UnityMCP** for Claude Code integration. Key workflows:

### Checking Editor State
- Use `unity://` resource URIs to read script contents
- Check `editor_state` resource for compilation status before testing
- Use `read_console` tool to monitor errors after script changes

### Script Management
- Prefer `script_apply_edits` for structured edits (method/class operations)
- Use `apply_text_edits` for precise character-range replacements
- Always verify content with `read_resource` before editing
- Check `read_console` after modifications for compilation errors

### Scene Management
- Use `manage_scene` actions: get_hierarchy, get_active, load, save
- Use `manage_gameobject` for runtime GameObject manipulation
- Always include Camera and Directional Light in new scenes

### Asset Operations
- Use `manage_asset` for prefab/material/ScriptableObject operations
- Paths relative to `Assets/` folder
- Use forward slashes for cross-platform compatibility

### Play Mode Control
- Use `manage_editor` action "play" to enter Play Mode
- Wait for compilation to finish before entering Play Mode
- Monitor `editor_state.isCompiling` to avoid domain reload issues
