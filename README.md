# ReadMe - HIVE EMPIRE - Game Concept

**Version:** Prototype v1.0  
**Target:** 3-4 week prototype for itch.io WebGL play testing  
**Core Focus:** Incremental optimization with strategic resource management  
**Itch command line** to upload new build: butler push "D:\Unity\Games\HiveEmpire\Build\WebGL\hiveempire.zip" bouldouklu/hiveempire:HTML

---

## High-Level Concept

> **Build an automated pollen network, connecting resource-producing flower patchs to your central hive.  
> Discover synergies, manage bottlenecks, and optimize production chains for exponential growth.**

**Genre:** Incremental Strategy + Resource Management  
**Style:** Minimalist 3D, top-down view (Mini Metro aesthetic)  
**Platform:** WebGL (browser-based)  
**Core Appeal:** Satisfying optimization, exponential growth, strategic choices

---

## The Core Hook

**IS about:** Identifying bottlenecks, strategic upgrades, synergies optimization between resources gathering, recipe production and unlocking/upgrades choices  
**NOT about:** Perfect flight path routing  
**Player Fantasy:** "I'm building an efficient pollen empire by managing complexity, bottleneck and optimizing the production chain."

---

## Core Game Loop

```
Flower Patch generates Resource (different types of pollen)
    ↓
Bee automatically delivers to Hive
    ↓
Hive COMBINES 1 or several resources → Honey is produced for money
    ↓
Player earns exponential Money
    ↓
Player spends on: New Flower Patchs, Upgrades, New Biomes, new recipes
    ↓
Player optimizes bottlenecks, balance gathering rates  production rates
    ↓
LOOP with increasing complexity (more biomes, more patches, more upgrades, more recipes, more bottlenecks)
```

---

## Win Condition

**Goal:** Maximize income & growth over one campaign: Spring, Summer, Autumn  
**Failure state** - OPTIONAL  
**Endgame** - OPTIONAL

---

## Core Mechanics

### 1. RESOURCE SYNERGY SYSTEM (The Heart)

Resources have **exponential value** when combined at the hive.

#### Single Resources recipe (Low Value):
```
1x Forest Pollen => Forest Honey  = $1
3x Plains Pollen => Wildflower honey = $5
3x Mountain Pollen =>  Mountain Honey = $12
```

#### Multiple resource recipe (High Value):
```
TWO RESOURCES:
5x Forest Pollen + 3x Desert Pollen => Desert Blossom $25
More to develop..

THREE RESOURCES:
More to develop..

FOUR RESOURCES:
More to develop..

FIVE RESOURCES:
More to develop..

SIX RESOURCES:
2x of EACH Pollen => Premium Blend = Very high price TBD
More to develop..

```

**Strategic Depth:**
- Early: Unlock patches/biomes for more single resource recipe production
- Mid: Unlock patches/biome for multiple resource recipe. Start to upgrades patches or recipe production tiers to avoid bottleneck
- Late: Balance unlocking of patches/biomes and all upgrades to avoid bottlenecks and reach maximum growth

---

### 2. BIOMES & RESOURCES (placeholder names and types for now)

#### Starting Biomes (Always Available):
- **Forest** → Forest Pollen (fast gathering as they are close to beehive)

#### Unlockable Biomes (Choose Unlock Order):
- **Plains** → Plains Pollen (fast gathering as they are close to beehive)
- **Mountain** → Mountain Pollen (medium gathering speed as a bit distant from beehive) [medium $ to unlock]
- **Desert** → Desert Pollen (medium gathering speed as a bit distant from beehive) [medium $ to unlock]
- **Coastal** → Coastal Pollen (slow gathering speed as a very distant from beehive) [high $ to unlock]
- **Tundra** → Tundra Pollen (slow gathering speed as a very distant from beehive) [high $ to unlock]

#### Future Expansion Biomes:
- Jungle → Exotic Pollen
- Volcano → Fire Pollen
- More to be developed..

---

### 3. FlowerPatches

#### Basic Flower Patch
- **Function:** Generates 1 resource type based on biome
- **Visual:** Simple low poly flower patch biome-colored accent
- **Placement:** Appears in discovered biome when purchased

#### Flower Patch Upgrades (Choose Specialization Path):

**PRODUCER Path:**
```
Tier 1: more bees on route
Tier 2: even more bees on route
Tier 3: even more more bees on route
More to develop..
```
**When to use:** Bottleneck is resource gathering or speed of delivery

**CAPACITY Path:**
```
Tier 1: Have a limited capacity of bees that can be on this route
Tier 2: Have more capacity on route
Tier 3: Have even more capacity on route
More to develop..
```
**When to use:** When bees are overpopulating one flowerpatches and are needed elsewhere to avoid bottlenecks of resource deliveries.  
See 4. Global Bee Pool core mechanic below

**No "correct" specialization** → player expression and problem-solving

---

### 4. Global Bee Pool

**Core Concept:** Instead of each flower patch having unlimited independent bees, there is a **shared global pool** of bees that must be strategically allocated across all active pollen routes.

#### How It Works:

**Global Fleet Management:**
```
Total Bees Owned: Player's bee fleet size (grows with flower patch purchases and upgrades)
Available Bees: Total owned minus currently allocated bees
Allocated Bees: Bees actively assigned to flower patch routes
```

**Allocation Constraints:**
- Each flower patch has a **Maximum Bee Capacity** (starts low, can be upgraded via Capacity Path)
- Players can only allocate bees up to each flower patch's capacity limit
- Bees in the global pool can be freely reallocated between flower patches
- When a flower patch is destroyed/removed, its allocated bees return to the available pool

**Strategic Depth:**
```
Early Game:
- Limited bees (few flower patches unlocked)
- Simple allocation decisions
- Focus on getting first routes operational

Mid Game:
- More bees and flower patches
- Bottlenecks emerge as high-value recipes need more resources
- Must choose: spread bees thin vs concentrate on profitable routes

Late Game:
- Many flower patches competing for limited bee capacity
- Optimize allocation to avoid bottlenecks in complex recipe chains
- Balance between unlocking new patches vs upgrading capacity of existing ones
```

**Player Decisions:**
- **"Do I allocate more bees to this Forest Pollen route to increase throughput?"**
- **"Should I pull bees from low-value Plains routes to boost my Premium Blend production?"**
- **"Is it better to upgrade this flower patch's capacity or unlock a new biome?"**

**Feedback & Visibility:**
```
UI displays:
- Total bees owned / Available for assignment
- Current allocation per flower patch (e.g., "3/5 bees")
- Visual indicators when flower patches are at capacity
- Warning when trying to allocate with no available bees
```

**Integration with Flower Patch Upgrades:**
- **PRODUCER Path:** Increases pollen generation rate → more deliveries with same bees
- **CAPACITY Path:** Increases max bee capacity → allows more simultaneous deliveries
- **Trade-off:** Upgrade generation rate (efficient) vs capacity (volume)?

---

### 5. THE BEEHIVE

#### Hive Function:
- **Receives:** All pollen deliveries
- **Combines:** Resources into high-value honey automatically, with a production time. 
- **Pays Out:** Money based on combination value

#### Hive Upgrades:
```
Recipe Tier: Increase the honey value by combining more quantity of pollens together, for a shorter time of production
- Recipe Forest Honey Tier 1: 1x forest pollen create $2 in 7 sec. 
- Recipe Forest Honey Tier 2: 2x forest pollen, creates 5€, in 6.5secs.
- Recipe Forest Honey Tier 3: 3x forest pollen, creates 9€, in 6.2secs.
More to develop..

- Recipe Wildflower Honey Tier 1: 3x plains pollen, creates 5€ in 10secs
- Recipe Wildflower Honey Tier 2: 6x plains pollen, creates 11€ in 9.5secs
More to develop..

etc..


Storage Capacity: Holds resources before combining
- Forest Pollen Tier 1: 100 of each resource
- Forest Pollen Tier 1: 120 of each resource
More to develop..

- Plains Pollen Tier 1: 100 of each resource
- Plains Pollen Tier 1: 120 of each resource
More to develop..

etc..
```

---

### 5. BEES

**Visual:** Small white spheres with colored trails from biomes they are currently assigned to.  
**Behavior:** Fully automated (no player control needed)  
**AI Logic:**
```
1. Spawn at Flower Patch when resource ready
2. Fly to Hive along straight line route
3. Deliver pollen to Hive
4. Return to Flower Patch
5. Repeat
```

**No route drawing needed!** - BEES automatically find Hive from any Flower Patch

**Bee Properties:**
- Speed: 4 units/second (affected by Flower Patch upgrades)
- Capacity: 1 resource (maybe upgradable via Flower Patch upgrades)
- Visual feedback: Carries visible colored cube (resource type)

---

### 6. PROGRESSION & UNLOCKS

#### Money Economy

**Earning Money:**
- from single resource recipes
- from multiple resource recipes
- from upgrading recipes tiers to optimize production and increase honey output,thus money generation 

**Income scales exponentially with multiple resource recuipes and upgrades of recipes tiers**

---

### 7. SEASONALITY & CAMPAIGN

**Core Concept:** The game runs as a year-long campaign divided into three distinct seasons, each lasting 8 weeks. Seasons apply dynamic global modifiers that change how the game plays, forcing players to adapt strategies throughout the year.

#### Campaign Structure

**The Year:**
```
Total Duration: 24 weeks (Spring → Summer → Autumn)
Week 1-8: Spring (8 weeks)
Week 9-16: Summer (8 weeks)
Week 17-24: Autumn (8 weeks)
Week 25+: Winter (Campaign End)
```

**Time Progression:**
- Default: 60 real seconds = 1 game week
- Configurable for playtesting
- Total campaign length: ~24 minutes at default speed
- Respects Time.timeScale for pause/speed control

**Campaign Flow:**
```
1. Player starts in Spring, Week 1
2. Season timer automatically advances each week
3. Visual/audio feedback when seasons change
4. At Week 25 (Winter arrival), campaign ends
5. End-of-Year Summary Panel displays
6. Player can restart for new playthrough or quit
```

#### Seasonal Modifiers System

Each season applies **global multipliers** that affect all game systems simultaneously. Modifiers are defined in ScriptableObject assets for each season.

**Modifier Types:**

**Income Modifier** (affects honey recipe values)
```
Example:
- Spring: 1.0x (baseline income)
- Summer: 1.5x (+50% income - high honey demand season)
- Autumn: 1.2x (+20% income - harvest season premium)

Strategic Impact: Summer is prime time for high-value recipe production
```

**Bee Speed Modifier** (affects delivery speed)
```
Example:
- Spring: 1.1x (+10% faster - favorable flying conditions)
- Summer: 0.9x (-10% slower - heat affects flight)
- Autumn: 1.0x (baseline speed)

Strategic Impact: Spring accelerates resource gathering, Summer slows logistics
```

**Production Time Modifier** (affects recipe crafting speed)
```
Example:
- Spring: 1.0x (baseline production time)
- Summer: 0.85x (15% faster production - optimal conditions)
- Autumn: 1.1x (10% slower - preparation for winter)

Strategic Impact: Summer boosts throughput, Autumn creates bottlenecks
```

**Storage Capacity Modifier** (affects hive pollen storage)
```
Example:
- Spring: 1.2x (+20% storage - stockpiling for busy season)
- Summer: 1.0x (baseline storage)
- Autumn: 1.3x (+30% storage - preparing for winter)

Strategic Impact: Spring/Autumn allow resource buffering, reduces bottlenecks
```

**Strategic Implications:**

**Seasonal Adaptation:**
```
Spring Strategy:
- Fast bees + bonus storage = ideal for expansion
- Unlock new biomes and flower patches
- Build up infrastructure

Summer Strategy:
- Maximum income multiplier = focus on high-value recipes
- Bees are slower, but production is faster
- Push for recipe tier upgrades
- Accept delivery bottlenecks for premium honey prices

Autumn Strategy:
- Bonus storage + slower production = optimization phase
- Rebalance bee allocations
- Fine-tune bottlenecks before year end
- Max out production efficiency
```

**No "Perfect" Season:**
- Each season has trade-offs (high income vs slow bees, fast production vs baseline income)
- Players must adapt strategy every 8 weeks
- Forces diverse optimization approaches

#### End-of-Year Summary

When Week 25 (Winter) arrives, the campaign ends and players see a comprehensive **End-of-Year Panel** showing:

**Hero Stats:**
```
- Total Money Earned (across all 3 seasons)
- Total Recipes Completed
- Total Resources Collected
```

**Economic Performance:**
```
- Starting Money
- Ending Money
- Highest Single Transaction
```

**Empire Statistics:**
```
- Total Flower Patches Placed
- Peak Bee Fleet Size
```

**Seasonal Breakdown:**
```
Per-season display showing:
- Spring: Money earned, Recipes completed, Resources collected
- Summer: Money earned, Recipes completed, Resources collected
- Autumn: Money earned, Recipes completed, Resources collected
```

**High Score Tracking:**
```
- Compare current run to previous best runs
- Show new records broken this playthrough
- Persist high scores across sessions
- Categories: Total money, recipes completed, fastest completion, etc.
```

**Restart Options:**
```
- "Play Again" button: Reset to Spring Week 1 with fresh economy
- "Quit" button: Exit to main menu (optional)
- Future: "Endless Mode" toggle (continues past Week 24)
```

#### Stats Tracking System

**Automatic Collection:**
- YearStatsTracker singleton monitors events from all managers
- Tracks money changes (only positive deltas = "earned")
- Tracks recipe completions by type
- Tracks resource deliveries by biome
- Tracks seasonal breakdowns in real-time

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
- Only queries data at campaign end

#### Replayability & Meta-Progression

**Current Implementation:**
```
- High scores persist across playthroughs
- Each playthrough is independent (no prestige system yet)
```

**Future Expansion:**
```
- Prestige system (start new year with bonuses)
- Challenge modes (modifiers, constraints)
- Multiple campaign maps
- Leaderboards
- Seasonal events with unique modifiers
```

#### Design Philosophy

**Why Seasons?**
1. **Pacing:** Breaks 24-minute campaign into distinct phases
2. **Variety:** Modifiers force strategy adaptation, prevents monotony
3. **Tension:** Timer creates urgency ("Must optimize before Autumn!")
4. **Satisfaction:** End-of-year summary provides closure and performance feedback

**Why Fixed Campaign Length?**
1. **Focused Experience:** Prototype validates core loop in 20-30 minutes
2. **Replayability:** Short enough to play multiple times, optimize strategies
3. **Clear Goal:** "Survive to Winter" is tangible, achievable
4. **Expandable:** Can add endless mode later if successful

---

## Strategic Decision Examples

### Opening (0-5 minutes):
```
To be developed..
```

### Early Mid-Game (5-15 minutes):
```
To be developed..
```

### Mid-Game Pivot (15-30 minutes):
```
To be developed..
```

### Late Game Optimization (30-60 minutes):
```
To be developed..
```

---

## Visual Style

### Art Direction
- **Aesthetic:** Minimalist infographic meets resource management
- **Tone:** Clean, professional, satisfying
- **References:** Mini Metro, Thronefall, Monument Valley

### Color Palette (TO BE UPDATED!!!)
```
!!! Complete color palette TO BE UPDATED !!!
BACKGROUND:
- Hive/Ground: Dark gray (#2a2a2a)
- Grid: Subtle white (#ffffff at 10%)

BIOMES:
- Forest: Green (#00aa44)
- Mountain: Gray (#888888)
- Desert: Orange (#ff8844)
- Plains: Yellow (#ffcc44)
- Coastal: Blue (#4488ff)
- Tundra: Cyan (#44ccff)

BUILDINGS:
- Flower Patch: Biome color + white accent
- Hive: Multi-color (all biome colors mixed)

UI:
- Money: Gold (#ffaa00)
- Positive: Green (#00ff00)
- Locked: Dark gray (#444444)
```

### Camera & View
- **Camera:** Orthographic, 45° top-down angle
- **Zoom:** Optional (scroll to zoom in/out)
- **Pan:** Optional (drag or edge-scroll)
- **Focus:** Hive is center, flower patchs around it

---

## Technical Setup

### Unity Configuration
- **Version:** Unity 6.2
- **Pipeline:** URP (better WebGL performance)
- **Target:** WebGL build, 60 FPS with 100+ Bees

### Scene Layout (TO BE UPDATED!!!)
```
!!!TO BE UPDATED!!!
World: 40x40 unit grid
Hive: Center (0, 0, 0)
Flower Patchs: Spawn in ring around hive
  - Close biomes: 10-15 units from center
  - Far biomes: 20-30 units from center
```

### Performance Target
- Use object pooling if needed (simple optimization), if 60fps not sustainable

---

## Critical Design Pillars

### 1. SATISFYING AUTOMATION
Watch your network operate automatically. No micromanagement.

### 2. EXPONENTIAL GROWTH
Income scales 10x, 50x, 200x through combos. Incremental satisfaction.

### 3. MEANINGFUL CHOICES
Bees assignement, upgrade paths, unlock order. No obvious "correct" answer.

### 4. VISIBLE OPTIMIZATION
Bottlenecks are clear. Solutions are multiple. Player feels smart.

### 5. RAPID PROTOTYPING
Scope for few weeks validation. Expandable if successful.

---

## What Makes This Fun?

✅ **Optimization:** "How do I fix this bottleneck?"  
✅ **Growth:** "My income went from $10/sec to $500/sec!"  
✅ **Problem-Solving:** "Multiple ways to solve this. What's best?"  

**Most Important:** Every decision has **opportunity cost**. That's what creates strategy.

---
