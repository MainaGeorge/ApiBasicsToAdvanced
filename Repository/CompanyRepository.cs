using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Entities;
using Entities.Models;
using Entities.Paging;
using Entities.RequestParameters;
using Microsoft.EntityFrameworkCore;
using Repository.QueryExtensions;

namespace Repository
{
    public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(RepositoryContext context) : base(context)
        {
        }

        public async Task<PagedList<Company>> GetAllCompanies(CompanyRequestParameter parameter, bool trackChanges)
        {
            var companies = await FindAll(trackChanges)
                .SearchCompanyByName(parameter.SearchTerm)
                .OrderBy(c => c.Name)
                .Skip((parameter.PageNumber - 1) * parameter.PageSize)
                .Take(parameter.PageSize)
                .ToListAsync();

            var count = await FindAll(trackChanges).CountAsync();

            return PagedList<Company>.ToPagedList(companies, parameter.PageNumber, parameter.PageSize, count);
        }

        public async Task<Company> GetCompany(Guid companyId, bool trackChanges)
        {
            return await FindByCondition(c => c.Id == companyId, trackChanges)
                .SingleOrDefaultAsync();
        }

        public void CreateCompany(Company company)
        {
            Create(company);
        }

        public async Task<IEnumerable<Company>> GetByIds(IEnumerable<Guid> companyIds, bool trackChanges)
        {
            return await FindByCondition(x => companyIds.Contains(x.Id), trackChanges).ToListAsync();
        }

        public void DeleteCompany(Company company)
        {
            Delete(company);
        }
    }
}