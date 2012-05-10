using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Aicl.DbToSS.CSharp
{
	public class AppHostCreator
	{
		public AppHostCreator ()
		{
		}
		
		public string ConnectionString
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
			set;
		}
		
		public virtual void Write()
		{
			
			if(string.IsNullOrEmpty(OutputDirectory))
				OutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "src");
			
			if(string.IsNullOrEmpty( ServiceNameSpace)) ServiceNameSpace="Interface";
			
			if(string.IsNullOrEmpty( ModelNameSpace)) ModelNameSpace="Model";
			
			if(string.IsNullOrEmpty( DataAccesNameSpace)) DataAccesNameSpace="DataAccess";
			
			if(string.IsNullOrEmpty( HostWebNameSpace)) HostWebNameSpace="Host.Web";
			
			if(string.IsNullOrEmpty( AppTitle)) AppTitle=SolutionName;
						
			if(!Directory.Exists(OutputDirectory))		
				Directory.CreateDirectory(OutputDirectory);
			
			string hostWebDir= Path.Combine( OutputDirectory,
			                                string.Format("{0}.{1}",SolutionName, HostWebNameSpace));
			
			if(!Directory.Exists(hostWebDir))		
				Directory.CreateDirectory(hostWebDir);
			
			
			string logDir= Path.Combine( hostWebDir,"log");
			
			if(!Directory.Exists(logDir))		
				Directory.CreateDirectory(logDir);
			
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(hostWebDir, "AppHost.cs")))
			{
				twp.Write(string.Format(appHostTemplate,SolutionName,ModelNameSpace,
				                        DataAccesNameSpace,ServiceNameSpace,
				                        HostWebNameSpace,AppTitle));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(hostWebDir, "Default.aspx")))
			{
				twp.Write(string.Format(defaultAspxTemplate,SolutionName,
				                        HostWebNameSpace,AppTitle));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(hostWebDir, "Default.aspx.cs")))
			{
				twp.Write(string.Format(defaultAspxCsTemplate,SolutionName,
				                        HostWebNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(hostWebDir, "Default.aspx.designer.cs")))
			{
				twp.Write(string.Format(defaultAspxDesignerCsTemplate,SolutionName,
				                        HostWebNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(hostWebDir, "Global.asax")))
			{
				twp.Write(string.Format(globalAsaxTemplate,SolutionName,
				                        HostWebNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(hostWebDir, "Global.asax.cs")))
			{
				twp.Write(string.Format(globalAsaxCsTemplate,SolutionName,
				                        HostWebNameSpace));				
				twp.Close();
			}
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(hostWebDir, "web.config")))
			{
				twp.Write(string.Format(webConfigTemplate,ConnectionString));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(hostWebDir, "log4net.conf")))
			{
				twp.Write(string.Format(log4NetTemplate,
				                        Path.Combine(logDir, SolutionName+".log"),
				                        Path.Combine(logDir, SolutionName+"-rolling.log")));				
				twp.Close();
			}
			
			if(! File.Exists(Path.Combine(logDir, SolutionName+".log")))
				File.Create(Path.Combine(logDir, SolutionName+".log"));
			
			if(! File.Exists(Path.Combine(logDir, SolutionName+"-rolling.log")))
				File.Create(Path.Combine(logDir, SolutionName+"-rolling.log"));
			
			using (TextWriter twp = new StreamWriter(Path.Combine(hostWebDir,
			                            string.Format("{0}.{1}.csproj",SolutionName,HostWebNameSpace))))
			{
				twp.Write(string.Format(projectTemplate,SolutionName,ServiceNameSpace,
				                        ModelNameSpace, DataAccesNameSpace,HostWebNameSpace));				
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
using ServiceStack.Logging;
using ServiceStack.Logging.Support.Logging;
using ServiceStack.Logging.Log4Net ;
using ServiceStack.OrmLite;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface;
using ServiceStack.OrmLite.Firebird;

using {0}.Model.Types;
using {0}.{2};
using {0}.{3};

namespace {0}.{4}
{{
	public class AppHost:AppHostBase
	{{
		private static ILog log;
		
		public AppHost (): base(""{5}"", typeof(AuthenticationService).Assembly)
		{{
			var appSettings = new ConfigurationResourceManager();
			if (appSettings.Get(""EnableLog4Net"", false))
			{{
				var cf=""log4net.conf"".MapHostAbsolutePath();
				log4net.Config.XmlConfigurator.Configure(
					new System.IO.FileInfo(cf));
				LogManager.LogFactory = new  Log4NetFactory();
			}}
			else
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
			
			//ConfigureServiceRoutes();
			
			log.InfoFormat(""AppHost Configured: "" + DateTime.Now);
		}}
		
		
		private void ConfigureServiceRoutes()
		{{
			/*
			Routes
				.Add<UserAuth>(""/UserAuth"",ApplyTo.Get)
				.Add<UserAuth>(""/UserAuth/Id/{{Id}}"",ApplyTo.Get)
				.Add<UserAuth>(""/UserAuth/UserName/{{UserName*}}"",ApplyTo.Get)
				.Add<UserAuth>(""/UserAuth"",ApplyTo.Post)
				.Add<UserAuth>(""/UserAuth/Id/{{Id}}"",ApplyTo.Put)
				.Add<UserAuth>(""/UserAuth/Id/{{Id}}"",ApplyTo.Delete);
		 	*/
		}}
		
		private void ConfigureAuth(Container container){{
			
			container.Register<ICacheClient>(new MemoryCacheClient());
			
			Plugins.Add(new AuthFeature(
				 () => new AuthUserSession(), // or Use your own typed Custom AuthUserSession type
				new IAuthProvider[]
        	{{
				new AuthenticationProvider()
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
						
		}}
		
	}}
}}";
		private string defaultAspxTemplate=@"<%@ Page Language=""C#"" Inherits=""{0}.{1}.Default"" %>
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
<html>
<head runat=""server"">
	<title>{2}</title>
</head>
<body>
	<form id=""form1"" runat=""server"">
	</form>
</body>
</html>";

		private string defaultAspxCsTemplate=@"using System;
using System.Configuration;
using System.Web;
using System.Web.UI;

namespace {0}.{1}
{{
	public partial class Default : System.Web.UI.Page
	{{
		protected void Page_Load(object sender, EventArgs e)
		{{
			string redirect= ConfigurationManager.AppSettings.Get(""RedirectTo"");
			if(!string.IsNullOrEmpty(redirect))
				Response.Redirect(redirect);
		}}	
	}}
}}
";
		private string defaultAspxDesignerCsTemplate=@"namespace {0}.{1} {{
	
	public partial class Default {{
		
		protected System.Web.UI.HtmlControls.HtmlForm form1;
	}}
}}
";
		private string globalAsaxTemplate=@"<%@ Application Inherits=""{0}.{1}.Global"" %>";
		
		private string globalAsaxCsTemplate=@"using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;

namespace {0}.{1}
{{
	public class Global : System.Web.HttpApplication
	{{
		
		protected virtual void Application_Start (Object sender, EventArgs e)
		{{
			AppHost appHost= new AppHost();
			appHost.Init();
		}}
		
		protected virtual void Session_Start (Object sender, EventArgs e)
		{{
		}}
		
		protected virtual void Application_BeginRequest (Object sender, EventArgs e)
		{{
		}}
		
		protected virtual void Application_EndRequest (Object sender, EventArgs e)
		{{
		}}
		
		protected virtual void Application_AuthenticateRequest (Object sender, EventArgs e)
		{{
		}}
		
		protected virtual void Application_Error (Object sender, EventArgs e)
		{{
		}}
		
		protected virtual void Session_End (Object sender, EventArgs e)
		{{
		}}
		
		protected virtual void Application_End (Object sender, EventArgs e)
		{{
		}}
	}}
}}
";
		
		private string webConfigTemplate=@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!--Standard IIS 7.0 Web.config as created by Visual Studio.NET -->
<!-- All changes from the default configuaration is prefixed by '[ServiceStack Specific]:' -->
<configuration>
  <configSections>
    <section name=""log4net"" type=""log4net.Config.Log4NetConfigurationSectionHandler,log4net"" />
  </configSections>
  <location path=""api"">
    <system.web>
      <httpHandlers>
        <add path=""*"" type=""ServiceStack.WebHost.Endpoints.ServiceStackHttpHandlerFactory, ServiceStack"" verb=""*"" />
      </httpHandlers>
    </system.web>
    <system.webServer>
      <handlers>
        <add path=""*"" name=""ServiceStack.Factory"" type=""ServiceStack.WebHost.Endpoints.ServiceStackHttpHandlerFactory, ServiceStack"" verb=""*"" preCondition=""integratedMode"" resourceType=""Unspecified"" allowPathInfo=""true"" />
      </handlers>
    </system.webServer>
  </location>
  <httpRuntime executionTimeout=""900"" maxRequestLength=""4096"" useFullyQualifiedRedirectUrl=""false"" minFreeThreads=""8"" minLocalRequestFreeThreads=""4"" appRequestQueueLimit=""100"" />
  <system.web>
    <!-- 
            Set compilation debug=""true"" to insert debugging 
            symbols into the compiled page. Because this 
            affects performance, set this value to true only  
            during development.  
        -->
    <compilation debug=""true"">
      <assemblies>
        <add assembly=""System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"" />
        <add assembly=""System.Data.DataSetExtensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"" />
        <add assembly=""System.Xml.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"" />
        <add assembly=""System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"" />
      </assemblies>
    </compilation>
    <!-- 
            The <authentication> section enables configuration 
            of the security authentication mode used by 
            ASP.NET to identify an incoming user. 
        -->
    <authentication mode=""Windows"" />
    <!--
            The <customErrors> section enables configuration 
            of what to do if/when an unhandled error occurs 
            during the execution of a request. Specifically,  
            it enables developers to configure html error pages 
            to be displayed in place of a error stack trace. 
 
        <customErrors mode=""RemoteOnly"" defaultRedirect=""GenericErrorPage.htm"">
            <error statusCode=""403"" redirect=""NoAccess.htm"" />
            <error statusCode=""404"" redirect=""FileNotFound.htm"" />
        </customErrors>
        -->
    <pages>
      <controls>
        <add tagPrefix=""asp"" namespace=""System.Web.UI"" assembly=""System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"" />
        <add tagPrefix=""asp"" namespace=""System.Web.UI.WebControls"" assembly=""System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"" />
      </controls>
    </pages>
    <httpHandlers>
      <remove verb=""*"" path=""*.asmx"" />
      <add verb=""*"" path=""*.asmx"" validate=""false"" type=""System.Web.Script.Services.ScriptHandlerFactory, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"" />
      <add verb=""*"" path=""*_AppService.axd"" validate=""false"" type=""System.Web.Script.Services.ScriptHandlerFactory, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"" />
      <add verb=""GET,HEAD"" path=""ScriptResource.axd"" type=""System.Web.Handlers.ScriptResourceHandler, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"" validate=""false"" />
      <!-- ServiceStack: Required for MONO -->
      <add path=""api*"" type=""ServiceStack.WebHost.Endpoints.ServiceStackHttpHandlerFactory, ServiceStack"" verb=""*"" />
    </httpHandlers>
  </system.web>
  <system.codedom>
    <compilers>
      <compiler language=""c#;cs;csharp"" extension="".cs"" warningLevel=""4"" type=""Microsoft.CSharp.CSharpCodeProvider, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"">
        <providerOption name=""CompilerVersion"" value=""v3.5"" />
        <providerOption name=""WarnAsError"" value=""false"" />
      </compiler>
    </compilers>
  </system.codedom>
  <!-- 
        The system.webServer section is required for running ASP.NET AJAX under Internet
        Information Services 7.0.  It is not necessary for previous version of IIS.
    -->
  <system.webServer>
    <validation validateIntegratedModeConfiguration=""false"" />
    <handlers>
      <!-- ServiceStack: Only required for IIS 7.0 -->
      <!--<add name=""ServiceStack.Factory"" path=""servicestack"" type=""ServiceStack.WebHost.Endpoints.ServiceStackHttpHandlerFactory, ServiceStack"" verb=""*"" preCondition=""integratedMode"" resourceType=""Unspecified"" allowPathInfo=""true""/>-->
    </handlers>
  </system.webServer>
  <runtime>
    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"" appliesTo=""v2.0.50727"">
      <dependentAssembly>
        <assemblyIdentity name=""System.Web.Extensions"" publicKeyToken=""31bf3856ad364e35"" />
        <bindingRedirect oldVersion=""1.0.0.0-1.1.0.0"" newVersion=""3.5.0.0"" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name=""System.Web.Extensions.Design"" publicKeyToken=""31bf3856ad364e35"" />
        <bindingRedirect oldVersion=""1.0.0.0-1.1.0.0"" newVersion=""3.5.0.0"" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
  <appSettings>
    <add key=""RedirectTo"" value=""WebApp"" />
	<add key=""EnableRegistrationFeature"" value=""false""/>	
    <add key=""EnableLog4Net"" value=""false"" />    
  </appSettings>
  <connectionStrings>
    <add name=""ApplicationDb"" connectionString=""{0}"" />
    <add name=""UserAuth"" connectionString=""{0}"" />    
  </connectionStrings>
</configuration>";
		
		private string projectTemplate=@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project DefaultTargets=""Build"" ToolsVersion=""3.5"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <ProductVersion>9.0.21022</ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>{{B0D07483-B16A-4AE8-92B9-B7EE01CEE11F}}</ProjectGuid>
    <ProjectTypeGuids>{{349C5851-65DF-11DA-9384-00065B846F21}};{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}</ProjectTypeGuids>
    <OutputType>Library</OutputType>
    <RootNamespace>{0}.{4}</RootNamespace>
    <AssemblyName>{0}.{4}</AssemblyName>
    <TargetFrameworkVersion>v3.5</TargetFrameworkVersion>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin</OutputPath>
    <DefineConstants>DEBUG;</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <ConsolePause>false</ConsolePause>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">
    <DebugType>none</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin</OutputPath>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <ConsolePause>false</ConsolePause>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include=""System"" />
    <Reference Include=""System.Web"" />
    <Reference Include=""System.Xml"" />
    <Reference Include=""System.Web.Services"" />
    <Reference Include=""System.Configuration"" />
    <Reference Include=""System.Core"" />
    <Reference Include=""System.Data"" />
    <Reference Include=""FirebirdSql.Data.FirebirdClient"">
      <HintPath>..\..\lib\FirebirdSql.Data.FirebirdClient.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack.Common"">
      <HintPath>..\..\lib\ServiceStack.Common.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack"">
      <HintPath>..\..\lib\ServiceStack.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack.{1}s"">
      <HintPath>..\..\lib\ServiceStack.{1}s.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack.OrmLite"">
      <HintPath>..\..\lib\ServiceStack.OrmLite.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack.OrmLite.Firebird"">
      <HintPath>..\..\lib\ServiceStack.OrmLite.Firebird.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack.ServiceInterface"">
      <HintPath>..\..\lib\ServiceStack.ServiceInterface.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack.Text"">
      <HintPath>..\..\lib\ServiceStack.Text.dll</HintPath>
    </Reference>
    <Reference Include=""log4net"">
      <HintPath>..\..\lib\log4net.dll</HintPath>
    </Reference>
    <Reference Include=""ServiceStack.Logging.Log4Net"">
      <HintPath>..\..\lib\ServiceStack.Logging.Log4Net.dll</HintPath>
    </Reference>
  </ItemGroup>
  <ItemGroup>
    <Content Include=""Global.asax"" />
    <Content Include=""web.config"" />
    <Content Include=""Default.aspx"" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include=""Global.asax.cs"">
      <DependentUpon>Global.asax</DependentUpon>
    </Compile>
    <Compile Include=""Default.aspx.cs"">
      <DependentUpon>Default.aspx</DependentUpon>
    </Compile>
    <Compile Include=""Default.aspx.designer.cs"">
      <DependentUpon>Default.aspx</DependentUpon>
    </Compile>
    <Compile Include=""AppHost.cs"" />
  </ItemGroup>
  <Import Project=""$(MSBuildBinPath)\Microsoft.CSharp.targets"" />
  <Import Project=""$(MSBuildExtensionsPath)\Microsoft\VisualStudio\v9.0\WebApplications\Microsoft.WebApplication.targets"" />
  <ProjectExtensions>
    <MonoDevelop>
      <Properties VerifyCodeBehindFields=""true"" VerifyCodeBehindEvents=""true"">
        <XspParameters Port=""8080"" Address=""0.0.0.0"" SslMode=""None"" SslProtocol=""Default"" KeyType=""None"" CertFile="""" KeyFile="""" PasswordOptions=""None"" Password="""" Verbose=""true"" />
      </Properties>
    </MonoDevelop>
  </ProjectExtensions>
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
</Project>";
		
		private string log4NetTemplate=@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<log4net debug=""false"">
	<appender name=""LogFileAppender"" type=""log4net.Appender.FileAppender"" >
		<file value=""{0}"" />
		<appendToFile value=""true"" />
		<layout type=""log4net.Layout.PatternLayout"">
			<conversionPattern value=""%date [%thread] %-5level %logger [%ndc] - %message%newline"" />
		</layout>
	</appender>
	<appender name=""HttpTraceAppender"" type=""log4net.Appender.AspNetTraceAppender"" >
		<layout type=""log4net.Layout.PatternLayout"">
			<conversionPattern value=""%date [%thread] %-5level %logger [%ndc] - %message%newline"" />
		</layout>
	</appender>
	<appender name=""RollingLogFileAppender"" type=""log4net.Appender.RollingFileAppender"">
		<file value=""{1}"" />
		<appendToFile value=""true"" />
		<maxSizeRollBackups value=""10"" />
		<maximumFileSize value=""5MB"" />
		<rollingStyle value=""Size"" />
		<staticLogFileName value=""true"" />
		<layout type=""log4net.Layout.PatternLayout"">
			<conversionPattern value=""%date [%thread] %-5level %logger [%ndc] - %message%newline"" />
		</layout>
	</appender>
	<root>
		<level value=""DEBUG"" />
		<appender-ref ref=""LogFileAppender"" />
		<appender-ref ref=""HttpTraceAppender"" />
		<!-- <appender-ref ref=""RollingLogFileAppender"" /> -->
	</root>
</log4net>";
		
	}
}

