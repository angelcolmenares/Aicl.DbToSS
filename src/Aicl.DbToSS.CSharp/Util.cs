using System;
using System.IO;
using System.Diagnostics;

namespace Aicl.DbToSS.CSharp
{
	public static class Util
	{
		
		public static  int Execute(string exe, string args)
        {
			Console.WriteLine("{0} {1}", exe, args);
            ProcessStartInfo oInfo = new ProcessStartInfo(exe, args);
            oInfo.UseShellExecute = false;
            oInfo.CreateNoWindow = true;

            oInfo.RedirectStandardOutput = true;
            oInfo.RedirectStandardError = true;

            StreamReader srOutput = null;
            StreamReader srError = null;

            Process proc = System.Diagnostics.Process.Start(oInfo);
            proc.WaitForExit();
            srOutput = proc.StandardOutput;
            StandardOutput = srOutput.ReadToEnd();
            srError = proc.StandardError;
            StandardError = srError.ReadToEnd();
            int exitCode = proc.ExitCode;
            proc.Close();
			
			Console.WriteLine("Exticode {0}", exitCode );
            return exitCode;
        }

        private static string StandardOutput
        {
            get;
            set;
        }
        
		private static string StandardError
        {
            get;
            set;
		}
		
	}
}

