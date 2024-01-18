
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Infotecs.Source.Controllers;
using Infotecs.Source.Services;
namespace TestInfotecs
{
    public class ExperimentControllerTests
    {
        private readonly Mock<IExperimentService> _mockExperimentService;
        private readonly ExperimentController _controller;
        private readonly Mock<IFormFile> _mockFormFile;

        public ExperimentControllerTests()
        {
            // Mock the dependencies
            _mockExperimentService = new Mock<IExperimentService>();
            _controller = new ExperimentController(_mockExperimentService.Object);
            _mockFormFile = new Mock<IFormFile>();
        }

        [Fact]
        public async Task ProcessFile_ReturnsOk_WhenFileIsProcessedSuccessfully()
        {
            // Arrange
            var fileName = "test.txt";
            var fileContent = "2021-01-02_21:33:21;4";
            var authorName = "Test Author";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(fileContent);
            writer.Flush();
            ms.Position = 0;

            _mockFormFile.Setup(f => f.FileName).Returns(fileName);
            _mockFormFile.Setup(_ => _.OpenReadStream()).Returns(ms);
            _mockFormFile.Setup(_ => _.Length).Returns(ms.Length);

            // Act
            var result = await _controller.ProcessFile(_mockFormFile.Object, authorName);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ProcessFile_ReturnsBadRequest_WhenFileIsNull()
        {
            // Arrange
            IFormFile nullFile = null;
            var authorName = "Test Author";

            // Act
            var result = await _controller.ProcessFile(nullFile, authorName);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Файл не загружен", badRequestResult.Value);
        }

        // Additional test for the scenario when ProcessFileAsync throws an exception
    }
}