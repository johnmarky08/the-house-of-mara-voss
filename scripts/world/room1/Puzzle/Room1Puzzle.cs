using Godot;
using System;

public partial class Room1Puzzle : Area2D
{
	private const string PuzzleCursorPath = "res://assets/images/core/cursor_magnifying_glass.png";
	private const string PointerCursorPath = "res://assets/images/core/cursor_pointer.png";

	private int[] expectedCombination = { 7, 1, 4, 8, 3 };
	private string _currentHoverCursorPath = string.Empty;

	public override void _Ready()
	{
		InputPickable = true;
		for (int i = 0; i < 5; i++)
		{
			var spinbox = GetNode<SpinBox>($"Padlock/padlock-{i}");
			if (spinbox != null)
			{

				spinbox.MinValue = 0;
				spinbox.MaxValue = 9;
				spinbox.Step = 1;
				spinbox.ValueChanged += (_) => CheckPadlockSolution();
			}
		}
	}

	public override void _Process(double delta)
	{
		string desiredCursor = GetHoverCursorPath();

		if (string.IsNullOrEmpty(desiredCursor))
		{
			if (!string.IsNullOrEmpty(_currentHoverCursorPath))
			{
				CursorHelper.EndHover(this);
				_currentHoverCursorPath = string.Empty;
			}

			return;
		}

		if (_currentHoverCursorPath != desiredCursor)
		{
			CursorHelper.BeginHover(this, desiredCursor, 32, 32);
			_currentHoverCursorPath = desiredCursor;
		}
	}

	public override void _ExitTree()
	{
		if (!string.IsNullOrEmpty(_currentHoverCursorPath))
		{
			CursorHelper.EndHover(this);
			_currentHoverCursorPath = string.Empty;
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is not InputEventMouseButton mouseEvent)
			return;

		if (!mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left)
			return;


		var brassKey = GetNodeOrNull<Area2D>("BrassKey1");
		if (brassKey != null && brassKey.Visible)
		{
			var cs = brassKey.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
			if (cs?.Shape is RectangleShape2D rect)
			{
				var localMouse = cs.GetGlobalTransform().AffineInverse() * mouseEvent.Position;
				var half = rect.Size / 2f;
				if (localMouse.X >= -half.X && localMouse.X <= half.X && localMouse.Y >= -half.Y && localMouse.Y <= half.Y)
				{
					Globals.Instance.SHARDS_COLLECTED[0] = true;
					SceneManager.ChangeScene("res://scenes/world/room_1.tscn");
					GetViewport().SetInputAsHandled();
					return;
				}
			}
		}

		if (IsClickInsidePuzzle(mouseEvent.Position))
		{
			OnPuzzleClicked();
			GetViewport().SetInputAsHandled();
		}
	}

	private bool IsClickInsidePuzzle(Vector2 globalMousePosition)
	{
		var collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (collisionShape?.Shape is not RectangleShape2D rect)
			return false;

		var localMousePosition = collisionShape.GetGlobalTransform().AffineInverse() * globalMousePosition;
		var halfSize = rect.Size / 2f;

		return localMousePosition.X >= -halfSize.X &&
			   localMousePosition.X <= halfSize.X &&
			   localMousePosition.Y >= -halfSize.Y &&
			   localMousePosition.Y <= halfSize.Y;
	}

	private void OnPuzzleClicked()
	{
		GetNode<Node2D>("Padlock").Visible = true;

	}

	private string GetHoverCursorPath()
	{
		var mousePosition = GetGlobalMousePosition();

		var brassKey = GetNodeOrNull<Area2D>("BrassKey1");
		if (brassKey != null && brassKey.Visible && IsInsideArea2D(brassKey, mousePosition))
			return PointerCursorPath;

		var padlock = GetNodeOrNull<Node2D>("Padlock");
		if (padlock != null && padlock.Visible && IsMouseOverPadlock(mousePosition))
			return PointerCursorPath;

		if (IsClickInsidePuzzle(mousePosition))
			return PuzzleCursorPath;

		return string.Empty;
	}

	private bool IsInsideArea2D(Area2D area, Vector2 globalMousePosition)
	{
		var collisionShape = area.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (collisionShape?.Shape is not RectangleShape2D rect)
			return false;

		var localMousePosition = collisionShape.GetGlobalTransform().AffineInverse() * globalMousePosition;
		var halfSize = rect.Size / 2f;

		return localMousePosition.X >= -halfSize.X &&
			   localMousePosition.X <= halfSize.X &&
			   localMousePosition.Y >= -halfSize.Y &&
			   localMousePosition.Y <= halfSize.Y;
	}

	private bool IsMouseOverPadlock(Vector2 globalMousePosition)
	{
		for (int i = 0; i < 5; i++)
		{
			var spinbox = GetNodeOrNull<Control>($"Padlock/padlock-{i}");
			if (spinbox != null && spinbox.IsVisibleInTree() && spinbox.GetGlobalRect().HasPoint(globalMousePosition))
				return true;
		}

		var title = GetNodeOrNull<Control>("Padlock/Title");
		return title != null && title.IsVisibleInTree() && title.GetGlobalRect().HasPoint(globalMousePosition);
	}

	private void CheckPadlockSolution()
	{

		int[] currentValues = new int[5];
		bool allFound = true;

		for (int i = 0; i < 5; i++)
		{
			var spinbox = GetNode<SpinBox>($"Padlock/padlock-{i}");
			if (spinbox != null)
			{
				currentValues[i] = (int)spinbox.Value;
			}
			else
			{
				allFound = false;
				break;
			}
		}

		if (!allFound) return;


		bool isCorrect = true;
		for (int i = 0; i < 5; i++)
		{
			if (currentValues[i] != expectedCombination[i])
			{
				isCorrect = false;
				break;
			}
		}

		if (isCorrect)
			PlayDrawerAnimation();

	}

	private async void PlayDrawerAnimation()
	{
		var drawer = GetNode<AnimatedSprite2D>("Drawer");
		if (drawer == null)

			return;


		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

		drawer.Play("DrawerOpen");


		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		drawer.Stop();
		drawer.Frame = 1;


		var padlock = GetNode<Node2D>("Padlock");
		if (padlock != null)
			padlock.Visible = false;


		await ToSignal(GetTree().CreateTimer(0.03f), "timeout");

		var brassKey1 = GetNode<Area2D>("BrassKey1");
		if (brassKey1 != null)
			brassKey1.Visible = true;
	}
}