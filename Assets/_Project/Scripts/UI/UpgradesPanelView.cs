using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LastVein.Core;

namespace LastVein.UI
{
    public class UpgradesPanelView : MonoBehaviour
    {
        [SerializeField] GameManager gameManager;

        [Header("Pickaxe row")]
        [SerializeField] TMP_Text pickaxeLevelText;
        [SerializeField] TMP_Text pickaxeNextPriceText;
        [SerializeField] Button pickaxeBuyButton;

        [Header("Gnome row")]
        [SerializeField] TMP_Text gnomeCountText;
        [SerializeField] TMP_Text gnomeNextPriceText;
        [SerializeField] Button gnomeHireButton;

        [SerializeField] float disabledAlpha = 0.5f;

        void OnEnable()
        {
            gameManager.OreEconomy.OnOreChanged += HandleOreChanged;
            gameManager.PickaxeUpgradeManager.OnPickaxeUpgraded += RefreshPickaxeRow;
            gameManager.GnomeManager.OnGnomeCountChanged += RefreshGnomeRow;

            pickaxeBuyButton.onClick.AddListener(HandlePickaxeBuyClicked);
            gnomeHireButton.onClick.AddListener(HandleGnomeHireClicked);

            RefreshPickaxeRow();
            RefreshGnomeRow();
        }

        void OnDisable()
        {
            gameManager.OreEconomy.OnOreChanged -= HandleOreChanged;
            gameManager.PickaxeUpgradeManager.OnPickaxeUpgraded -= RefreshPickaxeRow;
            gameManager.GnomeManager.OnGnomeCountChanged -= RefreshGnomeRow;

            pickaxeBuyButton.onClick.RemoveListener(HandlePickaxeBuyClicked);
            gnomeHireButton.onClick.RemoveListener(HandleGnomeHireClicked);
        }

        void HandlePickaxeBuyClicked()
        {
            gameManager.PickaxeUpgradeManager.TryPurchase();
        }

        void HandleGnomeHireClicked()
        {
            gameManager.GnomeManager.TryHire();
        }

        void HandleOreChanged(double ore)
        {
            UpdateAffordability(pickaxeBuyButton, gameManager.PickaxeUpgradeManager.GetNextPrice(), ore);
            UpdateAffordability(gnomeHireButton, gameManager.GnomeManager.GetNextPrice(), ore);
        }

        void RefreshPickaxeRow()
        {
            var pickaxe = gameManager.PickaxeUpgradeManager;
            pickaxeLevelText.text = $"Рівень {pickaxe.PickaxeLevel} (сила {pickaxe.ClickPower})";
            pickaxeNextPriceText.text = BigNumberFormatter.Format(pickaxe.GetNextPrice());
            UpdateAffordability(pickaxeBuyButton, pickaxe.GetNextPrice(), gameManager.OreEconomy.Ore);
        }

        void RefreshGnomeRow()
        {
            var gnomes = gameManager.GnomeManager;
            gnomeCountText.text = $"Гноми: {gnomes.GnomeCount}";
            gnomeNextPriceText.text = BigNumberFormatter.Format(gnomes.GetNextPrice());
            UpdateAffordability(gnomeHireButton, gnomes.GetNextPrice(), gameManager.OreEconomy.Ore);
        }

        void UpdateAffordability(Button button, double price, double ore)
        {
            bool affordable = ore >= price;
            button.interactable = affordable;

            Graphic graphic = button.targetGraphic;
            if (graphic != null)
            {
                Color c = graphic.color;
                c.a = affordable ? 1f : disabledAlpha;
                graphic.color = c;
            }
        }
    }
}
