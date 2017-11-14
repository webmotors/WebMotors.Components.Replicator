using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using WebMotors.Components.Replicator.Implementations;

namespace WebMotors.Components.Replicator
{
	public static class ReplicatorThread
	{
		private static CancellationTokenSource _cancellationTokenSource = null;
		private static Task _thread = null;

		public static void Start()
		{
			if (_thread == null || _thread.Status == TaskStatus.RanToCompletion)
				Run();
		}

		public static void Stop()
		{
			if (_thread != null && _thread.Status == TaskStatus.Running)
			{
				_cancellationTokenSource.Cancel();
				_thread.GetAwaiter().GetResult();
			}
		}

		public static TaskStatus Status()
		{
			return _thread.Status;
		}

		private static void Run()
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_thread = Task.Run(async () =>
			{
				IUnityContainer container = new UnityContainer();
				container.LoadConfiguration();
				var proccess = container.Resolve<IProccess>(ConfigurationManager.AppSettings["proccess-type"]);
				var constants = container.Resolve<Constants>(Constants.configuration);
				while (!_cancellationTokenSource.IsCancellationRequested)
				{
					proccess.Run(container);
					await Task.Delay(constants.proccessTime);
				}
			});
		}
	}
}
