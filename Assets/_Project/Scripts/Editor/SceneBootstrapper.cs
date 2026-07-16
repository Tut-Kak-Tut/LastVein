using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using LastVein.Core;
using LastVein.Data;
using LastVein.Economy;
using LastVein.Workers;
using LastVein.Atlas;
using LastVein.Mining;
using LastVein.UI;
using Object = UnityEngine.Object;

namespace LastVein.EditorTools
{
    /// One-click builder for the Main.unity MVP hierarchy (Canvas, GameManager, block, HUD, side panel).
    /// Safe to inspect/tweak by hand afterwards — this only saves time on the initial mechanical wiring.
    public static class SceneBootstrapper
    {
        // Stone/copper palette matching the HTML prototype reference.
        static readonly Color StoneDark = new Color(0.16471f, 0.14902f, 0.13333f);
        static readonly Color StoneMid = new Color(0.26667f, 0.23529f, 0.2f);
        static readonly Color StoneLight = new Color(0.41961f, 0.36471f, 0.30588f);
        static readonly Color Moss = new Color(0.48627f, 0.58039f, 0.45098f);
        static readonly Color Copper = new Color(0.78824f, 0.48235f, 0.29020f);
        static readonly Color CopperLight = new Color(0.87843f, 0.62745f, 0.41569f);
        static readonly Color Parchment = new Color(0.94118f, 0.91765f, 0.84706f);
        static readonly Color ParchmentDim = new Color(0.72157f, 0.69020f, 0.61176f);
        static readonly Color Crack = new Color(0.10196f, 0.09020f, 0.07843f);

        [MenuItem("LastVein/Setup Main Scene")]
        public static void SetupMainScene()
        {
            if (Object.FindObjectsByType<GameManager>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 0)
            {
                Debug.LogError("LastVein: a 'GameManager' GameObject already exists in the open scene (possibly " +
                                "inactive, left over from an interrupted run). Delete the existing LastVein setup " +
                                "before re-running this tool.");
                return;
            }

            LocationData location = ContentAuthoringTool.GenerateContentAssets();
            var pickaxeConfig = AssetDatabase.LoadAssetAtPath<PickaxeUpgradeConfig>(
                "Assets/_Project/ScriptableObjects/Upgrades/PickaxeUpgradeConfig.asset");
            var gnomeConfig = AssetDatabase.LoadAssetAtPath<GnomeConfig>(
                "Assets/_Project/ScriptableObjects/Upgrades/GnomeConfig.asset");
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/InputSystem_Actions.inputactions");

            if (inputActions == null)
            {
                Debug.LogError("LastVein: could not find Assets/InputSystem_Actions.inputactions");
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            // Build GameManager's whole component set while INACTIVE — Awake()/OnEnable() fire
            // immediately on AddComponent in the editor, so if the GameObject were active, GameManager's
            // Awake() would run before currentLocation/pickaxeConfig/gnomeConfig get wired below.
            var gameManagerGO = new GameObject("GameManager");
            gameManagerGO.SetActive(false);
            GameManager gm = gameManagerGO.AddComponent<GameManager>();
            gameManagerGO.AddComponent<OreEconomy>();
            PickaxeUpgradeManager pickaxeManagerComp = gameManagerGO.AddComponent<PickaxeUpgradeManager>();
            gameManagerGO.AddComponent<GnomeManager>();
            gameManagerGO.AddComponent<AtlasManager>();
            MiningManager miningManagerComp = gameManagerGO.AddComponent<MiningManager>();

            SetSerializedField(gm, "currentLocation", location);
            SetSerializedField(gm, "pickaxeConfig", pickaxeConfig);
            SetSerializedField(gm, "gnomeConfig", gnomeConfig);

            // Don't rely on SetActive(true) triggering Awake() at a reliably-ordered moment relative
            // to the Canvas subtree below — Unity's editor-time Awake()/OnEnable() scheduling isn't
            // guaranteed the same way Play Mode's is. Call the init logic directly and synchronously
            // instead, so every OreEconomy/AtlasManager/MiningManager property below is guaranteed
            // populated before any UI view gets a chance to read them.
            gm.Bootstrap();

            // Same reasoning for the whole Canvas subtree: build it fully wired while inactive,
            // then activate once at the very end so every view's OnEnable() sees a ready GameManager.
            var canvasGO = new GameObject("Canvas");
            canvasGO.SetActive(false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();
            RectTransform canvasRT = canvasGO.GetComponent<RectTransform>();

            // HorizontalLayoutGroup/VerticalLayoutGroup can't coexist on one GameObject
            // (LayoutGroup is [DisallowMultipleComponent]), so orientation switching uses two
            // separate wrapper containers and reparents the panels between them at runtime.
            RectTransform rootLayout = NewUI("RootLayout", canvasRT);
            Stretch(rootLayout);

            RectTransform horizontalWrapper = NewUI("HorizontalWrapper", rootLayout);
            Stretch(horizontalWrapper);
            var hLayout = horizontalWrapper.gameObject.AddComponent<HorizontalLayoutGroup>();
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = true;
            hLayout.childForceExpandHeight = true;
            hLayout.spacing = 12;
            hLayout.padding = new RectOffset(16, 16, 16, 16);

            RectTransform verticalWrapper = NewUI("VerticalWrapper", rootLayout);
            Stretch(verticalWrapper);
            var vLayout = verticalWrapper.gameObject.AddComponent<VerticalLayoutGroup>();
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = true;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = true;
            vLayout.spacing = 12;
            vLayout.padding = new RectOffset(16, 16, 16, 16);
            verticalWrapper.gameObject.SetActive(false); // horizontal (desktop) is the default orientation

            RectTransform gameAreaPanel = NewUI("GameAreaPanel", horizontalWrapper);
            var gameAreaLE = gameAreaPanel.gameObject.AddComponent<LayoutElement>();
            gameAreaLE.flexibleWidth = 2;
            gameAreaLE.flexibleHeight = 1;
            var gameAreaVLayout = gameAreaPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            gameAreaVLayout.childControlWidth = true;
            gameAreaVLayout.childControlHeight = true;
            gameAreaVLayout.childForceExpandWidth = true;
            gameAreaVLayout.spacing = 12;

            // --- Top HUD: resource-block (ore + "N сили · M гномів") | income-block ("X руди / сек") ---
            RectTransform topHud = NewUI("TopHUD", gameAreaPanel);
            var topHudLE = topHud.gameObject.AddComponent<LayoutElement>();
            topHudLE.preferredHeight = 100;
            var topHudLayout = topHud.gameObject.AddComponent<HorizontalLayoutGroup>();
            topHudLayout.childControlWidth = true;
            topHudLayout.childControlHeight = true;
            topHudLayout.childForceExpandWidth = true;
            topHudLayout.spacing = 16;

            RectTransform resourceBlock = NewUI("ResourceBlock", topHud);
            var resourceBlockLE = resourceBlock.gameObject.AddComponent<LayoutElement>();
            resourceBlockLE.flexibleWidth = 1;
            var resourceBlockLayout = resourceBlock.gameObject.AddComponent<VerticalLayoutGroup>();
            resourceBlockLayout.childAlignment = TextAnchor.UpperLeft;
            resourceBlockLayout.spacing = 4;
            resourceBlockLayout.childControlWidth = true;
            resourceBlockLayout.childControlHeight = true;
            resourceBlockLayout.childForceExpandWidth = true;

            RectTransform oreRow = NewUI("OreRow", resourceBlock);
            var oreRowLayout = oreRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            oreRowLayout.spacing = 8;
            oreRowLayout.childAlignment = TextAnchor.MiddleLeft;
            oreRowLayout.childControlWidth = true;
            oreRowLayout.childControlHeight = true;
            var oreRowLE = oreRow.gameObject.AddComponent<LayoutElement>();
            oreRowLE.preferredHeight = 44;

            Image oreIcon = NewImage("OreIcon", oreRow, Copper);
            oreIcon.raycastTarget = false;
            var oreIconLE = oreIcon.gameObject.AddComponent<LayoutElement>();
            oreIconLE.preferredWidth = 22;
            oreIconLE.preferredHeight = 22;
            TextMeshProUGUI oreText = NewText("OreText", oreRow, "0", 40, TextAlignmentOptions.Left, Parchment);
            var oreTextLE = oreText.gameObject.AddComponent<LayoutElement>();
            oreTextLE.preferredWidth = 220;

            TextMeshProUGUI substatText = NewText("SubstatText", resourceBlock, "1 сили · 0 гномів", 20,
                TextAlignmentOptions.Left, ParchmentDim);

            TextMeshProUGUI incomeText = NewText("IncomeText", topHud, "0 руди / сек", 22,
                TextAlignmentOptions.Right, Moss);
            var incomeTextLE = incomeText.gameObject.AddComponent<LayoutElement>();
            incomeTextLE.flexibleWidth = 1;

            // --- Title + layer label, centered above the block ---
            RectTransform titleBlock = NewUI("TitleBlock", gameAreaPanel);
            var titleBlockLE = titleBlock.gameObject.AddComponent<LayoutElement>();
            titleBlockLE.preferredHeight = 90;
            var titleBlockLayout = titleBlock.gameObject.AddComponent<VerticalLayoutGroup>();
            titleBlockLayout.childAlignment = TextAnchor.MiddleCenter;
            titleBlockLayout.spacing = 4;
            titleBlockLayout.childControlWidth = true;
            titleBlockLayout.childControlHeight = true;
            titleBlockLayout.childForceExpandWidth = true;

            NewText("TitleText", titleBlock, "Остання жила", 34, TextAlignmentOptions.Center, CopperLight);
            NewText("SubtitleText", titleBlock, "THE LAST VEIN — прототип", 16, TextAlignmentOptions.Center, ParchmentDim);

            RectTransform layerLabelRow = NewUI("LayerLabelRow", gameAreaPanel);
            var layerLabelLE = layerLabelRow.gameObject.AddComponent<LayoutElement>();
            layerLabelLE.preferredHeight = 36;
            TextMeshProUGUI layerNameText = NewTextInPlace(layerLabelRow, "Шар 1", 20,
                TextAlignmentOptions.Center, ParchmentDim);
            Stretch(layerNameText.rectTransform);

            // --- Block + progress bar ---
            RectTransform blockContainer = NewUI("BlockContainer", gameAreaPanel);
            var blockContainerLE = blockContainer.gameObject.AddComponent<LayoutElement>();
            blockContainerLE.flexibleHeight = 1;
            var blockContainerVLayout = blockContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            blockContainerVLayout.childAlignment = TextAnchor.MiddleCenter;
            blockContainerVLayout.spacing = 24;
            blockContainerVLayout.childControlWidth = false;
            blockContainerVLayout.childControlHeight = false;

            Image blockImage = NewImage("BlockImage", blockContainer, StoneMid);
            blockImage.raycastTarget = false;
            RectTransform blockRT = blockImage.rectTransform;
            blockRT.sizeDelta = new Vector2(400, 400);

            CreateOreFleck(blockImage.transform, new Vector2(-120, 80), 20f);
            CreateOreFleck(blockImage.transform, new Vector2(90, -60), 14f);
            CreateOreFleck(blockImage.transform, new Vector2(30, 50), 17f);

            var crackImages = new[]
            {
                CreateCrackImage(blockImage.transform, new Vector2(-10, 30), new Vector2(6, 120), 0f),
                CreateCrackImage(blockImage.transform, new Vector2(-30, 55), new Vector2(120, 6), 20f),
                CreateCrackImage(blockImage.transform, new Vector2(35, -55), new Vector2(110, 6), -15f),
                CreateCrackImage(blockImage.transform, new Vector2(80, 10), new Vector2(6, 140), 25f),
            };

            BlockClickHandler clickHandler = blockImage.gameObject.AddComponent<BlockClickHandler>();
            BlockView blockView = blockImage.gameObject.AddComponent<BlockView>();
            SetSerializedField(clickHandler, "inputActions", inputActions);
            SetSerializedField(clickHandler, "blockRect", blockRT);

            RectTransform progressBG = NewUI("ProgressBarBG", blockContainer);
            var progressBGImage = progressBG.gameObject.AddComponent<Image>();
            progressBGImage.color = StoneDark;
            // blockContainerVLayout has childControlWidth/Height=false (blockImage above sizes itself
            // the same way), so this sibling needs its size set directly rather than via LayoutElement.
            progressBG.sizeDelta = new Vector2(400, 28);

            RectTransform progressFillRT = NewUI("ProgressBarFill", progressBG);
            Stretch(progressFillRT);
            var progressFill = progressFillRT.gameObject.AddComponent<Image>();
            progressFill.color = Copper;
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillAmount = 0f;

            SetSerializedField(blockView, "miningManager", miningManagerComp);
            SetSerializedField(blockView, "pickaxeUpgradeManager", pickaxeManagerComp);
            SetSerializedField(blockView, "clickHandler", clickHandler);
            SetSerializedField(blockView, "blockImage", blockImage);
            SetSerializedField(blockView, "progressBarFill", progressFill);
            SetSerializedObjectArray(blockView, "crackImages", crackImages);
            SetSerializedField(blockView, "floatingTextParent", blockContainer);

            // --- Side panel: tabs, upgrades, atlas ---
            RectTransform sidePanel = NewUI("SidePanel", horizontalWrapper);
            var sidePanelLE = sidePanel.gameObject.AddComponent<LayoutElement>();
            sidePanelLE.flexibleWidth = 1;
            sidePanelLE.flexibleHeight = 1;
            var sidePanelImage = sidePanel.gameObject.AddComponent<Image>();
            sidePanelImage.color = StoneMid;
            var sidePanelVLayout = sidePanel.gameObject.AddComponent<VerticalLayoutGroup>();
            sidePanelVLayout.childControlWidth = true;
            sidePanelVLayout.childControlHeight = true;
            sidePanelVLayout.childForceExpandWidth = true;
            sidePanelVLayout.spacing = 12;
            sidePanelVLayout.padding = new RectOffset(8, 8, 8, 8);

            RectTransform tabBar = NewUI("TabBar", sidePanel);
            var tabBarLE = tabBar.gameObject.AddComponent<LayoutElement>();
            tabBarLE.preferredHeight = 100;
            var tabBarLayout = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            tabBarLayout.childControlWidth = true;
            tabBarLayout.childControlHeight = true;
            tabBarLayout.childForceExpandWidth = true;
            tabBarLayout.childForceExpandHeight = true;
            tabBarLayout.spacing = 4;

            Button upgradesTabButton = NewButton("UpgradesTabButton", tabBar, "Прокачка", StoneDark, ParchmentDim);
            Button atlasTabButton = NewButton("AtlasTabButton", tabBar, "Атлас", StoneDark, ParchmentDim);
            AddMinHeight(upgradesTabButton.gameObject, 100);
            AddMinHeight(atlasTabButton.gameObject, 100);
            AddHoverTint(upgradesTabButton, StoneDark, StoneLight);
            AddHoverTint(atlasTabButton, StoneDark, StoneLight);

            RectTransform upgradesPanel = NewUI("UpgradesPanel", sidePanel);
            var upgradesPanelLE = upgradesPanel.gameObject.AddComponent<LayoutElement>();
            upgradesPanelLE.flexibleHeight = 1;
            var upgradesVLayout = upgradesPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            upgradesVLayout.spacing = 16;
            upgradesVLayout.childControlWidth = true;
            upgradesVLayout.childControlHeight = true;
            upgradesVLayout.childForceExpandWidth = true;

            RectTransform pickaxeRow = NewUI("PickaxeRow", upgradesPanel);
            var pickaxeRowLE = pickaxeRow.gameObject.AddComponent<LayoutElement>();
            pickaxeRowLE.preferredHeight = 120;
            var pickaxeRowImage = pickaxeRow.gameObject.AddComponent<Image>();
            pickaxeRowImage.color = StoneDark;
            var pickaxeRowLayout = pickaxeRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            pickaxeRowLayout.spacing = 8;
            pickaxeRowLayout.padding = new RectOffset(10, 10, 10, 10);
            pickaxeRowLayout.childControlWidth = true;
            pickaxeRowLayout.childControlHeight = true;
            pickaxeRowLayout.childForceExpandWidth = true;

            TextMeshProUGUI pickaxeLevelText = NewText("LevelText", pickaxeRow, "Рівень 0", 24,
                TextAlignmentOptions.Left, Parchment);
            TextMeshProUGUI pickaxeNextPriceText = NewText("NextPriceText", pickaxeRow, "10", 24,
                TextAlignmentOptions.Center, CopperLight);
            Button pickaxeBuyButton = NewButton("BuyButton", pickaxeRow, "Купити", StoneMid, Parchment);
            AddMinHeight(pickaxeBuyButton.gameObject, 100);

            RectTransform gnomeRow = NewUI("GnomeRow", upgradesPanel);
            var gnomeRowLE = gnomeRow.gameObject.AddComponent<LayoutElement>();
            gnomeRowLE.preferredHeight = 120;
            var gnomeRowImage = gnomeRow.gameObject.AddComponent<Image>();
            gnomeRowImage.color = StoneDark;
            var gnomeRowLayout = gnomeRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            gnomeRowLayout.spacing = 8;
            gnomeRowLayout.padding = new RectOffset(10, 10, 10, 10);
            gnomeRowLayout.childControlWidth = true;
            gnomeRowLayout.childControlHeight = true;
            gnomeRowLayout.childForceExpandWidth = true;

            TextMeshProUGUI gnomeCountText = NewText("CountText", gnomeRow, "Гноми: 0", 24,
                TextAlignmentOptions.Left, Parchment);
            TextMeshProUGUI gnomeNextPriceText = NewText("NextPriceText", gnomeRow, "25", 24,
                TextAlignmentOptions.Center, CopperLight);
            Button gnomeHireButton = NewButton("HireButton", gnomeRow, "Найняти", StoneMid, Parchment);
            AddMinHeight(gnomeHireButton.gameObject, 100);

            UpgradesPanelView upgradesPanelView = upgradesPanel.gameObject.AddComponent<UpgradesPanelView>();
            SetSerializedField(upgradesPanelView, "gameManager", gm);
            SetSerializedField(upgradesPanelView, "pickaxeLevelText", pickaxeLevelText);
            SetSerializedField(upgradesPanelView, "pickaxeNextPriceText", pickaxeNextPriceText);
            SetSerializedField(upgradesPanelView, "pickaxeBuyButton", pickaxeBuyButton);
            SetSerializedField(upgradesPanelView, "gnomeCountText", gnomeCountText);
            SetSerializedField(upgradesPanelView, "gnomeNextPriceText", gnomeNextPriceText);
            SetSerializedField(upgradesPanelView, "gnomeHireButton", gnomeHireButton);

            RectTransform atlasPanel = NewUI("AtlasPanel", sidePanel);
            var atlasPanelLE = atlasPanel.gameObject.AddComponent<LayoutElement>();
            atlasPanelLE.flexibleHeight = 1;
            var atlasVLayout = atlasPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            atlasVLayout.spacing = 12;
            atlasVLayout.childControlWidth = true;
            atlasVLayout.childControlHeight = true;
            atlasVLayout.childForceExpandWidth = true;

            RectTransform eraTabBar = NewUI("EraTabBar", atlasPanel);
            var eraTabBarLE = eraTabBar.gameObject.AddComponent<LayoutElement>();
            eraTabBarLE.preferredHeight = 90;
            var eraTabBarLayout = eraTabBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            eraTabBarLayout.spacing = 6;
            eraTabBarLayout.childControlWidth = true;
            eraTabBarLayout.childControlHeight = true;
            eraTabBarLayout.childForceExpandWidth = true;
            eraTabBarLayout.childForceExpandHeight = true;

            Button era1Button = NewButton("Era1Button", eraTabBar, "Ера 1", StoneDark, ParchmentDim);
            Button era2Button = NewButton("Era2Button", eraTabBar, "Ера 2", StoneDark, ParchmentDim);
            Button era3Button = NewButton("Era3Button", eraTabBar, "Ера 3", StoneDark, ParchmentDim);
            AddHoverTint(era1Button, StoneDark, StoneLight);
            AddHoverTint(era2Button, StoneDark, StoneLight);
            AddHoverTint(era3Button, StoneDark, StoneLight);

            RectTransform scrollAreaRT = NewUI("MineralScrollArea", atlasPanel);
            var scrollAreaLE = scrollAreaRT.gameObject.AddComponent<LayoutElement>();
            scrollAreaLE.flexibleHeight = 1;
            var scrollBg = scrollAreaRT.gameObject.AddComponent<Image>();
            scrollBg.color = StoneDark;
            var scrollRect = scrollAreaRT.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            RectTransform viewport = NewUI("Viewport", scrollAreaRT);
            Stretch(viewport);
            var viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = Color.clear;
            var mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            RectTransform gridContent = NewUI("MineralGrid", viewport);
            gridContent.anchorMin = new Vector2(0, 1);
            gridContent.anchorMax = new Vector2(1, 1);
            gridContent.pivot = new Vector2(0.5f, 1f);
            gridContent.anchoredPosition = Vector2.zero;
            var gridLayout = gridContent.gameObject.AddComponent<GridLayoutGroup>();
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 4;
            gridLayout.cellSize = new Vector2(150, 150);
            gridLayout.spacing = new Vector2(8, 8);
            var contentFitter = gridContent.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport;
            scrollRect.content = gridContent;

            RectTransform progressTextRT = NewUI("ProgressText", atlasPanel);
            var progressTextLE = progressTextRT.gameObject.AddComponent<LayoutElement>();
            progressTextLE.preferredHeight = 50;
            TextMeshProUGUI progressText = NewTextInPlace(progressTextRT, "0 / 0", 28,
                TextAlignmentOptions.Center, Parchment);

            MineralCellView cellPrefab = CreateMineralCellPrefab();

            AtlasPanelView atlasPanelView = atlasPanel.gameObject.AddComponent<AtlasPanelView>();
            SetSerializedField(atlasPanelView, "gameManager", gm);
            SetSerializedField(atlasPanelView, "gridParent", gridContent);
            SetSerializedField(atlasPanelView, "cellPrefab", cellPrefab);
            SetSerializedField(atlasPanelView, "progressText", progressText);
            SetSerializedField(atlasPanelView, "era1TabButton", era1Button);
            SetSerializedField(atlasPanelView, "era2TabButton", era2Button);
            SetSerializedField(atlasPanelView, "era3TabButton", era3Button);

            atlasPanel.gameObject.SetActive(false);

            SidePanelController sidePanelController = sidePanel.gameObject.AddComponent<SidePanelController>();
            SetSerializedField(sidePanelController, "upgradesPanel", upgradesPanel.gameObject);
            SetSerializedField(sidePanelController, "atlasPanel", atlasPanel.gameObject);

            upgradesTabButton.onClick.AddListener(sidePanelController.ShowUpgrades);
            atlasTabButton.onClick.AddListener(sidePanelController.ShowAtlas);

            HUDView hudView = topHud.gameObject.AddComponent<HUDView>();
            SetSerializedField(hudView, "gameManager", gm);
            SetSerializedField(hudView, "oreText", oreText);
            SetSerializedField(hudView, "powerGnomeSubstatText", substatText);
            SetSerializedField(hudView, "incomePerSecText", incomeText);
            SetSerializedField(hudView, "layerNameText", layerNameText);

            ResponsiveLayoutController responsive = rootLayout.gameObject.AddComponent<ResponsiveLayoutController>();
            SetSerializedField(responsive, "horizontalWrapper", horizontalWrapper);
            SetSerializedField(responsive, "verticalWrapper", verticalWrapper);
            SetSerializedField(responsive, "gameAreaPanel", gameAreaPanel);
            SetSerializedField(responsive, "sidePanel", sidePanel);

            // --- Toast, anchored to the top of the screen ---
            RectTransform toastRT = NewUI("ToastPopup", canvasRT);
            toastRT.anchorMin = new Vector2(0.5f, 1f);
            toastRT.anchorMax = new Vector2(0.5f, 1f);
            toastRT.pivot = new Vector2(0.5f, 1f);
            toastRT.anchoredPosition = new Vector2(0, -20);
            toastRT.sizeDelta = new Vector2(700, 140);
            var toastBg = toastRT.gameObject.AddComponent<Image>();
            toastBg.color = Copper;
            CanvasGroup toastCanvasGroup = toastRT.gameObject.AddComponent<CanvasGroup>();

            RectTransform toastContentRT = NewUI("Content", toastRT);
            Stretch(toastContentRT);
            var toastVLayout = toastContentRT.gameObject.AddComponent<VerticalLayoutGroup>();
            toastVLayout.padding = new RectOffset(16, 16, 12, 12);
            toastVLayout.spacing = 6;
            toastVLayout.childControlWidth = true;
            toastVLayout.childControlHeight = true;
            toastVLayout.childForceExpandWidth = true;

            TextMeshProUGUI toastNameText = NewText("NameText", toastContentRT, "", 28,
                TextAlignmentOptions.Left, StoneDark);
            TextMeshProUGUI toastFactText = NewText("FactText", toastContentRT, "", 20,
                TextAlignmentOptions.Left, StoneDark);

            DiscoveryToastView toastView = toastRT.gameObject.AddComponent<DiscoveryToastView>();
            SetSerializedField(toastView, "gameManager", gm);
            SetSerializedField(toastView, "canvasGroup", toastCanvasGroup);
            SetSerializedField(toastView, "rectTransform", toastRT);
            SetSerializedField(toastView, "nameText", toastNameText);
            SetSerializedField(toastView, "factText", toastFactText);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            // Activate GameManager first so its Awake() (and MiningManager.SpawnNextBlock) completes
            // before the Canvas subtree's views enable and read from it.
            gameManagerGO.SetActive(true);
            canvasGO.SetActive(true);

            Debug.Log("LastVein: Main scene setup complete. Enter Play mode to test.");
        }

        static MineralCellView CreateMineralCellPrefab()
        {
            const string folder = "Assets/_Project/Prefabs/UI";
            ContentAuthoringTool.EnsureFolder(folder);
            const string path = folder + "/MineralCell.prefab";

            // Always rebuild rather than reuse-if-exists: this is generated placeholder content
            // (not hand-tweaked art), and a stale prefab from a previous MineralCellView field
            // layout would otherwise silently keep broken/null serialized references.
            AssetDatabase.DeleteAsset(path);

            var root = new GameObject("MineralCell", typeof(RectTransform));
            RectTransform rootRT = root.GetComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(150, 150);

            Image background = root.AddComponent<Image>();
            background.color = StoneDark;

            TextMeshProUGUI glyphText = NewText("GlyphText", rootRT, "?", 36, TextAlignmentOptions.Center, ParchmentDim);
            Stretch(glyphText.rectTransform);

            RectTransform tooltipRT = NewUI("Tooltip", rootRT);
            tooltipRT.anchorMin = new Vector2(0.5f, 1f);
            tooltipRT.anchorMax = new Vector2(0.5f, 1f);
            tooltipRT.pivot = new Vector2(0.5f, 0f);
            tooltipRT.anchoredPosition = new Vector2(0, 6);
            tooltipRT.sizeDelta = new Vector2(170, 44);
            Image tooltipBg = tooltipRT.gameObject.AddComponent<Image>();
            tooltipBg.color = StoneDark;
            TextMeshProUGUI tooltipText = NewText("TooltipText", tooltipRT, "???", 16,
                TextAlignmentOptions.Center, Parchment);
            Stretch(tooltipText.rectTransform);
            tooltipRT.gameObject.SetActive(false);

            MineralCellView cellView = root.AddComponent<MineralCellView>();
            SetSerializedField(cellView, "backgroundImage", background);
            SetSerializedField(cellView, "glyphText", glyphText);
            SetSerializedField(cellView, "tooltip", tooltipRT.gameObject);
            SetSerializedField(cellView, "tooltipText", tooltipText);

            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            return prefabAsset.GetComponent<MineralCellView>();
        }

        static void CreateOreFleck(Transform parent, Vector2 position, float size)
        {
            Image fleck = NewImage("OreFleck", parent, Copper);
            fleck.raycastTarget = false;
            RectTransform rt = fleck.rectTransform;
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(size, size);
        }

        static Image CreateCrackImage(Transform parent, Vector2 position, Vector2 size, float rotation)
        {
            Image crack = NewImage("Crack", parent, Crack);
            crack.raycastTarget = false;
            RectTransform rt = crack.rectTransform;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            rt.localRotation = Quaternion.Euler(0, 0, rotation);
            Color c = crack.color;
            c.a = 0f;
            crack.color = c;
            return crack;
        }

        static RectTransform NewUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            return rt;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static Image NewImage(string name, Transform parent, Color color)
        {
            RectTransform rt = NewUI(name, parent);
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        static TextMeshProUGUI NewText(string name, Transform parent, string text, float fontSize,
            TextAlignmentOptions align, Color color)
        {
            RectTransform rt = NewUI(name, parent);
            return NewTextInPlace(rt, text, fontSize, align, color);
        }

        static TextMeshProUGUI NewTextInPlace(RectTransform rt, string text, float fontSize,
            TextAlignmentOptions align, Color color)
        {
            TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        static Button NewButton(string name, Transform parent, string label, Color bgColor, Color textColor)
        {
            Image bgImage = NewImage(name, parent, bgColor);
            Button btn = bgImage.gameObject.AddComponent<Button>();
            btn.targetGraphic = bgImage;

            TextMeshProUGUI labelText = NewText("Label", bgImage.transform, label, 24,
                TextAlignmentOptions.Center, textColor);
            Stretch(labelText.rectTransform);

            return btn;
        }

        static void AddHoverTint(Button button, Color normal, Color hover)
        {
            UIHoverTint hoverTint = button.gameObject.AddComponent<UIHoverTint>();
            hoverTint.SetColors(button.targetGraphic, normal, hover);
        }

        static void AddMinHeight(GameObject go, float height)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
        }

        static void SetSerializedField(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError($"LastVein SceneBootstrapper: field '{fieldName}' not found on {target.GetType().Name}");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }

        static void SetSerializedObjectArray(Object target, string fieldName, Object[] values)
        {
            var so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError($"LastVein SceneBootstrapper: array field '{fieldName}' not found on {target.GetType().Name}");
                return;
            }
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            so.ApplyModifiedProperties();
        }
    }
}
