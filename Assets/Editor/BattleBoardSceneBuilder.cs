using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MakeMeHero.EditorTools
{
    [InitializeOnLoad]
    public static class BattleBoardSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/BattleBoardScene.unity";
        private const string SceneSignature = "Battle Board 2.5D Layout v3";
        private const float CellWidth = 2.35f;
        private const float CellHeight = 2.1f;
        private static Camera _camera;

        static BattleBoardSceneBuilder() { EditorApplication.delayCall += CreateInitialSceneIfNeeded; }

        [MenuItem("Make Me Hero/Build Battle Board Scene")]
        public static void RebuildFromMenu() { Build(true); }

        private static void CreateInitialSceneIfNeeded()
        {
            if (!File.Exists(ScenePath) || !File.ReadAllText(ScenePath).Contains(SceneSignature)) Build(false);
        }

        private static void Build(bool openWhenFinished)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            scene.name = "BattleBoardScene";
            _camera = CreateCamera();
            CreateGroundSprite("Battlefield Background", "Assets/Art/Board/battlefield-background.png", Vector2.zero, Vector3.one, -20);

            var root = new GameObject(SceneSignature).transform;
            for (var row = 0; row < 3; row++)
            for (var column = 0; column < 4; column++)
            {
                var tilePath = column == 0 && row == 1 ? "Assets/Art/Board/tile-city-outline.png" : column == 3 && row == 1 ? "Assets/Art/Board/tile-spawn-outline.png" : "Assets/Art/Board/tile-outline.png";
                var tile = CreateGroundSprite("Tile " + (row + 1) + "-" + (column + 1), tilePath, new Vector2(-3.55f + column * CellWidth, 1.6f - row * CellHeight), Vector3.one, -5);
                tile.transform.SetParent(root, true);
            }

            CreateUnit("Healer", "Assets/Art/Characters/healer-spritesheet.png", new Vector3(-3.55f, 1.6f, 0));
            CreateUnit("Soldier", "Assets/Art/Characters/soldier-spritesheet.png", new Vector3(-1.2f, -0.5f, 0));
            CreateUnit("Mage", "Assets/Art/Characters/mage-spritesheet.png", new Vector3(-3.55f, -0.5f, 0));
            CreateUnit("Archer", "Assets/Art/Characters/archer-spritesheet.png", new Vector3(-1.2f, 1.6f, 0));
            CreateUnit("Wolf", "Assets/Art/Characters/wolf-spritesheet.png", new Vector3(1.15f, -0.5f, 0));
            CreateUnit("Wolf Reinforcement", "Assets/Art/Characters/wolf-spritesheet.png", new Vector3(3.5f, -2.6f, 0));
            CreateBillboardSprite("Monster Portal", "Assets/Art/Board/vortex-idle-spritesheet.png", new Vector2(4.85f, 0.25f), Vector3.one * 1.45f, 1);

            CreateHud();

            EditorSceneManager.SaveScene(scene, ScenePath);
            if (openWhenFinished) EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single); else EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.Refresh();
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = false;
            camera.fieldOfView = 39f;
            camera.backgroundColor = new Color(0.035f, 0.055f, 0.09f);
            cameraObject.transform.position = new Vector3(0, 10.5f, -8.5f);
            cameraObject.transform.LookAt(new Vector3(0, 0, 0.15f));
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static GameObject CreateGroundSprite(string objectName, string assetPath, Vector2 boardPosition, Vector3 scale, int sortingOrder)
        {
            var sprite = FirstSprite(assetPath);
            if (sprite == null) throw new System.InvalidOperationException("Missing sprite: " + assetPath);
            var gameObject = new GameObject(objectName);
            gameObject.transform.position = new Vector3(boardPosition.x, 0, boardPosition.y);
            gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
            gameObject.transform.localScale = scale;
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return gameObject;
        }

        private static GameObject CreateBillboardSprite(string objectName, string assetPath, Vector2 boardPosition, Vector3 scale, int sortingOrder)
        {
            var sprite = FirstSprite(assetPath);
            if (sprite == null) throw new System.InvalidOperationException("Missing sprite: " + assetPath);
            var gameObject = new GameObject(objectName);
            gameObject.transform.position = new Vector3(boardPosition.x, 0.42f, boardPosition.y);
            gameObject.transform.rotation = Quaternion.LookRotation(_camera.transform.position - gameObject.transform.position);
            gameObject.transform.localScale = scale;
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return gameObject;
        }

        private static void CreateUnit(string objectName, string assetPath, Vector3 position)
        {
            var unit = CreateBillboardSprite(objectName, assetPath, new Vector2(position.x, position.y), Vector3.one * 0.68f, 2);
            unit.transform.SetParent(GameObject.Find(SceneSignature).transform, true);
        }

        private static void CreateHud()
        {
            var canvasObject = new GameObject("HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObject.AddComponent<GraphicRaycaster>();

            CreateHudText(canvas.transform, "KingdomTitle", "AURELLIA", 70, -58, 320, 60, TextAnchor.MiddleLeft, new Color(1f, 0.84f, 0.45f));
            CreateHudText(canvas.transform, "CityHealth", "CITY  30 / 30", 650, -58, 270, 60, TextAnchor.MiddleCenter, Color.white);
            CreateHudText(canvas.transform, "Gold", "GOLD  320", 1010, -58, 220, 60, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.3f));
            CreateHudText(canvas.transform, "Day", "DAY  3", 1320, -58, 180, 60, TextAnchor.MiddleCenter, Color.white);
            CreateHudText(canvas.transform, "FieldReserve", "FIELD  4 / 10", 70, 70, 260, 50, TextAnchor.MiddleLeft, new Color(0.55f, 0.85f, 1f));
            CreateHudText(canvas.transform, "Speed", "▶▶  x2", 1650, -230, 180, 54, TextAnchor.MiddleCenter, Color.white);
        }

        private static void CreateHudText(Transform parent, string objectName, string text, float x, float y, float width, float height, TextAnchor anchor, Color color)
        {
            var gameObject = new GameObject(objectName);
            gameObject.transform.SetParent(parent, false);
            var rect = gameObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
            var label = gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = text;
            label.fontSize = 30;
            label.fontStyle = FontStyle.Bold;
            label.alignment = anchor;
            label.color = color;
        }

        private static Sprite FirstSprite(string assetPath) { return AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().FirstOrDefault(); }
    }
}
