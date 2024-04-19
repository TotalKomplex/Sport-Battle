using NUnit.Framework;
using Moq;
using Swen1Csharp.httpserver.http;
using Swen1Csharp.httpserver.server;
using System;
using System.Collections.Generic;
using Swen1Csharp.seb.service;
using Npgsql;
using Swen1Csharp.seb;

namespace Swen1Csharp.Tests
{
    [TestFixture]
    public class LoginServiceTests
    {
        private Mock<NpgsqlCommand> _mockCommand;
        private Mock<RouterOverhead> _mockOverhead;
        private Login _loginService;

        [SetUp]
        public void Setup()
        {
            
            _mockCommand = new Mock<NpgsqlCommand>();
            _mockOverhead = new Mock<RouterOverhead>();
            _mockOverhead.Setup(routerOverhead => routerOverhead.GetCommand()).Returns(_mockCommand.Object);
            _mockCommand.Setup(cmd => cmd.ExecuteScalar()).Returns("password");
            _mockCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1);
            
            _loginService = new Login { routerOverhead = _mockOverhead.Object };
            //_loginService = new Login ();
        }

        [Test]
        public void HandleRequest_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var request = new Request
            {
                body = "{\"Username\":\"testUser\",\"Password\":\"password\"}"
            };

            // Act
            var response = _loginService.HandleRequest(request);

            // Assert
            Assert.IsNotNull(response);
            TestContext.WriteLine(response.Content);
            Assert.IsTrue(response.Content.Contains("Basic testUser-sebToken"));
        }

        [Test]
        public void HandleRequest_InvalidCredentials_ReturnsError()
        {
            // Arrange
            var request = new Request
            {
                body = "{\"Username\":\"testUser\",\"Password\":\"wrongPassword\"}"
            };

            // Act
            var response = _loginService.HandleRequest(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual("Wrong username or password!", response.Content);
        }

        // Additional tests can be added here to cover other scenarios.
    }
}
