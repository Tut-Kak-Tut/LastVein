using UnityEngine;

namespace LastVein.UI
{
    public class SidePanelController : MonoBehaviour
    {
        [SerializeField] GameObject upgradesPanel;
        [SerializeField] GameObject atlasPanel;

        public void ShowUpgrades()
        {
            upgradesPanel.SetActive(true);
            atlasPanel.SetActive(false);
        }

        public void ShowAtlas()
        {
            upgradesPanel.SetActive(false);
            atlasPanel.SetActive(true);
        }
    }
}
