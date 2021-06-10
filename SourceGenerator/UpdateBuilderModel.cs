using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace SourceGenerator
{
    public class UpdateBuilderModel
    {
        public string GeneratedNamespace { get; set; }

        public string ClassName { get; set; }

        public List<Field> Properties { get; }

        public UpdateBuilderModel()
        {
            Properties = new List<Field>();
        }
    }

    public class Field
    {
        public ITypeSymbol Type { get; }

        public string Name { get; }

        public Field(ITypeSymbol type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}
