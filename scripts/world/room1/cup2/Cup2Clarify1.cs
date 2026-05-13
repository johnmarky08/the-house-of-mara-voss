using Godot;

public partial class Cup2Clarify1 : ExamineHandler
{
	protected override void OnExamineClicked()
	{
		var parent = GetParent();
		if (parent == null)
			return;

		foreach (var child in parent.GetChildren())
		{
			if (child is ExamineHandler examineHandler && examineHandler != this && child.Name.ToString().StartsWith("Examine"))
			{
				examineHandler.HoverEnabled = false;
				examineHandler.SetProcessInput(false);
				examineHandler.SetProcess(false);
				examineHandler.QueueFree();
			}
		}
	}
}