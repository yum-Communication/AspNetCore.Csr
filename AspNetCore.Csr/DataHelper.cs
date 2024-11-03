using System.Data.Common;

namespace AspNetCore.Csr; 

public static class DataHelper {

	public static int GetOrdinal(DbDataReader r, string name) {
		try {
			return r.GetOrdinal(name);
		} catch (IndexOutOfRangeException) {
			return -1;
		}
	}
	public static int GetOrdinal(DbDataReader r, string name, string name2) {
		try {
			return r.GetOrdinal(name);
		} catch (IndexOutOfRangeException) {
			try {
				return r.GetOrdinal(name2);
			} catch (IndexOutOfRangeException) {
				return -1;
			}
		}
	}
	public static int GetOrdinal(DbDataReader r, string name, string name2, string name3) {
		try {
			return r.GetOrdinal(name);
		} catch (IndexOutOfRangeException) {
			try {
				return r.GetOrdinal(name2);
			} catch (IndexOutOfRangeException) {
				try {
					return r.GetOrdinal(name3);
				} catch (IndexOutOfRangeException) {
					return -1;
				}
			}
		}
	}

	public static void AddWithValue(this DbCommand cmd, string name, bool? value) {
		DbParameter param = cmd.CreateParameter();
		param.ParameterName = name;
		param.Value = value == null ? DBNull.Value : value;
		param.DbType = System.Data.DbType.Boolean;
		cmd.Parameters.Add(param);
	}

	public static void AddWithValue(this DbCommand cmd, string name, int? value) {
		DbParameter param = cmd.CreateParameter();
		param.ParameterName = name;
		param.Value = value == null ? DBNull.Value : value;
		param.DbType = System.Data.DbType.Int32;
		cmd.Parameters.Add(param);
	}

	public static void AddWithValue(this DbCommand cmd, string name, long? value) {
		DbParameter param = cmd.CreateParameter();
		param.ParameterName = name;
		param.Value = value == null ? DBNull.Value : value;
		param.DbType = System.Data.DbType.Int64;
		cmd.Parameters.Add(param);
	}

	public static void AddWithValue(this DbCommand cmd, string name, decimal? value) {
		DbParameter param = cmd.CreateParameter();
		param.ParameterName = name;
		param.Value = value == null ? DBNull.Value : value;
		param.DbType = System.Data.DbType.VarNumeric;
		cmd.Parameters.Add(param);
	}

	public static void AddWithValue(this DbCommand cmd, string name, string? value) {
		DbParameter param = cmd.CreateParameter();
		param.ParameterName = name;
		param.Value = value == null ? DBNull.Value : value;
		param.DbType = System.Data.DbType.String;
		cmd.Parameters.Add(param);
	}

	public static void AddWithValue(this DbCommand cmd, string name, DateTime? value) {
		DbParameter param = cmd.CreateParameter();
		param.ParameterName = name;
		param.Value = value == null ? DBNull.Value : value;
		param.DbType = System.Data.DbType.DateTime;
		cmd.Parameters.Add(param);
	}

	public static void AddWithValue(this DbCommand cmd, string name, Guid? value) {
		DbParameter param = cmd.CreateParameter();
		param.ParameterName = name;
		param.Value = value == null ? DBNull.Value : value;
		param.DbType = System.Data.DbType.Guid;
		cmd.Parameters.Add(param);
	}
}
