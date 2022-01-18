using System;
using System.Collections.Generic;
using System.Linq;
using Contracts;
using Entities.DataTransferObjects;
using Entities.LinkModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;

namespace CompanyEmployees.Utility
{
    public class EmployeeLinks
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IDataShaper<EmployeeDto> _dataSaShaper;

        public EmployeeLinks(LinkGenerator linkGenerator, IDataShaper<EmployeeDto> dataSaShaper)
        {
            _linkGenerator = linkGenerator;
            _dataSaShaper = dataSaShaper;
        }

        public LinkResponse TryGenerateLinks(IEnumerable<EmployeeDto> employeesDtos, string fields, Guid companyId,
            HttpContext httpContext)
        {
            var employeesDto = employeesDtos as EmployeeDto[] ?? employeesDtos.ToArray();
            var shapedEmployees = ShapeData(employeesDto, fields);

            return ShouldGenerateLinks(httpContext) ?
                ReturnLinkedEmployees(employeesDto, fields, companyId, httpContext, shapedEmployees)
                : ReturnShapedData(shapedEmployees);
        }

        private static LinkResponse ReturnShapedData(List<Entity> shapedEmployees)
        {
            return new LinkResponse { ShapedEntities = shapedEmployees };
        }

        private LinkResponse ReturnLinkedEmployees(IReadOnlyList<EmployeeDto> employeesDtos, string fields, Guid companyId,
            HttpContext httpContext, List<Entity> shapedEmployees)
        {
            for (var index = 0; index < employeesDtos.Count; index++)
            {
                var employeeLinks = CreateLinksForEmployee(httpContext, companyId, employeesDtos[index].Id, fields);
                shapedEmployees[index].Add("Links", employeeLinks);
            }

            var employeeCollection = new LinkCollectionWrapper<Entity>(shapedEmployees);
            var linkedEmployees = CreateLinksForEmployees(httpContext, employeeCollection);

            return new LinkResponse { HasLinks = true, LinkedEntities = linkedEmployees };
        }

        private List<Link> CreateLinksForEmployee(HttpContext httpContext, Guid companyId, Guid employeeId, string fields = "")
        {
            return new List<Link>()
            {
                new Link(_linkGenerator.GetUriByAction(httpContext, "GetEmployeeForCompany", values: new {companyId, employeeId, fields}), "self", "GET"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "DeleteEmployeeForCompany", values: new {companyId, employeeId}), "delete_employee", "DELETE"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "UpdateEmployeeForCompany", values: new {companyId, employeeId}), "update_employee", "PUT"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "PartiallyUpdateEmployeeForCompany", values: new {companyId, employeeId}), "partially_update_employee", "PATCH"),

            };
        }

        private LinkCollectionWrapper<Entity> CreateLinksForEmployees(HttpContext httpContext, LinkCollectionWrapper<Entity> employeeCollection)
        {
            employeeCollection.Links.Add(new Link(_linkGenerator.GetUriByAction(httpContext,
                    "GetEmployeesForCompany", values: new { }),
                "self",
                "GET"));

            return employeeCollection;

        }

        private static bool ShouldGenerateLinks(HttpContext httpContext)
        {
            var mediaType = (MediaTypeHeaderValue)httpContext.Items["AcceptHeaderMediaType"];
            return mediaType is not null && mediaType.SubTypeWithoutSuffix.EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);
        }


        private List<Entity> ShapeData(IEnumerable<EmployeeDto> employeesDto, string fields) =>
            _dataSaShaper.ShapeData(employeesDto, fields)
                .Select(e => e.Entity)
                .ToList();
    }
}
