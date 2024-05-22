using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Analysis
{
    public static class AnalysisHelper
    {
        public static T? GetParentOfType<T>(this SyntaxNode syntaxNode) where T : class
        {
            SyntaxNode? parentNode = syntaxNode.Parent;
            while (parentNode != null)
            {
                if (parentNode is T declarationSyntax)
                {
                    return declarationSyntax;
                }

                parentNode = parentNode.Parent;
            }

            return default;
        }
        
        /// <summary>
        /// 获取所有指定类型的子节点
        /// </summary>
        public static IEnumerable<T> DescendantNodes<T>(this SyntaxNode syntaxNode) where T : SyntaxNode
        {
            foreach (var descendantNode in syntaxNode.DescendantNodes())
            {
                if (descendantNode is T node)
                {
                    yield return node;
                }
            }
        }

        public static IEnumerable<T> DescendantNodesAndSelf<T>(this SyntaxNode syntaxNode) where T : SyntaxNode
        {
            foreach (var descendantNode in syntaxNode.DescendantNodesAndSelf())
            {
                if (descendantNode is T node)
                {
                    yield return node;
                }
            }
        }
        
        public static T? DescendantNodeAndSelfFirst<T>(this SyntaxNode syntaxNode) where T : SyntaxNode
        {
            foreach (var descendantNode in syntaxNode.DescendantNodesAndSelf())
            {
                if (descendantNode is T node)
                {
                    return node;
                }
            }
            
            return default(T);
        }
        
        /// <summary>
        ///     判断指定的程序集是否需要分析
        /// </summary>
        public static bool IsAssemblyNeedAnalyze(string? assemblyName, params string[] analyzeAssemblyNames)
        {
            if (assemblyName == null)
            {
                return false;
            }

            foreach (string analyzeAssemblyName in analyzeAssemblyNames)
            {
                if (assemblyName == analyzeAssemblyName)
                {
                    return true;
                }
            }

            return false;
        }
    }
    
}