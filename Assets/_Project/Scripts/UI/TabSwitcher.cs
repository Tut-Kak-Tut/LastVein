using UnityEngine;
using UnityEngine.UI;

namespace LastVein.UI
{
    public class TabSwitcher : MonoBehaviour
    {
        [SerializeField] GameObject upgradesPanel;
        [SerializeField] GameObject atlasPanel;
        [SerializeField] Button upgradesTabButton;
        [SerializeField] Button atlasTabButton;

        void OnEnable()
        {
            upgradesTabButton.onClick.AddListener(ShowUpgrades);
            atlasTabButton.onClick.AddListener(ShowAtlas);
        }

        void OnDisable()
        {
            upgradesTabButton.onClick.RemoveListener(ShowUpgrades);
            atlasTabButton.onClick.RemoveListener(ShowAtlas);
        }

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
