using Contracts;
using Entities;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }

        public async Task<PagedList<Company>> GetAllCompaniesAsync(CompanyParameters companyParameters, bool trackChanges)
        {
            var companies = await FindAll(trackChanges)
                                  .OrderBy(c => c.Name)
                                  .ToListAsync();

            return PagedList<Company>.ToPagedList(companies, companyParameters.PageNumber, companyParameters.PageSize);
        }


        public async Task<Company> GetCompanyAsync(Guid companyId, bool trackChanges)
            =>
            await
                FindByCondition(c => c.Id.Equals(companyId), trackChanges)
                .SingleOrDefaultAsync();

        public void CreateCompany(Company company)
            =>
                Create(company);

        public async Task<IEnumerable<Company>> GetCompaniesByIdsAsync(IEnumerable<Guid> companyIds, bool trackChanges)
            =>
            await
                FindByCondition(c => companyIds.Contains(c.Id), trackChanges)
                .ToListAsync();

        public void DeleteCompany(Company company)
            =>
                Delete(company);
    }
}
