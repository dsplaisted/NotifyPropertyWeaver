﻿using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

static class CecilExtensions
{
    public static string GetName(this PropertyDefinition propertyDefinition)
    {
        return string.Format("{0}.{1}", propertyDefinition.DeclaringType.FullName, propertyDefinition.Name);
    }

    public static bool IsCall(this OpCode opCode)
    {
        return (opCode.Code == Code.Call) || (opCode.Code == Code.Callvirt);
    }

    public static string GetName(this MethodDefinition methodDefinition)
    {
        return string.Format("{0}.{1}", methodDefinition.DeclaringType.FullName, methodDefinition.Name);
    }

    public static MethodDefinition Constructor(this TypeDefinition typeDefinition)
    {
        return typeDefinition.Methods.First(x => x.IsConstructor);
    }

    public static FieldReference GetGeneric(this FieldDefinition definition)
    {
        if (definition.DeclaringType.HasGenericParameters)
        {
            var declaringType = new GenericInstanceType(definition.DeclaringType);
            foreach (var parameter in definition.DeclaringType.GenericParameters)
            {
                declaringType.GenericArguments.Add(parameter);
            }
            return new FieldReference(definition.Name, definition.FieldType, declaringType);
        }

        return definition;
    }

    public static MethodReference GetGeneric(this MethodReference reference)
    {
        if (reference.DeclaringType.HasGenericParameters)
        {
            var declaringType = new GenericInstanceType(reference.DeclaringType);
            foreach (var parameter in reference.DeclaringType.GenericParameters)
            {
                declaringType.GenericArguments.Add(parameter);
            }
            var methodReference = new MethodReference(reference.Name, reference.MethodReturnType.ReturnType, declaringType);
            foreach (var parameterDefinition in reference.Parameters)
            {
                methodReference.Parameters.Add(parameterDefinition);
            }
            methodReference.HasThis = reference.HasThis;
            return methodReference;
        }

        return reference;
    }


    public static CustomAttribute GetAttribute(this IEnumerable<CustomAttribute> attributes, string attributeName)
    {
        return attributes.FirstOrDefault(attribute => attribute.Constructor.DeclaringType.Name == attributeName);
    }

    public static bool ContainsAttribute(this IEnumerable<CustomAttribute> attributes, string attributeName)
    {
        return attributes.Any(attribute => attribute.Constructor.DeclaringType.Name == attributeName);
    }


    public static List<TypeDefinition> GetAllTypeDefinitions(this ModuleDefinition moduleDefinition)
    {
        var definitions = new List<TypeDefinition>();
        //First is always module so we will skip that;
        GetTypes(moduleDefinition.Types.Skip(1), definitions);
        return definitions;
    }

    static void GetTypes(IEnumerable<TypeDefinition> typeDefinitions, List<TypeDefinition> definitions)
    {
        foreach (var typeDefinition in typeDefinitions)
        {
            GetTypes(typeDefinition.NestedTypes, definitions);
            definitions.Add(typeDefinition);

        }
    }
}