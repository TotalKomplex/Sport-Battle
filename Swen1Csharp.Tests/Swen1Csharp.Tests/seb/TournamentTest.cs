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
    public class TournamentServiceTest
    {
        private Mock<NpgsqlCommand> _mockCommand;
        private Mock<RouterOverhead> _mockOverhead;
        private Tournament _tournamentService;

        [SetUp]
        public void Setup()
        {
            _mockCommand = new Mock<NpgsqlCommand>();
            _mockOverhead = new Mock<RouterOverhead>();
            _mockOverhead.Setup(routerOverhead => routerOverhead.GetCommand()).Returns(_mockCommand.Object);
            _mockOverhead.Setup(routerOverhead => routerOverhead.CheckToken(It.IsAny<NpgsqlCommand>(), It.IsAny<string>()))
                         .Returns(new Tuple<bool, string>(true, "testuser")); // Simulate successful token check
            _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(1); // Default setup for ExecuteScalar
            _mockCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1); // Default setup for ExecuteNonQuery

            _tournamentService = new Tournament { routerOverhead = _mockOverhead.Object };
        }

        [Test]
        public void HandleRequest_TournamentFound()
        {
            // Arrange
            var request = new Request();
            _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(DateTime.Now.AddMinutes(3)); // Simulate tournament found

            // Act
            var response = _tournamentService.HandleRequest(request);

            // Assert
            Assert.IsTrue(response.Content.Contains("Current leaders"));
        }

        [Test]
        public void HandleRequest_NoTournamentFound()
        {
            // Arrange
            var request = new Request();
            _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(null); // Simulate no tournament found

            // Act
            var response = _tournamentService.HandleRequest(request);

            // Assert
            Assert.AreEqual("No Tournament found!", response.Content);
        }
        [Test]
        public void HandleRequest_NoTournamentRunning()
        {
            // Arrange
            var request = new Request();
            _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(DateTime.Now.AddMinutes(-3)); // Simulate no tournament found

            // Act
            var response = _tournamentService.HandleRequest(request);

            // Assert
            Assert.AreEqual("No Tournament running!", response.Content);
        }
    }


}
