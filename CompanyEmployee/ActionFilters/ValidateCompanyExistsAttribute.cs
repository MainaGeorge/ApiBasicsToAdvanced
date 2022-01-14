using System;
using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompanyEmployees.ActionFilters
{
    public class ValidateCompanyExistsAttribute : ValidateExists, IAsyncActionFilter
    {

        public ValidateCompanyExistsAttribute(ILoggerManager logger, IRepositoryManager repo) : base(logger, repo)
        {
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            var companyId = GetId(context, "companyId");
            var trackChanges = TrackChanges(context);
            var company = await Repo.Company.GetCompany(companyId, trackChanges);

            if (company is null)
            {
                Logger.LogInfo($"company with id {companyId} does not exist");
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
