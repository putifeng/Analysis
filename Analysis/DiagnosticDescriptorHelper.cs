using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
namespace Analysis
{
    public static class DiagnosticDescriptorHelper
    {
        public static DiagnosticDescriptor LocalizationLimitDescriptors => AnalyzerDiagnosticId.LocalizationLimit.Rule;

        public static DiagnosticDescriptor NewFreeMatchDescriptors =>
                AnalyzerDiagnosticId.NewFreeLimit.Rule;
        
        public static DiagnosticDescriptor RecursionLimitDescriptors =>
            AnalyzerDiagnosticId.RecursionLimit.Rule;

        public static ImmutableArray<DiagnosticDescriptor> TotalDescriptors => AnalyzerDiagnosticId.TotalFree;

        
    }
}