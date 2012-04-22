using System;
using System.IO;
using ServiceStack.Text;
using ServiceStack.OrmLite;

namespace Aicl.DotJs.Ext
{
	public class Controller
	{
		private string template=@"Ext.define('{0}',{{
	extend: '{1}',
    stores: ['{2}'],  
    models: ['{2}'],
    views:  ['{3}.List','{3}.Form' ],
    refs:[
    	{{ref: '{3}List',    	 selector: '{3}list' }},
    	{{ref: '{3}DeleteButton', selector: '{3}list button[action=delete]' }},
    	{{ref: '{3}NewButton',    selector: '{3}list button[action=new]' }},
    	{{ref: '{3}Form',    	 selector: '{3}form' }}, 
    	{{ref: '{3}SaveButton', 	 selector: '{3}form button[action=save]' }}
    ],

    init: function(application) {{
    	    	
        this.control({{
            '{3}list': {{ 
                selectionchange: function( sm,  selections,  eOpts){{
                	this.refreshButtons(selections);
                }}
            }},
            
            '{3}list button[action=delete]': {{
                click: function(button, event, options){{
                	var grid = this.get{2}List();
                	var record = grid.getSelectionModel().getSelection()[0];
        			this.get{2}Store().remove(record);
                }}
            }},
            
            '{3}list button[action=new]': {{
            	click:function(button, event, options){{
            		this.get{2}List().getSelectionModel().deselectAll();
            	}}
            }},
            
            '{3}form button[action=save]':{{            	
            	click:function(button, event, options){{
            		var model = this.get{2}Store();
            		var record = this.get{2}Form().getForm().getFieldValues(true);
            		this.get{2}Store().save(record);
            	}}
            }}
        }});
    }},
    
    onLaunch: function(application){{
    	this.get{2}Store().on('write', function(store, operation, eOpts ){{
    		var record =  operation.getRecords()[0];                                    
            if (operation.action != 'destroy') {{
               this.get{2}List().getSelectionModel().select(record,true,true);
               this.refreshButtons([record]);
            }}
    	}}, this);
    }},
        	
	refreshButtons: function(selections){{	
		selections=selections||[];
		if (selections.length){{
			this.get{2}NewButton().setDisabled(!this.get{2}Store().canCreate());
        	this.get{2}Form().getForm().loadRecord(selections[0]);
            this.get{2}SaveButton().setText('Update');
            this.get{2}DeleteButton().setDisabled(!this.get{2}Store().canDestroy());
            this.get{2}SaveButton().setDisabled(!this.get{2}Store().canUpdate());
        }}
        else{{
        	this.get{2}Form().getForm().reset();            
        	this.get{2}SaveButton().setText('Add');
        	this.get{2}DeleteButton().setDisabled(true);
        	this.get{2}NewButton().setDisabled(true);
        	this.get{2}SaveButton().setDisabled(!this.get{2}Store().canCreate());
        	this.get{2}Form().setFocus();
        }};
        this.enableAll();
	}},
	
	disableForm:function(){{
		this.get{2}Form().setDisabled(true);
	}},
	
	enableForm:function(){{
		this.get{2}Form().setDisabled(false);	
	}},

	disableList:function(){{
		this.get{2}List().setDisabled(true);
	}},
	
	enableList:function(){{
		this.get{2}List().setDisabled(false);
	}},
	
	disableAll: function(){{
		this.get{2}List().setDisabled(true);
		this.get{2}Form().setDisabled(true);
	}},
	
	enableAll: function(){{
		this.get{2}List().setDisabled(false);
		this.get{2}Form().setDisabled(false);
	}},
	
	onselectionchange:function(fn, scope){{
		this.get{2}List().on('selectionchange', fn, scope);
	}},
	
	onwrite:function(fn, scope){{
		this.get{2}Store().on('write', fn, scope);
	}}
	
}});
";
		private Type type;
		
		public Controller(Type type)
		{
			this.type=type;
		}
		
		public string AppName{ get; set;}
		
		public string Define { get; set;}
		
		public string Extend{ get; set;}
			
		public string FileName { get; set;}
		
		public string OutputDirectory { get; set;}
		
		public void Write(){
			
			if(string.IsNullOrEmpty(AppName)) AppName="App";
			
			if(string.IsNullOrEmpty(Define))
				Define= string.Format("{0}.controller.{1}",AppName,type.Name);
						
			if(string.IsNullOrEmpty(Extend))
				Extend= Config.Controller;
			
			
			if(string.IsNullOrEmpty(OutputDirectory))
			{
				OutputDirectory= Path.Combine(Directory.GetCurrentDirectory(),Config.ControllerDirectory);
				
				if (!Directory.Exists(OutputDirectory))
					Directory.CreateDirectory(OutputDirectory);
			}
			
			if(string.IsNullOrEmpty(OutputDirectory))
				OutputDirectory= Path.Combine(Directory.GetCurrentDirectory(), "modules");				
			
			if (!Directory.Exists(OutputDirectory))
					Directory.CreateDirectory(OutputDirectory);
			
			string controlerDir = Path.Combine(OutputDirectory, Config.ControllerDirectory);
			
			if (!Directory.Exists(controlerDir))
					Directory.CreateDirectory(controlerDir);
						
			if(string.IsNullOrEmpty(FileName))
				FileName= type.Name+ ".js";
			
			
			using (TextWriter tw = new StreamWriter(Path.Combine(controlerDir, FileName)))
			{
				tw.Write(string.Format(template, Define, Extend, type.Name, type.Name.ToLower()));
				tw.Close();
			}
			
			
		}		
		
	}
}

