using System;
using System.Collections.Generic;
using UnityEngine;
using LastVein.Core;
using LastVein.Data;

namespace LastVein.Atlas
{
    public class AtlasManager : MonoBehaviour
    {
        public event Action<MineralData> OnMineralDiscovered;

        readonly List<MineralData> allMinerals = new List<MineralData>();
        readonly HashSet<MineralData> discovered = new HashSet<MineralData>();

        public IReadOnlyList<MineralData> AllMinerals => allMinerals;

        public void Init(LocationData location)
        {
            allMinerals.Clear();
            discovered.Clear();

            var seen = new HashSet<MineralData>();
            foreach (LayerData layer in location.layers)
            {
                foreach (LayerMineralEntry entry in layer.minerals)
                {
                    if (entry.mineral != null && seen.Add(entry.mineral))
                    {
                        allMinerals.Add(entry.mineral);
                    }
                }
            }
        }

        public bool Discover(MineralData mineral)
        {
            if (mineral == null) return false;
            bool isNew = discovered.Add(mineral);
            if (isNew) OnMineralDiscovered?.Invoke(mineral);
            return isNew;
        }

        public bool IsDiscovered(MineralData mineral) => discovered.Contains(mineral);

        public (int discovered, int total) GetProgress()
        {
            return (discovered.Count, allMinerals.Count);
        }

        public (int discovered, int total) GetProgress(Era era)
        {
            int total = 0, disc = 0;
            foreach (MineralData m in allMinerals)
            {
                if (m.era != era) continue;
                total++;
                if (discovered.Contains(m)) disc++;
            }
            return (disc, total);
        }
    }
}
