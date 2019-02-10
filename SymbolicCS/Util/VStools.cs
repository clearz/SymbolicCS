using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SymbolicCS.Util
{
    static class VsTools
    {
        private static readonly string Name;
        static VsTools()
        {
            for (int i = 10; i <= 20; i++) // Not sure if there is a less hacky way to find the version of VS
            {
                var name = $"VisualStudio.DTE.{i}.0";
                if(Type.GetTypeFromProgID(name, false) != null)
                {
                    Name = name;
                    break;
                }
            }
        }
        public static void Break(int line, string file)
        {
            var ide = (EnvDTE.DTE)Marshal.GetActiveObject(Name);
            if (ide != null)
            {
                ide.Debugger.Breakpoints.Add(Line: line, File: file);
                Marshal.ReleaseComObject(ide);
            }
        }

        [Conditional("DEBUG")]
        public static void ClearOutputWindow()
        {
            if (!Debugger.IsAttached)
            {
                return;
            }


            Thread.Sleep(100);
            // In VS2008 use EnvDTE80.DTE2
            var ide = (EnvDTE.DTE)Marshal.GetActiveObject(Name);
            if (ide != null)
            {
                ide.ExecuteCommand("Edit.ClearOutputWindow", "");
                Marshal.ReleaseComObject(ide);
            }
        }
    }
}
