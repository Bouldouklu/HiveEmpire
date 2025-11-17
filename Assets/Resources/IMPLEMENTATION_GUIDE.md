# Hive Empire - Balance Update Implementation Guide

## Overview

The game has been updated with **balanced economic parameters** designed for a 24-minute campaign with stepwise income growth, wave-pattern decision density, and 60-80% completion targets.

**What Changed:**
- ✅ Recipe upgrades: **3 → 5 tiers**
- ✅ Capacity upgrades: **3 → 5 tiers**
- ✅ Bee purchases: **4 → 10 tiers**
- ✅ Starting money: **$0 → $50**
- ✅ Created 7 new recipe templates (total: 12 recipes)
- ✅ Added ProgressionTracker for balance validation
- ✅ Created EconomySimulator for testing parameters

---

## 🚨 FIXING CONSOLE ERRORS

### Step 1: Update All Existing Assets

The console errors are caused by existing ScriptableObject assets having old array sizes (3 tiers instead of 5, etc.).

**To fix:**

1. **Open Unity Editor**
2. **Navigate to:** `Game → Fix All Assets - Update to 5-Tier System` (top menu)
3. **Click "Update All"** in the dialog
4. **Wait for completion message**

This will automatically update:
- All FlowerPatchData assets → 5 capacity tiers
- All HoneyRecipe assets → 5 upgrade tiers
- All BeeFleetUpgradeData assets → 10 bee purchase tiers

**Expected output:**
```
✓ Updated: WildMeadowPatchData (WildMeadow)
✓ Updated: OrchardPatchData (Orchard)
✓ Updated: Forest Honey
✓ Updated: Wildflower Honey
...
Successfully updated X assets total
```

---

### Step 2: Generate 7 New Recipe Assets

To expand from 5 to 12 total recipes:

1. **Navigate to:** `Game → Generate New Recipes` (top menu)
2. **Click "Generate"** in the dialog
3. **Verify creation** of 7 new recipes:
   - OrchardBlossom.asset
   - FieldGold.asset
   - SpringBlend.asset
   - SummerBlend.asset
   - AutumnBlend.asset
   - SeasonalHarmony.asset
   - PremiumReserve.asset

**Location:** `Assets/Resources/Recipes/`

---

### Step 3: Add ProgressionTracker to GameScene

The ProgressionTracker monitors decision density and completion percentage:

1. **Open GameScene** (`Assets/Scenes/GameScene.unity`)
2. **Create new GameObject:** Right-click Hierarchy → Create Empty
3. **Rename to:** "ProgressionTracker"
4. **Add Component:** `ProgressionTracker.cs`
5. **Position in Managers section** (alongside GameManager, EconomyManager, etc.)

**Integration (Optional - Advanced):**
- Call `ProgressionTracker.Instance.LogPatchUnlock(cost)` when flower patches are unlocked
- Call `ProgressionTracker.Instance.LogBeePurchase(cost, beesAdded)` when bees are purchased
- Call `ProgressionTracker.Instance.OnWeekAdvanced()` when SeasonManager advances weeks

---

### Step 4: Verify Updates

After running the asset updater:

1. **Check Console** - Should be clear of errors
2. **Inspect a FlowerPatchData asset:**
   - `Assets/Resources/FlowerPatchData/WildMeadowPatchData.asset`
   - Verify **Capacity Upgrade Costs** array has **5 elements**: `[50, 150, 400, 900, 2000]`
   - Verify **Max Capacity Tier** = `5`
   - Verify **Bonus Capacity Per Upgrade** = `5`

3. **Inspect a HoneyRecipe asset:**
   - `Assets/Resources/Recipes/ForestHoney.asset`
   - Verify **Upgrade Costs** array has **5 elements**: `[100, 300, 800, 2000, 5000]`
   - Verify **Ingredient Reduction Percent** array has **6 elements**: `[0, 10, 20, 30, 40, 50]`
   - Verify **Production Time Reduction Percent** array has **6 elements**: `[0, 15, 25, 35, 50, 60]`
   - Verify **Value Increase Percent** array has **6 elements**: `[0, 20, 40, 60, 100, 150]`

4. **Inspect BeeFleetUpgradeData asset:**
   - Search for "BeeFleetUpgradeData" in Project window
   - Verify **Bee Purchase Costs** array has **10 elements**: `[25, 60, 150, 350, 800, 1800, 4000, 8500, 18000, 38000]`
   - Verify **Bees Per Purchase** array has **10 elements**: `[2, 3, 5, 8, 12, 18, 25, 35, 50, 70]`
   - Verify **Max Purchase Tier** = `10`

---

## 📊 TESTING BALANCE

### Using the Economy Simulator

Test different parameter configurations:

1. **Open Simulator:** `Game → Economy Simulator` (top menu)
2. **Review default parameters** (already set to balanced values)
3. **Click "Run Simulation"**
4. **Review results:**
   - Total Decisions: Should be ~80-100 (60-80% completion)
   - Avg Decisions/Week: Should be ~4-6
   - Completion %: Should be 65-80%
   - Final Money: Should be $100,000-150,000

**Adjust parameters if needed:**
- Increase costs → fewer decisions (harder)
- Decrease costs → more decisions (easier)
- Change bee purchase tiers → affects late-game scaling

---

### Validating Progression

During gameplay testing:

1. **Play through a campaign** (24 weeks)
2. **After campaign ends:** Select ProgressionTracker GameObject
3. **Right-click component:** `Log Progression Summary` (context menu)
4. **Check Console output:**
   ```
   === PROGRESSION TRACKER SUMMARY ===
   Total Decisions: 85/118
   Completion: 72.0% (Target: 60-80%)
   Within Target: YES
   Avg Decisions/Week: 3.5
   ```

5. **Right-click component:** `Validate Balance` (context menu)
6. **Check validation results:**
   ```
   === BALANCE VALIDATION ===
   ✓ Completion: 72.0% [PASS]
   ✓ Avg Decisions/Week: 3.5 [PASS]
   ✓ Wave Pattern: [PASS]
   ✅ BALANCE VALIDATED
   ```

---

## 📝 NEW RECIPE INTEGRATION

### Adding Recipes to Production System

After generating new recipes, add them to RecipeProductionManager:

1. **Open GameScene**
2. **Select "RecipeProductionManager" GameObject**
3. **In Inspector → Recipe Production Manager component:**
   - Find **Active Recipes** list
   - **Expand from 5 to 12 slots**
4. **Drag new recipe assets** from `Assets/Resources/Recipes/`:
   - Slot 0: ForestHoney (existing)
   - Slot 1: WildflowerHoney (existing)
   - Slot 2: MountainHoney (existing)
   - Slot 3: DesertBlossom (existing)
   - Slot 4: PremiumBlend (existing)
   - **Slot 5: OrchardBlossom** (NEW)
   - **Slot 6: FieldGold** (NEW)
   - **Slot 7: SpringBlend** (NEW)
   - **Slot 8: SummerBlend** (NEW)
   - **Slot 9: AutumnBlend** (NEW)
   - **Slot 10: SeasonalHarmony** (NEW)
   - **Slot 11: PremiumReserve** (NEW)

**List order = Production priority** (higher priority recipes process first)

---

### Recipe Unlock Progression (Recommended)

Based on balanced parameters (`BalancedParameters.md`):

| Recipe | Unlock Cost | Timing | Purpose |
|--------|-------------|--------|---------|
| Forest Honey | $0 (default) | Week 1 | Tutorial recipe |
| Orchard Blossom | $25 | Week 2-3 | Early expansion |
| Wildflower Honey | $50 | Week 3-4 | Mid-early value |
| Mountain Honey | $100 | Week 5-6 | Before Summer |
| Desert Blossom | $150 | Week 7-9 | Dual-blend intro |
| Spring Blend | $150 | Week 8-10 | Alternative blend |
| Summer Blend | $300 | Week 11-13 | Summer optimization |
| Premium Blend | $100 | Week 12-14 | Existing multi-blend |
| Field Gold | $400 | Week 15-17 | Late single-resource |
| Autumn Blend | $600 | Week 17-19 | Autumn synergy |
| Seasonal Harmony | $800 | Week 19-21 | Complex multi-blend |
| Premium Reserve | $1,500 | Week 22-24 | Ultimate recipe |

---

## 🎯 PLAYTESTING CHECKLIST

### Week 1-3 (Spring Early) - Foundation
- [ ] Starting money: $50
- [ ] Can afford first bee tier ($25) + first patch ($1)
- [ ] First recipe (Forest Honey) generates income
- [ ] 3-4 decisions available (bee tier, patch unlock, recipe unlock)
- [ ] Income: ~$100-300/week

### Week 8-10 (Summer Start) - Expansion
- [ ] Player has 2-3 patches unlocked
- [ ] Income jumps significantly (+50% Summer modifier)
- [ ] Can afford mid-tier upgrades ($300-800 range)
- [ ] 4-6 decisions available
- [ ] Income: ~$2,000-4,000/week

### Week 17-20 (Autumn) - Optimization
- [ ] Player has 4-6 patches unlocked
- [ ] Premium recipes unlocked
- [ ] Recipe tier 3-4 upgrades available
- [ ] Fast production cycle creates urgency
- [ ] Income: ~$8,000-12,000/week

### Week 24 (End) - Completion
- [ ] Player achieved 60-80% completion
- [ ] Total decisions: 80-100
- [ ] Final money: $100,000-150,000
- [ ] Player feels satisfied (not rushed, not bored)
- [ ] End-of-year summary shows clear progression

---

## 🔧 TROUBLESHOOTING

### "Array index out of range" errors

**Cause:** Old ScriptableObject assets with 3-tier arrays
**Fix:** Run `Game → Fix All Assets - Update to 5-Tier System`

### "Recipe not producing" errors

**Cause:** Recipe upgrade arrays mismatch
**Fix:** Select recipe asset → Inspector → Check array lengths:
- Upgrade Costs: 5 elements
- Ingredient Reduction: 6 elements (tier 0-5)
- Production Time Reduction: 6 elements
- Value Increase: 6 elements

### "Max tier already reached" warnings

**Cause:** Code still references old max tier (3 instead of 5)
**Fix:** Check `HoneyRecipe.cs` line 174: `return currentTier >= 0 && currentTier < 5;`

### Economy feels too hard/easy

**Adjust via EconomySimulator:**
- **Too hard** (< 60% completion): Reduce costs by 10-20%
- **Too easy** (> 80% completion): Increase costs by 10-20%
- **Test again** and iterate

---

## 📚 REFERENCE DOCUMENTS

Detailed documentation available in `Assets/Resources/`:

- **BalancedParameters.md** - Complete parameter tables, decision timeline, income projections
- **ToDo.md** - Active development tasks and bug tracking
- **Readme.md** - Game design and mechanics overview
- **CLAUDE.md** - Architecture documentation for Claude Code

---

## 🎮 NEXT STEPS

1. ✅ **Fix console errors** (Run asset updater)
2. ✅ **Generate new recipes** (Run recipe generator)
3. ✅ **Add recipes to RecipeProductionManager** (Manual step)
4. ✅ **Add ProgressionTracker to scene** (Manual step)
5. ✅ **Playtest 24-week campaign** (Validate balance)
6. ✅ **Iterate on feel** (Adjust costs if needed)

**Questions or Issues?**
- Check console output for detailed logs
- Use ProgressionTracker's validation tools
- Refer to BalancedParameters.md for design rationale

**Happy Balancing! 🐝**
