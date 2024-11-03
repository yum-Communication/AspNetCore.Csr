using Npgsql;
using NpgsqlTypes;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace AspNetCore.Csr.Npgsql; 

public static class DataHelper {

	public static int GetOrdinal(DbDataReader ddr, string name) {

		NpgsqlDataReader r = (NpgsqlDataReader)ddr;
		try {
			return r.GetOrdinal(name);
		} catch (IndexOutOfRangeException) {
			return -1;
		}
	}

	public static void AddWithValue(this DbCommand dbCmd, string name, bool? value) {
		NpgsqlCommand cmd = (NpgsqlCommand)dbCmd;

		var param = new NpgsqlParameter(name, NpgsqlDbType.Boolean);
		param.Value = value == null ? DBNull.Value : value;
		cmd.Parameters.Add(param);
	}

	public static void AddWithValue(this DbCommand dbCmd, string name, int? value) {
		NpgsqlCommand cmd = (NpgsqlCommand)dbCmd;

		var param = new NpgsqlParameter(name, NpgsqlDbType.Integer);
		param.Value = value == null ? DBNull.Value : value;
		cmd.Parameters.Add(param);
	}

	public static void AddWithValue(this DbCommand dbCmd, string name, long? value) {
		NpgsqlCommand cmd = (NpgsqlCommand)dbCmd;

		var param = new NpgsqlParameter(name, NpgsqlDbType.Bigint);
		param.Value = value == null ? DBNull.Value : value;
		cmd.Parameters.Add(param);
	}

	public static void AddWithValue(this DbCommand dbCmd, string name, decimal? value) {
		NpgsqlCommand cmd = (NpgsqlCommand)dbCmd;

		var param = new NpgsqlParameter(name, NpgsqlDbType.Numeric);
		param.Value = value == null ? DBNull.Value : value;
		cmd.Parameters.Add(param);
	}

	public static void AddWithValue(this DbCommand dbCmd, string name, string? value) {
		NpgsqlCommand cmd = (NpgsqlCommand)dbCmd;

		var param = new NpgsqlParameter(name, NpgsqlDbType.Varchar);
		param.Value = value == null ? DBNull.Value : value;
		cmd.Parameters.Add(param);
	}

	public static void AddWithValue(this DbCommand dbCmd, string name, DateTime? value) {
		NpgsqlCommand cmd = (NpgsqlCommand)dbCmd;

		var param = new NpgsqlParameter(name, NpgsqlDbType.Timestamp);
		param.Value = value == null ? DBNull.Value : value;
		cmd.Parameters.Add(param);
	}

	public static void AddWithValue(this DbCommand dbCmd, string name, Guid? value) {
		NpgsqlCommand cmd = (NpgsqlCommand)dbCmd;

		var param = new NpgsqlParameter(name, NpgsqlDbType.Uuid);
		param.Value = value == null ? DBNull.Value : value;
		cmd.Parameters.Add(param);
	}
}
