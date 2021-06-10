using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator
{
    [Generator]
    public class UpdateBuilderAttributeGenerator : ISourceGenerator
    {
        private List<string> Logs { get; }

        public UpdateBuilderAttributeGenerator()
        {
            Logs = new List<string>();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            const string template = @"
using System;

namespace Generator
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class UpdateBuilderAttribute : Attribute
    {
    }
}
";
            context.AddSource("UpdateBuilderAttribute", SourceText.From(template, Encoding.UTF8));
        }
    }
}
