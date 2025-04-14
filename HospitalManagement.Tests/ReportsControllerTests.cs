using Moq;
using Xunit;
using HospitalManagement.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace HospitalManagement.Tests
{
    public class ReportsControllerTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly ReportsController _controller;

        public ReportsControllerTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(config => config.GetConnectionString("HospitalDB")).Returns("YourConnectionString");

            _controller = new ReportsController(_mockConfig.Object);
        }

        [Fact]
        public void StaffWorkload_ReturnsViewResult_WithDataTable()
        {
            var result = _controller.StaffWorkload();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<DataTable>(viewResult.Model);
            Assert.NotNull(model);
        }

        [Fact]
        public void DepartmentPerformance_ReturnsViewResult_WithDataTable()
        {
            var result = _controller.DepartmentPerformance();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<DataTable>(viewResult.Model);
            Assert.NotNull(model);
        }
    }
}