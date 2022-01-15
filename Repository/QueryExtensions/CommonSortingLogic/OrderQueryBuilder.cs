using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Repository.QueryExtensions.CommonSortingLogic
{
    public static class OrderQueryBuilder
    {
        public static string CreateOrderQuery<T>(string propertyNames)
        {
            var orderParameters = propertyNames.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var propertyInfos = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

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

            return orderByStringBuilder.ToString().Trim(' ', ',');
        }
    }
}
