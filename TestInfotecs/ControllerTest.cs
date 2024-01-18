
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Infotecs.Source.Controllers;
using Infotecs.Source.Services;
using Infotecs.Source.Data.Models;
namespace TestInfotecs
{
    public class ExperimentControllerTests
    {
        private readonly Mock<IExperimentService> _mockExperimentService;
        private readonly ExperimentController _controller;
        private readonly Mock<IFormFile> _mockFormFile;

        public ExperimentControllerTests()
        {
            _mockExperimentService = new Mock<IExperimentService>();
            _controller = new ExperimentController(_mockExperimentService.Object);
            _mockFormFile = new Mock<IFormFile>();
        }

        [Fact]
        public async Task ProcessFile_ReturnsBadRequest_WhenFileIsNull()
        {
            IFormFile? nullFile = null;
            var authorName = "Test Author";
            var result = await _controller.ProcessFile(nullFile, authorName);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Файл не загружен", badRequestResult.Value);
        }
        [Fact]
        public async Task GetResultsByQueryParams_ReturnsBadRequest_ForNoParameters()
        {
            var result = await _controller.GetResultsByQueryParams(null, null, null, null, null);
            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public async Task GetResultsByQueryParams_ReturnsBadRequest_ForRange()
        {
            var result = await _controller.GetResultsByQueryParams(null, 1, null, null, null);
            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public async Task GetResultsByQueryParams_ReturnsNotFound_WhenNoResults()
        {
            _mockExperimentService.Setup(s => s.GetResultsByQueryParams(It.IsAny<string>(), It.IsAny<double?>(), It.IsAny<double?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                                    .ReturnsAsync(new List<Result>());
            var result = await _controller.GetResultsByQueryParams("testname.csv", 1, 2, 1, 2);
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
       
 }
