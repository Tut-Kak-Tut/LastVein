using UnityEngine;
using LastVein.Data;
using LastVein.Economy;
using LastVein.Workers;
using LastVein.Atlas;
using LastVein.Mining;

namespace LastVein.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] LocationData currentLocation;
        [SerializeField] PickaxeUpgradeConfig pickaxeConfig;
        [SerializeField] GnomeConfig gnomeConfig;

        public OreEconomy OreEconomy { get; private set; }
        public PickaxeUpgradeManager PickaxeUpgradeManager { get; private set; }
        public GnomeManager GnomeManager { get; private set; }
        public AtlasManager AtlasManager { get; private set; }
        public MiningManager MiningManager { get; private set; }

        bool bootstrapped;

        void Awake() => Bootstrap();

        // Called automatically by Awake() in normal (Play Mode / runtime) use. Also called directly
        // and synchronously by SceneBootstrapper right after wiring currentLocation/pickaxeConfig/gnomeConfig —
        // Unity's Awake()/OnEnable() timing during editor-time procedural construction isn't reliably
        // ordered, so the editor tool can't just count on SetActive(true) triggering this at the right
        // moment. Guarded so it only ever runs once regardless of which path calls it first.
        public void Bootstrap()
        {
            if (bootstrapped) return;
            bootstrapped = true;

            Instance = this;

            OreEconomy = GetComponent<OreEconomy>();
            PickaxeUpgradeManager = GetComponent<PickaxeUpgradeManager>();
            GnomeManager = GetComponent<GnomeManager>();
            AtlasManager = GetComponent<AtlasManager>();
            MiningManager = GetComponent<MiningManager>();

            OreEconomy.Init();
            PickaxeUpgradeManager.Init(OreEconomy, pickaxeConfig);
            GnomeManager.Init(OreEconomy, gnomeConfig);
            AtlasManager.Init(currentLocation);
            MiningManager.Init(currentLocation, OreEconomy, PickaxeUpgradeManager, AtlasManager);

            MiningManager.SpawnNextBlock();
        }
    }
}
