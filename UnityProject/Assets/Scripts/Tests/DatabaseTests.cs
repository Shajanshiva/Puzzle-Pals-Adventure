using NUnit.Framework;
using UnityEngine;
using PuzzlePals.Backend;
using System.Threading.Tasks;

namespace PuzzlePals.Tests
{
    public class DatabaseTests
    {
        private PlayerProfile testProfile;

        [SetUp]
        public void Setup()
        {
            testProfile = new PlayerProfile
            {
                UserId = "test_user_001",
                Username = "TestPal",
                AvatarId = "avatar_01",
                Level = 1,
                Experience = 500,
                Coins = 100,
                Gems = 10
            };
        }

        [Test]
        public void TestAddRewards_IncreasesValuesCorrectly()
        {
            // Arrange
            int coinsToAdd = 50;
            int gemsToAdd = 5;
            int xpToAdd = 200;

            // Act
            testProfile.Coins += coinsToAdd;
            testProfile.Gems += gemsToAdd;
            testProfile.Experience += xpToAdd;

            // Assert
            Assert.AreEqual(150, testProfile.Coins);
            Assert.AreEqual(15, testProfile.Gems);
            Assert.AreEqual(700, testProfile.Experience);
        }

        [Test]
        public void TestLevelUpTransition_WhenXpThresholdCrossed()
        {
            // Arrange
            int largeXp = 1600;

            // Act
            testProfile.Experience += largeXp; // Total: 2100
            
            while (testProfile.Experience >= 1000)
            {
                testProfile.Experience -= 1000;
                testProfile.Level++;
            }

            // Assert
            Assert.AreEqual(3, testProfile.Level); // Level up twice
            Assert.AreEqual(100, testProfile.Experience); // 2100 - 2000 = 100 remainder
        }

        [Test]
        public void TestProfileSerialization_JsonCompatibility()
        {
            // Act
            string json = JsonUtility.ToJson(testProfile);
            PlayerProfile deserialized = JsonUtility.FromJson<PlayerProfile>(json);

            // Assert
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(testProfile.UserId, deserialized.UserId);
            Assert.AreEqual(testProfile.Username, deserialized.Username);
            Assert.AreEqual(testProfile.Level, deserialized.Level);
        }
    }
}
