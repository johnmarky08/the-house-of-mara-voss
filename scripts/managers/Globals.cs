using Godot;

public partial class Globals : Node
{
	public static Globals Instance { get; private set; }

	[ExportCategory("Gameplay")]
	[Export] public int CORRUPTION_COUNT = 0;

	public override void _Ready()
	{
		Instance = this;

		Logger.Info("Initializing Globals...");
	}
}