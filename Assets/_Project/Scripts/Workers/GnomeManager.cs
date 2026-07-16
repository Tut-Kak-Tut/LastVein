using System;
using UnityEngine;
using LastVein.Economy;

namespace LastVein.Workers
{
    public class GnomeManager : MonoBehaviour
    {
        public int GnomeCount { get; private set; }
        public event Action OnGnomeCountChanged;
        public double IncomePerSecond => GnomeCount * config.incomePerGnome;

        OreEconomy oreEconomy;
        GnomeConfig config;
        float tickTimer;

        public void Init(OreEconomy economy, GnomeConfig cfg)
        {
            oreEconomy = economy;
            config = cfg;
            GnomeCount = 0;
            tickTimer = 0f;
        }

        public double GetNextPrice()
        {
            return config.basePrice * Math.Pow(config.growthRate, GnomeCount);
        }

        public bool TryHire()
        {
            double price = GetNextPrice();
            if (!oreEconomy.TrySpend(price)) return false;

            GnomeCount++;
            OnGnomeCountChanged?.Invoke();
            return true;
        }

        void Update()
        {
            if (GnomeCount <= 0) return;

            tickTimer += Time.deltaTime;
            while (tickTimer >= config.tickInterval)
            {
                tickTimer -= config.tickInterval;
                oreEconomy.AddOre(IncomePerSecond * config.tickInterval);
            }
        }
    }
}
