using System;
using Aicl.DbToSS.CSharp;
using Aicl.DotJs.Ext;
using ServiceStack.OrmLite.Firebird;
namespace Demo
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");
	/*
			
			CSharpSolutionCreator solution= new CSharpSolutionCreator(){
				ConnectionString="User=SYSDBA;Password=masterkey;Database=Aicl.Galapago.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;",
				DialectProvider= FirebirdOrmLiteDialectProvider.Instance,
				SolutionName="Aicl.Galapago",
				AppTitle ="Galapago",
				SourceLibDir="/home/angel/Projects/github/ServiceStack/lib"
			};
			
			solution.Write();
			
			ExtSolutionCreator ext = new ExtSolutionCreator(){
				AppTitle=solution.AppTitle,
				AssemblyName= solution.OutputAssembly,
				Theme="ext-all-gray.css",
				SolutionName=solution.SolutionName,
				NameSpace=string.Format("{0}.{1}.Types",solution.SolutionName, solution.ModelNameSpace),
				ExtDir="/home/angel/Projects/librerias/extjs/extjs"
				
			};
			
			ext.Write();
			
			
			SqlCreator sqlCreator= new SqlCreator(){
				AssemblyName=solution.OutputAssembly,
				SolutionName=solution.SolutionName,
				NameSpace=ext.NameSpace
			};
			sqlCreator.Write();
   */       

            string assembly ="/home/angel/Projects/github/Aicl.Galapago/gh/src/Aicl.Galapago.Model/bin/Debug/Aicl.Galapago.Model.dll";

            ExtSolutionCreator ext = new ExtSolutionCreator(){
                AppTitle="Galapago Gestion Ingresos y Gastos",
                AssemblyName= assembly,
                Theme="ext-all-gray.css",
                SolutionName="Aicl.Galapago",
                NameSpace=string.Format("{0}.{1}.Types","Aicl.Galapago", "Model"),
                ExtDir="/home/angel/Projects/librerias/extjs/extjs"
                
            };
            
            ext.Write();
            
            
            SqlCreator sqlCreator= new SqlCreator(){
                AssemblyName=assembly,
                SolutionName="Aicl.Galapago",
                NameSpace=ext.NameSpace
            };
            sqlCreator.Write();


			Console.WriteLine ("This is The End my friend!");

			
		}
	}
}
