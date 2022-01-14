using System;
using Contracts;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompanyEmployees.ActionFilters
{
    public abstract class ValidateExists
    {
        protected readonly ILoggerManager Logger;
        protected readonly IRepositoryManager Repo;

        protected ValidateExists(ILoggerManager logger, IRepositoryManager repo)
        {
            Logger = logger;
            Repo = repo;
        }

        protected static Guid GetId(ActionExecutingContext context, string entityType)
        {
            return (Guid)context.ActionArguments[entityType];
        }

        protected static bool TrackChanges(ActionExecutingContext context)
        {
            var httpMethod = context.HttpContext.Request.Method;
            var trackChanges =
                httpMethod.Equals("PUT", StringComparison.InvariantCultureIgnoreCase)
                || httpMethod.Equals("PATCH", StringComparison.InvariantCultureIgnoreCase);
            return trackChanges;
        }
    }
}
