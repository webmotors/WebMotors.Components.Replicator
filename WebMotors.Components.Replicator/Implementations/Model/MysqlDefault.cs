using Newtonsoft.Json;
using System.Collections.Generic;
using WebMotors.Components.Model.Core;

namespace WebMotors.Components.Replicator.Implementations.Model
{
	public class MysqlDefault : MysqlModel
	{
		#region [ private ]
		private string sqlPK =
		@"SHOW KEYS FROM {0} WHERE Key_name = 'PRIMARY';";
		private string _pk = null;

		private string FindPK()
		{
			string pk = string.Empty;
			var command = database.CreateCommand(string.Format(sqlPK, table));
			using (var reader = command.ExecuteReader())
			{
				bool first = true;
				while (reader.Read())
				{
					if (first)
					{
						pk = reader.GetReaderValue<string>("Column_name");
						first = false;
					}
					else
						pk = string.Empty;
				}
			}
			return pk;
		} 
		#endregion

		#region [ internal ]
		internal void setPK(string pk)
		{
			this._pk = pk;
		} 
		#endregion

		#region [ public ]
		[JsonIgnore]
		public override object PKValue
		{
			get
			{
				if (fieldsDatabase != null && fieldsDatabase.ContainsKey(PKField)) return fieldsDatabase[PKField];
				throw new KeyNotFoundException("database not found pk");
			}
		}
		[JsonIgnore]
		public override string PKField
		{
			get
			{
				if (_pk == null)
					_pk = FindPK();
				if (!string.IsNullOrWhiteSpace(_pk)) return _pk;
				throw new KeyNotFoundException("table not contains 1 pk");
			}
		} 
		#endregion
	}
}