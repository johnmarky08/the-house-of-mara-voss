using System;
using System.Threading.Tasks;
using Godot;

public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }
    public static bool IsChanging { get; private set; }

    [ExportCategory("Scene Manager Variables")]
    [Export] public NodePath SceneRootPath = "../Control/SubViewportContainer/SubViewport";
    [Export] public NodePath FadeRectPath = "../Control/FadeRect";
    [Export(PropertyHint.Range, "0.05,2.0,0.05")] public float FadeDuration = 0.25f;

    private Node _sceneRoot;
    private ColorRect _fadeRect;

    public Node CurrentScene { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        IsChanging = false;

        _sceneRoot = GetNodeOrNull(SceneRootPath);
        _fadeRect = GetNodeOrNull<ColorRect>(FadeRectPath);

        if (_sceneRoot == null)
        {
            Logger.Error($"SceneManager could not resolve SceneRootPath: {SceneRootPath}");
        }

        if (_fadeRect == null)
        {
            Logger.Error($"SceneManager could not resolve FadeRectPath: {FadeRectPath}");
        }
        else
        {
            var color = _fadeRect.Color;
            color.A = 1.0f;
            _fadeRect.Color = color;
            _fadeRect.Visible = true;
            _fadeRect.MouseFilter = Control.MouseFilterEnum.Ignore;
        }

        Logger.Info("Initializing Scene Manager...");
    }

    public static async void ChangeScene(string scenePath)
    {
        if (Instance == null)
        {
            Logger.Error("SceneManager.ChangeScene called before SceneManager was initialized.");
            return;
        }

        if (string.IsNullOrWhiteSpace(scenePath))
        {
            Logger.Error("SceneManager.ChangeScene received an empty scene path.");
            return;
        }

        while (IsChanging)
            await Instance.ToSignal(Instance.GetTree(), SceneTree.SignalName.ProcessFrame);

        IsChanging = true;

        try
        {
            await Instance.LoadScene(scenePath);
            await Instance.FadeIn();
        }
        catch (Exception ex)
        {
            Logger.Error("Scene change failed: ", ex.Message);
        }
        finally
        {
            IsChanging = false;
        }
    }

    public static void ChangeSceneFromTileMapLayer(TileMapLayer layer)
    {
        if (layer == null)
            return;

        string targetScene = layer.GetMeta("target_scene_path", "").AsString();
        if (string.IsNullOrWhiteSpace(targetScene))
        {
            Logger.Error("TileMapLayer does not define metadata key 'target_scene_path': ", layer.Name);
            return;
        }

        ChangeScene(targetScene);
    }

    private async Task LoadScene(string scenePath)
    {
        if (_sceneRoot == null)
            throw new Exception("Scene root is null. Cannot load scenes.");

        if (CurrentScene != null)
        {
            await FadeOut();
            _sceneRoot.RemoveChild(CurrentScene);
            CurrentScene.QueueFree();
            CurrentScene = null;
        }

        var packedScene = GD.Load<PackedScene>(scenePath);
        if (packedScene == null)
            throw new Exception("Could not load scene at path: " + scenePath);

        CurrentScene = packedScene.Instantiate<Node>();
        _sceneRoot.AddChild(CurrentScene);
    }

    private async Task FadeOut()
    {
        if (_fadeRect == null)
            return;

        Tween tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", 1.0f, FadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    private async Task FadeIn()
    {
        if (_fadeRect == null)
            return;

        Tween tween = CreateTween();
        tween.TweenProperty(_fadeRect, "color:a", 0.0f, FadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);
    }
}