# End-of-Year System Setup Guide

This guide explains how to set up the end-of-year summary system in Unity Editor.

## Overview

The end-of-year system consists of:
1. **YearStatsTracker** - Collects statistics throughout the year
2. **AchievementManager** - Defines and checks achievement milestones
3. **HighScoreManager** - Persists high scores across playthroughs
4. **EndOfYearPanel** - Displays the year summary UI

## Step 1: Create Manager GameObjects

### 1.1 Create UIManager GameObject (if not exists)

1. In the Hierarchy, create an empty GameObject at scene root
2. Name it: `UIManager`
3. This GameObject should be separate from the Canvas

### 1.2 Add Manager Components

Add these components to the UIManager GameObject:
- `YearStatsTracker` component
- `AchievementManager` component
- `HighScoreManager` component
- `EndOfYearPanel` component

**Note:** These are all singleton managers and should only exist once in the scene.

---

## Step 2: Create Achievement ScriptableObjects

### 2.1 Create Achievements Folder

1. In Project window, navigate to `Assets/ScriptableObjects/`
2. Create folder: `Achievements`

### 2.2 Create Achievement Assets

Right-click in the Achievements folder → Create → Hive Empire → Achievement

Create the following achievements (suggested milestones):

#### Money Achievements
- **Honey Starter**
  - Achievement ID: `honey_starter`
  - Name: Honey Starter
  - Description: Earn $100 in a single year
  - Type: TotalMoneyEarned
  - Threshold: 100

- **Sweet Success**
  - Achievement ID: `sweet_success`
  - Name: Sweet Success
  - Description: Earn $1,000 in a single year
  - Type: TotalMoneyEarned
  - Threshold: 1000

- **Honey Tycoon**
  - Achievement ID: `honey_tycoon`
  - Name: Honey Tycoon
  - Description: Earn $10,000 in a single year
  - Type: TotalMoneyEarned
  - Threshold: 10000

#### Production Achievements
- **First Batch**
  - Achievement ID: `first_batch`
  - Name: First Batch
  - Description: Complete 10 recipes
  - Type: TotalRecipes
  - Threshold: 10

- **Recipe Master**
  - Achievement ID: `recipe_master`
  - Name: Recipe Master
  - Description: Complete 50 recipes
  - Type: TotalRecipes
  - Threshold: 50

- **Production Line**
  - Achievement ID: `production_line`
  - Name: Production Line
  - Description: Complete 100 recipes
  - Type: TotalRecipes
  - Threshold: 100

#### Empire Achievements
- **Flower Power**
  - Achievement ID: `flower_power`
  - Name: Flower Power
  - Description: Place 5 flower patches
  - Type: FlowerPatches
  - Threshold: 5

- **Garden Empire**
  - Achievement ID: `garden_empire`
  - Name: Garden Empire
  - Description: Place 15 flower patches
  - Type: FlowerPatches
  - Threshold: 15

- **Bee Baron**
  - Achievement ID: `bee_baron`
  - Name: Bee Baron
  - Description: Reach 25 bees in your fleet
  - Type: PeakBeeFleet
  - Threshold: 25

- **Bee Overlord**
  - Achievement ID: `bee_overlord`
  - Name: Bee Overlord
  - Description: Reach 50 bees in your fleet
  - Type: PeakBeeFleet
  - Threshold: 50

#### Resource Achievements
- **Resource Gatherer**
  - Achievement ID: `resource_gatherer`
  - Name: Resource Gatherer
  - Description: Collect 100 total resources
  - Type: TotalResources
  - Threshold: 100

- **Resource Hoarder**
  - Achievement ID: `resource_hoarder`
  - Name: Resource Hoarder
  - Description: Collect 500 total resources
  - Type: TotalResources
  - Threshold: 500

### 2.3 Assign Achievements to Manager

1. Select UIManager GameObject in Hierarchy
2. Find `AchievementManager` component
3. Set `All Achievements` list size to match the number of achievements you created
4. Drag and drop each achievement ScriptableObject into the list

---

## Step 3: Create End-of-Year Panel UI

### 3.1 Create Panel Structure

In the Canvas hierarchy, create:

```
Canvas
├── EndOfYearPanelBlocker (Image)
│   └── EndOfYearPanelRoot (Image)
│       ├── HeaderSection (Vertical Layout Group)
│       │   ├── TitleText (TextMeshProUGUI) - "Year Complete!"
│       │   └── SubtitleText (TextMeshProUGUI) - "Year X"
│       │
│       ├── ContentScrollView (Scroll Rect)
│       │   └── ContentContainer (Vertical Layout Group)
│       │       ├── HeroStatsSection (Horizontal Layout Group)
│       │       │   ├── MoneyStatCard
│       │       │   │   ├── Label: "Money Earned"
│       │       │   │   └── ValueText (TMP) - "$X,XXX"
│       │       │   ├── RecipesStatCard
│       │       │   │   ├── Label: "Recipes Completed"
│       │       │   │   └── ValueText (TMP) - "XXX"
│       │       │   └── ResourcesStatCard
│       │       │       ├── Label: "Resources Collected"
│       │       │       └── ValueText (TMP) - "XXX"
│       │       │
│       │       ├── EconomicStatsSection (Vertical Layout Group)
│       │       │   ├── SectionTitle (TMP) - "Economic Stats"
│       │       │   ├── StartingMoneyText (TMP) - "Starting: $X"
│       │       │   ├── EndingMoneyText (TMP) - "Ending: $X"
│       │       │   └── HighestTransactionText (TMP) - "Highest: $X"
│       │       │
│       │       ├── EmpireStatsSection (Vertical Layout Group)
│       │       │   ├── SectionTitle (TMP) - "Empire Stats"
│       │       │   ├── FlowerPatchesText (TMP) - "Flower Patches: X"
│       │       │   └── PeakBeeFleetText (TMP) - "Peak Bee Fleet: X"
│       │       │
│       │       ├── SeasonBreakdownSection (Vertical Layout Group)
│       │       │   ├── SectionTitle (TMP) - "Season Breakdown"
│       │       │   ├── SpringStatsRow (Horizontal Layout Group)
│       │       │   │   ├── Label (TMP) - "Spring"
│       │       │   │   ├── SpringMoneyText (TMP) - "$X"
│       │       │   │   └── SpringRecipesText (TMP) - "X recipes"
│       │       │   ├── SummerStatsRow (Horizontal Layout Group)
│       │       │   │   ├── Label (TMP) - "Summer"
│       │       │   │   ├── SummerMoneyText (TMP) - "$X"
│       │       │   │   └── SummerRecipesText (TMP) - "X recipes"
│       │       │   └── AutumnStatsRow (Horizontal Layout Group)
│       │       │       ├── Label (TMP) - "Autumn"
│       │       │       ├── AutumnMoneyText (TMP) - "$X"
│       │       │       └── AutumnRecipesText (TMP) - "X recipes"
│       │       │
│       │       ├── AchievementsSection (Vertical Layout Group)
│       │       │   ├── SectionTitle (TMP) - "Achievements"
│       │       │   ├── AchievementCountText (TMP) - "X/12 Achievements"
│       │       │   └── AchievementsContainer (Vertical Layout Group)
│       │       │       └── (Achievement entries spawned here)
│       │       │
│       │       └── HighScoreSection (Vertical Layout Group)
│       │           ├── SectionTitle (TMP) - "Your Best"
│       │           ├── NewRecordsText (TMP) - "X NEW RECORDS!"
│       │           └── HighScoreSummaryText (TMP) - Multi-line stats
│       │
│       └── ButtonsSection (Horizontal Layout Group)
│           ├── PlayAgainButton (Button)
│           │   └── ButtonText (TMP) - "Play Again"
│           └── QuitButton (Button)
│               └── ButtonText (TMP) - "Quit"
```

### 3.2 Panel Blocker Setup

**EndOfYearPanelBlocker:**
- Component: Image
- Color: Black with 128 alpha (semi-transparent)
- Raycast Target: Enabled
- Stretch to fill entire canvas (Anchor: stretch all)

### 3.3 Panel Root Setup

**EndOfYearPanelRoot:**
- Component: Image
- Background color: Dark UI color
- Size: 800x600 (or adjust to preference)
- Anchor: Center
- Add: CanvasGroup component (for fade effects if desired)

### 3.4 Optional: Create Achievement Entry Prefab

Create a prefab for achievement display:

1. Create GameObject: `AchievementEntry`
2. Add Horizontal Layout Group
3. Add children:
   - AchievementName (TextMeshProUGUI) - Bold text
   - AchievementDescription (TextMeshProUGUI) - Smaller text
4. Save as prefab in `Assets/Prefabs/UI/`

---

## Step 4: Wire Up EndOfYearPanel Component

Select UIManager GameObject and find the EndOfYearPanel component:

### 4.1 Panel References
- **Panel Root**: Drag EndOfYearPanelRoot from Canvas
- **Panel Blocker**: Drag EndOfYearPanelBlocker from Canvas

### 4.2 Header Section
- **Title Text**: Drag TitleText (TMP)
- **Subtitle Text**: Drag SubtitleText (TMP)

### 4.3 Hero Stats Section
- **Total Money Earned Text**: Drag MoneyStatCard ValueText
- **Total Recipes Text**: Drag RecipesStatCard ValueText
- **Total Resources Text**: Drag ResourcesStatCard ValueText

### 4.4 Economic Stats Section
- **Starting Money Text**: Drag StartingMoneyText
- **Ending Money Text**: Drag EndingMoneyText
- **Highest Transaction Text**: Drag HighestTransactionText

### 4.5 Empire Stats Section
- **Flower Patches Text**: Drag FlowerPatchesText
- **Peak Bee Fleet Text**: Drag PeakBeeFleetText

### 4.6 Season Breakdown Section
- **Spring Money Text**: Drag SpringMoneyText
- **Spring Recipes Text**: Drag SpringRecipesText
- **Summer Money Text**: Drag SummerMoneyText
- **Summer Recipes Text**: Drag SummerRecipesText
- **Autumn Money Text**: Drag AutumnMoneyText
- **Autumn Recipes Text**: Drag AutumnRecipesText

### 4.7 Achievements Section
- **Achievements Container**: Drag AchievementsContainer Transform
- **Achievement Count Text**: Drag AchievementCountText
- **Achievement Entry Prefab**: Drag AchievementEntry prefab (if created)

### 4.8 High Score Section
- **High Score Summary Text**: Drag HighScoreSummaryText
- **New Records Text**: Drag NewRecordsText

### 4.9 Buttons
- **Play Again Button**: Drag PlayAgainButton
- **Quit Button**: Drag QuitButton

### 4.10 Colors
- **New Record Color**: Set to Yellow (#FFFF00) or desired highlight color
- **Below Record Color**: Set to Gray (#808080) or desired muted color

---

## Step 5: Testing

### 5.1 Quick Test with Debug Keys

The SeasonManager has debug shortcuts:
- Press **W** key to skip to next week
- Press **S** key to skip to next season
- Press **R** key to restart year

To quickly test the end-of-year panel:
1. Enter Play Mode
2. Press **S** three times to reach Winter (end of year)
3. The EndOfYearPanel should appear automatically

### 5.2 Test Reset Functionality

When the panel appears:
1. Click "Play Again" button
2. Verify that:
   - Panel closes
   - Money resets to $0
   - All flower patches are destroyed
   - Season restarts at Spring Week 1
   - Stats tracker resets

### 5.3 Test High Scores

1. Complete a full year
2. Note your stats
3. Click "Play Again"
4. Complete another year with different performance
5. Verify that high scores are preserved and "NEW RECORD" indicators appear

---

## Troubleshooting

### Panel Doesn't Appear

- Check that EndOfYearPanel component is on UIManager GameObject
- Verify that SeasonManager fires OnYearEnded event (check console for "[SeasonManager] Year has ended!" message)
- Check that panel references are wired correctly in inspector
- Ensure panelRoot and panelBlocker are initially inactive

### Stats Show Zero

- Verify YearStatsTracker is in scene and active
- Check console for "[YearStatsTracker] Stats initialized" message
- Ensure managers (EconomyManager, RecipeProductionManager, etc.) are present in scene
- Verify YearStatsTracker subscribes to events (check Start() is called)

### Achievements Don't Appear

- Check that Achievement ScriptableObjects are assigned to AchievementManager
- Verify thresholds are achievable (not too high)
- Check console for "[AchievementManager] Achievement Unlocked: X" messages
- Ensure Achievement Entry Prefab has correct child objects named "AchievementName" and "AchievementDescription"

### Reset Doesn't Work

- Check that GameManager.Instance exists
- Verify all reset methods are called (check console logs)
- Ensure flower patch GameObjects have FlowerPatchController component
- Check that Time.timeScale is reset to 1.0

---

## Customization

### Styling

The panel appearance can be customized:
- Adjust panel background color and opacity
- Change text fonts and sizes
- Add visual effects (animations, particles)
- Customize button styles

### Additional Stats

To add more statistics:
1. Track the stat in YearStatsTracker
2. Add text field to UI hierarchy
3. Wire text field to EndOfYearPanel component
4. Update DisplaySummary() method to populate the text

### More Achievements

Create additional achievement ScriptableObjects and add to AchievementManager list. Achievement types available:
- TotalMoneyEarned
- EndingMoney
- HighestTransaction
- TotalRecipes
- FlowerPatches
- PeakBeeFleet
- TotalResources

---

## Complete Setup Checklist

- [ ] UIManager GameObject created with all manager components
- [ ] Achievement ScriptableObjects created (suggested: 12 achievements)
- [ ] Achievements assigned to AchievementManager
- [ ] End-of-Year panel UI hierarchy created in Canvas
- [ ] Panel blocker and root configured
- [ ] All text fields created and labeled
- [ ] (Optional) Achievement entry prefab created
- [ ] EndOfYearPanel component references wired in inspector
- [ ] Tested year end with debug keys (S x3)
- [ ] Tested "Play Again" reset functionality
- [ ] Tested high score persistence across multiple runs

Once all items are checked, the end-of-year system is fully operational!
