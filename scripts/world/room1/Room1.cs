using Godot;
public partial class Room1 : ExamineHandler
{
    public override void _Ready()
    {
        if (Globals.Instance != null && Globals.Instance.SHARDS_COLLECTED[0])
        {
            var drawerExamine1 = GetNode<TileMapLayer>("Objects/DrawerFinal/Examine1");
            var drawerExamine2 = GetNode<TileMapLayer>("Objects/DrawerFinal/Examine2");
            var brassKey = GetNode<Sprite2D>("UI/Inventory/BrassKey");
            var rightArrow = GetNode<Sprite2D>("UI/RightArrow");

            drawerExamine1.Visible = false;
            drawerExamine1.QueueFree();

            var color = drawerExamine2.Modulate;
            color.A = 1.0f;
            drawerExamine2.Modulate = color;

            var keyColor = brassKey.Modulate;
            keyColor.A = 1.0f;
            brassKey.Modulate = keyColor;

            var rightArrowColor = rightArrow.Modulate;
            rightArrowColor.A = 1.0f;
            rightArrow.Modulate = rightArrowColor;
        }
    }
}
