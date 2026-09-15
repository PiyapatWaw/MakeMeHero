using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MakeMeHero.EditorTools
{
    // Assembles only sprites already present in Assets/Art. No art or import settings are changed.
    public static class ExistingAssetsSceneBuilder
    {
        private const string Board = "Assets/Art/Board/";
        private const string Characters = "Assets/Art/Characters/";
        private const string Health = "Assets/Art/UI/health-bar-spritesheet.png";
        private const float Width = 1672f, Height = 941f, Ppu = 100f;
        private static readonly Color Gold = new Color(0.95f, 0.82f, 0.57f);
        private static Sprite _barFrame, _barBack, _barGreen, _barRed;
        private static Font _font;

        [MenuItem("Make Me Hero/Arrange Reference - Existing Assets Only")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            RequireAssets();
            var previous = SceneManager.GetActiveScene();
            var previousPath = previous.path;
            var rebuildCurrent = previous.name == "AurelliaAssetScene" && previous.GetRootGameObjects().Any(g => g.name == "Aurellia - Existing Asset Composition");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            var root = new GameObject("Aurellia - Existing Asset Composition").transform;
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = Height / Ppu / 2f;
            camera.transform.position = new Vector3(0, 0, -20);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.05f, 0.075f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<MakeMeHero.Game.ReferenceCameraFit>();

            var environment = Group(root, "01 - Battlefield");
            Place(environment, "Parchment Background", First(Board + "battlefield-background.png"), 836, 470.5f, Width, Height, -100);
            var grid = Group(root, "02 - Battle Grid (4 columns x 3 rows)");
            float[,] centers = { { 261, 477, 720, 968 }, { 218, 466, 724, 986 }, { 166, 442, 720, 1012 } };
            float[] rowY = { 326, 469, 628 };
            float[] tileW = { 195, 213, 236 };
            float[] tileH = { 95, 114, 126 };
            var connector = First(Board + "connector-horizontal.png");
            for (int row = 0; row < 3; row++)
            for (int col = 0; col < 4; col++)
            {
                string tileAsset = col == 0 && row == 1 ? "tile-city-outline.png" : "tile-outline.png";
                Place(grid, "Tile " + (row + 1) + "-" + (col + 1), First(Board + tileAsset), centers[row, col], rowY[row], tileW[row], tileH[row], -20);
                if (col < 3)
                    Place(grid, "Link " + (row + 1) + "-" + (col + 1) + " horizontal", connector,
                        (centers[row, col] + centers[row, col + 1]) / 2, rowY[row], 32, 7, -21);
                if (row < 2)
                {
                    var link = Place(grid, "Link " + (row + 1) + "-" + (col + 1) + " vertical", connector,
                        (centers[row, col] + centers[row + 1, col]) / 2, (rowY[row] + rowY[row + 1]) / 2, 28, 7, -21);
                    link.transform.rotation = Quaternion.Euler(0, 0, 77);
                }
            }

            var enemies = Group(root, "03 - Enemy Spawn");
            Place(enemies, "Spawn Tile", First(Board + "tile-spawn-outline.png"), 1400, 503, 420, 222, -19);
            Place(enemies, "Monster Portal", First(Board + "vortex-idle-spritesheet.png"), 1460, 340, 328, 345, -5);

            var units = Group(root, "04 - Characters");
            Unit(units, "Archer", Idle(Characters + "archer-spritesheet.png"), 639, 334, 156, false);
            Unit(units, "Healer", Idle(Characters + "healer-spritesheet.png"), 223, 462, 183, false);
            Unit(units, "Soldier", Idle(Characters + "soldier-spritesheet.png"), 491, 503, 180, false);
            Unit(units, "Mage", Idle(Characters + "mage-spritesheet.png"), 310, 577, 177, false);
            Unit(units, "Wolf", Idle(Characters + "wolf-spritesheet.png"), 727, 502, 123, true);
            Unit(units, "Wolf Reinforcement", Idle(Characters + "wolf-spritesheet.png"), 962, 651, 125, true);

            var canvasObject = new GameObject("05 - HUD", typeof(RectTransform), typeof(Canvas));
            canvasObject.transform.SetParent(root, false);
            canvasObject.transform.localScale = Vector3.one / Ppu;
            var canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(Width, Height);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            canvas.sortingOrder = 200;
            var ui = canvas.transform;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _barFrame = Named(Health, "health-bar-spritesheet_0");
            _barBack = Named(Health, "health-bar-spritesheet_1");
            _barGreen = Named(Health, "health-bar-spritesheet_2");
            _barRed = Named(Health, "health-bar-spritesheet_3");

            // The existing dark bar sprite also supplies the HUD panels.
            Picture(ui, "Header", _barBack, 0, 0, Width, 82);
            Label(ui, "Kingdom", "Aurellia", 28, 7, 300, 42, 34, Gold);
            Label(ui, "Subtitle", "Last Light Defenders", 30, 47, 285, 25, 21, new Color(0.88f, 0.86f, 0.80f));
            Bar(ui, "City Health", 360, 14, 269, 55, 1f, true);
            Label(ui, "City Health Value", "30 / 30", 387, 21, 215, 37, 30, Color.white, TextAnchor.MiddleCenter);
            Panel(ui, "Gold", "320", 651, 17, 160, 51, 30);
            Panel(ui, "Day", "Day 3", 841, 17, 198, 51, 30);
            Panel(ui, "Speed Display", ">> x2", 1512, 159, 140, 65, 34);
            Panel(ui, "Field Count", "FIELD   4 / 10", 24, 829, 350, 65, 29);

            var bars = Rect(ui, "Unit Health Bars", 0, 0, Width, Height);
            Bar(bars, "Healer Health", 179, 292, 89, 20, 1f, false);
            Bar(bars, "Soldier Health", 440, 330, 98, 20, 0.82f, false);
            Bar(bars, "Wolf Health", 688, 384, 82, 17, 0.51f, true);
            Bar(bars, "Reinforcement Health", 922, 529, 82, 17, 0.64f, true);

            Directory.CreateDirectory("Assets/Scenes");
            var path = rebuildCurrent ? previousPath : AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/AurelliaAssetScene.unity");
            if (!EditorSceneManager.SaveScene(scene, path)) throw new IOException("Unable to save " + path);
            Selection.activeGameObject = root.gameObject;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.in2DMode = true;
                SceneView.lastActiveSceneView.LookAt(Vector3.zero, Quaternion.identity, 6f, true);
            }
            EditorApplication.delayCall += Capture;
            Debug.Log("Existing-assets reference scene saved: " + path + ". 12 tiles, 6 characters, portal, editable HUD. Art reused without modification.");
        }

        [MenuItem("Make Me Hero/Capture Existing Asset Scene")]
        public static void Capture()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.name.StartsWith("AurelliaAssetScene", StringComparison.Ordinal)) return;
            var camera = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Camera>()).First();
            var oldTarget = camera.targetTexture;
            var oldActive = RenderTexture.active;
            var oldSize = camera.orthographicSize;
            var target = new RenderTexture(1672, 941, 24);
            var pixels = new Texture2D(1672, 941, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.orthographicSize = Height / Ppu / 2;
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 1672, 941), 0, 0);
                pixels.Apply();
                Directory.CreateDirectory("Library/AurelliaPreview");
                File.WriteAllBytes("Library/AurelliaPreview/scene.png", pixels.EncodeToPNG());
                var missing = scene.GetRootGameObjects().Sum(g => g.GetComponentsInChildren<Transform>(true).Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
                File.WriteAllText("Library/AurelliaPreview/validation.txt", "Scene: " + scene.path + "\nMissing scripts: " + missing + "\nSpriteRenderers: " + scene.GetRootGameObjects().Sum(g => g.GetComponentsInChildren<SpriteRenderer>(true).Length) + "\nCapture: 1672 x 941\n");
            }
            finally
            {
                camera.targetTexture = oldTarget;
                camera.orthographicSize = oldSize;
                RenderTexture.active = oldActive;
                UnityEngine.Object.DestroyImmediate(pixels);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void RequireAssets()
        {
            foreach (var path in new[] { Board + "battlefield-background.png", Board + "tile-outline.png", Board + "tile-city-outline.png", Board + "tile-spawn-outline.png", Board + "connector-horizontal.png", Board + "vortex-idle-spritesheet.png", Health,
                Characters + "soldier-spritesheet.png", Characters + "archer-spritesheet.png", Characters + "mage-spritesheet.png", Characters + "healer-spritesheet.png", Characters + "wolf-spritesheet.png" })
                if (First(path) == null) throw new InvalidOperationException("Missing existing sprite: " + path);
        }
        private static Sprite[] Sprites(string path) { return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray(); }
        private static Sprite First(string path) { return Sprites(path).OrderByDescending(s => s.rect.width * s.rect.height).FirstOrDefault(); }
        private static Sprite Named(string path, string name) { return Sprites(path).First(s => s.name == name); }
        private static Sprite Idle(string path) { return Sprites(path).Where(s => s.rect.height > 100).OrderByDescending(s => Mathf.Round(s.rect.y / 20)).ThenBy(s => s.rect.x).First(); }
        private static Transform Group(Transform parent, string name) { var go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform; }
        private static GameObject Place(Transform parent, string name, Sprite sprite, float x, float y, float width, float height, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3((x - Width / 2) / Ppu, (Height / 2 - y) / Ppu, 0);
            go.transform.localScale = new Vector3(width / Ppu / sprite.bounds.size.x, height / Ppu / sprite.bounds.size.y, 1);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return go;
        }
        private static void Unit(Transform parent, string name, Sprite sprite, float x, float footY, float height, bool enemy)
        {
            float width = height * sprite.rect.width / sprite.rect.height;
            var go = Place(parent, name, sprite, x, footY - height / 2, width, height, 10 + Mathf.RoundToInt(footY / 10));
            go.GetComponent<SpriteRenderer>().flipX = enemy;
        }
        private static RectTransform Rect(Transform parent, string name, float x, float y, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }
        private static Image Picture(Transform parent, string name, Sprite sprite, float x, float y, float width, float height)
        {
            var image = Rect(parent, name, x, y, width, height).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }
        private static void Label(Transform parent, string name, string value, float x, float y, float width, float height, int size, Color color, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var label = Rect(parent, name, x, y, width, height).gameObject.AddComponent<Text>();
            label.font = _font;
            label.fontSize = size;
            label.text = value;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
        }
        private static void Bar(Transform parent, string name, float x, float y, float width, float height, float fill, bool red)
        {
            var root = Rect(parent, name, x, y, width, height);
            Picture(root, "Background", _barBack, width * .1f, height * .21f, width * .8f, height * .55f);
            var image = Picture(root, "Fill", red ? _barRed : _barGreen, width * .1f, height * .21f, width * .8f, height * .55f);
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillAmount = fill;
            Picture(root, "Gold Frame", _barFrame, 0, 0, width, height);
        }
        private static void Panel(Transform parent, string name, string text, float x, float y, float width, float height, int size)
        {
            Picture(parent, name + " Background", _barBack, x, y, width, height);
            Picture(parent, name + " Frame", _barFrame, x - 9, y - 8, width + 18, height + 16);
            Label(parent, name, text, x, y, width, height, size, Gold, TextAnchor.MiddleCenter);
        }
    }
}
