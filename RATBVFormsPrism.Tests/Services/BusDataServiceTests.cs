using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using RATBVData.Models.Models;
using RATBVFormsPrism.Services;

namespace RATBVFormsPrism.Tests.Services
{
    public class BusDataServiceTests
    {
        #region Fields

        private IFixture _fixture;
        private Mock<ISQLiteAsyncConnection> _sqliteAsyncConnectionMock;

        #endregion

        #region Setup Methods

        [SetUp]
        public void TestSetup()
        {
            _fixture = new Fixture();

            _fixture.Customize(new AutoMoqCustomization());

            _sqliteAsyncConnectionMock = new Mock<ISQLiteAsyncConnection>();
        }

        #endregion

        #region Universal Test Methods

        [Test]
        public async Task ShouldCreateAllDatabaseTables()
        {
            // Arrange
            _fixture.Inject(_sqliteAsyncConnectionMock);

            var expectedTablesToBeCreated = 3;

            var SUT = _fixture.Create<BusDataService>();

            // Act
            await SUT.CreateAllTablesAsync();

            // Assert
            _sqliteAsyncConnectionMock.Verify(c => c.CreateTableAsync<object>(SQLite.CreateFlags.None),
                                              Times.Exactly(expectedTablesToBeCreated));
        }

        [Test]
        public async Task ShouldDropAllDatabaseTables()
        {
            // Arrange
            _fixture.Inject(_sqliteAsyncConnectionMock);

            var expectedTablesToBeDropped = 3;

            var SUT = _fixture.Create<BusDataService>();

            // Act
            await SUT.DropAllTablesAsync();

            // Assert
            _sqliteAsyncConnectionMock.Verify(c => c.DropTableAsync<object>(),
                                              Times.Exactly(expectedTablesToBeDropped));
        }

        [Test]
        public async Task ShouldDeleteAllDatabaseTables()
        {
            // Arrange
            _fixture.Inject(_sqliteAsyncConnectionMock);

            var expectedTablesToBeDeleted = 3;

            var SUT = _fixture.Create<BusDataService>();

            // Act
            await SUT.DeleteAllTablesAsync();

            // Assert
            _sqliteAsyncConnectionMock.Verify(c => c.GetConnection(),
                                              Times.Exactly(expectedTablesToBeDeleted));
        }

        #endregion

        #region Bus Lines Test Methods

        [Test]
        [Ignore("Ignored because I cannot mock second grade methods: .CountAsync()")]
        public async Task ShouldGetDatabaseTables()
        {
            // Arrange
            var expectedBusTableRowCount = 3;

            _sqliteAsyncConnectionMock.Setup(c => c.Table<object>().CountAsync())
                                      .Returns(Task.FromResult(expectedBusTableRowCount));

            _fixture.Inject(_sqliteAsyncConnectionMock);

            var SUT = _fixture.Create<BusDataService>();

            // Act
            var busTableRowCount = await SUT.CountBusLinesAsync();

            // Assert
            Assert.That(busTableRowCount, Is.EqualTo(expectedBusTableRowCount));
        }        

        [Test]
        [Ignore("Ignored because I cannot mock Table<T>")]
        public void ShouldInsertOrReplaceBusLine()
        {
            // Arrange
            var expecteReplacedBusLinesCount = 1;

            _sqliteAsyncConnectionMock.Setup(c => c.DeleteAsync(It.IsAny<object>()))
                                      .Returns(Task.FromResult(expecteReplacedBusLinesCount));

            _fixture.Inject(_sqliteAsyncConnectionMock);

            var SUT = _fixture.Create<BusDataService>();

            // Act
            var replacedBusLines = 0; //await SUT.InsertOrReplaceBusStationsAsync(It.IsAny<List<BusStationModel>>());

            // Assert
            Assert.That(replacedBusLines, Is.EqualTo(expecteReplacedBusLinesCount));

            _sqliteAsyncConnectionMock.Verify(c => c.DeleteAsync(It.IsAny<BusLineModel>()),
                                              Times.Once);
        }

        #endregion

        #region Insert -- Update -- Delete test we dont use anymore

        [Test]
        [Ignore("Ignored because we dont use this functionality")]
        public void ShouldGetBusLineById()
        {
            // Arrange
            _fixture.Inject(_sqliteAsyncConnectionMock);

            var SUT = _fixture.Create<BusDataService>();

            // Act
            //await SUT.GetBusLineByIdAsync(It.IsAny<int>());

            // Assert
            _sqliteAsyncConnectionMock.Verify(c => c.GetAsync<BusLineModel>(It.IsAny<int>()),
                                              Times.Once);
        }

        [Test]
        [Ignore("Ignored because we dont use this functionality")]
        public void ShouldInsertBusLine()
        {
            // Arrange
            var expectedInsertedBusLinesCount = 1;

            _sqliteAsyncConnectionMock.Setup(c => c.InsertAsync(It.IsAny<object>()))
                                      .Returns(Task.FromResult(expectedInsertedBusLinesCount));

            _fixture.Inject(_sqliteAsyncConnectionMock);

            var SUT = _fixture.Create<BusDataService>();

            // Act
            var insertedBusLines = 0; //await SUT.InsertBusLineAsync(It.IsAny<BusLineModel>());

            // Assert
            Assert.That(insertedBusLines, Is.EqualTo(expectedInsertedBusLinesCount));

            _sqliteAsyncConnectionMock.Verify(c => c.InsertAsync(It.IsAny<BusLineModel>()),
                                              Times.Once);
        }

        [Test]
        [Ignore("Ignored because we dont use this functionality")]
        public void ShouldUpdateBusLine()
        {
            // Arrange
            var expectedUpdatedBusLinesCount = 1;

            _sqliteAsyncConnectionMock.Setup(c => c.UpdateAsync(It.IsAny<object>()))
                                      .Returns(Task.FromResult(expectedUpdatedBusLinesCount));

            _fixture.Inject(_sqliteAsyncConnectionMock);

            var SUT = _fixture.Create<BusDataService>();

            // Act
            var updatedBusLines = 0; //await SUT.UpdateBusLineAsync(It.IsAny<BusLineModel>());

            // Assert
            Assert.That(updatedBusLines, Is.EqualTo(expectedUpdatedBusLinesCount));

            _sqliteAsyncConnectionMock.Verify(c => c.UpdateAsync(It.IsAny<BusLineModel>()),
                                              Times.Once);
        }

        [Test]
        [Ignore("Ignored because we dont use this functionality")]
        public void ShouldDeleteBusLine()
        {
            // Arrange
            var expectedDeletedBusLinesCount = 1;

            _sqliteAsyncConnectionMock.Setup(c => c.DeleteAsync(It.IsAny<object>()))
                                      .Returns(Task.FromResult(expectedDeletedBusLinesCount));

            _fixture.Inject(_sqliteAsyncConnectionMock);

            var SUT = _fixture.Create<BusDataService>();

            // Act
            var deletedBusLines = 0; //await SUT.DeleteBusLineAsync(It.IsAny<BusLineModel>());

            // Assert
            Assert.That(deletedBusLines, Is.EqualTo(expectedDeletedBusLinesCount));

            _sqliteAsyncConnectionMock.Verify(c => c.DeleteAsync(It.IsAny<BusLineModel>()),
                                              Times.Once);
        }

        #endregion
    }
}