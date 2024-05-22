using System.Collections.Generic;

namespace Analysis
{
    public static class Definition
    {
        public static string LanguageClassName = "LanguageCode";
        
        public static string EnableAnalysisClassName = "Assembly-CSharp";
        
        public static HashSet<string> LocalizationLimitIngores = new HashSet<string>()
        {
            "log",
            "logerror",
            "LogErrorFormat".ToLower(),
            "LogWarning".ToLower(),
            "EditorError".ToLower(),
            "EditorLog".ToLower(),
        };
        
    }
}