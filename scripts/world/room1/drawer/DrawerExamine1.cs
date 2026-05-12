using Godot;

public partial class DrawerExamine1 : ExamineHandler
{
	protected override void OnExamineClicked()
	{
		string roomName = "Room1";
		var parent = GetParent();
		if (RoomExamineTracker.HasRoomBeenFullyExamined(roomName) && parent != null && parent.Name.ToString().EndsWith("Final"))
		{
			SceneManager.ChangeScene("res://scenes/world/room_1_puzzle.tscn");
		}
	}
}