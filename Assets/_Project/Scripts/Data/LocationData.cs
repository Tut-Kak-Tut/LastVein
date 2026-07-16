using UnityEngine;

namespace LastVein.Data
{
    [CreateAssetMenu(menuName = "LastVein/Location", fileName = "Location_")]
    public class LocationData : ScriptableObject
    {
        public string id;
        public string displayName;
        public LayerData[] layers;
        public MineralData[] locationMineralPool;
    }
}
