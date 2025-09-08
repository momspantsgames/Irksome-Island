# Irksome Island

_A reference implementation for building online features and clean architecture in [Godot Engine](https://godotengine.org/) with C#._

## Purpose
This project exists as a **community reference**.
It demonstrates practical patterns for:
- Networking with Godot’s high-level multiplayer API
- Authority transfer and synchronization of props/players
- State machines for characters and NPCs
- Separation of local-only vs. replicated components
- Clean directory and namespace organization for C#

By serving as a working codebase, the goal is to give developers a starting point for their own projects.

## Features
- Example multiplayer setup (host + clients)
- Character controller in C# with finite state machine
- Prop ownership and authority transfer (server → client → server)
- UI synchronization across peers
- Project layout that balances Godot and .NET conventions
- `.editorconfig` and `.gitignore` included

## Requirements
- [Godot Engine 4.4+](https://godotengine.org/download)
- .NET 6+ SDK

## Getting Started

### Clone the repository
```bash
git clone https://github.com/momspants/godot-csharp-online-reference.git
cd godot-csharp-online-reference

