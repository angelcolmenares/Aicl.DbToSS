using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Aicl.DbToSS.CSharp
{
	public class SetupCreator
	{
		public SetupCreator ()
		{
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
		
		
		public string ConnectionString
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
			set;
		}
		
		public virtual void Write()
		{
			
			if(string.IsNullOrEmpty(OutputDirectory))
				OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "src");
			
			if(string.IsNullOrEmpty( ServiceNameSpace)) ServiceNameSpace="Interface";
			
			if(string.IsNullOrEmpty( ModelNameSpace)) ModelNameSpace="Model";
			
			if(string.IsNullOrEmpty( DataAccesNameSpace)) DataAccesNameSpace="DataAccess";
						
			if(string.IsNullOrEmpty( AppTitle)) AppTitle=SolutionName;
						
			if(!Directory.Exists(OutputDirectory))		
				Directory.CreateDirectory(OutputDirectory);
			
			string setupDir= Path.Combine( OutputDirectory,
			                                string.Format("{0}.{1}",SolutionName, "Setup"));
			
			if(!Directory.Exists(setupDir))		
				Directory.CreateDirectory(setupDir);
			
			using (TextWriter twp = new StreamWriter(Path.Combine(setupDir, "Main.cs")))
			{
				twp.Write(string.Format(mainTemplate,SolutionName));				
				twp.Close();
			}
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(setupDir, "AppHost.cs")))
			{
				twp.Write(string.Format(appHostTemplate,SolutionName,ModelNameSpace,
				                        DataAccesNameSpace,ServiceNameSpace,
				                        AppTitle));				
				twp.Close();
			}
					
			using (TextWriter twp = new StreamWriter(Path.Combine(setupDir, "app.config")))
			{
				twp.Write(string.Format(appConfigTemplate,ConnectionString));				
				twp.Close();
			}
			
			string propertiesDir = Path.Combine(setupDir,"Properties");
						
			if(!Directory.Exists(propertiesDir))		
				Directory.CreateDirectory(propertiesDir);
			
			using (TextWriter twp = new StreamWriter(Path.Combine(propertiesDir, "AssemblyInfo.cs")))
			{
				twp.Write(string.Format(assemblyTemplate,SolutionName));				
				twp.Close();
			}
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(setupDir,
			                            string.Format("{0}.Setup.csproj",SolutionName))))
			{
				twp.Write(string.Format(projectTemplate,SolutionName,ServiceNameSpace,
				                        ModelNameSpace, DataAccesNameSpace));				
				twp.Close();
			}
			
			
		}
		
		private string appHostTemplate=@"using System;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using Funq;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.Common.Web;
using ServiceStack.Common.Utils;
using ServiceStack.Configuration;
using ServiceStack.DataAccess;
using ServiceStack.Logging;
using ServiceStack.Logging.Support.Logging;
using ServiceStack.OrmLite;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface;
using ServiceStack.OrmLite.Firebird;

using {0}.{1}.Types;
using {0}.{2};
using {0}.{3};

namespace {0}.Setup
{{
	public class AppHost:AppHostHttpListenerBase
	{{
		private static ILog log;
		
		public AppHost (): base(""{4}"", typeof(AuthenticationService).Assembly)
		{{
			LogManager.LogFactory = new ConsoleLogFactory();
			log = LogManager.GetLogger(typeof (AppHost));
		}}
		
		public override void Configure(Container container)
		{{
			//Permit modern browsers (e.g. Firefox) to allow sending of any REST HTTP Method
			base.SetConfig(new EndpointHostConfig
			{{
				GlobalResponseHeaders =
					{{
						{{ ""Access-Control-Allow-Origin"", ""*"" }},
						{{ ""Access-Control-Allow-Methods"", ""GET, POST, PUT, DELETE, OPTIONS"" }},
					}},
				  DefaultContentType = ContentType.Json 
			}});
						
			var config = new AppConfig(new ConfigurationResourceManager());
			container.Register(config);
			
			
			OrmLiteConfig.DialectProvider= FirebirdOrmLiteDialectProvider.Instance;
			
			IDbConnectionFactory dbFactory = new OrmLiteConnectionFactory(
				ConfigUtils.GetConnectionString(""ApplicationDb""));
			
			container.Register<Factory>(
				new Factory(){{
					DbFactory=dbFactory
				}}
			);
									
			ConfigureAuth(container);
						
			log.InfoFormat(""AppHost Configured: "" + DateTime.Now);
		}}
		
		
		
		private void ConfigureAuth(Container container){{
			
			container.Register<ICacheClient>(new MemoryCacheClient());
			
			Plugins.Add(new AuthFeature(
				 () => new AuthUserSession(), // or Use your own typed Custom AuthUserSession type
				new IAuthProvider[]
        	{{
				new CredentialsAuthProvider()
        	}})
			{{
				IncludeAssignRoleServices=false
			}});
		    				
			var dbFactory = new OrmLiteConnectionFactory(ConfigUtils.GetConnectionString(""UserAuth"")) ;
			
			OrmLiteAuthRepository authRepo = new OrmLiteAuthRepository(
				dbFactory
			);
			
			container.Register<IUserAuthRepository>(
				c => authRepo
			); //Use OrmLite DB Connection to persist the UserAuth and AuthProvider info

			var appSettings = new ConfigurationResourceManager();
			if (appSettings.Get(""EnableRegistrationFeature"", false))
				Plugins.Add( new  RegistrationFeature());
			
			var oldL =FirebirdOrmLiteDialectProvider.Instance.DefaultStringLength;
			
			FirebirdOrmLiteDialectProvider.Instance.DefaultStringLength=1024;
			if (appSettings.Get(""RecreateAuthTables"", false))
				authRepo.DropAndReCreateTables(); //Drop and re-create all Auth and registration tables
			else{{
				authRepo.CreateMissingTables();   //Create only the missing tables				
			}}
			
			FirebirdOrmLiteDialectProvider.Instance.DefaultStringLength=oldL;
						
		    //Add admin user  
			string userName = ""admin"";
			string password = ""admin"";
		
			List<string> permissions= new List<string>(
			new string[]{{	
		
			}});
			
			if ( authRepo.GetUserAuthByUserName(userName)== default(UserAuth) ){{
				List<string> roles= new List<string>();
				roles.Add(RoleNames.Admin);
			    string hash;
			    string salt;
			    new SaltedHash().GetHashAndSaltString(password, out hash, out salt);
			    authRepo.CreateUserAuth(new UserAuth {{
				    DisplayName = userName,
			        Email = userName+""@mail.com"",
			        UserName = userName,
			        FirstName = """",
			        LastName = """",
			        PasswordHash = hash,
			        Salt = salt,
					Roles =roles,
					Permissions=permissions
			    }}, password);
			}}
			
			userName = ""test1"";
			password = ""test1"";
		
			permissions= new List<string>(
			new string[]{{	
			
			}});
			
			if ( authRepo.GetUserAuthByUserName(userName)== default(UserAuth) ){{
				List<string> roles= new List<string>();
				roles.Add(""Test"");
				string hash;
			    string salt;
			    new SaltedHash().GetHashAndSaltString(password, out hash, out salt);
			    authRepo.CreateUserAuth(new UserAuth {{
				    DisplayName = userName,
			        Email = userName+""@mail.com"",
			        UserName = userName,
			        FirstName = """",
			        LastName = """",
			        PasswordHash = hash,
			        Salt = salt,
					Roles =roles,
					Permissions=permissions
			    }}, password);
			}}	
		}}
		
	}}
}}";
		
		private string appConfigTemplate=@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
	<appSettings>
		<add key=""ListeningOn"" value=""http://localhost:8082/""/>
		<add key=""EnableRegistrationFeature"" value=""false""/>
		<add key=""RecreateAuthTables"" value=""false""/>
	</appSettings>	
	<connectionStrings>
    	<add name=""ApplicationDb"" connectionString=""{0}"" />
    	<add name=""UserAuth"" connectionString=""{0}"" />    
  	</connectionStrings>
</configuration>
";
		
		private string mainTemplate=@"using System;
using ServiceStack.Configuration;

namespace {0}.Setup
{{
	class MainClass
	{{
		private static readonly string ListeningOn = ConfigUtils.GetAppSetting(""ListeningOn"");
		
		public static void Main (string[] args)
		{{
			
			var appHost = new AppHost();
			appHost.Init();
			appHost.Start(ListeningOn);

			Console.WriteLine(""Started listening on: "" + ListeningOn);

			Console.WriteLine(""AppHost Created at {{0}}, listening on {{1}}"",
				DateTime.Now, ListeningOn);

			Console.WriteLine(""ReadKey()"");
			Console.ReadKey();
			
		}}
	}}
}}";
		
		private string assemblyTemplate=@"using System.Reflection;
using System.Runtime.CompilerServices;

// Information about this assembly is defined by the following attributes. 
// Change them to the values specific to your project.

[assembly: AssemblyTitle(""{0}.Setup"")]
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
//[assembly: AssemblyKeyFile("""")]";
		
		private string projectTemplate=@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project DefaultTargets=""Build"" ToolsVersion=""3.5"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">x86</Platform>
    <ProductVersion>9.0.21022</ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>{{DDCCAE05-5A85-4E49-9A91-1F5DDAAB2B64}}</ProjectGuid>
    <OutputType>Exe</OutputType>
    <RootNamespace>{0}.Setup</RootNamespace>
    <AssemblyName>{0}.Setup</AssemblyName>
    <TargetFrameworkVersion>v3.5</TargetFrameworkVersion>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|x86' "">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug</OutputPath>
    <DefineConstants>DEBUG;</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <PlatformTarget>x86</PlatformTarget>
    <Externalconsole>true</Externalconsole>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|x86' "">
    <DebugType>none</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Release</OutputPath>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <PlatformTarget>x86</PlatformTarget>
    <Externalconsole>true</Externalconsole>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include=""System"" />
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
    <Reference Include=""ServiceStack.OrmLite.Firebird"">
      <HintPath>..\..\lib\ServiceStack.OrmLite.Firebird.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack.Text"">
      <HintPath>..\..\lib\ServiceStack.Text.dll</HintPath>
    </Reference>
    <Reference Include=""System.Core"" />
    <Reference Include=""System.Configuration"" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include=""Main.cs"" />
    <Compile Include=""AppHost.cs"" />
    <Compile Include=""Properties\AssemblyInfo.cs"" />
  </ItemGroup>
  <Import Project=""$(MSBuildBinPath)\Microsoft.CSharp.targets"" />
  <ItemGroup>
    <ProjectReference Include=""..\{0}.{1}\{0}.{1}.csproj"">
      <Project>{{A04440F6-FF47-4E5B-9B61-C2EC27DC52F9}}</Project>
      <Name>{0}.{1}</Name>
    </ProjectReference>
    <ProjectReference Include=""..\{0}.{2}\{0}.{2}.csproj"">
      <Project>{{E0C40BDB-3D96-4FE3-850C-944FFFD63F8C}}</Project>
      <Name>{0}.{2}</Name>
    </ProjectReference>
    <ProjectReference Include=""..\{0}.{3}\{0}.{3}.csproj"">
      <Project>{{C0319E77-DA72-47AD-A64F-5E25C634DC4B}}</Project>
      <Name>{0}.{3}</Name>
    </ProjectReference>
  </ItemGroup>
  <ItemGroup>
    <None Include=""app.config"">
      <Gettext-ScanForTranslations>false</Gettext-ScanForTranslations>
    </None>
  </ItemGroup>
  <ItemGroup>
    <Folder Include=""Properties\"" />
  </ItemGroup>
</Project>";
				
	}	
}

