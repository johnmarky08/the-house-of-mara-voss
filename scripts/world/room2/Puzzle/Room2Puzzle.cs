using System;
using Godot;

public partial class Room2Puzzle : Area2D
{
	private const string TargetWord = "HOME";
	private const string NextScenePath = "res://scenes/world/room_3.tscn";

	private Node2D _padlock;
	private LineEdit[] _letterBoxes;
	private bool _isSolved;

	public override void _Ready()
	{
		_padlock = GetNodeOrNull<Node2D>("Padlock");
		if (_padlock != null)
			_padlock.Visible = true;

		_letterBoxes = new LineEdit[4];
		for (int i = 0; i < _letterBoxes.Length; i++)
		{
			var box = GetNodeOrNull<LineEdit>($"Padlock/letter-{i}");
			if (box == null)
			{
				Logger.Error($"Room2Puzzle missing node: Padlock/letter-{i}");
				continue;
			}

			box.MaxLength = 1;
			box.Alignment = HorizontalAlignment.Center;

			int index = i;
			box.TextChanged += (string newText) => OnLetterBoxTextChanged(index, newText);
			box.GuiInput += (InputEvent @event) => OnLetterBoxGuiInput(index, @event);

			_letterBoxes[i] = box;
		}

		_letterBoxes[0]?.GrabFocus();
	}

	private void OnLetterBoxGuiInput(int index, InputEvent @event)
	{
		if (_isSolved)
			return;

		if (@event is not InputEventKey keyEvent)
			return;

		if (!keyEvent.Pressed)
			return;

		if (keyEvent.Keycode != Key.Backspace)
			return;

		var current = GetBox(index);
		if (current == null)
			return;

		if (!string.IsNullOrEmpty(current.Text))
			return;

		if (index <= 0)
			return;

		var prev = GetBox(index - 1);
		if (prev == null)
			return;

		prev.Text = "";
		prev.GrabFocus();
	}

	private void OnLetterBoxTextChanged(int index, string newText)
	{
		if (_isSolved)
			return;

		var box = GetBox(index);
		if (box == null)
			return;

		string filtered = FilterToSingleCapitalLetter(newText);
		if (box.Text != filtered)
			box.Text = filtered;

		if (filtered.Length == 1 && index < _letterBoxes.Length - 1)
			GetBox(index + 1)?.GrabFocus();

		CheckSolution();
	}

	private void CheckSolution()
	{
		if (_isSolved)
			return;

		if (_letterBoxes == null || _letterBoxes.Length != 4)
			return;

		Span<char> chars = stackalloc char[4];
		for (int i = 0; i < 4; i++)
		{
			var box = _letterBoxes[i];
			if (box == null)
				return;

			string text = box.Text?.Trim() ?? string.Empty;
			if (text.Length != 1)
				return;

			chars[i] = char.ToUpperInvariant(text[0]);
		}

		string attempt = new string(chars);
		if (!string.Equals(attempt, TargetWord, StringComparison.Ordinal))
			return;

		_isSolved = true;
		SolveAsync();
	}

	private async void SolveAsync()
	{
		await PlayDrawerAnimation();
		RevealPhotoReward();
		_padlock?.SetDeferred("visible", false);
	}

	private void RevealPhotoReward()
	{
		var photoReward = GetNodeOrNull<Area2D>("Photo1");
		if (photoReward != null)
			photoReward.Visible = true;
	}

	private async System.Threading.Tasks.Task PlayDrawerAnimation()
	{
		var drawer = GetNodeOrNull<AnimatedSprite2D>("Drawer");
		if (drawer == null)
			return;

		drawer.Play("DrawerOpen");
		await ToSignal(GetTree().CreateTimer(0.35f), SceneTreeTimer.SignalName.Timeout);
		drawer.Stop();
		drawer.Frame = 1;
	}

	private LineEdit GetBox(int index)
	{
		if (_letterBoxes == null)
			return null;

		if (index < 0 || index >= _letterBoxes.Length)
			return null;

		return _letterBoxes[index];
	}

	private static string FilterToSingleCapitalLetter(string raw)
	{
		if (string.IsNullOrWhiteSpace(raw))
			return string.Empty;

		char c = char.ToUpperInvariant(raw.Trim()[0]);
		return c is >= 'A' and <= 'Z' ? c.ToString() : string.Empty;
	}
}
