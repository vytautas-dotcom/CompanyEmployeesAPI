using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.ModelBinders;
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
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    //[ResponseCache(CacheProfileName = "Duration-90seconds")]
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
        [HttpGet(Name = "GetCompanies")]
        public async Task<IActionResult> GetCompanies([FromQuery] CompanyParameters companyParameters)
        {
            var companies = await _repositoryManager.Company.GetAllCompaniesAsync(companyParameters, false);

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(companies.MetaData));

            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            return Ok(companiesDto);
        }
        [HttpGet("{companyId}", Name = "CompanyById")]
        //[ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetCompany(Guid companyId)
        {
            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, false);
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
        public async Task<IActionResult> GetCompanyColletion([ModelBinder(BinderType = typeof(ArrayModelBinder))]
                                                 IEnumerable<Guid> companyIds)
        {
            if (companyIds == null)
            {
                _loggerManager.LogError("Parameter ids is null");
                return BadRequest("Parameter ids is null");
            }
            var companies = await _repositoryManager.Company.GetCompaniesByIdsAsync(companyIds, false);

            if (companyIds.Count() != companies.Count())
            {
                _loggerManager.LogError("Some ids are not valid in a collection");
                return NotFound();
            }

            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            return Ok(companiesToReturn);
        }

        [HttpPost(Name = "CreateCompany")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
        {

            var companyEntity = _mapper.Map<Company>(company);

            _repositoryManager.Company.CreateCompany(companyEntity);
            await _repositoryManager.SaveAsync();

            var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);

            return CreatedAtRoute("CompanyById", new { companyId = companyToReturn.Id }, companyToReturn);
        }

        [HttpPost("collection")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCompanyColletion([FromBody] IEnumerable<CompanyForCreationDto> companies)
        {

            var companyEntities = _mapper.Map<IEnumerable<Company>>(companies);

            foreach (var company in companyEntities)
            {
                _repositoryManager.Company.CreateCompany(company);
            }
            await _repositoryManager.SaveAsync();

            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);

            var companyIds = string.Join(',', companiesToReturn.Select(c => c.Id));

            return CreatedAtRoute("CompanyCollection", new { companyIds }, companiesToReturn);
        }
        [HttpDelete("{companyId}")]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteCompany(Guid companyId)
        {
            var company = HttpContext.Items["company"] as Company;

            _repositoryManager.Company.DeleteCompany(company);
            await _repositoryManager.SaveAsync();

            return NoContent();
        }
        [HttpPut("{companyId}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateCompany(Guid companyId, [FromBody] CompanyForUpdateDto company)
        {

            var companyFromDb = HttpContext.Items["company"] as Company;

            _mapper.Map(company, companyFromDb);
            await _repositoryManager.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{companyId}")]
        public async Task<IActionResult> PartiallyUpdateCompany(Guid companyId, JsonPatchDocument<CompanyForUpdateDto> patch)
        {
            if (patch == null)
            {
                _loggerManager.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }
            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, true);
            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }
            var companyToPatch = _mapper.Map<CompanyForUpdateDto>(company);

            patch.ApplyTo(companyToPatch, ModelState);

            TryValidateModel(companyToPatch);

            if (!ModelState.IsValid)
            {
                _loggerManager.LogError("Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(companyToPatch, company);

            await _repositoryManager.SaveAsync();

            return NoContent();
        }
        [HttpOptions]
        public IActionResult GetCompaniesOptions()
        {
            Response.Headers.Add("Allow", "GET, OPTIONS, POST");
            return Ok();
        }
    }
}
