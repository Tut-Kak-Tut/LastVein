using System;
using LastVein.Economy;
using LastVein.Workers;

namespace LastVein.Core
{
    public class UpgradesState
    {
        readonly PickaxeUpgradeConfig pickaxeConfig;
        readonly GnomeConfig gnomeConfig;
        float gnomeTickTimer;

        public int ClickPower { get; private set; } = 1;
        public int PickaxeLevel { get; private set; }
        public int GnomeCount { get; private set; }
        public double IncomePerSecond => GnomeCount * gnomeConfig.incomePerGnome;

        public event Action OnPickaxeChanged;
        public event Action OnGnomeChanged;

        public UpgradesState(PickaxeUpgradeConfig pickaxeConfig, GnomeConfig gnomeConfig)
        {
            this.pickaxeConfig = pickaxeConfig;
            this.gnomeConfig = gnomeConfig;
        }

        public double GetNextPickaxePrice() => pickaxeConfig.basePrice * Math.Pow(pickaxeConfig.growthRate, PickaxeLevel);

        public bool TryBuyPickaxe(OreWallet wallet)
        {
            if (!wallet.TrySpend(GetNextPickaxePrice())) return false;

            PickaxeLevel++;
            ClickPower += pickaxeConfig.clickPowerPerLevel;
            OnPickaxeChanged?.Invoke();
            return true;
        }

        public double GetNextGnomePrice() => gnomeConfig.basePrice * Math.Pow(gnomeConfig.growthRate, GnomeCount);

        public bool TryHireGnome(OreWallet wallet)
        {
            if (!wallet.TrySpend(GetNextGnomePrice())) return false;

            GnomeCount++;
            OnGnomeChanged?.Invoke();
            return true;
        }

        public void Tick(float deltaTime, OreWallet wallet)
        {
            if (GnomeCount <= 0) return;

            gnomeTickTimer += deltaTime;
            while (gnomeTickTimer >= gnomeConfig.tickInterval)
            {
                gnomeTickTimer -= gnomeConfig.tickInterval;
                wallet.Add(IncomePerSecond * gnomeConfig.tickInterval);
            }
        }
    }
}
