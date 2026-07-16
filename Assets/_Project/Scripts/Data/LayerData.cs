using UnityEngine;
using LastVein.Core;

namespace LastVein.Data
{
    [CreateAssetMenu(menuName = "LastVein/Layer", fileName = "Layer_")]
    public class LayerData : ScriptableObject
    {
        public int layerIndex;
        public string displayName;
        public Era era;
        public float baseHealth;
        public int blocksToAdvance;
        public Color paletteColor = Color.white;
        public LayerMineralEntry[] minerals;
    }
}
