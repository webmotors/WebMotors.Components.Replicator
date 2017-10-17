using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using WebMotors.Components.Model.Core;
using WebMotors.Components.Replicator.Implementations;
using WebMotors.Components.Replicator.Implementations.Model;

namespace WebMotors.Components.Replicator
{
	public class MysqlModel : CoreModel
	{
		#region [ private ]
		private string sqlGet = "SELECT {0} FROM {1} WHERE {2}";
		private string sqlGetLimit = "SELECT {0} FROM {1} {2} ORDER BY {3} LIMIT {4};";
		private string sqlFields =
		@"SELECT
			COLUMN_NAME, DATA_TYPE 
		FROM
			INFORMATION_SCHEMA.COLUMNS 
		WHERE
			TABLE_NAME = @table;";
		private Dictionary<string, Type> _defaultFields = null;
		private object ReplicateItem(DbDataReader reader, IUnityContainer container, Constants constants)
		{
			var fields = new Dictionary<string, object>();
			int numCampos = reader.FieldCount;
			for (int i = 0; i < numCampos; i++)
				fields.Add(reader.GetName(i).ToLower(), reader.GetValue(i));
			if (fields.Count > 0)
			{
				var replicator = container.Resolve<IReplicator>(Constants.replicator);
				if (this is MysqlDefault)
					replicator.insert(fields[PKField], table, JsonDocument(fields), constants);
				else
				{
					CoreModel instance = (CoreModel)Activator.CreateInstance(this.GetType());
					instance.Fill(fields);
					replicator.insert(fields[PKField], table, instance.JsonDocument(fields), constants);
				}
			}
			return fields[PKField];
		}
		#endregion

		#region [ override ]
		[JsonIgnore]
		public override object PKValue
		{
			get { throw new NotImplementedException(); }
		}
		[JsonIgnore]
		public override string PKField
		{
			get { throw new NotImplementedException(); }
		}

		[JsonIgnore]
		public override Dictionary<string, Type> defaultFields
		{
			get
			{
				if (_defaultFields == null)
				{
					_defaultFields = new Dictionary<string, Type>();
					var command = database.CreateCommand(sqlFields);
					command.Parameters.Add(database.CreateParameter("table", table));
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var fieldType = Constants.fieldType(reader.GetReaderValue<string>("DATA_TYPE"));
							if (fieldType != null)
								_defaultFields.Add(reader.GetReaderValue<string>("COLUMN_NAME"), fieldType);
						}
					}
				}
				return _defaultFields;
			}
		}

		protected override void FillInsert(Dictionary<string, object> fields)
		{
			if (fieldsDatabase != null) return;
			if (fields == null || fields.Count == 0) return;
			var queryFields = string.Join(",", (from f in defaultFields select f.Key).ToList());
			var whereFields = string.Join(" AND ", (from f in fields select string.Format("{0} = @{0}", f.Key)).ToList());
			var fieldsDB = new Dictionary<string, object>();
			var command = database.CreateCommand(string.Format(sqlGet, queryFields, table, whereFields));
			foreach (var field in fields)
				command.Parameters.Add(database.CreateParameter(field.Key, field.Value));
			using (var reader = command.ExecuteReader())
			{
				bool first = true;
				while (reader.Read())
				{
					if (first)
					{
						int numCampos = reader.FieldCount;
						for (int i = 0; i < numCampos; i++)
							fieldsDB.Add(reader.GetName(i).ToLower(), reader.GetValue(i));
						first = false;
					}
					//insert resolve only 1 register
					else
						fieldsDB = new Dictionary<string, object>();
				}
			}
			this.Fill(fieldsDB);
		}

		protected override List<CoreModel> FillUpdate(Dictionary<string, object> fields)
		{
			if (fields == null) fields = new Dictionary<string, object>();
			List<CoreModel> list = new List<CoreModel>();
			object pkvalue = null;
			while (true)
			{
				var queryFields = string.Join(",", (from f in defaultFields select f.Key).ToList());
				var whereFields = fields.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", (from f in fields select string.Format("{0} = @{0}", f.Key)).ToList());
				if (pkvalue != null) whereFields = string.Format("{0} {1} {2} > @{3}", whereFields, string.IsNullOrWhiteSpace(whereFields) ? "WHERE" : "AND", PKField);
				var command = database.CreateCommand(string.Format(sqlGetLimit, queryFields, table, whereFields, PKField, Constants.limitUpdate));
				foreach (var field in fields)
					command.Parameters.Add(database.CreateParameter(field.Key, field.Value));
				if (pkvalue != null) command.Parameters.Add(database.CreateParameter(PKField, pkvalue));
				using (var reader = command.ExecuteReader())
				{
					int count = 0;
					while (reader.Read())
					{
						count++;
						var fieldsDB = new Dictionary<string, object>();
						int numCampos = reader.FieldCount;
						for (int i = 0; i < numCampos; i++)
							fieldsDB.Add(reader.GetName(i).ToLower(), reader.GetValue(i));
						CoreModel model = (CoreModel)Activator.CreateInstance(this.GetType());
						if (model is MysqlDefault)
							((MysqlDefault)model).setPK(this.PKField);
						model.table = this.table;
						model.Fill(fieldsDB);
						list.Add(model);
						pkvalue = model.PKValue;
						if (pkvalue == null)
							throw new KeyNotFoundException("pk not fill on update");
					}
					if (count == 0 || count != Constants.limitUpdate) break;
				}
			}
			return list;
		}

		protected override bool FillDelete(Dictionary<string, object> fields)
		{
			if (fields == null || fields.Count != 1 || !fields.ContainsKey(PKField)) return false;
			var queryFields = string.Join(",", (from f in defaultFields select f.Key).ToList());
			var whereFields = string.Join(" AND ", (from f in fields select string.Format("{0} = @{0}", f.Key)).ToList());
			var command = database.CreateCommand(string.Format(sqlGet, queryFields, table, whereFields));
			foreach (var field in fields)
				command.Parameters.Add(database.CreateParameter(field.Key, field.Value));
			using (var reader = command.ExecuteReader())
			{
				return !reader.HasRows;
			}
		}

		public override void Migrate(IUnityContainer container, Constants constants)
		{
			object pkvalue = null;
			while (true)
			{
				var queryFields = string.Join(",", (from f in defaultFields select f.Key).ToList());
				var whereFields = pkvalue != null ? string.Format("WHERE {0} > @{0}", PKField) : string.Empty;
				var command = database.CreateCommand(string.Format(sqlGetLimit, queryFields, table, whereFields, PKField, Constants.limitMigrate));
				if (pkvalue != null)
					command.Parameters.Add(database.CreateParameter(PKField, pkvalue));
				using (var reader = command.ExecuteReader())
				{
					int count = 0;
					while (reader.Read())
					{
						pkvalue = ReplicateItem(reader, container, constants);
					}
					if (count == 0 || count != Constants.limitMigrate) break;
				}
			}
		}
		#endregion
	}
}
