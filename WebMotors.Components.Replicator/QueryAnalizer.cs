using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using WebMotors.Components.Model.Core;
using WebMotors.Components.Replicator.Implementations;

namespace WebMotors.Components.Replicator
{
	public abstract class QueryAnalizer
	{
		#region [ Properties ]
		protected IUnityContainer _container = null;
		protected string Query { private set; get; }
		public Database database { set; get; }
		#endregion

		#region [ public ]
		public void Resolve(IUnityContainer container, string query, Database database, Constants constants)
		{
			this._container = container;
			this.Query = query;
			this.database = database;
			var model = GetModel(container, query, database, constants);
			if (model != null)
				Resolve(model, constants);
		}
		#endregion

		#region [ protected ]
		protected abstract void Resolve(CoreModel model, Constants constants);
		protected abstract string GetTableName();
		protected abstract Dictionary<string, object> QueryFields(CoreModel model);

		protected object TranformFieldValue(string field, string queryValue, Dictionary<string, Type> defaultFields)
		{
			try
			{
				if (defaultFields.ContainsKey(field))
				{
					return Convert.ChangeType(queryValue, defaultFields[field]);
				}
			}
			catch { }
			return null;
		}
		#endregion

		#region [ private ]
		private CoreModel GetModel(IUnityContainer container, string query, Database database, Constants constants)
		{
			CoreModel model = null;
			var table = GetTableName();

			if (Constants.hasModel(table, constants))
				model = container.Resolve<CoreModel>(Constants._default);
			else if (Constants.hasCustomModel(table, constants))
				model = container.Resolve<CoreModel>(table);

			if (model != null)
			{
				model.table = table;
				model.database = database;
			}
			return model;
		}
		#endregion
	}
}
