using System;
using System.Collections.Generic;
using System.Linq;

namespace ProtoStar
{
    public static class TypeExtensions
    {        

        private static Type[] GetGenericArgumentsForBaseTypeClass(this Type givenType, Type genericType)=>
            givenType == typeof(object) ?
                Enumerable.Empty<Type>().ToArray() :
                ((givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType) ?
                    givenType.GetGenericArguments() :
                    givenType.BaseType.GetGenericArgumentsForBaseTypeClass(genericType));

        private static IEnumerable<Type[]> GetGenericArgumentsForBaseTypeInterface(this Type givenType, Type genericType) =>
            givenType.GetInterfaces().Where((t) => t.IsGenericType && t.GetGenericTypeDefinition() == genericType).Select(t=> t.GetGenericArguments());
    
        public static IEnumerable<Type[]> GetGenericArgumentsForBaseType(this Type givenType, Type genericType) =>
            genericType.IsInterface ?
            givenType.GetGenericArgumentsForBaseTypeInterface(genericType) :
            new[] { givenType.GetGenericArgumentsForBaseTypeClass(genericType) };

        public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            if(givenType == typeof(object)) return false;

            var ownTypeResult =
                genericType.IsInterface?
                givenType.IsAssignableToGenericInterface(genericType):
                (givenType.IsGenericType && givenType.GetGenericTypeDefinition()==genericType);

            return ownTypeResult || IsAssignableToGenericType(givenType.BaseType,genericType);
        }

        private static bool IsAssignableToGenericInterface(this Type givenType, Type genericType) =>
            givenType.GetInterfaces().
            Any(intType=> intType.IsGenericType && 
                          intType.GetGenericTypeDefinition()==genericType);

    }
}