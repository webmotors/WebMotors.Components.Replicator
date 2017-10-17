using System.Collections.Generic;
using System.Linq;

namespace WebMotors.Components.Replicator.Implementations.Analizers
{
	public class InsertAnalizer : QueryAnalizer
	{
		#region [ protected ]
		protected override void Resolve(CoreModel model, Constants constants)
		{
			model.insert(QueryFields(model), _container, constants);
		}

		protected override string GetTableName()
		{
			if (string.IsNullOrWhiteSpace(Query)) return string.Empty;
			var table = Query.ToLower().Replace("insert", string.Empty).Replace("into", string.Empty).Replace("`", string.Empty).Replace("'", string.Empty).Trim().Split(' ')[0];
			if (table.IndexOf(".") > -1)
				table = table.Split('.')[table.Split('.').Count() - 1];
			return table;
		}

		protected override Dictionary<string, object> QueryFields(CoreModel model)
		{
			var fields = new Dictionary<string, object>();
			var fields1 = GetFields(Query);
			var fields2 = new List<string>();
			if (Query.IndexOf(')') > -1)
				fields2 = GetFields(Query.Substring(Query.IndexOf(')')));
			//Insert com fields e values
			if (fields2.Count > 0 && fields1.Count == fields2.Count)
			{
				for (var i = 0; i < fields1.Count; i++)
					if (model.defaultFields.ContainsKey(fields1[i].ToLower()))
					{
						var fieldValue = TranformFieldValue(fields1[i], fields2[i], model.defaultFields);
						if (fieldValue != null)
							fields.Add(fields1[i].ToLower(), fieldValue);
					}
			}
			//insert apenas com values
			else
			{
				foreach (var item in model.defaultFields)
					fields2.Add(item.Key);
				for (var i = 0; i < fields2.Count; i++)
				{
					var fieldValue = TranformFieldValue(fields2[i], fields1.Count > i ? fields1[i] : string.Empty, model.defaultFields);
					if (fieldValue != null)
						fields.Add(fields2[i].ToLower(), fieldValue);
				}
			}
			return fields;
		}
		#endregion

		#region [ private ]
		private List<string> GetFields(string _query)
		{
			var retorno = new List<string>();
			if (_query.IndexOf('(') > -1)
			{
				_query = _query.Substring(_query.IndexOf('(') + 1);
				if (_query.IndexOf(')') > -1)
					_query = _query.Substring(0, _query.IndexOf(')'));
				var itens = _query.Split(',');
				foreach (var item in itens)
				{
					var field = item.Replace("'", string.Empty).Replace("`", string.Empty).Trim();
					if (!string.IsNullOrWhiteSpace(field))
						retorno.Add(field);
				}
			}
			return retorno;
		}
		#endregion
	}
}
