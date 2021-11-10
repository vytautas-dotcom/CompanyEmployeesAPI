using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies/{companyId}/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _loggerManager;
        private readonly IMapper _mapper;

        public EmployeesController(IRepositoryManager repositoryManager, ILoggerManager loggerManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _loggerManager = loggerManager;
            _mapper = mapper;
        }
        [HttpGet]
        public IActionResult GetEmployeesForCompany(Guid companyId)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            var employees = _repositoryManager.Employee.GetEmployees(company.Id, false);
            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employees);
            return Ok(employeesDto);
        }
        [HttpGet("{employeeId}", Name = nameof(GetEmployeeForCompany))]
        public IActionResult GetEmployeeForCompany(Guid companyId, Guid employeeId)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            var employee = _repositoryManager.Employee.GetEmployee(company.Id, employeeId, false);
            if (employee == null)
            {
                _loggerManager.LogInfo($"Employee with id: {employeeId} doesn't exist in the database.");
                return NotFound();
            }
            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            return Ok(employeeDto);
        }
        [HttpPost]
        public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
        {
            if (employee == null)
            {
                _loggerManager.LogError("EmployeeForCreationDto object sent from client is null.");
                return BadRequest("EmployeeForCreationDto object is null");
            }

            if (!ModelState.IsValid)
            {
                _loggerManager.LogError("Invalid model state for the EmployeeForCreationDto object");
                return UnprocessableEntity(ModelState);
            }

            var company = _repositoryManager.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeEntity = _mapper.Map<Employee>(employee);

            _repositoryManager.Employee.CreateEmployeeForCompany(company.Id, employeeEntity);
            _repositoryManager.Save();

            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);

            return CreatedAtRoute(nameof(GetEmployeeForCompany), new
            {
                companyId = company.Id,
                employeeId = employeeToReturn.Id
            }, employeeToReturn);
        }

        [HttpDelete("{employeeId}")]
        public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid employeeId)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            var employee = _repositoryManager.Employee.GetEmployee(company.Id, employeeId, false);
            if (employee == null)
            {
                _loggerManager.LogInfo($"Employee with id: {employeeId} doesn't exist in the database.");
                return NotFound();
            }
            _repositoryManager.Employee.DeleteEmployee(employee);
            _repositoryManager.Save();

            return NoContent();
        }
        [HttpPut("{employeeId}")]
        public IActionResult UpdateEmployeeForCompany(Guid companyId, 
                                                      Guid employeeId, 
                                                      [FromBody] EmployeeForUpdateDto employee)
        {
            if (employee == null)
            {
                _loggerManager.LogError("EmployeeForUpdateDto object sent from client is null.");
                return BadRequest("EmployeeForUpdateDto object is null");
            }

            if (!ModelState.IsValid)
            {
                _loggerManager.LogError("Invalid model state for the EmployeeForUpdateDto object");
                return UnprocessableEntity(ModelState);
            }

            var company = _repositoryManager.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            var employeeFromDb = _repositoryManager.Employee.GetEmployee(company.Id, employeeId, true);
            if (employeeFromDb == null)
            {
                _loggerManager.LogInfo($"Employee with id: {employeeId} doesn't exist in the database.");
                return NotFound();
            }
            _mapper.Map(employee, employeeFromDb);
            _repositoryManager.Save();

            return NoContent();
        }
        [HttpPatch("{employeeId}")]
        public IActionResult PartiallyUpdateEmployeeForCompany(Guid companyId, Guid employeeId,
                                                               [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patch)
        {
            if (patch == null)
            {
                _loggerManager.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }
            var company = _repositoryManager.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            var employee = _repositoryManager.Employee.GetEmployee(company.Id, employeeId, true);
            if (employee == null)
            {
                _loggerManager.LogInfo($"Employee with id: {employeeId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employee);

            patch.ApplyTo(employeeToPatch, ModelState);

            TryValidateModel(employeeToPatch);

            if (!ModelState.IsValid)
            {
                _loggerManager.LogError("Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(employeeToPatch, employee);

            _repositoryManager.Save();

            return NoContent();
        }
    }
}
