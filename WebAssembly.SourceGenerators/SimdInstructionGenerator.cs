using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace WebAssembly.SourceGenerators;

[Generator]
public class SimdInstructionGenerator : IIncrementalGenerator
{
    private static readonly string defaultDocs = """
        <member>
        <summary>
        TODO: Missing docs! 
        </summary>
        </member>
    """;
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        static string GetSummary(string xmlDocs)
        {
            // NOTE: misfortunate, we get the XML docs as a string we need to parse :(
            var lines = xmlDocs.Split('\n').ToList();
            var start = lines.FindIndex(line => line.Contains("<summary>"));
            var end = lines.FindIndex(line => line.Contains("</summary>"));
            return string.Join("\n", lines.GetRange(start, end - start + 1).Select(line => $"/// {line.Trim()}"));
        }
        
        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName("WebAssembly.SimdInstructionGenerateAttribute`1",
            predicate: static (_, _) => true,
            transform: static (context, _) =>
            {
                var attr = context.Attributes[0];
                var docs = GetSummary(context.TargetSymbol.GetDocumentationCommentXml() ?? defaultDocs);
                var @base = attr.AttributeClass!.TypeArguments[0].Name;
                var @class = context.TargetSymbol.Name;
                return new Model(docs, @class, @base);
            });
        
        context.RegisterSourceOutput(pipeline, static (context, model) =>
        {
            var @class = model.ClassName;
            var sourceText = SourceText.From(
                $$"""
                namespace WebAssembly.Instructions;
                
                {{model.Docs}}
                public class {{@class}} : {{model.BaseClassName}}
                {
                     /// <summary>
                     /// Always <see cref="SimdOpCode.{{@class}}"/>.
                     /// </summary>
                     public sealed override SimdOpCode SimdOpCode => SimdOpCode.{{@class}};
                     
                     /// <summary>
                     /// Creates a new  <see cref="{{@class}}"/> instance.
                     /// </summary>
                     public {{@class}}() {}
                }
                """, Encoding.UTF8);

            context.AddSource($"{@class}.g.cs", sourceText);
        });
    }

    private record Model(string Docs, string ClassName, string BaseClassName);
}
