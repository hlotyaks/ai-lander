# GitHub Copilot Instructions for LanderGame

This document provides guidelines for leveraging GitHub Copilot to extend and maintain this .NET WinForms lunar lander game.

## Project Overview

- **Framework**: .NET 8.0 Windows Forms
- **Language**: C#
- **Key Features**:
  - Vector‐graphics ship with rotation and thrust
  - Configurable gravity (Moon, Earth, Mars)
  - Infinite horizontal scrolling jagged terrain
  - Flat landing pad (twice ship width) with blinking side lights
  - HUD following the craft (Vx, Vy, Alt, Fuel)
  - Debris explosion on crash and end‐game messages

## Coding Conventions

- Use PascalCase for methods, properties, fields.
- Keep existing indentation and brace style.
- Qualify ambiguous types (e.g. `System.Windows.Forms.Timer`).
- Prefer concise `InsertEdit` modifications for existing files.

## Common Tasks & Prompts

- **Add a new control**: "Insert a button to toggle debug overlay in `Form1.Designer.cs` using Copilot suggestions."
- **Modify game physics**: "Update the gravity multiplier in `gameTimer_Tick` and adjust fuel consumption rate in `ResetGame`."
- **Terrain changes**: "Adjust `terrainVariation` clamp in `Form1_Load` and `ResetGame` so peaks don’t exceed 1/3 of screen height."
- **UI tweaks**: "Enhance the HUD drawing code in `Form1_Paint` to include FPS counter."
- **New feature**: "Implement a parallax background layer; add drawing code in `Form1_Paint` before terrain rendering."

## File Structure

```md
ai-lander.sln
README.md
COPILOT_INSTRUCTIONS.md    ← Copilot guidance
LanderGame/
  ├ Form1.cs
  ├ Form1.Designer.cs
  ├ Program.cs
  └ LanderGame.csproj
```

## Best Practices

- **Single Responsibility**: Keep methods focused (e.g. separate physics, input, drawing where feasible).
- **Reusability**: Extract repeated code (e.g. terrain generation) into helper methods.
- **Performance**: Minimize allocating new objects inside tight loops (e.g. avoid `new Random()` per frame).
- **Testing**: Validate new logic by stepping through in debugger or adding simple unit tests if possible.

---
*Use these guidelines when prompting Copilot to maintain a consistent codebase and accelerate feature development.*
