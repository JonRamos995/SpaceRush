using NUnit.Framework;
using UnityEngine;
using SpaceRush.Systems;
using SpaceRush.Models;
using System.Collections.Generic;

namespace SpaceRush.Tests
{
    public class LogisticsTests
    {
        private GameObject logisticsGo;
        private LogisticsSystem logisticsSystem;

        [SetUp]
        public void Setup()
        {
            logisticsGo = new GameObject("LogisticsSystem");
            logisticsSystem = logisticsGo.AddComponent<LogisticsSystem>();
        }

        [TearDown]
        public void Teardown()
        {
            if (logisticsGo != null)
                UnityEngine.Object.DestroyImmediate(logisticsGo);
        }

        [Test]
        public void Test_CalculateTransfer_DefaultBalance()
        {
            var stockpile = new Dictionary<ResourceType, int>
            {
                { ResourceType.Iron, 100 },
                { ResourceType.Gold, 100 }
            };
            int capacity = 10;
            var allocations = new Dictionary<ResourceType, float>();

            var result = logisticsSystem.CalculateTransfer(stockpile, capacity, allocations);

            Assert.IsTrue(result.ContainsKey(ResourceType.Iron));
            Assert.IsTrue(result.ContainsKey(ResourceType.Gold));
            Assert.AreEqual(5, result[ResourceType.Iron]);
            Assert.AreEqual(5, result[ResourceType.Gold]);
        }

        [Test]
        public void Test_CalculateTransfer_CustomAllocation()
        {
            var stockpile = new Dictionary<ResourceType, int>
            {
                { ResourceType.Iron, 100 },
                { ResourceType.Gold, 100 }
            };
            int capacity = 10;
            var allocations = new Dictionary<ResourceType, float>
            {
                { ResourceType.Iron, 0.8f },
                { ResourceType.Gold, 0.2f }
            };

            var result = logisticsSystem.CalculateTransfer(stockpile, capacity, allocations);

            Assert.AreEqual(8, result[ResourceType.Iron]);
            Assert.AreEqual(2, result[ResourceType.Gold]);
        }

        [Test]
        public void Test_CalculateTransfer_CapacityLimit()
        {
            var stockpile = new Dictionary<ResourceType, int>
            {
                { ResourceType.Iron, 100 },
                { ResourceType.Gold, 100 },
                { ResourceType.Platinum, 100 }
            };
            int capacity = 10;
            var allocations = new Dictionary<ResourceType, float>();

            var result = logisticsSystem.CalculateTransfer(stockpile, capacity, allocations);

            int total = 0;
            foreach (var val in result.Values) total += val;

            Assert.AreEqual(10, total);
        }

        [Test]
        public void Test_CalculateTransfer_PartialAvailability()
        {
            // Request 50/50, but only have 3 Iron. Should take 3 Iron, 5 Gold.
             var stockpile = new Dictionary<ResourceType, int>
            {
                { ResourceType.Iron, 3 },
                { ResourceType.Gold, 100 }
            };
            int capacity = 10;
            var allocations = new Dictionary<ResourceType, float>
            {
                { ResourceType.Iron, 0.5f },
                { ResourceType.Gold, 0.5f }
            };

            var result = logisticsSystem.CalculateTransfer(stockpile, capacity, allocations);

            Assert.AreEqual(3, result[ResourceType.Iron]);
            Assert.AreEqual(5, result[ResourceType.Gold]);
        }

        [Test]
        public void Test_CalculateTransfer_DefaultBalance_FillRemaining()
        {
            var stockpile = new Dictionary<ResourceType, int>
            {
                { ResourceType.Iron, 2 },
                { ResourceType.Gold, 100 }
            };
            int capacity = 10;
            var allocations = new Dictionary<ResourceType, float>();

            var result = logisticsSystem.CalculateTransfer(stockpile, capacity, allocations);

            Assert.AreEqual(2, result[ResourceType.Iron]);
            Assert.AreEqual(8, result[ResourceType.Gold]);

            int total = 0;
            foreach (var val in result.Values) total += val;
            Assert.AreEqual(10, total);
        }
    }
}
