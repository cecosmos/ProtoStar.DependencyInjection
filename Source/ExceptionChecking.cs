using System;
using System.Collections.Generic;
using System.Linq;

namespace ProtoStar
{
    public static class ExceptionChecking
    {
        public static void EnsureInheritance(this Type baseType, Type derivedType, string derivedParameterName = "")
        {
            if (!baseType.IsAssignableFrom(derivedType))
                throw new ArgumentException($"The {derivedType.FullName} is not an inheritor of {baseType.FullName}.",derivedParameterName);
        }

        public static T ThrowOnNull<T>(this T obj, string paramName)
            where T:class
        {
            if (obj == null) throw new ArgumentNullException(paramName);
            return obj;
        }

        public static T ThrowOnNull<T>(this T obj) where T:class =>
            obj.ThrowOnNull("");

    }
}