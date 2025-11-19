# HIVE EMPIRE - Game Mechanics Summary for Economy Simulator

## Core Game Loop

```
Flower Patch → Bee gathers pollen (2.5s) → Bee flies to Hive (distance/speed dependent)
→ Hive stores pollen (100-130 capacity per type) → Recipe consumes pollen (priority-based)
→ Recipe produces honey → Player earns money → Player invests in upgrades/bees → LOOP
```

---

## 1. RESOURCE GENERATION

### Flower Patches
- **Total Available:** 6 flower patches in the game
- **One Pollen Type Per Patch:** Each flower patch produces exactly one type of pollen (6 pollen types total)
- **Distance Range:** Patches are positioned at distances ranging from 7 to 44 units from the hive
- **Infinite pollen dispensers** (no generation rate limits)
- Pollen created **instantly** when bee arrives
- **Bees are the only bottleneck** for resource flow

### Biome Distance Configuration
```
Current 6 Biomes with Distance to Hive:
  - Biome 1: 7 units   (closest - fastest bee round-trips)
  - Biome 2: 15 units
  - Biome 3: 22 units
  - Biome 4: 28 units
  - Biome 5: 36 units
  - Biome 6: 44 units  (farthest - slowest bee round-trips)

Distance Impact on Transport:
  - Shorter distance = Faster round-trips = Higher throughput per bee
  - Longer distance = Slower round-trips = Lower throughput per bee
  - Distance affects both directions (to hive and back to patch)
```

### Gathering Phase
- **Duration:** 2.5 seconds (configurable per biome)
- Bee hovers at patch with Perlin noise movement
- After gathering, bee picks up 1 pollen and flies to hive

### Capacity System
```
Base Capacity: 5 bees per patch
Upgrades: 5 tiers (5 → 10 → 15 → 20 → 25 → 30 bees max)
Costs: $50, $150, $400, $900, $2000 (total: $3,500)
```

---

## 2. TRANSPORTATION MECHANICS

### Bee Behavior
- **Base Speed:** 6 units/second
- **Flight Path:** Cubic Bezier curve with arc altitude of 2 units
- **Pollen Capacity:** 1 pollen per trip (no upgrades)
- **Journey Cycle:** Patch → Gather (2.5s) → Fly to Hive → Deliver → Fly back → REPEAT

### Critical Transport Formula
```
Arc Length ≈ √(distance² + (2 × altitude)²) × 1.1
One-Way Time = Arc Length / (Base Speed × Season Modifier)
Round-Trip Time = One-Way Time × 2
Spawn Interval = Round-Trip Time / Allocated Bees

Example 1 - Closest Patch (7 units distance, 6 base speed, Spring 1.1× modifier, 5 bees):
  Arc Length ≈ √(7² + 4²) × 1.1 ≈ √65 × 1.1 ≈ 8.9 units
  Effective Speed = 6 × 1.1 = 6.6 units/s
  One-Way Time = 8.9 / 6.6 ≈ 1.35 seconds
  Round-Trip = 2.7 seconds
  Spawn Interval = 2.7s / 5 bees ≈ 0.54 seconds between spawns

Example 2 - Farthest Patch (44 units distance, 6 base speed, Summer 0.9× modifier, 5 bees):
  Arc Length ≈ √(44² + 4²) × 1.1 ≈ √1952 × 1.1 ≈ 48.6 units
  Effective Speed = 6 × 0.9 = 5.4 units/s
  One-Way Time = 48.6 / 5.4 ≈ 9.0 seconds
  Round-Trip = 18.0 seconds
  Spawn Interval = 18s / 5 bees ≈ 3.6 seconds between spawns

Distance Impact: 6.3× difference in throughput between closest and farthest patches
```

### Seasonal Speed Modifiers
```
Spring:  1.1× (6.6 units/s) - Fast bees
Summer:  0.9× (5.4 units/s) - Slow bees
Autumn:  1.0× (6.0 units/s) - Baseline
```

---

## 3. STORAGE SYSTEM

### Hive Inventory
```
Base Capacity: 100 pollen per type
Seasonal Modifiers:
  - Spring: 120 (1.2×)
  - Summer: 100 (1.0×)
  - Autumn: 130 (1.3×)
```

### Overflow Handling
- Storage full → **pollen discarded** (no penalties, just waste)
- No global storage upgrades (fixed per pollen type)
- If needed, implementation of upgrade system to serve gameplay.

---

## 4. RECIPE PRODUCTION

### Priority-Based System
1. Recipes execute in **list order** (top = highest priority)
2. Each frame: Check if ingredients available
3. If yes → **consume resources immediately** → start production timer
4. Multiple recipes can produce **simultaneously**

### Production Formula
```
Final Production Time = Base Time × (1 - Tier Reduction%) × Season Modifier
Recipe Value = Base Value × (1 + Tier Increase%) × Season Income Modifier
```

### Recipe Tier System (6 tiers: 0-5)
```
Tier | Ingredient Reduction | Time Reduction | Value Increase | Upgrade Cost
-----|---------------------|----------------|----------------|-------------
  0  |        0%           |       0%       |       0%       |      -
  1  |       10%           |      15%       |      20%       |    $100
  2  |       20%           |      25%       |      40%       |    $300
  3  |       30%           |      35%       |      60%       |    $800
  4  |       40%           |      50%       |     100%       |   $2,000
  5  |       50%           |      60%       |     150%       |   $5,000

Total cost to max tier: $8,200 per recipe
```

### Example Recipe Scaling
```
Base Recipe (Tier 0, Spring):
  - Ingredients: 1× Wild Meadow Pollen
  - Production Time: 5 seconds
  - Value: $2

Max Recipe (Tier 5, Summer):
  - Ingredients: 1× Wild Meadow Pollen (can't reduce below 1)
  - Production Time: 2 seconds (5s × 0.4)
  - Value: $5 × 1.5 = $7.50

Income rate increase: 9.375× (2.5× value, 2.5× speed, 1.5× season)
```

---

## 5. ECONOMIC SYSTEM

### Starting Economy
```
Starting Money: $50
Starting Bees: 2 bees (global pool)
```

### Income Formula
```
Income Per Recipe = Base Value × (1 + Tier Increase%) × Season Income Modifier
```

### Money Sinks

**A. Bee Fleet Purchases (10 tiers)**
```
Tier | Cost     | Bees Added | Cumulative Bees | Cumulative Cost
-----|----------|------------|-----------------|----------------
  0  | $25      | 2          | 2               | $25
  1  | $60      | 3          | 5               | $85
  2  | $150     | 5          | 10              | $235
  3  | $350     | 8          | 18              | $585
  4  | $800     | 12         | 30              | $1,385
  5  | $1,800   | 18         | 48              | $3,185
  6  | $4,000   | 25         | 73              | $7,185
  7  | $8,500   | 35         | 108             | $15,685
  8  | $18,000  | 50         | 158             | $33,685
  9  | $38,000  | 70         | 228             | $71,685

Total for max fleet: $71,685
```

**B. Flower Patch Capacity Upgrades**
```
5 tiers: $50 → $150 → $400 → $900 → $2,000 (total: $3,500 per patch)
Bonus: +5 bees per tier (5 → 10 → 15 → 20 → 25 → 30 max)
```

**C. Recipe Tier Upgrades**
```
5 tiers: $100 → $300 → $800 → $2,000 → $5,000 (total: $8,200 per recipe)
```

**D. Flower Patch Unlock Costs**
```
Varies per biome: $50-$500 (configurable in FlowerPatchData)
```

**E. Recipe Unlock Costs**
```
Varies per recipe (configurable)
Some recipes require prerequisites (unlock tree)
```

### Upgrade Systems Impact Summary

**Four Interconnected Upgrade Paths:**

**1. Bee Fleet Purchases** (10 tiers, $25 → $38,000)
```
Impact: Increases global bee pool
Direct Effect: More bees available for allocation
Strategic Value: Unlocks ability to activate more patches simultaneously
Bottleneck: Most expensive upgrade path ($71,685 total)
ROI: Diminishing returns (bee #100 adds less value than bee #10)
Player Decision: "Do I need more bees, or better utilization of current bees?"
```

**2. Flower Patch Capacity Upgrades** (5 tiers per patch, $50 → $2,000)
```
Impact: Increases max bees per individual patch (5 → 30)
Direct Effect: Allows concentrating more bees on high-value patches
Strategic Value: Removes allocation bottleneck for critical patches
Cost Multiplier: 6 patches × $3,500 = $21,000 to max all capacities
ROI: High for close/high-value patches, low for distant patches
Player Decision: "Which patches deserve capacity investment?"
Example: 7-unit patch (fast) vs 44-unit patch (slow) - prioritize close patch
```

**3. Recipe Tier Upgrades** (5 tiers per recipe, $100 → $5,000)
```
Impact: Increases recipe value (+150% at Tier 5), reduces production time (-60%)
Direct Effect: Exponentially higher income per recipe completion
Strategic Value: Compounds with seasonal bonuses (Tier 5 + Summer = 3.75× base value)
Cost Per Recipe: $8,200 to max tier
ROI: Extremely high - income rate increase of 9.375× (value + speed + season)
Player Decision: "Which recipe to upgrade first? Single-pollen or multi-pollen?"
Priority: Upgrade recipes you can SUSTAIN (have pollen supply for)
```

**4. Flower Patch Unlocks** ($50 → $500 per biome)
```
Impact: Unlocks new pollen type and patch location
Direct Effect: Enables new recipes requiring that pollen type
Strategic Value: Exponential when unlocking multi-pollen recipe prerequisites
Cost: Relatively cheap (early-game accessible)
ROI: High if it unlocks valuable recipes, low if pollen type unused
Player Decision: "Do I need this pollen type for a recipe I want to produce?"
Distance Factor: Close patches (7-15 units) more valuable than distant (36-44 units)
```

**Synergies Between Upgrade Paths:**
```
Optimal Build Example (Week 12, ~$10,000 spent):
  1. Unlock 3 close patches ($150 total)
  2. Buy Bee Fleet Tier 3 (18 bees total, $585)
  3. Upgrade capacity on 2 close patches to Tier 2 ($200 each = $400)
  4. Upgrade 2 valuable recipes to Tier 3 ($1,200 each = $2,400)
  5. Allocate bees to upgraded patches, sustain upgraded recipes
  Result: Strong foundation, positioned for Summer income spike

Trap Build Example (poor synergy):
  1. Unlock all 6 patches ($1,500+ total)
  2. Buy Bee Fleet Tier 2 (10 bees, $235)
  3. Spread 10 bees across 6 patches (1-2 bees per patch)
  Result: Weak throughput everywhere, can't sustain any recipes effectively
```

**Cumulative Costs for "Full Build":**
```
Max Fleet (228 bees): $71,685
Max All Patch Capacities (6 × $3,500): $21,000
Unlock All Patches (6 patches): ~$1,500
Max All Recipes (assume 8 recipes): ~$65,600
TOTAL "Perfect Build": ~$159,785

Impossible within 24-week campaign - forces trade-offs and prioritization
```

---

## 6. SEASONAL MODIFIERS

### Campaign Structure

**3-Season Campaign (24 Weeks Total):**
```
Total Duration: 24 weeks (default: 60s per week = 24 minutes real-time)
  - Weeks 1-8:   Spring (8 weeks)
  - Weeks 9-16:  Summer (8 weeks)
  - Weeks 17-24: Autumn (8 weeks)
  - Week 25+:    Winter triggers campaign end → End-of-Year Summary
```

**Campaign as Core Constraint:**
- **Fixed time limit** creates urgency and prevents infinite grinding
- Players must optimize within 24-week window (no "eventually I'll unlock everything")
- End-of-Year Summary displays performance metrics and high scores
- Encourages replayability - "Can I earn more next run?"
- Different strategies may be optimal for different campaign lengths

**Strategic Implications of Time Constraint:**
```
Early Weeks (1-4):
  - Limited income, expensive upgrades feel out of reach
  - Must choose: Expand (unlock patches) vs Optimize (upgrade recipes/capacity)
  - Every minute counts - poor decisions have lasting impact

Mid Weeks (5-16):
  - Summer income bonus (Week 9-16) is critical earning window
  - Must position for Summer: Unlock high-value recipes BEFORE Week 9
  - Summer's slow bee speed (-10%) creates different optimal allocation

Late Weeks (17-24):
  - Final push to maximize total earnings
  - No time to recover from bad investments
  - Autumn storage bonus (1.3×) helps buffer production spikes
  - "One more upgrade" risk - will it pay off before Week 25?
```

**Why 3 Seasons Creates Depth:**
- Each season has different optimal strategy (modifiers change trade-offs)
- Seasonal transitions force adaptation, not static "solved" builds
- 8-week cycles provide rhythm: Expand → Optimize → Adapt
- Summer income spike creates strategic "rush window" for profit maximization

### Modifier Table
```
Modifier                  | Spring | Summer | Autumn
--------------------------|--------|--------|--------
Income Multiplier         | 1.0×   | 1.5×   | 1.2×
Bee Speed Multiplier      | 1.1×   | 0.9×   | 1.0×
Production Time Mult.     | 1.0×   | 0.85×  | 1.1×
Storage Capacity Mult.    | 1.2×   | 1.0×   | 1.3×
```

### Strategic Implications
- **Spring:** Fast expansion (fast bees, bonus storage, baseline income)
- **Summer:** Maximum profit (1.5× income, fast production, slow bees)
- **Autumn:** Final optimization (max storage, slow production, moderate income)

---

## 7. STRATEGIC CONSTRAINTS

### A. Global Bee Pool (Primary Constraint)

**Core Strategic Mechanic:**
- **Single shared pool** of bees across ALL flower patches (not per-patch ownership)
- Starting with only 2 bees forces immediate allocation decisions
- Maximum theoretical pool: 228 bees (requires $71,685 investment across 10 tiers)

**Allocation Mechanics:**
- Players manually allocate/deallocate bees via Fleet Management Panel
- Allocation is NOT automatic - deliberate player choice required
- Deallocated bees return to available pool immediately
- When deallocating, excess bees beyond available pool are destroyed

**Strategic Depth:**
```
Early Game (2-10 bees):
  - Every bee allocation is critical
  - Can only activate 1-2 patches effectively
  - Must choose between exploring new patches vs optimizing existing ones

Mid Game (10-50 bees):
  - Competition between patches intensifies
  - Distance efficiency becomes critical (close patches vs distant patches)
  - Must balance multiple pollen types for multi-pollen recipes
  - Capacity limits start constraining individual patches (5-bee caps)

Late Game (50-228 bees):
  - Complex optimization puzzle with 6 patches competing for bees
  - Capacity upgrades required to fully utilize large bee fleet
  - Seasonal changes force reallocation (e.g., deprioritize distant patches in Summer)
  - Diminishing returns - 100th bee adds less value than 10th bee
```

**Example Allocation Trade-offs:**
```
Scenario: Player has 10 bees, 3 patches unlocked (7 units, 22 units, 44 units)

Option A - Distance Optimization:
  - Allocate 5 bees to closest patch (7 units) = High throughput, single pollen type
  - Allocate 5 bees to medium patch (22 units) = Moderate throughput, two pollen types
  - Result: Fast income from simple recipes, limited growth potential

Option B - Synergy Strategy:
  - Allocate 3 bees to each patch = Lower throughput per patch
  - Unlock multi-pollen recipes requiring all 3 types
  - Result: Slower start, exponentially higher income potential

Option C - Seasonal Hedging:
  - Reallocate during seasonal transitions
  - Spring: Favor distant patches (fast bee speed 1.1×)
  - Summer: Pull bees from distant patches (slow bee speed 0.9×)
  - Result: Dynamic optimization, more micro-management
```

**Why This Creates Strategic Depth:**
- Forces meaningful trade-offs (not "build everything" incremental game)
- Distance creates natural tier system (close = efficient, far = inefficient)
- Capacity limits prevent single-patch dominance
- Reallocation allows experimentation and adaptation
- Scarcity creates opportunity cost for every decision

### B. Patch Capacity Limits
- Each patch has max bee capacity (base 5, upgrades to 30)
- Can't dump all bees on one optimal patch
- Forces diversification

### C. Storage Bottleneck
- Fixed capacity per pollen type (100-130)
- Overflow is **discarded** (waste)
- High production without consumption = inefficiency

### D. Distance-Based Transport Time
- **Critical strategic factor:** 6 patches at distances from 7 to 44 units (6.3× throughput difference)
- Distant patches have significantly longer round-trips (2.7s vs 18s in worst case)
- Slower throughput per bee on distant patches
- Mitigation: Allocate more bees to distant patches or prioritize closer patches during slow seasons (Summer)

### E. Multi-Pollen Recipe Requirements
- High-value recipes need multiple biomes unlocked
- Requires coordinated bee allocation across multiple patches
- **Exponential income scaling** reward for complexity

---

## 8. SUCCESS METRICS

### Win Condition
- Maximize total money earned over 24 weeks
- No strict win/lose (incremental game)

### Tracked Statistics
```
Hero Stats:
  - Total Money Earned (all seasons)
  - Total Recipes Completed
  - Total Resources Collected

Economic Performance:
  - Starting Money ($50)
  - Ending Money
  - Highest Single Transaction

Empire Statistics:
  - Total Flower Patches Placed
  - Peak Bee Fleet Size

Seasonal Breakdown:
  - Per-season money, recipes, resources
```

---

## 9. BOTTLENECK PROGRESSION

### Early Game (Weeks 1-4)
- **Bottleneck:** Bee pool (2-10 bees total)
- **Focus:** Unlock patches, allocate scarce bees
- **Income:** Low ($2-$5/recipe)

### Mid Game (Weeks 5-12)
- **Bottleneck:** Patch capacities (5-bee limits), storage overflow
- **Focus:** Upgrade capacities, unlock multi-pollen recipes
- **Income:** Moderate ($10-$50/recipe)

### Late Game (Weeks 13-24)
- **Bottleneck:** Complex allocation optimization (50+ bees, 10+ patches)
- **Focus:** Recipe tier upgrades, Summer income maximization
- **Income:** High ($100-$500/recipe with Tier 5 + Summer bonuses)

---

## 10. ECONOMIC FEEDBACK LOOPS

### Positive Loops (Exponential Growth)
```
1. Bee Investment: More bees → More pollen → More recipes → More income → Buy more bees
2. Recipe Tier: Upgrade recipe → Higher value → More income → Upgrade more recipes
3. Multi-Pollen Synergy: Unlock biomes → High-value recipes → Exponential income
4. Capacity Expansion: Upgrade capacity → More bees per patch → More throughput
```

### Negative Loops (Balancing)
```
1. Exponential Costs: Bee tiers cost $25 → $60 → $150 → $38,000
2. Diminishing Returns: 10th bee adds only 11% throughput increase
3. Storage Overflow: High production → Overflow → Wasted bees
4. Seasonal Trade-offs: Summer +50% income BUT -10% bee speed
```

---

## 11. KEY FORMULAS FOR SIMULATOR

### Transport Throughput
```
Deliveries Per Minute Per Bee = 60s / Round-Trip Time
Total Pollen Per Minute = Deliveries Per Minute × Allocated Bees × 1 pollen
```

### Storage Fill Rate
```
Fill Rate = Total Pollen Income - Recipe Consumption Rate
Time Until Full = Remaining Capacity / Fill Rate
Overflow Rate = max(0, Fill Rate) when storage full
```

### Income Rate
```
Recipe Income Rate = (Base Value × (1 + Tier Increase%) × Season Modifier) / Production Time
Total Income Rate = Sum of all active recipes' income rates
```

### ROI Analysis
```
Bee Purchase ROI = Income Increase Per Second / Bee Purchase Cost
Capacity Upgrade ROI = Income Increase / $3,500 total investment
Recipe Tier ROI = (Value Increase + Speed Benefit) / $8,200 total investment
```

---

## 12. SIMULATOR REQUIREMENTS

### Must Accurately Model:

1. **Distance-Based Transport**
   - Bezier arc length calculation (not straight-line distance)
   - Seasonal speed modifiers
   - Per-patch unique distances

2. **Priority-Based Recipe Consumption**
   - Recipes check in list order
   - Immediate ingredient consumption when available
   - Simultaneous multi-recipe production

3. **Storage Overflow**
   - Per-type capacity limits (100-130)
   - Discard overflow pollen (track waste)
   - No retroactive storage

4. **Seasonal Transitions**
   - Week 9: Spring → Summer (all modifiers change instantly)
   - Week 17: Summer → Autumn (all modifiers change instantly)
   - Week 25: Campaign ends (End-of-Year Summary)

5. **Dynamic Unlock Progression**
   - Recipes unlock mid-simulation (prerequisites)
   - Recipe tiers upgrade dynamically
   - Bee fleet purchases add to pool dynamically

### Critical State Variables:
```
Game State:
  - Money (float)
  - Total Bees (int)
  - Available Bees (int)
  - Allocated Bees Per Patch (int[])
  - Inventory Per Pollen Type (int[])
  - Storage Capacity Per Type (int[], seasonal)
  - Unlocked Recipes + Tiers (bool[], int[])
  - Active Productions (Recipe[], float[] timers)
  - Current Week (int, 1-24)
  - Current Season (enum)
  - Flower Patch States (unlocked, capacity tier, bee allocation)
```

### Recommended Approach:
- **Discrete Event Simulation** with time-stepped ticks (0.1s resolution)
- **Event-driven architecture** (bee arrival, recipe completion, season change)
- **Decision engine** to simulate player upgrade choices (AI logic or manual input)

---

## 13. PLAYER PLAYSTYLES & STRATEGIES

### Core Strategic Archetypes

The simulator should test multiple playstyles to identify balanced gameplay:

**1. "Bee Rush" Strategy**
```
Philosophy: Maximize bee fleet first, allocate later
Investment Priority:
  - Bee Fleet Purchases: HIGH (rush to Tier 5-6, 48-73 bees)
  - Patch Unlocks: LOW (only unlock 2-3 essential patches)
  - Capacity Upgrades: MEDIUM (upgrade only when bees exceed patch capacity)
  - Recipe Tiers: LOW (delay until late-game)

Strengths:
  - Large bee pool enables flexibility
  - Can quickly capitalize on discovered synergies
  - Strong mid-late game scaling

Weaknesses:
  - Slow early income (expensive bee purchases)
  - Underutilized bees without capacity upgrades
  - Misses Summer income spike (Week 9-16) due to slow recipe tier upgrades

Example Timeline:
  - Week 1-4: Struggle with 2-10 bees, limited patches
  - Week 5-12: Bee fleet explodes (30-50 bees), income accelerates
  - Week 13-24: Dominant fleet, upgrade recipes, maximize earnings
```

**2. "Patch Expansion" Strategy**
```
Philosophy: Unlock all patches early, diversify pollen sources
Investment Priority:
  - Patch Unlocks: HIGH (unlock 4-6 patches by Week 5)
  - Bee Fleet: MEDIUM (buy bees as needed to staff patches)
  - Recipe Tiers: HIGH (unlock multi-pollen recipes)
  - Capacity Upgrades: LOW (spread bees thin, don't hit caps)

Strengths:
  - Access to all pollen types enables multi-pollen recipes
  - Diversified income streams (not dependent on single patch)
  - Unlocks high-value recipes early

Weaknesses:
  - Spread thin - low bee count per patch = slow throughput
  - Unlock costs drain early capital
  - Distant patches (36-44 units) waste bee efficiency

Example Timeline:
  - Week 1-4: Rapid patch unlocking ($500-1,500 spent)
  - Week 5-8: Unlock multi-pollen recipes, struggle to sustain them
  - Week 9-16: Summer income spike, start scaling bee fleet
  - Week 17-24: Finally have bees to sustain all patches
```

**3. "Recipe Tier Rush" Strategy**
```
Philosophy: Maximize income per recipe, not total production
Investment Priority:
  - Recipe Tiers: HIGHEST (rush 1-2 recipes to Tier 5)
  - Patch Unlocks: LOW (only patches needed for target recipes)
  - Bee Fleet: MEDIUM (enough bees to sustain upgraded recipes)
  - Capacity Upgrades: MEDIUM (ensure patches can handle allocated bees)

Strengths:
  - Extremely high income rate per recipe (9.375× multiplier at Tier 5)
  - Compounds with Summer income (Week 9-16 = peak earnings)
  - Efficient use of limited bee fleet

Weaknesses:
  - Vulnerable to single recipe bottlenecks
  - Expensive early investment ($8,200 per recipe to max)
  - Requires sustaining specific pollen types (allocation pressure)

Example Timeline:
  - Week 1-6: Focus income on one recipe tier upgrade
  - Week 7-8: Achieve Tier 3-4 on primary recipe
  - Week 9-16: Dominate Summer with high-tier recipe income
  - Week 17-24: Diversify or continue milking primary recipe
```

**4. "Balanced/Adaptive" Strategy**
```
Philosophy: Respond to opportunities, no rigid plan
Investment Priority:
  - All paths equally weighted
  - Decisions based on current income rate and bottlenecks
  - Seasonal adaptation (reallocate bees, shift priorities)

Strengths:
  - Flexible, can pivot based on discovered synergies
  - Avoids over-investment in any single path
  - Strong seasonal optimization (e.g., favor close patches in Summer)

Weaknesses:
  - Lacks explosive scaling of specialized strategies
  - No clear "identity" - may feel unfocused
  - Requires more player skill/knowledge

Example Timeline:
  - Week 1-8: Unlock 2-3 patches, buy modest bee fleet (10-18 bees)
  - Week 5-8: Upgrade 1 recipe to Tier 2-3 before Summer
  - Week 9-16: Capitalize on Summer income, invest in bee fleet
  - Week 17-24: Finish recipe upgrades, optimize allocations
```

**5. "Efficiency/Close Patch" Strategy**
```
Philosophy: Prioritize close patches (7-22 units) for maximum throughput
Investment Priority:
  - Patch Unlocks: SELECTIVE (only unlock close patches)
  - Capacity Upgrades: HIGH (max out close patch capacities)
  - Bee Fleet: HIGH (stuff close patches with max bees)
  - Recipe Tiers: MEDIUM (upgrade recipes using close patch pollen)

Strengths:
  - Maximum throughput per bee (6.3× better than distant patches)
  - Avoids wasting bees on slow 36-44 unit patches
  - Simple allocation (all bees on 2-3 optimal patches)

Weaknesses:
  - Limited pollen diversity (misses multi-pollen recipes)
  - Vulnerable if close patch recipes are low-value
  - Hits capacity caps early (requires expensive upgrades)

Example Timeline:
  - Week 1-8: Unlock only 7-unit and 15-unit patches
  - Week 5-12: Max capacity on close patches (30 bees each)
  - Week 9-16: Sustain simple recipes at high throughput
  - Week 17-24: Potentially stuck with limited recipe options
```

**6. "Summer Maximizer" Strategy**
```
Philosophy: Position for Week 9-16 income spike (1.5× multiplier)
Investment Priority:
  - Pre-Week 9: Unlock patches, upgrade recipes to Tier 3+
  - Week 9-16: Maximize production volume (buy bees, allocate aggressively)
  - Post-Week 16: Coast on Summer earnings

Strengths:
  - Exploits strongest income window (50% bonus)
  - Forces disciplined early-game preparation
  - Clear milestone (be ready by Week 9)

Weaknesses:
  - Vulnerable to early-game mistakes (no time to recover)
  - Summer's slow bee speed (-10%) hurts distant patches
  - Autumn/Winter income drop feels punishing

Example Timeline:
  - Week 1-8: Rush recipe tiers, unlock patches, build foundation
  - Week 9-16: ALL IN - max production, earn 60-70% of total income
  - Week 17-24: Maintain infrastructure, diminishing returns
```

### Simulator Playstyle Testing

The economy simulator should test ALL six archetypes to validate:
1. **No single dominant strategy** - multiple paths to success
2. **Clear trade-offs** - each strategy has strengths and weaknesses
3. **Seasonal balance** - all seasons matter, not just Summer
4. **Accessible skill ceiling** - "Balanced" strategy performs decently, specialists excel

**Metrics to Compare Across Strategies:**
- Total money earned (Week 24)
- Peak income rate ($/second)
- Money earned per season (Spring/Summer/Autumn breakdown)
- Upgrade efficiency (ROI per dollar spent)
- Time to first major milestone ($1,000, $10,000, etc.)

---

## 14. BALANCING & CONFIGURABILITY

### All Numbers Are Tunable Parameters

**Critical Design Principle:**
> Every numeric value in this document is a **tunable parameter**, not a fixed constant.
> The economy simulator exists to test different configurations and identify optimal balance points.

### Configurable Parameters by Category

**1. Transport & Distance Parameters**
```
Currently Configured:
  - Bee Base Speed: 6 units/second
  - Flight Arc Altitude: 2 units
  - Gathering Duration: 2.5 seconds
  - Patch Distances: 7, 15, 22, 28, 36, 44 units

Why Configurable:
  - Patch distances create throughput tier system (6.3× difference)
  - Bee speed affects campaign pacing (faster = more player actions)
  - Gathering duration creates baseline production floor

Potential Adjustments:
  - Reduce distance spread (7-30 units instead of 7-44) = less distance penalty
  - Increase bee speed (8 units/s) = faster gameplay, more frequent income
  - Add gathering upgrades (reduce from 2.5s to 1.5s) = new upgrade path
```

**2. Economic Parameters (Costs & Income)**
```
Currently Configured:
  - Starting Money: $50
  - Bee Fleet Costs: $25 → $38,000 (10 tiers)
  - Recipe Base Values: $2 → $25+ (varies per recipe)
  - Upgrade Costs: $50 → $5,000 (varies per system)

Why Configurable:
  - Income vs cost balance determines upgrade frequency (player pacing)
  - Exponential cost scaling creates strategic scarcity
  - Starting money affects early-game difficulty

Potential Adjustments:
  - Increase starting money ($100) = easier early game, faster unlocks
  - Reduce bee tier costs (50% reduction) = more bees available, less scarce
  - Increase recipe base values (2× income) = faster upgrade cycle
```

**3. Seasonal Modifiers**
```
Currently Configured:
  Spring: 1.0× income, 1.1× speed, 1.0× production, 1.2× storage
  Summer: 1.5× income, 0.9× speed, 0.85× production, 1.0× storage
  Autumn: 1.2× income, 1.0× speed, 1.1× production, 1.3× storage

Why Configurable:
  - Modifiers create seasonal identity and force adaptation
  - Summer income spike (1.5×) is primary earning window
  - Speed/production trade-offs change optimal allocation

Potential Adjustments:
  - Reduce Summer income spike (1.3× instead of 1.5×) = less dominant
  - Add Winter season (4th season with harsh penalties) = longer campaign
  - Dynamic modifiers (scale with player progress) = adaptive difficulty
```

**4. Capacity & Upgrade Parameters**
```
Currently Configured:
  - Base Patch Capacity: 5 bees
  - Max Patch Capacity: 30 bees (5 tiers × +5 per tier)
  - Storage Capacity: 100 pollen per type (seasonal modifiers apply)
  - Recipe Tier Bonuses: 20% → 150% value increase

Why Configurable:
  - Capacity limits force diversification (prevent single-patch dominance)
  - Storage caps create overflow pressure (waste if not consumed)
  - Tier bonuses determine upgrade ROI (9.375× at Tier 5 very strong)

Potential Adjustments:
  - Increase base capacity (8 bees) = less upgrade pressure early
  - Add storage upgrades (unlock 150-200 capacity) = new money sink
  - Reduce tier bonuses (100% at Tier 5 instead of 150%) = less exponential
```

**5. Campaign & Time Parameters**
```
Currently Configured:
  - Total Duration: 24 weeks (3 seasons × 8 weeks)
  - Week Duration: 60 seconds real-time (24 minutes total)
  - Season Length: 8 weeks each

Why Configurable:
  - Campaign length affects replayability (shorter = more runs)
  - Week duration affects player pacing (60s vs 30s = different tempo)
  - Season length determines adaptation window

Potential Adjustments:
  - Shorten campaign (18 weeks = 3 seasons × 6 weeks) = faster runs
  - Lengthen weeks (90s per week) = more time to optimize
  - Add "Endless Mode" (no Week 25 end) = different audience
```

### Player Engagement Pacing Goal

**Target: Player Action Every 5-10 Seconds**

**What Counts as "Player Action":**
- Making a purchase (bee tier, recipe upgrade, patch unlock, capacity upgrade)
- Allocating/reallocating bees to patches
- Unlocking a new recipe
- Monitoring production and making strategic decision

**Current Pacing Analysis:**
```
Early Game (Weeks 1-4, $50 starting money):
  - First action (Patch unlock): ~10-20 seconds (earn $50-100)
  - Second action (Bee purchase Tier 0): ~30-40 seconds (earn $25)
  - Third action (Recipe unlock): ~60-90 seconds (earn $100+)

  Assessment: TOO SLOW for 5-10 second target
  Issue: Starting income too low, first upgrades too expensive

Mid Game (Weeks 5-12, ~$1,000 earned):
  - Action frequency: Every 20-40 seconds (earn $100-500 per action)

  Assessment: Closer to target, but still slow

Late Game (Weeks 13-24, ~$10,000+ earned):
  - Action frequency: Every 5-15 seconds (rapid upgrade cycle)

  Assessment: WITHIN TARGET during high-income periods
```

**Implications for Simulator:**

To achieve 5-10 second action frequency, the simulator should test configurations where:

1. **Early Game Income Acceleration:**
   - Starting money: $100-200 (instead of $50)
   - First recipe base value: $5-10 (instead of $2)
   - First bee tier cost: $15-20 (instead of $25)
   - Result: First action at 5-10 seconds, second action at 15-20 seconds

2. **Mid Game Upgrade Clustering:**
   - Recipe tiers cost less ($50/$150/$400 instead of $100/$300/$800)
   - More frequent small upgrades instead of rare large ones
   - Result: Constant progression, always saving for "next thing"

3. **Late Game Money Sinks:**
   - Expensive end-game upgrades ($50,000+) to drain accumulated wealth
   - Prevent "nothing left to buy" scenario before Week 24
   - Result: Maintain tension throughout campaign

**Formula for Target Pacing:**
```
Target Action Frequency = 7.5 seconds average
Campaign Duration = 24 minutes (1,440 seconds)
Total Actions Per Campaign = 1,440 / 7.5 = 192 actions

Current upgrade counts:
  - Bee Fleet Tiers: 10 actions
  - Patch Unlocks: 6 actions
  - Capacity Upgrades: 6 patches × 5 tiers = 30 actions
  - Recipe Tier Upgrades: 8 recipes × 5 tiers = 40 actions
  - Recipe Unlocks: ~8 actions
  - Bee Allocations: ~50-100 reallocation decisions
  TOTAL: ~104-154 actions

Gap Analysis: Need 40-90 additional actions OR faster income to compress timeline
```

**Recommended Adjustments for Simulator Testing:**
1. Increase starting income generation (2-3× base recipe values)
2. Add more frequent small upgrades (micro-tiers between current tiers)
3. Reduce early-game upgrade costs (50-75% of current)
4. Add dynamic pacing (early game boosted income to accelerate start)

---

## CONCLUSION

### Economy Simulator Requirements Summary

Your economy simulator needs to capture these **core mechanics**:

1. **Global bee pool** as the primary strategic constraint
   - Single shared pool competing across all patches
   - Manual allocation creates meaningful player decisions
   - Scarcity forces trade-offs throughout campaign

2. **Distance-based transport timing** (not instant delivery)
   - 6 patches at 7-44 unit distances (6.3× throughput difference)
   - Bezier arc calculations with seasonal speed modifiers
   - Close patches inherently more efficient than distant ones

3. **Priority-based recipe consumption** (not parallel resource checking)
   - List-order execution creates strategic recipe ordering
   - Immediate ingredient consumption when available
   - Multiple recipes can produce simultaneously

4. **Storage overflow waste** (fixed capacity, no backflow)
   - 100-130 pollen per type (seasonal modifiers)
   - Overflow discarded (waste mechanic)
   - Creates pressure to consume resources efficiently

5. **Seasonal modifier transitions** (abrupt changes at Week 9 and 17)
   - 3-season campaign (Spring/Summer/Autumn)
   - Fixed 24-week time limit creates urgency
   - Summer income spike (1.5×) is critical earning window

6. **Exponential cost scaling** vs **exponential income growth**
   - Bee fleet costs: $25 → $38,000 (10 tiers)
   - Recipe tier bonuses: up to 9.375× income rate at Tier 5
   - "Perfect build" costs ~$159,785 (impossible within campaign)

7. **Four interconnected upgrade systems**
   - Bee Fleet Purchases (expand global pool)
   - Patch Capacity Upgrades (remove per-patch bottlenecks)
   - Recipe Tier Upgrades (exponential income scaling)
   - Patch Unlocks (access new pollen types and recipes)

8. **Multiple viable playstyles**
   - Bee Rush, Patch Expansion, Recipe Tier Rush
   - Balanced/Adaptive, Efficiency Focus, Summer Maximizer
   - No single dominant strategy (all paths have trade-offs)

9. **All parameters are configurable**
   - Transport (speed, distance, gathering time)
   - Economics (costs, income, starting money)
   - Seasonal modifiers (all four multipliers)
   - Capacities (patch limits, storage, tier bonuses)
   - Campaign (duration, week length, season count)

10. **Player engagement pacing goal: 5-10 second action frequency**
    - Current config: ~104-154 total actions over 24 minutes
    - Target: ~192 actions for ideal pacing
    - Simulator should test income acceleration to meet target

### Critical Success Criteria for Simulator

The economy simulator must be able to:

1. **Run multiple playstyles** and compare total earnings
2. **Identify dominant strategies** (if any exist - should avoid this)
3. **Test parameter configurations** to optimize player pacing
4. **Validate seasonal balance** (all seasons matter, not just Summer)
5. **Measure action frequency** (time between player decisions)
6. **Track bottleneck progression** (bee pool → capacity → storage → optimization)
7. **Calculate ROI for all upgrade paths** across different strategies
8. **Simulate 24-week campaigns** with realistic decision-making

### What Makes This Game Unique

- **Global bee pool** (not per-patch ownership) creates strategic scarcity
- **Distance-based efficiency tiers** (6.3× throughput difference) reward smart allocation
- **Campaign time limit** (24 weeks) prevents infinite grinding, encourages replayability
- **Seasonal adaptation** (3 different modifier sets) prevents "solved" builds
- **Four upgrade paths** with clear synergies and anti-synergies
- **Tunable parameters** allow rapid iteration and balance testing

This summary provides all parameters, formulas, constraints, and strategic depth analysis needed to build a high-fidelity economy simulator that accurately reflects your game's design and identifies optimal balance points.

---

**Next Steps:**
Once you review this summary, we can proceed to Step 2: Designing and implementing the new economy simulator based on these mechanics and strategic archetypes.