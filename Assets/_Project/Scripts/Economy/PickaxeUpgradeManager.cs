using System;
using UnityEngine;

namespace LastVein.Economy
{
    public class PickaxeUpgradeManager : MonoBehaviour
    {
        public int ClickPower { get; private set; } = 1;
        public int PickaxeLevel { get; private set; } = 0;
        public event Action OnPickaxeUpgraded;

        OreEconomy oreEconomy;
        PickaxeUpgradeConfig config;

        public void Init(OreEconomy economy, PickaxeUpgradeConfig cfg)
        {
            oreEconomy = economy;
            config = cfg;
            ClickPower = 1;
            PickaxeLevel = 0;
        }

        public double GetNextPrice()
        {
            return config.basePrice * Math.Pow(config.growthRate, PickaxeLevel);
        }

        public bool TryPurchase()
        {
            double price = GetNextPrice();
            if (!oreEconomy.TrySpend(price)) return false;

            PickaxeLevel++;
            ClickPower += config.clickPowerPerLevel;
            OnPickaxeUpgraded?.Invoke();
            return true;
        }
    }
}
