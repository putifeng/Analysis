using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Analysis
{
    
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LocalizationLimitAnalyzer: DiagnosticAnalyzer
    {

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            DiagnosticDescriptorHelper.LocalizationLimitDescriptors.immutableArray;
        
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction((analysisContext =>
            {
                if (AnalysisHelper.IsAssemblyNeedAnalyze(analysisContext.Compilation.AssemblyName,Definition.EnableAnalysisClassName)
                    || AnalysisHelper.IsAssemblyNeedAnalyze(analysisContext.Compilation.AssemblyName,"Test")
                    )
                {
                    analysisContext.RegisterSyntaxNodeAction(AnalyzeNode,SyntaxKind.StringLiteralExpression);
                    analysisContext.RegisterOperationAction(AnalyzeInvoke, OperationKind.Invocation);
                }
     
            }));
            
       
        }

        // 中文直接定义解析
        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            
            if (context.Node.GetParentOfType<AttributeArgumentSyntax>() != default)
                return;
                
            var literalExpressionSyntax = (LiteralExpressionSyntax) context.Node;
              
            SourceText text =  literalExpressionSyntax.GetText();
            if(!IsCN(text.ToString()))
            {
                // 不是中文不处理
                return;
            }

            if (Ingore(literalExpressionSyntax))
                return;
            
            // 判断是否在类型里
            var classDeclaration = literalExpressionSyntax.GetParentOfType<ClassDeclarationSyntax>();
            if (classDeclaration != null)
            {
                if (classDeclaration.Identifier.ToString() == Definition.LanguageClassName)
                {
                    return;
                }
            }
            
            var diagnostic = Diagnostic.Create( DiagnosticDescriptorHelper.LocalizationLimitDescriptors.descriptor,
                // The highlighted area in the analyzed source code. Keep it as specific as possible.
                context.Node.GetLocation(),
                // The value is passed to the 'MessageFormat' argument of your rule.
                literalExpressionSyntax);
            // Reporting a diagnostic is the primary outcome of analyzers.
            context.ReportDiagnostic(diagnostic);
        }

        private void AnalyzeInvoke(OperationAnalysisContext context)
        {
            if (!(context.Operation is IInvocationOperation invocationOperation) ||
                !(context.Operation.Syntax is InvocationExpressionSyntax invocationSyntax))
                return;

            for (var i = 0; i < invocationSyntax.ArgumentList.Arguments.Count; i++)
            {
                var interpolatedStringExpressionSyntaxs = invocationSyntax.ArgumentList.Arguments[i].Expression
                    .DescendantNodesAndSelf<InterpolatedStringExpressionSyntax>();
                foreach (var stringExpressionSyntax in interpolatedStringExpressionSyntaxs)
                {
                    foreach (var syntaxNode in stringExpressionSyntax.ChildNodes())
                    {
                        if (syntaxNode is InterpolatedStringTextSyntax stringTextSyntax)
                        {
                            if(IsCN(stringTextSyntax.ToString()))
                            {
                                if(Ingore(stringTextSyntax))
                                    continue;
                                
                                var diagnostic = Diagnostic.Create( DiagnosticDescriptorHelper.LocalizationLimitDescriptors.descriptor,
                                    // The highlighted area in the analyzed source code. Keep it as specific as possible.
                                    stringTextSyntax.GetLocation(),
                                    // The value is passed to the 'MessageFormat' argument of your rule.
                                    stringTextSyntax);
                                // Reporting a diagnostic is the primary outcome of analyzers.
                                context.ReportDiagnostic(diagnostic);
                            }
                        }
                    }
                }
            }
        }

        private static bool Ingore(SyntaxNode syntaxNode)
        {
            var att = syntaxNode.GetParentOfType<AttributeSyntax>();
            if (att != null)
                return false;
            
            var ex = syntaxNode.GetParentOfType<ExpressionStatementSyntax>();
            if (ex == null)
                return false;
       
            var invoke = ex.DescendantNodeAndSelfFirst<InvocationExpressionSyntax>();
            if (invoke != null)
            {
                var names = invoke.Expression.ToString().Split('.');
                if (names.Length != 0)
                {
                    if (Definition.LocalizationLimitIngores.Contains( names[names.Length -1].ToLower()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        private static bool IsCN(string content)
        {
            return Regex.IsMatch(content, @"[\u4e00-\u9fa5]");
        }
    }
    
    
}