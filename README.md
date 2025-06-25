# Galactic Escape (SEN3301 Project)

This project is a 3D space-themed game developed for the SEN3301 Computer Graphics and Animation. "Galactic Escape" challenges players to navigate a procedurally generated spaceship through a hazardous asteroid field, collect fuel cells to survive, and manage their ship's energy and health while aiming for a high score within a time limit. The game heavily utilizes procedural content generation (PCG) for creating the spaceship, asteroids, and fuel cells.

## 1. Project Overview

The primary goal of this project is to demonstrate various game development concepts within the Unity game engine, focusing on:

*   **3D Game Environment:** Creating an immersive space setting.

*   **Procedural Content Generation (PCG):**

    *   Dynamically generating the player's spaceship with multiple customizable parts and materials.

    *   Procedurally creating asteroid fields with varied shapes and random movement patterns.

    *   Generating fuel cell collectibles with unique meshes and materials.

*   **Player Control:** Implementing responsive spaceship controls for navigation and interaction.

*   **Camera System:** Developing a third-person camera that follows the player, allows for mouse rotation, and zooming.

*   **Game Mechanics:**

    *   Energy management for ship maneuvers.

    *   Health system with damage from asteroid collisions.

    *   Collectible fuel cells for energy replenishment and scoring.

    *   Time-based scoring bonuses.

*   **UI System:** Displaying critical game information (score, time, energy, health) and game state messages (pause, game over).

*   **Object-Oriented Programming (OOP):** Structuring the codebase with distinct classes for different game components and functionalities.

*   **Audio Management:** Implementing a centralized system for background music and sound effects.

## 2. Features

*   **Procedurally Generated Spaceship:**

    *   The player's spaceship is created at runtime using the SpaceshipGenerator.cs script.

    *   Features multiple distinct parts (nose, hull, cockpit, wings, tail, engine, guns) with customizable dimensions, positions, rotations, and colors.

    *   Includes emissive materials for glow effects on engines, wing tips, and gun tips.

*   **Procedurally Generated Asteroids:**

    *   Asteroids are generated using AsteroidGenerator.cs with varied shapes based on icosphere subdivision, irregular scaling, and FBM noise deformation.

    *   Each asteroid has a unique, procedurally generated flat-shaded mesh and can have a random grayscale material.

    *   Asteroids move randomly with continuous rotation, managed by AsteroidAnimator.cs.

*   **Procedurally Generated Fuel Cells:**

    *   Fuel cells are generated as cylindrical meshes using FuelCellGenerator.cs with customizable dimensions and emissive materials.

    *   Spawned by FuelCellSpawner.cs within a defined area, attempting to avoid overlaps.

    *   Animated with rotation and floating effects managed by FuelCell.cs.

*   **Player Spaceship Control (SpaceshipController.cs):**

    *   Full 3D movement: forward/backward thrust, horizontal strafing (left/right), vertical strafing (up/down).

    *   Yaw rotation (turning left/right).

    *   Energy consumption tied to movement and turning actions.

    *   Health system with damage calculated from asteroid collision impact force.

*   **Dynamic Camera (CameraController.cs):**

    *   Third-person camera that smoothly follows the player's spaceship.

    *   Mouse-controlled rotation (orbiting around the spaceship).

    *   Mouse scroll wheel controlled zoom.

*   **Game Management (GameManager.cs):**

    *   Tracks and displays score, remaining time, player energy, and player health via UI.

    *   Manages game state (playing, paused, game over).

    *   Implements multiple game end conditions:

        *   Victory: All fuel cells collected.

        *   Defeat: Time runs out, spaceship health depleted, or spaceship energy depleted.

    *   Handles pause menu functionality (resume, restart, quit).

    *   Calculates score with a time bonus for collecting fuel cells quickly.

*   **Audio System (AudioManager.cs):**

    *   Centralized management for background music, jingles (win/lose), and sound effects (asteroid impact, fuel pickup, button clicks, low energy warning).

    *   Singleton pattern for easy access.

    *   Handles music transitions based on game state and scene loading.

*   **User Interface (UI):**

    *   Displays Score, Time, Energy, and Health using TextMeshPro.

    *   Pause Menu with Resume, Restart, and Quit options.

    *   Game Over screen displaying win/loss status, reason, and final score.

*   **Initial Settings (SettingsController.cs):**

    *   Sets the game to full-screen windowed mode at the current monitor's resolution.

    *   Applies a skybox material for the space background.

## 3. Technologies Used

*   **Game Engine:** Unity (Version 6.0.25f1 used during development, likely compatible with similar versions)

*   **Programming Language:** C#

*   **Key Unity Features:**

    *   Rigidbody physics for spaceship and asteroid movement.

    *   Procedural Mesh Generation.

    *   Material and Shader manipulation (URP Lit/Standard with Emission).

    *   UI System (Canvas, TextMeshPro, Buttons).

    *   Event System (for OnAllFuelCellsCollected).

    *   Input System (legacy Input Manager).

    *   AudioSource components.

    *   Scene Management.

## 4. Setup and Running the Project

**Prerequisites:**

*   Unity Hub.

*   Unity Editor (developed with Unity 6.0.25f1).

*   An IDE like Visual Studio, VS Code (with Unity integration), or JetBrains Rider.

**Steps:**

*   **1. Clone the Repository:**
```bash
git clone https://github.com/Semtomer/GalacticEscape-SEN3301.git
cd GalacticEscape
 ```

*   **2. Open in Unity Hub:**

    *   Open Unity Hub.

    *   Click "Add" or "Open".

    *   Navigate to the cloned GalacticEscape project folder and select it.

    *   Ensure you open the project with a compatible Unity Editor version.

*   **3. Open the Main Scene:**

    *   In the Unity Editor, navigate to the Assets/Scenes/ folder (or wherever your main game scene is located, e.g., "GameScene").

    *   Double-click the main game scene file to open it.

*   **4. Run the Game:**

    *   Press the Play button at the top of the Unity Editor.

## 5. Game Controls

*   **Spaceship Movement:**

    *   **Forward Thrust:** W or Up Arrow

    *   **Backward Thrust:** S or Down Arrow

    *   **Strafe Left:** A or Left Arrow

    *   **Strafe Right:** D or Right Arrow

    *   **Strafe Up:** Spacebar or Numpad +

    *   **Strafe Down:** Left Alt or Numpad -

    *   **Turn Left (Yaw):** Q

    *   **Turn Right (Yaw):** E

*   **Camera Control:**

    *   **Rotate Camera:** Hold Right Mouse Button + Move Mouse

    *   **Zoom Camera:** Mouse Scroll Wheel

*   **Game:**

    *   **Pause/Resume:** ESC

## 6. Gameplay

*   **Objective:** Collect all Fuel Cells scattered in the asteroid field.

*   **Survival:**

    *   Avoid colliding with asteroids, as they damage your ship's health.

    *   Manage your energy, which is consumed by maneuvering your spaceship.

*   **Time Limit:** You have 120 seconds to complete your objective.

*   **Scoring:**

    *   Gain points for each Fuel Cell collected.

    *   Collect Fuel Cells quickly to earn a higher time bonus for each.

*   **Game End:**

    *   **Win:** Collect all Fuel Cells.

    *   **Lose:** Time runs out, or your ship's health or energy reaches zero.

## 7. Code Structure Overview

The project is organized into several C# scripts, each responsible for a specific aspect of the game:

*   **SpaceshipGenerator.cs:** Procedurally creates the player's spaceship model.

*   **SpaceshipController.cs:** Handles player input, spaceship movement, energy, and health.

*   **AsteroidGenerator.cs:** Procedurally creates asteroid models.

*   **AsteroidAnimator.cs:** Manages the random movement and rotation of asteroids.

*   **FuelCellGenerator.cs:** Procedurally creates the fuel cell models.

*   **FuelCell.cs:** Defines the properties and animation of individual fuel cells.

*   **FuelCellSpawner.cs:** Spawns fuel cells in the game world and tracks their collection.

*   **GameManager.cs:** Manages game state, UI, scoring, time, and win/loss conditions.

*   **AudioManager.cs:** Handles all audio playback (music, SFX, jingles).

*   **CameraController.cs:** Controls the third-person game camera.

*   **SettingsController.cs:** Sets initial screen resolution and background settings.

## 8. License

This project is open source. Feel free to use, modify, and distribute it as you see fit. This project is licensed under the MIT License - see the LICENSE.md file for details.
