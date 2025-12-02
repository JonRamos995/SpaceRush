using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceRush.Models;
using SpaceRush.Systems;
using SpaceRush.Core;

namespace SpaceRush.UI
{
    public class LogisticsItemUI : MonoBehaviour
    {
        [Header("UI References")]
        public TMP_Text ResourceNameText;
        public Slider PercentageSlider;
        public TMP_Text PercentageValueText;

        private ResourceType resourceType;
        private LogisticsUIController controller;

        public void Setup(ResourceType type, float currentPct, LogisticsUIController ctrl)
        {
            resourceType = type;
            controller = ctrl;

            ResourceData res = ResourceManager.Instance.GetResourceData(type);
            if (res != null)
            {
                ResourceNameText.text = res.Name;
            }
            else
            {
                ResourceNameText.text = type.ToString();
            }

            PercentageSlider.minValue = 0f;
            PercentageSlider.maxValue = 100f; // 0 to 100%
            PercentageSlider.value = currentPct * 100f;
            UpdatePercentageText(PercentageSlider.value);

            PercentageSlider.onValueChanged.RemoveAllListeners();
            PercentageSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        private void OnSliderChanged(float val)
        {
            UpdatePercentageText(val);
            if (controller != null)
            {
                controller.OnAllocationChanged(resourceType, val / 100f);
            }
        }

        private void UpdatePercentageText(float val)
        {
            if (PercentageValueText != null)
                PercentageValueText.text = $"{val:F0}%";
        }
    }
}
