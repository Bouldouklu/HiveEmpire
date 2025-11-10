# HIVE EMPIRE - Game Concept Document

**Version:** Prototype v1.0  
**Target:** 3-4 week prototype for itch.io WebGL  
**Core Focus:** Incremental optimization with strategic resource management

---

## High-Level Concept

> **"Build an automated pollen network connecting resource-producing flower patchs to your central hive. Discover synergies, manage bottlenecks, and optimize production chains for exponential growth."**

**Genre:** Incremental Strategy + Resource Management  
**Style:** Minimalist 3D, top-down view (Mini Metro aesthetic)  
**Platform:** WebGL (browser-based)  
**Core Appeal:** Satisfying optimization, exponential growth, strategic choices

---

## The Core Hook

**NOT about:** Perfect flight path routing  
**IS about:** Identifying bottlenecks, strategic upgrades, resource synergy optimization

**Player Fantasy:** "I'm building an efficient pollen empire by understanding which resources combine for maximum profit."

---

## Core Game Loop

```
Flower Patch generates Resource (different types of pollen)
    ↓
Bee automatically delivers to Hive
    ↓
Hive COMBINES resources → Higher value products
    ↓
Earn exponential Money
    ↓
Spend on: New Flower Patchs, Upgrades, Unlock Biomes
    ↓
Optimize bottlenecks, balance production rates
    ↓
LOOP with increasing complexity
```

---

## Win Condition

**Goal:** Maximize income per second from ONE hive  
**Failure state** - TBD  
**Endgame** - TBD

---

## Core Mechanics

### 1. RESOURCE SYNERGY SYSTEM (The Heart)

Resources have **exponential value** when combined at the hive.

#### Individual Resources (Low Value):
```
Wood (Forest)  = $1
Stone (Mountain) = $1
Oil (Desert) = $1
Food (Plains) = $1
```

#### Synergy Combinations (High Value):
```
TWO RESOURCES:
Wood + Stone → Buildings = $10 (10x!)
Wood + Oil → Plastics = $8
Stone + Oil → Concrete = $12
Food + Oil → Chemicals = $9

THREE RESOURCES:
Wood + Stone + Oil → Advanced Materials = $50
Wood + Food + Stone → Trade Goods = $40
Stone + Oil + Food → Industrial Products = $45

FOUR RESOURCES:
Wood + Stone + Oil + Food → Tech Products = $200
```

**Strategic Depth:**
- Early: Connect 2 biomes for first combo
- Mid: Choose which third biome unlocks best triple combo
- Late: Balance all four for maximum multiplier

---

### 2. LIMITED CONNECTION SLOTS (Forces Choices)

**The Constraint:** Hive can only connect to **limited number of flower patchs**

```
Starting: 2 connection slots (free)
Upgrade 1: 3 slots ($500)
Upgrade 2: 4 slots ($2000)
Upgrade 3: 5 slots ($10000)
```

**Why This Matters:**
- You discover 6+ biome types
- Can only connect 2-3 at start
- Must choose: "Which resources create best combos?"
- Can disconnect/reconnect (costs money)

**Creates Trade-offs:**
- "Add Desert for triple combo, but disconnect Food?"
- "Save money for slot upgrade, or unlock new biome?"
- "This combo is good now, but will I regret it later?"

---

### 3. BIOMES & RESOURCES

#### Starting Biomes (Always Available):
- **Forest** → Wood (fast generation: 1 per 2 seconds)
- **Plains** → Food (fast generation: 1 per 2 seconds)

#### Unlockable Biomes (Choose Unlock Order):
- **Mountain** → Stone (medium: 1 per 4 seconds) [$100 to unlock]
- **Desert** → Oil (slow: 1 per 6 seconds) [$150 to unlock]
- **Coastal** → Fish (medium: 1 per 3 seconds) [$100 to unlock]
- **Tundra** → Minerals (slow: 1 per 5 seconds) [$200 to unlock]

#### Future Expansion Biomes:
- Jungle → Exotic Wood
- Volcano → Rare Metals
- Savanna → Livestock
- Swamp → Chemicals

**Generation Rate = Strategic Consideration**
- Fast biomes: Reliable but may oversupply
- Slow biomes: Bottleneck but unlock high-value combos
- Player must balance production rates

---

### 4. AIRPORTS (Buildings)

#### Basic Flower Patch
- **Function:** Generates 1 resource type based on biome
- **Visual:** Simple 2x2 platform with biome-colored accent
- **Cost:** $50 (after first free flower patch per biome)
- **Placement:** Appears in discovered biome when purchased

#### Flower Patch Upgrades (Choose Specialization Path):

**PRODUCER Path:**
```
Tier 1: +50% generation speed ($50)
Tier 2: +100% generation speed ($150)
Tier 3: Generates 2 resources simultaneously ($500)
```
**When to use:** Bottleneck is resource production

**SPEED HUB Path:**
```
Tier 1: Bees 2x faster from this flower patch ($75)
Tier 2: Bees 3x faster + carry 2 items ($200)
Tier 3: Instant delivery (teleport-like) ($600)
```
**When to use:** Flower Patch is far from hive (slow delivery)

**PROCESSOR Path:**
```
Tier 1: Can pre-combine 2 resources before sending ($100)
Tier 2: Combines 3 resources, sends high-value product ($300)
Tier 3: Generates bonus resources on combos ($800)
```
**When to use:** Want to create value chains at source

**STORAGE Path:**
```
Tier 1: Buffers 10 resources (smooths timing) ($40)
Tier 2: Generates passive income from stored goods ($120)
Tier 3: Overflow converts to multiplier bonus ($400)
```
**When to use:** Mismatched generation rates, need buffer

**No "correct" specialization** → player expression and problem-solving

---

### 5. THE CITY (Central Hub)

#### Hive Function:
- **Receives:** All pollen deliveries
- **Combines:** Resources into high-value products automatically
- **Pays Out:** Money based on combination value
- **Has:** Limited connection slots (upgradable)

#### Hive Upgrades:
```
Processing Speed: How fast hive combines resources
- Tier 1: 1 combo per 3 seconds ($100)
- Tier 2: 1 combo per 1 second ($500)
- Tier 3: 2 combos per 1 second ($2000)

Connection Slots: How many flower patchs can connect
- Start: 2 slots (free)
- Tier 2: 3 slots ($500)
- Tier 3: 4 slots ($2000)
- Tier 4: 5 slots ($10000)

Storage Capacity: Holds resources before combining
- Tier 1: 5 of each resource ($200)
- Tier 2: 10 of each ($600)
- Tier 3: Unlimited ($2000)
```

---

### 6. AIRPLANES (Automated Transport)

**Visual:** Small white spheres with cyan trails  
**Behavior:** Fully automated (no player control needed)  
**AI Logic:**
```
1. Spawn at Flower Patch when resource ready
2. Fly to Hive along straight line route
3. Deliver pollen to Hive
4. Return to Flower Patch
5. Repeat
```

**No route drawing needed!** - AIRPLANES automatically find Hive from any Flower Patch

**Bee Properties:**
- Speed: 3 units/second (affected by Flower Patch Nectar Flow upgrades)
- Capacity: 1 resource (upgradable via Flower Patch upgrades)
- Visual feedback: Carries visible colored cube (resource type)

---

### 7. PROGRESSION & UNLOCKS

#### Money Economy

**Earning Money:**
- Single resource delivered: $1
- Two-resource combo: $8-12
- Three-resource combo: $40-50
- Four-resource combo: $200+

**Income scales exponentially with combos!**

#### Unlock Progression:
```
$0 → Start
  ✓ Hive + Forest + Plains
  ✓ 2 connection slots

$100 → Unlock First Tier Biome
  ✓ Choose: Mountain or Coastal
  ✓ Enable two-resource combos

$250 → Flower Patch Upgrade Available
  ✓ Can specialize flower patchs

$500 → Third Connection Slot
  ✓ Can connect 3 biomes
  ✓ Enable three-resource combos

$1000 → Unlock Second Tier Biome
  ✓ Desert or Tundra available

$2000 → Fourth Connection Slot
  ✓ Four-resource combos possible

$5000 → Processing Speed Upgrade

$10000 → Fifth Connection Slot
  ✓ Can have all resources flowing

$50000 → Prestige Available
  ✓ "Expand to New Continent"
```

---

### 8. PRESTIGE SYSTEM (Meta-Progression)

**"Expand to New Continent"**

**Trigger:** After earning $50,000  
**Effect:** Reset everything, BUT choose ONE permanent bonus:

```
"Industrial Tycoon"
  → All Producers generate 50% faster (forever)

"Logistics Master"  
  → All Bees 2x speed (forever)

"Resource Abundance"
  → Start with 3 connection slots instead of 2

"Synergy Specialist"
  → All combos give +50% value (forever)

"Fast Track"
  → Start with Mountain + Desert unlocked
```

**Strategic Depth:**
- Different bonuses enable different strategies
- First run: Learn combos, experiment
- Second run: Optimize based on permanent bonus
- Third run: Try different bonus, new strategy emerges

---

## Strategic Decision Examples

### Opening (0-5 minutes):
```
Have: Hive + Forest (wood) + Plains (food)
Slots: 2/2 (full)
Money: $0

Decision: "Upgrade Forest to generate faster ($50)
          or save for Mountain unlock ($100)?"

Strategy A: Upgrade → faster early income
Strategy B: Save → unlock combos sooner
```

### Early Mid-Game (5-15 minutes):
```
Have: Forest + Mountain connected (Wood+Stone combo active)
Combo: Buildings = $10 per delivery
Bottleneck: Stone generates slowly
Money: $200

Decision: "Upgrade Mountain to Producer ($50)
          or unlock Desert ($150) for triple combos?"

Strategy A: Fix bottleneck now
Strategy B: Long-term greed, save for Desert
```

### Mid-Game Pivot (15-30 minutes):
```
Have: Forest, Mountain, Desert unlocked
Slots: 2/3 (Hive upgraded)
Current: Forest + Mountain connected
Potential: Wood+Stone+Oil = $50 (vs current $10)

Decision: "Disconnect Forest, connect Desert?
          Or disconnect Mountain, connect Desert?"

Trade-off: Lose current steady income to chase 5x multiplier
Risk vs Reward calculation
```

### Late Game Optimization (30-60 minutes):
```
Have: All 4 basic biomes, 4 connection slots
Tech Combo: Wood+Stone+Oil+Food = $200
Bottleneck: Oil is slowest, limits Tech combo rate

Multiple Solutions:
A) Upgrade Desert to Producer (more oil) $150
B) Upgrade Desert to Nectar Flow (faster delivery) $75
C) Add second Desert flower patch (parallel generation) $50
D) Upgrade Hive processing speed (combine faster) $500
E) Upgrade Desert to Processor (pre-combine at source) $100

NO "correct" answer → player creativity!
```

---

## Visual Style

### Art Direction
- **Aesthetic:** Minimalist infographic meets resource management
- **Tone:** Clean, professional, satisfying
- **References:** Mini Metro, Dorfromantik, Monument Valley

### Color Palette
```
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
- **Version:** Unity 2022 LTS or Unity 6
- **Pipeline:** URP (better WebGL performance)
- **Target:** WebGL build, 60 FPS with 100+ Bees

### Scene Layout
```
World: 40x40 unit grid
Hive: Center (0, 0, 0)
Flower Patchs: Spawn in ring around hive
  - Close biomes: 10-15 units from center
  - Far biomes: 20-30 units from center
```

### Performance Target
- 100 Bees: 60 FPS (easy)
- 200 Bees: 50+ FPS (target)
- Use object pooling if needed (simple optimization)

---

## Week-by-Week Prototype Plan

### WEEK 1: Core Loop (Validate Fun)

**Goal:** Prove that resource synergy + bottleneck optimization is satisfying

**Day 1-2: Foundation**
- [ ] Top-down camera setup
- [ ] Hive building (center, receives pollen)
- [ ] 2 Flower Patch types: Forest (wood), Plains (food)
- [ ] Click to build Flower Patch ($50)
- [ ] Basic resource generation (1 per 2 seconds)

**Day 3-4: Automation**
- [ ] Bee spawning at Flower Patch
- [ ] Bee flies to Hive
- [ ] Delivers resource to Hive
- [ ] Single resource = $1 income

**Day 5-6: The Hook - Synergies**
- [ ] Mountain Flower Patch (stone) unlockable ($100)
- [ ] Hive combines Wood + Stone → Buildings ($10)
- [ ] Visual feedback: combo notification, money earned
- [ ] Combo counter: "Buildings created: X"

**Day 7: Balancing & Testing**
- [ ] Tune generation rates
- [ ] Tune costs
- [ ] Playtest: "Is watching combos satisfying?"

**Week 1 Success Criteria:**
✅ Connecting multiple biomes feels good  
✅ Seeing combos create value is satisfying  
✅ Want to unlock more biomes  
✅ Can identify "Stone is my bottleneck"  

---

### WEEK 2: Strategic Depth

**Goal:** Add meaningful choices and optimization challenges

**Tasks:**
- [ ] Connection Slot limit (2 slots, upgradable to 3 for $500)
- [ ] Desert Flower Patch (oil) - slow generation
- [ ] Triple combo: Wood+Stone+Oil = $50
- [ ] Flower Patch upgrade system: Producer specialization (+50% speed)
- [ ] Visual distinction between specialized flower patchs
- [ ] Money counter shows "$/sec" rate
- [ ] Bottleneck identification: visual indicator on slow flower patchs

**Week 2 Success Criteria:**
✅ Limited slots create meaningful choices  
✅ Upgrade decisions feel impactful  
✅ Can see and solve bottlenecks  
✅ Multiple valid strategies emerge  

---

### WEEK 3: Polish & Publish

**Goal:** Make it feel complete and publish to itch.io

**Tasks:**
- [ ] All 4 specialization paths (Producer/Speed/Processor/Storage)
- [ ] Hive upgrades (processing speed, storage)
- [ ] 6 biome types total
- [ ] Four-resource combo (Tech Products = $200)
- [ ] UI polish (tooltips, upgrade trees, stats screen)
- [ ] Visual polish (particles, animations, juice)
- [ ] Sound effects (optional):
  - Resource generated
  - Combo created
  - Money earned
  - Building placed
- [ ] Tutorial tooltips (first-time guidance)
- [ ] Balance pass (progression curve)
- [ ] WebGL build optimization
- [ ] Itch.io page creation

**Week 3 Success Criteria:**
✅ Feels like complete game  
✅ WebGL runs smoothly  
✅ Friends want to play more  
✅ Published and getting plays  

---

### WEEK 4+ (If Successful):

**Phase 2 - Meta Content:**
- [ ] Prestige system with permanent bonuses
- [ ] 3 new biome types
- [ ] Advanced combos (5+ resources)
- [ ] Achievements
- [ ] Stats tracking
- [ ] Leaderboard integration

**Phase 3 - Steam Considerations:**
- [ ] Multiple maps/continents
- [ ] Challenge modes
- [ ] Endless mode vs target modes
- [ ] Workshop support
- [ ] More visual polish

---

## Critical Design Pillars

### 1. SATISFYING AUTOMATION
Watch your network operate automatically. No micromanagement.

### 2. EXPONENTIAL GROWTH
Income scales 10x, 50x, 200x through combos. Incremental satisfaction.

### 3. MEANINGFUL CHOICES
Limited slots, upgrade paths, unlock order. No obvious "correct" answer.

### 4. VISIBLE OPTIMIZATION
Bottlenecks are clear. Solutions are multiple. Player feels smart.

### 5. RAPID PROTOTYPING
Scope for 3-week validation. Expandable if successful.

---

## What Makes This Fun?

✅ **Discovery:** "What do these resources combine into?"  
✅ **Optimization:** "How do I fix this bottleneck?"  
✅ **Growth:** "My income went from $10/sec to $500/sec!"  
✅ **Choices:** "Should I specialize or diversify?"  
✅ **Problem-Solving:** "Multiple ways to solve this. What's best?"  

**Most Important:** Every decision has **opportunity cost**. That's what creates strategy.

---

## Success Metrics

### Prototype Validation (Week 1-2):
- Playtester session length: 15+ minutes (engaged)
- Comments: "Just one more upgrade..."
- Can articulate their strategy: "I'm rushing Desert for triple combos"

### Itch.io Launch (Week 3-4):
- 500+ plays in first month
- 4+ star rating
- Positive comments about depth/strategy
- Some players return multiple times (retention)

### Go/No-Go Decision:
- **Continue to Steam if:** 1000+ plays, 4+ stars, community requests more
- **Pivot if:** <100 plays, <3 stars, feedback says "boring/shallow"
- **Quick iteration if:** Good engagement but specific complaints ("too slow", "too easy", etc.)

---

## FAQ / Design Choices

**Q: Why not manual route drawing like Mini Metro?**  
A: Focus is optimization, not routing. Automation keeps it incremental-focused.

**Q: Why limited connection slots?**  
A: Creates strategic trade-offs. Without limits, answer is always "add everything."

**Q: Why specialization paths?**  
A: Player expression. Different problems, different solutions. Replayability.

**Q: Why exponential combo values?**  
A: Core of incremental games. 10x jumps feel great. Linear growth is boring.

**Q: Why synergies instead of just "more flower patchs = more money"?**  
A: Strategy requires choices. Synergies create "which combination?" puzzle.

---

## Open Questions (Decide During Prototyping)

**Flower Patch Placement:**
- Player chooses location? OR
- Auto-spawns in biome zones?
- **Recommendation:** Auto-spawn for simplihive (Week 1), add placement in Week 2 if needed

**Bee Count:**
- One Bee per flower patch? OR
- Global Bee pool? OR
- Automatic scaling (more Bees spawn as needed)?
- **Recommendation:** Auto-scale (more flower patchs = more Bees automatically)

**Save System:**
- LocalStorage (save progress) OR
- Session-only (reset on close)?
- **Recommendation:** LocalStorage for retention

**Visual Distance:**
- Do far biomes actually look far? OR
- Abstract representation?
- **Recommendation:** Visually far (creates timing gameplay)

---

## Next Steps

1. **Generate visuals** using image prompts (see SKYLINES_ImagePrompts.md)
2. **Set up Unity project** (URP, orthographic camera, grid)
3. **Build Week 1 prototype** (focus on combo validation)
4. **Playtest at Day 7** (is synergy fun?)
5. **Iterate or pivot** based on feedback

---

**Remember:** The goal is to validate "resource synergy + bottleneck optimization" is fun. Everything else is secondary. If Week 1 doesn't feel good, pivot quickly!

---

*This is your starting point. Adapt as you learn. Prototype fast, fail fast, iterate fast.*
