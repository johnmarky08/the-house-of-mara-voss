using Godot;

public partial class Globals : Node
{
	public static Globals Instance { get; private set; }

	[ExportCategory("Gameplay")]
	[Export] public int CORRUPTION_COUNT = 0;
	[Export] public int CLARITY_TOGGLE_COUNT = 5;
	[Export] public bool IS_CLARIFYING = false;
	[Export] public bool HAS_RETURNED_FROM_PUZZLE = false;

	public override void _Ready()
	{
		Instance = this;

		Logger.Info("Initializing Globals...");
	}

	public void MarkReturnedFromPuzzle()
	{
		HAS_RETURNED_FROM_PUZZLE = true;
	}
}