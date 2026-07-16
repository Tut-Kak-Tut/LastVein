using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LastVein.Data;

namespace LastVein.UI
{
    public class MineralCellView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Image backgroundImage;
        [SerializeField] TMP_Text glyphText;
        [SerializeField] GameObject tooltip;
        [SerializeField] TMP_Text tooltipText;

        [SerializeField] Color notFoundBackground = new Color(0.164f, 0.149f, 0.133f);
        [SerializeField] Color foundBackground = new Color(0.788f, 0.482f, 0.290f);
        [SerializeField] Color foundTextColor = new Color(0.164f, 0.149f, 0.133f);
        [SerializeField] Color notFoundTextColor = new Color(0.722f, 0.690f, 0.612f);

        MineralData mineral;
        bool discovered;

        public void SetData(MineralData data, bool isDiscovered)
        {
            mineral = data;
            discovered = isDiscovered;

            backgroundImage.color = discovered ? foundBackground : notFoundBackground;
            glyphText.text = discovered ? "◆" : "?";
            glyphText.color = discovered ? foundTextColor : notFoundTextColor;

            if (tooltip != null) tooltip.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltip == null || tooltipText == null) return;
            tooltipText.text = discovered ? mineral.displayName : "???";
            tooltip.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltip == null) return;
            tooltip.SetActive(false);
        }
    }
}
