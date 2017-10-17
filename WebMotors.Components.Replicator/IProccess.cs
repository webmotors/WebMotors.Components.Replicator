using Microsoft.Practices.Unity;

namespace WebMotors.Components.Replicator
{
	public interface IProccess
	{
		void Run(IUnityContainer container);
		void Migrate(IUnityContainer container);
	}
}
