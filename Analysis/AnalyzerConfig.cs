using System;
using System.Collections.Generic;

namespace Analysis
{
    public class AnalyzerConfig
    {
        private static AnalyzerConfig? s_GlobalAnalyzerConfig;
        private static Dictionary<string,AnalyzerConfig> s_Configs = new  Dictionary<string, AnalyzerConfig>(StringComparer.CurrentCultureIgnoreCase);
        
        public static string s_ActiveConfigPath;
        public static DateTime lastWriteTime = DateTime.MinValue;

        public static AnalyzerConfig s_ActiveAnalyzerConfig = null;

        
        public static bool s_UseGlobalConfig = true;
        
        public bool CN_Limit = true;
        
        public string LanguageClassName = "LanguageCode";

        public bool NewFree_Limit = true;
        // new,free 
        public string BaseClassName = "Test.BaseView";
        public string FreeMethodName = "Free";

        
        public bool PropertyRecursion_Limit = false;
        
        public static bool TryGetValue(string [] keyValue,string key,ref bool writeValue)
        {
            if (keyValue.Length == 2)
            {
                if (string.Compare(keyValue[0].Trim(), key, StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    if (string.Compare(keyValue[1].Trim(),"true", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        writeValue = true;
                        return true;
                    }
                    else if (string.Compare(keyValue[1].Trim(), "false", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        writeValue = false;
                        return true;
                    }
                }
            }
            return false;
        }
        
        public static bool TryGetValue(string [] keyValue,string key,ref string writeValue)
        {
            if (keyValue.Length == 2)
            {
                if (string.Compare(keyValue[0].Trim(), key, StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    writeValue = keyValue[1];
                    return true;
                }
            }
            return false;
        }


        public static void SwitchActiveConfig(string assembly)
        {
            if (s_Configs.TryGetValue(assembly, out var activeAnalyzerConfig))
            {
                s_ActiveAnalyzerConfig =  activeAnalyzerConfig;
            }
            else
            {
                if(s_UseGlobalConfig)
                    s_ActiveAnalyzerConfig = s_GlobalAnalyzerConfig;
            }
        }

        public static void Clear()
        {
            s_Configs.Clear();
            lastWriteTime = DateTime.MinValue;
            s_GlobalAnalyzerConfig = null;
            s_ActiveAnalyzerConfig = null;
        }

        public static void CreateGlobalDefaultIsNull()
        {
            if(s_GlobalAnalyzerConfig == null)
                s_GlobalAnalyzerConfig = new AnalyzerConfig();
        }
        
        /*

# global 
CN_Limit=true

# custom assembly
Assembly=hotfix,_hotfix,other
CN_Limit=true
LanguageClassName=LanguageCode

NewFree_Limit=true
BaseClassName=Test.BaseView
FreeMethodName=Free

*/
        public static void CreateConfigs(string[] contexts)
        {
            s_UseGlobalConfig = true;
            s_GlobalAnalyzerConfig = new AnalyzerConfig();
            AnalyzerConfig writeConfig = s_GlobalAnalyzerConfig;
            for (var i = 0; i < contexts.Length; i++)
            {
                string context = contexts[i];
                context = context.Trim();
                if (context.StartsWith("//"))
                    continue;
                
                if (context.StartsWith("#"))
                    continue;
                
                if(string.IsNullOrEmpty(context))
                    continue;

                string[] keyValue = context.Split('=');

                bool enableGlobal = true;
                if (TryGetValue(keyValue, "IsGlobal", ref enableGlobal))
                {
                    if (!enableGlobal)
                    {
                        s_UseGlobalConfig = false;
                        s_GlobalAnalyzerConfig = null;
                    }
                    
                }
                
                
                string targetAssemblys = string.Empty;
                if (TryGetValue(keyValue, "Assembly",ref targetAssemblys))
                {
                    if (!string.IsNullOrEmpty(targetAssemblys))
                    {
                        string[] targetAssemblyArray = targetAssemblys.Split(',');
                        writeConfig = new AnalyzerConfig();
                        foreach (var se in targetAssemblyArray)
                        {
                            string targetAssembly = se.Trim();
                            s_Configs[targetAssembly] = writeConfig;
                        }
                    }
                }
                
                TryGetValue(keyValue, "CN_Limit", ref writeConfig.CN_Limit);
                
                TryGetValue(keyValue, "LanguageClassName", ref writeConfig.LanguageClassName);
                
                TryGetValue(keyValue, "NewFree_Limit", ref writeConfig.NewFree_Limit);
                TryGetValue(keyValue, "BaseClassName", ref writeConfig.BaseClassName);
                TryGetValue(keyValue, "FreeMethodName", ref writeConfig.FreeMethodName);
                TryGetValue(keyValue, "PropertyRecursion_Limit", ref writeConfig.PropertyRecursion_Limit);
            }
            
            
        }
        
    }
}