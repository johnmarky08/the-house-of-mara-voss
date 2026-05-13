using Godot;
public partial class Room2 : ExamineHandler
{
    public override void _Ready()
    {
        if (Globals.Instance != null && Globals.SHARDS_COLLECTED[1])
        {
            var rightArrow = GetNode<Area2D>("UI/RightArrow");
            var leftArrow = GetNode<Area2D>("UI/LeftArrow");
            var photo = GetNode<Sprite2D>("UI/Photo");

            var rightArrowColor = rightArrow.Modulate;
            rightArrowColor.A = 1.0f;
            rightArrow.Modulate = rightArrowColor;

            var leftArrowColor = leftArrow.Modulate;
            leftArrowColor.A = 1.0f;
            leftArrow.Modulate = leftArrowColor;

            var photoColor = photo.Modulate;
            photoColor.A = 1.0f;
            photo.Modulate = photoColor;
        }
    }
}
