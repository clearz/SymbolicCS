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
        private static EnvDTE80.DTE2 vs_env = (EnvDTE80.DTE2)Marshal.GetActiveObject("VisualStudio.DTE.15.0");
        public static void Break(int line, string file)
        {
            vs_env.Debugger.Breakpoints.Add(Line: line, File: file);
        }

        public static void ClearOutputWindow()
        {
            if (!Debugger.IsAttached)
            {
                return;
            }

            //Application.DoEvents();  // This is for Windows.Forms.
            // This delay to get it to work. Unsure why. See http://stackoverflow.com/questions/2391473/can-the-visual-studio-debug-output-window-be-programatically-cleared
            Thread.Sleep(100);
            // In VS2008 use EnvDTE80.DTE2
            EnvDTE.DTE ide = (EnvDTE.DTE)Marshal.GetActiveObject("VisualStudio.DTE.15.0");
            if (ide != null)
            {
                ide.ExecuteCommand("Edit.ClearOutputWindow", "");
                Marshal.ReleaseComObject(ide);
            }
        }
    }
}
