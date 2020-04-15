using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using Prism.Navigation;
using RATBVData.Models.Models;
using RATBVFormsPrism.ViewModels;
using RATBVFormsPrism.Views;

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
                           .Without(l => l.SchedualLink)
                           .Do(l => l.Id = id)
                           .Do(l => l.Name = name)
                           .Do(l => l.SchedualLink = scheduleLink)
                           .Create();
        }

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
            var SUT = _fixture.Build<BusStationViewModel>()
                              .OmitAutoProperties()
                              .Create();

            // Assert
            Assert.That(SUT.Id, Is.EqualTo(expectedId));
            Assert.That(SUT.Name, Is.EqualTo(expectedName));
            Assert.That(SUT.ScheduleLink, Is.EqualTo(expectedScheduleLinke));
        }

        [Test]
        public void ShouldGetExceptionFromNullServiceParametersInConstructor()
        {
            // Act / Assert
            Assert.Throws(Is.TypeOf<ArgumentNullException>(),
                          () => new BusStationViewModel(null, null));
        }

        [Test]
        public void ShouldSetNullParametersInTheConstructor()
        {
            // Arrange
            _fixture.Inject(_navigationServiceMock);
            _fixture.Inject<BusStationModel>(null);

            // Act
            var SUT = _fixture.Build<BusStationViewModel>()
                              .OmitAutoProperties()
                              .Create();

            // Assert
            Assert.That(SUT.Id, Is.EqualTo(0));
            Assert.That(SUT.Name, Is.EqualTo(null));
            Assert.That(SUT.ScheduleLink, Is.EqualTo(null));
        }

        [Test]
        public void ShouldNaviagteToShowSelectedBusStationTimetables()
        {
            // Arrange
            _fixture.Inject(_navigationServiceMock);

            var SUT = _fixture.Create<BusStationViewModel>();

            // Act
            SUT.ShowSelectedBusTimeTableCommand.Execute(It.IsAny<object>());

            // Assert
            _navigationServiceMock.Verify(n => n.NavigateAsync(nameof(BusTimeTable),
                                                               It.IsAny<NavigationParameters>()),
                                          Times.Once);
        }

        #endregion
    }
}
