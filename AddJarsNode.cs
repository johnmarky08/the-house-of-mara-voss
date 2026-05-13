using Godot;

public partial class AddJarsNode : SceneTree
{
    public override void _Initialize()
    {
        var scenePath = "res://scenes/world/room_3.tscn";
        var packedScene = ResourceLoader.Load<PackedScene>(scenePath);
        var root = packedScene.Instantiate();

        var objectsNode = root.GetNode("Objects");
        
        var jarsFinal = new Node2D();
        jarsFinal.Name = "JarsFinal";
        objectsNode.AddChild(jarsFinal);
        jarsFinal.Owner = root;

        var examine1 = new TileMapLayer();
        examine1.Name = "Examine1";
        examine1.Modulate = new Color(1, 1, 1, 0); // transparent
        examine1.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
        examine1.TileSet = ResourceLoader.Load<TileSet>("res://resources/tilemaps/core.tres");
        var script = ResourceLoader.Load<Script>("res://scripts/world/room3/jars/JarsExamine1.cs");
        examine1.SetScript(script);

        for (int x = 16; x <= 34; x++)
        {
            for (int y = 8; y <= 14; y++)
            {
                examine1.SetCell(new Vector2I(x, y), 0, new Vector2I(0, 0));
            }
        }

        examine1.Set("ExamineText", "\"The jar labeled Mara opens. The folded piece of paper inside reads: \\\"I'll wait for you.\\\"\"");
        examine1.Set("DialogueDuration", 5.0f);
        examine1.Set("DialogueFontSize", 28);

        jarsFinal.AddChild(examine1);
        examine1.Owner = root;

        var newScene = new PackedScene();
        newScene.Pack(root);
        ResourceSaver.Save(newScene, scenePath);

        GD.Print("Added JarsFinal successfully.");
        Quit();
    }
}
