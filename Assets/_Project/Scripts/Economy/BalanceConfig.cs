using UnityEngine;

namespace LastVein.Economy
{
    [CreateAssetMenu(menuName = "LastVein/Balance Config", fileName = "BalanceConfig")]
    public class BalanceConfig : ScriptableObject
    {
        public float perBlockHealthGrowth = 1.10f;
        public float oreRewardMultiplier = 0.4f;
    }
}
