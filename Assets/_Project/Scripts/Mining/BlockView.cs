using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LastVein.Data;
using LastVein.Economy;

namespace LastVein.Mining
{
    public class BlockView : MonoBehaviour
    {
        [SerializeField] MiningManager miningManager;
        [SerializeField] PickaxeUpgradeManager pickaxeUpgradeManager;
        [SerializeField] BlockClickHandler clickHandler;
        [SerializeField] Image blockImage;
        [SerializeField] Image progressBarFill;
        [SerializeField] RectTransform blockRectForPunch;
        [SerializeField] Image[] crackImages;
        [SerializeField] RectTransform floatingTextParent;

        [SerializeField] float punchScale = 0.92f;
        [SerializeField] float punchDuration = 0.12f;

        [SerializeField] Color floatingTextColor = new Color(0.878f, 0.627f, 0.416f);
        [SerializeField] float floatingTextFontSize = 28f;
        [SerializeField] float floatingTextRiseDistance = 40f;
        [SerializeField] float floatingTextDuration = 0.6f;

        Coroutine punchRoutine;

        void Awake()
        {
            if (blockRectForPunch == null)
                blockRectForPunch = blockImage != null ? blockImage.rectTransform : GetComponent<RectTransform>();
        }

        void OnEnable()
        {
            miningManager.OnBlockSpawned += HandleBlockSpawned;
            miningManager.OnBlockDamaged += HandleBlockDamaged;
            miningManager.OnLayerChanged += HandleLayerChanged;
            clickHandler.OnBlockTapped += HandleTapped;

            // GameManager.Awake() may have already spawned the first block before this OnEnable
            // ran (all Awakes in a scene complete before any OnEnable), so pull current state directly.
            if (blockImage != null) blockImage.color = miningManager.CurrentLayer.paletteColor;
            if (miningManager.CurrentBlock != null) HandleBlockSpawned(miningManager.CurrentBlock);
        }

        void OnDisable()
        {
            miningManager.OnBlockSpawned -= HandleBlockSpawned;
            miningManager.OnBlockDamaged -= HandleBlockDamaged;
            miningManager.OnLayerChanged -= HandleLayerChanged;
            clickHandler.OnBlockTapped -= HandleTapped;
        }

        void HandleTapped()
        {
            miningManager.ApplyClick();
            PlayClickPunch();
            SpawnFloatingText();
        }

        void HandleBlockSpawned(BlockRuntimeState block)
        {
            UpdateProgress(block.currentHealth, block.maxHealth);
        }

        void HandleBlockDamaged(float current, float max)
        {
            UpdateProgress(current, max);
        }

        void HandleLayerChanged(LayerData layer, int layerIndex)
        {
            if (blockImage != null) blockImage.color = layer.paletteColor;
        }

        void UpdateProgress(float current, float max)
        {
            float progress = 1f - Mathf.Clamp01(current / max);
            if (progressBarFill != null) progressBarFill.fillAmount = progress;

            if (crackImages == null) return;
            for (int i = 0; i < crackImages.Length; i++)
            {
                if (crackImages[i] == null) continue;
                float threshold = (i + 1) * 0.2f;
                Color c = crackImages[i].color;
                c.a = progress >= threshold ? 1f : 0f;
                crackImages[i].color = c;
            }
        }

        void PlayClickPunch()
        {
            if (punchRoutine != null) StopCoroutine(punchRoutine);
            punchRoutine = StartCoroutine(ClickPunchRoutine());
        }

        IEnumerator ClickPunchRoutine()
        {
            if (blockRectForPunch == null) yield break;

            Vector3 baseScale = blockRectForPunch.localScale;
            float half = punchDuration * 0.5f;
            float t = 0f;

            while (t < half)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(1f, punchScale, t / half);
                blockRectForPunch.localScale = baseScale * s;
                yield return null;
            }

            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(punchScale, 1f, t / half);
                blockRectForPunch.localScale = baseScale * s;
                yield return null;
            }

            blockRectForPunch.localScale = baseScale;
        }

        void SpawnFloatingText()
        {
            if (floatingTextParent == null || pickaxeUpgradeManager == null) return;

            var go = new GameObject("FloatingText", typeof(RectTransform));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(floatingTextParent, false);
            rt.anchoredPosition = new Vector2(Random.Range(-60f, 60f), Random.Range(-20f, 40f));

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = "+" + pickaxeUpgradeManager.ClickPower;
            tmp.fontSize = floatingTextFontSize;
            tmp.color = floatingTextColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            StartCoroutine(FloatingTextRoutine(rt, tmp));
        }

        IEnumerator FloatingTextRoutine(RectTransform rt, TextMeshProUGUI tmp)
        {
            Vector2 startPos = rt.anchoredPosition;
            Vector2 endPos = startPos + Vector2.up * floatingTextRiseDistance;
            float t = 0f;

            while (t < floatingTextDuration)
            {
                t += Time.deltaTime;
                float p = t / floatingTextDuration;
                rt.anchoredPosition = Vector2.Lerp(startPos, endPos, p);
                Color c = tmp.color;
                c.a = 1f - p;
                tmp.color = c;
                yield return null;
            }

            Destroy(rt.gameObject);
        }
    }
}
