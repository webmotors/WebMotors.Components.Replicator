using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using WebMotors.Components.Model.Core;

namespace WebMotors.Components.Replicator.Implementations.Proccess
{
	public class MysqlProccess : IProccess
	{
		#region [ properties ]
		private static string sql =
			@"SELECT
				event_time, argument
			FROM 
				mysql.general_log 
			WHERE 
				user_host != 'rdsadmin[rdsadmin] @ localhost [127.0.0.1]' and
				user_host != '{0}' and
				command_type = 'Query' and 
				event_time > @data
			Limit 1000;";
		#endregion

		#region [ Run ]
		public void Run(IUnityContainer container)
		{
			var constants = container.Resolve<Constants>(Constants.configuration);
			using (var database = new Database(constants.connectionStringName))
			{
				var command = database.CreateCommand(string.Format(sql, constants.mysqlProccessUser));
				command.Parameters.Add(database.CreateParameter("data", AutoResume(constants)));
				Dictionary<DateTime, string> queries = new Dictionary<DateTime, string>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
						queries.Add(reader.GetReaderValue<DateTime>("event_time"), GetQuery(reader.GetValue(reader.GetOrdinal("argument"))));
				}
				foreach (var query in queries)
				{
					var commandQuery = Command(query.Value);
					if (Constants.hasCommand(commandQuery))
					{
						var queryAnalizer = container.Resolve<QueryAnalizer>(commandQuery);
						queryAnalizer.database = database;
						queryAnalizer.Resolve(container, query.Value, database, constants);
					}
					AutoResume(query.Key, constants);
				}
			}
		}
		#endregion

		#region [ Migrate ]
		public void Migrate(IUnityContainer container)
		{
			var constants = container.Resolve<Constants>(Constants.configuration);
			AutoResume(DateTime.Now, constants);
			using (var database = new Database(constants.connectionStringName))
			{
				foreach (var table in constants.models)
					MigrateItem(container, table, true, database, constants);
				foreach (var table in constants.custom_models)
					MigrateItem(container, table, false, database, constants);
			}
		}
		private void MigrateItem(IUnityContainer container, string table, bool _default, Database database, Constants constants)
		{
			CoreModel.Log("Migrate table {0}", table);
			var model = container.Resolve<CoreModel>(_default ? Constants._default : table);
			model.database = database;
			model.table = table;
			model.Migrate(container, constants);
		}
		#endregion

		#region [ helpers ]
		private static string GetQuery(object argument)
		{
			try
			{
				return Encoding.GetEncoding("ISO-8859-1").GetString((byte[])argument).Replace("\t", " ").Replace("	", " ").Replace("\n", " ").Trim();
			}
			catch { }
			try
			{
				return System.Text.Encoding.UTF8.GetString((byte[])argument).Replace("\t", " ").Replace("	", " ").Replace("\n", " ").Trim();
			}
			catch { }
			return string.Empty;
		}

		private static string Command(string query)
		{
			if (!string.IsNullOrWhiteSpace(query))
				return query.Split(' ')[0].ToLower();
			return string.Empty;
		}
		#endregion

		#region [ AutoResume ]
		private static string _fileResume = "auto-resume.txt";
		private static DateTime AutoResume(Constants constants)
		{
			CoreModel.Log("Get autoresume");
			var dataInicial = DateTime.Now;
			try
			{
				var file = GetFileName(constants);
				if (File.Exists(file))
				{
					string[] txtfile = File.ReadAllLines(file, Encoding.UTF8);
					if (txtfile.Length > 0)
					{
						var resume = JsonConvert.DeserializeObject<ArquivoResume>(txtfile[0]);
						if (resume != null && resume.data > DateTime.MinValue)
							return resume.data;
					}
				}
				AutoResume(dataInicial, constants);
			}
			catch { }
			return dataInicial;
		}

		private static void AutoResume(DateTime Date, Constants constants)
		{
			CoreModel.Log("Save autoresume date: {0}", Date);
			try
			{
				var file = GetFileName(constants);
				if (File.Exists(file))
					File.Delete(file);
				using (StreamWriter _streamWriter = File.AppendText(file))
				{
					_streamWriter.WriteLine(JsonConvert.SerializeObject(new ArquivoResume { data = Date }));
				}
			}
			catch { }
		}

		private static string GetFileName(Constants constants)
		{
			return string.Format("{0}\\{1}", constants.logFolder, _fileResume);
		}

		private class ArquivoResume
		{
			public DateTime data { get; set; }
		}
		#endregion
	}
}
