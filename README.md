[![.NET Build & Test](https://github.com/hloty/ai-lander/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/hloty/ai-lander/actions/workflows/dotnet-build.yml)
[![codecov](https://codecov.io/gh/hloty/ai-lander/branch/main/graph/badge.svg)](https://codecov.io/gh/hloty/ai-lander)

# AI Lander

AI Lander is a classic lunar lander-style game built with C# and Windows Forms. The player controls a lander as it descends onto a procedurally generated terrain, aiming to land safely on a designated landing pad.

## Gameplay

- Use the keyboard to rotate and thrust the lander.
- Land gently on the green landing pad to win.
- Avoid crashing into the terrain or running out of fuel.
- The terrain and landing pad are randomly generated each game.

## Controls

- **Up Arrow**: Thrust
- **Left/Right Arrows**: Rotate
- **R**: Restart after crash or landing
- **X**: Exit the game

## Features

- Object-oriented design: Lander, LandingPad, and Terrain are encapsulated as separate classes.
- Realistic gravity and physics for different environments (Moon, Earth, Mars).
- Blinking landing pad lights and debris effects on crash.
- Modern, clean codebase for easy extension and experimentation.

## Technical Notes

- Built with .NET (Windows Forms).
- To run: open the solution in Visual Studio or run `dotnet run --project LanderGame/LanderGame.csproj` from the command line.

Enjoy the challenge of landing safely!
