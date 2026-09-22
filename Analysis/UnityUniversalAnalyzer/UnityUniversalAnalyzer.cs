using System;
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
    
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public partial class UnityUniversalAnalyzer: DiagnosticAnalyzer
    {

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            DiagnosticDescriptorHelper.TotalDescriptors;
        
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None |  GeneratedCodeAnalysisFlags.ReportDiagnostics | GeneratedCodeAnalysisFlags.Analyze);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction((analysisContext =>
            {
                string projectPath = string.Empty;
                foreach (var optionsAdditionalFile in analysisContext.Options.AdditionalFiles)
                {
                    var text = optionsAdditionalFile.GetText(analysisContext.CancellationToken);
                    if (text != null)
                    {
                        projectPath = text.ToString();
                        // 这里获取的是 Library\Bee\artifacts\19xxxx\xxxx\Assembly-CSharp.UnityAdditionalFile.txt 的内容
                        // 里面存放了 项目路径
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(projectPath))
                {
                    string configPath = $"{projectPath}/Assets/Editor/Analysis/AnalysisConfig/Analysis.Config.txt";
                    if (File.Exists(configPath))
                    {
                        DateTime lastWriteTime = File.GetLastWriteTime(configPath);
                        if (AnalyzerConfig.lastWriteTime < lastWriteTime)
                        {
                            string[] lines = File.ReadAllLines(configPath);
                            if (lines.Length > 0)
                            {
                                AnalyzerConfig.CreateConfigs(lines);
                            }
                            AnalyzerConfig.lastWriteTime = lastWriteTime;

                            AnalyzerConfig.s_ActiveConfigPath = configPath;
                        }
                    }
                    else
                    {
                        AnalyzerConfig.Clear();
                    }
                }
                else
                {
               
                }

                bool isTest = AnalysisHelper.IsAssemblyNeedAnalyze(analysisContext.Compilation.AssemblyName, "Test");
                if (isTest)
                {
                    AnalyzerConfig.CreateGlobalDefaultIsNull();
                }
                
                AnalyzerConfig.SwitchActiveConfig(analysisContext.Compilation.AssemblyName);


                if (isTest)
                {
                    AnalyzerConfig.s_ActiveAnalyzerConfig.CN_Limit = false;
                    AnalyzerConfig.s_ActiveAnalyzerConfig.NewFree_Limit = true;
                    AnalyzerConfig.s_ActiveAnalyzerConfig.PropertyRecursion_Limit = true;
                }

                if (
                    AnalyzerConfig.s_ActiveAnalyzerConfig != null
                    )
                {
                    
                    if(AnalyzerConfig.s_ActiveAnalyzerConfig.CN_Limit)
                    {
                        analysisContext.RegisterSyntaxNodeAction(OnStringLiteralExpressionAnalyzeNode,
                        SyntaxKind.StringLiteralExpression);
                        analysisContext.RegisterOperationAction(OnInvocation, OperationKind.Invocation);
                    }


                    if (AnalyzerConfig.s_ActiveAnalyzerConfig.NewFree_Limit)
                    {

                        analysisContext.RegisterSyntaxNodeAction(OnNewFreeMatchLimitAnalyzeNode,
                            SyntaxKind.ObjectCreationExpression);

                        analysisContext.RegisterSyntaxNodeAction(OnNewFreeMatchLimit_Implicit_AnalyzeNode,
                            SyntaxKind.ImplicitObjectCreationExpression);
                    }

                    if (AnalyzerConfig.s_ActiveAnalyzerConfig.PropertyRecursion_Limit)
                    {
                        analysisContext.RegisterSyntaxNodeAction(OnPropertyDeclarationAnalyzeNode,
                            SyntaxKind.PropertyDeclaration);
                    }
                    
                }

            }));


        }

        // 中文直接定义解析
        private void OnStringLiteralExpressionAnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            OnLocaliztionLimitAnalyzeNode(context);
            // OnNewFreeMatchLimitAnalyzeNode(context);
        }

        private void OnInvocation(OperationAnalysisContext context)
        {
            OnLocaliztionLimitAnalyzeInvoke(context);
            // OnNewFreeMatchLimitAnalyzeInvoke(context);
        }

    
    }
    
    
}