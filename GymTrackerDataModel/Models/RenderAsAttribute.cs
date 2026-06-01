using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerDataModel.Models
{
    public enum RenderType
    {
        Default,
        Video,
        Link,
        Image
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RenderAsAttribute : Attribute
    {
        public RenderType Type { get; }

        public RenderAsAttribute(RenderType type)
        {
            Type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreInGridAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CollectionAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class NavigationPropertyAttribute : Attribute
    {
        public Type EntityType { get; }

        public string ValueField { get; set; } = "Id";
        public string TextField { get; set; } = "Name";

        public NavigationPropertyAttribute(Type entityType)
        {
            EntityType = entityType;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DetailPropertyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SearchableAttribute : Attribute
    {
    }
}
