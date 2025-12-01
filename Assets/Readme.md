# ReadMe - HIVE EMPIRE - Technical Reference & Game Design

**Version:** v1.0 (Production Release)
**Target:** WebGL browser-based incremental strategy game
**Core Focus:** Incremental optimization with strategic resource management
**Itch command line** to upload new build: butler push "D:\Unity\Games\HiveEmpire\Build\WebGL\hiveempire.zip" bouldouklu/hiveempire:HTML

---

## High-Level Concept

> **Build an automated pollen network, connecting resource-producing hexagonal flower patch regions to your central hive.
> Discover synergies, manage bottlenecks, and optimize production chains for exponential growth.**

**Genre:** Incremental Strategy + Resource Management
**Style:** Minimalist 3D, top-down orthographic view (Mini Metro aesthetic)
**Platform:** WebGL (browser-based, deployed to itch.io)
**Core Appeal:** Satisfying optimization, exponential growth, strategic choices

---

## The Core Hook

**IS about:** Identifying bottlenecks, strategic upgrades, synergies optimization between resources gathering, recipe production and unlocking/upgrades choices
**NOT about:** Perfect flight path routing
**Player Fantasy:** "I'm building an efficient pollen empire by managing complexity, bottlenecks and optimizing the production chain."

---

## Core Game Loop

```
BiomeRegion (7-hex cluster) generates Resources (different types of pollen)
    ↓
Bee automatically delivers to Hive
    ↓
Hive COMBINES 1 or several resources → Honey is produced for money
    ↓
Player earns exponential Money
    ↓
Player spends on: New BiomeRegions, Recipe Unlocks, Recipe Upgrades, Capacity Upgrades, More Bees
    ↓
Player optimizes bottlenecks, balances gathering rates vs production rates
    ↓
LOOP with increasing complexity (more biomes, more recipes, more upgrades, more bottlenecks)
```

---

## Win Condition

**Goal:** Maximize income & growth over one campaign: Spring, Summer, Autumn (21 weeks total)
**Campaign End:** Week 22 = Winter arrival = End-of-Year Summary Panel
**Victory Metrics:** Total money earned, recipes completed, resources collected (tracked per season)
**Replayability:** High score persistence, "Play Again" to restart campaign

---

## Core Mechanics

### 1. RESOURCE SYNERGY SYSTEM (The Heart)

Resources have **exponential value** when combined at the hive.

#### Implemented Recipe System

**13 fully implemented recipes with 5-tier upgrade system (Tiers 0-4)**

**Single-Resource Recipes (Base Income):**
```
1. WildMeadowHoney - Wild Meadow Pollen only
2. OrchardHoney - Orchard Pollen only
3. GardenHoney - Cultivated Garden Pollen only
4. HarvestHoney - Agricultural Field Pollen only
5. MarshlandHoney - Marsh Pollen only
6. ForestHoney - Forest Edge Pollen only
```

**Multi-Resource Recipes (Exponential Income):**
```
7. PastoralHoney - 2 resource types
8. AbundantHoney - 3 resource types
9. PrimordialHoney - 3 resource types (different combination)
10. CountrysideHoney - 4 resource types
11. TerroirHoney - 4 resource types (different combination)
12. ConvergenceHoney - 5 resource types
13. ConcordenceHoney - All 6 resource types (ultimate recipe)
```

**Recipe Progression System:**
- Managed by **RecipeProgressionManager** (`Assets/Scripts/RecipeProgressionManager.cs`)
- Recipes unlock via prerequisite chains (must unlock lower-tier recipes first)
- Each recipe has unlock cost (one-time payment)
- **5 upgrade tiers per recipe (0-4):**
  - Tier 0: Base recipe (default upon unlock)
  - Tiers 1-4: Progressive upgrades modifying:
    - Ingredient quantity requirements (reduction %)
    - Production time (reduction %)
    - Honey value output (increase %)
- Defined in `HoneyRecipe` ScriptableObject assets (`Assets/Resources/Recipes/`)

**Strategic Depth:**
- Early: Unlock single-resource recipes, build basic income
- Mid: Unlock multi-resource recipes, upgrade tiers to avoid bottlenecks
- Late: Balance bee allocation, region unlocks, recipe upgrades to maximize growth

---

### 2. BIOMES & RESOURCES

#### Implemented Biomes (6 Total)

**Starting Biomes (Always Available):**
- **WildMeadow** → Wild Meadow Pollen (fast gathering, close to hive)

**Unlockable Biomes (Choose Unlock Order):**
- **Orchard** → Orchard Pollen (fast gathering, close to hive) [low $ to unlock]
- **CultivatedGarden** → Cultivated Garden Pollen (medium gathering, moderate distance) [medium $ to unlock]
- **AgriculturalField** → Agricultural Field Pollen (slow gathering, distant) [high $ to unlock]
- **Marsh** → Marsh Pollen (slow gathering, distant) [high $ to unlock]
- **ForestEdge** → Forest Edge Pollen (medium gathering, moderate distance) [medium $ to unlock]

**Technical Implementation:**
- Biome types defined in `BiomeType` enum (`Assets/Scripts/GameTypes.cs`)
- Each biome has dedicated ScriptableObject data (`Assets/Resources/BiomeRegionData/*.asset`)
- Visual consistency enforced by `FlowerPatchMaterialMapper.asset` (centralized color/material mapping)

---

### 3. BIOME REGIONS & HEXAGONAL TILES

#### Architecture: Region-Based Multi-Hex System

**BiomeRegion** (`Assets/Scripts/BiomeRegion.cs`):
- Core entity managing pollen production and bee allocation
- Composed of **7 hexagonal tiles** arranged in cluster formation
- Unified unlock/upgrade mechanics (entire region purchased at once)
- Round-robin bee spawn distribution across tiles for visual variety

**HexTile** (`Assets/Scripts/HexTile.cs`):
- Individual hexagonal tile within a BiomeRegion
- Provides visual representation and bee spawn point
- Minimal logic - all game mechanics handled by parent BiomeRegion
- `BeeGatherPosition` property defines where bees spawn/gather

**BiomeRegionData** ScriptableObject (`Assets/Resources/BiomeRegionData/*.asset`):
- Configuration data for each biome region
- Defines:
  - `biomeType`: BiomeType enum value
  - `UnlockCost`: Money cost to unlock region
  - `BaseCapacity`: Starting bee capacity for region
  - `BonusCapacityPerUpgrade`: Capacity increase per tier
  - `capacityUpgradeCosts[]`: Cost array for capacity tiers
  - `maxCapacityTier`: Maximum upgrade tier

**FlowerPatchController** (`Assets/Scripts/FlowerPatchController.cs`):
- Legacy compatibility bridge - minimal role in current architecture
- Delegates to BiomeRegion for most operations
- Retained for backwards compatibility with existing systems

#### Biome Region Upgrades

**CAPACITY Path (Implemented):**
```
Tier 0: Base capacity (defined in BiomeRegionData)
Tier 1: +bonus capacity (more bee slots)
Tier 2: +more bonus capacity
Tier 3: +even more bonus capacity
Maximum tiers: Defined per biome in ScriptableObject
```

**When to use:** When bees are at capacity for a region and more throughput is needed.
See Global Bee Pool system below for allocation mechanics.

**Note:** No "PRODUCER Path" is implemented. Recipe tier upgrades serve the role of increasing production efficiency.

---

### 4. Global Bee Pool

**Core Concept:** Instead of unlimited bees per region, there is a **shared global pool** of bees that must be strategically allocated across all active pollen routes.

#### How It Works

**Managed by BeeFleetManager** (`Assets/Scripts/BeeFleetManager.cs`):
```
Total Bees Owned: Player's bee fleet size (grows with purchases)
Available Bees: Total owned minus currently allocated bees
Allocated Bees: Bees actively assigned to BiomeRegion routes
```

**Starting Values:**
- Starting bees: **2 bees** (hardcoded in BeeFleetManager)
- Starting money: **$50** (hardcoded in EconomyManager)

**Allocation Constraints:**
- Each BiomeRegion has **Maximum Bee Capacity** (starts low, upgradeable via Capacity Path)
- Players can only allocate bees up to each region's capacity limit
- Bees in global pool can be freely reallocated between regions
- When a region is removed, its allocated bees return to available pool

**Bee Purchase System:**
- **Independent of region unlocks** (unlike original design)
- Managed by `BeeFleetUpgradeData` system (separate upgrade tiers)
- Players purchase additional bees to expand global fleet
- Bee cost scales with each purchase

**Strategic Depth:**
```
Early Game:
- Limited bees (few regions unlocked)
- Simple allocation decisions
- Focus on getting first routes operational

Mid Game:
- More bees and regions
- Bottlenecks emerge as high-value recipes need more resources
- Must choose: spread bees thin vs concentrate on profitable routes

Late Game:
- Many regions competing for limited bee capacity
- Optimize allocation to avoid bottlenecks in complex recipe chains
- Balance between unlocking new regions vs upgrading capacity of existing ones
```

**Player Decisions:**
- "Do I allocate more bees to this Wild Meadow route to increase throughput?"
- "Should I pull bees from low-value Marsh routes to boost my Concordence Honey production?"
- "Is it better to upgrade this region's capacity or unlock a new biome?"

**UI Feedback:**
- **FleetManagementPanel** (`Assets/Scripts/UI/FleetManagementPanel.cs`) displays:
  - Total bees owned / Available for assignment
  - Current allocation per region (e.g., "3/5 bees")
  - Visual indicators when regions are at capacity
  - Warning when trying to allocate with no available bees

**Integration with Region Capacity Upgrades:**
- **Capacity Upgrade:** Increases max bee slots → allows more simultaneous deliveries per region
- **Trade-off:** Upgrade region capacity (more slots) vs buy more bees (more total fleet)?

---

### 5. THE BEEHIVE

#### Hive Function

**HiveController** (`Assets/Scripts/HiveController.cs`):
- **Receives:** All pollen deliveries from BeeController instances
- **Stores:** **Unlimited pollen inventory** (no capacity constraints)
  - Note: SeasonData has `storageCapacityModifier` field but it's unused
  - Original design had storage upgrades - removed for v1.0
- **Provides:** Resources to RecipeProductionManager for honey crafting
- **Events:** `OnResourcesChanged` for UI updates

#### Recipe Production System

**RecipeProductionManager** (`Assets/Scripts/RecipeProductionManager.cs`):
- Priority-based recipe production (list order determines priority)
- Automatically consumes pollen from HiveController when ingredients available
- Multiple recipes can produce simultaneously
- Production timers affected by seasonal modifiers
- Generates income via EconomyManager when recipes complete

**Recipe Tier Upgrades (5 Tiers: 0-4):**
```
Example: WildMeadowHoney Recipe
- Tier 0 (Base): 3x Wild Meadow Pollen, produces $5 in 10 seconds
- Tier 1: 2x Wild Meadow Pollen (-33% ingredients), produces $7 (+40%) in 9 seconds (-10% time)
- Tier 2: 2x Wild Meadow Pollen, produces $10 (+100% from base) in 8 seconds (-20% time)
- Tier 3: 1x Wild Meadow Pollen (-67% ingredients), produces $14 (+180%) in 7 seconds (-30% time)
- Tier 4: 1x Wild Meadow Pollen, produces $20 (+300%) in 6 seconds (-40% time)

(Values are examples - actual values defined per recipe in ScriptableObject assets)
```

**Upgrade Benefits:**
- Reduced ingredient requirements → fewer deliveries needed
- Faster production time → higher throughput
- Increased honey value → exponential income growth

**Strategic Depth:**
- Early: Focus on single-resource recipes with low tiers
- Mid: Upgrade frequently-used recipes to tier 2-3
- Late: Max out high-value multi-resource recipes to tier 4

---

### 6. BEES

**BeeController** (`Assets/Scripts/BeeController.cs`)

**Visual:** Small white spheres with colored trails matching biome they're assigned to
**Behavior:** Fully automated (no player control needed)

**AI Logic:**
```
1. Spawn at HexTile.BeeGatherPosition within BiomeRegion
2. Fly to Hive using smooth arc trajectory (not straight lines!)
   - Arc height controlled by altitude curve
   - Smooth acceleration/deceleration
3. Deliver pollen to Hive (HiveController.ReceiveResources)
4. Return to HexTile spawn point
5. Hover with Perlin noise pattern while gathering
6. Repeat cycle
```

**No route drawing needed!** - Bees automatically find Hive from any BiomeRegion

**Bee Properties:**
- **Speed:** Base speed (units/second) affected by seasonal modifiers
  - SeasonManager applies `beeSpeedModifier` from SeasonData
- **Capacity:** 1 resource per trip (not upgradeable in v1.0)
- **Visual feedback:**
  - Carries colored cube matching resource type
  - Trail renderer shows flight path in biome color
- **Flight Animation:**
  - Smooth arc trajectories (not straight lines as in original design)
  - Perlin noise hovering animation while gathering at tiles

---

### 7. PROGRESSION & UNLOCKS

#### Money Economy

**EconomyManager** (`Assets/Scripts/EconomyManager.cs`):
- Starting money: **$50**
- Tracks current money, fires `OnMoneyChanged` events for UI
- Methods: `EarnMoney()`, `SpendMoney()`, `CanAfford()`, `ResetToInitialState()`

**Earning Money:**
- From recipe completions (RecipeProductionManager → EconomyManager)
- Income scales exponentially with:
  - Multi-resource recipes (higher base value)
  - Recipe tier upgrades (increased honey value per tier)
  - Seasonal modifiers (income multipliers per season)

**Spending Money:**
- Unlock BiomeRegions (one-time cost)
- Unlock recipes (one-time cost, prerequisites required)
- Upgrade recipes (per-tier cost, must be unlocked first)
- Upgrade region capacity (per-tier cost)
- Purchase additional bees for global fleet (scaling cost)

**Income scales exponentially with recipe complexity and tier upgrades**

---

### 8. SEASONALITY & CAMPAIGN

**Core Concept:** The game runs as a year-long campaign divided into three distinct seasons, each lasting 7 weeks. Seasons apply dynamic global modifiers that change how the game plays, forcing players to adapt strategies throughout the year.

**Managed by SeasonManager** (`Assets/Scripts/SeasonManager.cs`)

#### Campaign Structure

**The Year:**
```
Total Duration: 21 weeks (Spring → Summer → Autumn)
Week 1-7: Spring (7 weeks)
Week 8-14: Summer (7 weeks)
Week 15-21: Autumn (7 weeks)
Week 22+: Winter (Campaign End - no gameplay, triggers EndOfYearPanel)
```

**Time Progression:**
- Default: **60 real seconds = 1 game week**
- Configurable via `realSecondsPerGameWeek` field in SeasonManager
- Total campaign length: **~21 minutes at default speed** (60 seconds × 21 weeks)
- Respects `Time.timeScale` for pause/speed control

**Campaign Flow:**
```
1. Player starts in Spring, Week 1 (SeasonManager.StartNewYear())
2. Season timer automatically advances each week
3. Visual/audio feedback when seasons change (OnSeasonChanged event)
4. At Week 22 (Winter arrival), campaign ends (OnYearEnded event)
5. End-of-Year Summary Panel displays (EndOfYearPanel.cs)
6. Player can restart for new playthrough ("Play Again") or quit
```

**Enable/Disable Toggle:**
- `enableSeasonSystem` field in SeasonManager allows disabling entire system
- Useful for testing or alternate game modes

#### Seasonal Modifiers System

Each season applies **global multipliers** that affect all game systems simultaneously. Modifiers are defined in `SeasonData` ScriptableObject assets (`Assets/Resources/Seasons/*.asset`).

**Modifier Types:**

**Income Modifier** (affects honey recipe values)
```
Example values (defined in SeasonData assets):
- Spring: 1.0x (baseline income)
- Summer: 1.5x (+50% income - high honey demand season)
- Autumn: 1.2x (+20% income - harvest season premium)

Strategic Impact: Summer is prime time for high-value recipe production
Applied by: RecipeProductionManager reads SeasonManager.GetCurrentSeasonData()
```

**Bee Speed Modifier** (affects delivery speed)
```
Example values:
- Spring: 1.1x (+10% faster - favorable flying conditions)
- Summer: 0.9x (-10% slower - heat affects flight)
- Autumn: 1.0x (baseline speed)

Strategic Impact: Spring accelerates resource gathering, Summer slows logistics
Applied by: BeeController reads seasonal modifier on spawn/flight
```

**Production Time Modifier** (affects recipe crafting speed)
```
Example values:
- Spring: 1.0x (baseline production time)
- Summer: 0.85x (15% faster production - optimal conditions)
- Autumn: 1.1x (10% slower - preparation for winter)

Strategic Impact: Summer boosts throughput, Autumn creates bottlenecks
Applied by: RecipeProductionManager modifies production timers
```

**Storage Capacity Modifier** (field exists but unused in v1.0)
```
SeasonData has storageCapacityModifier field
HiveController has unlimited storage in current implementation
Field retained for potential future features
```

**Event-Driven Architecture:**
```
SeasonManager.OnSeasonChanged event
    ↓
Systems subscribe and query GetCurrentSeasonData()
    ↓
BeeController applies beeSpeedModifier
RecipeProductionManager applies productionTimeModifier and incomeModifier
    ↓
Game systems automatically adapt to seasonal changes
```

**Strategic Implications:**

**Seasonal Adaptation:**
```
Spring Strategy:
- Fast bees + baseline income = ideal for expansion
- Unlock new biomes and regions
- Build up infrastructure and bee fleet
- Focus on establishing resource flow

Summer Strategy:
- Maximum income multiplier = focus on high-value recipes
- Bees are slower, but production is faster
- Push for recipe tier upgrades
- Accept delivery bottlenecks for premium honey prices
- Maximize multi-resource recipe production

Autumn Strategy:
- Bonus income + slower production = optimization phase
- Rebalance bee allocations to compensate for slowdown
- Fine-tune bottlenecks before year end
- Max out production efficiency
- Final push for high scores
```

**No "Perfect" Season:**
- Each season has trade-offs (high income vs slow bees, fast production vs baseline income)
- Players must adapt strategy every 7 weeks
- Forces diverse optimization approaches

#### End-of-Year Summary

**EndOfYearPanel** (`Assets/Scripts/UI/EndOfYearPanel.cs`)

When Week 22 (Winter) arrives, the campaign ends and players see a comprehensive summary showing:

**Hero Stats:**
```
- Total Money Earned (across all 3 seasons)
- Total Recipes Completed
- Total Resources Collected
```

**Economic Performance:**
```
- Starting Money ($50)
- Ending Money
- Highest Single Transaction
```

**Empire Statistics:**
```
- Total BiomeRegions Unlocked
- Peak Bee Fleet Size
```

**Seasonal Breakdown:**
```
Per-season display showing:
- Spring: Money earned, Recipes completed, Resources collected
- Summer: Money earned, Recipes completed, Resources collected
- Autumn: Money earned, Recipes collected, Resources collected
```

**High Score Tracking:**
```
- Managed by HighScoreManager (persists across sessions)
- Compare current run to previous best runs
- Show new records broken this playthrough
- Categories: Total money, recipes completed, fastest completion, etc.
```

**Restart Options:**
```
- "Play Again" button: Calls GameManager.ResetYear() - resets to Spring Week 1
- Resets all managers: EconomyManager, BeeFleetManager, RecipeProgressionManager, etc.
- Destroys all BiomeRegion GameObjects
- Preserves high scores
```

#### Stats Tracking System

**YearStatsTracker** (`Assets/Scripts/YearStatsTracker.cs`):
- Singleton manager for passive stat collection
- Tracks all campaign metrics automatically

**Automatic Collection:**
- Monitors events from all managers (event-driven architecture)
- Tracks money changes (only positive deltas = "earned")
- Tracks recipe completions by type
- Tracks resource deliveries by biome
- Tracks seasonal breakdowns in real-time (switches buckets on season change)

**Event-Driven Architecture:**
```
EconomyManager.OnMoneyChanged → Track earnings
RecipeProductionManager.OnRecipeCompleted → Track production
HiveController.OnResourcesChanged → Track deliveries
SeasonManager.OnSeasonChanged → Switch seasonal tracking buckets
```

**No Performance Impact:**
- Passive event listeners, no polling
- Lightweight dictionary aggregation
- Only queries data at campaign end for EndOfYearPanel

#### Replayability & Meta-Progression

**Current Implementation:**
```
- High scores persist across playthroughs (HighScoreManager)
- Each playthrough is independent (no prestige system in v1.0)
- 21-minute campaign length encourages multiple runs
```

**Future Expansion Potential:**
```
- Prestige system (start new year with bonuses from previous run)
- Challenge modes (modified seasonal modifiers, constraints)
- Multiple campaign maps
- Leaderboards
- Seasonal events with unique modifiers
```

#### Design Philosophy

**Why Seasons?**
1. **Pacing:** Breaks 21-minute campaign into distinct phases (7 weeks each)
2. **Variety:** Modifiers force strategy adaptation, prevents monotony
3. **Tension:** Timer creates urgency ("Must optimize before Autumn!")
4. **Satisfaction:** End-of-year summary provides closure and performance feedback

**Why 21 Weeks (not 24)?**
- Current implementation: 7 weeks per season (3 seasons = 21 weeks)
- Week 22 = Winter = immediate game over (no Winter gameplay)
- Clean division: 7-week seasons feel balanced for ~21-minute playthroughs

**Why Fixed Campaign Length?**
1. **Focused Experience:** Validates core loop in 20-25 minutes
2. **Replayability:** Short enough to play multiple times, optimize strategies
3. **Clear Goal:** "Survive to Winter" is tangible, achievable
4. **Expandable:** Can add endless mode later if successful

**Debug Controls (Editor Only):**
```
S key: Skip to next season
W key: Skip to next week
R key: Restart year
1/2/3 keys: Game speed control (1x, 3x, 10x)
F12 key: Capture screenshot
```
(Note: These controls are stripped from production builds via `#if UNITY_EDITOR` directives)

---

## UI System

### Implemented UI Panels

**HUDController** (`Assets/Scripts/UI/HUDController.cs`):
- Main game HUD displaying:
  - Current money (subscribes to EconomyManager.OnMoneyChanged)
  - Bee fleet status (subscribes to BeeFleetManager events)
  - Hive pollen inventory (subscribes to HiveController.OnResourcesChanged)

**SeasonUI** (`Assets/Scripts/UI/SeasonUI.cs`):
- Current season display
- Week counter (1-21)
- Week progress bar
- Subscribes to SeasonManager.OnSeasonChanged and OnWeekChanged

**RecipeDisplayPanel** (`Assets/Scripts/UI/RecipeDisplayPanel.cs`):
- Shows active recipes with production progress
- Ingredient requirements visualization
- Production timers with seasonal modifiers
- Recipe unlock/upgrade buttons

**EndOfYearPanel** (`Assets/Scripts/UI/EndOfYearPanel.cs`):
- End-of-campaign summary screen
- Hero stats, economic performance, empire statistics
- Per-season breakdown
- High score comparison
- "Play Again" and "Quit" buttons

**FleetManagementPanel** (`Assets/Scripts/UI/FleetManagementPanel.cs`):
- Bee allocation UI for all BiomeRegions
- Shows: Total bees / Available bees / Allocated per region
- Allocation/deallocation controls
- Capacity indicators

**FlowerPatchUpgradePanel** (`Assets/Scripts/UI/FlowerPatchUpgradePanel.cs`):
- Region capacity upgrade interface
- Shows current tier, next tier capacity, upgrade cost
- Upgrade button with affordability checks

**FlowerPatchUnlockPanel** (`Assets/Scripts/UI/FlowerPatchUnlockPanel.cs`):
- BiomeRegion unlock UI
- Shows biome type, unlock cost, biome description
- Lock/unlock visual states

**HowToPlayPanel** (`Assets/Scripts/UI/HowToPlayPanel.cs`):
- Tutorial/help system for new players
- Explains core mechanics, controls, strategies
- Accessible from main UI

**TooltipController** (`Assets/Scripts/UI/TooltipController.cs`):
- Centralized tooltip system
- Shows contextual information on hover
- Auto-positioning relative to cursor

**TooltipTrigger** (`Assets/Scripts/UI/TooltipTrigger.cs`):
- Component attached to UI elements
- Triggers tooltip display on hover
- Configurable tooltip content

**RecipeEntryUI** (`Assets/Scripts/UI/RecipeEntryUI.cs`):
- Individual recipe display in RecipeDisplayPanel
- Shows recipe name, ingredients, progress

**IngredientEntryUI** (`Assets/Scripts/UI/IngredientEntryUI.cs`):
- Individual ingredient display
- Shows pollen type, quantity required, quantity available

**SettingsController** (`Assets/Scripts/UI/SettingsController.cs`):
- Game settings panel
- Audio controls, visual toggles, debug options

**PanelBlocker / UIBlocker**:
- UI interaction management
- Prevents unwanted clicks during animations/transitions

---

## Visual Style

### Art Direction
- **Aesthetic:** Minimalist infographic meets resource management
- **Tone:** Clean, professional, satisfying
- **References:** Mini Metro, Thronefall, Monument Valley

### Camera & View
- **Camera:** Orthographic, 45° top-down angle
- **Zoom:** Optional (scroll to zoom in/out)
- **Pan:** Optional (drag or edge-scroll)
- **Focus:** Hive is center, BiomeRegions arranged around it

### Material System

**FlowerPatchMaterialMapper** (`Assets/Resources/FlowerPatchMaterialMapper.asset`):
- Centralized source of truth for all biome visuals
- Maps `BiomeType` enum to Unity Materials and Colors
- Provides:
  - Original materials per biome (unlocked state)
  - Locked materials per biome (locked state with reduced saturation)
  - Hover materials per biome (highlight state)
- **Always reference this asset** when assigning biome visuals - ensures consistency

**Biome-Specific Materials:**
- Located: `Assets/Material/`
- Naming convention: `{BiomeName}Material.mat`, `{BiomeName}Locked.mat`
- Biomes: WildMeadow, Orchard, CultivatedGarden, AgriculturalField, Marsh, ForestEdge

---

## Technical Setup

### Unity Configuration
- **Version:** Unity 6000.2.10f1 (Unity 6)
- **Pipeline:** URP (Universal Render Pipeline - better WebGL performance)
- **Target:** WebGL build, 60 FPS with 100+ Bees, 50+ FPS with 200+ Bees

### Scene Layout
- **Main Scene:** `Assets/Scenes/GameScene.unity`
- **World Structure:**
  - Hive: Center (0, 0, 0)
  - BiomeRegions: Arranged in hexagonal grid around hive
  - Each BiomeRegion: 7 HexTile children in cluster formation

### Performance Target
- Object pooling if needed (60fps not sustainable above 200 bees)
- Current optimization: Instantiate/Destroy pattern works well up to ~150 bees
- BeeController uses simple arc trajectory calculation (low overhead)
- HexTile system distributes bee spawns across region for visual variety

---

## Architecture & Core Systems

### Manager Hierarchy (Singletons)

All managers use singleton pattern with `Instance` property. Awake() enforces single instance, OnDestroy() cleans up reference.

**GameManager** (`Assets/Scripts/GameManager.cs`):
- Central coordinator for all game systems
- Manages Time.timeScale for game speed control (1x, 3x, 10x via hotkeys)
- Handles year reset flow via `ResetYear()` method:
  - Destroys all BiomeRegion GameObjects
  - Calls ResetToInitialState() on all managers in proper order:
    1. EconomyManager
    2. BeeFleetManager
    3. RecipeProgressionManager
    4. SeasonManager
    5. YearStatsTracker
    6. RecipeProductionManager
    7. HiveController
- Tracks global statistics (bee count, elapsed time)

**EconomyManager** (`Assets/Scripts/EconomyManager.cs`):
- Starting money: $50
- Methods: `EarnMoney()`, `SpendMoney()`, `CanAfford()`
- Events: `OnMoneyChanged`
- Reset: Returns to $50 on `ResetToInitialState()`

**BeeFleetManager** (`Assets/Scripts/BeeFleetManager.cs`):
- Starting bees: 2
- Tracks: Total bees owned, available bees, allocated bees per BiomeRegion
- Methods: `AllocateBee()`, `DeallocateBee()`, `GetAllocatedBeeCount()`, `GetMaxCapacity()`
- **Works with BiomeRegion** (not individual FlowerPatchController)
- Events: `OnBeeAllocationChanged`, `OnTotalBeesChanged`
- Bee purchase system: Independent upgrade tiers via BeeFleetUpgradeData

**RecipeProductionManager** (`Assets/Scripts/RecipeProductionManager.cs`):
- Priority-based production (list order)
- Tracks active recipes, production timers
- Methods: `StartProduction()`, `PauseProduction()`, `ResumeProduction()`
- Applies seasonal modifiers to production time and honey value
- Automatically consumes pollen from HiveController
- Events: `OnRecipeCompleted`, `OnRecipeStarted`

**RecipeProgressionManager** (`Assets/Scripts/RecipeProgressionManager.cs`):
- Runtime unlock/upgrade tracking (resets each campaign)
- Recipe state storage: `isUnlocked`, `currentTier` (0-4)
- Methods: `IsRecipeUnlocked()`, `GetRecipeTier()`, `TryUnlockRecipe()`, `TryUpgradeRecipe()`, `CanUnlockRecipe()`, `CanUpgradeRecipe()`
- Prerequisite validation via `ArePrerequisitesMet()` (checks prerequisiteRecipes array)
- Events: `OnRecipeUnlocked`, `OnRecipeUpgraded`

**SeasonManager** (`Assets/Scripts/SeasonManager.cs`):
- Campaign: 21 weeks (7 per season)
- Methods: `StartNewYear()`, `GetCurrentSeasonData()`, `PauseSeasonTimer()`, `ResumeSeasonTimer()`
- Properties: `CurrentSeason`, `CurrentWeek`, `WeekProgress`, `YearProgress`
- Events: `OnSeasonChanged`, `OnWeekChanged`, `OnYearEnded`
- Time progression: 60 real seconds = 1 game week (configurable)

**YearStatsTracker** (`Assets/Scripts/YearStatsTracker.cs`):
- Event-driven stat collection
- Tracks per-season breakdowns (Spring, Summer, Autumn)
- Methods: `GetTotalMoneyEarned()`, `GetTotalRecipesCompleted()`, `GetSeasonStats()`
- No polling - passive listeners only

**HiveController** (`Assets/Scripts/HiveController.cs`):
- Central pollen hub
- **Unlimited storage capacity** (no constraints in v1.0)
- Methods: `ReceiveResources()`, `TryConsumeResources()`, `GetResourceCount()`
- Events: `OnResourcesChanged`

**HighScoreManager** (exists, manages score persistence)

### Entity Controllers

**BiomeRegion** (`Assets/Scripts/BiomeRegion.cs`):
- Multi-hex region manager (7 HexTile children)
- Properties: `BiomeType`, `IsLocked`, `MaxBeeCapacity`, `CapacityTier`
- Methods: `UnlockRegion()`, `UpgradeCapacity()`, `GetNextBeeSpawnTile()`, `GetRegionCenter()`
- Unified capacity system (entire region shares capacity)
- Round-robin bee spawn distribution across tiles

**HexTile** (`Assets/Scripts/HexTile.cs`):
- Individual hexagonal tile
- Property: `BeeGatherPosition` (Vector3 spawn point for bees)
- Methods: `ApplyMaterial()`, `SetParentRegion()`
- Minimal logic - visual representation only

**FlowerPatchController** (`Assets/Scripts/FlowerPatchController.cs`):
- Legacy compatibility bridge
- Delegates to parent BiomeRegion for operations
- Retained for backwards compatibility

**BeeController** (`Assets/Scripts/BeeController.cs`):
- Automated pollen delivery
- Smooth arc flight trajectories (not straight lines)
- Perlin noise hovering animation while gathering
- Methods: `Initialize()`, `StartDelivery()`, `ReturnToPatch()`
- Seasonal speed modifiers applied from SeasonManager

**RouteController** (`Assets/Scripts/RouteController.cs`):
- Visualizes pollen delivery routes
- Color-coded by biome type

### Data-Driven Design (ScriptableObjects)

**FlowerPatchData** (`Assets/Resources/FlowerPatchData/*.asset`):
- Legacy system - partially replaced by BiomeRegionData
- Still used for some compatibility

**BiomeRegionData** (`Assets/Resources/BiomeRegionData/*.asset`):
- Configuration for BiomeRegions
- Fields: `biomeType`, `UnlockCost`, `BaseCapacity`, `BonusCapacityPerUpgrade`, `capacityUpgradeCosts[]`, `maxCapacityTier`
- 6 assets: WildMeadowBiome, OrchardBiome, CultivatedGardenBiome, AgriculturalFieldBiome, MarshBiome, ForestEdgeBiome

**HoneyRecipe** (`Assets/Resources/Recipes/*.asset`):
- Recipe configuration
- Fields: `recipeName`, `ingredients[]`, `baseProductionTime`, `baseHoneyValue`, `unlockCost`, `upgradeCosts[]`, `prerequisiteRecipes[]`, `isUnlockedByDefault`
- Upgrade tier modifiers: `ingredientReductionPerTier[]`, `productionTimeReductionPerTier[]`, `honeyValueIncreasePerTier[]`
- Methods: `CanProduce()`, `GetIngredients()`, `GetProductionTime()`, `GetHoneyValue()`, `CanUpgrade()`, `GetUpgradeCost()`
- 13 assets: WildMeadowHoney, OrchardHoney, GardenHoney, HarvestHoney, MarshlandHoney, ForestHoney, PastoralHoney, AbundantHoney, PrimordialHoney, CountrysideHoney, TerroirHoney, ConvergenceHoney, ConcordenceHoney

**SeasonData** (`Assets/Resources/Seasons/*.asset`):
- Seasonal modifier configuration
- Fields: `seasonName`, `incomeModifier`, `beeSpeedModifier`, `productionTimeModifier`, `storageCapacityModifier` (unused), `seasonStartSound`
- 3 assets: Spring, Summer, Autumn

**FlowerPatchMaterialMapper** (`Assets/Resources/FlowerPatchMaterialMapper.asset`):
- Centralized visual configuration
- Maps BiomeType → Material/Color
- Single source of truth for all biome visuals

### Event Communication Pattern

**Decoupled Architecture:**
```
Manager fires UnityEvent → UI subscribes → UI updates display
Example:
  EconomyManager.OnMoneyChanged event
      ↓
  HUDController subscribes in Start()
      ↓
  HUDController.UpdateMoneyDisplay() called with new value
```

**Best Practice:**
- UI scripts subscribe to events in `Start()` (not `OnEnable()` - avoids race conditions)
- Managers fire events after state changes
- No direct manager → UI references (fully decoupled)

### Creation Workflows

**Creating New BiomeRegion:**
1. Create new BiomeType enum value in `GameTypes.cs`
2. Create new ResourceType enum value (matching biome)
3. Create material in `Assets/Material/` for biome
4. Add biome to `FlowerPatchMaterialMapper.asset`
5. Create BiomeRegionData asset: Right-click → `Game/Biome Region Data`
   - Set biome type, unlock cost, base capacity, upgrade costs
6. Create prefab with 7 HexTile children arranged in hex cluster
7. Create recipes using the new pollen type

**Creating New Recipe:**
1. Create HoneyRecipe asset: Right-click → `Game/Honey Recipe`
2. Configure: ingredients, production time, honey value, unlock cost
3. Set upgrade costs array (5 tiers)
4. Set prerequisite recipes (if any)
5. Configure tier modifiers (ingredient reduction %, time reduction %, value increase %)
6. Add to `RecipeProgressionManager.allRecipes` list in scene inspector
7. Test unlock flow and production

---

## Critical Design Pillars

### 1. SATISFYING AUTOMATION
Watch your network operate automatically. No micromanagement of individual bees or routes.

### 2. EXPONENTIAL GROWTH
Income scales 10x, 50x, 200x through recipe combinations and tier upgrades. Incremental satisfaction.

### 3. MEANINGFUL CHOICES
Bee allocation, region unlocks, recipe upgrades, capacity tiers. No obvious "correct" answer - multiple viable strategies.

### 4. VISIBLE OPTIMIZATION
Bottlenecks are clear (bee capacity, ingredient flow, production time). Solutions are multiple (upgrade capacity, reallocate bees, upgrade recipes). Player feels smart solving them.

### 5. RAPID PROTOTYPING
Architecture designed for fast iteration. ScriptableObject data-driven design allows balance changes without code. Expandable if successful.

---

## What Makes This Fun?

✅ **Optimization:** "How do I fix this bottleneck? Do I need more bees, higher capacity, or recipe upgrades?"
✅ **Growth:** "My income went from $5/min to $500/min by upgrading to Concordence Honey!"
✅ **Problem-Solving:** "Multiple ways to solve this. Spread bees evenly or concentrate on high-value routes?"
✅ **Strategic Depth:** "Do I unlock new biomes or upgrade existing recipes? Both have trade-offs."
✅ **Seasonal Adaptation:** "Summer boosts income but slows bees - time to focus on production upgrades!"

**Most Important:** Every decision has **opportunity cost**. That's what creates strategy.

---

## Architecture Health Status

**Last Review:** 2025-12-01 (v1.0 Production Release)

### Performance Recommendations

**Object Pooling:** Consider if bee count exceeds 200+
- Current target: 60 FPS @ 100+ bees (achieved)
- Instantiate/Destroy pattern works well at current scale
- Can add pooling as optimization if player feedback requests higher bee counts

**Data-Oriented Design (DOTS/ECS):** Not recommended
- Current entity count (100-200 bees, 6-12 regions) performs excellently with MonoBehaviour
- ECS would add complexity without performance benefit at this scale
- WebGL deployment better suited to traditional Unity patterns

---

## Third-Party Assets

### Runtime Assets
- **PolygonParticleFX** - Particle effects (`Assets/FX/PolygonParticleFX/`)

### Editor Plugins (Development Only)
- AssetInventory, TableForge, vFolders, vHierarchy, HotReload

---

## Known Issues & Future Features

See CLAUDE.md for development roadmap and known issues tracking.

**Recent Changes:**
- Hive storage capacity made unlimited (removed upgrade complexity)
- Campaign duration finalized at 21 weeks (7 per season)

**Potential Expansions:**
- Prestige system (start new year with bonuses from previous run)
- Additional biomes (Jungle, Volcano, etc.)
- Challenge modes (modified seasonal parameters, constraints)
- Endless mode (continue past Week 21)
- Weather events (SeasonData has fields for this - currently unused)

---

## Summary

Hive Empire is a feature-complete incremental strategy game with:
- **13 fully balanced recipes** with 5-tier upgrade progression
- **6 unique biomes** in hexagonal tile regions
- **21-week seasonal campaign** with dynamic modifiers
- **Global bee pool** strategic constraint
- **Event-driven architecture** for maintainable code
- **Data-driven design** for easy balance iteration
- **Polished UI** with tooltips, tutorials, and comprehensive feedback

The game validates core incremental mechanics while maintaining strategic depth. Architecture is scalable for future expansion while performing excellently in WebGL at target specifications.

**Core Loop:** Unlock → Allocate → Optimize → Upgrade → Repeat
**Player Fantasy:** "I built an efficient pollen empire through smart optimization."

---

*End of Technical Reference - See CLAUDE.md for detailed development guide*
