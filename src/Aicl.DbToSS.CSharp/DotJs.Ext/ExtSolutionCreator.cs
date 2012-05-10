using System;
using System.Reflection;
using System.IO;
using ServiceStack.Text;
using Aicl.DbToSS.CSharp;
namespace Aicl.DotJs.Ext
{
	public class ExtSolutionCreator
	{
		
		public ExtSolutionCreator ()
		{
		}
		
		public string AppName { get; set;}
		public string AppTitle { get; set;}
		
		public string Theme {get; set;}
		
		public string ExtDir { get; set;}
		
		public string SolutionName
		{
			get;
			set;
		}
		
		public string AssemblyName
		{ 
			get;
			set;
		}
		
		public string NameSpace
		{ 
			get;
			set;
		}
		
		
		private string OutputDirectory
		{
			get;
			set;
		}
		
		public virtual void Write()
		{
			if(string.IsNullOrEmpty(AppName)) AppName="App";
			
			if(string.IsNullOrEmpty(Theme)) Theme="ext-all.css";
			
			string solutionDir=Path.Combine(Directory.GetCurrentDirectory(), SolutionName);
			
			if(!Directory.Exists(solutionDir))		
				Directory.CreateDirectory(solutionDir);
								
			OutputDirectory = Path.Combine(solutionDir, "src");
			
			if(!Directory.Exists(OutputDirectory))		
				Directory.CreateDirectory(OutputDirectory);
			
			string webAppDir=Path.Combine(OutputDirectory,string.Format("{0}.{1}",SolutionName, "WebApp"));
			
			if(!Directory.Exists(webAppDir))		
				Directory.CreateDirectory(webAppDir);
			
			
			string modulesDir=Path.Combine(webAppDir, "modules");
			
			if(!Directory.Exists(modulesDir))		
				Directory.CreateDirectory(modulesDir);
			
			
			string resourcesDir=Path.Combine(webAppDir, "resources");
			
			if(!Directory.Exists(resourcesDir))		
				Directory.CreateDirectory(resourcesDir);
			
			if(!string.IsNullOrEmpty(ExtDir))
			{
				string extDir= Path.Combine(webAppDir, "extjs");
				Util.Execute("ln", string.Format(" -s {0} {1}",ExtDir,extDir));
				Util.Execute("ln", string.Format("-s extjs/examples/ux/ {0}",
				                                 Path.Combine(webAppDir, "ux")));
			}
			
			
			var assembly = Assembly.LoadFrom(AssemblyName);
			Console.WriteLine("Starting js  generation for assembly:'{0}' ...", assembly);
			foreach(Type t in  assembly.GetTypes()){
				if (t.Namespace==NameSpace)
				{
					Console.Write("Generating js for class:'{0}'...", t.FullName);
					
					Model model = new Model(t){OutputDirectory=modulesDir,AppName=AppName};
			
					model.Write<ExtModel,ExtModelField>();
			
					Store store = new Store(t){OutputDirectory=modulesDir,AppName=AppName};;
					store.Write();
			
					List list = new List(t){OutputDirectory=modulesDir,AppName=AppName};;
					list.Write();
			
					Form form = new Form(t){OutputDirectory=modulesDir,AppName=AppName};;
					form.Write();
			
					Controller controller = new Controller(t){OutputDirectory=modulesDir,AppName=AppName};;
					controller.Write();
			
					Application app = new Application(t){OutputDirectory=modulesDir,AppName=AppName,Theme=Theme};;
					app.Write();
					
					Console.WriteLine(" Done.");
								
				}
			}
			
			Console.WriteLine("js for assembly:'{0}' Done", assembly);
			
			using (TextWriter twp = new StreamWriter(Path.Combine(webAppDir,"app.js")))
			{
				twp.Write(string.Format(appTemplate));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(webAppDir,"index.html")))
			{
				twp.Write(string.Format( indexTemplate, AppTitle, Theme));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(webAppDir,"intro.html")))
			{
				twp.Write(string.Format(introTemplate));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(webAppDir,"license.txt")))
			{
				twp.Write(string.Format(licenseTemplate, AppTitle));				
				twp.Close();
			}
			
			
			using (TextWriter twp = new StreamWriter(Path.Combine(resourcesDir,"util.js")))
			{
				twp.Write(string.Format(utilJsTemplate));				
				twp.Close();
			}
			
			using (TextWriter twp = new StreamWriter(Path.Combine(resourcesDir,"util.css")))
			{
				twp.Write(string.Format(utilCssTemplate));				
				twp.Close();
			}
			
			string loginAppDir= Path.Combine(modulesDir,"login");
			if(! Directory.Exists(loginAppDir))
				Directory.CreateDirectory(loginAppDir);
			
			using (TextWriter twp = new StreamWriter(Path.Combine(loginAppDir,"app.js")))
			{
				twp.Write(string.Format(loginAppTemplate, AppName));				
				twp.Close();
			}
							
			string loginViewDir= Path.Combine( Path.Combine(modulesDir,"app"), "view");
			if(! Directory.Exists(loginViewDir))
				Directory.CreateDirectory(loginViewDir);
			
			using (TextWriter twp = new StreamWriter(Path.Combine(loginViewDir,"Login.js")))
			{
				twp.Write(string.Format(loginViewTemplate, AppName));				
				twp.Close();
			}
	
			string loginControllerDir= Path.Combine( Path.Combine(modulesDir,"app"), "controller");
			if(! Directory.Exists(loginControllerDir))
				Directory.CreateDirectory(loginControllerDir);
			
			using (TextWriter twp = new StreamWriter(Path.Combine(loginControllerDir,"Login.js")))
			{
				twp.Write(string.Format(loginControllerTemplate, AppName));				
				twp.Close();
			}
			
		}
		
		private string appTemplate=@"Ext.require(['*']);
Ext.onReady(function() {{

	Aicl.Util.setUrlApi(location.protocol + ""//"" + location.host + '/api');
	Aicl.Util.setHttpUrlApi(location.protocol + ""//"" + location.host + '/api'+'/json/asynconeway')
	Aicl.Util.setPhotoDir(location.protocol + ""//"" + location.host +  '' + location.pathname+ 'photos');
	Aicl.Util.setEmptyImgUrl('../../resources/icons/fam/user.png');
	
	var loginPath = '/auth/credentials';
	var logoutPath = '/auth/logout';

	var buttons = [];
	var i = 0;
	var grupos = [{{
				title : 'Companies',
				dir : 'company'
			}}, {{
				title : 'Countries',
				dir : 'country'
			}}, {{
				title : 'Cities',
				dir : 'city'
			}}, {{
				title : 'Persons',
				dir : 'person'
			}}, {{
				title : 'Authors',
				dir : 'author'
			}}, {{
				title : 'UserAuth',
				dir : 'userAuth'
			}}];
	for (var grupo in grupos) {{
		buttons[i] = Ext.create('Ext.Button', {{
					text : grupos[grupo].title,
					directory : grupos[grupo].dir,
					scale : 'large',
					handler : function() {{
						Ext.getDom('iframe-win').src = 'modules/'
								+ this.directory;
					}}
				}});
		i++;
	}};

	var loginButton = Ext.create('Ext.Button', {{
				text : 'Login',
				scale : 'large',
				disabled : Aicl.Util.isAuth(),
				handler : function() {{
					login();

				}}
			}});

	buttons[i++] = loginButton;

	var logoutButton = Ext.create('Ext.Button', {{
				text : 'Logout',
				scale : 'large',
				disabled : !Aicl.Util.isAuth(),
				handler : function() {{
					logout();
				}}
			}});

	buttons[i] = logoutButton;

	Ext.create('Ext.Viewport', {{
				layout : {{
					type : 'border',
					padding : 5
				}},
				defaults : {{
					split : true
				}},
				items : [{{
							region : 'west',
							layout : 'fit',
							items : [{{
										layout : {{
											type : 'vbox',
											align : 'stretch'
										}},
										defaults : {{
											margins : '5 5 5 5'
										}},
										items : buttons
									}}],
							collapsible : true,
							split : true,
							width : '20%'
						}}, {{
							region : 'center',
							layout : 'fit',
							items : [{{
										xtype : ""component"",
										id : 'iframe-win',
										autoEl : {{
											tag : ""iframe"",
											src : ""intro.html""
										}}
									}}]
						}}]
			}});

	var formLogin = Ext.create('Ext.form.Panel', {{
		frame : true,
		bodyStyle : 'padding:5px 5px 0',
		fieldDefaults : {{
			msgTarget : 'side',
			labelWidth : 75
		}},
		defaultType : 'textfield',
		defaults : {{
			anchor : '100%'
		}},

		items : [{{
					fieldLabel : 'User Name',
					name : 'UserName',
					allowBlank : false
				}}, {{
					fieldLabel : 'Password',
					name : 'Password',
					inputType : 'password',
					allowBlank : false
				}}],

		buttons : [{{
			text : 'Login',
			formBind : true,
			handler : function() {{

				var form = this.up('form').getForm();
				var record = form.getFieldValues();
				Aicl.Util.executeRestRequest({{
					url : Aicl.Util.getUrlApi() + loginPath,
					method : 'get',
					success : function(result) {{
						Aicl.Util.setAuth(true);
						logoutButton.setDisabled(false);
						loginButton.setDisabled(true);
						windowLogin.hide();
						var ifr = Ext.getDom('iframe-win').contentWindow;
						ifr.document.open();
						ifr.document.write('Welcome : '
								+ result.UserName);
						ifr.document.close();
					}},
					failure : function(response, options) {{
						console.log(arguments);
					}},
					params : record
				}});
			}}
		}}]

	}});

	var windowLogin = Ext.create('Ext.Window', {{
				title : 'Login',
				closable : true,
				closeAction : 'hide',
				height : 150,
				width : 300,
				layout : 'fit',
				modal : true,
				y : 65,
				items : [formLogin]
			}});

	var login = function() {{
		windowLogin.show();
	}}

	var logout = function() {{

		Aicl.Util.executeRestRequest({{
					url : Aicl.Util.getUrlApi() + logoutPath,
					method : 'POST',
					callback : function(result, success) {{
						Aicl.Util.setAuth(false);
						logoutButton.setDisabled(true);
						loginButton.setDisabled(false);
						var ifr = Ext.getDom('iframe-win').contentWindow;
						ifr.document.open();
						ifr.document.write('by');
						ifr.document.close();
					}}
				}});
	}}

}});";
		
		private string indexTemplate=@"<html>
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=iso-8859-1"" />
    <title>{0}</title>
    <link rel=""stylesheet"" type=""text/css"" href=""extjs/resources/css/{1}""/>
    <link rel=""stylesheet"" type=""text/css"" href=""resources/util.css""/>
	<script type=""text/javascript"" src=""extjs/bootstrap.js""></script>
	<script type=""text/javascript"" src=""resources/util.js""></script>
    <script type=""text/javascript"" src=""modules/login/app.js"" ></script>
</head>
<body></body>
</html>";
		
		private string utilJsTemplate=@"(function(){{
	Ext.ns('Aicl.Util');
	Aicl.Util = {{}}; 
	
	var Util = Aicl.Util, 
		_msgCt, 
		_createBox= function (title, content){{
       		return '<div class=""msg""><h3>' + title + '</h3><p>' + content + '</p></div>';
    	}};
    	
    _registerSession=function(result){{
		sessionStorage[""authenticated""]= true;
		sessionStorage[""roles""]=Ext.encode(result.Roles);
		sessionStorage[""permissions""]=Ext.encode(result.Permissions);
		sessionStorage[""displayName""]=result.DisplayName;				
	}};

	_clearSession=function(result){{
		sessionStorage[""authenticated""]= false;
		sessionStorage.removeItem(""roles"");
		sessionStorage.removeItem(""permissions"");
		sessionStorage.removeItem(""displayName"");
	}};
	        
    Ext.apply(Util,{{
    	
    	convertToDate: function (v){{
			if (!v) return null;
			return (typeof v == 'string')
			?new Date(parseFloat(/Date\(([^)]+)\)/.exec(v)[1])) // thanks demis bellot!
			:v ;   
		}},

		convertToUTC: function (date){{
			return Ext.Date.format(date,'MS');
		}},
		
		formatInt: function (value, format){{
			return this.formatNumber(value, ',0');		
		}},

		formatNumber: function (value, format){{
			format= format|| ',0.00'; 
			return ( typeof(value) =='number')?
				Ext.util.Format.number(value,format):
				Ext.util.Format.currency( this.unFormatValue(value),format );
		}},

		formatCurrency: function (value){{
			return typeof(value=='number')?
				Ext.util.Format.currency(value):
				Ext.util.Format.currency( this.unFormatValue(value) );
			  
		}},

		unFormatValue: function (value){{
			return  value.replace(/[^0-9\.]/g, '');
		}},

		isToday: function(date){{
			var d ;
			if( typeof date=='object') d= Ext.Date.format(date,'d.m.Y');
			else d= Ext.util.Format.substr(date,0,10);
	
			var today = Ext.Date.format(new Date() ,'d.m.Y');
			return today==d;
		}},
		
		// ajax request
		
		executeAjaxRequest: function (config){{
			Ext.MessageBox.show({{
				msg: config.msg || 'Please wait...',
				progressText: config.progessText || 'Executing...',
		        width: config.width || 300,
				wait:true,
		        waitConfig: {{interval:200}}
		   		//icon:'ext-mb-download' //custom class in msg-box.html
			}});
		
			Ext.Ajax.request({{
				url: config.url + ( Ext.util.Format.uppercase(config.method)=='DELETE'
									? Aicl.Util.paramsToUrl(config.params) 
									:config.format==undefined?'': Ext.String.format('?format={{0}}', config.format)),
				method: config.method, 
			    success: function(response, options) {{
		            var result = Ext.decode(response.responseText);
					if(result.ResponseStatus.ErrorCode){{
						Aicl.Util.msg('Ajax request error in ResponseStatus', result.ResponseStatus.Message + ' -( '); 
						return ;
					}}
					if(config.showReady) Aicl.Util.msg('Ready', result.ResponseStatus.Message);
					if( config.success ) config.success(result);
					
			    }},
			    failure: function(response, options) {{
			    	var result={{}};
			    	if(response.responseText){{
			    		 result= Ext.decode(response.responseText);
			    	}}
			    	else{{
			    		result=response;
			    	}}
		            
					Aicl.Util.msg('Ajax request failure', 'Status: ' + response.status +'<br/>Message: '+
					 ((result.ResponseStatus)? result.ResponseStatus.Message: response.statusText) +' -( ');
					if( config.failure ) config.failure(result);
				}},
				callback: function(options, success, response) {{
					Ext.MessageBox.hide();	
					var result={{}};
					if(response.responseText){{
						result = Ext.decode(response.responseText);
					}}
					else{{
						result=response;
					}}
					if( config.callback ) config.callback(result, success);
				}},
				params:config.params
			}});	
		}},

		executeRestRequest: function (config){{
			config.format=  config.format|| 'json';
			this.executeAjaxRequest(config)
		}},

		paramsToUrl:function(params){{
			var s='';
			for( p in params){{
				s= Ext.String.urlAppend(s, Ext.String.format('{{0}}={{1}}', p, params[p]));
			}};
			return s;
			
		}},
		
		// proxies
			
		createRestProxy:function (config){{
			
			config.format=config.format|| 'json';
			config.type= config.type || 'rest';
			
			config.api=config.api||{{}};
			config.api.create=config.api.create|| config.url+'/create';
			config.api.read=config.api.read|| config.url+'/read';
			config.api.update=config.api.update|| config.url+'/update';
			config.api.destroy=config.api.destroy|| config.url+'/destroy';
			config.url= config.url || Aicl.Util.getUrlApi()+'/' + config.storeId;
			
			return this.createProxy(config);
			
		}},
		
		createAjaxProxy:function (config){{
			config.type=config.type||'ajax';
			config.url= config.url || Aicl.Util.getHttpUrlApi()+'/' + config.storeId;
			var proxy= this.createProxy(config);
			proxy.actionMethods= {{create: ""POST"", read: ""GET"", update: ""PUT"", destroy: ""DELETE""}};
			return proxy;
		}},
		
		createProxy:function (config){{
				
			if(config.format){{
				config.extraParams=config.extraParams||{{}};
				config.extraParams['format']=config.format
			}}
			
			config.api=config.api||{{}};
			config.api.create=config.api.create;
			config.api.read=config.api.read;
			config.api.update=config.api.update;
			config.api.destroy=config.api.destroy;
			
			config.reader= config.reader||{{
				type: 'json',
		        root: 'Data', 
				totalProperty : undefined,
		    	successProperty	: undefined,
				messageProperty :  undefined
			}};
			
			config.writer= config.writer || {{
				type: 'json',
				getRecordData: function(record) {{ 
					console.log('Proxy writer getRecordData', record);
					return record.data; 
				}}
			}};
			
			var proxy={{
				type: config.type,
				url : config.url,
				api : config.api,
		    	reader: config.reader,
				writer: config.writer,
				pageParam: config.pageParam? config.pageParam: undefined,
				limitParam: config.limitParam? config.limitParam:undefined,
				startParam: config.startParam? config.startParam:undefined,
				extraParams: config.extraParams?config.extraParams:undefined,
				storeId: config.storeId,
				listeners:config.listeners ||
				{{
		        	exception:function(proxy, response,  operation,  options) {{
		        		console.log('Proxy exception store: '+ this.storeId, arguments)
		            	var result={{}};
			    		if(response.responseText){{
			    	 		result= Ext.decode(response.responseText);
			    		}}
			    		else{{
			    			result=response;
			    		}}
						Aicl.Util.msg('Proxy exception store:' + this.storeId ,
						'Status: ' + response.status +'<br/>Message: '+
							((result.ResponseStatus)? result.ResponseStatus.Message: response.statusText) +' -( ');
					 	
					 	if(this.storeId){{
					 		var store= Ext.getStore(this.storeId);
					 		if(store) store.rejectChanges();
					 	}}
		        	}}
		        	
		    	}}
		    }};
		    return proxy;
		}},
				
		isAuth: function (){{
			var v = sessionStorage[""authenticated""]
			return v==undefined? false: Ext.decode(v);
		}},
				
		login: function(config){{
			this.executeRestRequest({{
				url : Aicl.Util.getUrlLogin(),
				method : 'get',
				success : function(result) {{
					_registerSession(result);
					if(config.success) config.success(result);
				}},
				failure : config.failure,
				callback: config.callback,
				params : config.params
			}});
		}},
				
		logout: function(config){{
			config=config||{{}};
			this.executeRestRequest({{
				url : Aicl.Util.getUrlLogout()+'?format=json',
				method : 'delete',
				callback : function(result, success) {{
					_clearSession();
					if(config.callback) config.callback(result,success);
				}},
				failure : config.failure,
				success : config.success
			}});
		}},
				
		getRoles:function(){{
			return sessionStorage.roles? Ext.decode(sessionStorage.roles): [];		
		}},
		
		setUrlLogin: function (urlLogin){{
			sessionStorage[""urlLogin""]=urlLogin;
		}},
		
		getUrlLogin:function (){{
			return sessionStorage[""urlLogin""];
		}},
		
		setUrlLogout: function (urlLogout){{
			sessionStorage[""urlLogout""]=urlLogout;
		}},
		
		getUrlLogout:function (){{
			return sessionStorage[""urlLogout""];
		}},
				
		setUrlApi: function (urlApi){{
			sessionStorage[""urlApi""]=urlApi;
		}},
		
		getUrlApi:function (){{
			return sessionStorage[""urlApi""];
		}},
		
		getHttpUrlApi:function (){{
			return sessionStorage[""httpUrlApi""];
		}},
		
		setHttpUrlApi:function(httpUrlApi){{
			sessionStorage[""httpUrlApi""]=httpUrlApi;
		}},
		
		setPhotoDir: function(photoDir){{
			sessionStorage[""photoDir""]=photoDir;
		}},
		
		getPhotoDir: function(){{
			return sessionStorage[""photoDir""];
		}},
		
		setEmptyImgUrl: function( url){{
			sessionStorage[""emptyImgUrl""]= url; 
		}},
		
		getEmpytImgUrl: function(){{
			return sessionStorage[""emptyImgUrl""];
		}},
		
		hasRole:function (role){{
			var roles= this.getRoles();
			for(var r in roles){{
 			  if (roles[r].Name==role) return true;
			}};
			return false;
		}},
		
		hasPermission:function (permission){{
			var a= sessionStorage.permissions? Ext.decode(sessionStorage.permissions): [];
			return a.indexOf(permission)>=0;
		}},
		
		
		//helpers
		isValidEmail:function (email) {{
			var filter = /^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{{2,4}})+$/;
			return filter.test(email)
		}},
		
		msg: function(title, format){{
            if(!_msgCt){{
                _msgCt = Ext.core.DomHelper.insertFirst(document.body, {{id:'msg-div'}}, true);
            }}
            var s = Ext.String.format.apply(String, Array.prototype.slice.call(arguments, 1));
            var m = Ext.core.DomHelper.append(_msgCt, _createBox(title, s), true);
            m.hide();
            m.slideIn('t').ghost(""t"", {{ delay: 1000, remove: true}});
		}}   	
    	
    }});	
    
}})();
    	

Ext.define('Aicl.data.Store',{{
	extend: 'Ext.data.Store',
    constructor: function(config){{    	    	
    	
    	config.proxy= config.proxy || Aicl.Util.createRestProxy( {{
    		storeId: config.storeId,
			url: config.proxy && config.proxy.url?
				config.proxy.url:
				Aicl.Util.getUrlApi()+'/' + config.storeId
    	}});
    	
    	config.autoLoad= config.autoLoad==undefined? false: config.autoLoad;
    	config.autoSync= config.autoSync==undefined? true: config.autoSync;
    	config.listeners= config.listeners ||
    	{{
            write: (config.listeners && config.listeners.write)?
            config.listeners.write(store, operation, options):
            function(store, operation, options){{
            	//console.log('store'+ this.storeId+ '  write arguments: ', arguments); 
                var record =  operation.getRecords()[0],
                    name = Ext.String.capitalize(operation.action),
                    verb;                                
                if (name == 'Destroy') {{
                	record =operation.records[0];
                    verb = 'Destroyed';
                }} else {{
                    verb = name + 'd';
                }}
                //console.log('store'+ this.storeId +' write record: ', record);
                Aicl.Util.msg(name, Ext.String.format(""{{0}} {{1}}: {{2}}"", verb, this.storeId , record.getId()));
            }}
        }};
        
        this.callParent(arguments);
    }}
}});  


Ext.data.Store.implement({{
    rejectChanges: function(){{
    	Ext.each(this.removed, function(record){{
   			this.insert(record.lastIndex || 0, record);
  		}}, this);
 
  		Ext.each(this.getUpdatedRecords(), function(record) {{
   			record.reject();
  		}}, this);
 
  		this.remove(this.getNewRecords());
   		//this.each(function(record) {{   		//	record.reject();  		//}}, this);
   		this.removed = [];
    }},
    //record={{somefield:'value', othefield:'value'}}
    save:function(record){{
		if (record.Id){{
			var keys = Ext.create( this.model.getName(),{{}}).fields.keys;
			var sr = this.getById(parseInt( record.Id) );
			sr.beginEdit();
			for( var r in record){{
				if( keys.indexOf(r)>0 )
					sr.set(r, record[r])
			}}
			sr.endEdit(); 
		}}
		else{{
			var nr = Ext.create( this.model.getName(),record );
			this.add(nr);
		}}			
	}},
	
	canCreate:function(){{
		 return Aicl.Util.hasPermission(Ext.String.format('{{0}}.create', this.storeId));
	}},
    canRead:function(){{
		 return Aicl.Util.hasPermission(Ext.String.format('{{0}}.read', this.storeId));
	}},
	canUpdate:function(){{
		 return Aicl.Util.hasPermission(Ext.String.format('{{0}}.update', this.storeId));
	}},
	canDestroy:function(){{
		 return Aicl.Util.hasPermission(Ext.String.format('{{0}}.destroy', this.storeId));
	}},
	canExecute:function(operation){{
		 return Aicl.Util.hasPermission(Ext.String.format('{{0}}.{{1}}', this.storeId,operation));
	}}
    
}});

Ext.form.Panel.implement({{
    setFocus:function(item){{
    	var ff = item==undefined?this.items.items[1].name:Ext.isNumber(item)?this.items.items[item].name:item;
    	this.getForm().findField(ff).focus(false,10);
    }}
}});";
		
		private string utilCssTemplate=@"body {{
    padding:5px;
    padding-top:5px;
}}
.x-body {{
    font-family:helvetica,tahoma,verdana,sans-serif;
    font-size:13px;
}}
p {{
    margin-bottom:15px;
}}
h1 {{
    font-size:18px;
    margin-bottom:20px;
}}
h2 {{
    font-size:14px;
    color:#333;
    font-weight:bold;
    margin:10px 0;
}}
.example-info{{
    width:150px;
    border:1px solid #c3daf9;
    border-top:1px solid #DCEAFB;
    border-left:1px solid #DCEAFB;
    background:#ecf5fe url( info-bg.gif ) repeat-x;
    font-size:10px;
    padding:8px;
}}
pre.code{{
    background: #F8F8F8;
    border: 1px solid #e8e8e8;
    padding:10px;
    margin:10px;
    margin-left:0px;
    border-left:5px solid #e8e8e8;
    font-size: 12px !important;
    line-height:14px !important;
}}
.msg .x-box-mc {{
    font-size:14px;
}}
#msg-div {{
    position:absolute;
    left:35%;
    top:10px;
    width:300px;
    z-index:20000;
}}
#msg-div .msg {{
    border-radius: 8px;
    -moz-border-radius: 8px;
    background: #F6F6F6;
    border: 2px solid #ccc;
    margin-top: 2px;
    padding: 10px 15px;
    color: #555;
}}
#msg-div .msg h3 {{
    margin: 0 0 8px;
    font-weight: bold;
    font-size: 15px;
}}
#msg-div .msg p {{
    margin: 0;
}}
.x-grid3-row-body p {{
    margin:5px 5px 10px 5px !important;
}}

.feature-list {{
    margin-bottom: 15px;
}}
.feature-list li {{
    list-style: disc;
    margin-left: 17px;
    margin-bottom: 4px;
}}

.x-cell-positive {{color:black; text-align : right;}}
.x-cell-negative {{color:red;   text-align : right;}}


.x-item-disabled {{
    color: #888888 !important;
    -moz-opacity: 100;
    opacity: 1;
}}

.x-form-item-label .x-item-disabled {{
    color: #888888 !important;
    -moz-opacity: 100;
    opacity: 1;
}}

.add {{
	background-image:url(icons/fam/add.gif) !important;
}}
.remove {{
    background-image:url(icons/fam/delete.gif) !important;
}}
.save {{
    background-image:url(icons/fam/database_save.png) !important;
}}
.preview {{
    background-image:url(icons/fam/eye.png) !important;
}}
.print {{
    background-image:url(icons/fam/printer.png) !important;
}}
.load {{
    background-image:url(icons/fam/database_refresh.png) !important;
}}
.edit {{
    background-image:url(icons/fam/page_edit.gif) !important;
}}

.stop{{
	background-image:url(icons/fam/cross.gif) !important;
}}

.select{{
	background-image:url(icons/silk/arrow_right.png) !important;
}}

.password{{
	background-image:url(icons/silk/key.png) !important;
}}

.mail{{
	background-image:url(icons/silk/email.png) !important;
}}

.upload{{
	background-image:url(icons/silk/up.png) !important;
}}

#module{{
	margin-left:auto;
	margin-right:auto;
	width: 79em;
	text-align: left;
}}

// Login form 
.form-login-icon-title {{
    background-image: url(""icons/silk/locked.png"")
}}

.form-login-header {{
    background: transparent url(""icons/silk/lock.png"") no-repeat 97% 50%;
    font-size: 11px;
    font-weight: bold;
    padding: 10px 45px 10px 10px;
}}

.form-login-header .error {{
    color: red;
}}

.form-login-icon-login {{
    background-image: url(""icons/silk/login.png"")
}}

.form-login-icon-cancel {{
    background-image: url(""icons/silk/close.png"")
}}

.form-login-warning {{
    background: url(""icons/silk/warning.png"") no-repeat center left;
    padding: 2px;
    padding-left: 20px;
    font-weight: bold;
}}

// Navigation 

.expand {{
    background-image: url('icons/silk/expand.png') !important;
}}

.collapse {{
    background-image: url('icons/silk/collapse.png') !important;
}}

.datalink div{{
   text-decoration: underline;
    cursor: pointer;
}}

.logout {{
    background-image: url('icons/silk/logout.png') !important;
}}";
		
		private string introTemplate=@"<html>
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=iso-8859-1"" />
</head>
<body></body>
</html>";
		
		private string licenseTemplate=@"{0}
Copyright (c) Angel Ignacio Colmenares Laguado
All rights reserved.

Open Source License
------------------------------------------------------------------------------------------
This software is licensed under the terms of the Open Source GPL 3.0 license. 

http://www.gnu.org/licenses/gpl.html


This library is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, AND NON-INFRINGEMENT OF THIRD-PARTY INTELLECTUAL PROPERTY RIGHTS.
See the GNU General Public License for more details.";

		
		private string loginAppTemplate=@"Ext.Loader.setConfig({{enabled: true}});
Ext.Loader.setPath('{0}', 'modules/app');
    
Ext.application({{
name: '{0}',
appFolder: 'modules/app',

launch: function(){{
	Aicl.Util.setUrlApi(location.protocol + '//' + location.host + '/api');
	Aicl.Util.setHttpUrlApi(location.protocol + '//' + location.host + '/api'+'/json/asynconeway')
		
	Aicl.Util.setUrlLogin(location.protocol + '//' + location.host + '/api/login');
	Aicl.Util.setUrlLogout(location.protocol + '//' + location.host + '/api/logout');
	
	Aicl.Util.setPhotoDir(location.protocol + '//' + location.host +  '' + location.pathname+ 'photos');
	Aicl.Util.setEmptyImgUrl('../../resources/icons/fam/user.png');
    var loginWin = Ext.create('App.view.Login');
    loginWin.show();
}},
    
controllers: ['Login']
    
}});  
";
		private string loginViewTemplate=@"Ext.define('{0}.view.Login', {{
    extend:'Ext.window.Window',
    alias:'widget.login',
    iconCls:'form-login-icon-title',
    width:420,
    height:210,
    resizable:false,
    closable:false,
    draggable:false,
    modal:true,
    closeAction:'hide',
    layout:'border',
    title:'Login',

    initComponent:function () {{

        Ext.apply(this, {{
            items:[
                {{
                    xtype:'panel',
                    cls:'form-login-header',
                    baseCls:'x-plain',
                    //html:'intro',
                    region:'north',
                    height:60
                }},
                {{
                    xtype:'form',
                    baseCls:'x-plain',
                    ui:'default-framed',
                    bodyPadding:10,
                    header:false,
                    region:'center',
                    border:false,
                    waitMsgTarget:true,
                    layout:{{
                        type:'vbox',
                        align:'stretch'
                    }},
                    defaults:{{
                        labelWidth:85
                    }},
                    items:[
                        {{
                            itemId:'userName',
                            xtype:'textfield',
                            fieldLabel:'Username',
                            name:'userName',
                            allowBlank:false,
                            anchor:'100%',
                            validateOnBlur:false
                        }},
                        {{
                            xtype:'textfield',
                            fieldLabel:'Password',
                            name:'password',
                            allowBlank:false,
                            inputType:'password',
                            anchor:'100%',
                            validateOnBlur:false,
                            enableKeyEvents:true,
                            listeners:{{
                                render:{{
                                    fn:function (field, eOpts) {{
                                        field.capsWarningTooltip = Ext.create('Ext.tip.ToolTip', {{
                                            target:field.bodyEl,
                                            anchor:'top',
                                            width:305,
                                            html:'Caps lock warning'
                                        }});

                                        // disable to tooltip from showing on mouseover
                                        field.capsWarningTooltip.disable();
                                    }},
                                    scope:this
                                }},

                                keypress:{{
                                    fn:function (field, e, eOpts) {{
                                        var charCode = e.getCharCode();
                                        if ((e.shiftKey && charCode >= 97 && charCode <= 122) ||
                                            (!e.shiftKey && charCode >= 65 && charCode <= 90)) {{

                                            field.capsWarningTooltip.enable();
                                            field.capsWarningTooltip.show();
                                        }}
                                        else {{
                                            if (field.capsWarningTooltip.hidden === false) {{
                                                field.capsWarningTooltip.disable();
                                                field.capsWarningTooltip.hide();
                                            }}
                                        }}
                                    }},
                                    scope:this
                                }},

                                blur:function (field) {{
                                    if (field.capsWarningTooltip.hidden === false) {{
                                        field.capsWarningTooltip.hide();
                                    }}
                                }}
                            }}
                        }}
                    ]
                }}
            ],
            buttons:[
                {{
                    action:""login"",
                    formBind:true,
                    text:'Login',
                    iconCls:'form-login-icon-login',
                    scale:'medium',
                    width:90
                }}
            ]
        }});
        this.callParent(arguments);
    }},
    defaultFocus:'userName'
}});
";
		
		private string loginControllerTemplate=@"Ext.define('{0}.controller.Login',{{
    extend:'Ext.app.Controller',
    init:function () {{
        this.control({{
          
            'login button[action=login]':{{
                click:this.login
            }},
            'login textfield':{{
                specialkey:this.keyenter
            }}
        }});
    }},
    views:[
        'Login'
    ],
    refs:[
         {{ref:'loginWindow', selector:'login'}},
         {{ref:'loginForm', selector:'form'}}
    ],
    
    login:function () {{
    	var form = this.getLoginForm();
    	if(!form.getForm().isValid()){{
    		Aicl.Util.msg('Empty fields','please write username and password');
    		return;
    	}}
        var me=this;
    	var record = form.getValues();
				Aicl.Util.login({{
					success : function(result) {{
						me.getLoginWindow().hide()
						me.createMenu();
					}},
					failure : function(response, options) {{
						console.log(arguments);
					}},
					params : record
				}});
    	
    }},
    
    keyenter:function (item, event) {{
        if (event.getKey() == event.ENTER) {{
            this.login();
        }}

    }},
    
    createMenu: function(){{
		var me = this;
		var buttons=[];
		var i=0;
		var grupos = Aicl.Util.getRoles();
		for(var grupo in grupos ){{
			if(grupos[grupo].Directory){{
				buttons[i]= Ext.create('Ext.Button', {{
    				text    : grupos[grupo].Title,
    				directory:grupos[grupo].Directory,
    				scale   : 'small',
    				handler	: function(){{
    				Ext.getDom('iframe-win').src = 'modules/'+this.directory;
    				}}
				}});
				i++;
			}}
		}};
		
		buttons[i]= Ext.create('Ext.Button', {{
	    	text    : 'Salir',
	    	scale   : 'small',
	    	handler	: function(){{
	    		Aicl.Util.logout({{
	    			callback:function(result, success){{
	    				vp.destroy();
	    				me.getLoginWindow().show();
	    			}}
	    		}});
	    	}}
		}});
		
    	var vp=Ext.create('Ext.Viewport', {{
        	layout: {{
        		type: 'border',
            	padding: 5
        	}},
        	defaults: {{
            	split: true
        	}},
        	items: [{{
            	region: 'west',
            	layout:'fit',
            	items:[{{
            		layout: {{                        
    	    			type: 'vbox',
        				align:'stretch'
    				}},
    				defaults:{{margins:'5 5 5 5'}},
        			items:buttons
            	}}],
            	collapsible: true,
            	split: true,
            	width: '20%'
        	}},{{
            	region: 'center',
            	layout:'fit',
            	items:[{{
        			xtype : 'component',
        			id    : 'iframe-win', 
        			autoEl : {{
	            		tag : 'iframe',
            			src : 'intro.html'
        			}}
            	}}]
        	}}]
    	}});
	}}
    
}});
";
		
	}
}
