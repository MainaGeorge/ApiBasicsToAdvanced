using System.Collections.Generic;
using Entities.Models;

namespace Contracts
{
    public interface IDataShaper<in T>
    {
        IEnumerable<ShapedEntity> ShapeData(IEnumerable<T> entities, string fieldsString);
        ShapedEntity ShapeData(T entity, string fieldsString);
    }
}
