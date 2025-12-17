using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace WebAssembly.SourceGenerators;

[Generator]
public class UnitTestMethodGenerator : IIncrementalGenerator
{
  
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var additionalFilePaths = context.AdditionalTextsProvider
            .Select((file, _) => file.Path)
            .Collect(); 
        
        context.RegisterSourceOutput(additionalFilePaths, (ctx, filePaths) =>
        {
            if (filePaths.IsEmpty) return;
            
            var methods = string.Join("\n", 
                filePaths.Select(path =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(path);

                    var method = $$"""
                                 
                                     /// <summary>
                                     /// Runs the {{fileName}} tests.
                                     /// </summary>
                                     [TestMethod]
                                     public void SpecTest_{{fileName.Replace('-','_')}}()
                                     {
                                         SpecTestRunner.Run(Path.Combine("Runtime", "SpecTestData", "{{fileName}}"), "{{fileName}}.json");
                                     }
                                 """;
                    return method;
                }));
            
            var sourceText = SourceText.From(
                $$"""
                using Microsoft.VisualStudio.TestTools.UnitTesting;
                using System.IO;
                
                namespace WebAssembly.Runtime;
                
                /// <summary>
                /// Auto-generated test methods of the sizeable subset of tests of the SIMD support.
                /// </summary>
                public partial class SpecTests
                {
                {{methods}}
                }    
                """, Encoding.UTF8);
            
            ctx.AddSource("SpecTests.g.cs", sourceText);
        });
    }
    
}
