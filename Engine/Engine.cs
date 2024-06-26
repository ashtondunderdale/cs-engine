﻿using System.Text.Json;

namespace engine;

internal abstract class Engine
{
    private readonly Vector2 ScreenSize = new(1080, 768);
    private readonly Canvas Window;
    public readonly Color BackgroundColour = Color.White;

    private readonly Thread GameLoopThread;
    public static string? ProjectPath;

    public static List<Shape2D> Shapes = new();
    public static List<Sprite2D> Sprites = new();

    public Engine()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Window = new Canvas()
        {
            Size = new((int)ScreenSize.X, (int)ScreenSize.Y),
            Text = "New Game",
        };

        Window.Paint += Renderer;

        Window.KeyDown += Window_KeyDown;
        Window.KeyUp += Window_KeyUp;

        GameLoopThread = new Thread(GameLoop);
        GameLoopThread.Start();

        Application.Run(Window);
    }

    void GameLoop()
    {
        OnLoad();

        while (GameLoopThread.IsAlive)
        {
            try
            {
                OnDraw();
                Window.BeginInvoke((MethodInvoker)delegate { Window.Refresh(); });
                OnUpdate();

                Thread.Sleep(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    private void Renderer(object sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        //g.Clear(BackgroundColour);

        foreach (Shape2D shape in Shapes)
            g.FillRectangle(new SolidBrush(Color.Red), (int)shape.Position.X, (int)shape.Position.Y, (int)shape.Scale.X, (int)shape.Scale.Y);

        foreach (Sprite2D sprite in Sprites)
            g.FillRectangle(new SolidBrush(Color.Blue), (int)sprite.PlayPosition.X, (int)sprite.PlayPosition.Y, (int)sprite.Scale.X, (int)sprite.Scale.Y);
    }

    public static void RegisterShape(Shape2D shape)
    {
        Shapes.Add(shape);
        UpdateListBoxItems();
    }

    public static void UnregisterShape(Shape2D shape)
    {
        Shapes.Remove(shape);
        UpdateListBoxItems();
    }

    public static void UpdateListBoxItems()
    {
        GetCanvas()!.UpdateListBoxItems();
    }

    public static Canvas? GetCanvas()
    {
        if (Application.OpenForms.Count > 0)
        {
            Form canvasForm = Application.OpenForms[0];

            if (canvasForm is Canvas canvas)
                return canvas;
        }

        return null;
    }

    public static void RegisterSprite(Sprite2D sprite)
    {
        Sprites.Add(sprite);
        UpdateListBoxItems();
    }

    public static void UnregisterSprite(Sprite2D sprite)
    {
        Sprites.Remove(sprite);
        UpdateListBoxItems();
    }

    public static void SaveAsJson(string filePath)
    {
        if (ProjectPath is not null) filePath = ProjectPath;

        string skytaieFolderPath = Path.Combine(filePath, "skytaie");

        if (!Directory.Exists(skytaieFolderPath) && ProjectPath is null)
            Directory.CreateDirectory(skytaieFolderPath);
        else skytaieFolderPath = ProjectPath;

        string shapesFilePath = Path.Combine(skytaieFolderPath, "shapes.json");
        string spritesFilePath = Path.Combine(skytaieFolderPath, "sprites.json");

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        List<Shape2D> shapesList = new(Shapes);
        List<Sprite2D> spritesList = new(Sprites);

        string shapesJson = JsonSerializer.Serialize(shapesList, options);
        File.WriteAllText(shapesFilePath, shapesJson);

        string spritesJson = JsonSerializer.Serialize(spritesList, options);
        File.WriteAllText(spritesFilePath, spritesJson);
    }


    public static void LoadJson(string path)
    {
        string skytaieFolderPath = Path.Combine(path);

        if (Directory.Exists(skytaieFolderPath))
        {
            string[] jsonFiles = Directory.GetFiles(skytaieFolderPath, "*.json");

            foreach (string jsonFile in jsonFiles)
            {
                string json = File.ReadAllText(jsonFile);

                if (jsonFile.EndsWith("shapes.json"))
                    JsonSerializer.Deserialize<List<Shape2D>>(json);

                else if (jsonFile.EndsWith("sprites.json"))
                    JsonSerializer.Deserialize<List<Sprite2D>>(json);
            }
        }

        ProjectPath = path;
    }



    public void Window_KeyDown(object sender, KeyEventArgs e) => GetKeyDown(e); 
    public void Window_KeyUp(object sender, KeyEventArgs e) => GetKeyUp(e);
 
    public abstract void OnLoad();
    public abstract void OnUpdate();
    public abstract void OnDraw();
    public abstract void GetKeyDown(KeyEventArgs e);
    public abstract void GetKeyUp(KeyEventArgs e);
}
