using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;
using System.Configuration;
using System.ServiceProcess;

namespace WebMotors.Components.Replicator.Tests
{
	public class Program : ServiceBase
	{
		static void Main(string[] args)
		{
			if (Environment.UserInteractive)
			{
				if ("Y".Equals(ConfigurationManager.AppSettings["migrate-data"]))
					MigrateData();
				else
					ServiceStart();
			}
			else
				ServiceBase.Run(new Program());
		}

		protected override void OnStart(string[] args)
		{
			ServiceStart();
		}

		protected override void OnStop()
		{
			ServiceStop();
		}

		public static void ServiceStart()
		{
			ReplicatorThread.Start();
			if (Environment.UserInteractive)
			{
				Console.WriteLine("send \"exit\" to quit:");
				while (true)
				{
					var texto = Console.ReadLine();
					if (texto == "exit")
					{
						ReplicatorThread.Stop();
						break;
					}
				}
			}
		}

		public static void ServiceStop()
		{
			ReplicatorThread.Stop();
		}

		private static void MigrateData()
		{
			IUnityContainer container = new UnityContainer();
			container.LoadConfiguration();
			var proccess = container.Resolve<IProccess>(ConfigurationManager.AppSettings["proccess-type"]);
			proccess.Migrate(container);
		}
	}
}
