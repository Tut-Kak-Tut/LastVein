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
            gameManager.Wallet.OnChanged += HandleOreChanged;
            gameManager.Upgrades.OnPickaxeChanged += RefreshPickaxeRow;
            gameManager.Upgrades.OnGnomeChanged += RefreshGnomeRow;

            pickaxeBuyButton.onClick.AddListener(HandlePickaxeBuyClicked);
            gnomeHireButton.onClick.AddListener(HandleGnomeHireClicked);

            RefreshPickaxeRow();
            RefreshGnomeRow();
        }

        void OnDisable()
        {
            gameManager.Wallet.OnChanged -= HandleOreChanged;
            gameManager.Upgrades.OnPickaxeChanged -= RefreshPickaxeRow;
            gameManager.Upgrades.OnGnomeChanged -= RefreshGnomeRow;

            pickaxeBuyButton.onClick.RemoveListener(HandlePickaxeBuyClicked);
            gnomeHireButton.onClick.RemoveListener(HandleGnomeHireClicked);
        }

        void HandlePickaxeBuyClicked() => gameManager.TryBuyPickaxeUpgrade();

        void HandleGnomeHireClicked() => gameManager.TryHireGnome();

        void HandleOreChanged(double ore)
        {
            UpdateAffordability(pickaxeBuyButton, gameManager.Upgrades.GetNextPickaxePrice(), ore);
            UpdateAffordability(gnomeHireButton, gameManager.Upgrades.GetNextGnomePrice(), ore);
        }

        void RefreshPickaxeRow()
        {
            var upgrades = gameManager.Upgrades;
            pickaxeLevelText.text = $"Рівень {upgrades.PickaxeLevel} (сила {upgrades.ClickPower})";
            pickaxeNextPriceText.text = BigNumberFormatter.Format(upgrades.GetNextPickaxePrice());
            UpdateAffordability(pickaxeBuyButton, upgrades.GetNextPickaxePrice(), gameManager.Wallet.Ore);
        }

        void RefreshGnomeRow()
        {
            var upgrades = gameManager.Upgrades;
            gnomeCountText.text = $"Гноми: {upgrades.GnomeCount}";
            gnomeNextPriceText.text = BigNumberFormatter.Format(upgrades.GetNextGnomePrice());
            UpdateAffordability(gnomeHireButton, upgrades.GetNextGnomePrice(), gameManager.Wallet.Ore);
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
