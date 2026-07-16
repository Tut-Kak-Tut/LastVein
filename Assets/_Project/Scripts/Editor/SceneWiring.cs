using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using LastVein.Core;
using LastVein.Data;
using LastVein.Economy;
using LastVein.Workers;
using LastVein.UI;

namespace LastVein.EditorTools
{
    // Wires the manager/view components from the "new code" pass onto the already-built
    // Main.unity hierarchy. The scene's visual layout was authored ahead of the scripts,
    // so this finds objects by their existing names/paths rather than building anything.
    public static class SceneWiring
    {
        const string LocationPath = "Assets/_Project/ScriptableObjects/Locations/Location_RootMine.asset";
        const string PickaxeConfigPath = "Assets/_Project/ScriptableObjects/Upgrades/PickaxeUpgradeConfig.asset";
        const string GnomeConfigPath = "Assets/_Project/ScriptableObjects/Upgrades/GnomeConfig.asset";
        const string MineralCellPrefabPath = "Assets/_Project/Prefabs/UI/MineralCell.prefab";

        [MenuItem("LastVein/Wire Main Scene")]
        public static void WireMainScene()
        {
            GameObject canvasGO = GameObject.Find("Canvas");
            GameObject gameManagerGO = GameObject.Find("GameManager");

            if (canvasGO == null || gameManagerGO == null)
            {
                Debug.LogError("SceneWiring: could not find 'Canvas' or 'GameManager' in the open scene. Open Assets/_Project/Scenes/Main.unity first.");
                return;
            }

            Transform canvas = canvasGO.transform;

            EnsureEra3Section(canvas);

            GameManager gameManager = WireGameManager(gameManagerGO);
            WireBlockView(canvas, gameManager);
            WireHUDView(canvas, gameManager);
            WireUpgradesPanelView(canvas, gameManager);
            WireAtlasPanelView(canvas, gameManager);
            WireTabSwitcher(canvas);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("SceneWiring: Main scene wired.");
        }

        static GameManager WireGameManager(GameObject gameManagerGO)
        {
            var gm = gameManagerGO.GetComponent<GameManager>();
            if (gm == null) gm = Undo.AddComponent<GameManager>(gameManagerGO);

            var so = new SerializedObject(gm);
            so.FindProperty("currentLocation").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LocationData>(LocationPath);
            so.FindProperty("pickaxeConfig").objectReferenceValue = AssetDatabase.LoadAssetAtPath<PickaxeUpgradeConfig>(PickaxeConfigPath);
            so.FindProperty("gnomeConfig").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GnomeConfig>(GnomeConfigPath);
            so.ApplyModifiedPropertiesWithoutUndo();

            return gm;
        }

        static void WireBlockView(Transform canvas, GameManager gameManager)
        {
            Transform blockImage = Require(canvas, "PlayerUi/LeftBar/BlockContainer/BlockImage");
            Transform progressFill = Require(canvas, "PlayerUi/LeftBar/BlockContainer/ProgressBarBG/ProgressBarFill");

            var view = blockImage.GetComponent<BlockView>();
            if (view == null) view = Undo.AddComponent<BlockView>(blockImage.gameObject);

            // Authored with raycasting off (it started as a purely visual placeholder) - clicks would
            // otherwise pass straight through and never reach BlockView's IPointerClickHandler.
            Image blockImageGraphic = blockImage.GetComponent<Image>();
            if (blockImageGraphic != null) blockImageGraphic.raycastTarget = true;

            var cracks = new List<Image>();
            foreach (Transform child in blockImage)
            {
                if (child.name == "Crack") cracks.Add(child.GetComponent<Image>());
            }

            // Image.Type.Filled is silently ignored by Unity when no sprite is assigned - it falls back
            // to drawing the full rect regardless of fillAmount, which is why the bar looked static.
            Image progressBarFillImage = progressFill.GetComponent<Image>();
            if (progressBarFillImage.sprite == null)
                progressBarFillImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            var so = new SerializedObject(view);
            so.FindProperty("gameManager").objectReferenceValue = gameManager;
            so.FindProperty("progressBarFill").objectReferenceValue = progressBarFillImage;

            var cracksProp = so.FindProperty("crackImages");
            cracksProp.arraySize = cracks.Count;
            for (int i = 0; i < cracks.Count; i++)
                cracksProp.GetArrayElementAtIndex(i).objectReferenceValue = cracks[i];

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WireHUDView(Transform canvas, GameManager gameManager)
        {
            Transform topHud = Require(canvas, "PlayerUi/LeftBar/TopHUD");
            var view = topHud.GetComponent<HUDView>();
            if (view == null) view = Undo.AddComponent<HUDView>(topHud.gameObject);

            var so = new SerializedObject(view);
            so.FindProperty("gameManager").objectReferenceValue = gameManager;
            so.FindProperty("oreText").objectReferenceValue = Require(canvas, "PlayerUi/LeftBar/TopHUD/ResourceBlock/OreRow/OreText").GetComponent<TMP_Text>();
            so.FindProperty("substatText").objectReferenceValue = Require(canvas, "PlayerUi/LeftBar/TopHUD/ResourceBlock/SubstatText").GetComponent<TMP_Text>();
            so.FindProperty("incomeText").objectReferenceValue = Require(canvas, "PlayerUi/LeftBar/TopHUD/IncomeText").GetComponent<TMP_Text>();
            so.FindProperty("layerText").objectReferenceValue = Require(canvas, "PlayerUi/LeftBar/LayerLabelRow").GetComponent<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WireUpgradesPanelView(Transform canvas, GameManager gameManager)
        {
            Transform panel = Require(canvas, "PlayerUi/RightBar/UpgradePanel");
            var view = panel.GetComponent<UpgradesPanelView>();
            if (view == null) view = Undo.AddComponent<UpgradesPanelView>(panel.gameObject);

            var so = new SerializedObject(view);
            so.FindProperty("gameManager").objectReferenceValue = gameManager;
            so.FindProperty("pickaxeLevelText").objectReferenceValue = Require(panel, "PickaxeRow/LevelText").GetComponent<TMP_Text>();
            so.FindProperty("pickaxeNextPriceText").objectReferenceValue = Require(panel, "PickaxeRow/NextPriceText").GetComponent<TMP_Text>();
            so.FindProperty("pickaxeBuyButton").objectReferenceValue = Require(panel, "PickaxeRow/BuyButton").GetComponent<Button>();
            so.FindProperty("gnomeCountText").objectReferenceValue = Require(panel, "GnomeUpgrade/CountText").GetComponent<TMP_Text>();
            so.FindProperty("gnomeNextPriceText").objectReferenceValue = Require(panel, "GnomeUpgrade/NextPriceText").GetComponent<TMP_Text>();
            so.FindProperty("gnomeHireButton").objectReferenceValue = Require(panel, "GnomeUpgrade/HireButton").GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WireAtlasPanelView(Transform canvas, GameManager gameManager)
        {
            Transform panel = Require(canvas, "PlayerUi/RightBar/AtlasPanel");
            var view = panel.GetComponent<AtlasPanelView>();
            if (view == null) view = Undo.AddComponent<AtlasPanelView>(panel.gameObject);

            var cellPrefab = AssetDatabase.LoadAssetAtPath<MineralCellView>(MineralCellPrefabPath);

            var so = new SerializedObject(view);
            so.FindProperty("gameManager").objectReferenceValue = gameManager;
            so.FindProperty("cellPrefab").objectReferenceValue = cellPrefab;

            string[] eraNames = { "Era1", "Era2", "Era3" };
            var sectionsProp = so.FindProperty("sections");
            sectionsProp.arraySize = eraNames.Length;

            for (int i = 0; i < eraNames.Length; i++)
            {
                Transform eraTransform = Require(panel, eraNames[i]);
                var element = sectionsProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("era").enumValueIndex = i;
                element.FindPropertyRelative("mineralsContainer").objectReferenceValue = Require(eraTransform, "Minerals");
                element.FindPropertyRelative("progressText").objectReferenceValue = Require(eraTransform, "NameEra/Label").GetComponent<TMP_Text>();
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WireTabSwitcher(Transform canvas)
        {
            Transform rightBar = Require(canvas, "PlayerUi/RightBar");
            var switcher = rightBar.GetComponent<TabSwitcher>();
            if (switcher == null) switcher = Undo.AddComponent<TabSwitcher>(rightBar.gameObject);

            GameObject upgradePanel = Require(rightBar, "UpgradePanel").gameObject;
            GameObject atlasPanel = Require(rightBar, "AtlasPanel").gameObject;

            Button upgradesTabButton = EnsureButton(Require(rightBar, "Nav/Upgrade").gameObject);
            Button atlasTabButton = EnsureButton(Require(rightBar, "Nav/Atlas").gameObject);

            var so = new SerializedObject(switcher);
            so.FindProperty("upgradesPanel").objectReferenceValue = upgradePanel;
            so.FindProperty("atlasPanel").objectReferenceValue = atlasPanel;
            so.FindProperty("upgradesTabButton").objectReferenceValue = upgradesTabButton;
            so.FindProperty("atlasTabButton").objectReferenceValue = atlasTabButton;
            so.ApplyModifiedPropertiesWithoutUndo();

            upgradePanel.SetActive(true);
            atlasPanel.SetActive(false);
        }

        static Button EnsureButton(GameObject go)
        {
            var button = go.GetComponent<Button>();
            if (button == null)
            {
                button = Undo.AddComponent<Button>(go);
                Graphic graphic = go.GetComponent<Graphic>();
                if (graphic != null) button.targetGraphic = graphic;
            }
            return button;
        }

        static void EnsureEra3Section(Transform canvas)
        {
            Transform atlasPanel = Require(canvas, "PlayerUi/RightBar/AtlasPanel");
            if (atlasPanel.Find("Era3") != null) return;

            Transform era2 = Require(atlasPanel, "Era2");
            GameObject era3 = Object.Instantiate(era2.gameObject, atlasPanel);
            era3.name = "Era3";

            Transform minerals = Require(era3.transform, "Minerals");
            for (int i = minerals.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(minerals.GetChild(i).gameObject);

            TMP_Text label = Require(era3.transform, "NameEra/Label").GetComponent<TMP_Text>();
            if (label != null) label.text = "Ера 3";
        }

        static Transform Require(Transform root, string path)
        {
            Transform found = root.Find(path);
            if (found == null)
                Debug.LogError($"SceneWiring: could not find '{path}' under '{root.name}'.");
            return found;
        }
    }
}
