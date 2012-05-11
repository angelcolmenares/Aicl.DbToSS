using System;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Aicl.DbToSS.CSharp
{
	public class SqlPermissionsCreator
	{
		public SqlPermissionsCreator ()
		{
		}
		
		public string AssemblyName { get; set;}
		
		public string NameSpace {get; set;}
		
		public string OutputDirectory { get; set;}
		
		public int Count {get; private set;}
		public int CountAuth {get; private set;}
		
		public virtual void Write()
		{
		
			Count=0;
			CountAuth=0;
			
			if(string.IsNullOrEmpty(OutputDirectory))
				OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "scripts");
			
			if(!Directory.Exists(OutputDirectory))		
				Directory.CreateDirectory(OutputDirectory);
			
			var assembly = Assembly.LoadFrom(AssemblyName);
			Console.WriteLine("Starting  permissions script  generation for assembly:'{0}' ...", assembly);
			
					
			using (TextWriter twp = new StreamWriter(Path.Combine(OutputDirectory,"permission.sql")))
			{
				foreach(Type t in  assembly.GetTypes()){
					if (t.Namespace==NameSpace && ( t.Name.StartsWith("Auth") || t.Name.StartsWith("Userauth")) )
					{	
						CountAuth+=4;
						twp.WriteLine(string.Format(sqlTemplate, t.Name, "create"));				
						twp.WriteLine(string.Format(sqlTemplate, t.Name,"read"));				
						twp.WriteLine(string.Format(sqlTemplate, t.Name,"update"));				
						twp.WriteLine(string.Format(sqlTemplate, t.Name,"destroy"));				
						
					}
				}
				
				
				foreach(Type t in  assembly.GetTypes()){
					if (t.Namespace==NameSpace && !( t.Name.StartsWith("Auth") || t.Name.StartsWith("Userauth")))
					{	
						Count+=4;
						twp.WriteLine(string.Format(sqlTemplate, t.Name, "create"));				
						twp.WriteLine(string.Format(sqlTemplate, t.Name,"read"));				
						twp.WriteLine(string.Format(sqlTemplate, t.Name,"update"));				
						twp.WriteLine(string.Format(sqlTemplate, t.Name,"destroy"));				
						
					}
				}
				twp.Close();
			}
					
						
			
			Console.WriteLine("permissions script for assembly:'{0}' Done", assembly);
		
		}
		
		
		private string sqlTemplate=@"INSERT INTO AUTH_PERMISSION (NAME) VALUES ('{0}.{1}');";
		
	}
}

