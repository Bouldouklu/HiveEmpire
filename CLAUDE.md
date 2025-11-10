# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Hive Empire** - An incremental strategy resource management game where players build automated pollen networks connecting resource-producing flower patches to a central hive. The core gameplay focuses on discovering resource synergies, managing bottlenecks, and optimizing production chains for exponential growth.

- **Genre:** Incremental Strategy + Resource Management
- **Style:** Minimalist 3D, top-down orthographic view (Mini Metro aesthetic)
- **Target Platform:** WebGL (browser-based, itch.io)
- **Unity Version:** 6000.2.10f1 (Unity 6)
- **Render Pipeline:** URP (Universal Render Pipeline)

## Project Structure

### Core Scripts
- **`Assets/Scripts/BeeController.cs`** - Controls Bee movement and pollen delivery
- **`Assets/Scripts/FlowerPatchController.cs`** - Manages flower patch resource generation and upgrades

### Resources
- **`Assets/Resources/AirFlow_GameConcept.md`** - Complete game design document with mechanics, progression, and implementation roadmap
- **`Assets/Resources/AirFlow_ColorPalette.md`** - Comprehensive color palette and visual style guide

### Assets
- **`Assets/Material/`** - Material assets for biomes, buildings, and UI elements (color-coded by resource/biome type)
- **`Assets/Prefabs/`** - Prefabs for game objects (currently contains Bee prefab)
- **`Assets/Scenes/GameScene.unity`** - Main game scene
- **`Assets/FX/PolygonParticleFX/`** - Third-party particle effects asset pack

### Editor Plugins
- **AssetInventory** - Asset management tool
- **TableForge** - Data table editor
- **vFolders** - Visual folder organization
- **vHierarchy** - Enhanced hierarchy view

## Architecture & Game Systems

### Core Gameplay Loop
```
Flower Patch → Generates Resource (Wood/Stone/Oil/Food)
       ↓
Bee → Automatically delivers to Hive
     ↓
Hive → Combines resources into higher-value products
    ↓
Earn → Exponential money based on synergies
    ↓
Spend → New flower patchs, upgrades, unlock biomes
     ↓
Optimize → Identify and solve bottlenecks
        ↓
LOOP with increasing complexity
```

### Key Systems to Implement

1. **Resource Synergy System** - Exponential value when combining resources at the hive
   - Individual resources: $1 each
   - Two-resource combos: $8-12
   - Three-resource combos: $40-50
   - Four-resource combos: $200+

2. **Limited Connection Slots** - Strategic constraint forcing meaningful choices
   - Hive can only connect to limited flower patchs (2-5 slots, upgradable)
   - Players must choose which resource combinations to prioritize

3. **Biome & Resource Types**
   - Forest → Wood (fast: 1 per 2s)
   - Plains → Food (fast: 1 per 2s)
   - Mountain → Stone (medium: 1 per 4s)
   - Desert → Oil (slow: 1 per 6s)
   - Coastal → Fish (medium: 1 per 3s)
   - Tundra → Minerals (slow: 1 per 5s)

4. **Flower Patch Specialization Paths**
   - Producer: Increased generation speed
   - Speed Hub: Faster Bee delivery
   - Processor: Pre-combine resources at source
   - Storage: Buffer resources, passive income

5. **Automated Bee Transport**
   - Fully automated pathfinding (no manual routing)
   - Visual feedback with colored pollen cubes and trails
   - Object pooling for performance (target: 200+ Bees at 50+ FPS)

## Development Workflow

### Unity Editor Testing
Open the project in Unity 6 Editor and use Play Mode for testing:
```
1. Open Assets/Scenes/GameScene.unity
2. Press Play button in Unity Editor
3. Test gameplay mechanics in Game view
```

### Building for WebGL
```
File → Build Settings → WebGL → Build
```
- Ensure URP settings are optimized for WebGL
- Target 60 FPS with 100+ Bees, 50+ FPS with 200 Bees
- Use object pooling if performance issues arise

### Version Control Notes
- `.meta` files are tracked - do not exclude them
- Unity Library folder is gitignored
- Scene changes should be committed carefully to avoid merge conflicts

## Design Constraints & Philosophy

### Critical Design Pillars
1. **Satisfying Automation** - Players watch their network operate without micromanagement
2. **Exponential Growth** - Income scales dramatically through smart resource combinations
3. **Meaningful Choices** - Limited slots and multiple upgrade paths with no "correct" answer
4. **Visible Optimization** - Bottlenecks are clear, solutions are multiple, players feel smart
5. **Rapid Prototyping** - Scoped for 3-4 week validation, expandable if successful

### Visual Style Requirements
- **Aesthetic:** Minimalist, infographic-inspired (reference: Mini Metro, Dorfromantik)
- **Color Palette:** Desaturated, harmonious, sophisticated (see `AirFlow_ColorPalette.md`)
- **Camera:** Orthographic, 45° top-down angle
- **World:** 40x40 unit grid with Hive at center (0,0,0)
- **Materials:** Flat shading with subtle emission glow, no metallic surfaces

### Key Design Decisions
- **NOT about flight path routing** - Focus is on optimization and strategy, not logistics
- **Fully automated Bees** - No manual route drawing, Bees automatically pathfind to hive
- **Connection slot limits create strategy** - Without limits, there's no meaningful choice
- **Exponential combo values** - Core to incremental game satisfaction (10x, 50x, 200x jumps)

## Implementation Roadmap

### Week 1: Core Loop (Validate Fun)
- Top-down camera and basic scene setup
- Hive building (central hub, receives pollen)
- Two flower patch types: Forest (wood), Plains (food)
- Basic resource generation system
- Bee spawning, pathfinding, and delivery
- **THE HOOK:** Resource synergy combos (Wood + Stone → Buildings = $10)

### Week 2: Strategic Depth
- Connection slot limit system (2 slots, upgradable)
- Additional biomes (Desert with oil)
- Triple-resource combos
- Flower Patch upgrade system (specialization paths)
- Money counter with $/sec rate
- Visual bottleneck indicators

### Week 3: Polish & Publish
- All 4 specialization paths
- Hive upgrade system
- 6 total biome types
- Four-resource combo system
- UI polish (tooltips, upgrade trees, stats)
- Visual polish (particles, animations, juice)
- WebGL build optimization
- Itch.io publication

## Code Style & Best Practices

### Unity 6 Specific
- Use `FindFirstObjectByType<T>()` instead of deprecated `FindObjectOfType<T>()`
- Use `FindAnyObjectByType<T>()` when finding any instance is acceptable
- Prefer event-driven architecture over Update() loops where possible
- Link references in Unity Editor Inspector rather than using GetComponent() at runtime

### Component Architecture
- Design as small, focused components that compose together
- Use Awake() for local initialization
- Use Start() to access external references
- Minimize runtime component searching - prefer serialized field references

### Event Communication
- Prioritize UnityActions for simple script-to-script communication
- Use observer pattern to decouple code
- Consider ScriptableObjects for data-driven design

### Performance Considerations
- Implement object pooling for Bees (frequently instantiated)
- Target 60 FPS with 100+ Bees
- Use profiler-driven optimization decisions
- Be conscious of GC allocations in WebGL builds

### Code Quality
- Never use hardcoded strings - use constants
- Keep it simple (KISS principle) - favor simple solutions
- XML documentation for public APIs
- Explain "why" in comments, not "what" (code should be self-documenting)

## Common Tasks

### Adding a New Biome
1. Create material in `Assets/Material/` using color palette from `AirFlow_ColorPalette.md`
2. Add resource type to enum/constants
3. Implement generation rate in flower patch controller
4. Add synergy combinations to hive processing logic
5. Create unlock condition and cost

### Adding Flower Patch Upgrade Path
1. Define specialization type (Producer/Speed/Processor/Storage)
2. Implement tier system (Tier 1, 2, 3 with increasing costs)
3. Create visual distinction for specialized flower patchs
4. Add upgrade UI panel with tooltips
5. Test trade-offs vs other specialization paths

### Optimizing Bee Performance
1. Check current Bee count and FPS in profiler
2. Implement/tune object pooling system
3. Consider LOD for distant Bees
4. Simplify trail renderers if needed
5. Batch material instances

## Testing & Validation

### Prototype Success Criteria
- **Week 1:** Connecting multiple biomes feels satisfying, combos create visible value, players can identify bottlenecks
- **Week 2:** Limited slots create meaningful choices, upgrade decisions feel impactful, multiple valid strategies emerge
- **Week 3:** Feels like complete game, WebGL runs smoothly, generates player interest

### Playtesting Focus
- Session length (target: 15+ minutes engaged)
- "Just one more upgrade" feeling
- Players can articulate their strategy
- Income progression feels rewarding (exponential growth visible)
- No obvious "correct" answer for upgrades

## Important Files Reference

- **Game Design:** `Assets/Resources/AirFlow_GameConcept.md` (complete mechanics and progression)
- **Visual Guide:** `Assets/Resources/AirFlow_ColorPalette.md` (exact hex colors and material settings)
- **Main Scene:** `Assets/Scenes/GameScene.unity`
- **Core Controllers:** `Assets/Scripts/BeeController.cs`, `Assets/Scripts/FlowerPatchController.cs`

## Development Philosophy

This is a **rapid prototype** project focused on validating core mechanics quickly. Prioritize:
1. Testing if resource synergy + bottleneck optimization is fun
2. Simple, working implementations over perfect architecture
3. Playtesting early and iterating based on feedback
4. Keeping scope tight for 3-4 week timeline
5. Making "go/no-go" decision after Week 1 validation

If Week 1 doesn't feel good, pivot quickly. If successful, expand to Steam with meta-content (prestige system, challenge modes, multiple maps).
- don't embed 2 classes in one single file