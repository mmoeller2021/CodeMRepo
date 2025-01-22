using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeChallenge.Models;
using Microsoft.Extensions.Logging;
using CodeChallenge.Repositories;

namespace CodeChallenge.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICompensationRepository _compensationRepository;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ILogger<EmployeeService> logger, IEmployeeRepository employeeRepository, ICompensationRepository compensationRepository)
        {
            _employeeRepository = employeeRepository;
            _compensationRepository = compensationRepository;
            _logger = logger;
        }

        public Employee Create(Employee employee)
        {
            if (employee != null)
            {
                _employeeRepository.Add(employee);
                _employeeRepository.SaveAsync().Wait();
            }

            return employee;
        }

        public Employee GetById(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                return _employeeRepository.GetById(id);
            }

            return null;
        }

        public Employee Replace(Employee originalEmployee, Employee newEmployee)
        {
            if (originalEmployee != null)
            {
                _employeeRepository.Remove(originalEmployee);
                if (newEmployee != null)
                {
                    // ensure the original has been removed, otherwise EF will complain another entity w/ same id already exists
                    _employeeRepository.SaveAsync().Wait();

                    _employeeRepository.Add(newEmployee);
                    // overwrite the new id with previous employee id
                    newEmployee.EmployeeId = originalEmployee.EmployeeId;
                }
                _employeeRepository.SaveAsync().Wait();
            }

            return newEmployee;
        }

        private int GetNumberOfReports(List<Employee> directReports, int reportCount = 0)
        {
            //check nulls
            if (directReports == null)
            {
                return 0;
            }
            //add to reportcount for every direct report for each employee under the inputed id
            reportCount += directReports.Count;
            //get employee record for each level to get each direct report list 
            directReports.ForEach(employee =>
            {
                var employeeTemp = _employeeRepository.GetById(employee.EmployeeId);

                if (employeeTemp.DirectReports != null && employeeTemp.DirectReports.Any())
                {
                    reportCount = GetNumberOfReports(employee.DirectReports, reportCount);
                }
             
            });
            return reportCount;
        }
    

        public ReportingStructure GetReportingStructure(string id)
        {
            //get the employee by id and create a new report 
            var employeeTemp = _employeeRepository.GetById(id);
            if (employeeTemp != null)
            {
                var reportingStructure = new ReportingStructure();
                reportingStructure.Employee = employeeTemp;
                reportingStructure.NumberOfReports = GetNumberOfReports(employeeTemp.DirectReports);
                return reportingStructure;
            }
            return null;
        }

        //adding compensation to employee as part of employee service, coulld have made separate service but for simplicity added it here
        // i thought compensation was an addition to the employee

        public Compensation CreateCompensation(Compensation compensation)
            //set object and look up employeeId 
        {
            if (compensation != null)
            {
                _compensationRepository.Add(compensation);
                _compensationRepository.SaveAsync().Wait();
            }

            return compensation;
        }

        public Compensation GetCompensation(string employeeId)
        {
            if (!String.IsNullOrEmpty(employeeId))
            {
                return _compensationRepository.GetByEmployeeId(employeeId);
            }

            return null;
        }
    }
}
