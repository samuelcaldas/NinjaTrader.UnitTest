using System;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace NinjaTrader.UnitTest.TestAdapter
{
    /// <summary>
    /// Utility to locate source file path and line numbers using PDB debug symbols.
    /// </summary>
    public static class SourceCodeNavigator
    {
        public static (string FilePath, int LineNumber) GetSourceLocation(string assemblyPath, string className, string methodName)
        {
            if (string.IsNullOrEmpty(assemblyPath) || !File.Exists(assemblyPath))
                return (null, 0);

            try
            {
                using (var diaSession = new DiaSession(assemblyPath))
                {
                    var navData = diaSession.GetNavigationData(className, methodName);
                    if (navData != null && !string.IsNullOrEmpty(navData.FileName))
                    {
                        return (navData.FileName, navData.MinLineNumber);
                    }
                }
            }
            catch
            {
                // Fallback silently if PDB is unavailable or DiaSession cannot be initialized
            }

            return (null, 0);
        }
    }
}
