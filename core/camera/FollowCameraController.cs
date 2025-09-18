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

namespace IrksomeIsland.Core.Camera;

public partial class FollowCameraController : CameraController
{

	public float FollowSpeed { get; set; } = 10f;
	public bool LookAtTarget { get; set; } = true;
	public Vector3 Offset { get; set; } = new(0, 2.0f, -5.0f);
	private bool OffsetLocal { get; set; } = true;
	public Node3D? Target { get; set; }

	public void SetTarget(Node3D target) => Target = target;

	public override void OnAttach(CameraRig rig)
	{
		if (Target == null) return;
		var desired = ComputeDesiredPosition();
		rig.GlobalPosition = desired;
		if (LookAtTarget) rig.LookAt(Target.GlobalTransform.Origin, Vector3.Up);
	}

	public override void OnDetach(CameraRig rig)
	{
		// nothing
	}

	public override void HandleInput(CameraRig rig, InputEvent e)
	{
		// no input
	}

	public override void UpdateCamera(CameraRig rig, double delta)
	{
		if (Target == null || !IsInstanceValid(Target)) return;

		var desired = ComputeDesiredPosition();

		var a = 1f - Mathf.Exp(-FollowSpeed * (float)delta);
		rig.GlobalPosition = rig.GlobalPosition.Lerp(desired, a);

		if (!LookAtTarget) return;
		var targetPos = Target.GlobalTransform.Origin;
		targetPos.Y = rig.GlobalTransform.Origin.Y;
		if ((targetPos - rig.GlobalTransform.Origin).LengthSquared() > 1e-6f)
			rig.LookAt(targetPos, Vector3.Up);
	}

	private Vector3 ComputeDesiredPosition()
	{
		if (Target == null) return Vector3.Zero;

		return OffsetLocal
			? Target.GlobalTransform.Origin + Target.GlobalTransform.Basis * Offset
			: Target.GlobalTransform.Origin + Offset;
	}
}
