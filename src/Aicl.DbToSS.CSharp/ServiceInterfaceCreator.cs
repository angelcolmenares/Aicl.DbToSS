using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Aicl.DbToSS.CSharp
{
	public class ServiceInterfaceCreator{
		
		
		public ServiceInterfaceCreator()
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
		
		public string ClassName
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
					
			if(!Directory.Exists(OutputDirectory))		
				Directory.CreateDirectory(OutputDirectory);
			
			string interfaceDir = Path.Combine(OutputDirectory,
			                                   string.Format("{0}.{1}",SolutionName,ServiceNameSpace));
			
			if(!Directory.Exists(interfaceDir))		
				Directory.CreateDirectory(interfaceDir);
			
			
			string servDir=Path.Combine(interfaceDir,"Services");
			if(!Directory.Exists(servDir))		
				Directory.CreateDirectory(servDir);
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(servDir, ClassName + "Service.cs")))
			{
				twp.Write(string.Format(serviceTemplate,SolutionName,ModelNameSpace,
				                        ServiceNameSpace, ClassName));				
				twp.Close();
			}
			
			
			string propertiesDir = Path.Combine(interfaceDir,"Properties");
						
			if(!Directory.Exists(propertiesDir))		
				Directory.CreateDirectory(propertiesDir);
			
			using (TextWriter twp = new StreamWriter(Path.Combine(propertiesDir, "AssemblyInfo.cs")))
			{
				twp.Write(string.Format(assemblyTemplate,SolutionName,ServiceNameSpace));				
				twp.Close();
			}
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(interfaceDir, "AppConfig.cs")))
			{
				twp.Write(string.Format(appConfigTemplate,SolutionName,ServiceNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(interfaceDir, "AppRestService.cs")))
			{
				twp.Write(string.Format(appRestServiceTemplate,SolutionName,ModelNameSpace,
				                        DataAccesNameSpace, ServiceNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(interfaceDir, "AuthenticationService.cs")))
			{
				twp.Write(string.Format(authServiceTemplate,SolutionName,ModelNameSpace,
				                        DataAccesNameSpace, ServiceNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(interfaceDir, "AuthorizationService.cs")))
			{
				twp.Write(string.Format(authorizationServiceTemplate,SolutionName,ModelNameSpace,
				                        DataAccesNameSpace, ServiceNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(interfaceDir, "HttpResponse.cs")))
			{
				twp.Write(string.Format(httpResponseTemplate,SolutionName,ServiceNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(interfaceDir, "PermissionAttribute.cs")))
			{
				twp.Write(string.Format(permissionTemplate,SolutionName,ServiceNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(interfaceDir, "RoleAttribute.cs")))
			{
				twp.Write(string.Format(roleTemplate,SolutionName,ServiceNameSpace));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(interfaceDir,
			                            string.Format("{0}.{1}.csproj",SolutionName,ServiceNameSpace))))
			{
				twp.Write(string.Format(projectTemplate,SolutionName,ServiceNameSpace,
				                        ModelNameSpace,DataAccesNameSpace));				
				twp.Close();
			}
			
		}

				
		private string serviceTemplate =@"using System;
﻿using ServiceStack.CacheAccess;
using ServiceStack.Common;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

using {0}.{1}.Types;
using {0}.{1}.Operations;

namespace {0}.{2}
{{
	[Authenticate]
	[RequiredPermission(""{3}.read"")]
	[RequiredPermission(ApplyTo.Post, ""{3}.create"")]	
	[RequiredPermission(ApplyTo.Put , ""{3}.update"")]	
	[RequiredPermission(ApplyTo.Delete, ""{3}.destroy"")]
	public class {3}Service:AppRestService<{3}>
	{{
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

		private string appConfigTemplate=@"using System;
using System.IO;
using ServiceStack.Common.Utils;
using ServiceStack.Configuration;

namespace {0}.{1}
{{
	public class AppConfig
	{{
		
		public AppConfig(IResourceManager resources)
		{{			
		}}
				
	}}
}}";
		private string appRestServiceTemplate=@"using System;
﻿using ServiceStack.CacheAccess;
using ServiceStack.Common;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.DesignPatterns.Model;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface.Auth;

using {0}.{1}.Types;
using {0}.{1}.Operations;
using {0}.{2};

namespace {0}.{3}
{{
	public class AppRestService<T>:RestServiceBase<T> where T:new()
	{{
		//public Session Session{{get; protected set; }}
		
		//public ICacheClient CacheClient {{ get; set; }}
				
		public Factory Factory{{ get; set;}}
		
		public override object OnGet (T request)
		{{		
			try{{
				return  new Response<T>(){{
					Data=Factory.Get<T>(request)
				}};
			}}
			catch(Exception e ){{
				return HttpResponse.ErrorResult<Response<T>>(e, ""GetError"");
			}}
		}}
		
		public override object OnPost (T request)
		{{
			try{{		
				return new Response<T>(){{
					Data=Factory.Post<T>(request)
				}};			
			}}
			catch(Exception e ){{
				return HttpResponse.ErrorResult<Response<T>>(e, ""PostError"");
			}}
		}}
		
		public override object OnPut (T request)
		{{
			
			try{{
				return new Response<T>(){{
					Data=Factory.Put<T>(request)
				}};
			}}
			catch(Exception e ){{
				return HttpResponse.ErrorResult<Response<T>>(e, ""PutError"");
			}}
		}}
		
		public override object OnDelete (T request)
		{{
		
			try{{
				return  new Response<T>(){{
					Data=Factory.Delete<T>(request)
				}};
			}}
			catch(Exception e ){{
				return HttpResponse.ErrorResult<Response<T>>(e, ""DeleteError"");
			}}
		}}
		
		
		public object GetById(object id) 
		{{
			try{{
				return new Response<T>(){{
					Data=Factory.GetById<T>(id) 	
				}};
			}}
			catch(Exception e ){{
				return HttpResponse.ErrorResult<Response<T>>(e, ""GetByIdError"");
			}}
		}}
				
	}}
}}

";
		private string  authServiceTemplate=@"using System;
using System.Collections.Generic;
using System.Linq;
﻿using ServiceStack.CacheAccess;
using ServiceStack.Common;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

using {0}.{1}.Types;
using {0}.{1}.Operations;
using {0}.{2};

namespace {0}.{3}
{{
	public class AuthenticationService:RestServiceBase<Authentication>
	{{
		public Factory Factory{{ get; set;}}
		
		public override object OnPost (Authentication request)
		{{
			
			AuthService authService = base.ResolveService<AuthService>();
			
			object fr= authService.Post(new Auth {{
				provider = AuthService.CredentialsProvider,
				UserName = request.UserName,
				Password = request.Password
			}}) ;
						
			
			IAuthSession session = this.GetSession();
			
			if(!session.IsAuthenticated)
			{{
				HttpError e = fr as HttpError;
				if(e!=null) throw e;
				
				Exception ex = fr as Exception;
				throw ex;
			}};
			
			Authorization auth = new Authorization(){{
				UserId= int.Parse(session.UserAuthId)
			}};
			
			AuthorizationResponse aur = auth.GetRolesAndPermissions(Factory.DbFactory);
			
			session.Permissions= aur.Permissions;
			session.Roles= (from r in aur.Roles select r.Name).ToList();
			
			authService.SaveSession(session, TimeSpan.FromDays(1));
			
			return new AuthenticationResponse(){{
				DisplayName= session.DisplayName.IsNullOrEmpty()? session.UserName: session.DisplayName,
				Roles= aur.Roles,
				Permissions= aur.Permissions
			}};
			
		}}
		
		public override object OnGet (Authentication request)
		{{
			return OnPost(request);
		}}
		
		public override object OnDelete (Authentication request)
		{{
			AuthService authService = base.ResolveService<AuthService>();
			var response =authService.Delete(new Auth {{
					provider = AuthService.LogoutAction
			}});
				
			return response;
		}}
		
	}}
}}";
		
		private string authorizationServiceTemplate=@"using System;
using System.Collections.Generic;
using System.Linq;
﻿using ServiceStack.CacheAccess;
using ServiceStack.Common;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

using {0}.{1}.Types;
using {0}.{1}.Operations;
using {0}.{2};

namespace {0}.{3}
{{
	[Authenticate]
	public class AuthorizationService:RestServiceBase<Authorization>
	{{
		public Factory Factory{{ get; set;}}
		
		public override object OnPost (Authorization request)
		{{
			IAuthSession session = this.GetSession();
						
			if (!session.HasRole(RoleNames.Admin))
			{{
				request.UserId= int.Parse(session.UserAuthId);
			}}
			return  request.GetRolesAndPermissions(Factory.DbFactory);
			 
		}}
		
	}}
}}";
		
		private string httpResponseTemplate=@"using System;
using System.Net;
﻿using ServiceStack.CacheAccess;
using ServiceStack.Common;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.DesignPatterns.Model;
using ServiceStack.OrmLite;

namespace {0}.{1}
{{
	public static class HttpResponse
	{{
		
		public static HttpResult ErrorResult<TResponse>(string message, string  stackTrace, string errorCode)
			where TResponse:IHasResponseStatus, new() 			
		{{
			return new HttpResult( new TResponse(){{
					ResponseStatus= new ResponseStatus(){{
						Message=message,
						StackTrace=stackTrace,
						ErrorCode=errorCode
					}}
				}},
			HttpStatusCode.InternalServerError);
		}}
		
		public static HttpResult ErrorResult<TResponse>(string message,string errorCode)
			where TResponse:IHasResponseStatus, new() 
		{{
			return ErrorResult<TResponse>(message, string.Empty, errorCode);
		}}
		
		
		public static HttpResult ErrorResult<TResponse>(Exception exception,string errorCode)
			where TResponse:IHasResponseStatus, new() 
		{{
			return ErrorResult<TResponse>(exception.Message, exception.StackTrace, errorCode);
		}}		
		
	}}
		
}}";
		
		private string permissionTemplate=@"using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
﻿using ServiceStack.CacheAccess;
using ServiceStack.Common;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

namespace {0}.{1}
{{
	public class PermissionAttribute:RequiredPermissionAttribute
	{{
		public PermissionAttribute(params string[] permissions):base(ApplyTo.All, permissions)
		{{
		}}
		public PermissionAttribute(ApplyTo applyTo, params string[] permissions):base(applyTo, permissions)
		{{
		}}
		
		public override void Execute (IHttpRequest req, IHttpResponse res, object requestDto)
		{{
			AuthenticateAttribute.AuthenticateIfBasicAuth(req, res);

			var session = req.GetSession();
			if (HasAllPermissions(session)) return;

			res.StatusCode = (int)HttpStatusCode.Unauthorized;
			res.StatusDescription = ""Invalid Permissions"";
			res.Close();
		}}
				
	}}
}}";
		
		private string roleTemplate=@"using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
﻿using ServiceStack.CacheAccess;
using ServiceStack.Common;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

namespace {0}.{1}
{{
	public class RoleAttribute:RequiredRoleAttribute
	{{
		public RoleAttribute(ApplyTo applyTo, params string[] roles)
			:base(applyTo, roles) {{}}

		public RoleAttribute(params string[] roles)
			: base(ApplyTo.All, roles) {{}}
		
		
		public override void Execute(IHttpRequest req, IHttpResponse res, object requestDto)
		{{
			AuthenticateAttribute.AuthenticateIfBasicAuth(req, res);

			var session = req.GetSession();
			if (HasAllRoles(session)) return;

			res.StatusCode = (int)HttpStatusCode.Unauthorized;
			res.StatusDescription = ""Invalid Role"";
			res.Close();
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
    <ProjectGuid>{{A04440F6-FF47-4E5B-9B61-C2EC27DC52F9}}</ProjectGuid>
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
    <Reference Include=""ServiceStack.{1}s"">
      <HintPath>..\..\lib\ServiceStack.{1}s.dll</HintPath>
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
    <Compile Include=""AppConfig.cs"" />
    <Compile Include=""HttpResponse.cs"" />
    <Compile Include=""AppRestService.cs"" />
    <Compile Include=""PermissionAttribute.cs"" />
    <Compile Include=""RoleAttribute.cs"" />
    <Compile Include=""AuthenticationService.cs"" />
    <Compile Include=""AuthorizationService.cs"" />
  </ItemGroup>
  <Import Project=""$(MSBuildBinPath)\Microsoft.CSharp.targets"" />
  <ItemGroup>
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
    <Folder Include=""Properties\"" />
    <Folder Include=""Services\"" />
  </ItemGroup>
</Project>";
	}
	
}

//using Aicl.GestionNegocios.Model.Types;

//namespace Aicl.GestionNegocios.Interface


