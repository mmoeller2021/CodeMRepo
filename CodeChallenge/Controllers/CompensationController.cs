using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CodeChallenge.Services;
using CodeChallenge.Models;
using CodeChallenge.Dtos;

namespace CodeChallenge.Controllers
{
    [ApiController]
    [Route("api/compensation")]
    public class CompensationController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IEmployeeService _employeeService;

        public CompensationController(ILogger<EmployeeController> logger, IEmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        //a different resourcce so a different controller, but in business process,
        //made sense to add compensation to employee so added the compensation to that service

        //create compensation
        [HttpPost("employee/{employeeId}", Name = "createCompensation")]
        public IActionResult CreateEmployeeCompensation(String employeeId, [FromBody] CompensationRequestDto compensationRequestDto )
        //dto for compensation salary, effectiveDate, and lookup employeeId and make sure employee exists
        {
            _logger.LogDebug($"Received employee create request for '{employeeId}'");

            var employee = _employeeService.GetById(employeeId);

            if (employee == null)
                return NotFound();

            var compensation = _employeeService.GetCompensation(employeeId);
            //is there a compensation for employee then bad request bc compensation is already there
            if (compensation != null)
                return BadRequest("Compensation already exists for employee");
            //new compensation object
            var compensationModel = new Compensation();
            //map compenseantion request dto to compensation model to create a new compensation per employeeId
            compensationModel.Employee = employee;
            compensationModel.Salary = compensationRequestDto.Salary;
            compensationModel.EffectiveDate = compensationRequestDto.EffectiveDate;

            var result = _employeeService.CreateCompensation(compensationModel);

            return CreatedAtRoute("getCompensation", new { employeeId = employee.EmployeeId }, compensationModel);
        }

        //read compenstation by id
        [HttpGet("employee/{employeeId}", Name = "getCompensation")]
        public IActionResult GetEmployeeCompensation(String employeeId)
        {
            _logger.LogDebug($"Received employee get request for '{employeeId}'");

            var compensation = _employeeService.GetCompensation(employeeId);

            if (compensation == null)
                return NotFound();

            return Ok(compensation);
        }



    }
}
