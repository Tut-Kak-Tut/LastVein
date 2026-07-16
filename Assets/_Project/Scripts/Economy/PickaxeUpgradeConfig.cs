using UnityEngine;

namespace LastVein.Economy
{
    [CreateAssetMenu(menuName = "LastVein/Pickaxe Upgrade Config", fileName = "PickaxeUpgradeConfig")]
    public class PickaxeUpgradeConfig : ScriptableObject
    {
        public double basePrice = 10;
        public float growthRate = 1.15f;
        public int clickPowerPerLevel = 1;
    }
}
