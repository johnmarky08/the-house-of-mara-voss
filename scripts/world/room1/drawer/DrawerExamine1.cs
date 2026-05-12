using Godot;

public partial class DrawerExamine1 : ExamineHandler
{
	protected override void OnExamineClicked()
	{
		string roomName = "Room1";
		var parent = GetParent();
		bool isFullyExamined = RoomExamineTracker.HasRoomBeenFullyExamined(roomName);
		bool hasRightParent = parent != null && parent.Name.ToString().EndsWith("Final");

		if (isFullyExamined && hasRightParent)
		{
			Logger.Debug("[Drawer Unlocked] Changing to puzzle scene!");
			Logger.Info("Current Corruption Count: " + Globals.Instance.CORRUPTION_COUNT);
			SceneManager.ChangeScene("res://scenes/world/room_1_puzzle.tscn");
		}
	}
}