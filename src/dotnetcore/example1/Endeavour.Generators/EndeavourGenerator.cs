using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Endeavour
{

    public readonly struct EndeavourToGenerate
    {

        public readonly string Name;

        public readonly string TypeNamespace;
        public readonly string Type;

        public readonly string ClassNamespace;
        public readonly string ClassName;

        public EndeavourToGenerate(string classNs, string className, string name, string typeNamespace, string type)
        {
            ClassNamespace = classNs;
            ClassName = className;
            Name = name;
            TypeNamespace = typeNamespace;
            Type = type;
        }
    }


    [Generator]
    public class EndeavourGenerator : IIncrementalGenerator
    {

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
         //  int wait = 0;

         //  while (!System.Diagnostics.Debugger.IsAttached && wait < 200)
         //  {
          //     System.Threading.Thread.Sleep(500);
          //     wait++;
           //}


            var enumDeclarations = context.SyntaxProvider.CreateSyntaxProvider<ClassDeclarationSyntax>(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(static m => m is not null)!;

            // Combine the selected enums with the `Compilation`
            IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndEnums
                = context.CompilationProvider.Combine(enumDeclarations.Collect());

            // Generate the source using the compilation and enums
            context.RegisterSourceOutput(compilationAndEnums,
                static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        static void Execute(
            Compilation compilation,
            ImmutableArray<ClassDeclarationSyntax> enums,
            SourceProductionContext context)
        {
            if (enums.IsDefaultOrEmpty)
            {
                // nothing to do yet
                return;
            }

            // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
            IEnumerable<ClassDeclarationSyntax> distinctEnums = enums.Distinct();

            // Convert each EnumDeclarationSyntax to an EnumToGenerate
            List<EndeavourToGenerate> enumsToGenerate = GetTypesToGenerate(compilation, distinctEnums, context.CancellationToken);

            // If there were errors in the EnumDeclarationSyntax, we won't create an
            // EnumToGenerate for it, so make sure we have something to generate
            if (enumsToGenerate.Count > 0)
            {
                // generate the source code and add it to the output
                string result = GenerateExtensionClass(enumsToGenerate);
                context.AddSource("EnumExtensions.g.cs", SourceText.From(result, Encoding.UTF8));
            }
        }

        public static string GenerateExtensionClass(List<EndeavourToGenerate> enumsToGenerate)
        {
            var sb = new StringBuilder();
            foreach (var e in enumsToGenerate)
            {
                
        
            sb.Append("using ").Append(e.TypeNamespace).Append(";")
              .Append("using Microsoft.AspNetCore.Mvc;")
               .Append("using Endeavour;")
               .Append("using System;")
               .Append("using System.Threading.Tasks;")
                   .Append("namespace ").Append(e.ClassNamespace)
                   .Append(@"{
                        public partial class ").Append(e.ClassName)
                        .Append("{")
                        .Append("[HttpPost(Name = \"Post").Append(e.Name).Append("\")]")
                            .Append("public async Task<Endeavour<").Append(e.Type).Append(">> Post(").Append(e.Type).Append(" data)")
                            .Append("{")
                            .Append(" await Task.Yield();")
                            .Append("return new Endeavour<").Append(e.Type).Append(">(){StartedDate=DateTimeOffset.Now,Data=data};")
                            .Append("}")
                        .Append("}")
                    .Append("}");
                
    


                // sb.Append(@"
                //     using ").Append(e.TypeNamespace).Append(";")
                //    .Append("namespace ").Append(e.ClassNamespace)
                //    .Append(@"{
                //         public partial class ").Append(e.ClassName)
                //         .Append("{")
                //             .Append("public void Testing123()")
                //             .Append("{")
                //             .Append("var i = 1;")
                //             .Append("}")
                //         .Append("}")
                //     .Append("}");

            }
            return sb.ToString();
        }

        static List<EndeavourToGenerate> GetTypesToGenerate(
            Compilation compilation,
            IEnumerable<ClassDeclarationSyntax> classes,
            CancellationToken ct)
        {
            // Create a list to hold our output
            var enumsToGenerate = new List<EndeavourToGenerate>();
            // Get the semantic representation of our marker attribute 
            INamedTypeSymbol? enumAttribute =
                compilation.GetTypeByMetadataName("Endeavour.EndeavourAttribute`1");


            if (enumAttribute == null)
            {
                // If this is null, the compilation couldn't find the marker attribute type
                // which suggests there's something very wrong! Bail out..
                return enumsToGenerate;
            }

            foreach (ClassDeclarationSyntax classDeclarationSyntax in classes)
            {
                // stop if we're asked to
                ct.ThrowIfCancellationRequested();

                // Get the semantic representation of the enum syntax
                SemanticModel semanticModel = compilation.GetSemanticModel(
                    classDeclarationSyntax.SyntaxTree);

                if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax)
                    is not INamedTypeSymbol classSymbol)
                {
                    // something went wrong, bail out
                    continue;
                }

                // Get the full type name of the enum e.g. Colour, 
                // or OuterClass<T>.Colour if it was nested in a generic type (for example)
                string attributedClassName = classSymbol.ToString();

                var justClassName = classSymbol.Name.ToString();
                var ns = classSymbol.ContainingNamespace.ToString();

                var atts = classDeclarationSyntax.AttributeLists
                  .SelectMany(x => x.Attributes)
                  .ToList();

                foreach (var att in atts)
                {
                    var name = att.Name;
                    var nStr = name.ToString();
                    var gen = name as GenericNameSyntax;

                    if (gen != null)
                    {
                        var n = gen.ToString();
                        var nn = gen.ToFullString();
                        var idd = gen.Identifier.ToString();


                        var types = gen.TypeArgumentList;
                        foreach (var ttt in types.Arguments)
                        {
                            var ttt2 = ttt.ToString();
                        }


                        if (idd == "Endeavour" || idd == "EndeavourAttribute")
                        {
                            var firstGenericType = types.Arguments.First();

                            var attibSymbol = semanticModel.GetSymbolInfo(firstGenericType).Symbol;

 
                            var classTypeSymbolForAttrib = attibSymbol as INamedTypeSymbol;

                            var type = firstGenericType.ToString();

                            var iconArg = att.ArgumentList.Arguments[0];
                            var iconExpr = iconArg.Expression;
                            var endeavourName = semanticModel.GetConstantValue(iconExpr).ToString();

                            enumsToGenerate.Add(new EndeavourToGenerate(
      ns,
      justClassName,
      endeavourName,
      classTypeSymbolForAttrib.ContainingNamespace.Name,
      classTypeSymbolForAttrib.Name));
                        }
                    }
                }


            }

            return enumsToGenerate;
        }


        static bool IsSyntaxTargetForGeneration(SyntaxNode node)
            => node is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax m;

        static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {

            // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
            var classSyntax = (ClassDeclarationSyntax)context.Node;

            var atts = classSyntax.AttributeLists
            .SelectMany(x => x.Attributes)
            .ToList();

            foreach (var att in atts)
            {
                var name = att.Name;
                var gen = name as GenericNameSyntax;
                if (gen != null)
                {
                    var idd = gen.Identifier.ToString();
                    if (idd == "Endeavour" || idd == "EndeavourAttribute")
                        return classSyntax;
                }
            }

            return null;
        }
    }



}