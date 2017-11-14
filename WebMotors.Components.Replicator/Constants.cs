using System;
using System.Collections.Generic;

namespace WebMotors.Components.Replicator
{
	public abstract class Constants
	{
		public abstract List<string> models { get; }
		public abstract List<string> custom_models { get; }
		public abstract string connectionStringName { get; }
		public abstract string mysqlProccessUser { get; }
		public abstract string logFolder { get; }
		public abstract string elasticSearchUrl { get; }
		/// <summary>
		/// Send 1000 documents per time, override and change if is necessary to you.
		/// </summary>
		public virtual int limitMigrate { get { return 1000; } }
		/// <summary>
		/// In update querie send 1000 documents per time, override and change if is necessary to you.
		/// </summary>
		public virtual int limitUpdate { get { return 1000; } }
		/// <summary>
		/// Sleep 2 seconds to verify log and send new documents, override and change if is necessary to you.
		/// </summary>
		public virtual int proccessTime { get { return 2000; } }

		internal static bool hasModel(string model, Constants constants)
		{
			if (string.IsNullOrWhiteSpace(model)) return false;
			return constants.models.Contains(model.ToLower());
		}
		internal static bool hasCustomModel(string custom_model, Constants constants)
		{
			if (string.IsNullOrWhiteSpace(custom_model)) return false;
			return constants.custom_models.Contains(custom_model.ToLower());
		}

		internal static List<string> commands = new List<string>() { "insert", "update", "delete" };
		internal static bool hasCommand(string command)
		{
			if (string.IsNullOrWhiteSpace(command)) return false;
			return commands.Contains(command.ToLower());
		}

		internal static Type fieldType(string type)
		{
			if (string.IsNullOrWhiteSpace(type)) return null;
			switch (type.Trim().ToLower())
			{
				case "bit":
					return typeof(bool);
				case "smallint":
					return typeof(Int16);
				case "tinyint":
					return typeof(byte);
				case "int":
					return typeof(Int32);
				case "real":
					return typeof(Int64);
				case "float":
					return typeof(double);
				case "decimal":
					return typeof(decimal);
				case "varchar":
					return typeof(string);
				case "char":
					return typeof(string);
				case "datetime":
					return typeof(DateTime);
				default:
					return null;
			}
		}

		internal static string configuration = "constants";
		internal static string contentType = "application/json";
		internal static string replicator = "replicator";
		internal static string _default = "default";
	}
}
