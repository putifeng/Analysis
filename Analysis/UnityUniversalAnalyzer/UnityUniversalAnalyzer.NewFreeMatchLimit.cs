using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.IO;
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
    public partial class UnityUniversalAnalyzer
    {
        private void OnNewFreeMatchLimitProcessAnalyzeNode(SyntaxNodeAnalysisContext context,BaseObjectCreationExpressionSyntax objectCreationEx,IdentifierNameSyntax identifierNameSyntax,AssignmentExpressionSyntax  assignmentExpressionSyntax)
        {
        

            if (objectCreationEx == null)
                return;
            
              var typeInfo = context.SemanticModel.GetTypeInfo(objectCreationEx);

            if (typeInfo.Type == null)
                return;
            
            var nameTypeSybol = typeInfo.Type as INamedTypeSymbol;
            if (nameTypeSybol == null)
                return;

            var basePlaneType = context.Compilation.GetTypeByMetadataName(AnalyzerConfig.s_ActiveAnalyzerConfig.BaseClassName);
            if (basePlaneType == null)
                return;
         
            var conversion = context.Compilation.ClassifyConversion(nameTypeSybol,basePlaneType);
            if (conversion.IsImplicit && conversion.IsReference) { 
                // symbol 继承或实现了 targetTypeSymbol
            }
            else
            {
                return;
            }
            
            
            var leftSymbol = context.SemanticModel.GetSymbolInfo(assignmentExpressionSyntax.Left).Symbol;

            if (leftSymbol is IFieldSymbol fieldSymbol || leftSymbol is IPropertySymbol propertySymbol)
            {
                // Test(context, $"left:{assignmentExpressionSyntax.Left.ToString()},type:{ leftSymbol.GetType().ToString()}" );
            }
            else
            {
                return;
            }
            
            
            // is propertys
            
       
            // string typeName = objectCreationEx.Type.ToString();
            // if (typeName == "Plane")
            {
                bool hasFree = false;
                // 检查当前 context,如果当前文件 有调用 Free,则正常过去,否则报错   

                var root = context.Node.SyntaxTree.GetRoot();

                {
                    var memberAccessNodes =
                        root.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>().Where(
                            a => a.Kind() == SyntaxKind.SimpleMemberAccessExpression);

                    // property.Free();
                    foreach (var memberAccessExpressionSyntaxBase in memberAccessNodes)
                    {
                        var memberAccessExpressionSyntax =
                            memberAccessExpressionSyntaxBase as MemberAccessExpressionSyntax;
                        if (
                            string.Compare(memberAccessExpressionSyntax.Name.Identifier.ValueText,
                                AnalyzerConfig.s_ActiveAnalyzerConfig.FreeMethodName,
                                StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            var freeName =
                                memberAccessExpressionSyntax.DescendantNodeAndSelfFirst<IdentifierNameSyntax>();
                            if (freeName != null)
                            {
                                if (freeName.Identifier.ValueText == identifierNameSyntax.Identifier.ValueText)
                                {
                                    hasFree = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                // property?.Free();
                if (!hasFree)
                {
                    
                    var memberAccessNodes =
                        root.DescendantNodesAndSelf().OfType<MemberBindingExpressionSyntax>().Where(
                            a => a.Kind() == SyntaxKind.MemberBindingExpression);

                    foreach (var memberAccessExpressionSyntaxBase in memberAccessNodes)
                    {
                  
                        if (
                            string.Compare(memberAccessExpressionSyntaxBase.Name.Identifier.ValueText,
                                AnalyzerConfig.s_ActiveAnalyzerConfig.FreeMethodName,StringComparison.CurrentCultureIgnoreCase) == 0)
                        {

                            memberAccessNodes?.ToString();
                            var condition = memberAccessExpressionSyntaxBase.GetParentOfType<ConditionalAccessExpressionSyntax>();
                            if (condition != null)
                            {
                                if (condition.Expression is IdentifierNameSyntax identifierName)
                                {
                                    if (identifierName.Identifier.ValueText == identifierNameSyntax.Identifier.ValueText)
                                    {
                                        hasFree = true;
                                        break;
                                    }
                                }
                            }
                            
                        }
                       
                        
                    }

                }
                

                if (!hasFree)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptorHelper.NewFreeMatchDescriptors,
                        // The highlighted area in the analyzed source code. Keep it as specific as possible.
                        objectCreationEx.GetLocation(),
                        // The value is passed to the 'MessageFormat' argument of your rule.
                        objectCreationEx);
                    // Reporting a diagnostic is the primary outcome of analyzers.
                    context.ReportDiagnostic(diagnostic);
                }
            }

        }

        private void Test(SyntaxNodeAnalysisContext context)
        {
            {
                var diagnostic = Diagnostic.Create(DiagnosticDescriptorHelper.NewFreeMatchDescriptors,
                    // The highlighted area in the analyzed source code. Keep it as specific as possible.
                    context.Node.GetLocation(),
                    // The value is passed to the 'MessageFormat' argument of your rule.
                    context.Node);
                // Reporting a diagnostic is the primary outcome of analyzers.
                context.ReportDiagnostic(diagnostic);
                return;
            } 
        }
        
        private void Test(SyntaxNodeAnalysisContext context,string str)
        {
            {
                var diagnostic = Diagnostic.Create(new DiagnosticDescriptor( "P00001",
                    "P00001",
                    str,
                    "Usage",
                    DiagnosticSeverity.Error,isEnabledByDefault: true, description: ""),
                    // The highlighted area in the analyzed source code. Keep it as specific as possible.
                    context.Node.GetLocation(),
                    // The value is passed to the 'MessageFormat' argument of your rule.
                    context.Node);
                // Reporting a diagnostic is the primary outcome of analyzers.
                context.ReportDiagnostic(diagnostic);
                return;
            } 
        }
        
        private void OnNewFreeMatchLimitAnalyzeNode(SyntaxNodeAnalysisContext context)
        {
           
            
            if (AnalyzerConfig.s_ActiveAnalyzerConfig == null)
                return;

            if (!AnalyzerConfig.s_ActiveAnalyzerConfig.NewFree_Limit)
            {
                return;
            }
            
            var memberSyntax = context.Node.GetParentOfType<AssignmentExpressionSyntax>();
            if ( memberSyntax == default)
                return;

            if (memberSyntax.Kind() != SyntaxKind.SimpleAssignmentExpression)
                return;
            
            var identifierNameSyntax = memberSyntax.DescendantNodeAndSelfFirst<IdentifierNameSyntax>();
            if (identifierNameSyntax == null)
                return;
            
          
            var objectCreationEx = context.Node as ObjectCreationExpressionSyntax;
            OnNewFreeMatchLimitProcessAnalyzeNode(context,objectCreationEx,identifierNameSyntax,memberSyntax);

        }

        private void OnNewFreeMatchLimit_Implicit_AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            
            if (AnalyzerConfig.s_ActiveAnalyzerConfig == null)
                return;
            
            if (!AnalyzerConfig.s_ActiveAnalyzerConfig.NewFree_Limit)
            {
                return;
            }
            
            var memberSyntax = context.Node.GetParentOfType<AssignmentExpressionSyntax>();
            if ( memberSyntax == default)
                return;

            if (memberSyntax.Kind() != SyntaxKind.SimpleAssignmentExpression)
                return;
            
                        
            var identifierNameSyntax = memberSyntax.DescendantNodeAndSelfFirst<IdentifierNameSyntax>();
            if (identifierNameSyntax == null)
                return;
            
            var objectCreationEx = context.Node as ImplicitObjectCreationExpressionSyntax;

            OnNewFreeMatchLimitProcessAnalyzeNode(context,objectCreationEx,identifierNameSyntax,memberSyntax);
        }
        
    }
}