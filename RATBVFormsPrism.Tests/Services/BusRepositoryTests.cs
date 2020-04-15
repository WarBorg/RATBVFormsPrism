using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using RATBVData.Models.Models;
using RATBVFormsPrism.Services;
using Refit;

namespace RATBVFormsPrism.Tests.Services
{
    public class BusRepositoryTests
    {
        #region Fields

        private IFixture _fixture;
        private Mock<IBusApi> _busApiMock;
        private Mock<IBusDataService> _busDataServiceMock;

        #endregion

        #region Setup Methods

        [SetUp]
        public void TestSetup()
        {
            _fixture = new Fixture();

            _fixture.Customize(new AutoMoqCustomization());

            _busApiMock = new Mock<IBusApi>();
            _busDataServiceMock = new Mock<IBusDataService>();
        }

        #endregion

        #region Mock Data Methods

        private List<BusLineModel> CreateMockBusLines(int busLineCount)
        {
            return _fixture.CreateMany<BusLineModel>(busLineCount)
                           .ToList();
        }

        private List<BusStationModel> CreateMockBusStations(int busStationCount)
        {
            return _fixture.CreateMany<BusStationModel>(busStationCount)
                           .ToList();
        }

        private List<BusTimeTableModel> CreateMockBusTimetables(int busTimetableCount)
        {
            return _fixture.CreateMany<BusTimeTableModel>(busTimetableCount)
                           .ToList();
        }

        #endregion

        #region Bus Lines Test Methods

        [Test]
        [TestCase(0, false, Description = "When No Bus Lines Exist In The Local DataBase")]
        [TestCase(20, true, Description = "When Forced Refresh")]
        public async Task ShouldGetBusLinesFromWeb(int initialBusLineCount,
                                                   bool isForcedRefresh)
        {
            // Arrange
            _busDataServiceMock.Setup(s => s.CountBusLinesAsync())
                               .Returns(Task.FromResult(initialBusLineCount));

            var expectedBusLineCount = 20;

            var mockedBusLines = CreateMockBusLines(expectedBusLineCount);

            _busApiMock.Setup(a => a.GetBusLines())
                       .ReturnsAsync(mockedBusLines);

            _busDataServiceMock.Setup(s => s.GetBusLineAsync())
                               .ReturnsAsync(mockedBusLines);

            _fixture.Inject(_busDataServiceMock);
            _fixture.Inject(_busApiMock);

            var SUT = _fixture.Create<BusRepository>();

            // Act
            var busLines = await SUT.GetBusLinesAsync(isForcedRefresh);

            // Assert
            Assert.That(busLines.Count, Is.EqualTo(expectedBusLineCount));

            _busApiMock.Verify(a => a.GetBusLines(), Times.Once);

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusLinesAsync(
                                                It.Is<List<BusLineModel>>(
                                                    l => l.Count == expectedBusLineCount)),
                                       Times.Once);
        }

        [Test]
        public async Task ShouldGetBusLinesFromLocalDatabaseWhenAvailable()
        {
            // Arrange
            var initialBusLineCount = new Random().Next(1, 20);

            _busDataServiceMock.Setup(s => s.CountBusLinesAsync())
                               .Returns(Task.FromResult(initialBusLineCount));

            var expectedBusLineCount = new Random().Next(1, 20);

            var mockedBusLine = CreateMockBusLines(expectedBusLineCount);

            _busDataServiceMock.Setup(s => s.GetBusLineAsync())
                               .ReturnsAsync(mockedBusLine);

            _fixture.Inject(_busDataServiceMock);
            _fixture.Inject(_busApiMock);

            var SUT = _fixture.Create<BusRepository>();

            // Act
            var busLines = await SUT.GetBusLinesAsync(isForcedRefresh: false);

            // Assert
            Assert.That(busLines.Count, Is.EqualTo(expectedBusLineCount));

            _busApiMock.Verify(a => a.GetBusLines(), Times.Never);

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusLinesAsync(
                                                It.IsAny<IEnumerable<BusLineModel>>()),
                                       Times.Never);
            
        }

        [Test]
        [TestCase(typeof(ApiException), Description = "Throws a Api Exception")]
        [TestCase(typeof(Exception), Description = "Throws random Exception")]
        public async Task ShouldGetExceptionFromBusLinesWebRetrival(Type exceptionType)
        {
            // Arrange
            var initialBusLineCount = new Random().Next(1, 20);

            _busDataServiceMock.Setup(s => s.CountBusLinesAsync())
                               .Returns(Task.FromResult(initialBusLineCount));

            Exception exception;

            if (exceptionType == typeof(ApiException))
            {

                exception = await ApiException.Create(new HttpRequestMessage(),
                                                      HttpMethod.Get,
                                                      new HttpResponseMessage(HttpStatusCode.NotFound));
            }
            else
            {
                exception = new Exception();
            }

            _busApiMock.Setup(a => a.GetBusLines())
                       .ThrowsAsync(exception);

            _fixture.Inject(_busDataServiceMock);
            _fixture.Inject(_busApiMock);

            var SUT = _fixture.Create<BusRepository>();

            // Act / Assert
            var exeption = Assert.ThrowsAsync(Is.TypeOf(exceptionType),
                                              async () => await SUT.GetBusLinesAsync(isForcedRefresh: true));

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusLinesAsync(
                                                It.IsAny<IEnumerable<BusLineModel>>()),
                                       Times.Never);
        }

        [Test]
        public void ShouldGetExceptionFromBusLinesLocalDatabaseRetrival()
        {
            // Arrange
            var expectedExceptionMessage = "No Database found";

            _busDataServiceMock.Setup(s => s.CountBusLinesAsync())
                               .Throws(new Exception(expectedExceptionMessage));

            _fixture.Inject(_busDataServiceMock);

            var SUT = _fixture.Create<BusRepository>();

            // Act / Assert
            Assert.ThrowsAsync(Is.TypeOf<Exception>(),
                               async () => await SUT.GetBusLinesAsync(isForcedRefresh: true),
                               expectedExceptionMessage);

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusLinesAsync(
                                                It.IsAny<IEnumerable<BusLineModel>>()),
                                       Times.Never);
        }

        #endregion

        #region Bus Stations Test Methods

        [Test]
        [TestCase(0, false, Description = "When No Bus Stations Exist In The Local DataBase")]
        [TestCase(20, true, Description = "When Forced Refresh")]
        public async Task ShouldGetBusStationsFromWeb(int initialBusStationCount,
                                                      bool isForcedRefresh)
        {
            // Arrange
            int mockBusLineId = new Random().Next(0, 30);

            _busDataServiceMock.Setup(s => s.CountBusStationsByBusLineIdAndDirectionAsync(mockBusLineId,
                                                                                          It.IsAny<string>()))
                               .Returns(Task.FromResult(initialBusStationCount));

            var expectedBusStationsCount = new Random().Next(1, 20);

            var mockedBusStations = CreateMockBusStations(expectedBusStationsCount);

            _busApiMock.Setup(a => a.GetBusStations(It.IsAny<string>()))
                       .ReturnsAsync(mockedBusStations);

            _busDataServiceMock.Setup(s => s.GetBusStationsByBusLineIdAndDirectionAsync(mockBusLineId,
                                                                                        default))
                               .ReturnsAsync(mockedBusStations);

            _fixture.Inject(_busDataServiceMock);
            _fixture.Inject(_busApiMock);

            var SUT = _fixture.Create<BusRepository>();

            // Act
            var busStations = await SUT.GetBusStationsAsync(directionLink: default,
                                                            direction: default,
                                                            mockBusLineId,
                                                            isForcedRefresh);

            // Assert
            Assert.That(busStations.Count, Is.EqualTo(expectedBusStationsCount));

            _busApiMock.Verify(a => a.GetBusStations(It.IsAny<string>()), Times.Once);

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusStationsAsync(
                                                It.Is<List<BusStationModel>>(
                                                    l => l.Count == expectedBusStationsCount)),
                                       Times.Once);
        }

        [Test]
        public async Task ShouldGetBusStationsFromLocalDatabaseWhenAvailable()
        {
            // Arrange
            var initialBusStationCount = new Random().Next(1, 20);
            int mockBusLineId = new Random().Next(0, 30);

            _busDataServiceMock.Setup(s => s.CountBusStationsByBusLineIdAndDirectionAsync(mockBusLineId,
                                                                                          It.IsAny<string>()))
                               .Returns(Task.FromResult(initialBusStationCount));

            var expectedBusStationCount = new Random().Next(1, 20);

            var mockedBusStations = CreateMockBusStations(expectedBusStationCount);

            _busDataServiceMock.Setup(s => s.GetBusStationsByBusLineIdAndDirectionAsync(mockBusLineId,
                                                                                        default))
                               .ReturnsAsync(mockedBusStations);

            _fixture.Inject(_busDataServiceMock);
            _fixture.Inject(_busApiMock);

            var SUT = _fixture.Create<BusRepository>();

            // Act
            var busStations = await SUT.GetBusStationsAsync(directionLink: default,
                                                            direction: default,
                                                            mockBusLineId,
                                                            isForcedRefresh: false);

            // Assert
            Assert.That(busStations.Count, Is.EqualTo(expectedBusStationCount));

            _busApiMock.Verify(a => a.GetBusStations(It.IsAny<string>()), Times.Never);

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusStationsAsync(
                                                It.IsAny<IEnumerable<BusStationModel>>()),
                                       Times.Never);
        }

        [Test]
        [TestCase(typeof(ApiException), Description = "Throws a Api Exception")]
        [TestCase(typeof(Exception), Description = "Throws random Exception")]
        public async Task ShouldGetExceptionFromBusStationsWebRetrival(Type exceptionType)
        {
            // Arrange
            var initialBusStationCount = new Random().Next(1, 20);

            _busDataServiceMock.Setup(s => s.CountBusStationsByBusLineIdAndDirectionAsync(It.IsAny<int>(),
                                                                                          It.IsAny<string>()))
                               .Returns(Task.FromResult(initialBusStationCount));

            Exception exception;

            if (exceptionType == typeof(ApiException))
            {

                exception = await ApiException.Create(new HttpRequestMessage(),
                                                      HttpMethod.Get,
                                                      new HttpResponseMessage(HttpStatusCode.NotFound));
            }
            else
            {
                exception = new Exception();
            }

            _busApiMock.Setup(a => a.GetBusStations(It.IsAny<string>()))
                       .ThrowsAsync(exception);

            _fixture.Inject(_busDataServiceMock);
            _fixture.Inject(_busApiMock);

            var SUT = _fixture.Create<BusRepository>();

            // Act / Assert
            var exeption = Assert.ThrowsAsync(Is.TypeOf(exceptionType),
                                              async () => await SUT.GetBusStationsAsync(directionLink: default,
                                                                                        direction: default,
                                                                                        busLineId: default,
                                                                                        isForcedRefresh: true));

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusStationsAsync(
                                                It.IsAny<IEnumerable<BusStationModel>>()),
                                       Times.Never);
        }

        [Test]
        public void ShouldGetExceptionFromBusStationsLocalDatabaseRetrival()
        {
            // Arrange
            var expectedExceptionMessage = "No Database found";

            _busDataServiceMock.Setup(s => s.CountBusStationsByBusLineIdAndDirectionAsync(It.IsAny<int>(),
                                                                   It.IsAny<string>()))
                               .ThrowsAsync(new Exception(expectedExceptionMessage));

            _fixture.Inject(_busDataServiceMock);

            var SUT = _fixture.Create<BusRepository>();

            // Act / Assert
            var exeption = Assert.ThrowsAsync(Is.TypeOf<Exception>(),
                                              async () => await SUT.GetBusStationsAsync(directionLink: default,
                                                                                        direction: default,
                                                                                        busLineId: default,
                                                                                        isForcedRefresh: true),
                                              expectedExceptionMessage);

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusStationsAsync(
                                                It.IsAny<IEnumerable<BusStationModel>>()),
                                       Times.Never);
        }

        #endregion

        #region Bus Timetable Test Methods

        [Test]
        [TestCase(0, false, Description = "When No Bus Stations Exist In The Local DataBase")]
        [TestCase(20, true, Description = "When Forced Refresh")]
        public async Task ShouldGetBusTimetablesFromWeb(int initialBusTimetableCount,
                                                       bool isForcedRefresh)
        {
            // Arrange
            int mockBusStationId = new Random().Next(0, 30);

            _busDataServiceMock.Setup(s => s.CountBusTimeTableByBusStationIdAsync(mockBusStationId))
                               .Returns(Task.FromResult(initialBusTimetableCount));

            var expectedBusTimetableCount = new Random().Next(1, 20);

            var mockedBusTimetables = CreateMockBusTimetables(expectedBusTimetableCount);

            _busApiMock.Setup(a => a.GetBusTimeTables(It.IsAny<string>()))
                       .ReturnsAsync(mockedBusTimetables);

            _busDataServiceMock.Setup(s => s.GetBusTimeTableByBusStationId(mockBusStationId))
                               .ReturnsAsync(mockedBusTimetables);

            _fixture.Inject(_busDataServiceMock);
            _fixture.Inject(_busApiMock);

            var SUT = _fixture.Create<BusRepository>();

            // Act
            var busTimetables = await SUT.GetBusTimeTableAsync(schedualLink: default,
                                                               mockBusStationId,
                                                               isForcedRefresh);

            // Assert
            Assert.That(busTimetables.Count, Is.EqualTo(expectedBusTimetableCount));

            _busApiMock.Verify(a => a.GetBusTimeTables(It.IsAny<string>()), Times.Once);

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusTimeTablesAsync(
                                                It.Is<List<BusTimeTableModel>>(
                                                    l => l.Count == expectedBusTimetableCount)),
                                       Times.Once);
        }

        [Test]
        public async Task ShouldGetBusTimetablesFromLocalDatabaseWhenAvailable()
        {
            // Arrange
            var initialBusTimetableCount = new Random().Next(1, 20);
            int mockBusStationId = new Random().Next(0, 30);

            _busDataServiceMock.Setup(s => s.CountBusTimeTableByBusStationIdAsync(mockBusStationId))
                               .Returns(Task.FromResult(initialBusTimetableCount));

            var expectedBusTimetableCount = new Random().Next(1, 20);

            var mockedBusTimetables = CreateMockBusTimetables(expectedBusTimetableCount);

            _busDataServiceMock.Setup(s => s.GetBusTimeTableByBusStationId(mockBusStationId))
                               .ReturnsAsync(mockedBusTimetables);

            _fixture.Inject(_busDataServiceMock);
            _fixture.Inject(_busApiMock);

            var SUT = _fixture.Create<BusRepository>();

            // Act
            var busTimetables = await SUT.GetBusTimeTableAsync(schedualLink: default,
                                                               mockBusStationId,
                                                               isForcedRefresh: false);

            // Assert
            Assert.That(busTimetables.Count, Is.EqualTo(expectedBusTimetableCount));

            _busApiMock.Verify(a => a.GetBusTimeTables(It.IsAny<string>()), Times.Never);

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusTimeTablesAsync(
                                                It.IsAny<IEnumerable<BusTimeTableModel>>()),
                                       Times.Never);
        }

        [Test]
        [TestCase(typeof(ApiException), Description = "Throws a Api Exception")]
        [TestCase(typeof(Exception), Description = "Throws random Exception")]
        public async Task ShouldGetExceptionFromBusTimetablesWebRetrival(Type exceptionType)
        {
            // Arrange
            var initialBusTimetableCount = new Random().Next(1, 20);

            _busDataServiceMock.Setup(s => s.CountBusTimeTableByBusStationIdAsync(It.IsAny<int>()))
                               .Returns(Task.FromResult(initialBusTimetableCount));

            Exception exception;

            if (exceptionType == typeof(ApiException))
            {

                exception = await ApiException.Create(new HttpRequestMessage(),
                                                      HttpMethod.Get,
                                                      new HttpResponseMessage(HttpStatusCode.NotFound));
            }
            else
            {
                exception = new Exception();
            }

            _busApiMock.Setup(a => a.GetBusTimeTables(It.IsAny<string>()))
                       .ThrowsAsync(exception);

            _fixture.Inject(_busDataServiceMock);
            _fixture.Inject(_busApiMock);

            var SUT = _fixture.Create<BusRepository>();

            // Act / Assert
            var exeption = Assert.ThrowsAsync(Is.TypeOf(exceptionType),
                                              async () => await SUT.GetBusTimeTableAsync(schedualLink: default,
                                                                                         busStationId: default,
                                                                                         isForcedRefresh: true));

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusTimeTablesAsync(
                                                It.IsAny<IEnumerable<BusTimeTableModel>>()),
                                       Times.Never);
        }

        [Test]
        public void ShouldGetExceptionFromBusTimetablesLocalDatabaseRetrival()
        {
            // Arrange
            var expectedExceptionMessage = "No Database found";

            _busDataServiceMock.Setup(s => s.CountBusTimeTableByBusStationIdAsync(It.IsAny<int>()))
                               .ThrowsAsync(new Exception(expectedExceptionMessage));

            _fixture.Inject(_busDataServiceMock);

            var SUT = _fixture.Create<BusRepository>();

            // Act / Assert
            var exeption = Assert.ThrowsAsync(Is.TypeOf<Exception>(),
                                              async () => await SUT.GetBusTimeTableAsync(schedualLink: default,
                                                                                         busStationId: default,
                                                                                         isForcedRefresh: true),
                                              expectedExceptionMessage);

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusTimeTablesAsync(
                                                It.IsAny<IEnumerable<BusTimeTableModel>>()),
                                       Times.Never);
        }

        [Test]
        public async Task ShouldDownloadBusTimetablesForAllStations()
        {
            // Arrange
            var expectedBusStationsCount = new Random().Next(1, 20);
            var expectedBusTimetableCount = new Random().Next(1, 20);

            var mockedBusStations = CreateMockBusStations(expectedBusStationsCount);

            _busApiMock.Setup(a => a.GetBusStations(It.IsAny<string>()))
                       .ReturnsAsync(mockedBusStations);

            var mockedBusTimetables = CreateMockBusTimetables(expectedBusTimetableCount);

            _busApiMock.Setup(a => a.GetBusTimeTables(It.IsAny<string>()))
                       .ReturnsAsync(mockedBusTimetables);

            _fixture.Inject(_busDataServiceMock);
            _fixture.Inject(_busApiMock);

            var SUT = _fixture.Create<BusRepository>();

            // Act
            await SUT.DownloadAllStationsTimetablesAsync(normalDirectionLink: default,
                                                         reverseDirectionLink: default,
                                                         busLineId: default);

            // Assert
            _busApiMock.Verify(a => a.GetBusStations(It.IsAny<string>()),
                               Times.Exactly(2));
            _busApiMock.Verify(a => a.GetBusTimeTables(It.IsAny<string>()),
                               Times.Exactly(expectedBusStationsCount * 2));

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusStationsAsync(
                                                It.IsAny<IEnumerable<BusStationModel>>()),
                                       Times.Once);

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusTimeTablesAsync(
                                                It.IsAny<IEnumerable<BusTimeTableModel>>()),
                                       Times.Exactly(expectedBusStationsCount * 2));
        }

        [Test]
        [TestCase(typeof(ApiException), Description = "Throws a Api Exception")]
        [TestCase(typeof(Exception), Description = "Throws random Exception")]
        public async Task ShouldGetExceptionFromBusTimetablesDownloadWebRetrival(Type exceptionType)
        {
            // Arrange
            Exception exception;

            if (exceptionType == typeof(ApiException))
            {

                exception = await ApiException.Create(new HttpRequestMessage(),
                                                      HttpMethod.Get,
                                                      new HttpResponseMessage(HttpStatusCode.NotFound));
            }
            else
            {
                exception = new Exception();
            }

            _busApiMock.Setup(a => a.GetBusStations(It.IsAny<string>()))
                       .ThrowsAsync(exception);

            _fixture.Inject(_busDataServiceMock);
            _fixture.Inject(_busApiMock);

            var SUT = _fixture.Create<BusRepository>();

            // Act / Assert
            var exeption = Assert.ThrowsAsync(Is.TypeOf(exceptionType),
                                              async () => await SUT.DownloadAllStationsTimetablesAsync(normalDirectionLink: default,
                                                                                                       reverseDirectionLink: default,
                                                                                                       busLineId: default));

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusStationsAsync(
                                                It.IsAny<IEnumerable<BusStationModel>>()),
                                       Times.Never);

            _busDataServiceMock.Verify(s => s.InsertOrReplaceBusTimeTablesAsync(
                                                It.IsAny<IEnumerable<BusTimeTableModel>>()),
                                       Times.Never);
        }

        #endregion
    }
}