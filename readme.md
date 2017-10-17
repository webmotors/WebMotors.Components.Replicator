# WebMotors Components Replicator
Component used to replicate data from a mysql database to the elasticsearch with dotnet C#
##Nuget url
https://www.nuget.org/packages/WebMotors.Components.Replicator
##Database configuration
```
	Turn ON Query Log in Mysql
		SET GLOBAL binlog_format = 'ROW';
		SET GLOBAL binlog_row_image = 'full';
		SET GLOBAL general_log = 'ON';

	In Our case we use Amazon RDS as Database in that case configura Database Parameter Group
		binlog_format set ROW
		binlog_row_image set full
		general_log set 1

	Create user with a custom permission in database to replicate with access SELECT, SHOW DATABASE AND SHOW VIEW
	After crate user makes some queries in database and test this querie
		SELECT
			event_time, argument
		FROM 
			mysql.general_log 
		WHERE 
			command_type = 'Query' 
		Limit 1000;
	Verify if the argument field came with the comand executed
```
##web.config or app.config configuration
```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<configSections>
		<section name="unity" type="Microsoft.Practices.Unity.Configuration.UnityConfigurationSection, Microsoft.Practices.Unity.Configuration" />
	</configSections>
	<connectionStrings>
		<add name="mysql:replicator" connectionString="your connection string;" providerName="MySql.Data.MySqlClient" />
	</connectionStrings>
	<unity xmlns="http://schemas.microsoft.com/practices/2010/unity">
		<alias alias="Constants" type="WebMotors.Components.Replicator.Constants, WebMotors.Components.Replicator" />
		<alias alias="Proccess" type="WebMotors.Components.Replicator.IProccess, WebMotors.Components.Replicator" />
		<alias alias="MysqlProccess" type="WebMotors.Components.Replicator.Implementations.Proccess.MysqlProccess, WebMotors.Components.Replicator" />

		<alias alias="QueryAnalizer" type="WebMotors.Components.Replicator.QueryAnalizer, WebMotors.Components.Replicator" />
		<alias alias="UpdateAnalizer" type="WebMotors.Components.Replicator.Implementations.Analizers.UpdateAnalizer, WebMotors.Components.Replicator" />
		<alias alias="InsertAnalizer" type="WebMotors.Components.Replicator.Implementations.Analizers.InsertAnalizer, WebMotors.Components.Replicator" />
		<alias alias="DeleteAnalizer" type="WebMotors.Components.Replicator.Implementations.Analizers.DeleteAnalizer, WebMotors.Components.Replicator" />

		<alias alias="Replicator" type="WebMotors.Components.Replicator.IReplicator, WebMotors.Components.Replicator" />
		<alias alias="ElasticSearchReplicator" type="WebMotors.Components.Replicator.Implementations.Replicators.ElasticSearch, WebMotors.Components.Replicator" />

		<alias alias="CoreModel" type="WebMotors.Components.Replicator.CoreModel, WebMotors.Components.Replicator" />
		<alias alias="MysqlDefault" type="WebMotors.Components.Replicator.Implementations.Model.MysqlDefault, WebMotors.Components.Replicator" />
		
		<!--Add alias to your custom objects like your Constants implementation and custom tables-->
		<alias alias="Configuration" type="YourNameSpace.Configuration, YourAssembly" />
		<container>
			<register type="Proccess" mapTo="MysqlProccess" name="mysql" />

			<register type="QueryAnalizer" mapTo="UpdateAnalizer" name="update" />
			<register type="QueryAnalizer" mapTo="InsertAnalizer" name="insert" />
			<register type="QueryAnalizer" mapTo="DeleteAnalizer" name="delete" />

			<register type="Replicator" mapTo="ElasticSearchReplicator" name="replicator" />
			<register type="CoreModel" mapTo="MysqlDefault" name="default" />

			<!--Add your custom objects in container like your Constants implementation and custom tables-->
			<register type="Constants" mapTo="Configuration" name="constants" />
		</container>
	</unity>
	<appSettings>
		<add key="proccess-type" value="mysql" />
	</appSettings>
</configuration>
```
##Usage Code Configuration
```
public class Configuration : Constants
{
	private List<string> _models = new List<string>() { "advert", "message_type" }; //Table names replication exactly on elasticsearch
	public override List<string> models
	{
		get { return _models; }
	}

	private List<string> _custom_models = new List<string>() { "client" }; //Table custom table let you configure json send to elasticsearch
	public override List<string> custom_models
	{
		get { return _custom_models; }
	}

	public override string connectionStringName
	{
		get { return ConfigurationManager.AppSettings["connection-database-name"]; }
	}

	public override string mysqlProccessUser
	{
		get { return ConfigurationManager.AppSettings["mysql-proccess-user"]; }
	}

	public override string logFolder
	{
		get { return ConfigurationManager.AppSettings["log-folder"]; }
	}

	public override string elasticSearchUrl
	{
		get { return ConfigurationManager.AppSettings["elastic-search-url"]; }
	}
}
```
##Usage Code Service as Console application
```
add 
	reference System.ServiceProccess
add
	using System.ServiceProcess;
code class
public class Program : ServiceBase
{
	static void Main(string[] args)
	{
		if (Environment.UserInteractive)
		{
			if ("S".Equals(ConfigurationManager.AppSettings["migrate-data"]))
				MigrateData();
			else
				ServiceStart();
		}
		else
			ServiceBase.Run(new Program());
	}

	protected override void OnStart(string[] args)
	{
		ServiceStart();
	}

	protected override void OnStop()
	{
		ServiceStop();
	}

	public static void ServiceStart()
	{
		ReplicatorThread.Start();
		if (Environment.UserInteractive)
		{
			Console.WriteLine("send \"exit\" to quit:");
			while (true)
			{
				var texto = Console.ReadLine();
				if (texto == "exit")
				{
					ReplicatorThread.Stop();
					break;
				}
			}
		}
	}

	public static void ServiceStop()
	{
		ReplicatorThread.Stop();
	}

	private static void MigrateData()
	{
		IUnityContainer container = new UnityContainer();
		container.LoadConfiguration();
		var proccess = container.Resolve<IProccess>(ConfigurationManager.AppSettings["proccess-type"]);
		proccess.Migrate(container);
	}
}
```
Translated by GOOGLE translate