using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
namespace Analysis
{
    public static class DiagnosticDescriptorHelper
    {
        public static (ImmutableArray<DiagnosticDescriptor> immutableArray,DiagnosticDescriptor descriptor) LocalizationLimitDescriptors =
            AnalyzerDiagnosticId.LocalizationLimit.ToDiagnosticDescriptorArray();
    }
}