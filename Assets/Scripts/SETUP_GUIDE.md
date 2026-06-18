# Penalty-3D — Phase 1 Scene Setup Guide

## Scene Hierarchy to Create

```
GameScene
├── GameManager          (empty GameObject)
├── Camera               (Main Camera)
├── Lighting             (Directional Light)
├── Pitch                (Plane, scale 20x1x30, tag: Ground)
├── Goal                 (empty GameObject, position: 0, 0, 18)
│   └── [GoalPostBuilder builds children at runtime]
├── Ball                 (football_lp.fbx, tag: Ball)
└── Goalkeeper           (Saha.fbx, position: 0, 1, 16.5)
```

---

## Step-by-Step Setup

### 1. Pitch
- Create > 3D Object > Plane
- Scale: (20, 1, 30), Position: (0, 0, 15)
- Tag it **Ground** (add the tag if it doesn't exist)
- Assign the Green.mat material

### 2. Goal
- Create an empty GameObject, name it **Goal**
- Position: **(0, 0, 18)** — this places it at the far end
- Add Component → **GoalPostBuilder**
- Leave post material blank for now (white default is fine)

### 3. Ball
- Drag `football/source/football_lp.fbx` into the scene
- Scale: (0.22, 0.22, 0.22)  — regulation ~22cm diameter
- Position: **(0, 0.11, 5)**  — penalty spot
- Tag it **Ball**
- Add Component → **Rigidbody** (already handled by BallController)
- Add Component → **Sphere Collider**, radius: 0.5 (in local space)
- Add Component → **BallController**

### 4. Goalkeeper
- Drag `Saha.fbx` into the scene
- Position: **(0, 0, 16.5)**, Rotation: **(0, 180, 0)** — facing the ball
- Add Component → **Animator**
  - We'll create the Animator Controller in Phase 3
  - For now leave Controller slot empty
- Add Component → **GoalkeeperController**

### 5. GameManager
- Create empty GameObject named **GameManager**
- Add Component → **GameManager**
  - Ball → drag the Ball GameObject
  - Goalkeeper → drag Goalkeeper GameObject
- Add Component → **BallLauncher**
  - Game Manager → drag GameManager
  - Ball → drag Ball
  - Goal Transform → drag Goal

### 6. Camera
- Position: **(0, 2, 0)**, Rotation: **(10, 0, 0)**
- This gives a low behind-the-ball perspective

---

## What Works After This Setup

| Action | Result |
|--------|--------|
| Move mouse left/right | Aim direction changes |
| Hold LMB + drag up | Charge kick power |
| Release LMB | Ball kicks toward goal |
| A / D keys while aiming | Add curl to shot |
| Ball enters goal trigger | "GOAL!" fires (wire to UI) |
| 2s after kick (no goal) | "SAVED!" fires |
| After 5 kicks | Game Over fires |

---

## Phase 2 — Next Steps
- [ ] Add UI Canvas (score display, result text, power bar)
- [ ] Aim arrow / trajectory line (LineRenderer already wired in BallLauncher)
- [ ] Phase 3: Goalkeeper Animator Controller with Dive animations
```
