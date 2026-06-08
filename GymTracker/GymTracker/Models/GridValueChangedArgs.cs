using System.Reflection;

namespace GymTracker.Models
{
    public class GridValueChangedArgs
    {
        public object Item { get; set; }
        public PropertyInfo Property { get; set; }
        public object Value { get; set; }
    }
}
