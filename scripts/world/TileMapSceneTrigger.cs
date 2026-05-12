using Godot;

public partial class TileMapSceneTrigger : TileMapLayer
{
    [ExportCategory("Scene Trigger")]
    [Export(PropertyHint.File, "*.tscn")] public string TargetScenePath = "";
    [Export] public bool TriggerEnabled = true;
    [Export] public bool TriggerOnLeftClick = true;

    public override void _Ready()
    {
        if (!string.IsNullOrWhiteSpace(TargetScenePath))
            SetMeta("target_scene_path", TargetScenePath);
    }

    public override void _Input(InputEvent @event)
    {
        if (Dialogue.IsInputBlocked)
            return;

        if (!TriggerEnabled || !TriggerOnLeftClick)
            return;

        if (@event is not InputEventMouseButton mouseButton)
            return;

        if (!mouseButton.Pressed || mouseButton.ButtonIndex != MouseButton.Left)
            return;

        if (!HasPointedTile(mouseButton.Position))
            return;

        TriggerSceneChange();
    }

    public void TriggerSceneChange()
    {
        if (!TriggerEnabled)
            return;

        if (string.IsNullOrWhiteSpace(TargetScenePath))
        {
            Logger.Error("TileMapSceneTrigger has no TargetScenePath: ", Name);
            return;
        }

        SceneManager.ChangeScene(TargetScenePath);
    }

    private bool HasPointedTile(Vector2 globalMousePosition)
    {
        Vector2 localMousePosition = ToLocal(globalMousePosition);
        Vector2I cell = LocalToMap(localMousePosition);
        return GetCellSourceId(cell) != -1;
    }
}