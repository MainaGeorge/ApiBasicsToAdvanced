using System.Linq;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompanyEmployees.ActionFilters
{
    public class ValidateModelState : IActionFilter
    {
        private readonly ILoggerManager _logger;

        public ValidateModelState(ILoggerManager logger)
        {
            _logger = logger;
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var action = context.RouteData.Values["action"];
            var controller = context.RouteData.Values["controller"];

            var passedDto = context.ActionArguments
                .Values;

            if (!passedDto.Any())
            {
                _logger.LogError($"no object was passed in the action {action} of the controller{controller}");
                context.Result = new BadRequestObjectResult($"no object was passed to the action {action}");
            }

            if (passedDto.SingleOrDefault(x => x.ToString()!.Contains("Dto")) is null)
            {
                _logger.LogError($"object sent by the client is null. Controller {controller}, action {action}");
                context.Result = new BadRequestObjectResult($"object is null. controller {controller}, action {action}");
                return;
            }

            if (context.ModelState.IsValid) return;

            _logger.LogError($"Invalid model state for the object. Controller: {controller}, action: {action}");
            context.Result = new UnprocessableEntityObjectResult(context.ModelState);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
