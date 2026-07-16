using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using LastVein.Core;

namespace LastVein.UI
{
    public class BlockView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] GameManager gameManager;
        [SerializeField] Image progressBarFill;
        [SerializeField] Image[] crackImages;

        void OnEnable()
        {
            gameManager.Mining.OnBlockProgress += HandleProgress;
            HandleProgress(gameManager.Mining.CurrentHealth, gameManager.Mining.MaxHealth);
        }

        void OnDisable()
        {
            gameManager.Mining.OnBlockProgress -= HandleProgress;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            gameManager.ApplyClick();
        }

        void HandleProgress(float current, float max)
        {
            float progress = 1f - Mathf.Clamp01(current / max);
            if (progressBarFill != null) progressBarFill.fillAmount = progress;

            if (crackImages == null) return;
            for (int i = 0; i < crackImages.Length; i++)
            {
                if (crackImages[i] == null) continue;
                float threshold = (i + 1) * (1f / crackImages.Length);
                Color c = crackImages[i].color;
                c.a = progress >= threshold ? 1f : 0f;
                crackImages[i].color = c;
            }
        }
    }
}
