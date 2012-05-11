using System;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Aicl.DbToSS.CSharp
{
	public class SqlCreator
	{
		public SqlCreator ()
		{
		}
		
		
		public string SolutionName
		{
			get;
			set;
		}
								
		public string OutputDirectory
		{
			get;
			private set;
		}
		
		public string AssemblyName{
			get;
			set;
		}
		
		public string NameSpace{
			get;
			set;
		}
		
		public virtual void Write()
		{
			string solutionDir=Path.Combine(Directory.GetCurrentDirectory(), SolutionName);
			
			if(!Directory.Exists(solutionDir))		
				Directory.CreateDirectory(solutionDir);
								
			OutputDirectory = Path.Combine(solutionDir, "scripts");
			
			if(!Directory.Exists(OutputDirectory))		
				Directory.CreateDirectory(OutputDirectory);
			
			
			SqlPermissionsCreator sqlPermissions= new SqlPermissionsCreator(){
				AssemblyName=AssemblyName,
				OutputDirectory=OutputDirectory,
				NameSpace=NameSpace
			};
			sqlPermissions.Write();
			
			
			
			SqlRolesCreator rc = new SqlRolesCreator(){
				OutputDirectory= OutputDirectory,
				Count= sqlPermissions.Count,
				CountAuth=sqlPermissions.CountAuth
			};
			rc.Write();
			
			
		}		
	}
}

