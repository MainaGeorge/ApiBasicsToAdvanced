using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Entities.Paging;
using Entities.RequestParameters;

namespace Contracts
{
    public interface ICompanyRepository
    {
        Task<PagedList<Company>> GetAllCompanies(CompanyRequestParameter parameter, bool trackChanges);
        Task<Company> GetCompany(Guid companyId, bool trackChanges);
        void CreateCompany(Company company);
        Task<IEnumerable<Company>> GetByIds(IEnumerable<Guid> companyIds, bool trackChanges);
        void DeleteCompany(Company company);
    }
}