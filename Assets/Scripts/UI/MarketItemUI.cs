using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceRush.Models;
using SpaceRush.Systems;
using SpaceRush.Core;

namespace SpaceRush.UI
{
    public class MarketItemUI : MonoBehaviour
    {
        [Header("UI References")]
        public TMP_Text ResourceNameText;
        public TMP_Text QuantityText;
        public TMP_Text SellPriceText;
        public TMP_Text BuyPriceText;
        public Button SellButton;
        public Button BuyButton;

        private ResourceType resourceType;

        public void Setup(ResourceType type)
        {
            resourceType = type;
            ResourceData res = ResourceManager.Instance.GetResource(type);
            if (res != null)
            {
                ResourceNameText.text = res.Name;
            }

            SellButton.onClick.RemoveAllListeners();
            BuyButton.onClick.RemoveAllListeners();

            SellButton.onClick.AddListener(() => OnSellClicked());
            BuyButton.onClick.AddListener(() => OnBuyClicked());
        }

        public void UpdateUI()
        {
            ResourceData res = ResourceManager.Instance.GetResource(resourceType);
            if (res == null) return;

            QuantityText.text = $"Qty: {res.Quantity}";

            float sellPrice = TradingSystem.Instance.GetSellPrice(resourceType);
            float buyPrice = TradingSystem.Instance.GetBuyPrice(resourceType);

            SellPriceText.text = $"Sell: {sellPrice:F0} CR";
            BuyPriceText.text = $"Buy: {buyPrice:F0} CR";

            // Interactive state
            SellButton.interactable = res.Quantity > 0;
            BuyButton.interactable = ResourceManager.Instance.Credits >= buyPrice;
        }

        private void OnSellClicked()
        {
            // For now, sell 1 or 10? Let's sell 1 for simplicity, or we can hold shift.
            // Let's assume the button sells 1 unit.
            TradingSystem.Instance.SellResource(resourceType, 1);
            UpdateUI();
        }

        private void OnBuyClicked()
        {
            TradingSystem.Instance.BuyResource(resourceType, 1);
            UpdateUI();
        }
    }
}
