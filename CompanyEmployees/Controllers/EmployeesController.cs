using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public EmployeesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public IActionResult GetEmployees(Guid companyId)
        {
            var company = _repository.Company.GetCompany(companyId, false);

            if (company != null)
            {
                var employeesFromDb = _repository.Employee.GetEmployees(company.Id, false);
                var employeeDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
                return Ok(employeeDto);
            }
            else
            {
                _logger.LogError($"Company with id : {companyId} does not exist in the database");
                return NotFound();
            }
        }

        [HttpGet("{id}", Name = "GetEmployeeForCompany")]
        public IActionResult GetEmployee(Guid companyId, Guid id)
        {
            var company = _repository.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _logger.LogError($"Company with id {companyId} was not found");
                return NotFound();
            }
            var employee = _repository.Employee.GetEmployee(companyId, id, false);

            if (employee == null)
            {
                _logger.LogError($"Unable to find the employee with the provided id {id}");
                return NotFound();
            }
            else
            {
                var employeeDto = _mapper.Map<EmployeeDto>(employee);
                return Ok(employeeDto);
            }
        }

        [HttpPost]
        public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeCreationDto employee)
        {
            if (employee == null)
            {
                _logger.LogError($"EmployeeCreationDto object sent from client is null");
                return BadRequest("EmployeeCreationDtoObject is null");
            }

            var company = _repository.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _logger.LogError($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeEntity = _mapper.Map<Employee>(employee);

            _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            _repository.Save();

            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);

            return CreatedAtRoute("GetEmployeeForCompany", new {companyId, id = employeeToReturn.Id}, employeeToReturn);


        }
    }
}