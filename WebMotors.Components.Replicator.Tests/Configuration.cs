using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMotors.Components.Replicator.Tests
{
	public class Configuration : Constants
	{
		private List<string> _models = new List<string>() { "advert", "message_type" };
		public override List<string> models
		{
			get { return _models; }
		}

		private List<string> _custom_models = new List<string>() { "client" };
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
}
