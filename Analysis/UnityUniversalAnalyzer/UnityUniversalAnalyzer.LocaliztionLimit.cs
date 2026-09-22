using System;
using System.Collections.Immutable;
using System.Data;
using System.IO;
// using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Analysis
{
    public partial class UnityUniversalAnalyzer
    {
        static string? GetInvokedMethodName(InvocationExpressionSyntax invocation)
            => invocation.Expression switch
            {
                SimpleNameSyntax s => s.Identifier.ValueText,          // Foo(...)
                MemberAccessExpressionSyntax m => m.Name.Identifier.ValueText, // a.Foo(...)
                _ => null
                };
        
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

                string methodName = GetInvokedMethodName(invoke);

                if (!string.IsNullOrEmpty(methodName))
                {
                    foreach (string localizationLimitIngore in Definition.LocalizationLimitIngores)
                    {
                        if (methodName.EndsWith(localizationLimitIngore, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return true;
                        }
                    }
                 
                    // Definition.LocalizationLimitIngores.Contains()
                }
                
                // var names = invoke.Expression.ToString().Split('.');
                // if (names.Length != 0)
                // {
                //     if (Definition.LocalizationLimitIngores.Contains( names[names.Length -1].ToLower()))
                //     {
                //         return true;
                //     }
                // }
                
            }
            return false;
        }
        
        private static bool IsCN(string content)
        {
            return Regex.IsMatch(content, @"[\u4e00-\u9fa5]");
        }
        
        private static bool IsCN(SourceText content)
        {
            return ContainsChinese(content);
        }
        
        
        private static bool IsCN(LiteralExpressionSyntax expressionSyntax)
        {
            return ContainsChinese(expressionSyntax.Token.ValueText);
        }
          
        
        static bool ContainsChinese(string sourceText)
        {
            for (int i = 0; i <   sourceText.Length; i++)
            {
                char c = sourceText[i];
                if (c >= '\u4e00' && c <= '\u9fa5')
                    return true;
            }
            return false;
        }

        static bool ContainsChinese(SourceText sourceText)
        {
            for (int i = 0; i <   sourceText.Length; i++)
            {
                char c = sourceText[i];
                if (c >= '\u4e00' && c <= '\u9fa5')
                    return true;
            }
            return false;
        }

        private void OnLocaliztionLimitAnalyzeInvoke(OperationAnalysisContext context)
        {
            if (AnalyzerConfig.s_ActiveAnalyzerConfig == null)
                return;
            
            if (!AnalyzerConfig.s_ActiveAnalyzerConfig.CN_Limit)
                return;
            
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
                            if(IsCN(stringTextSyntax.TextToken.ValueText))
                            {
                                if(Ingore(stringTextSyntax))
                                    continue;
                                
                                var diagnostic = Diagnostic.Create( DiagnosticDescriptorHelper.LocalizationLimitDescriptors,
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
        
        private void OnLocaliztionLimitAnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            if (AnalyzerConfig.s_ActiveAnalyzerConfig == null)
                return;
            
            if (!AnalyzerConfig.s_ActiveAnalyzerConfig.CN_Limit)
                return;
            
            if (context.Node.GetParentOfType<AttributeArgumentSyntax>() != default)
                return;
                
            var literalExpressionSyntax = (LiteralExpressionSyntax) context.Node;
            
              
            SourceText text =  literalExpressionSyntax.GetText();
            if(!IsCN(text))
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
                if ( string.Compare(classDeclaration.Identifier.ValueText ,AnalyzerConfig.s_ActiveAnalyzerConfig.LanguageClassName, StringComparison.CurrentCultureIgnoreCase) == 0)
                // if (classDeclaration.Identifier.ToString() == AnalyzerConfig.s_ActiveAnalyzerConfig.LanguageClassName)
                {
                    return;
                }
            }
            
            var diagnostic = Diagnostic.Create( DiagnosticDescriptorHelper.LocalizationLimitDescriptors,
                // The highlighted area in the analyzed source code. Keep it as specific as possible.
                context.Node.GetLocation(),
                // The value is passed to the 'MessageFormat' argument of your rule.
                literalExpressionSyntax);
            // Reporting a diagnostic is the primary outcome of analyzers.
            context.ReportDiagnostic(diagnostic);
        }
        
        
    }
}