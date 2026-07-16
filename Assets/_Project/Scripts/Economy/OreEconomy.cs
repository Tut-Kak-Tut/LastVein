using System;
using UnityEngine;

namespace LastVein.Economy
{
    public class OreEconomy : MonoBehaviour
    {
        public double Ore { get; private set; }
        public event Action<double> OnOreChanged;

        public void Init()
        {
            Ore = 0;
            OnOreChanged?.Invoke(Ore);
        }

        public void AddOre(double amount)
        {
            if (amount <= 0) return;
            Ore += amount;
            OnOreChanged?.Invoke(Ore);
        }

        public bool TrySpend(double cost)
        {
            if (Ore < cost) return false;
            Ore -= cost;
            OnOreChanged?.Invoke(Ore);
            return true;
        }
    }
}
