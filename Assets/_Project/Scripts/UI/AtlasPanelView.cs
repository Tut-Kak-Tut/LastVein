using System;
using TMPro;
using UnityEngine;
using LastVein.Core;

namespace LastVein.UI
{
    public class AtlasPanelView : MonoBehaviour
    {
        [Serializable]
        public class EraSection
        {
            public Era era;
            public Transform mineralsContainer;
            public TMP_Text progressText;
        }

        [SerializeField] GameManager gameManager;
        [SerializeField] MineralCellView cellPrefab;
        [SerializeField] EraSection[] sections;

        void OnEnable()
        {
            gameManager.Atlas.OnDiscovered += HandleDiscovered;
            RebuildAll();
        }

        void OnDisable()
        {
            gameManager.Atlas.OnDiscovered -= HandleDiscovered;
        }

        void HandleDiscovered(LastVein.Data.MineralData mineral) => RebuildAll();

        void RebuildAll()
        {
            MineralAtlas atlas = gameManager.Atlas;

            foreach (EraSection section in sections)
            {
                for (int i = section.mineralsContainer.childCount - 1; i >= 0; i--)
                {
                    Destroy(section.mineralsContainer.GetChild(i).gameObject);
                }

                foreach (var mineral in atlas.AllMinerals)
                {
                    if (mineral.era != section.era) continue;

                    MineralCellView cell = Instantiate(cellPrefab, section.mineralsContainer);
                    cell.SetData(mineral, atlas.IsDiscovered(mineral));
                }

                var (discovered, total) = atlas.GetProgress(section.era);
                if (section.progressText != null) section.progressText.text = $"{discovered} / {total}";
            }
        }
    }
}
