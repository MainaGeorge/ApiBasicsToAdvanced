using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Entities.Models;
using System.Linq.Dynamic.Core;

namespace Repository.QueryExtensions
{
    public static class QuerySearchingAndFiltering
    {
        public static IQueryable<Employee> SearchEmployeeByName(this IQueryable<Employee> source, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return source;

            name = name.Trim().ToLower();
            return source.Where(e => e.Name.ToLower().Contains(name));

        }

        public static IQueryable<Company> SearchCompanyByName(this IQueryable<Company> source, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return source;

            name = name.Trim().ToLower();
            return source.Where(e => e.Name.ToLower().Contains(name));
        }
        public static IQueryable<Employee> FilterByAge(this IQueryable<Employee> source,
            uint minAge, uint maxAge)
        {
            return source.Where(e => e.Age > minAge && e.Age < maxAge);
        }

        public static IQueryable<Employee> OrderByGivenProperties(this IQueryable<Employee> source, string propertyNames)
        {
            if (string.IsNullOrWhiteSpace(propertyNames))
                return source;

            //expected format --> name desc, age asc
            var orderParameters = propertyNames.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var propertyInfos = typeof(Employee).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var orderByStringBuilder = new StringBuilder();

            foreach (var parameter in orderParameters)
            {
                var propertyName = parameter.Trim().Split(' ')[0];

                var objectProperty = propertyInfos.FirstOrDefault(pi =>
                    pi.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));

                if (objectProperty is null) continue;

                var direction = parameter.EndsWith(" desc") ? "descending" : "ascending";
                orderByStringBuilder.Append($"{objectProperty.Name} {direction}, ");
            }

            var orderByString = orderByStringBuilder.ToString().Trim(' ', ',');

            return string.IsNullOrWhiteSpace(orderByString) ? source : source.OrderBy(orderByString);
        }
    }
}
