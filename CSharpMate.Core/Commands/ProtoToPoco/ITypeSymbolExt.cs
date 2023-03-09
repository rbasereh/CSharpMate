using System.Linq;
using Microsoft.CodeAnalysis;

namespace CSharpMate.Core.ProtoToPoco
{
    public static class ITypeSymbolExt
    {
        public static bool IsAssignableFrom(this ITypeSymbol t, string typename)
            => t.Name == typename || t.AllInterfaces.Any(i => i.Name == typename);

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

