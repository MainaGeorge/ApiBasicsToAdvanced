using System.Linq;
using Entities.Models;

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
    }
}
