using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompanyEmployees.ActionFilters
{
    public class ValidateEmployeeExistsAttribute : ValidateExists, IAsyncActionFilter

    {
        public ValidateEmployeeExistsAttribute(ILoggerManager logger, IRepositoryManager repo) : base(logger, repo)
        {
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var employeeId = GetId(context, "employeeId");
            var companyId = GetId(context, "companyId");
            var trackChanges = TrackChanges(context);

            var company = await Repo.Company.GetCompany(companyId, false);
            if (company is null)
            {
                Logger.LogInfo($"company with id {companyId} does not exist");
                context.Result = new NotFoundResult();
                return;
            }

            var employee = await Repo.Employee.GetEmployeeAsync(companyId, employeeId, trackChanges);
            if (employee is null)
            {
                Logger.LogInfo($"Employee with id: {employeeId} doesn't exist in the database.");
                context.Result = new NotFoundResult();
            }
            else
            {
                context.HttpContext.Items.Add("employee", employee);
                await next();
            }

        }
    }
}
