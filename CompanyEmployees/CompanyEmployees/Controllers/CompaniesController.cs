﻿using AutoMapper;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ILoggerManager _loggerManager;
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;

        public CompaniesController(ILoggerManager loggerManager,
                                   IRepositoryManager repositoryManager,
                                   IMapper mapper)
        {
            _loggerManager = loggerManager;
            _repositoryManager = repositoryManager;
            _mapper = mapper;
        }
        [HttpGet]
        public IActionResult GetCompanies()
        {
            var companies = _repositoryManager.Company.GetAllCompanies(false);
            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            return Ok(companiesDto);
        }
        [HttpGet("{companyId}", Name = "CompanyById")]
        public IActionResult GetCompany(Guid companyId)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} doesn't exist in the database");
                return NotFound();
            }
            else
            {
                var companyDto = _mapper.Map<CompanyDto>(company);
                return Ok(companyDto);
            }
        }

        [HttpGet("collection/{companyIds}", Name = "CompanyCollection")]
        public IActionResult GetCompanyColletion([ModelBinder(BinderType = typeof(ArrayModelBinder))]
                                                 IEnumerable<Guid> companyIds)
        {
            if (companyIds == null)
            {
                _loggerManager.LogError("Parameter ids is null");
                return BadRequest("Parameter ids is null");
            }
            var companies = _repositoryManager.Company.GetCompaniesByIds(companyIds, false);

            if (companyIds.Count() != companies.Count())
            {
                _loggerManager.LogError("Some ids are not valid in a collection");
                return NotFound();
            }

            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            return Ok(companiesToReturn);
        }

        [HttpPost]
        public IActionResult CreateCompany([FromBody] CompanyForCreationDto company)
        {
            if (company == null)
            {
                _loggerManager.LogError("CompanyForCreationDto object sent from client is null.");
                return BadRequest("CompanyForCreationDto object is null");
            }
            var companyEntity = _mapper.Map<Company>(company);

            _repositoryManager.Company.CreateCompany(companyEntity);
            _repositoryManager.Save();

            var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);

            return CreatedAtRoute("CompanyById", new { companyId = companyToReturn.Id }, companyToReturn);
        }

        [HttpPost("collection")]
        public IActionResult CreateCompanyColletion([FromBody] IEnumerable<CompanyForCreationDto> companies)
        {
            if (companies == null)
            {
                _loggerManager.LogError("Company collection sent from client is null.");
                return BadRequest("Company collection is null");
            }
            var companyEntities = _mapper.Map<IEnumerable<Company>>(companies);

            foreach (var company in companyEntities)
            {
                _repositoryManager.Company.CreateCompany(company);
            }
            _repositoryManager.Save();

            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);

            var companyIds = string.Join(',', companiesToReturn.Select(c => c.Id));

            return CreatedAtRoute("CompanyCollection", new { companyIds }, companiesToReturn);
        }
        [HttpDelete("{companyId}")]
        public IActionResult DeleteCompany(Guid companyId)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, false);
            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            _repositoryManager.Company.DeleteCompany(company);
            _repositoryManager.Save();

            return NoContent();
        }
        [HttpPut("{companyId}")]
        public IActionResult UpdateCompany(Guid companyId, [FromBody] CompanyForUpdateDto company)
        {
            if (company == null)
            {
                _loggerManager.LogError("CompanyForUpdateDto object sent from client is null.");
                return BadRequest("CompanyForUpdateDto object is null");
            }
            var companyFromDb = _repositoryManager.Company.GetCompany(companyId, true);
            if (companyFromDb == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            _mapper.Map(company, companyFromDb);
            _repositoryManager.Save();

            return NoContent();
        }
    }
}
