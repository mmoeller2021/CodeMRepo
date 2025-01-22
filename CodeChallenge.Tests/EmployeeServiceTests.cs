using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeChallenge.Models;
using CodeChallenge.Repositories;
using CodeChallenge.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace CodeCodeChallenge.Tests.Integration
{
    [TestClass]
    public class EmployeeServiceTests
    {
        private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
        private readonly Mock<ICompensationRepository> _mockCompensationRepository;
        private readonly Mock<ILogger<EmployeeService>> _mockLogger;
        private readonly EmployeeService _employeeService;

        public EmployeeServiceTests()
        {
            _mockEmployeeRepository = new Mock<IEmployeeRepository>();
            _mockCompensationRepository = new Mock<ICompensationRepository>();
            _mockLogger = new Mock<ILogger<EmployeeService>>();
            _employeeService = new EmployeeService(_mockLogger.Object, _mockEmployeeRepository.Object, _mockCompensationRepository.Object);
        }

        [Fact]
        [TestMethod]
        public void GetNumberOfReports_ShouldReturnCorrectReportCount()
        {
            // Arrange
            var employeeId = "1";
            var directReports = new List<Employee>
            {
                new Employee { EmployeeId = "2", DirectReports = new List<Employee>() },
                new Employee { EmployeeId = "3", DirectReports = new List<Employee>() }
            };
            var employee = new Employee { EmployeeId = employeeId, DirectReports = directReports };

            _mockEmployeeRepository.Setup(repo => repo.GetById(It.IsAny<string>())).Returns((string id) =>
            {
                return directReports.Find(e => e.EmployeeId == id) ?? employee;
            });

            // Act
            var reportCount = _employeeService.GetReportingStructure(employeeId).NumberOfReports;

            // Assert
            Assert.Equal(2, reportCount);
        }

        //The method adds and saves the compensation when the compensation object is not null.
        //The method returns null and does not attempt to add or save when the compensation object is null.

        [Fact]
        [TestMethod]
        public void CreateCompensation_ShouldAddCompensation_WhenCompensationIsNotNull()
        {
            // Arrange
            var compensation = new Compensation
            {
                Employee = new Employee { EmployeeId = "1" },
                CompensationId = 1,
                Salary = 100000,
                EffectiveDate = DateTime.Now
            };

            _mockCompensationRepository.Setup(repo => repo.Add(It.IsAny<Compensation>())).Returns(compensation);
            _mockCompensationRepository.Setup(repo => repo.SaveAsync()).Returns(Task.CompletedTask);

            // Act
            var result = _employeeService.CreateCompensation(compensation);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(compensation, result);
            _mockCompensationRepository.Verify(repo => repo.Add(It.IsAny<Compensation>()), Times.Once);
            _mockCompensationRepository.Verify(repo => repo.SaveAsync(), Times.Once);
        }

        [Fact]
        [TestMethod]
        public void CreateCompensation_ShouldReturnNull_WhenCompensationIsNull()
        {
            // Act
            var result = _employeeService.CreateCompensation(null);

            // Assert
            Assert.Null(result);
            _mockCompensationRepository.Verify(repo => repo.Add(It.IsAny<Compensation>()), Times.Never);
            _mockCompensationRepository.Verify(repo => repo.SaveAsync(), Times.Never);
        }
    }
}