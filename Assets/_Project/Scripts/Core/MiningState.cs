using System;
using UnityEngine;
using LastVein.Data;
using LastVein.Economy;

namespace LastVein.Core
{
    public class MiningState
    {
        readonly LocationData location;
        readonly BalanceConfig balance;
        int currentLayerIndex;
        int blocksBrokenInLayer;

        public LayerData CurrentLayer => location.layers[currentLayerIndex];
        public float CurrentHealth { get; private set; }
        public float MaxHealth { get; private set; }

        public event Action<float, float> OnBlockProgress; // current, max
        public event Action<double, MineralData> OnBlockBroken; // ore awarded, mineral found
        public event Action<LayerData> OnLayerChanged;

        public MiningState(LocationData location, BalanceConfig balance)
        {
            this.location = location;
            this.balance = balance;
            SpawnNextBlock();
        }

        public void ApplyDamage(float power)
        {
            CurrentHealth -= power;

            if (CurrentHealth <= 0f)
            {
                BreakBlock();
            }
            else
            {
                OnBlockProgress?.Invoke(CurrentHealth, MaxHealth);
            }
        }

        void BreakBlock()
        {
            LayerData layer = CurrentLayer;
            double oreAwarded = Math.Ceiling(MaxHealth * balance.oreRewardMultiplier);
            MineralData mineral = PickMineral(layer.minerals);
            OnBlockBroken?.Invoke(oreAwarded, mineral);

            blocksBrokenInLayer++;

            if (blocksBrokenInLayer >= layer.blocksToAdvance && currentLayerIndex < location.layers.Length - 1)
            {
                currentLayerIndex++;
                blocksBrokenInLayer = 0;
                OnLayerChanged?.Invoke(CurrentLayer);
            }

            SpawnNextBlock();
        }

        void SpawnNextBlock()
        {
            MaxHealth = CurrentLayer.baseHealth * Mathf.Pow(balance.perBlockHealthGrowth, blocksBrokenInLayer);
            CurrentHealth = MaxHealth;
            OnBlockProgress?.Invoke(CurrentHealth, MaxHealth);
        }

        static MineralData PickMineral(LayerMineralEntry[] entries)
        {
            float total = 0f;
            for (int i = 0; i < entries.Length; i++) total += entries[i].weight;

            float roll = UnityEngine.Random.value * total;
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
