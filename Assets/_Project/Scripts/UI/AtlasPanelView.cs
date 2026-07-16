using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LastVein.Core;
using LastVein.Data;
using LastVein.Atlas;

namespace LastVein.UI
{
    public class AtlasPanelView : MonoBehaviour
    {
        [SerializeField] GameManager gameManager;
        [SerializeField] Transform gridParent;
        [SerializeField] MineralCellView cellPrefab;
        [SerializeField] TMP_Text progressText;

        [SerializeField] Button era1TabButton;
        [SerializeField] Button era2TabButton;
        [SerializeField] Button era3TabButton;

        Era currentEraFilter = Era.Era1;
        readonly List<MineralCellView> spawnedCells = new List<MineralCellView>();

        void OnEnable()
        {
            gameManager.AtlasManager.OnMineralDiscovered += HandleDiscovered;

            era1TabButton.onClick.AddListener(HandleEra1Clicked);
            era2TabButton.onClick.AddListener(HandleEra2Clicked);
            era3TabButton.onClick.AddListener(HandleEra3Clicked);

            RebuildGrid();
        }

        void OnDisable()
        {
            gameManager.AtlasManager.OnMineralDiscovered -= HandleDiscovered;

            era1TabButton.onClick.RemoveListener(HandleEra1Clicked);
            era2TabButton.onClick.RemoveListener(HandleEra2Clicked);
            era3TabButton.onClick.RemoveListener(HandleEra3Clicked);
        }

        void HandleEra1Clicked() => SetEraFilter(Era.Era1);
        void HandleEra2Clicked() => SetEraFilter(Era.Era2);
        void HandleEra3Clicked() => SetEraFilter(Era.Era3);

        void SetEraFilter(Era era)
        {
            currentEraFilter = era;
            RebuildGrid();
        }

        void HandleDiscovered(MineralData mineral)
        {
            RebuildGrid();
        }

        void RebuildGrid()
        {
            foreach (MineralCellView cell in spawnedCells) Destroy(cell.gameObject);
            spawnedCells.Clear();

            AtlasManager atlas = gameManager.AtlasManager;

            foreach (MineralData mineral in atlas.AllMinerals)
            {
                if (mineral.era != currentEraFilter) continue;

                MineralCellView cell = Instantiate(cellPrefab, gridParent);
                cell.SetData(mineral, atlas.IsDiscovered(mineral));
                spawnedCells.Add(cell);
            }

            var (discovered, total) = atlas.GetProgress(currentEraFilter);
            progressText.text = $"{discovered} / {total}";
        }
    }
}
