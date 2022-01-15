using System.Linq;
using Entities.Models;
using System.Linq.Dynamic.Core;
using Repository.QueryExtensions.CommonSortingLogic;

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
                return source.OrderBy(e => e.Name);

            //expected format --> name desc, age asc
            var orderByString = OrderQueryBuilder.CreateOrderQuery<Employee>(propertyNames);

            return string.IsNullOrWhiteSpace(orderByString) ? source.OrderBy(e => e.Name) : source.OrderBy(orderByString);
        }

        public static IQueryable<Company> OrderByGivenProperties(this IQueryable<Company> source, string propertyNames)
        {
            if (string.IsNullOrWhiteSpace(propertyNames))
                return source.OrderBy(e => e.Name);

            //expected format --> name desc, age asc
            var orderByString = OrderQueryBuilder.CreateOrderQuery<Company>(propertyNames);

            return string.IsNullOrWhiteSpace(orderByString) ? source.OrderBy(e => e.Name) : source.OrderBy(orderByString);
        }
    }
}
