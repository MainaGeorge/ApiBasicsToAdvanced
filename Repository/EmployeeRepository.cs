using System;
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
    public class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(RepositoryContext context) : base(context)
        {
        }

        public async Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeRequestParameters requestParameters,
            bool trackChanges)
        {

            var employees = await FindByCondition(
                    e => e.CompanyId == companyId, trackChanges)
                .FilterByAge(requestParameters.MinAge, requestParameters.MaxAge)
                .SearchEmployeeByName(requestParameters.SearchTerm)
                .OrderByGivenProperties(requestParameters.OrderBy)
                .Skip((requestParameters.PageNumber - 1) * requestParameters.PageSize)
                .Take(requestParameters.PageSize)
                .ToListAsync();

            var count = await FindByCondition(e => e.CompanyId == companyId, trackChanges).CountAsync();

            return PagedList<Employee>.ToPagedList(employees, requestParameters.PageNumber, requestParameters.PageSize, count);
        }

        public async Task<Employee> GetEmployeeAsync(Guid companyId, Guid employeeId, bool trackChanges)
        {
            return await FindByCondition(e => e.CompanyId == companyId && e.Id == employeeId, trackChanges)
                .SingleOrDefaultAsync();
        }

        public void CreateEmployeeForCompany(Guid companyId, Employee employee)
        {
            employee.CompanyId = companyId;
            Create(employee);
        }

        public void DeleteEmployee(Employee employee)
        {
            Delete(employee);
        }
    }
}
