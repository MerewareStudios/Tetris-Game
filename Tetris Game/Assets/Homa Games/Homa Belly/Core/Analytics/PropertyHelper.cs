using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace HomaGames.HomaBelly
{
    public class PropertyHelper
    {
        private static readonly ConcurrentDictionary<Type, PropertyHelper[]> Cache
            = new ConcurrentDictionary<Type, PropertyHelper[]>();
        public string Name { get; set; }
        public Func<object, object> Getter { get; set; }
        public Type DeclaringType { get; set; }
        public static PropertyHelper[] GetProperties(Type type)
            => Cache
                .GetOrAdd(type, _ => type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                    .Select(property =>
                    {
                        var declaringClass = property.DeclaringType;
                        var getMethod = property.GetMethod;
                        return new PropertyHelper
                        {
                            Name = property.Name,
                            Getter = o => getMethod.Invoke(o, Array.Empty<object>()),
                            DeclaringType = declaringClass
                        };
                    })
                    .ToArray());
    }
}