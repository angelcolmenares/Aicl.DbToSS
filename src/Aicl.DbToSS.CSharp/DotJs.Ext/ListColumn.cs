using System;
namespace Aicl.DotJs.Ext
{
	public class ListColumn
	{
		public ListColumn ()
		{
		}
		
		public string text{get;set;}
		
		public string  dataIndex {get; set;}
		
		public int? flex {get; set;}
		
		public bool sortable  {get; set;}
		
		public object renderer { get; set;}
		
		public string xtype {get; set;}
		
		public string trueText {get; set;}
		
		public string falseText {get; set;}
		
		public string align {get; set;}
	}
	
}
/*
 
 {text: 'Department (Yrs)', xtype:'templatecolumn', tpl:'{dep} ({senority})'}
 
 */ 
