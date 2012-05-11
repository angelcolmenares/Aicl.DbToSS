using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Aicl.DbToSS.CSharp
{
	public class DataAccessCreator
	{
		
		public string SolutionName
		{
			get;
			set;
		}
		
		
		public string DataAccesNameSpace
		{
			get;
			set;
		}
		
		public string ModelNameSpace
		{
			get;
			set;
		}
				
		public string OutputDirectory
		{
			get;
			set;
		}
		
		
		public DataAccessCreator ()
		{
		}
		
		public virtual void Write()
		{
			if(string.IsNullOrEmpty(OutputDirectory))
				OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "src");
			
			if(string.IsNullOrEmpty( ModelNameSpace)) ModelNameSpace="Model";
			
			if(string.IsNullOrEmpty( DataAccesNameSpace)) DataAccesNameSpace="DataAccess";
			
			
			if(!Directory.Exists(OutputDirectory))		
				Directory.CreateDirectory(OutputDirectory);
			
			string dataAccessDir = Path.Combine(OutputDirectory,
			                                   string.Format("{0}.{1}",SolutionName,DataAccesNameSpace));
			
			if(!Directory.Exists(dataAccessDir))		
				Directory.CreateDirectory(dataAccessDir);		
			
			using (TextWriter twp = new StreamWriter(Path.Combine(dataAccessDir, "Factory.cs")))
			{
				twp.Write(string.Format(dataAccessTemplate,SolutionName,DataAccesNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(dataAccessDir, "ModelExtensions.cs")))
			{
				twp.Write(string.Format(extensionTemplate,SolutionName,ModelNameSpace, DataAccesNameSpace));				
				twp.Close();
			}
			
			string propertiesDir = Path.Combine(dataAccessDir,"Properties");
						
			if(!Directory.Exists(propertiesDir))		
				Directory.CreateDirectory(propertiesDir);
			
			using (TextWriter twp = new StreamWriter(Path.Combine(propertiesDir, "AssemblyInfo.cs")))
			{
				twp.Write(string.Format(assemblyTemplate,SolutionName,DataAccesNameSpace));				
				twp.Close();
			}
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(dataAccessDir,
			                            string.Format("{0}.{1}.csproj",SolutionName,DataAccesNameSpace))))
			{
				twp.Write(string.Format(projectTemplate,SolutionName,DataAccesNameSpace,ModelNameSpace));				
				twp.Close();
			}
			
		}
		
		private string dataAccessTemplate=@"using System;
using System.Reflection;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using ServiceStack.Common.Utils;
using ServiceStack.DesignPatterns.Model;

namespace {0}.{1}
{{
	public class Factory
	{{
		public IDbConnectionFactory DbFactory {{internal get;set;}}
		
		public Factory ()
		{{
		}}
		
		public  List<T> Get<T> (T request) where T:new()
		{{
			Type type = typeof(T);
			string id =string.Empty;
			PropertyInfo pi= ReflectionUtils.GetPropertyInfo(type, OrmLiteConfig.IdField);
			if( pi!=null ){{
				id= pi.GetValue(request, new object[]{{}}).ToString();
			}}
											
			return (string.IsNullOrEmpty(id) || id==""0"")? 
				DbFactory.Exec(	dbCmd => dbCmd.Select<T>()):
				GetById<T>(id);
		}}
		
		
		public  List<T> GetById<T> ( object id) where T:new()
		{{
			List<T> l = new List<T>();
			
			try{{
				T r = DbFactory.Exec(
					dbCmd => 
					dbCmd.GetById<T>(id)
					);
						
				l.Add(r);
			}}
			catch(System.ArgumentNullException) {{
			}}
			
			return l;
		}}
		
		public  List<T> Post<T> (T request) where T:new(){{

			DbFactory.Exec(	(dbCmd) =>
			{{ 
				dbCmd.Insert<T>(request);
				Type type = typeof(T);

				PropertyInfo pi= ReflectionUtils.GetPropertyInfo(type, OrmLiteConfig.IdField);
			
				if( pi!=null && pi.GetValue(request, new object[]{{}}).ToString() ==""0""){{
					var li = dbCmd.GetLastInsertId();
					if(pi.PropertyType == typeof(short))
						ReflectionUtils.SetProperty(request, pi, Convert.ToInt16(li));	
					else if(pi.PropertyType == typeof(int))
						ReflectionUtils.SetProperty(request, pi, Convert.ToInt32(li));	
					else
					ReflectionUtils.SetProperty(request, pi, Convert.ToInt64(li));
				}}
			}});
			
			List<T> l = new List<T>();
			
			
						
			l.Add(request);
			
			return l;
		}}
		
		
		public  List<T> Put<T> (T request) where T:new(){{

			DbFactory.Exec(
					dbCmd => 
					dbCmd.Update<T>(request)
			);
			List<T> l = new List<T>();
			l.Add(request);
			return l;
		}}
		
		public  List<T> Delete<T> (T request) where T:new(){{

			DbFactory.Exec(
					dbCmd => 
					dbCmd.Delete<T>(request)
			);
			return new List<T>();

		}}
		
	}}
}}";
		
		private string assemblyTemplate=@"using System.Reflection;
using System.Runtime.CompilerServices;

// Information about this assembly is defined by the following attributes. 
// Change them to the values specific to your project.

[assembly: AssemblyTitle(""{0}.{1}"")]
[assembly: AssemblyDescription("""")]
[assembly: AssemblyConfiguration("""")]
[assembly: AssemblyCompany(""aicl"")]
[assembly: AssemblyProduct("""")]
[assembly: AssemblyCopyright(""angel"")]
[assembly: AssemblyTrademark(""aicl"")]
[assembly: AssemblyCulture("""")]

// The assembly version has the format ""{{Major}}.{{Minor}}.{{Build}}.{{Revision}}"".
// The form ""{{Major}}.{{Minor}}.*"" will automatically update the build and revision,
// and ""{{Major}}.{{Minor}}.{{Build}}.*"" will update just the revision.

[assembly: AssemblyVersion(""1.0.*"")]

// The following attributes are used to specify the signing key for the assembly, 
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("""")]";
	
		private string extensionTemplate=@"using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using ServiceStack.Common.Utils;
using ServiceStack.DesignPatterns.Model;
using ServiceStack.ServiceInterface.Auth;

using {0}.{1}.Types;
using {0}.{1}.Operations;

namespace {0}.{2}
{{
	public static class ModelExtensions
	{{
				
		public static AuthorizationResponse GetRolesAndPermissions(this Authorization request, 
		                                           Factory factory){{
			
			StringBuilder  var1 = new StringBuilder();
			var1.Append(""SELECT b.ID as \""IdRole\"",\n b.name       AS \""Role\"", \n"");
			var1.Append(""       b.directory  AS \""Directory\"", \n"");
			var1.Append(""       b.show_order AS \""ShowOrder\"", \n"");
			var1.Append(""       d.name       AS \""Permission\"", \n"");
			var1.Append(""       b.title  AS \""Title\"" \n"");
			var1.Append(""FROM   auth_role_user a \n"");
			var1.Append(""       JOIN auth_role b \n"");
			var1.Append(""         ON b.id = a.id_auth_role \n"");
			var1.Append(""       JOIN auth_role_permission c \n"");
			var1.Append(""         ON c.id_auth_role = b.id \n"");
			var1.Append(""       JOIN auth_permission d \n"");
			var1.Append(""         ON d.id = c.id_auth_permission \n"");
			var1.AppendFormat(""WHERE  a.id_userauth = '{{0}}' order by \""ShowOrder\""  "", request.UserId);
			
			List<RoleAndPermission> ur= factory.DbFactory.Exec(dbCmd=> dbCmd.Select<RoleAndPermission>
			                      (var1.ToString()));
						
			List<string> permissions = ((from s in ur
                   select s.Permission).Distinct()).ToList() ;
			
			
			List<AuthRole> roles = new List<AuthRole>();
			
			var rolId=
					(from gr in ur
					orderby gr.ShowOrder
					select gr.IdRole).Distinct().ToList();
			
			foreach (var id in rolId){{
				var gr = ur.FirstOrDefault( r=>r.IdRole== id);
				roles.Add( new AuthRole (){{
					Id= id,
					Name= gr.Role,
					Directory=gr.Directory,
					ShowOrder=gr.ShowOrder,
					Title= gr.Title
				}});
			}}
			
			return new AuthorizationResponse(){{
				Permissions= permissions,
				Roles= roles
			}};
		}}
		
	}}
}}";
	
		private string projectTemplate=@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project DefaultTargets=""Build"" ToolsVersion=""3.5"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <ProductVersion>9.0.21022</ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>{{C0319E77-DA72-47AD-A64F-5E25C634DC4B}}</ProjectGuid>
    <OutputType>Library</OutputType>
    <RootNamespace>{0}.{1}</RootNamespace>
    <AssemblyName>{0}.{1}</AssemblyName>
    <TargetFrameworkVersion>v3.5</TargetFrameworkVersion>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug</OutputPath>
    <DefineConstants>DEBUG;</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <ConsolePause>false</ConsolePause>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">
    <DebugType>none</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Release</OutputPath>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <ConsolePause>false</ConsolePause>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include=""System"" />
    <Reference Include=""System.Core"" />
    <Reference Include=""System.Data"" />
    <Reference Include=""ServiceStack.Common"">
      <HintPath>..\..\lib\ServiceStack.Common.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack"">
      <HintPath>..\..\lib\ServiceStack.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack.Interfaces"">
      <HintPath>..\..\lib\ServiceStack.Interfaces.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack.OrmLite"">
      <HintPath>..\..\lib\ServiceStack.OrmLite.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack.ServiceInterface"">
      <HintPath>..\..\lib\ServiceStack.ServiceInterface.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack.Text"">
      <HintPath>..\..\lib\ServiceStack.Text.dll</HintPath>
    </Reference>
  </ItemGroup>
  <ItemGroup>
    <Compile Include=""Properties\AssemblyInfo.cs"" />
    <Compile Include=""Factory.cs"" />
    <Compile Include=""ModelExtensions.cs"" />
  </ItemGroup>
  <Import Project=""$(MSBuildBinPath)\Microsoft.CSharp.targets"" />
  <ItemGroup>
    <Folder Include=""Properties\"" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""..\{0}.{2}\{0}.{2}.csproj"">
      <Project>{{E0C40BDB-3D96-4FE3-850C-944FFFD63F8C}}</Project>
      <Name>{0}.{2}</Name>
    </ProjectReference>
  </ItemGroup>
</Project>";
		
	}
}

