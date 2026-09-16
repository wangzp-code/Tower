# Scene Setup Guide

## Step 1: Create Bootstrap Scene

1. In Unity, go to `File > New Scene`
2. Select `Basic (Built-in)`
3. Save as `Assets/Scenes/BootstrapScene.unity`

4. In the Hierarchy, delete `Main Camera` and `Directional Light`

5. Create a new empty GameObject named `Bootstrap`
6. Add the `Bootstrap` script to it

7. Save the scene

## Step 2: Create MainMenu Scene (Optional)

1. Create new scene, save as `Assets/Scenes/MainMenu.unity`
2. Add Canvas and basic UI
3. Add `MainMenuUI` script to Canvas

## Step 3: Create Game Scene

1. Create new scene, save as `Assets/Scenes/Game.unity`
2. Add Canvas
3. Add basic UI elements
4. Add `UIManager` script to Canvas
5. Add `GameTestManager` script to an empty GameObject

## Step 4: Add to Build Settings

1. Go to `File > Build Settings`
2. Add `BootstrapScene.unity` (index 0)
3. Add other scenes as needed

## Step 5: Configure ScriptableObjects

### Create GameConfig
1. Right-click in Project window > `Create > ParasiteTower > Config > GameConfig`
2. Name it `GameConfig`
3. Fill in the fields

### Create Test Monster Data
1. Right-click in Project window > `Create > ParasiteTower > MonsterData`
2. Create a few test monsters

### Create Test Class Data
1. Right-click in Project window > `Create > ParasiteTower > Class > ClassData`
2. Create a few test classes

## Step 6: Test the Game

1. Open `BootstrapScene.unity`
2. Enter Play mode
3. Check Console for initialization messages

## Expected Console Output
```
=== Starting Bootstrap ===
Created GameManager
Created EventBus
Created ConfigManager
Created SaveSystem
Created SceneLoader
Created TraitManager
Created CurseManager
Created AchievementManager
Created TutorialManager
Created NegotiateManager
Created ShopManager
Created RewardManager
Created FloorManager
Created PollutionSystem
Created FormManager
Created FragmentManager
Created ClassManager
Created UIManager
Created AudioManager
Created VFXManager
=== All Systems Initialized ===
```

## Troubleshooting

### If there are missing references
- Make sure all ScriptableObjects are created and configured
- Check that all scripts are compiled without errors

### If systems aren't initialized
- Verify Bootstrap script is on the scene
- Check Console for error messages
