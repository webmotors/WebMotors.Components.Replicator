using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using WebMotors.Components.Model.Core.Attributes;

namespace WebMotors.Components.Replicator.Tests.Models
{
	public class Client : MysqlModel
	{
		#region [ Properties ]
		[JsonIgnore]
		public override string PKField { get { return "id"; } }
		[JsonIgnore]
		public override object PKValue { get { return Id.ToString(); } }

		[MapperField(Field = "id", Type = DbType.Int64, Procedures = "Get")]
		public long Id { get; set; }
		[MapperField(Field = "email", Type = DbType.Int64, Procedures = "Get")]
		public string Email { get; set; }
		[MapperField(Field = "nome", Type = DbType.Int64, Procedures = "Get")]
		public string Nome { get; set; }
		[MapperField(Field = "data_inclusao", Type = DbType.Int64, Procedures = "Get")]
		public DateTime DataInclusao { get; set; }

		[JsonIgnore]
		public override Dictionary<string, Type> defaultFields
		{
			get
			{
				return new Dictionary<string, Type>() {
					{ "id", typeof(long) },
					{ "email", typeof(string) },
					{ "nome", typeof(string) },
					{ "data_inclusao", typeof(DateTime) }
				};
			}
		}
		#endregion

		#region [ abstract ]
		public override string JsonDocument(Dictionary<string, object> fields)
		{
			return JsonConvert.SerializeObject(this);
		}
		#endregion
	}
}
