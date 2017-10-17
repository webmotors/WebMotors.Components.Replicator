namespace WebMotors.Components.Replicator
{
	public interface IReplicator
	{
		void insert<T>(T pk, string table, string json, Constants constants);
		void update<T>(T pk, string table, string json, Constants constants);
		void delete<T>(T pk, string table, Constants constants);
	}
}
