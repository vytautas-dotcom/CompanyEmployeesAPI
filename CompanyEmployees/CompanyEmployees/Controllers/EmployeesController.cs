using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, 
                                                                [FromQuery] EmployeeParameters employeeParameters)
        {
            if (!employeeParameters.ValidAgeRange)
                return BadRequest("Max age can't be less than min age");

            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, false);
            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            var employees = await _repositoryManager.Employee.GetEmployeesAsync(company.Id, employeeParameters, false);

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(employees.MetaData));

            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employees);
            return Ok(employeesDto);
        }
        [HttpGet("{employeeId}", Name = nameof(GetEmployeeForCompany))]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid employeeId)
        {
            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, false);
            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            var employee = await _repositoryManager.Employee.GetEmployeeAsync(company.Id, employeeId, false);
            if (employee == null)
            {
                _loggerManager.LogInfo($"Employee with id: {employeeId} doesn't exist in the database.");
                return NotFound();
            }
            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            return Ok(employeeDto);
        }
        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
        {

            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, false);
            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeEntity = _mapper.Map<Employee>(employee);

            _repositoryManager.Employee.CreateEmployeeForCompany(company.Id, employeeEntity);
            await _repositoryManager.SaveAsync();

            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);

            return CreatedAtRoute(nameof(GetEmployeeForCompany), new
            {
                companyId = company.Id,
                employeeId = employeeToReturn.Id
            }, employeeToReturn);
        }

        [HttpDelete("{employeeId}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid employeeId)
        {
            var employee = HttpContext.Items["employee"] as Employee;

            _repositoryManager.Employee.DeleteEmployee(employee);
            await _repositoryManager.SaveAsync();

            return NoContent();
        }
        [HttpPut("{employeeId}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]

        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, 
                                                      Guid employeeId, 
                                                      [FromBody] EmployeeForUpdateDto employee)
        {

            var employeeFromDb = HttpContext.Items["employee"] as Employee;

            _mapper.Map(employee, employeeFromDb);
            await _repositoryManager.SaveAsync();

            return NoContent();
        }
        [HttpPatch("{employeeId}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid employeeId,
                                                               [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patch)
        {
            if (patch == null)
            {
                _loggerManager.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }

            var employee = HttpContext.Items["employee"] as Employee;

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employee);

            patch.ApplyTo(employeeToPatch, ModelState);

            TryValidateModel(employeeToPatch);

            if (!ModelState.IsValid)
            {
                _loggerManager.LogError("Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(employeeToPatch, employee);

            await _repositoryManager.SaveAsync();

            return NoContent();
        }
    }
}
