using System;
using Game;

[PluginLoaderAttribute("Small Inventory Slot", "", 0)]
class SmallInventorySlot
{
	static void Initialize()
	{
		InventorySlotWidget.ctor1 = (Action<InventorySlotWidget>)Delegate.Combine(InventorySlotWidget.ctor1, (Action<InventorySlotWidget>)ctor1);
	}
	static void ctor1(CanvasWidget slot)
	{
		slot.Size = new Engine.Vector2(58f);
	}
}