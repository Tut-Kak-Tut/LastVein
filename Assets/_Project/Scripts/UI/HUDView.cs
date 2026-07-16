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
        [SerializeField] TMP_Text substatText;
        [SerializeField] TMP_Text incomeText;
        [SerializeField] TMP_Text layerText;

        void OnEnable()
        {
            gameManager.Wallet.OnChanged += HandleOreChanged;
            gameManager.Upgrades.OnPickaxeChanged += HandleStatsChanged;
            gameManager.Upgrades.OnGnomeChanged += HandleStatsChanged;
            gameManager.Mining.OnLayerChanged += HandleLayerChanged;

            HandleOreChanged(gameManager.Wallet.Ore);
            HandleStatsChanged();
            HandleLayerChanged(gameManager.Mining.CurrentLayer);
        }

        void OnDisable()
        {
            gameManager.Wallet.OnChanged -= HandleOreChanged;
            gameManager.Upgrades.OnPickaxeChanged -= HandleStatsChanged;
            gameManager.Upgrades.OnGnomeChanged -= HandleStatsChanged;
            gameManager.Mining.OnLayerChanged -= HandleLayerChanged;
        }

        void HandleOreChanged(double ore)
        {
            oreText.text = BigNumberFormatter.Format(ore);
        }

        void HandleStatsChanged()
        {
            substatText.text = $"{gameManager.Upgrades.ClickPower} сили · {gameManager.Upgrades.GnomeCount} гномів";
            incomeText.text = BigNumberFormatter.Format(gameManager.Upgrades.IncomePerSecond) + " руди / сек";
        }

        void HandleLayerChanged(LayerData layer)
        {
            layerText.text = $"Шар {layer.layerIndex} · {layer.displayName}";
        }
    }
}
