Energy Detective XR

Energy Detective XR is a Unity-based simulated AR educational game created for the 3702ICT XR and Games Development project.

The game places the player inside a virtual living space where they act as an energy detective. The player explores the room, finds household objects that are wasting energy, answers quiz questions, and fixes each problem to improve the final score.

## Project Overview

This project focuses on energy use in typical living quarters. It uses a first-person Unity environment with simulated AR-style visual cues, such as labels, status bulbs, interaction prompts, quiz panels, score feedback, and progress tracking.

The main learning goal is to help players recognise common household energy waste in an interactive and simple way.

## Core Gameplay

The player starts from the main menu and enters the game scene. Inside the room, the player moves around using keyboard and mouse controls. Objects that are wasting energy are marked with visual indicators. When the player clicks an energy-wasting object, a quiz question appears.

If the player chooses the correct answer, the object is fixed, the score increases, and the progress display updates. If the player chooses a wrong answer, the game gives feedback and the player can try again. When all energy-wasting objects are fixed, the game displays a completion message and stops the timer.

## Key Features

| Feature | Description |

| Simulated AR experience | Uses a virtual 3D room with AR-style labels, indicators, and UI overlays |
| First-person exploration | Player can move through the environment using keyboard and mouse |
| Energy object detection | Energy-wasting objects are marked with visual cues |
| Object interaction | Player clicks objects to trigger quiz-based repair actions |
| Quiz system | Each energy object can show a question with correct and incorrect answers |
| Score system | Correct answers increase the player score |
| Timer | Tracks how long the player takes to complete the game |
| Progress tracking | Shows how many energy problems have been fixed |
| Completion feedback | Displays a final completion message when all objects are fixed |
| Main menu | Includes Start Game, Settings, How to Play, and Quit options |
| Settings menu | Supports display mode, resolution, and movement key changes |
| Flashlight | Player can toggle a flashlight during gameplay |

## Controls

| Action | Key or Input |
|---|---|
| Move forward | W |
| Move backward | S |
| Move left | A |
| Move right | D |
| Look around | Mouse movement |
| Interact with object | Left mouse click |
| Toggle flashlight | F |

Movement keys can be changed in the settings menu.

## Technologies Used

| Technology | Purpose |
|---|---|
| Unity | Main game engine |
| C# | Gameplay scripting |
| Universal Render Pipeline | Unity rendering setup |
| TextMesh Pro | UI text and readable game feedback |
| Git and GitHub | Version control and team collaboration |

## Repository Structure

Energy-Detective-XR
├── Assets
│   ├── Flashlight
│   ├── Materials
│   ├── Scenes
│   │   ├── MainMenuScene.unity
│   │   └── GameScene.unity
│   ├── Scripts
│   │   ├── DashboardUI.cs
│   │   ├── EnergyObject.cs
│   │   ├── FlashLightController.cs
│   │   ├── GameManager.cs
│   │   ├── InstructionNote.cs
│   │   ├── KickDoor.cs
│   │   ├── MainMenuManager.cs
│   │   ├── PlayerInteraction.cs
│   │   ├── PlayerKeySettings.cs
│   │   ├── PlayerMovement.cs
│   │   ├── QuizManager.cs
│   │   └── UIManager.cs
│   ├── Settings
│   └── TextMesh Pro
├── Packages
├── ProjectSettings
├── .gitignore
└── README.md
