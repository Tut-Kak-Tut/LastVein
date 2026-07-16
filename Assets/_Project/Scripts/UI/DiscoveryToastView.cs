using System.Collections;
using TMPro;
using UnityEngine;
using LastVein.Core;
using LastVein.Data;

namespace LastVein.UI
{
    public class DiscoveryToastView : MonoBehaviour
    {
        [SerializeField] GameManager gameManager;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] RectTransform rectTransform;
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text factText;
        [SerializeField] float visibleDuration = 2.5f;
        [SerializeField] float slideDuration = 0.25f;
        [SerializeField] float fadeDuration = 0.4f;
        [SerializeField] float slideDistance = 40f;

        Coroutine toastRoutine;
        Vector2 restingPosition;

        void Awake()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            restingPosition = rectTransform.anchoredPosition;
        }

        void OnEnable()
        {
            gameManager.AtlasManager.OnMineralDiscovered += HandleDiscovered;
        }

        void OnDisable()
        {
            gameManager.AtlasManager.OnMineralDiscovered -= HandleDiscovered;
        }

        void HandleDiscovered(MineralData mineral)
        {
            nameText.text = mineral.displayName;
            factText.text = mineral.fact;

            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(ShowRoutine());
        }

        IEnumerator ShowRoutine()
        {
            canvasGroup.blocksRaycasts = true;
            Vector2 hiddenPosition = restingPosition + new Vector2(0, slideDistance);

            float t = 0f;
            while (t < slideDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / slideDuration);
                rectTransform.anchoredPosition = Vector2.Lerp(hiddenPosition, restingPosition, p);
                canvasGroup.alpha = p;
                yield return null;
            }
            rectTransform.anchoredPosition = restingPosition;
            canvasGroup.alpha = 1f;

            yield return new WaitForSeconds(visibleDuration);

            t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }

            canvasGroup.blocksRaycasts = false;
        }
    }
}
