// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace Microsoft.DotNet.SourceBuild.Tasks.LeakDetection
{
    internal class CustomAttributeTypeProvider : ICustomAttributeTypeProvider<Type>
    {
        private readonly MetadataReader _reader;

        public CustomAttributeTypeProvider(MetadataReader reader)
        {
            _reader = reader;
        }

        public Type GetPrimitiveType(PrimitiveTypeCode typeCode)
        {
            switch (typeCode)
            {
                case PrimitiveTypeCode.Boolean:
                    return typeof(bool);
                case PrimitiveTypeCode.Byte:
                    return typeof(byte);
                case PrimitiveTypeCode.Int32:
                    return typeof(int);
                case PrimitiveTypeCode.String:
                    return typeof(string);
                default:
                    throw new NotImplementedException();
            }
        }

        public Type GetSystemType()
        {
            return typeof(System.Type);
        }

        public Type GetSZArrayType(Type elementType)
        {
            Type arrayType = Array.CreateInstance(elementType, 0).GetType();
            return arrayType;
        }

        public Type GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            TypeDefinition typeDef = reader.GetTypeDefinition(handle);
            string ns = reader.GetString(typeDef.Namespace);
            string name = reader.GetString(typeDef.Name);
            Type type = Type.GetType($"{ns}.{name}");
            return type;
        }

        public Type GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            TypeReference typeRef = reader.GetTypeReference(handle);
            string ns = reader.GetString(typeRef.Namespace);
            string name = reader.GetString(typeRef.Name);
            Type type = Type.GetType($"{ns}.{name}");
            return type;
        }

        public Type GetTypeFromSerializedName(string name)
        {
            return Type.GetType(name);
        }

        public PrimitiveTypeCode GetUnderlyingEnumType(Type type)
        {
            if (type.IsEnum)
            {
                Type underlyingType = Enum.GetUnderlyingType(type);
                var underlyingTypeCode = Type.GetTypeCode(underlyingType);
                switch (underlyingTypeCode)
                {
                    case TypeCode.Boolean:
                        return PrimitiveTypeCode.Boolean;
                    case TypeCode.Byte:
                        return PrimitiveTypeCode.Byte;
                    case TypeCode.Int32:
                        return PrimitiveTypeCode.Int32;
                    case TypeCode.String:
                        return PrimitiveTypeCode.String;
                    default:
                        throw new NotImplementedException();
                }
            }
            throw new NotImplementedException();
        }
        
        public bool IsSystemType(Type type) => type.FullName == "System.Type";
    }
}
