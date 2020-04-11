﻿using Acr.UserDialogs;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using RATBVFormsPrism.Services;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace RATBVFormsPrism.Tests.Services
{
    [TestFixture]
    public class ConnectivityServiceTests
    {
        #region Fields

        private IFixture _fixture;
        private Mock<IUserDialogs> _userDialogsServiceMock;
        private Mock<IConnectivity> _connectivityServiceMock;
        
        #endregion

        #region Setup Methods

        [SetUp]
        public void TestSetup()
        {
            _fixture = new Fixture();

            _fixture.Customize(new AutoMoqCustomization());

            _userDialogsServiceMock = new Mock<IUserDialogs>();
            _connectivityServiceMock = new Mock<IConnectivity>();
        }

        #endregion

        #region Test Methods

        [Test]
        public void ShouldReturnThatInternetIsAvailable()
        {
            // Arrange
            _connectivityServiceMock.SetupGet(c => c.NetworkAccess)
                                    .Returns(NetworkAccess.Internet);

            _fixture.Inject(_connectivityServiceMock);
            _fixture.Inject(_userDialogsServiceMock);

            var expectedIsInternetAvailableResult = true;

            var SUT = _fixture.Create<ConnectivityService>();

            // Act
            var isInternetAvailale = SUT.IsInternetAvailable;

            // Assert
            Assert.That(isInternetAvailale, Is.EqualTo(expectedIsInternetAvailableResult));

            _userDialogsServiceMock.Verify(s => s.Toast(It.IsAny<string>(), default), Times.Never);
        }

        [Test]
        [TestCase(NetworkAccess.ConstrainedInternet, Description = "The internet is limited")]
        [TestCase(NetworkAccess.Local, Description = "Local access only")]
        [TestCase(NetworkAccess.None, Description = "No connectivity")]
        [TestCase(NetworkAccess.Unknown, Description = "The state of the connectivity is unknown")]
        public void ShouldReturnThatInternetIsNotAvailable(NetworkAccess networkAccess)
        {
            // Arrange
            _connectivityServiceMock.SetupGet(c => c.NetworkAccess)
                                    .Returns(networkAccess);

            _fixture.Inject(_connectivityServiceMock);
            _fixture.Inject(_userDialogsServiceMock);

            var expectedIsInternetAvailableResult = false;

            var SUT = _fixture.Create<ConnectivityService>();

            // Act
            var isInternetAvailale = SUT.IsInternetAvailable;

            // Assert
            Assert.That(isInternetAvailale, Is.EqualTo(expectedIsInternetAvailableResult));

            _userDialogsServiceMock.Verify(s => s.Toast(It.IsAny<string>(), default), Times.Once);
        }

        #endregion
    }
}
