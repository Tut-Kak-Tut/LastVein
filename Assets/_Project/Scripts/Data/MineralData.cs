using UnityEngine;
using LastVein.Core;

namespace LastVein.Data
{
    [CreateAssetMenu(menuName = "LastVein/Mineral", fileName = "Mineral_")]
    public class MineralData : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string fact;
        public Era era;
        public Color placeholderColor = Color.white;
        public Sprite sprite;
    }
}
