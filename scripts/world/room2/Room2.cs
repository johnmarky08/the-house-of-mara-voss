using Godot;

public partial class Room2 : ExamineHandler
{
    public override void _Ready()
    {
        if (Globals.Instance != null && Globals.Instance.PHOTO1_COLLECTED)
        {
            var tableExamine = GetNodeOrNull<TileMapLayer>("Objects/TableFinal/Examine1");
            var photoSprite = GetNodeOrNull<Sprite2D>("UI/Inventory/Photo1");
            var rightArrow = GetNodeOrNull<Sprite2D>("UI/RightArrow");

            if (tableExamine != null)
            {
                tableExamine.Visible = false;
                tableExamine.QueueFree();
            }

            if (photoSprite != null)
            {
                var color = photoSprite.Modulate;
                color.A = 1.0f;
                photoSprite.Modulate = color;
            }

            if (rightArrow != null)
            {
                var rightArrowColor = rightArrow.Modulate;
                rightArrowColor.A = 1.0f;
                rightArrow.Modulate = rightArrowColor;
            }
        }
    }
}
