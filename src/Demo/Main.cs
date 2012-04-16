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
			
			
			CSharpSolutionCreator solution= new CSharpSolutionCreator(){
				ConnectionString="User=SYSDBA;Password=masterkey;Database=/home/angel/bd/GestionNegocios.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;",
				DialectProvider= FirebirdOrmLiteDialectProvider.Instance,
				SolutionName="Aicl.GranSolucion",
				AppTitle ="La Gran Solucion",
				SourceLibDir="/home/angel/Projects/github/ServiceStack/lib"
			};
			
			solution.Write();
			
			ExtSolutionCreator ext = new ExtSolutionCreator(){
				AppTitle=solution.AppTitle,
				AssemblyName= solution.OutputAssembly,
				SolutionName=solution.SolutionName,
				NameSpace=string.Format("{0}.{1}.Types",solution.SolutionName, solution.ModelNameSpace),
				ExtDir="/home/angel/Projects/librerias/extjs/extjs"
				
			};
			
			ext.Write();
			
			Console.WriteLine ("This is The End my friend!");
			
		}
	}
}
