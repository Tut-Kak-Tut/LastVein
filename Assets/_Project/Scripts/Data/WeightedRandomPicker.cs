using UnityEngine;

namespace LastVein.Data
{
    public static class WeightedRandomPicker
    {
        public static MineralData PickMineral(LayerMineralEntry[] entries)
        {
            float total = 0f;
            for (int i = 0; i < entries.Length; i++) total += entries[i].weight;

            float roll = Random.value * total;
            float cumulative = 0f;

            for (int i = 0; i < entries.Length; i++)
            {
                cumulative += entries[i].weight;
                if (roll <= cumulative) return entries[i].mineral;
            }

            return entries[entries.Length - 1].mineral;
        }
    }
}
