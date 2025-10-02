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

using IrksomeIsland.Core.Props;

namespace IrksomeIsland.Core.Bus;

public sealed class CharacterBus
{
	public event Action? InteractionRequested;
	public event Action<NetworkedProp, string>? EquipRequested;
	public event Action<NetworkedProp, string>? Equipped;
	public event Action<int>? InteractionCycleRequested;
    public event Action? PrimaryUseRequested;
    public event Action? SecondaryUseRequested;

	public void RaiseInteractionRequested() => InteractionRequested?.Invoke();
	public void RaiseEquipRequested(NetworkedProp n, string slot) => EquipRequested?.Invoke(n, slot);
	public void RaiseEquipped(NetworkedProp n, string slot) => Equipped?.Invoke(n, slot);
	public void RaiseInteractionCycleRequested(int dir) => InteractionCycleRequested?.Invoke(dir);
    public void RaisePrimaryUseRequested() => PrimaryUseRequested?.Invoke();
    public void RaiseSecondaryUseRequested() => SecondaryUseRequested?.Invoke();
}
