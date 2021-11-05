using Contracts;
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

        public CompaniesController(ILoggerManager loggerManager, IRepositoryManager repositoryManager)
        {
            _loggerManager = loggerManager;
            _repositoryManager = repositoryManager;
        }
        [HttpGet]
        public IActionResult GetCompanies()
        {
            try
            {
                var companies = _repositoryManager.Company.GetAllCompanies(false);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"Something went wrong in the ${nameof(GetCompanies)} action {ex}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
