using Godot;

public partial class Room3 : TileMapLayer
{
	public override void _Ready()
	{
		if (Globals.Instance == null)
			return;

		var photo1 = GetNodeOrNull<Sprite2D>("UI/Inventory/Photo1");
		if (photo1 != null && Globals.Instance.PHOTO1_COLLECTED)
		{
			var c = photo1.Modulate;
			c.A = 1.0f;
			photo1.Modulate = c;
		}
	}
}
