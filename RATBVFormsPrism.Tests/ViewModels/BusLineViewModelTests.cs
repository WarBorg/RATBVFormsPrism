using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using RATBVData.Models.Models;
using RATBVFormsPrism.Views;
using static RATBVFormsPrism.ViewModels.BusLinesViewModel;

namespace RATBVFormsPrism.Tests.ViewModels
{
    public class BusLineViewModelTests
    {
        #region Fields

        private IFixture _fixture;
        private Mock<INavigationService> _navigationServiceMock;

        #endregion

        #region Setup Methods

        [SetUp]
        public void TestSetup()
        {
            _fixture = new Fixture();

            _fixture.Customize(new AutoMoqCustomization());

            _navigationServiceMock = new Mock<INavigationService>();
        }

        #endregion

        #region Mock Data Methods

        private BusLineModel CreateMockBusLineWithNameAndRoute(string name,
                                                               string route)
        {
            return _fixture.Build<BusLineModel>()
                           .Without(l => l.Name)
                           .Without(l => l.Route)
                           .Do(l => l.Name = name)
                           .Do(l => l.Route = route)
                           .Create();
        }

        #endregion

        #region Test Case Sources

        private static readonly object[] BusLinesItemViewModelNullParameterCases =
        {
            new object[] { null, null},
            new object[] { new BusLineModel(), null },
            new object[] { null, new Mock<INavigationService>().Object },
        };

        #endregion

        #region Test Methods

        [Test]
        public void ShouldSetPropertiesInTheConstructor()
        {
            // Arrange
            var expectedName = "TestName";
            var expectedRoute = "TestRoute";

            var mockedBusLine = CreateMockBusLineWithNameAndRoute(expectedName,
                                                                  expectedRoute);

            _fixture.Inject(_navigationServiceMock);
            _fixture.Inject(mockedBusLine);

            // Act
            var SUT = _fixture.Build<BusLinesItemViewModel>()
                              .OmitAutoProperties()
                              .Create();

            // Assert
            Assert.That(SUT.Name, Is.EqualTo(expectedName));
            Assert.That(SUT.Route, Is.EqualTo(expectedRoute));
        }

        [Test, TestCaseSource(nameof(BusLinesItemViewModelNullParameterCases))]
        public void ShouldGetExceptionFromNullParametersInConstructor(BusLineModel busLineModel,
                                                                      INavigationService navigationService)
        {
            // Act / Assert
            Assert.Throws(Is.TypeOf<ArgumentNullException>(),
                          () => new BusLinesItemViewModel(busLineModel, navigationService));
        }

        [Test]
        public void ShouldNaviagteToShowSelectedBusLineStations()
        {
            // Arrange
            _fixture.Inject(_navigationServiceMock);

            var SUT = _fixture.Create<BusLinesItemViewModel>();

            // Act
            SUT.BusLineSelectedCommand.Execute(It.IsAny<object>());

            // Assert
            _navigationServiceMock.Verify(n => n.NavigateAsync(nameof(BusStations),
                                                               It.IsAny<NavigationParameters>()),
                                          Times.Once);
        }

        #endregion
    }
}
