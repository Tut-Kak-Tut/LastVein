using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LastVein.UI
{
    public class UIHoverTint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Graphic target;
        [SerializeField] Color normalColor;
        [SerializeField] Color hoverColor;

        public void SetColors(Graphic graphic, Color normal, Color hover)
        {
            target = graphic;
            normalColor = normal;
            hoverColor = hover;
            target.color = normalColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (target != null) target.color = hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (target != null) target.color = normalColor;
        }
    }
}
