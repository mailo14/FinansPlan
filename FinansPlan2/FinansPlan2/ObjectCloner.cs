using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace FinansPlan2
{
    public static class ObjectCloner
    {
        /*// <summary>
     /// Perform a deep Copy of the object.
     /// </summary>
     /// <typeparam name="T">The type of object being copied.</typeparam>
     /// <param name="source">The object instance to copy.</param>
     /// <returns>The copied object.</returns>
        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(source));
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }*/

        public static T DeepClone<T>(this T source)
        {//return source.MemberwiseClone();
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace};

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }
        public static bool DeepEquals<T>(this T value, T target)
        {
            return JsonConvert.SerializeObject(value) == JsonConvert.SerializeObject(target);            
        }

        public static List<string> GetDiff<T>(T self, T to, params string[] ignore) where T : class
        {
            var result = new List<string>();

            if (self != null && to != null)
            {
                Type type = self.GetType();// typeof(T);
                List<string> ignoreList = new List<string>(ignore);
                foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (pi.GetCustomAttribute(typeof(JsonIgnoreAttribute)) != null) continue;

                    if (!ignoreList.Contains(pi.Name))
                    {
                        object selfValue = type.GetProperty(pi.Name).GetValue(self, null);
                        object toValue = type.GetProperty(pi.Name).GetValue(to, null);

                        if (selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue)))
                        {
                            var attr=pi.GetCustomAttribute(typeof(DisplayNameAttribute)) as DisplayNameAttribute;
                            var propName = attr?.DisplayName ?? pi.Name;
                            result.Add($"{propName}: {selfValue??"null"} => {toValue ?? "null"}");
                        }
                    }
                }
            }
            return result;
        }
    }
}
