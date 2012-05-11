using System;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Aicl.DbToSS.CSharp
{
	public class SqlRolesCreator
	{
		public SqlRolesCreator ()
		{
		}
		
		public int Count {get; set;}
		public int CountAuth {get; set;}
		
		public string OutputDirectory { get; set;}
		
		public virtual void Write()
		{
			
			if(string.IsNullOrEmpty(OutputDirectory))
				OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "scripts");
			
			if(!Directory.Exists(OutputDirectory))		
				Directory.CreateDirectory(OutputDirectory);
			
			using (TextWriter twp = new StreamWriter(Path.Combine(OutputDirectory,"role.sql")))
			{
				twp.WriteLine(string.Format(sqlTemplate, "Admin"));				
				twp.Write(string.Format(sqlTemplate,"Test"));				
				twp.Close();
			}
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(OutputDirectory,"rolePermission.sql")))
			{
				for(int i=1;i<=CountAuth;i++){
					twp.WriteLine(string.Format(rolePermissionTemplate, 1, i));				
				}
				
				for(int i=CountAuth+1;i<=Count+CountAuth;i++){
					twp.WriteLine(string.Format(rolePermissionTemplate, 2, i));				
				}
				
				twp.Close();
			}
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(OutputDirectory,"roleUsuario.sql")))
			{
				twp.WriteLine(string.Format(roleUserTemplate, 1, 1));				
				twp.WriteLine(string.Format(roleUserTemplate, 2, 2));				
				twp.Close();
			}
			
			
		
		}
			
		private string sqlTemplate="INSERT INTO AUTH_ROLE 	(NAME,  SHOW_ORDER, TITLE) 	VALUES 	('{0}',  '00', '{0}');";
		
		private string roleUserTemplate="INSERT INTO AUTH_ROLE_USER(ID_AUTH_ROLE, ID_USERAUTH) VALUES({0},{1});";
		
		private string rolePermissionTemplate="INSERT INTO AUTH_ROLE_PERMISSION (ID_AUTH_ROLE, ID_AUTH_PERMISSION) VALUES ({0}, {1});";
	}
}
