using UnityEngine;
using LastVein.Data;
using LastVein.Economy;
using LastVein.Workers;

namespace LastVein.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] LocationData currentLocation;
        [SerializeField] PickaxeUpgradeConfig pickaxeConfig;
        [SerializeField] GnomeConfig gnomeConfig;
        [SerializeField] BalanceConfig balanceConfig;

        [Header("Runtime — Economy (read-only, live during Play Mode)")]
        [ReadOnly] [SerializeField] double currentOre;
        [ReadOnly] [SerializeField] int currentClickPower;

        [Header("Runtime — Upgrades")]
        [ReadOnly] [SerializeField] int pickaxeLevel;
        [ReadOnly] [SerializeField] double nextPickaxePrice;
        [ReadOnly] [SerializeField] int gnomeCount;
        [ReadOnly] [SerializeField] double nextGnomePrice;
        [ReadOnly] [SerializeField] double incomePerSecond;

        [Header("Runtime — Mining")]
        [ReadOnly] [SerializeField] string currentLayerName;
        [ReadOnly] [SerializeField] float blockCurrentHealth;
        [ReadOnly] [SerializeField] float blockMaxHealth;

        bool initialized;
        OreWallet wallet;
        MiningState mining;
        UpgradesState upgrades;
        MineralAtlas atlas;

        // Views can call these from their own OnEnable, and Unity only guarantees Awake-before-OnEnable
        // within a single object, not across the whole scene - so a view's OnEnable can run before this
        // GameManager's Awake. Every accessor below self-initializes on first use to stay correct either way.
        public OreWallet Wallet { get { EnsureInitialized(); return wallet; } }
        public MiningState Mining { get { EnsureInitialized(); return mining; } }
        public UpgradesState Upgrades { get { EnsureInitialized(); return upgrades; } }
        public MineralAtlas Atlas { get { EnsureInitialized(); return atlas; } }

        void Awake() => EnsureInitialized();

        void EnsureInitialized()
        {
            if (initialized) return;
            initialized = true;

            wallet = new OreWallet();
            upgrades = new UpgradesState(pickaxeConfig, gnomeConfig);
            atlas = new MineralAtlas(currentLocation);
            mining = new MiningState(currentLocation, balanceConfig);

            mining.OnBlockBroken += HandleBlockBroken;
        }

        void OnDestroy()
        {
            if (mining != null) mining.OnBlockBroken -= HandleBlockBroken;
        }

        void Update()
        {
            Upgrades.Tick(Time.deltaTime, Wallet);
            RefreshRuntimeBalanceDisplay();
        }

        void RefreshRuntimeBalanceDisplay()
        {
            currentOre = Wallet.Ore;
            currentClickPower = Upgrades.ClickPower;
            pickaxeLevel = Upgrades.PickaxeLevel;
            nextPickaxePrice = Upgrades.GetNextPickaxePrice();
            gnomeCount = Upgrades.GnomeCount;
            nextGnomePrice = Upgrades.GetNextGnomePrice();
            incomePerSecond = Upgrades.IncomePerSecond;
            currentLayerName = Mining.CurrentLayer.displayName;
            blockCurrentHealth = Mining.CurrentHealth;
            blockMaxHealth = Mining.MaxHealth;
        }

        public void ApplyClick()
        {
            Mining.ApplyDamage(Upgrades.ClickPower);
        }

        public bool TryBuyPickaxeUpgrade() => Upgrades.TryBuyPickaxe(Wallet);

        public bool TryHireGnome() => Upgrades.TryHireGnome(Wallet);

        void HandleBlockBroken(double oreAwarded, MineralData mineral)
        {
            Wallet.Add(oreAwarded);
            Atlas.Discover(mineral);
        }
    }
}
