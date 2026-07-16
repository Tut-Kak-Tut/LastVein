using System;
using System.Collections.Generic;
using LastVein.Data;

namespace LastVein.Core
{
    public class MineralAtlas
    {
        readonly List<MineralData> allMinerals = new List<MineralData>();
        readonly HashSet<MineralData> discovered = new HashSet<MineralData>();

        public event Action<MineralData> OnDiscovered;
        public IReadOnlyList<MineralData> AllMinerals => allMinerals;

        public MineralAtlas(LocationData location)
        {
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
            if (isNew) OnDiscovered?.Invoke(mineral);
            return isNew;
        }

        public bool IsDiscovered(MineralData mineral) => discovered.Contains(mineral);

        public (int discovered, int total) GetProgress(Era era)
        {
            int total = 0, count = 0;
            foreach (MineralData mineral in allMinerals)
            {
                if (mineral.era != era) continue;
                total++;
                if (discovered.Contains(mineral)) count++;
            }
            return (count, total);
        }
    }
}
