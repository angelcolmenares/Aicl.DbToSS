using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using ServiceStack.Text;

namespace Aicl.DotJs.Ext
{
	public class Store
	{
		private Type type; 
		
		private Dictionary <string,object> config= new Dictionary<string, object>();
		
		
		public Store (Type type)
		{
			this.type=type;
		}
		
		public string AppName{get; set;}
		
		public string Define{get; set;}
		
		public string Extend{get; set;}
		
		public string Model{get; set;}
		
		public string StoreId { get; set;}
		
		public string FileName { get; set;}
		
		public string OutputDirectory { get; set;}
		
		public void Write()
		{
			
			if(string.IsNullOrEmpty(AppName)) AppName="App";
		
			if(string.IsNullOrEmpty( Define ))
				Define= string.Format("{0}.store.{1}",AppName, type.Name);
						
			if(string.IsNullOrEmpty(Extend))
				Extend= Config.Store;
			
			config.Add("extend",Extend);
			
			if(string.IsNullOrEmpty( Model ))
				Model = string.Format("{0}.model.{1}",AppName, type.Name);			
			
			config.Add("model",Model);
			
			if(string.IsNullOrEmpty( StoreId ))
				StoreId= type.Name;
			
			var store = new ExtStore(StoreId);
			store.constructor= store.constructor.ToString().
				Replace("{","<<>>").Replace("}",">><<").
				Replace("[","<*>").Replace("]",">*<");
		
			Type t = typeof(ExtStore);
			
			foreach(KeyValuePair<string, object> kv in config)
			{
				PropertyInfo pi=  t.GetProperty(kv.Key);
				if(pi !=null)
				{
					pi.SetValue(store, kv.Value);
				}
			}
			
			if(string.IsNullOrEmpty(FileName))
				FileName= type.Name+".js";
			
			
			if(string.IsNullOrEmpty(OutputDirectory))
				OutputDirectory= Path.Combine(Directory.GetCurrentDirectory(), "modules");				
			
			if (!Directory.Exists(OutputDirectory))
					Directory.CreateDirectory(OutputDirectory);
			
			string storeDir =  Path.Combine(OutputDirectory, Config.StoreDirectory);				
			
			if (!Directory.Exists(storeDir))
					Directory.CreateDirectory(storeDir);
					
			
			
			string r= store.SerializeAndFormat().
				Replace("<<>>","{").Replace(">><<","}").
				Replace("<*>","[").Replace(">*<","]");
			r= string.Format( "Ext.define('{0}',{1});",Define, r);
			
			
			using (TextWriter tw = new StreamWriter(Path.Combine(storeDir, FileName)))
			{
				tw.Write(r);
				tw.Close();
			}
			
			
		}
	}
}


