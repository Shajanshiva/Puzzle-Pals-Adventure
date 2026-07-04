using NUnit.Framework;
using UnityEngine;
using PuzzlePals.Multiplayer;
using System.Collections.Generic;

namespace PuzzlePals.Tests
{
    public class MatchmakerTests
    {
        private List<MatchmakingTicket> mockDatabaseQueue;

        [SetUp]
        public void Setup()
        {
            mockDatabaseQueue = new List<MatchmakingTicket>();
        }

        [Test]
        public void TestCreateMatchmakingTicket_AddsToQueue()
        {
            // Arrange
            MatchmakingTicket ticket = new MatchmakingTicket
            {
                TicketId = "ticket_user_001",
                HostUserId = "user_001",
                RoomCode = "ROOMAB",
                Status = "waiting",
                Timestamp = 123456789
            };

            // Act
            mockDatabaseQueue.Add(ticket);

            // Assert
            Assert.AreEqual(1, mockDatabaseQueue.Count);
            Assert.AreEqual("waiting", mockDatabaseQueue[0].Status);
        }

        [Test]
        public void TestClaimTicketTransaction_SucceedsForFirstPlayer()
        {
            // Arrange
            MatchmakingTicket ticket = new MatchmakingTicket
            {
                TicketId = "ticket_user_001",
                HostUserId = "user_001",
                RoomCode = "ROOMAB",
                Status = "waiting",
                Timestamp = 123456789
            };
            mockDatabaseQueue.Add(ticket);

            // Act - Player 2 attempts to claim
            bool transactionClaimed = false;
            var targetTicket = mockDatabaseQueue.Find(t => t.TicketId == "ticket_user_001");
            
            if (targetTicket != null && targetTicket.Status == "waiting")
            {
                targetTicket.Status = "matched";
                transactionClaimed = true;
            }

            // Assert
            Assert.IsTrue(transactionClaimed);
            Assert.AreEqual("matched", mockDatabaseQueue[0].Status);

            // Act - Player 3 attempts to claim same ticket
            bool secondClaimAttempt = false;
            if (targetTicket != null && targetTicket.Status == "waiting")
            {
                secondClaimAttempt = true;
            }

            // Assert second claim fails
            Assert.IsFalse(secondClaimAttempt);
        }

        [Test]
        public void TestMatchmakingTimeout_CancelsTicket()
        {
            // Arrange
            bool isSearching = true;
            int elapsedSeconds = 35; // past 30s threshold
            const int timeoutLimit = 30;

            // Act
            if (elapsedSeconds > timeoutLimit)
            {
                isSearching = false;
            }

            // Assert
            Assert.IsFalse(isSearching);
        }
    }
}
