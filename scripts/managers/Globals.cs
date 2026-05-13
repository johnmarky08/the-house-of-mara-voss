using Godot;
using Godot.Collections;

public partial class Globals : Node
{
	public static Globals Instance { get; private set; }

	[ExportCategory("Gameplay")]
	[Export] public int CORRUPTION_COUNT = 0;
	[Export] public int CLARITY_TOGGLE_COUNT = 5;
	[Export] public bool IS_CLARIFYING = false;
	[Export] public Array<bool> SHARDS_COLLECTED = [false, false, false, false, false, false];
	[Export] public bool PHOTO1_COLLECTED = false;

	public override void _Ready()
	{
		Instance = this;

		Logger.Info("Initializing Globals...");
	}
}