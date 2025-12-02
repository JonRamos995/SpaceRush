using NUnit.Framework;
using UnityEngine;
using SpaceRush.Systems;
using SpaceRush.Models;
using SpaceRush.Core;
using SpaceRush.Data;
using System.Collections.Generic;

namespace SpaceRush.Tests
{
    public class TradingSystemTests
    {
        private GameObject gameGameObject;
        private TradingSystem tradingSystem;
        private ResourceManager resourceManager;

        [SetUp]
        public void Setup()
        {
            gameGameObject = new GameObject("GameManager");
            resourceManager = gameGameObject.AddComponent<ResourceManager>();
            tradingSystem = gameGameObject.AddComponent<TradingSystem>();

            // Initialize Resource Manager manually since Awake might not run in non-PlayMode tests automatically
            // But we can simulate initialization.
            // Actually, we should probably mock this, but for simple tests we can just use the component.
            // Note: In EditMode tests, Awake is not called automatically unless we use GameObject creation in a specific way.
            // We will manually initialize for testing purposes.

            // Adding a test resource
            resourceManager.AddResource(ResourceType.Iron, 100);
            var res = resourceManager.GetResource(ResourceType.Iron);
            res.CurrentMarketValue = 10f; // Fixed price for testing
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(gameGameObject);
        }

        [Test]
        public void TestSellResource_Success()
        {
            // Arrange
            resourceManager.AddCredits(0); // Ensure 0
            int initialAmount = 100;
            float price = 10f;
            resourceManager.GetResource(ResourceType.Iron).CurrentMarketValue = price;

            // Act
            tradingSystem.SellResource(ResourceType.Iron, 10);

            // Assert
            Assert.AreEqual(initialAmount - 10, resourceManager.GetResource(ResourceType.Iron).Quantity);
            Assert.AreEqual(10 * price, resourceManager.Credits);
        }

        [Test]
        public void TestSellResource_NotEnoughStock()
        {
            // Arrange
            resourceManager.AddCredits(0);
            int initialAmount = 100;

            // Act
            tradingSystem.SellResource(ResourceType.Iron, 200); // Try to sell more than we have

            // Assert
            Assert.AreEqual(initialAmount, resourceManager.GetResource(ResourceType.Iron).Quantity); // Should not change
            Assert.AreEqual(0, resourceManager.Credits); // Should not gain money
        }

        [Test]
        public void TestBuyResource_Success()
        {
            // Arrange
            resourceManager.AddCredits(1000);
            float buyPrice = tradingSystem.GetBuyPrice(ResourceType.Iron); // Usually SellPrice * 1.1

            // Act
            tradingSystem.BuyResource(ResourceType.Iron, 10);

            // Assert
            Assert.AreEqual(100 + 10, resourceManager.GetResource(ResourceType.Iron).Quantity);
            Assert.Less(resourceManager.Credits, 1000);
        }
    }
}
