using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using ServiceStack.OrmLite;
using ServiceStack.Text;
using ServiceStack.Common.Extensions;

namespace Aicl.DotJs.Ext
{
	public class Form
	{
		
		private string template = @"Ext.define('{0}', {{
    extend: '{1}',
    alias : '{2}',
    ui:'default-framed',
    constructor: function(config){{
    	config=config|| {{}};
    	config.frame=config.frame==undefined?false: config.frame;
    	config.margin=config.margin|| '0 0 0 5';
    	config.bodyStyle = config.bodyStyle ||'padding:10px 10px 0';
    	config.width = config.width|| 360;
        config.height = config.height|| {4};
        config.autoScroll= config.autoScroll==undefined? true: config.autoScroll,
		config.fieldDefaults = config.fieldDefaults || {{
            msgTarget: 'side',
            labelWidth: 120,
			labelAlign: 'right'
        }};
        config.defaultType = config.defaultType|| 'textfield';
        config.defaults = config.defaults || {{
            anchor: '100%',
			labelStyle: 'padding-left:4px;'
        }};
    	if (arguments.length==0 )
    		this.callParent([config]);
    	else
    		this.callParent(arguments);
    }},
     
    initComponent: function() {{
        this.items = {3};
 
        this.buttons = [{{ 
            text:'Add',
            formBind: false,
            disabled:true,
            action:'save'      
	    }}];
 
        this.callParent(arguments);
    }}
}});";
		
		private Type type;
		
		public string AppName { get; set;}
		
		public string Define { get; set;}
		
		public string Extend { get; set;}
		
		public string Alias { get; set;}
		
		public string FileName { get; set;}
		
		public string OutputDirectory { get; set;}
		
		public Form (Type type)
		{
			this.type=type;
		}
		
		public void Write()
		{
			if(string.IsNullOrEmpty(AppName)) AppName="App";
			
			if(string.IsNullOrEmpty(Define))
				Define= string.Format("{0}.view.{1}.Form",AppName, type.Name.ToLower());
						
			if(string.IsNullOrEmpty(Extend))
				Extend= Config.Form;
			
			if(string.IsNullOrEmpty(Alias))
				Alias= string.Format("widget.{0}form",type.Name.ToLower());
			
			
			List<FormItem> items= new List<FormItem>();
			
			foreach(PropertyInfo pi in type.GetProperties())
			{
				FormItem item = new FormItem();
				item.name= string.Format("'{0}'",pi.Name);
				if(pi.Name== OrmLiteConfig.IdField)
				{
					item.xtype="'hidden'";
					items.Add(item);
					continue; 
				}
				
				item.fieldLabel=string.Format("'{0}'",pi.Name);
				
				if(!pi.PropertyType.ToString().StartsWith("System.Nullable") && pi.PropertyType!=typeof(string) )
				   item.allowBlank=false;
				   
				if( pi.PropertyType == typeof(string) )
				{
					RequiredAttribute ra = pi.FirstAttribute<RequiredAttribute>();
					if(ra != null)
					{
						item.allowBlank=false;
					}
					
					StringLengthAttribute la = pi.FirstAttribute<StringLengthAttribute>();
					if( la !=null )
					{
						item.maxLength= la.MaximumLength;
						item.enforceMaxLength=true;
					}
					if(pi.Name.ToUpper().Contains("MAIL") || pi.Name.ToUpper().Contains("CORREO"))
						item.vtype="'email'";
				}
				else if( pi.PropertyType == typeof(DateTime) || pi.PropertyType == typeof(DateTime?))
				{
					item.xtype="'datefield'";
					item.format="'d.m.Y'";
				}
				else if(pi.PropertyType== typeof(Int16) || pi.PropertyType== typeof(Int16?)
				        || pi.PropertyType== typeof(Int32) || pi.PropertyType== typeof(Int32?)
				        || pi.PropertyType== typeof(Int64) || pi.PropertyType== typeof(Int64?))
				{
					item.xtype="'numberfield'";
					item.allowDecimals=false;
				}
				else if(pi.PropertyType== typeof(decimal) || pi.PropertyType== typeof(decimal?)
				        || pi.PropertyType== typeof(double) || pi.PropertyType== typeof(double?)
				        || pi.PropertyType== typeof(float) || pi.PropertyType== typeof(float?))
				{
					item.xtype="'numberfield'";
				}
				else if(pi.PropertyType==typeof(bool) || pi.PropertyType==typeof(bool?))
				{
					item.xtype="'checkboxfield'";
				}
				
				items.Add(item);
			}
			
			int height = 80;
			
			int ic= items.Count(i=>i.xtype!="'hidden'");
			
			if(ic==2) height=140;
			else if (ic==3) height=150;
			else if (ic==4) height=160;
			else if (ic>=5) height=35*ic;
			if( height>350) height=350;
			
			string sItems= items.SerializeAndFormat().Replace("True","true").Replace("False","false");

			if(string.IsNullOrEmpty(OutputDirectory))
				OutputDirectory= Path.Combine(Directory.GetCurrentDirectory(), "modules");				
			
			if (!Directory.Exists(OutputDirectory))
					Directory.CreateDirectory(OutputDirectory);
			
			string viewDir = Path.Combine(Path.Combine(OutputDirectory, Config.ViewDirectory),type.Name.ToLower());				
			
			if (!Directory.Exists(viewDir))
					Directory.CreateDirectory(viewDir);
			
			if(string.IsNullOrEmpty(FileName))
				FileName= "Form.js";
			
				
			using (TextWriter tw = new StreamWriter(Path.Combine(viewDir, FileName)))
			{
				tw.Write(string.Format(template, Define, Extend, Alias, sItems, height));
				tw.Close();
			}
			
			
		}
		
		
	}
}

