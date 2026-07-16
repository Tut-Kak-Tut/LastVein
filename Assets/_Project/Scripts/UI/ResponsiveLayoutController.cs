using UnityEngine;

namespace LastVein.UI
{
    // HorizontalLayoutGroup and VerticalLayoutGroup can't coexist on one GameObject
    // (LayoutGroup is [DisallowMultipleComponent]), so orientation switching works by
    // reparenting the two panels between a horizontal and a vertical wrapper.
    public class ResponsiveLayoutController : MonoBehaviour
    {
        [SerializeField] RectTransform horizontalWrapper;
        [SerializeField] RectTransform verticalWrapper;
        [SerializeField] RectTransform gameAreaPanel;
        [SerializeField] RectTransform sidePanel;
        [SerializeField] float aspectThreshold = 1f; // width/height below this => narrow/mobile layout

        bool? lastWasNarrow;

        void Update()
        {
            bool isNarrow = (float)Screen.width / Screen.height < aspectThreshold;
            if (lastWasNarrow.HasValue && lastWasNarrow.Value == isNarrow) return;

            lastWasNarrow = isNarrow;
            RectTransform activeWrapper = isNarrow ? verticalWrapper : horizontalWrapper;
            RectTransform inactiveWrapper = isNarrow ? horizontalWrapper : verticalWrapper;

            gameAreaPanel.SetParent(activeWrapper, false);
            sidePanel.SetParent(activeWrapper, false);
            gameAreaPanel.SetAsFirstSibling();
            sidePanel.SetAsLastSibling();

            activeWrapper.gameObject.SetActive(true);
            inactiveWrapper.gameObject.SetActive(false);
        }
    }
}
