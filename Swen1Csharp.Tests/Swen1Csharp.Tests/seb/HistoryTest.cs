using NUnit.Framework;
using Moq;
using Swen1Csharp.httpserver.http;
using Swen1Csharp.httpserver.server;
using System;
using System.Collections.Generic;
using Swen1Csharp.seb.service;
using Npgsql;
using Swen1Csharp.seb;
using System.Data;

namespace Swen1Csharp.Tests
{
    [TestFixture]
    public class HistoryServiceTest
    {
        private Mock<NpgsqlCommand> _mockCommand;
        private Mock<RouterOverhead> _mockOverhead;
        private History _historyService;

        [SetUp]
        public void Setup()
        {
            _mockCommand = new Mock<NpgsqlCommand>();
            _mockOverhead = new Mock<RouterOverhead>();
            _mockOverhead.Setup(routerOverhead => routerOverhead.GetCommand()).Returns(_mockCommand.Object);
            _mockOverhead.Setup(routerOverhead => routerOverhead.CheckToken(It.IsAny<NpgsqlCommand>(), It.IsAny<string>()))
                         .Returns(new Tuple<bool,string>(true, "testuser")); // Simulate successful token check
            _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(1); // Default setup for ExecuteScalar
            _mockCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1); // Default setup for ExecuteNonQuery

            _historyService = new History { routerOverhead = _mockOverhead.Object };
        }

        [Test]
        public void AddHistory_Success()
        {
            // Arrange
            var request = new Request { _method = Method.POST, body = "{\"Count\":10,\"DurationInSeconds\":60}" };
            _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(DateTime.Now); // Simulate existing user and tournament

            // Act
            var response = _historyService.HandleRequest(request);

            // Assert
            Assert.AreEqual($"Succesfully created history entry with count: 10", response.Content);
        }

        [Test]
        public void AddHistory_Failure_TournamentStatusCheckFails()
        {
            // Arrange
            var request = new Request { _method = Method.POST, body = "{\"Count\":\"10\",\"DurationInSeconds\":\"60\"}" };
            _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(DateTime.Now.AddMinutes(-3)); // Simulate tournament status check failure

            // Act
            var response = _historyService.HandleRequest(request);

            // Assert
            Assert.AreEqual($"Started new tournament. 2 minutes left!\nSuccesfully created history entry with count: 10", response.Content);
        }
    }


}
