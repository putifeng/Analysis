using System;
using System.Collections.Immutable;
using System.Net.Mail;
using Microsoft.CodeAnalysis;

namespace Analysis
{
    using sampleDescriptor = ValueTuple<ImmutableArray<DiagnosticDescriptor>,DiagnosticDescriptor>;
    public static class AnalyzerDiagnosticId
    {
        public static AnalyzerDiagnostic LocalizationLimit = new AnalyzerDiagnostic(
            "P00001",
            "P00001",
            "请把中文定义在指定的类型内",
            "Usage",
            DiagnosticSeverity.Error);
        
        
        public static AnalyzerDiagnostic NewFreeLimit = new AnalyzerDiagnostic(
            "P00101",
            "P00101",
            "对象未释放",
            "Usage",
            DiagnosticSeverity.Error);

        
        public static AnalyzerDiagnostic RecursionLimit = new AnalyzerDiagnostic(
            "P00102",
            "P00102",
            "禁止递归属性",
            "Usage",
            DiagnosticSeverity.Error);

        public static ImmutableArray<DiagnosticDescriptor> TotalFree => ImmutableArray.Create(LocalizationLimit.Rule,NewFreeLimit.Rule,RecursionLimit.Rule);

    }

    public class AnalyzerDiagnostic
    {
        public DiagnosticDescriptor Rule;
        public AnalyzerDiagnostic(string id,string title,string message,string category,
            DiagnosticSeverity severity)
        {
            Rule = new DiagnosticDescriptor(id,title,message,category,severity, isEnabledByDefault: true, description: "");
        }

  
    }
}