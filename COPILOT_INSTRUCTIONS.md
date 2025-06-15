# GitHub Copilot Instructions

This document provides guidelines for leveraging GitHub Copilot to extend and maintain this .NET WinForms lunar lander game.

## Project Overview

- **Framework**: .NET 8.0 Windows Forms
- **Language**: C#
- **Architecture**: Event-driven MVC pattern with GameEngine handling all game logic
- **Key Features**:
  - Vector‐graphics ship with rotation and thrust
  - Configurable gravity (Moon, Earth, Mars)
  - Infinite horizontal scrolling jagged terrain
  - Multi-pad progression system with automatic pad generation
  - Flat landing pads with blinking side lights that stop when used
  - HUD following the craft (Vx, Vy, Alt, Fuel)
  - Debris explosion on crash and end‐game messages
  - Comprehensive physics simulation with collision detection

## Coding Conventions

- Use PascalCase for methods, properties, fields.
- Keep existing indentation and brace style.
- Qualify ambiguous types (e.g. `System.Windows.Forms.Timer`).
- Prefer concise `InsertEdit` modifications for existing files.

## Development tools

Use Powershell terminal and commands for actions involving the file system or builds and tests. For example
to concat 2 commands to a single line use a `;` in between the commands.

The task 'Run All Tests' can be used to execute and see the results of all tests.

If you need to run the tests to see the results in the terminal use this command: `Write-Host "Starting test..."; dotnet test Tests --no-build --no-restore 2>&1; Write-Host "Test completed"`

If an AI assistant needs to create temporary files that should not be included in the project it can use the `.scratch` folder located in the repository root.

## Common Tasks & Prompts

- **Add a new control**: "Insert a button to toggle debug overlay in `Form1.Designer.cs` using Copilot suggestions."
- **Modify game physics**: "Update the gravity multiplier in `gameTimer_Tick` and adjust fuel consumption rate in `ResetGame`."
- **Terrain changes**: "Adjust `terrainVariation` clamp in `Form1_Load` and `ResetGame` so peaks don’t exceed 1/3 of screen height."
- **UI tweaks**: "Enhance the HUD drawing code in `Form1_Paint` to include FPS counter."
- **New feature**: "Implement a parallax background layer; add drawing code in `Form1_Paint` before terrain rendering."
- **Game logic changes**: "Modify collision detection or landing criteria in `GameEngine.cs`."
- **Add new crash types**: "Extend crash detection logic in `GameEngine.Tick()` method."

## File Structure

```md
ai-lander.sln
README.md
COPILOT_INSTRUCTIONS.md    ← Copilot guidance
LanderGame/
  ├ Form1.cs                ← UI layer: input handling, rendering, environment selection
  ├ Form1.Designer.cs       ← UI controls and layout
  ├ GameEngine.cs           ← NEW: Core game logic, physics, state management
  ├ Lander.cs               ← Lander entity: state, physics, rendering
  ├ LandingPad.cs           ← Landing pad entity: blinking lights, collision bounds
  ├ Terrain.cs              ← Terrain generation, height queries, rendering
  ├ Program.cs              ← Application entry point
  └ LanderGame.csproj       ← Project configuration
Tests/
  ├ Form1Tests.cs           ← UI integration tests
  ├ Form1LandingTests.cs    ← Landing scenario tests
  ├ GameEngineTests.cs      ← NEW: Core game engine tests
  ├ GameEngineExtendedTests.cs ← NEW: Extended game engine scenarios
  ├ LanderTests.cs          ← Lander entity tests
  ├ LandingPadTests.cs      ← Landing pad tests
  ├ TerrainTests.cs         ← Terrain generation tests
  ├ MultiPadTests.cs        ← Multi-pad progression tests
  └ Tests.csproj            ← Test project configuration
```

## Architecture Overview

The game now uses a clean separation of concerns with an event-driven architecture:

### GameEngine Class

- **Purpose**: Central game logic controller
- **Responsibilities**:
  - Game state management (running, paused, game over)
  - Physics simulation and collision detection
  - Landing/crash logic with multiple failure modes
  - Camera following and scrolling
  - Debris simulation
  - Multi-pad progression system
- **Events**:
  - `GameStateChanged`: Notifies UI when game state changes
  - `RequestRedraw`: Requests UI refresh for smooth animation
- **Key Methods**:
  - `Initialize()`: Sets up new game
  - `Reset()`: Resets game state
  - `Tick()`: Main game loop iteration
  - `UpdateInput()`: Processes player input

### Form1 Class

- **Purpose**: Pure UI layer
- **Responsibilities**:
  - Input handling (keyboard events)
  - Rendering all game entities
  - Environment selection (gravity settings)
  - Event delegation to GameEngine
- **No longer contains**: Game logic, physics, collision detection, state management

### Entity Classes

- **Lander**: Ship state, physics integration, vector graphics rendering
- **LandingPad**: Blinking lights, collision bounds, usage tracking
- **Terrain**: Procedural generation, height queries, flattening operations

## Best Practices

- **Single Responsibility**: Keep methods focused (e.g. separate physics, input, drawing where feasible).
- **Reusability**: Extract repeated code (e.g. terrain generation) into helper methods.
- **Performance**: Minimize allocating new objects inside tight loops (e.g. avoid `new Random()` per frame).
- **Testing**: Validate new logic by stepping through in debugger or adding comprehensive unit tests.
- **Event-Driven Architecture**: Use GameEngine events for UI communication rather than direct coupling.
- **Separation of Concerns**: Keep game logic in GameEngine, UI handling in Form1, entity behavior in respective classes.

---

## Object-Oriented & Event-Driven Guidance

- The project uses a modern event-driven architecture with clear separation of concerns:
  - `GameEngine`: Central game logic controller handling physics, collision detection, state management
  - `Form1`: Pure UI layer for input handling, rendering, and environment selection
  - `Lander`, `LandingPad`, `Terrain`: Encapsulated game entities with their own behavior and rendering

- The GameEngine communicates with the UI through events (`GameStateChanged`, `RequestRedraw`) rather than direct coupling.

- When adding new features or making changes:
  - **Game Logic**: Extend or modify `GameEngine.cs` for physics, collision detection, game rules
  - **Entity Behavior**: Modify the respective entity class (`Lander.cs`, `LandingPad.cs`, `Terrain.cs`)
  - **UI Changes**: Keep input handling and rendering code in `Form1.cs`
  - **Avoid**: Adding game logic directly to Form1 or breaking the event-driven pattern

- All game state is managed by GameEngine. Form1 should query GameEngine properties for rendering but not modify game state directly.

- When testing game logic, focus on GameEngine tests rather than UI integration tests.

## General Coding Style

- Use C# best practices for encapsulation, naming, and method structure.
- Keep each class focused on a single responsibility.
- Document new methods and classes with XML comments.

## Example Usage

```csharp
// In Form1 - Event-driven UI pattern:
private void Form1_Load(object sender, EventArgs e)
{
    gameEngine = new GameEngine();
    gameEngine.GameStateChanged += () => Invalidate();
    gameEngine.RequestRedraw += () => Invalidate();
    gameEngine.Initialize(ClientSize.Width, ClientSize.Height, gravity);
}

private void gameTimer_Tick(object sender, EventArgs e)
{
    gameEngine.UpdateInput(thrusting, rotatingLeft, rotatingRight);
    gameEngine.Tick(gameTimer.Interval, ClientSize.Width, ClientSize.Height);
}

private void Form1_Paint(object sender, PaintEventArgs e)
{
    // Query GameEngine for rendering data
    var lander = gameEngine.LanderInstance;
    var terrain = gameEngine.TerrainInstance;
    
    // Render entities
    terrain.Draw(e.Graphics, gameEngine.CameraX, ClientSize.Width);
    foreach (var pad in gameEngine.Pads)
        pad.Draw(e.Graphics);
    lander.Draw(e.Graphics, thrusting);
}
```

Keep the codebase clean, modular, and event-driven with proper separation of concerns.
