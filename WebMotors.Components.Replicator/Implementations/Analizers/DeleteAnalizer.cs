using System;
using System.Collections.Generic;
using System.Linq;

namespace WebMotors.Components.Replicator.Implementations.Analizers
{
	public class DeleteAnalizer : QueryAnalizer
	{
		#region [ protected ]
		protected override void Resolve(CoreModel model, Constants constants)
		{
			model.delete(QueryFields(model), _container, constants);
		}

		protected override string GetTableName()
		{
			if (string.IsNullOrWhiteSpace(Query) || Query.ToLower().Contains(" join ")) return string.Empty;
			var table = Query.ToLower().Replace("delete", string.Empty).Replace("from", string.Empty).Replace("`", string.Empty).Replace("'", string.Empty).Trim().Split(' ')[0];
			if (table.IndexOf(".") > -1)
				table = table.Split('.')[table.Split('.').Count() - 1];
			return table;
		}

		protected override Dictionary<string, object> QueryFields(CoreModel model)
		{
			if (!string.IsNullOrWhiteSpace(Query) && Query.ToLower().Contains("where"))
			{
				var list = new Dictionary<string, object>();
				var queryItens = Query.Substring(Query.IndexOf("where")).Replace("where", string.Empty).Split(new string[] { "AND" }, StringSplitOptions.RemoveEmptyEntries);
				foreach (var item in queryItens)
				{
					var itemField = item.Split('=');
					if (itemField.Length == 2 && model.defaultFields.ContainsKey(itemField[0].Trim().ToLower()))
					{
						var valueField = TranformFieldValue(itemField[0].Trim(), itemField[1].Trim(), model.defaultFields);
						if (valueField != null)
							list.Add(itemField[0].Trim(), valueField);
					}
				}
				return list;
			}
			return null;
		}
		#endregion
	}
}
