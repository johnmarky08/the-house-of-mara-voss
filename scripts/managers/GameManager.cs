using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    [ExportCategory("Game Flow")]
    [Export(PropertyHint.File, "*.tscn")] public string FirstRoomScenePath = "res://scenes/world/room_3.tscn";

    public SubViewport GameViewport { get; private set; }

    public override async void _Ready()
    {
        Instance = this;
        GameViewport = GetNodeOrNull<SubViewport>("Control/SubViewportContainer/SubViewport");

        if (GameViewport == null)
        {
            Logger.Error("GameManager could not find SubViewport at Control/SubViewportContainer/SubViewport.");
            return;
        }

        Logger.Info("Initializing Game Manager...");
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        SceneManager.ChangeScene(FirstRoomScenePath);
    }

    public static SubViewport GetGameViewport()
    {
        return Instance?.GameViewport;
    }
}