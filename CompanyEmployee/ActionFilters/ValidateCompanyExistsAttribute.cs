using System;
using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompanyEmployees.ActionFilters
{
    public class ValidateCompanyExistsAttribute : IAsyncActionFilter
    {
        private readonly ILoggerManager _logger;
        private readonly IRepositoryManager _repo;

        public ValidateCompanyExistsAttribute(ILoggerManager logger, IRepositoryManager repo)
        {
            _logger = logger;
            _repo = repo;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpMethod = context.HttpContext.Request.Method;
            var trackChanges =
                httpMethod.Equals("PUT", StringComparison.InvariantCultureIgnoreCase)
                || httpMethod.Equals("PATCH", StringComparison.InvariantCultureIgnoreCase);

            var companyId = (Guid)context.ActionArguments["companyId"];

            var company = await _repo.Company.GetCompany(companyId, trackChanges);

            if (company is null)
            {
                _logger.LogInfo($"company with id {companyId} does not exist");
                context.Result = new NotFoundResult();
            }
            else
            {
                context.HttpContext.Items.Add("company", company);
                await next();
            }
        }
    }
}
