using UnityEngine;
using LastVein.Data;
using LastVein.Economy;
using LastVein.Workers;

namespace LastVein.Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] LocationData currentLocation;
        [SerializeField] PickaxeUpgradeConfig pickaxeConfig;
        [SerializeField] GnomeConfig gnomeConfig;

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
            mining = new MiningState(currentLocation);

            mining.OnBlockBroken += HandleBlockBroken;
        }

        void OnDestroy()
        {
            if (mining != null) mining.OnBlockBroken -= HandleBlockBroken;
        }

        void Update()
        {
            Upgrades.Tick(Time.deltaTime, Wallet);
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
