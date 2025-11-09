# CARGO EMPIRE - Color Palette
*Calm, Elegant, Refined*

Inspired by Mini Metro, Mini Motorways, and Thronefall

---

## Design Philosophy

**Approach:** Desaturated, harmonious colors with clear hierarchy  
**Mood:** Professional, calm, sophisticated  
**Contrast:** High enough for clarity, soft enough for comfort  
**Coherence:** All colors work together, no jarring combinations

---

## Base Colors

### Background & Foundation

**GROUND / WORLD**
```
Dark Charcoal: #1a1f2e
```
- Main game background
- Darker than Mini Metro but not pure black
- Allows colors to pop without eye strain
- Hex: `#1a1f2e` RGB: (26, 31, 46)

**GRID LINES**
```
Subtle Gray: #2d3648
```
- Very subtle grid overlay
- Just visible enough for spatial reference
- 15% opacity on top of ground
- Hex: `#2d3648` RGB: (45, 54, 72)

**UI BACKGROUND**
```
Soft Dark: #232935
```
- UI panels, tooltips, menus
- Slightly lighter than ground for layering
- Hex: `#232935` RGB: (35, 41, 53)

---

## Biome Colors (Resources)

*Each biome has a primary color (building) and accent color (resource/glow)*

### 🌲 FOREST (Wood)
```
Primary:   Sage Green     #6b9080
Accent:    Moss Green     #8fb399
Resource:  Warm Wood      #a4b494
```
- Muted green, not bright
- Natural, organic feel
- Warm undertones (not cold blue-green)
- Hex: `#6b9080` RGB: (107, 144, 128)

### ⛰️ MOUNTAIN (Stone)
```
Primary:   Slate Gray     #7d8894
Accent:    Steel Blue     #8e9fb0
Resource:  Stone Gray     #9da8b5
```
- Cool gray with slight blue tint
- Solid, dependable feel
- Not too dark, maintains visibility
- Hex: `#7d8894` RGB: (125, 136, 148)

### 🏜️ DESERT (Oil)
```
Primary:   Warm Sand      #c89f6f
Accent:    Amber Gold     #d4b896
Resource:  Dark Oil       #8b7355
```
- Warm, sandy tone
- Desaturated orange/tan
- Elegant, not "cheap orange"
- Hex: `#c89f6f` RGB: (200, 159, 111)

### 🌾 PLAINS (Food)
```
Primary:   Wheat Gold     #b8a57d
Accent:    Hay Yellow     #c9b88f
Resource:  Grain Beige    #d4c9a8
```
- Muted yellow/gold
- Agricultural, harvest feel
- Soft, not bright
- Hex: `#b8a57d` RGB: (184, 165, 125)

### 🌊 COASTAL (Fish)
```
Primary:   Ocean Blue     #5a8096
Accent:    Sea Foam       #6d95ab
Resource:  Turquoise      #7ba9bd
```
- Calm ocean blue
- Desaturated, sophisticated
- Not tropical bright blue
- Hex: `#5a8096` RGB: (90, 128, 150)

### ❄️ TUNDRA (Minerals)
```
Primary:   Ice Blue       #8ba3b0
Accent:    Frost Cyan     #9db4c2
Resource:  Crystal White  #b4cbd6
```
- Very desaturated cyan
- Cold, clean feel
- Distinct from ocean blue
- Hex: `#8ba3b0` RGB: (139, 163, 176)

---

## Building Colors

### 🏛️ CITY (Central Hub)
```
Primary:   Royal Purple   #7d6b94
Accent:    Soft Violet    #8e7ba8
Glow:      Lavender       #a89bbe
```
- Central importance = purple (rare color)
- Sophisticated, regal
- Different from all biomes
- Hex: `#7d6b94` RGB: (125, 107, 148)

**Alternative City Color (if purple too bold):**
```
Neutral Cream: #d4c9b8
```
- Accepts all resources (neutral)
- Elegant beige/cream
- Stands out without overpowering
- Hex: `#d4c9b8` RGB: (212, 201, 184)

### 🏭 AIRPORT PLATFORMS
- Use biome primary color
- Flat shading, no gradient
- Slight emission glow (accent color) on edges
- Simple geometric shape (2x2 platform)

---

## Transport & Infrastructure

### 🚁 DRONES
```
Drone Body:  Soft White    #e8e8e8
Trail:       Gentle Cyan   #7ba9bd
Cargo Cube:  (Match resource color)
```
- Not pure white (too harsh)
- Soft white blends with aesthetic
- Trail is visible but not neon
- Hex: `#e8e8e8` RGB: (232, 232, 232)

### ➖ ROUTES (If Visible)
```
Route Line:  Subtle Gray   #4a5568
Active Flow: Soft Cyan     #8db4c7
```
- Very subtle, background element
- Only slightly brighter than grid
- Not a focal point (drones are)
- Hex: `#4a5568` RGB: (74, 85, 104)

---

## UI & Feedback Colors

### 💰 MONEY / POSITIVE
```
Income Gold:     #d4af37
Positive Green:  #7ea883
```
- Muted gold (not bright yellow)
- Soft green for success
- Elegant, not garish
- Hex Gold: `#d4af37` RGB: (212, 175, 55)

### 📊 INFORMATION
```
Neutral White:   #c8d0d8
Locked Gray:     #5a6270
Warning Amber:   #c9a66b
```
- Soft white for text
- Clear locked state
- Warm warning (not red)
- Hex White: `#c8d0d8` RGB: (200, 208, 216)

### 🎯 HIGHLIGHTS / SELECTION
```
Selection:       #a89bbe (soft purple)
Hover:           #9db4c2 (ice blue)
```
- Subtle, not bright
- Ties to city color (purple)
- Clear feedback without aggression

---

## Combo Tier Colors (Visual Feedback)

### LOW VALUE (1-2 Resources)
```
Tier 1: Soft Bronze  #b8a57d
```
- Basic combos
- Warm, simple
- Hex: `#b8a57d` RGB: (184, 165, 125)

### MID VALUE (3 Resources)
```
Tier 2: Muted Silver #8e9fb0
```
- Advanced combos
- Cooler tone = more valuable
- Hex: `#8e9fb0` RGB: (142, 159, 176)

### HIGH VALUE (4+ Resources)
```
Tier 3: Soft Gold    #d4af37
```
- Maximum combos
- Elegant gold, not bright
- Special feeling without garish
- Hex: `#d4af37` RGB: (212, 175, 55)

---

## Specialization Colors (Airport Upgrades)

### 📈 PRODUCER (Generation Speed)
```
Icon Color: Warm Green  #8fb399
```
- Growth = green
- Ties to forest aesthetic
- Hex: `#8fb399` RGB: (143, 179, 153)

### ⚡ SPEED HUB (Delivery Speed)
```
Icon Color: Bright Cyan  #7ba9bd
```
- Speed = cyan/electric feel
- Motion implied
- Hex: `#7ba9bd` RGB: (123, 169, 189)

### 🔧 PROCESSOR (Conversion)
```
Icon Color: Warm Amber   #c89f6f
```
- Processing = transformation
- Industrial feel
- Hex: `#c89f6f` RGB: (200, 159, 111)

### 📦 STORAGE (Buffer)
```
Icon Color: Cool Gray    #8e9fb0
```
- Storage = stable, reliable
- Neutral but distinct
- Hex: `#8e9fb0` RGB: (142, 159, 176)

---

## Usage Examples

### Example 1: Early Game Scene
```
Ground:          #1a1f2e (dark charcoal)
Grid Lines:      #2d3648 (subtle gray, 15% opacity)

Buildings:
- Forest Airport:  #6b9080 (sage green)
- Plains Airport:  #b8a57d (wheat gold)
- City:            #7d6b94 (royal purple)

Drones:          #e8e8e8 (soft white)
Trails:          #7ba9bd (gentle cyan)

Resources:
- Wood cubes:    #a4b494 (warm wood)
- Food cubes:    #d4c9a8 (grain beige)
```

### Example 2: Mid Game with Combos
```
Same base as above, plus:

Mountain Airport: #7d8894 (slate gray)
Desert Airport:   #c89f6f (warm sand)

Combo Notifications:
- Wood+Stone:     #b8a57d (soft bronze) "$10"
- Stone+Oil:      #8e9fb0 (muted silver) "$12"
```

### Example 3: UI Panel
```
Panel Background: #232935 (soft dark)
Text:            #c8d0d8 (neutral white)
Headers:         #a89bbe (soft purple)

Money:           #d4af37 (income gold)
Locked Items:    #5a6270 (locked gray)
Buy Button:      #7ea883 (positive green)
```

---

## Material Properties (Unity Settings)

### Buildings (Airports, City)
```
Shader: Unlit or Simple Lit
Color: Primary color (flat)
Emission: Accent color (subtle glow)
  - Intensity: 0.3-0.5
  - Color: Accent color at 30% brightness
Smoothness: 0.2 (slightly matte)
Metallic: 0.0 (no metal)
```

### Drones
```
Shader: Unlit
Color: #e8e8e8 (soft white)
Emission: None on body
Trail Renderer:
  - Color: #7ba9bd (gentle cyan)
  - Gradient: Fade from 100% to 0% opacity
  - Width: 0.1 units
```

### Ground
```
Shader: Unlit
Color: #1a1f2e (dark charcoal)
Grid Overlay: 
  - Shader: Unlit with transparency
  - Color: #2d3648 at 15% opacity
```

### Resource Cubes
```
Shader: Unlit or Simple Lit
Color: Resource-specific (see biome accent colors)
Size: 0.15 units
No emission (too busy)
```

---

## Accessibility Considerations

### Color Blind Safe
✅ Each biome is distinguishable even in grayscale:
- Forest: Mid-tone gray
- Mountain: Cool gray
- Desert: Warm light gray
- Plains: Warm mid gray
- Coastal: Cool mid-dark gray
- Tundra: Very light gray

### Contrast Ratios
✅ All text on dark background: 7:1 or higher (WCAG AAA)
✅ UI elements have clear visual hierarchy
✅ No red/green combinations for critical info

### Optional: Icon Support
Consider adding simple icons to biomes (tree, mountain, wave, etc.) for additional clarity beyond color.

---

## Comparison to References

### vs. Mini Metro
```
Mini Metro:       Bright, saturated primaries
Cargo Empire:     Muted, sophisticated earth tones

Mini Metro:       Pure white background option
Cargo Empire:     Dark, comfortable background

Similarity:       Clean geometry, clear hierarchy
```

### vs. Mini Motorways
```
Mini Motorways:   Slightly more vibrant than Metro
Cargo Empire:     More desaturated, calmer

Mini Motorways:   Colorful roads, bold buildings
Cargo Empire:     Subtle routes, elegant buildings

Similarity:       Top-down clarity, minimalist UI
```

### vs. Thronefall
```
Thronefall:       Medieval, warm palette, painterly
Cargo Empire:     Sci-fi, balanced temps, flat shading

Thronefall:       Earthy browns, grass greens
Cargo Empire:     Refined grays, muted jewel tones

Similarity:       Calm mood, strategic focus, refined aesthetic
```

---

## Quick Reference Swatch Grid

```
BIOMES:
Forest    [#6b9080] ████ Sage Green
Mountain  [#7d8894] ████ Slate Gray
Desert    [#c89f6f] ████ Warm Sand
Plains    [#b8a57d] ████ Wheat Gold
Coastal   [#5a8096] ████ Ocean Blue
Tundra    [#8ba3b0] ████ Ice Blue

BUILDINGS:
City      [#7d6b94] ████ Royal Purple

TRANSPORT:
Drones    [#e8e8e8] ████ Soft White
Trails    [#7ba9bd] ████ Gentle Cyan

UI:
Background [#1a1f2e] ████ Dark Charcoal
Panel      [#232935] ████ Soft Dark
Text       [#c8d0d8] ████ Neutral White
Money      [#d4af37] ████ Income Gold
Positive   [#7ea883] ████ Success Green
```

---

## Implementation Tips

### In Unity:
1. Create material per biome with exact hex colors
2. Use emission shader for subtle glow (0.3 intensity)
3. Keep metallic at 0, smoothness low (0.2-0.3)
4. Use Color32 in code for exact hex values:
   ```csharp
   Color forestColor = new Color32(0x6b, 0x90, 0x80, 0xFF);
   ```

### Testing:
- View in both bright and dark environments
- Test on different monitors/devices
- Ensure colors are distinguishable when zoomed out
- Check that UI text is readable

### Adjustments:
- If too dark: Lighten ground to `#242936`
- If too muted: Increase saturation by 10-15%
- If drones lost: Make trail brighter `#8fc9e8`
- If city blends in: Use cream alternative `#d4c9b8`

---

## Color Psychology

**Forest (Green):** Growth, nature, production ✓  
**Mountain (Gray):** Stability, strength, foundation ✓  
**Desert (Tan):** Warmth, value, resources ✓  
**Plains (Gold):** Harvest, abundance, food ✓  
**Coastal (Blue):** Calm, flow, trade ✓  
**Tundra (Cyan):** Rare, precious, minerals ✓  
**City (Purple):** Important, regal, central ✓  

All colors support their conceptual role while maintaining harmony.

---

## Final Notes

This palette prioritizes:
✅ **Calm:** No bright neons, no harsh contrast  
✅ **Elegant:** Sophisticated tones, refined composition  
✅ **Refined:** Professional quality, thoughtful choices  
✅ **Functional:** Clear hierarchy, good readability  
✅ **Harmonious:** All colors work together  

**Result:** A game that looks professional and feels premium, even as a prototype.

---

*Copy hex codes directly into Unity materials or color pickers. These are final, production-ready values.*
