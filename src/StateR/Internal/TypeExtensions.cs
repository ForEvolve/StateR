using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateR.Internal
{
    public static class TypeExtensions
    {
        public static string GetStatorName(this Type type)
        {
            //if (type == null) { throw new ArgumentNullException(nameof(type)); }
            //Console.WriteLine($"[GetStatorName] {type.FullName}");
            if (type.IsGenericType)
            {
                //Console.WriteLine($"[GetStatorName](type) {type}");
                //Console.WriteLine($"[GetStatorName](Name) {type.Name}");
                //Console.WriteLine($"[GetStatorName](type.FullName) {type.FullName}");
                var indexOfThing = type.FullName.IndexOf('`');
                var name = type.FullName.Substring(0, indexOfThing);
                var indexOfDot = name.LastIndexOf('.');
                name = name.Substring(indexOfDot + 1);
                name += "<";
                foreach (var gType in type.GetGenericArguments())
                {
                    name += gType.GetStatorName();
                    name += ", ";
                }
                name = name.Trim(',', ' ');
                name += ">";
                return name;
            }
            if (type.FullName?.IndexOf('+') > -1)
            {
                var fullName = type.FullName;
                var lastDot = fullName.LastIndexOf('.');
                return fullName.Substring(lastDot + 1);
            }
            return type.Name;
        }
    }
}
