using System;
using System.Threading.Tasks;
using Entities.Models;
using Entities.Paging;

namespace Contracts
{
    public interface IEmployeeRepository
    {
        Task<PagedList<Employee>> GetEmployeesAsync(Guid companyId, EmployeeRequestParameters requestParameters,
            bool trackChanges);
        Task<Employee> GetEmployeeAsync(Guid companyId, Guid employeeId, bool trackChanges);
        void CreateEmployeeForCompany(Guid companyId, Employee employee);
        void DeleteEmployee(Employee employee);
    }
}
