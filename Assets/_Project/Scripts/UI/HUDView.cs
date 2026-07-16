using TMPro;
using UnityEngine;
using LastVein.Core;
using LastVein.Data;

namespace LastVein.UI
{
    public class HUDView : MonoBehaviour
    {
        [SerializeField] GameManager gameManager;
        [SerializeField] TMP_Text oreText;
        [SerializeField] TMP_Text powerGnomeSubstatText;
        [SerializeField] TMP_Text incomePerSecText;
        [SerializeField] TMP_Text layerNameText;

        void OnEnable()
        {
            gameManager.OreEconomy.OnOreChanged += HandleOreChanged;
            gameManager.GnomeManager.OnGnomeCountChanged += HandleStatsChanged;
            gameManager.PickaxeUpgradeManager.OnPickaxeUpgraded += HandleStatsChanged;
            gameManager.MiningManager.OnLayerChanged += HandleLayerChanged;

            HandleOreChanged(gameManager.OreEconomy.Ore);
            HandleStatsChanged();
            layerNameText.text = FormatLayerLabel(gameManager.MiningManager.CurrentLayer);
        }

        void OnDisable()
        {
            gameManager.OreEconomy.OnOreChanged -= HandleOreChanged;
            gameManager.GnomeManager.OnGnomeCountChanged -= HandleStatsChanged;
            gameManager.PickaxeUpgradeManager.OnPickaxeUpgraded -= HandleStatsChanged;
            gameManager.MiningManager.OnLayerChanged -= HandleLayerChanged;
        }

        void HandleOreChanged(double ore)
        {
            oreText.text = BigNumberFormatter.Format(ore);
        }

        void HandleStatsChanged()
        {
            powerGnomeSubstatText.text =
                $"{gameManager.PickaxeUpgradeManager.ClickPower} сили · {gameManager.GnomeManager.GnomeCount} гномів";
            incomePerSecText.text = BigNumberFormatter.Format(gameManager.GnomeManager.IncomePerSecond) + " руди / сек";
        }

        void HandleLayerChanged(LayerData layer, int layerIndex)
        {
            layerNameText.text = FormatLayerLabel(layer);
        }

        static string FormatLayerLabel(LayerData layer)
        {
            return $"Шар {layer.layerIndex} · {layer.displayName}";
        }
    }
}
