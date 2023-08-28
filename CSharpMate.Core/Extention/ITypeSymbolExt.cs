using System.Linq;
using Microsoft.CodeAnalysis;

namespace CSharpMate.Core
{
    public static class ITypeSymbolExt
    {
        public static bool IsAssignableFrom(this ITypeSymbol t, string typename)
            => t.Name == typename || t.AllInterfaces.Any(i => i.Name == typename);
        public static bool IsAssignableFromClass(this ITypeSymbol t, string typename)
            => t.Name == typename ||
                (t.BaseType != null && IsAssignableFromClass(t.BaseType, t.Name));


        public static string RemoveFromEnd(this string originalString, params string[] stringsToRemove)
        {
            foreach (var stringToRemove in stringsToRemove)
            {
                if (originalString.EndsWith(stringToRemove))
                    originalString = originalString.Substring(0, originalString.Length - stringToRemove.Length);
            }
            return originalString;
        }
    }

}

