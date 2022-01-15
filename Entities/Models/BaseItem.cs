using System;

namespace Entities.Models
{
    public abstract class BaseItem
    {
        public abstract Guid Id { get; set; }
        public abstract string Name { get; set; }

    }
}
