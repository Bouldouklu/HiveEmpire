# Airport Upgrade System - Unity Editor Setup Guide

## Overview
This guide walks through setting up the UI and components for the airport upgrade system in Unity Editor.

## System Components Created
- `AirportController.cs` - Manages upgrade state and logic
- `RouteController.cs` - Dynamically spawns airplanes based on tier
- `AirportClickHandler.cs` - Detects clicks on airports
- `AirportUpgradePanel.cs` - UI controller for upgrade interface

---

## Part 1: Create Upgrade Panel UI

### Step 1: Create Panel Container
1. Open `GameScene.unity`
2. In Hierarchy, find or create a Canvas (if none exists: `GameObject > UI > Canvas`)
3. Right-click Canvas → `UI > Panel`
4. Rename it to `AirportUpgradePanel`
5. Set RectTransform:
   - Anchor: Center-Middle
   - Width: 400
   - Height: 300
   - Pos X: 0, Pos Y: 0

### Step 2: Add Panel Background
1. Select `AirportUpgradePanel`
2. In Inspector, adjust Image component:
   - Color: Semi-transparent dark (R:0, G:0, B:0, A:200)

### Step 3: Create Title Text
1. Right-click `AirportUpgradePanel` → `UI > Text - TextMeshPro`
2. Rename to `AirportNameText`
3. Set RectTransform:
   - Anchor: Top-Center
   - Width: 360, Height: 50
   - Pos X: 0, Pos Y: -30
4. TextMeshPro Settings:
   - Text: "Forest Airport"
   - Font Size: 32
   - Alignment: Center
   - Color: White

### Step 4: Create Current Tier Text
1. Right-click `AirportUpgradePanel` → `UI > Text - TextMeshPro`
2. Rename to `CurrentTierText`
3. Set RectTransform:
   - Anchor: Top-Center
   - Width: 360, Height: 40
   - Pos X: 0, Pos Y: -80
4. TextMeshPro Settings:
   - Text: "Tier: Base"
   - Font Size: 24
   - Alignment: Center
   - Color: Light Gray

### Step 5: Create Current Airplanes Text
1. Right-click `AirportUpgradePanel` → `UI > Text - TextMeshPro`
2. Rename to `CurrentAirplanesText`
3. Set RectTransform:
   - Anchor: Top-Left
   - Width: 340, Height: 30
   - Pos X: 30, Pos Y: -130
4. TextMeshPro Settings:
   - Text: "Current: 3 planes"
   - Font Size: 20
   - Alignment: Left
   - Color: White

### Step 6: Create Next Tier Airplanes Text
1. Right-click `AirportUpgradePanel` → `UI > Text - TextMeshPro`
2. Rename to `NextTierAirplanesText`
3. Set RectTransform:
   - Anchor: Top-Left
   - Width: 340, Height: 30
   - Pos X: 30, Pos Y: -165
4. TextMeshPro Settings:
   - Text: "Next: 4 planes (+1)"
   - Font Size: 20
   - Alignment: Left
   - Color: Green

### Step 7: Create Upgrade Cost Text
1. Right-click `AirportUpgradePanel` → `UI > Text - TextMeshPro`
2. Rename to `UpgradeCostText`
3. Set RectTransform:
   - Anchor: Bottom-Center
   - Width: 360, Height: 35
   - Pos X: 0, Pos Y: 70
4. TextMeshPro Settings:
   - Text: "Cost: $50"
   - Font Size: 24
   - Alignment: Center
   - Color: Yellow

### Step 8: Create Upgrade Button
1. Right-click `AirportUpgradePanel` → `UI > Button - TextMeshPro`
2. Rename to `UpgradeButton`
3. Set RectTransform:
   - Anchor: Bottom-Center
   - Width: 200, Height: 50
   - Pos X: 0, Pos Y: 30
4. Button Settings:
   - Normal Color: Green
   - Highlighted Color: Light Green
   - Pressed Color: Dark Green
   - Disabled Color: Gray
5. Select child `Text (TMP)`:
   - Rename to `UpgradeButtonText`
   - Text: "Upgrade"
   - Font Size: 24
   - Color: White

### Step 9: Create Close Button
1. Right-click `AirportUpgradePanel` → `UI > Button - TextMeshPro`
2. Rename to `CloseButton`
3. Set RectTransform:
   - Anchor: Top-Right
   - Width: 40, Height: 40
   - Pos X: -10, Pos Y: -10
4. Select child `Text (TMP)`:
   - Text: "X"
   - Font Size: 24
   - Alignment: Center
   - Color: White

### Step 10: Attach AirportUpgradePanel Script
1. Select `AirportUpgradePanel` GameObject
2. In Inspector, click `Add Component`
3. Search for `AirportUpgradePanel` script and add it
4. Drag UI elements to script fields:
   - **Panel Root:** `AirportUpgradePanel` GameObject itself
   - **Airport Name Text:** `AirportNameText`
   - **Current Tier Text:** `CurrentTierText`
   - **Current Airplanes Text:** `CurrentAirplanesText`
   - **Next Tier Airplanes Text:** `NextTierAirplanesText`
   - **Upgrade Cost Text:** `UpgradeCostText`
   - **Upgrade Button:** `UpgradeButton`
   - **Close Button:** `CloseButton`
   - **Upgrade Button Text:** `UpgradeButton > Text (TMP)`
5. Set Colors:
   - **Affordable Color:** Green (0, 255, 0)
   - **Unaffordable Color:** Red (255, 0, 0)
   - **Max Tier Color:** Gray (128, 128, 128)

---

## Part 2: Setup Existing Airports

### For Each Airport in Scene:

#### Step 1: Add AirportClickHandler Component
1. Select airport GameObject (e.g., `Forest Airport`)
2. `Add Component` → `AirportClickHandler`
3. Script will auto-find AirportController and AirportUpgradePanel

#### Step 2: Ensure Collider Exists
1. If airport doesn't have a collider:
   - `Add Component` → `Box Collider`
   - Adjust size to cover the airport model
   - This is needed for mouse click detection

#### Step 3: (Optional) Add Hover Material
1. Create a hover material (or duplicate airport's material)
2. Make it brighter/emissive for visual feedback
3. Drag to `AirportClickHandler > Hover Material` field

#### Step 4: Verify AirportController Settings
1. Select airport GameObject
2. Find `AirportController` component
3. Verify upgrade settings (or leave defaults):
   - **Current Tier:** 0
   - **Airplanes Per Tier:** [3, 4, 5, 7]
   - **Upgrade Costs:** [50, 150, 400]

#### Step 5: Verify RouteController Updates
1. Select airport GameObject
2. Find `RouteController` component
3. Verify:
   - **Airport Controller:** Should auto-populate with AirportController on same GameObject
   - **Airplane Prefab:** Should already be set
   - Remove/ignore old `Max Airplanes On Route` field (no longer used)

---

## Part 3: Setup New Airports (Future Placements)

### Update Airport Prefab
1. Navigate to `Assets/Prefabs/`
2. Find airport prefab (or the placeholder system)
3. Add `AirportClickHandler` component to prefab
4. Add Collider if not present
5. Configure as described in Part 2

### Update AirportPlaceholder.cs (if needed)
The existing `AirportPlaceholder.cs` script should work without changes. When spawning airports, ensure:
- AirportController is added
- RouteController is added
- AirportClickHandler is added (or add via prefab)

---

## Part 4: Testing

### Test Checklist:
1. ✅ Enter Play Mode
2. ✅ Click on an airport
3. ✅ Upgrade panel appears with correct information
4. ✅ Verify displayed airplane counts (3 → 4 → 5 → 7)
5. ✅ Verify upgrade costs ($50 → $150 → $400)
6. ✅ Test with insufficient money (button should be disabled)
7. ✅ Earn money and click Upgrade button
8. ✅ Verify:
   - Money is deducted
   - Tier updates in panel
   - New airplanes spawn on route
   - Panel shows next tier info (or "MAX TIER" at tier 3)
9. ✅ Upgrade to max tier and verify "Max Tier" button appears
10. ✅ Test with multiple airports (each upgrades independently)
11. ✅ Click Close button (panel should hide)

### Expected Behavior:
- **Base (Tier 0):** 3 airplanes, can upgrade for $50
- **Tier 1:** 4 airplanes, can upgrade for $150
- **Tier 2:** 5 airplanes, can upgrade for $400
- **Tier 3:** 7 airplanes, MAX TIER (no more upgrades)

### Debug Console:
Watch for these log messages:
- `Airport [name] upgraded to Tier X. New airplane count: Y`
- `RouteController on [name]: Airport upgraded to tier X`
- `Airport [name] clicked - opening upgrade panel`

---

## Part 5: Visual Polish (Optional)

### Add Particle Effects on Upgrade:
1. Create particle system prefab
2. In `AirportController`, add public ParticleSystem field
3. In `UpgradeAirport()` method, instantiate/play particles

### Tier Visual Indicators:
1. Change airport material color based on tier
2. Add emission glow that increases with tier
3. Scale airport slightly with each tier

### UI Animations:
1. Add UI animations to panel (fade in/out)
2. Animate button press feedback
3. Add sound effects for upgrades

---

## Troubleshooting

### Panel doesn't appear when clicking airport:
- Check airport has Collider component
- Verify `AirportClickHandler` is attached
- Check Console for error messages
- Ensure `AirportUpgradePanel` GameObject exists in scene
- Verify camera has Physics Raycaster (usually auto-added)

### Button is always disabled:
- Check `EconomyManager.Instance` exists in scene
- Verify player has enough money
- Check Console for errors in `AirportController.UpgradeAirport()`

### New airplanes don't spawn after upgrade:
- Check `RouteController` is subscribed to `OnAirportUpgraded` event
- Verify `RouteController.OnAirportUpgraded()` is being called (check Console logs)
- Ensure airplane prefab is assigned to RouteController

### "Already at max tier" message:
- Airport is at tier 3 (maximum)
- This is expected behavior - working correctly!

---

## Configuration Options

### Adjust Airplane Scaling:
Edit `AirportController` Inspector:
- **Airplanes Per Tier:** Change [3, 4, 5, 7] to your preferred values
- Example for aggressive scaling: [3, 6, 12, 20]
- Example for conservative: [3, 4, 5, 6]

### Adjust Upgrade Costs:
Edit `AirportController` Inspector:
- **Upgrade Costs:** Change [50, 150, 400] to balance economy
- Example for expensive: [100, 500, 2000]
- Example for cheap: [25, 75, 200]

### Adjust Colors:
Edit `AirportUpgradePanel` Inspector:
- **Affordable Color:** Green (default)
- **Unaffordable Color:** Red (default)
- **Max Tier Color:** Gray (default)

---

## Architecture Notes

### Event Flow:
```
Player Clicks Airport
    ↓
AirportClickHandler.OnMouseDown()
    ↓
AirportUpgradePanel.ShowPanel(airportController)
    ↓
Player Clicks Upgrade Button
    ↓
AirportController.UpgradeAirport()
    ↓
Deducts money, increments tier, fires OnAirportUpgraded event
    ↓
RouteController.OnAirportUpgraded()
    ↓
Recalculates spawn interval, spawns additional airplanes
```

### Key Components:
- **AirportController:** Manages upgrade state (tier, costs, airplane counts)
- **RouteController:** Reads tier from AirportController, spawns airplanes dynamically
- **AirportClickHandler:** Detects clicks, opens UI
- **AirportUpgradePanel:** Displays info, handles upgrade button

---

## Next Steps

### Week 2 Features (Future):
- [ ] Connection slot limit system (2-5 slots, upgradable)
- [ ] Additional specialization paths (Producer, Processor, Storage)
- [ ] City upgrade system
- [ ] Visual bottleneck indicators

### Polish:
- [ ] Particle effects on upgrade
- [ ] Sound effects
- [ ] UI animations
- [ ] Tier-based visual changes to airports
- [ ] Tooltips explaining benefits

---

## Questions?

If you encounter issues:
1. Check Unity Console for error messages
2. Verify all components are attached
3. Ensure references are assigned in Inspector
4. Test with simple case (one airport, lots of money)

The system is designed to be modular and expandable. Enjoy building your cargo empire!
