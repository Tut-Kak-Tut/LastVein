using UnityEngine;

namespace LastVein.Workers
{
    [CreateAssetMenu(menuName = "LastVein/Gnome Config", fileName = "GnomeConfig")]
    public class GnomeConfig : ScriptableObject
    {
        public double basePrice = 25;
        public float growthRate = 1.18f;
        public double incomePerGnome = 0.5;
        public float tickInterval = 0.1f;
    }
}
