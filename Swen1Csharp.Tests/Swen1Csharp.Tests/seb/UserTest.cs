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
    public class UserServiceTest
    {
        private Mock<NpgsqlCommand> _mockCommand;
        private Mock<RouterOverhead> _mockOverhead;
        private User _userService;

        [SetUp]
        public void Setup()
        {
            _mockCommand = new Mock<NpgsqlCommand>();
            _mockOverhead = new Mock<RouterOverhead>();
            _mockOverhead.Setup(routerOverhead => routerOverhead.GetCommand()).Returns(_mockCommand.Object);
            _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(0); // Default setup for ExecuteScalar
            _mockCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1); // Default setup for ExecuteNonQuery

            _userService = new User { routerOverhead = _mockOverhead.Object };
        }

        [Test]
        public void CreateUser_Success()
        {
            // Arrange
            var request = new Request { _method = Method.POST, body = "{\"Username\":\"testuser\",\"Password\":\"testpass\"}" };
            _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(0); // Simulate no existing user

            // Act
            var response = _userService.HandleRequest(request);

            // Assert
            Assert.AreEqual("Succesfully Created User testuser", response.Content);
        }

        [Test]
        public void CreateUser_Failure_UsernameAlreadyTaken()
        {
            // Arrange
            var request = new Request { _method = Method.POST, body = "{\"Username\":\"testuser\",\"Password\":\"testpass\"}" };
            _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns(1); // Simulate existing user

            // Act
            var response = _userService.HandleRequest(request);

            // Assert
            Assert.AreEqual("Username testuser already taken!", response.Content);
        }
        [Test]
        public void UpdateUser_Success()
        {
            // Arrange
            var request = new Request { _method = Method.PUT, pathParts = new string[] { "users", "testuser" }, token = "validToken", body = "{\"Name\":\"New Name\",\"Bio\":\"New Bio\",\"Image\":\"New Image\"}" };
            _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns("validToken"); // Simulate valid token

            // Act
            var response = _userService.HandleRequest(request);

            // Assert
            Assert.AreEqual("Updated user: testuser", response.Content);
        }

    }

}
