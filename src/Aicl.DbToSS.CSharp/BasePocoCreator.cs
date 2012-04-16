using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Firebird.DbSchema;

namespace Aicl.DbToSS.CSharp
{
	public abstract class BasePocoCreator<TTable, TColumn, TProcedure, TParameter>
		where TTable : ITable, new()
		where TColumn : IColumn, new()
		where TProcedure : IProcedure, new()
		where TParameter : IParameter, new(){
		
		public BasePocoCreator()
		{

			Usings ="using System;\n" +
					"using System.ComponentModel.DataAnnotations;\n" +
					"using ServiceStack.Common;\n" +
					"using ServiceStack.DataAnnotations;\n" +
					"using ServiceStack.DesignPatterns.Model;\n";

			MetadataClassName="Me";
			IdField = OrmLiteConfig.IdField;
		}

		public bool GenerateMetadata{get;set;}
		
		public string MetadataClassName{get; set;}
		
		public string IdField{ get; set;}
		
		public string SolutionName
		{
			get;
			set;
		}
		
		public string ModelNameSpace
		{
			get;
			set;
		}
		
		public string Usings
		{
			get;
			set;
		}

		public string OutputDirectory
		{
			get;
			set;
		}
		
		public string TypesDirectory
		{
			get;
			private set;
		}
		
		public ISchema<TTable, TColumn, TProcedure, TParameter> Schema
		{
			get;
			set;
		}

		public virtual void WriteClass(TTable table)
		{
			WriteClass(table, table.Name);
		}

		public virtual void WriteClass(TTable table, string className)
		{
			if(string.IsNullOrEmpty( ModelNameSpace)) ModelNameSpace="Model";
			
			if(string.IsNullOrEmpty(OutputDirectory)) 
				OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "src");
						
			className = ToDotName(className);
			StringBuilder properties= new StringBuilder();
			StringBuilder meProperties= new StringBuilder();
			List<TColumn> columns = Schema.GetColumns(table.Name);

			bool hasIdField = columns.Count(r => ToDotName(r.Name) == IdField) == 1;
			string idType= string.Empty;

			foreach (var cl in columns)
			{
				properties.AppendFormat("\t\t[Alias(\"{0}\")]\n", cl.Name);
				if (!string.IsNullOrEmpty(cl.Sequence)) properties.AppendFormat("\t\t[Sequence(\"{0}\")]\n", cl.Sequence);
				if (cl.IsPrimaryKey) properties.Append("\t\t[PrimaryKey]\n");
				if (cl.AutoIncrement) properties.Append("\t\t[AutoIncrement]\n");
				if ( TypeToString(cl.NetType)=="System.String"){
					if (!cl.Nullable) properties.Append("\t\t[Required]\n");
					properties.AppendFormat("\t\t[StringLength({0})]\n",cl.Length);
				}
				if(cl.DbType.ToUpper()=="DECIMAL" || cl.DbType.ToUpper()=="NUMERIC")
					properties.AppendFormat("\t\t[DecimalLength({0},{1})]\n",cl.Presicion, cl.Scale);
				if (cl.IsComputed) properties.Append("\t\t[Compute]\n");
					
				string propertyName;
				if(cl.AutoIncrement && cl.IsPrimaryKey && !hasIdField){
					propertyName= IdField;
					idType = TypeToString(cl.NetType);
					hasIdField=true;
				}
				else{
					propertyName= ToDotName(cl.Name);
					if(propertyName==IdField) idType= TypeToString(cl.NetType);
					else if(propertyName==className) propertyName= propertyName+"Name";
				}
				
				properties.AppendFormat("\t\tpublic {0}{1} {2} {{ get; set;}} \n\n",
										TypeToString(cl.NetType),
										(cl.Nullable && cl.NetType != typeof(string)) ? "?" : "",
										 propertyName);
				
				if(GenerateMetadata){
					if(meProperties.Length==0)
						meProperties.AppendFormat("\n\t\t\tpublic static string ClassName {{ get {{ return \"{0}\"; }}}}",
						                          className);
					meProperties.AppendFormat("\n\t\t\tpublic static string {0} {{ get {{ return \"{0}\"; }}}}",
					                         propertyName);
				}
				
			}
				    
			if (!Directory.Exists(OutputDirectory))
				Directory.CreateDirectory(OutputDirectory);
			
			string modelDir = Path.Combine(OutputDirectory,
			                                   string.Format("{0}.{1}",SolutionName,ModelNameSpace));
			
			if(!Directory.Exists(modelDir))		
				Directory.CreateDirectory(modelDir);
			
			TypesDirectory=Path.Combine(modelDir,"Types");
			
			if(!Directory.Exists(TypesDirectory))		
				Directory.CreateDirectory(TypesDirectory);
						
			string attrDir=Path.Combine(modelDir,"Attributes");
			if(!Directory.Exists(attrDir))		
				Directory.CreateDirectory(attrDir);
			
			
			string operDir=Path.Combine(modelDir,"Operations");
			if(!Directory.Exists(operDir))		
				Directory.CreateDirectory(operDir);
					
			string propertiesDir = Path.Combine(modelDir,"Properties");
						
			if(!Directory.Exists(propertiesDir))		
				Directory.CreateDirectory(propertiesDir);
			
			using (TextWriter twp = new StreamWriter(Path.Combine(propertiesDir, "AssemblyInfo.cs")))
			{
				twp.Write(string.Format(assemblyTemplate,SolutionName,ModelNameSpace));				
				twp.Close();
			}
			
			using (TextWriter tw = new StreamWriter(Path.Combine(TypesDirectory, className + ".cs")))
			{
				StringBuilder ns = new StringBuilder();
				StringBuilder cl =  new StringBuilder();
				StringBuilder me = new StringBuilder();
				cl.AppendFormat("\t[Alias(\"{0}\")]\n", table.Name);
				if(GenerateMetadata){
					me.AppendFormat("\n\t\tpublic static class {0} {{\n\t\t\t{1}\n\n\t\t}}\n",
					                MetadataClassName, meProperties.ToString());
					
				}
				cl.AppendFormat("\tpublic partial class {0}{1}{{\n\n\t\tpublic {0}(){{}}\n\n{2}{3}\t}}",
								className, 
				                hasIdField?string.Format( ":IHasId<{0}>",idType):"", 
				                properties.ToString(),
				                me.ToString());
			
				ns.AppendFormat("namespace {0}.{1}.Types\n{{\n{2}\n}}", 
				                SolutionName,ModelNameSpace, cl.ToString());
				tw.WriteLine(Usings);
				tw.WriteLine(ns.ToString());	
				
				tw.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(attrDir, className + ".cs")))
			{
				twp.Write(string.Format(partialTemplate,SolutionName,className,IdField,ModelNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(attrDir,  "Authentication.cs")))
			{
				twp.Write(string.Format(authenticationAttributeTemplate,SolutionName,ModelNameSpace));				
				twp.Close();
			}
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(attrDir,  "AuthRole.cs")))
			{
				twp.Write(string.Format(authRoleAttribute,SolutionName,ModelNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(TypesDirectory,  "Authentication.cs")))
			{
				twp.Write(string.Format(authenticationTemplate,SolutionName,ModelNameSpace));				
				twp.Close();
			}
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(TypesDirectory,  "Authorization.cs")))
			{
				twp.Write(string.Format(authorizationTemplate,SolutionName,ModelNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(TypesDirectory,  "RoleAndPermission.cs")))
			{
				twp.Write(string.Format(roleAndPermissionTemplate,SolutionName,ModelNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(TypesDirectory,  "AuthRole.cs")))
			{
				twp.Write(string.Format(authRoleTemplate,SolutionName,ModelNameSpace));				
				twp.Close();
			}
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(operDir,  "AuthenticationResponse.cs")))
			{
				twp.Write(string.Format(authenticationResponseTemplate,SolutionName,ModelNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(operDir,  "AuthorizationResponse.cs")))
			{
				twp.Write(string.Format(authorizationResponseTemplate,SolutionName,ModelNameSpace));				
				twp.Close();
			}
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(operDir,  "Response.cs")))
			{
				twp.Write(string.Format(responseTemplate,SolutionName,ModelNameSpace));				
				twp.Close();
			}
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(modelDir,
			                            string.Format("{0}.{1}.csproj",SolutionName,ModelNameSpace))))
			{
				twp.Write(string.Format(projectTemplate,SolutionName,ModelNameSpace));				
				twp.Close();
			}
			
		}

		public virtual void WriteClass(TProcedure procedure)
		{
			WriteClass(procedure, procedure.Name);
		}

		public virtual  void WriteClass(TProcedure procedure, string className)
		{

		}

		protected string ToDotName(string name)
		{

			StringBuilder t = new StringBuilder();
			string [] parts = name.Split('_');
			foreach (var s in parts)
			{
				t.Append(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower()));
			}
			return t.ToString();
		}


		protected string TypeToString(Type type)
		{
			string st = type.ToString();
			return (!st.Contains("[")) ? st : st.Substring(st.IndexOf("[") + 1, st.IndexOf("]") - st.IndexOf("[") - 1);
		}
		
		private string partialTemplate=@"using System;
using ServiceStack.ServiceHost;

namespace {0}.{3}.Types
{{
	[RestService(""/{1}/create"",""post"")]
	[RestService(""/{1}/read"",""get"")]
	[RestService(""/{1}/read/{{{2}}}"",""get"")]
	[RestService(""/{1}/update/{{{2}}}"",""put"")]
	[RestService(""/{1}/destroy/{{{2}}}"",""delete"")]
	public partial class {1}
	{{
	}}
}}";
		
		
		private string authenticationTemplate=@"using System;

namespace {0}.{1}.Types
{{
	public partial class Authentication
	{{
		public Authentication ()
		{{
		}}
		public string UserName{{get ;set;}}
		public string Password{{get ;set;}}
	}}
}}
";
	
		private string authorizationTemplate=@"using System;

namespace {0}.{1}.Types
{{
	public partial class Authorization
	{{
		public Authorization ()
		{{
		}}
		
		public int UserId{{ get; set;}}
	}}
}}";
		
		private string roleAndPermissionTemplate=@"using System;

namespace {0}.{1}.Types
{{
	public class RoleAndPermission
	{{
		public RoleAndPermission ()
		{{
		}}
		
		public int IdRole {{get; set;}}
		public string Role {{ get; set;}}
		public string Directory {{ get; set;}}
		public string ShowOrder {{ get; set;}}
		public string Permission {{ get; set;}}
	}}
}}
";
		
		private string authRoleTemplate=@"using System;
using System.ComponentModel.DataAnnotations;
using ServiceStack.Common;
using ServiceStack.DataAnnotations;
using ServiceStack.DesignPatterns.Model;

namespace {0}.{1}.Types
{{
	[Alias(""AUTH_ROLE"")]
	public partial class AuthRole:IHasId<System.Int32>{{

		public AuthRole(){{}}

		[Alias(""ID"")]
		[Sequence(""AUTH_ROLE_ID_GEN"")]
		[PrimaryKey]
		[AutoIncrement]
		public System.Int32 Id {{ get; set;}} 

		[Alias(""NAME"")]
		[Required]
		[StringLength(30)]
		public System.String Name {{ get; set;}} 

		[Alias(""DIRECTORY"")]
		[StringLength(15)]
		public System.String Directory {{ get; set;}} 
				
		[Alias(""SHOW_ORDER"")]
		[StringLength(2)]
		public System.String ShowOrder {{ get; set;}} 

	}}
}}";

		private string authenticationResponseTemplate=@"using System;
using System.Collections.Generic;
using ServiceStack.ServiceInterface.ServiceModel;
using {0}.{1}.Types;

namespace {0}.{1}.Operations
{{
	public class AuthenticationResponse:IHasResponseStatus
	{{
		public AuthenticationResponse ()
		{{
			ResponseStatus= new ResponseStatus();
			Permissions= new List<string>();
			Roles = new List<AuthRole>();
		}}
		
		public ResponseStatus ResponseStatus {{ get; set; }}
		
		public List<string> Permissions {{get; set;}}
		public List<AuthRole> Roles {{get; set;}}
		public string DisplayName {{ get; set;}}
		
	}}
}}";
		
		private string authorizationResponseTemplate=@"using System;
using System.Collections.Generic;
using ServiceStack.ServiceInterface.ServiceModel;
using {0}.{1}.Types;

namespace {0}.{1}.Operations
{{
	public class AuthorizationResponse:IHasResponseStatus
	{{
		public AuthorizationResponse ()
		{{
			ResponseStatus= new ResponseStatus();
			Permissions= new List<string>();
			Roles = new List<AuthRole>();
		}}
		
		public ResponseStatus ResponseStatus {{ get; set; }}
		
		public List<string> Permissions {{get; set;}}
		public List<AuthRole> Roles {{get; set;}}
		
	}}
}}";
		
		private string responseTemplate=@"using System;
using System.Collections.Generic;
using ServiceStack.ServiceInterface.ServiceModel;

namespace {0}.{1}.Operations
{{
	public class Response<T>:IHasResponseStatus where T:new()
	{{
		public Response ()
		{{
			ResponseStatus= new ResponseStatus();
			Data= new List<T>();
		}}
		
		public ResponseStatus ResponseStatus {{ get; set; }}
		
		public List<T> Data {{get; set;}}
		
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
[assembly: AssemblyCopyright(""angel colmenares"")]
[assembly: AssemblyTrademark(""aicl"")]
[assembly: AssemblyCulture("""")]

// The assembly version has the format ""{{Major}}.{{Minor}}.{{Build}}.{{Revision}}"".
// The form ""{{Major}}.{{Minor}}.*"" will automatically update the build and revision,
// and ""{{Major}}.{{Minor}}.{{Build}}.*"" will update just the revision.

[assembly: AssemblyVersion(""1.0.*"")]

// The following attributes are used to specify the signing key for the assembly, 
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("""")]
";
	
		private string authenticationAttributeTemplate=@"using System;
using ServiceStack.ServiceHost;

namespace {0}.{1}.Types
{{
	[RestService(""/login/{{UserName}}/{{Password}}"",""post,get"")]
	[RestService(""/login"",""post,get"")]
	[RestService(""/logout"",""delete"")]
	public partial class Authentication
	{{
	}}
}}";
		
		private string authRoleAttribute=@"using System;
using ServiceStack.ServiceHost;

namespace {0}.{1}.Types
{{
	[RestService(""/AuthRole/create"",""post"")]
	[RestService(""/AuthRole/read"",""get"")]
	[RestService(""/AuthRole/read/{{Id}}"",""get"")]
	[RestService(""/AuthRole/update/{{Id}}"",""put"")]
	[RestService(""/AuthRole/destroy/{{Id}}"",""delete"")]
	public partial class AuthRole
	{{
	}}
}}";
		
		private string projectTemplate=@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project DefaultTargets=""Build"" ToolsVersion=""3.5"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <ProductVersion>9.0.21022</ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>{{E0C40BDB-3D96-4FE3-850C-944FFFD63F8C}}</ProjectGuid>
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
    <Reference Include=""ServiceStack.Common"">
      <HintPath>..\..\lib\ServiceStack.Common.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack.Interfaces"">
      <HintPath>..\..\lib\ServiceStack.Interfaces.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack.OrmLite"">
      <HintPath>..\..\lib\ServiceStack.OrmLite.dll</HintPath>
    </Reference>
    <Reference Include=""System.ComponentModel.DataAnnotations"" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include=""Properties\AssemblyInfo.cs"" />
    <Compile Include=""Operations\Response.cs"" />
    <Compile Include=""Operations\AuthenticationResponse.cs"" />
    <Compile Include=""Operations\AuthorizationResponse.cs"" />
    <Compile Include=""Types\Authentication.cs"" />
    <Compile Include=""Types\Authorization.cs"" />
    <Compile Include=""Types\AuthRole.cs"" />
    <Compile Include=""Types\RoleAndPermission.cs"" />
    <Compile Include=""Attributes\Authentication.cs"" />
    <Compile Include=""Attributes\AuthRole.cs"" />
  </ItemGroup>
  <Import Project=""$(MSBuildBinPath)\Microsoft.CSharp.targets"" />
  <ItemGroup>
    <Folder Include=""Operations\"" />
    <Folder Include=""Properties\"" />
    <Folder Include=""Attributes\"" />
  </ItemGroup>
</Project>";
		
		
	}
	
}

