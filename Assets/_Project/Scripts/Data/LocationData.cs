using UnityEngine;

namespace LastVein.Data
{
    [CreateAssetMenu(menuName = "LastVein/Location", fileName = "Location_")]
    public class LocationData : ScriptableObject
    {
        public string id;
        public string displayName;
        public LayerData[] layers;

        // Reserved for future secret-room bonus rewards (out of scope this pass) — don't remove.
        public MineralData[] locationMineralPool;
    }
}
