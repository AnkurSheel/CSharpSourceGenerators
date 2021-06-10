﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator
{
    [Generator]
    public class UpdateBuilderGenerator : ISourceGenerator
    {
        private List<string> Logs { get; }

        public UpdateBuilderGenerator()
        {
            Logs = new List<string>();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new UpdateBuilderSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // if (!Debugger.IsAttached)
            // {
            //     Debugger.Launch();
            // }

            if (context.SyntaxReceiver is not UpdateBuilderSyntaxReceiver receiver)
            {
                return;
            }

            var compilation = context.Compilation;
            var stringBuilder = new StringBuilder();

            foreach (var classDeclarationSyntax in receiver.Classes)
            {
                var model = new UpdateBuilderModel();

                var updateBuilderAttribute = classDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes)
                    .FirstOrDefault(y => y.Name.ToString() == UpdateBuilderSyntaxReceiver.UpdateBuilderAttributeName);

                if (updateBuilderAttribute == null)
                {
                    continue;
                }

                var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
                var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                Logs.Add($"Found a class named {classSymbol}");
                model.GeneratedNamespace = string.Join("", classSymbol.ToDisplayParts().SkipLast(2));
                model.ClassName = classSymbol.ToDisplayParts().Last().ToString();

                var properties = classDeclarationSyntax.DescendantNodes()
                    .OfType<PropertyDeclarationSyntax>()
                    .Select(p => compilation.GetSemanticModel(p.SyntaxTree).GetDeclaredSymbol(p) as IPropertySymbol)
                    .Where(m => m.DeclaredAccessibility == Accessibility.Public && !m.IsStatic)
                    .Where(p => GetPropertyType(p.Type) != null)
                    .ToList();

                AddPropertiesToModel(properties, model);

                stringBuilder.AppendLine($@"
// <auto-generated>
//     This code was generated by the update builder generator tool.
//     Changes to this file may cause incorrect behavior and will be lost if  the code is regenerated.

using System;
using Optional;

namespace {model.GeneratedNamespace}
{{
    public class {model.ClassName}Builder
    {{");
                AddFields(model, stringBuilder);

                stringBuilder.AppendLine($@"
        private bool _changesMade;

        public {model.ClassName}Builder() 
        {{
        }}");

                AddUpdateMethods(model, stringBuilder);

                stringBuilder.AppendLine($@"
        public Option<{model.ClassName}> Build()
                    => _changesMade
                        ? Option.Some(BuildUpdate())
                        : Option.None<{model.ClassName}>();

        private {model.ClassName}Builder UpdateField(Action updateField)
        {{
            updateField();
            _changesMade = true;
            return this;
        }}

        private {model.ClassName} BuildUpdate()
            => new {model.ClassName}(
                {string.Join($",{Environment.NewLine}\t\t\t\t", properties.Select(x => GetPropertyNameAsField(x.Name)))});
    }}
}}")
                    .Replace("\r\n", "\n")
                    .Replace("\n", Environment.NewLine); // normalize regardless of git checkout policy;

                context.AddSource($"{model.ClassName}Builder", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
                context.AddSource("Logs", SourceText.From($@"/*{Environment.NewLine + string.Join(Environment.NewLine, Logs) + Environment.NewLine}*/", Encoding.UTF8));
            }
        }

        private void AddUpdateMethods(UpdateBuilderModel model, StringBuilder stringBuilder)
        {
            foreach (var property in model.Properties)
            {
                Logs.Add($"properties {property.Type} {property.Name}");
                // var propertyType = GetPropertyType(property.Type);
                if (!property.Type.ContainingNamespace.Name.Equals("Optional"))
                {
                    continue;
                }

                var propertyNameAsVariable = GetPropertyNameAsVariable(property.Name);
                stringBuilder.AppendLine($@"
        public {model.ClassName}Builder Update{property.Name}({GetPropertyType(property.Type)} {propertyNameAsVariable})
        {{
            return UpdateField(() => {GetPropertyNameAsField(property.Name)} = Option.Some({propertyNameAsVariable}));
        }}");
            }
        }

        private void AddFields(UpdateBuilderModel model, StringBuilder stringBuilder)
        {
            foreach (var property in model.Properties)
            {
                stringBuilder.AppendLine($@"
        private {property.Type} {GetPropertyNameAsField(property.Name)};");
            }
        }

        private void AddPropertiesToModel(IReadOnlyList<IPropertySymbol> properties, UpdateBuilderModel model)
        {
            foreach (var property in properties)
            {
                model.Properties.Add(new Field(property.Type, property.Name));
            }
        }

        private string GetPropertyType(ITypeSymbol type)
        {
            var a = type.ToDisplayParts().FirstOrDefault(x => x.Kind is SymbolDisplayPartKind.Keyword or SymbolDisplayPartKind.ClassName or SymbolDisplayPartKind.EnumName);

            return a.ToString();
        }

        private string GetPropertyNameAsField(string name)
        {
            return $"_{GetPropertyNameAsVariable(name)}";
        }

        private string GetPropertyNameAsVariable(string name)
        {
            return char.ToLower(name[0]) + name.Substring(1);
        }
    }
}