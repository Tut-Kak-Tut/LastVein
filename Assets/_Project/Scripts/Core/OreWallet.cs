using System;

namespace LastVein.Core
{
    public class OreWallet
    {
        public double Ore { get; private set; }
        public event Action<double> OnChanged;

        public void Add(double amount)
        {
            if (amount <= 0) return;
            Ore += amount;
            OnChanged?.Invoke(Ore);
        }

        public bool TrySpend(double cost)
        {
            if (Ore < cost) return false;
            Ore -= cost;
            OnChanged?.Invoke(Ore);
            return true;
        }
    }
}
