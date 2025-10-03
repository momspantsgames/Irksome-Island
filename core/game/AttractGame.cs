// Copyright (c) 2025 Momspants Games
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Godot;
using IrksomeIsland.Core.Constants;
using IrksomeIsland.Ui.Menus.Main;

namespace IrksomeIsland.Core.Game;

public partial class AttractGame(GameConfiguration config) : IrkGame(config)
{
	private MainMenu? _mainMenu;

	public override void _Ready()
	{
		base._Ready();

		Name = "AttractGame";

		var scene = GD.Load<PackedScene>(Paths.MainMenuScene);
		_mainMenu = scene.Instantiate<MainMenu>();
		AddChild(_mainMenu);
	}

    public override void StartGame()
    {
        base.StartGame();

        // Position the camera rig for attract mode (no controller)
        if (CameraRig != null)
        {
            // Try to focus near the world's spawn point if present
            var spawn = GetNodeOrNull<Marker3D>("MainWorld/PlayerSpawnPoint");
            var world = GetNodeOrNull<Node3D>(NodeNames.WorldMain);
            var focus = spawn?.GlobalPosition ?? world?.GlobalPosition ?? Vector3.Zero;

            // Raise pivot slightly so we look over geometry
            CameraRig.DesiredPivot = focus + new Vector3(0, 2.0f, 0);
            CameraRig.DesiredArmLength = 10.0f;

            // Aim the rig at a shallow downward angle
            var yawDeg = 25.0f;
            var pitchRad = -0.35f; // ~-20 degrees
            var yawRad = Mathf.DegToRad(yawDeg);
            var cp = Mathf.Cos(pitchRad);
            var dir = new Vector3(cp * Mathf.Sin(yawRad), Mathf.Sin(pitchRad), cp * Mathf.Cos(yawRad));
            CameraRig.LookAt(CameraRig.DesiredPivot + dir, Vector3.Up);
            CameraRig.Cam.MakeCurrent();
        }
    }
}
