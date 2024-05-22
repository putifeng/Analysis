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
            "请把中文定义在 cfg_LanguageCode.xlsx 内 或 class LanguageCode 内 ",
            "Usage",
            DiagnosticSeverity.Error);
        
    }

    public class AnalyzerDiagnostic
    {
        public DiagnosticDescriptor Rule;
        public AnalyzerDiagnostic(string id,string title,string message,string category,
            DiagnosticSeverity severity)
        {
            Rule = new DiagnosticDescriptor(id,title,message,category,severity, isEnabledByDefault: true, description: "");
        }

        public sampleDescriptor ToDiagnosticDescriptorArray()
        {
            return new sampleDescriptor(ImmutableArray.Create(Rule),Rule);
        }
    }
}