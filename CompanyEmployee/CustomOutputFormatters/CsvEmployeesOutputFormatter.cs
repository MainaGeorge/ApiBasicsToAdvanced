using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace CompanyEmployees.CustomOutputFormatters
{
    public class CsvEmployeesOutputFormatter : TextOutputFormatter
    {
        public CsvEmployeesOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type)
        {
            if (typeof(EmployeeDto).IsAssignableFrom(type) || typeof(IEnumerable<EmployeeDto>).IsAssignableFrom(type))
            {
                return base.CanWriteType(type);
            }

            return false;
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            var buffer = new StringBuilder();

            if (context.Object is IEnumerable<EmployeeDto> dtos)
            {
                foreach (var companyDto in dtos)
                {
                    FormatCsv(buffer, companyDto);
                }
            }
            else
            {
                FormatCsv(buffer, (EmployeeDto)context.Object);
            }

            await response.WriteAsync(buffer.ToString());
        }
        private static void FormatCsv(StringBuilder buffer, EmployeeDto employeeDto)
        {
            buffer.AppendLine($"{employeeDto.Id}, {employeeDto.Name}, {employeeDto.Position}");
        }
    }
}
