using System.Reflection;

namespace GymTracker.Models
{
    public enum GridColumnType
    {
        String,
        Int,
        Decimal,
        Navigation
    }
    public class GridColumnDefinition
    {
        public PropertyInfo? Property { get; set; }
        public string? Title { get; set; }
        public GridColumnType Type { get; set; }
        public bool IsDetail { get; set; }
    }
}
