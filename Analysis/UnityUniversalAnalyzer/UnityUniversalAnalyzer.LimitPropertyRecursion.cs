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
        // public string testStr => testStr;
        //
        // public string testStr2
        // {
        //     get => testStr2;
        //     set => testStr2 = value;
        // }
        //
        public string testStr3
        {
            get
            {
                return testStr3;
            }
            set => testStr3 = value;
        }
        
        private void OnPropertyDeclarationAnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclarationSyntax = context.Node as PropertyDeclarationSyntax;
            if (propertyDeclarationSyntax == null)
                return;
            
            if (propertyDeclarationSyntax.Kind() != SyntaxKind.PropertyDeclaration)
                return;
            
            if (propertyDeclarationSyntax.ExplicitInterfaceSpecifier  != null)
            {
                return;
            }
        
         
            bool isError = false;
            try
            {
                {
                    var firstCluse = propertyDeclarationSyntax.ChildNodesFirst<ArrowExpressionClauseSyntax>();
                    if (firstCluse != null)
                    {
                        var expression = firstCluse.Expression as IdentifierNameSyntax;
                        if (expression != null &&
                            expression.Identifier.ValueText == propertyDeclarationSyntax.Identifier.ValueText )
                        {
                            isError = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Test(context,$"error:{e}");
            }
            // Test(context,$"isError33:{isError}");
         

            // Test(context,$"isError:{isError}");
            if (!isError)
            {
                var getsetSyntaxs =
                    context.Node.DescendantNodes<AccessorDeclarationSyntax>().Where(
                        a =>
                            a.Kind() == SyntaxKind.GetAccessorDeclaration ||
                            a.Kind() == SyntaxKind.SetAccessorDeclaration);
                
                // Test(context,$"getsetsyntaxs{getsetSyntaxs.Count()}");
                foreach (AccessorDeclarationSyntax declarationSyntax in getsetSyntaxs)
                {
                    if (declarationSyntax.Kind() == SyntaxKind.GetAccessorDeclaration)
                    {
                        var getCluse =  declarationSyntax.ChildNodesFirst<ArrowExpressionClauseSyntax>();
                        if (getCluse != null)
                        {
                            var expression = getCluse.Expression as IdentifierNameSyntax;
                            if (expression != null && expression.Identifier.ValueText == propertyDeclarationSyntax.Identifier.ValueText)
                            {
                                isError = true;
                                break;
                            }
                        }
                        else
                        {
                            var accessorDeclarationSyntax = declarationSyntax.ChildNodesFirst<Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax>();
                            if (accessorDeclarationSyntax != null)
                            {
                                var returnSyntaxs = accessorDeclarationSyntax.DescendantNodes<ReturnStatementSyntax>();
                                if (returnSyntaxs != null)
                                {
                                    foreach (var returnStatementSyntax in returnSyntaxs)
                                    {
                                        if(returnStatementSyntax.Expression == null)
                                            continue;
                                        
                                        var returnSymbol = context.SemanticModel.GetSymbolInfo(returnStatementSyntax.Expression).Symbol;
                                        if (returnSymbol is IPropertySymbol)
                                        {
                                            var returnPropertyIdName =
                                                returnStatementSyntax.ChildNodesFirst<IdentifierNameSyntax>();
                                            if (returnPropertyIdName != null)
                                            {
                                                if (returnPropertyIdName.Identifier.ValueText ==
                                                    propertyDeclarationSyntax.Identifier.ValueText)
                                                {
                                                    isError = true;
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {

                                         
                                        }
                                    }
                                }
                            }

                            if (isError)
                            {
                                break;
                            }
                        }
                        
                        
                    }else if (declarationSyntax.Kind() == SyntaxKind.SetAccessorDeclaration)
                    {
                        var setCluse =  declarationSyntax.ChildNodesFirst<ArrowExpressionClauseSyntax>();
                        if (setCluse != null)
                        {
                            AssignmentExpressionSyntax assignment =
                                setCluse.ChildNodesFirst<AssignmentExpressionSyntax>();
                            if (assignment != null)
                            {
                                var idName = assignment.Left as IdentifierNameSyntax;
                                // var idName = assignment.ChildNodesFirst<IdentifierNameSyntax>();
                                if (idName != null &&
                                    idName.Identifier.ValueText == propertyDeclarationSyntax.Identifier.ValueText)
                                {
                                    isError = true;
                                    break;
                                }
                            }
                        }
                        
                    }
                    
                }
            }

            if (isError)
            {
                var diagnostic = Diagnostic.Create( DiagnosticDescriptorHelper.RecursionLimitDescriptors,
                    // The highlighted area in the analyzed source code. Keep it as specific as possible.
                    propertyDeclarationSyntax.GetLocation(),
                    // The value is passed to the 'MessageFormat' argument of your rule.
                    propertyDeclarationSyntax);
                // Reporting a diagnostic is the primary outcome of analyzers.
                context.ReportDiagnostic(diagnostic);
            }
            
        }
        
    }
}