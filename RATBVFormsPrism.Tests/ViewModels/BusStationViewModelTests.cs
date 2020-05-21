using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using RATBVData.Models.Models;
using RATBVFormsPrism.Views;
using static RATBVFormsPrism.ViewModels.BusStationsViewModel;

namespace RATBVFormsPrism.Tests.ViewModels
{
    public class BusStationViewModelTests
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

        private BusStationModel CreateMockBusStationWithIdNameAndLink(int id,
                                                                      string name,
                                                                      string scheduleLink)
        {
            return _fixture.Build<BusStationModel>()
                           .Without(l => l.Id)
                           .Without(l => l.Name)
                           .Without(l => l.ScheduleLink)
                           .Do(l => l.Id = id)
                           .Do(l => l.Name = name)
                           .Do(l => l.ScheduleLink = scheduleLink)
                           .Create();
        }

        #endregion

        #region Test Case Sources

        private static readonly object[] BusStationsItemViewModelNullParameterCases =
        {
            new object[] { null, null},
            new object[] { new BusStationModel(), null },
            new object[] { null, new Mock<INavigationService>().Object },
        };

        #endregion

        #region Test Methods

        [Test]
        public void ShouldSetPropertiesInTheConstructor()
        {
            // Arrange
            var expectedId = new Random().Next(1, 30);
            var expectedName = "TestName";
            var expectedScheduleLinke = "TestScheduleRoute";

            var mockedBusStation = CreateMockBusStationWithIdNameAndLink(expectedId,
                                                                         expectedName,
                                                                         expectedScheduleLinke);

            _fixture.Inject(_navigationServiceMock);
            _fixture.Inject(mockedBusStation);

            // Act
            var SUT = _fixture.Build<BusStationsItemViewModel>()
                              .OmitAutoProperties()
                              .Create();

            // Assert
            Assert.That(SUT.Id, Is.EqualTo(expectedId));
            Assert.That(SUT.Name, Is.EqualTo(expectedName));
            Assert.That(SUT.ScheduleLink, Is.EqualTo(expectedScheduleLinke));
        }

        [Test, TestCaseSource(nameof(BusStationsItemViewModelNullParameterCases))]
        public void ShouldGetExceptionFromNullParametersInConstructor(BusStationModel busStationModel,
                                                                      INavigationService navigationService)
        {
            // Act / Assert
            Assert.Throws(Is.TypeOf<ArgumentNullException>(),
                          () => new BusStationsItemViewModel(busStationModel, navigationService));
        }

        [Test]
        public void ShouldNaviagteToShowSelectedBusStationTimetables()
        {
            // Arrange
            _fixture.Inject(_navigationServiceMock);

            var SUT = _fixture.Create<BusStationsItemViewModel>();

            // Act
            SUT.BusStationSelectedCommand.Execute(It.IsAny<object>());

            // Assert
            _navigationServiceMock.Verify(n => n.NavigateAsync(nameof(BusTimetables),
                                                               It.IsAny<NavigationParameters>()),
                                          Times.Once);
        }

        #endregion
    }
}
