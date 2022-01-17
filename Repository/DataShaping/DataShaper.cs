using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Contracts;

namespace Repository.DataShaping
{
    public class DataShaper<T> : IDataShaper<T> where T : class
    {
        public PropertyInfo[] Properties { get; set; }
        public DataShaper()
        {
            Properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }
        public IEnumerable<ExpandoObject> ShapeData(IEnumerable<T> entities, string fieldsString)
        {
            var requiredProperties = GetRequiredProperties(fieldsString);
            return FetchData(entities, requiredProperties);
        }

        public ExpandoObject ShapeData(T entity, string fieldsString)
        {
            var requiredProperties = GetRequiredProperties(fieldsString);
            return FetchDataForEntity(entity, requiredProperties);

        }

        private IEnumerable<PropertyInfo> GetRequiredProperties(string fieldsString)
        {
            //expected field string --> name,age,id...
            if (string.IsNullOrWhiteSpace(fieldsString)) return Properties;

            return fieldsString
                      .Split(",", StringSplitOptions.RemoveEmptyEntries)
                      .Select(field => Properties.FirstOrDefault(p => p.Name.Equals(field.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                      .Where(property => property is not null)
                      .ToArray();
        }

        private static ExpandoObject FetchDataForEntity(T entity, IEnumerable<PropertyInfo> requiredProperties)
        {
            var shapedObject = new ExpandoObject();
            foreach (var prop in requiredProperties)
            {
                var propValue = prop.GetValue(entity);
                shapedObject.TryAdd(prop.Name, propValue);
            }

            return shapedObject;
        }

        private static IEnumerable<ExpandoObject> FetchData(IEnumerable<T> entities,
            IEnumerable<PropertyInfo> requiredProperties)
        {
            return entities.Select(entity => FetchDataForEntity(entity, requiredProperties)).ToList();
        }
    }
}
