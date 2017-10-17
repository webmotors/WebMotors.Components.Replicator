using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WebMotors.Components.Model.Core;
using WebMotors.Components.Replicator.Implementations;

namespace WebMotors.Components.Replicator
{
	public abstract class CoreModel : BaseModel<string>
	{
		#region [ properties ]
		internal string table { get; set; }
		internal Database database { get; set; }
		public abstract string PKField { get; }
		public abstract object PKValue { get; }
		public abstract Dictionary<string, Type> defaultFields { get; }
		protected Dictionary<string, object> fieldsDatabase { get; private set; }
		#endregion

		#region [ abstract ]
		protected abstract void FillInsert(Dictionary<string, object> fields);
		protected abstract List<CoreModel> FillUpdate(Dictionary<string, object> fields);
		protected abstract bool FillDelete(Dictionary<string, object> fields);
		public abstract void Migrate(IUnityContainer container, Constants constants);
		#endregion

		#region [ protected methods ]
		public virtual string JsonDocument(Dictionary<string, object> fields)
		{
			return string.Concat(
				"{ ",
				string.Join(", ",
				(from f in fields
				 select string.Format("{0}{1}{0}: {2}{3}{2}",
					 "\"",
					 f.Key,
					 json_scape_coma_type(f.Value),
					 json_value_object(f.Value)
					 )).ToList()
				), " }"
			);
		}
		#endregion

		#region [ internal ]
		internal void Fill(Dictionary<string, object> fields)
		{
			this.FillEntity(fields);
			this.fieldsDatabase = fields;
		}
		#endregion

		#region [ public methods ]
		public virtual void insert(Dictionary<string, object> fields, IUnityContainer container, Constants constants)
		{
			FillInsert(fields);
			//has database register
			if (fieldsDatabase.Count > 0)
			{
				var replicator = container.Resolve<IReplicator>(Constants.replicator);
				replicator.insert(PKValue, table, JsonDocument(fieldsDatabase), constants);
			}
		}

		public virtual void update(Dictionary<string, object> fields, IUnityContainer container, Constants constants)
		{
			var models = FillUpdate(fields);
			foreach (var model in models)
			{
				var replicator = container.Resolve<IReplicator>(Constants.replicator);
				replicator.update(model.PKValue, table, model.JsonDocument(model.fieldsDatabase), constants);
			}
		}

		public virtual void delete(Dictionary<string, object> fields, IUnityContainer container, Constants constants)
		{
			//database register was deleted
			if (FillDelete(fields))
			{
				var replicator = container.Resolve<IReplicator>(Constants.replicator);
				replicator.delete(fields[PKField], table, constants);
			}
		}
		#endregion

		#region [ private methods ]
		public static void Log(string message, params object[] parameters)
		{
			if (parameters != null && parameters.Length > 0)
				Console.WriteLine(message, parameters);
			else
				Console.WriteLine(message);
		}

		public static void LogException(Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}

		private string json_value_object(object value)
		{
			if (value == null) return string.Empty;
			if (value.GetType().Equals(typeof(bool)))
				return Convert.ToBoolean(value) ? "true" : "false";
			return value.ToString();
		}

		private string json_scape_coma_type(object value)
		{
			if (value == null) return "\"";
			if (value.GetType().Equals(typeof(string)))
				return "\"";
			if (value.GetType().Equals(typeof(DateTime)))
				return "\"";
			return string.Empty;
		}
		#endregion
	}
}
