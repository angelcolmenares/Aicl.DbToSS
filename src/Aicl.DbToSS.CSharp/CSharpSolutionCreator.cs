using System;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Data;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Firebird;
using ServiceStack.OrmLite.Firebird.DbSchema;

namespace Aicl.DbToSS.CSharp
{
	public class CSharpSolutionCreator
	{
		public CSharpSolutionCreator ()
		{
		}
		
		public string ConnectionString
		{
			get;
			set;
		}
		
		public IOrmLiteDialectProvider DialectProvider
		{
			get;
			set;
		}
		
		public string SolutionName
		{
			get;
			set;
		}
		
		public string ServiceNameSpace
		{
			get;
			set;
		}
		
		public string ModelNameSpace
		{
			get;
			set;
		}
		
		public string DataAccesNameSpace
		{
			get;
			set;
		}
		
		
		public string HostWebNameSpace
		{
			get;
			set;
		}
		
		public string AppTitle
		{
			get;
			set;
		}
						
		public string OutputDirectory
		{
			get;
			private set;
		}
		
		public string OutputAssembly{
			get;
			private set;
		}
		
		public string SourceLibDir
		{
			get;
			set;
		}
		
		public virtual void Write()
		{
			string solutionDir=Path.Combine(Directory.GetCurrentDirectory(), SolutionName);
			
			if(!Directory.Exists(solutionDir))		
				Directory.CreateDirectory(solutionDir);
								
			OutputDirectory = Path.Combine(solutionDir, "src");
			
			if(!Directory.Exists(OutputDirectory))		
				Directory.CreateDirectory(OutputDirectory);
			
			if(string.IsNullOrEmpty( ServiceNameSpace)) ServiceNameSpace="Interface";
			
			if(string.IsNullOrEmpty( ModelNameSpace)) ModelNameSpace="Model";
			
			if(string.IsNullOrEmpty( DataAccesNameSpace)) DataAccesNameSpace="DataAccess";
			
			if(string.IsNullOrEmpty( HostWebNameSpace)) HostWebNameSpace="Host.Web";
			
			if(string.IsNullOrEmpty( AppTitle)) AppTitle=SolutionName;
								
			using (TextWriter twp = new StreamWriter(Path.Combine(OutputDirectory,
			                            string.Format("{0}.sln",SolutionName))))
			{
				twp.Write(string.Format(solutionTemplate,SolutionName,ModelNameSpace,
				                        DataAccesNameSpace,ServiceNameSpace,
				                        HostWebNameSpace ));				
				twp.Close();
			}
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(solutionDir,".gitignore")))
			{
				twp.Write(string.Format(gitIgnoreTemplate, SolutionName, HostWebNameSpace) );				
				twp.Close();
			}
			
						
			OrmLiteConfig.DialectProvider = DialectProvider;
			using (IDbConnection db =
				      ConnectionString.OpenDbConnection())
			using ( IDbCommand dbConn = db.CreateCommand())
			{
				Schema fbd= new Schema(){
					Connection = db
				};
				
				PocoCreator cw = new PocoCreator(){
					OutputDirectory=OutputDirectory,
					SolutionName=SolutionName,
					Schema =fbd
				};
				
				foreach(var t in fbd.Tables){
					Console.Write("Generating POCO Class for table:'{0}'...", t.Name);
					cw.WriteClass(t);	
					Console.WriteLine(" Done.");
				}
				Console.WriteLine("See classes in: '{0}'", cw.OutputDirectory);
								
				CompilerParameters cp = new CompilerParameters();
				cp.GenerateExecutable=false;
				cp.GenerateInMemory=false;
				cp.ReferencedAssemblies.AddRange(
					new string[]{
						"System.dll",
						"System.ComponentModel.DataAnnotations.dll",
						Path.Combine( Directory.GetCurrentDirectory(), "ServiceStack.OrmLite.dll"),
						Path.Combine( Directory.GetCurrentDirectory(), "ServiceStack.Common.dll"),
						Path.Combine( Directory.GetCurrentDirectory(),"ServiceStack.Interfaces.dll")
				});
				cp.OutputAssembly= Path.Combine(cw.OutputDirectory, 
				                               string.Format("{0}.{1}.Types.dll", SolutionName, cw.ModelNameSpace));
				
				var providerOptions = new Dictionary<string,string>();
    			providerOptions.Add("CompilerVersion", "v3.5");
				
				CodeDomProvider cdp =new CSharpCodeProvider(providerOptions);
			
				string [] files = Directory.GetFiles(cw.TypesDirectory,"*.cs");
				CompilerResults cr= cdp.CompileAssemblyFromFile(cp, files);
				
				if( cr.Errors.Count==0){
					Console.WriteLine("Generated types dll {0}", cp.OutputAssembly );
					OutputAssembly= cp.OutputAssembly;
				}
            	else{							
            		foreach (CompilerError ce in cr.Errors)
                		Console.WriteLine(ce.ErrorText);
					return;
				}
				
				Console.WriteLine("Starting interfaces generation...");
				
				ServiceInterfaceCreator si= new ServiceInterfaceCreator(){
					SolutionName=SolutionName,
					OutputDirectory=OutputDirectory
				};
				
				var assembly = Assembly.LoadFrom(cp.OutputAssembly);
				foreach(Type t in  assembly.GetTypes()){
					if( forbiddenServices.Contains(t.Name)) continue;
					Console.Write("Generating interface  for class:'{0}'...", t.FullName);
					si.ClassName= t.Name;
					si.Write();
					Console.WriteLine(" Done.");
				}
				Console.WriteLine("Interfaces generation: Done");
			}
						
			
			Console.WriteLine("Starting DataAccess generation...");
			DataAccessCreator da = new DataAccessCreator(){
				SolutionName=SolutionName,
				OutputDirectory=OutputDirectory
			};
			
			da.Write();
			
			Console.WriteLine("DataAccess generation: Done");
			
			Console.WriteLine("Starting WebHost generation...");
			AppHostCreator ah = new AppHostCreator() {
				SolutionName=SolutionName,
				OutputDirectory=OutputDirectory,
				ConnectionString=ConnectionString
			};
			
			ah.Write();
			
			Console.WriteLine("WebHost generation: Done");
			
			
			Console.WriteLine("Starting Setup generation...");
			SetupCreator st = new SetupCreator {
				SolutionName=SolutionName,
				OutputDirectory=OutputDirectory,
				ConnectionString=ConnectionString
			};
			
			st.Write();
			
			Console.WriteLine("Setup generation: Done");
						
			
			string libDir = Path.Combine(solutionDir, "lib");
			string scriptsDir = Path.Combine(solutionDir, "scripts");
			
			if(!Directory.Exists(libDir))		
				Directory.CreateDirectory(libDir);
			
			if(!Directory.Exists(scriptsDir))		
				Directory.CreateDirectory(scriptsDir);
			
			
			if(!string.IsNullOrEmpty(SourceLibDir) && Directory.Exists(SourceLibDir)){
				
				string[] files = System.IO.Directory.GetFiles(SourceLibDir);
                foreach (string s in files)
            	{
					string fileName = System.IO.Path.GetFileName(s);
					Console.WriteLine("Coping '{0}' ...", fileName);
                	string destFile = System.IO.Path.Combine(libDir, fileName);
                	System.IO.File.Copy(s, destFile, true);
            	}
				
			}
			
			string webAppdir=Path.Combine(
				Path.Combine(OutputDirectory, string.Format("{0}.{1}",SolutionName,HostWebNameSpace)),
				"WebApp");
						
			Util.Execute("ln", string.Format(" -s {0} {1}",
			                                 Path.Combine(OutputDirectory, string.Format("{0}.{1}",SolutionName,"WebApp")),
			                                 webAppdir)); 
			
			
		}	
				
		private string solutionTemplate=@"Microsoft Visual Studio Solution File, Format Version 10.00
# Visual Studio 2008
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{0}.{1}"", ""{0}.{1}\{0}.{1}.csproj"", ""{{E0C40BDB-3D96-4FE3-850C-944FFFD63F8C}}""
EndProject
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{0}.{2}"", ""{0}.{2}\{0}.{2}.csproj"", ""{{C0319E77-DA72-47AD-A64F-5E25C634DC4B}}""
EndProject
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{0}.{3}"", ""{0}.{3}\{0}.{3}.csproj"", ""{{A04440F6-FF47-4E5B-9B61-C2EC27DC52F9}}""
EndProject
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{0}.{4}"", ""{0}.{4}\{0}.{4}.csproj"", ""{{B0D07483-B16A-4AE8-92B9-B7EE01CEE11F}}""
EndProject
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{0}.Setup"", ""{0}.Setup\{0}.Setup.csproj"", ""{{DDCCAE05-5A85-4E49-9A91-1F5DDAAB2B64}}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|x86 = Debug|x86
		Release|x86 = Release|x86
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{{78B81B64-DC50-49F5-80D3-C54438BC8336}}.Debug|x86.ActiveCfg = Debug|x86
		{{78B81B64-DC50-49F5-80D3-C54438BC8336}}.Debug|x86.Build.0 = Debug|x86
		{{78B81B64-DC50-49F5-80D3-C54438BC8336}}.Release|x86.ActiveCfg = Release|x86
		{{78B81B64-DC50-49F5-80D3-C54438BC8336}}.Release|x86.Build.0 = Release|x86
		{{A04440F6-FF47-4E5B-9B61-C2EC27DC52F9}}.Debug|x86.ActiveCfg = Debug|Any CPU
		{{A04440F6-FF47-4E5B-9B61-C2EC27DC52F9}}.Debug|x86.Build.0 = Debug|Any CPU
		{{A04440F6-FF47-4E5B-9B61-C2EC27DC52F9}}.Release|x86.ActiveCfg = Release|Any CPU
		{{A04440F6-FF47-4E5B-9B61-C2EC27DC52F9}}.Release|x86.Build.0 = Release|Any CPU
		{{B0D07483-B16A-4AE8-92B9-B7EE01CEE11F}}.Debug|x86.ActiveCfg = Debug|Any CPU
		{{B0D07483-B16A-4AE8-92B9-B7EE01CEE11F}}.Debug|x86.Build.0 = Debug|Any CPU
		{{B0D07483-B16A-4AE8-92B9-B7EE01CEE11F}}.Release|x86.ActiveCfg = Release|Any CPU
		{{B0D07483-B16A-4AE8-92B9-B7EE01CEE11F}}.Release|x86.Build.0 = Release|Any CPU
		{{C0319E77-DA72-47AD-A64F-5E25C634DC4B}}.Debug|x86.ActiveCfg = Debug|Any CPU
		{{C0319E77-DA72-47AD-A64F-5E25C634DC4B}}.Debug|x86.Build.0 = Debug|Any CPU
		{{C0319E77-DA72-47AD-A64F-5E25C634DC4B}}.Release|x86.ActiveCfg = Release|Any CPU
		{{C0319E77-DA72-47AD-A64F-5E25C634DC4B}}.Release|x86.Build.0 = Release|Any CPU
		{{DDCCAE05-5A85-4E49-9A91-1F5DDAAB2B64}}.Debug|x86.ActiveCfg = Debug|x86
		{{DDCCAE05-5A85-4E49-9A91-1F5DDAAB2B64}}.Debug|x86.Build.0 = Debug|x86
		{{DDCCAE05-5A85-4E49-9A91-1F5DDAAB2B64}}.Release|x86.ActiveCfg = Release|x86
		{{DDCCAE05-5A85-4E49-9A91-1F5DDAAB2B64}}.Release|x86.Build.0 = Release|x86
		{{E0C40BDB-3D96-4FE3-850C-944FFFD63F8C}}.Debug|x86.ActiveCfg = Debug|Any CPU
		{{E0C40BDB-3D96-4FE3-850C-944FFFD63F8C}}.Debug|x86.Build.0 = Debug|Any CPU
		{{E0C40BDB-3D96-4FE3-850C-944FFFD63F8C}}.Release|x86.ActiveCfg = Release|Any CPU
		{{E0C40BDB-3D96-4FE3-850C-944FFFD63F8C}}.Release|x86.Build.0 = Release|Any CPU
		{{E4981EAA-AAAF-447D-B26A-3108D5DFE38F}}.Debug|x86.ActiveCfg = Debug|x86
		{{E4981EAA-AAAF-447D-B26A-3108D5DFE38F}}.Debug|x86.Build.0 = Debug|x86
		{{E4981EAA-AAAF-447D-B26A-3108D5DFE38F}}.Release|x86.ActiveCfg = Release|x86
		{{E4981EAA-AAAF-447D-B26A-3108D5DFE38F}}.Release|x86.Build.0 = Release|x86
		{{2E93AB30-20E5-42B7-95C4-1FA8DBE5310A}}.Debug|x86.ActiveCfg = Debug|x86
		{{2E93AB30-20E5-42B7-95C4-1FA8DBE5310A}}.Debug|x86.Build.0 = Debug|x86
		{{2E93AB30-20E5-42B7-95C4-1FA8DBE5310A}}.Release|x86.ActiveCfg = Release|x86
		{{2E93AB30-20E5-42B7-95C4-1FA8DBE5310A}}.Release|x86.Build.0 = Release|x86
	EndGlobalSection
	GlobalSection(MonoDevelopProperties) = preSolution
		StartupItem = {0}.{4}\{0}.{4}.csproj
	EndGlobalSection
EndGlobal";
		
		
		private List<string> forbiddenServices= new List<string>(new []
		{
			"Authentication",
			"Authorization",
			"AuthPermission",
			"AuthRole",
			"AuthRolPermission",
			"AuthRolePermission",
			"AuthRoleUser",
			"AuthRolUser",	
			"Userauth",
			"UserAuth",
			"Useroauthprovider",
			"UserOAuthProvider"
		});
		
		private string gitIgnoreTemplate=@"bin/
obj/
.idea/
latest/
/env-vars.bat
*.suo
#ignore thumbnails created by windows
Thumbs.db
#Ignore files build by Visual Studio
*.obj
*.exe
*.pdb
*.user
*.aps
*.pch
*.vspscc
*_i.c
*_p.c
*.ncb
*.suo
*.tlb
*.tlh
*.bak
*.cache
*.ilk
*.log
[Bb]in
[Dd]ebug*/
*.lib
*.sbr
*.resharper.user
obj/
[Rr]elease*/
_ReSharper*/
[Tt]est[Rr]esult*
App_Data/

NuGet/*.dll
*.nupkg
*.pidb
*.userprefs
add.sh
.gitignore
extjs
ux
photos
{0}.{1}/WebApp";
		
	}
}

