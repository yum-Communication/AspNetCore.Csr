using System.Data.Common;

namespace AspNetCore.Csr;

public delegate Task<DbConnection> GetConnection();

public static class DbMapperConfigure {

	private static GetConnection? getConn;

	public static void DbConnector(GetConnection getConnFunc) {
		getConn = getConnFunc;
	}

	public static Task<DbConnection> GetConnection() {
		if(getConn == null) {
			throw new NullReferenceException("DbConnector is not initialized.");
		}
		return getConn.Invoke();
	}
}
