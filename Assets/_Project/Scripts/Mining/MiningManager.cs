using System;
using UnityEngine;
using LastVein.Data;
using LastVein.Economy;
using LastVein.Atlas;

namespace LastVein.Mining
{
    public class MiningManager : MonoBehaviour
    {
        [SerializeField] float perBlockHealthGrowth = 1.10f;
        [SerializeField] float oreRewardMultiplier = 0.4f;

        LocationData location;
        OreEconomy oreEconomy;
        PickaxeUpgradeManager pickaxeUpgradeManager;
        AtlasManager atlasManager;

        int currentLayerIndex; // 0-based index into location.layers
        int blocksBrokenInLayer;

        public BlockRuntimeState CurrentBlock { get; private set; }
        public LayerData CurrentLayer => location.layers[currentLayerIndex];

        public event Action<BlockRuntimeState> OnBlockSpawned;
        public event Action<float, float> OnBlockDamaged; // current, max
        public event Action<double, MineralData> OnBlockBroken; // oreAwarded, mineral found
        public event Action<LayerData, int> OnLayerChanged; // layer, layerIndex (1-based)

        public void Init(LocationData loc, OreEconomy economy, PickaxeUpgradeManager pickaxe, AtlasManager atlas)
        {
            location = loc;
            oreEconomy = economy;
            pickaxeUpgradeManager = pickaxe;
            atlasManager = atlas;
            currentLayerIndex = 0;
            blocksBrokenInLayer = 0;
        }

        public void SpawnNextBlock()
        {
            LayerData layer = CurrentLayer;
            float maxHealth = layer.baseHealth * Mathf.Pow(perBlockHealthGrowth, blocksBrokenInLayer);

            CurrentBlock = new BlockRuntimeState
            {
                layerIndex = layer.layerIndex,
                blockIndexInLayer = blocksBrokenInLayer,
                maxHealth = maxHealth,
                currentHealth = maxHealth
            };

            OnBlockSpawned?.Invoke(CurrentBlock);
        }

        public void ApplyClick()
        {
            if (CurrentBlock == null) return;

            CurrentBlock.currentHealth -= pickaxeUpgradeManager.ClickPower;

            if (CurrentBlock.currentHealth <= 0f)
            {
                BreakBlock();
            }
            else
            {
                OnBlockDamaged?.Invoke(CurrentBlock.currentHealth, CurrentBlock.maxHealth);
            }
        }

        void BreakBlock()
        {
            LayerData layer = CurrentLayer;
            double oreAwarded = Math.Ceiling(CurrentBlock.maxHealth * oreRewardMultiplier);
            oreEconomy.AddOre(oreAwarded);

            MineralData mineral = WeightedRandomPicker.PickMineral(layer.minerals);
            atlasManager.Discover(mineral);

            OnBlockBroken?.Invoke(oreAwarded, mineral);

            blocksBrokenInLayer++;

            if (blocksBrokenInLayer >= layer.blocksToAdvance && currentLayerIndex < location.layers.Length - 1)
            {
                AdvanceLayer();
            }
            else
            {
                SpawnNextBlock();
            }
        }

        void AdvanceLayer()
        {
            currentLayerIndex++;
            blocksBrokenInLayer = 0;
            OnLayerChanged?.Invoke(CurrentLayer, CurrentLayer.layerIndex);
            SpawnNextBlock();
        }
    }
}
