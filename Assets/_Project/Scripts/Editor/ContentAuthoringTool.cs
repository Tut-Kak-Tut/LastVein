using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using LastVein.Core;
using LastVein.Data;
using LastVein.Economy;
using LastVein.Workers;

namespace LastVein.EditorTools
{
    /// Populates the Root Mine's placeholder content (10 layers, ~30 minerals, upgrade configs)
    /// as real ScriptableObject assets. Re-running updates existing assets in place instead of duplicating them.
    public static class ContentAuthoringTool
    {
        const string LayersFolder = "Assets/_Project/ScriptableObjects/Layers";
        const string MineralsFolder = "Assets/_Project/ScriptableObjects/Minerals";
        const string LocationsFolder = "Assets/_Project/ScriptableObjects/Locations";
        const string UpgradesFolder = "Assets/_Project/ScriptableObjects/Upgrades";

        struct LayerDef
        {
            public int index;
            public string name;
            public Era era;
            public float baseHealth;
            public int blocksToAdvance;
            public Color palette;
            public string[] mineralNames;
            public string[] mineralFacts;
        }

        [MenuItem("LastVein/Generate Content Assets")]
        public static LocationData GenerateContentAssets()
        {
            EnsureFolder(LayersFolder);
            EnsureFolder(MineralsFolder);
            EnsureFolder(LocationsFolder);
            EnsureFolder(UpgradesFolder);

            LayerDef[] layerDefs = BuildLayerDefinitions();
            var layerAssets = new List<LayerData>();

            foreach (LayerDef def in layerDefs)
            {
                var mineralEntries = new List<LayerMineralEntry>();
                for (int i = 0; i < def.mineralNames.Length; i++)
                {
                    MineralData mineral = CreateOrLoadMineral(def.mineralNames[i], def.mineralFacts[i], def.era, def.palette);
                    mineralEntries.Add(new LayerMineralEntry { mineral = mineral, weight = 1f });
                }

                layerAssets.Add(CreateOrLoadLayer(def, mineralEntries.ToArray()));
            }

            LocationData location = CreateOrLoadLocation("RootMine", "Коренева шахта", layerAssets.ToArray());

            CreateOrLoadPickaxeConfig();
            CreateOrLoadGnomeConfig();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("LastVein: content assets generated/updated.");
            Selection.activeObject = location;
            return location;
        }

        static LayerDef[] BuildLayerDefinitions()
        {
            return new[]
            {
                MakeLayer(1, "Трав'яний шар", Era.Era1, 20f, 12, new Color(0.42f, 0.55f, 0.24f),
                    new[] { "Гумус", "Річковий кремінь", "Бурштин" },
                    new[]
                    {
                        "Родючий шар землі, перший, з яким стикається кожен новий шахтар.",
                        "Твердий кремінь, що трапляється у прибережних відкладеннях.",
                        "Скам'яніла смола прадавніх дерев, що зберігає бульбашки повітря мільйони років."
                    }),
                MakeLayer(2, "Глина та коріння", Era.Era1, 38f, 13, new Color(0.55f, 0.4f, 0.28f),
                    new[] { "Глиняний сланець", "Скам'яніле коріння", "Кварц" },
                    new[]
                    {
                        "Шаруватий м'який камінь, що легко розколюється уздовж пластів.",
                        "Дерев'яні корені, що з часом перетворилися на камінь.",
                        "Один з найпоширеніших мінералів земної кори."
                    }),
                MakeLayer(3, "Пісковик", Era.Era1, 75f, 14, new Color(0.76f, 0.66f, 0.42f),
                    new[] { "Пісковикова жила", "Слюда", "Халцедон" },
                    new[]
                    {
                        "Осадова порода зі спресованих піщинок.",
                        "Мінерал, що розшаровується на тонкі блискучі пластинки.",
                        "Приховано-кристалічний різновид кварцу."
                    }),
                MakeLayer(4, "Граніт", Era.Era2, 150f, 15, new Color(0.55f, 0.53f, 0.56f),
                    new[] { "Гранітний кристал", "Польовий шпат", "Чорна слюда" },
                    new[]
                    {
                        "Магматична порода, що застигала глибоко під землею.",
                        "Група мінералів, що складають більшість земної кори.",
                        "Темний блискучий мінерал групи слюд."
                    }),
                MakeLayer(5, "Вапнякові печери", Era.Era2, 300f, 16, new Color(0.75f, 0.74f, 0.68f),
                    new[] { "Кальцит", "Сталактитовий уламок", "Флюорит" },
                    new[]
                    {
                        "Найпоширеніший карбонатний мінерал, основа вапняку.",
                        "Утворення, що росте краплина за краплиною тисячоліттями.",
                        "Мінерал, що світиться під ультрафіолетом."
                    }),
                MakeLayer(6, "Мідна жила", Era.Era2, 600f, 17, new Color(0.72f, 0.45f, 0.2f),
                    new[] { "Мідна руда", "Малахіт", "Азурит" },
                    new[]
                    {
                        "Один з перших металів, освоєних людством.",
                        "Яскраво-зелений мідний мінерал з характерним візерунком.",
                        "Насичено-синій мінерал, супутник малахіту."
                    }),
                MakeLayer(7, "Залізна руда", Era.Era2, 1200f, 18, new Color(0.55f, 0.32f, 0.24f),
                    new[] { "Звичайне залізо", "Гематит", "Піритове залізо" },
                    new[]
                    {
                        "Основа промислової революції.",
                        "Залізна руда з характерним червонуватим порошком.",
                        "Мінерал, відомий як «золото дурнів» через свій блиск."
                    }),
                MakeLayer(8, "Кристалічна печера", Era.Era3, 2400f, 19, new Color(0.55f, 0.3f, 0.75f),
                    new[] { "Аметист", "Топаз", "Невідомий кристал" },
                    new[]
                    {
                        "Фіолетовий різновид кварцу, що цінувався ще в давнину.",
                        "Прозорий коштовний камінь різних відтінків.",
                        "Кристал незвичної структури, походження якого поки не встановлено."
                    }),
                MakeLayer(9, "Лавова зона", Era.Era3, 4800f, 20, new Color(0.75f, 0.25f, 0.1f),
                    new[] { "Обсидіан", "Вулканічне скло", "Сірчаний кристал" },
                    new[]
                    {
                        "Вулканічне скло, що утворюється при швидкому охолодженні лави.",
                        "Гостра, як лезо, застигла лава.",
                        "Яскраво-жовтий кристал з різким запахом."
                    }),
                MakeLayer(10, "Метеоритне ядро", Era.Era3, 9600f, 21, new Color(0.3f, 0.32f, 0.4f),
                    new[] { "Метеоритне залізо", "Зоряний пил", "Ядро-осколок" },
                    new[]
                    {
                        "Залізо позаземного походження, старше за саму Землю.",
                        "Дрібні частинки космічного пилу, що пережили падіння крізь атмосферу.",
                        "Уламок ядра давнього метеорита, щільний і надзвичайно важкий."
                    }),
            };
        }

        static LayerDef MakeLayer(int index, string name, Era era, float baseHealth, int blocksToAdvance,
            Color palette, string[] mineralNames, string[] mineralFacts)
        {
            return new LayerDef
            {
                index = index,
                name = name,
                era = era,
                baseHealth = baseHealth,
                blocksToAdvance = blocksToAdvance,
                palette = palette,
                mineralNames = mineralNames,
                mineralFacts = mineralFacts
            };
        }

        static MineralData CreateOrLoadMineral(string displayName, string fact, Era era, Color color)
        {
            string safeName = MakeSafeFileName(displayName);
            string path = $"{MineralsFolder}/Mineral_{safeName}.asset";

            MineralData mineral = AssetDatabase.LoadAssetAtPath<MineralData>(path);
            if (mineral == null)
            {
                mineral = ScriptableObject.CreateInstance<MineralData>();
                AssetDatabase.CreateAsset(mineral, path);
            }

            mineral.id = safeName;
            mineral.displayName = displayName;
            mineral.fact = fact;
            mineral.era = era;
            mineral.placeholderColor = color;

            EditorUtility.SetDirty(mineral);
            return mineral;
        }

        static LayerData CreateOrLoadLayer(LayerDef def, LayerMineralEntry[] entries)
        {
            string safeName = MakeSafeFileName(def.name);
            string path = $"{LayersFolder}/Layer{def.index:00}_{safeName}.asset";

            LayerData layer = AssetDatabase.LoadAssetAtPath<LayerData>(path);
            if (layer == null)
            {
                layer = ScriptableObject.CreateInstance<LayerData>();
                AssetDatabase.CreateAsset(layer, path);
            }

            layer.layerIndex = def.index;
            layer.displayName = def.name;
            layer.era = def.era;
            layer.baseHealth = def.baseHealth;
            layer.blocksToAdvance = def.blocksToAdvance;
            layer.paletteColor = def.palette;
            layer.minerals = entries;

            EditorUtility.SetDirty(layer);
            return layer;
        }

        static LocationData CreateOrLoadLocation(string id, string displayName, LayerData[] layers)
        {
            string path = $"{LocationsFolder}/Location_{id}.asset";

            LocationData location = AssetDatabase.LoadAssetAtPath<LocationData>(path);
            if (location == null)
            {
                location = ScriptableObject.CreateInstance<LocationData>();
                AssetDatabase.CreateAsset(location, path);
            }

            location.id = id;
            location.displayName = displayName;
            location.layers = layers;

            EditorUtility.SetDirty(location);
            return location;
        }

        static void CreateOrLoadPickaxeConfig()
        {
            string path = $"{UpgradesFolder}/PickaxeUpgradeConfig.asset";
            PickaxeUpgradeConfig config = AssetDatabase.LoadAssetAtPath<PickaxeUpgradeConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<PickaxeUpgradeConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            config.basePrice = 10;
            config.growthRate = 1.15f;
            config.clickPowerPerLevel = 1;

            EditorUtility.SetDirty(config);
        }

        static void CreateOrLoadGnomeConfig()
        {
            string path = $"{UpgradesFolder}/GnomeConfig.asset";
            GnomeConfig config = AssetDatabase.LoadAssetAtPath<GnomeConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GnomeConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            config.basePrice = 25;
            config.growthRate = 1.18f;
            config.incomePerGnome = 0.5;
            config.tickInterval = 0.1f;

            EditorUtility.SetDirty(config);
        }

        internal static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folderName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        static string MakeSafeFileName(string name)
        {
            string result = name.Replace("'", "").Replace(" ", "");
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                result = result.Replace(c.ToString(), "");
            }
            return result;
        }
    }
}
