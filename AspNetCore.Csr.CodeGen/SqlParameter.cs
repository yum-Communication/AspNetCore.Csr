using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AspNetCore.Csr.CodeGen;

/// <summary>
/// SQLに埋め込まれたパラメータ
/// </summary>
internal class SqlParameter {
	/// <summary>
	///  出現順番。最終的なSQLには番号で渡す
	/// </summary>
	public int Seq { get; }
	/// <summary>
	/// SQL文字列の位置
	/// </summary>
	public int Index { get; }
	/// <summary>
	/// SQL文字列でのパラメータの文字数
	/// </summary>
	public int Length { get; }
	/// <summary>
	/// SQL文字列に埋め込まれたパラメータ
	/// </summary>
	public string Name { get; }
	public string DbType { get; }

	private static Regex RegexName = new("^[_.0-9A-Za-z]+");
	private static Regex RegexType = new("type=([A-Za-z]+)");

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="str"></param>
	/// <param name="index"></param>
	/// <param name="length"></param>
	public SqlParameter(string str, int index, int length, int seq) {
		Seq = seq;
		Index = index;
		Length = length;

		str = str.Substring(2, str.Length - 3);
		Regex rgxName = RegexName;
		Regex rgxType = RegexType;
		Match m = rgxName.Match(str);
		if (m.Success) {
			Name = m.Groups[0].Value;
		} else {
			Name = string.Empty;
		}

		m = rgxType.Match(str);
		if (m.Success) {
			DbType = "NpgsqlTypes." + ToNpgsqlDbType(m.Groups[1].Value);
		} else {
			DbType = "NpgsqlTypes.NpgsqlDbType.Unknown";
		}
	}

	private static string ToNpgsqlDbType(string dataTypeName) {

		var unqualifiedName = dataTypeName;
		if (dataTypeName.IndexOf(".", StringComparison.Ordinal) is not -1 and var index)
			unqualifiedName = dataTypeName.Substring(0, index);

		return unqualifiedName switch
		{
			// Numeric types
			"int2" => "NpgsqlDbType.Smallint",
			"int4" => "NpgsqlDbType.Integer",
			"int" => "NpgsqlDbType.Integer",
			"integer" => "NpgsqlDbType.Integer",
			"int8" => "NpgsqlDbType.Bigint",
			"bigint" => "NpgsqlDbType.Bigint",
			"float4" => "NpgsqlDbType.Real",
			"real" => "NpgsqlDbType.Real",
			"float8" => "NpgsqlDbType.Double",
			"double" => "NpgsqlDbType.Double",
			"numeric" => "NpgsqlDbType.Numeric",
			"money" => "NpgsqlDbType.Money",

			// Text types
			"char" => "NpgsqlDbType.Char",
			"text" => "NpgsqlDbType.Text",
			"xml" => "NpgsqlDbType.Xml",
			"varchar" => "NpgsqlDbType.Varchar",
			"bpchar" => "NpgsqlDbType.Char",
			"name" => "NpgsqlDbType.Name",
			"refcursor" => "NpgsqlDbType.Refcursor",
			"jsonb" => "NpgsqlDbType.Jsonb",
			"json" => "NpgsqlDbType.Json",
			"jsonpath" => "NpgsqlDbType.JsonPath",

			// Date/time types
			"timestamp" => "NpgsqlDbType.Timestamp",
			"timestamptz" => "NpgsqlDbType.TimestampTz",
			"date" => "NpgsqlDbType.Date",
			"time" => "NpgsqlDbType.Time",
			"timetz" => "NpgsqlDbType.TimeTz",
			"interval" => "NpgsqlDbType.Interval",

			// Network types
			"cidr" => "NpgsqlDbType.Cidr",
			"inet" => "NpgsqlDbType.Inet",
			"macaddr" => "NpgsqlDbType.MacAddr",
			"macaddr8" => "NpgsqlDbType.MacAddr8",

			// Full-text search types
			"tsquery" => "NpgsqlDbType.TsQuery",
			"tsvector" => "NpgsqlDbType.TsVector",

			// Geometry types
			"box" => "NpgsqlDbType.Box",
			"circle" => "NpgsqlDbType.Circle",
			"line" => "NpgsqlDbType.Line",
			"lseg" => "NpgsqlDbType.LSeg",
			"path" => "NpgsqlDbType.Path",
			"point" => "NpgsqlDbType.Point",
			"polygon" => "NpgsqlDbType.Polygon",

			// UInt types
			"oid" => "NpgsqlDbType.Oid",
			"xid" => "NpgsqlDbType.Xid",
			"xid8" => "NpgsqlDbType.Xid8",
			"cid" => "NpgsqlDbType.Cid",
			"regtype" => "NpgsqlDbType.Regtype",
			"regconfig" => "NpgsqlDbType.Regconfig",

			// Misc types
			"bool" => "NpgsqlDbType.Boolean",
			"bytea" => "NpgsqlDbType.Bytea",
			"uuid" => "NpgsqlDbType.Uuid",
			"varbit" => "NpgsqlDbType.Varbit",
			"bit" => "NpgsqlDbType.Bit",

			// Built-in range types
			"int4range" => "NpgsqlDbType.IntegerRange",
			"int8range" => "NpgsqlDbType.BigIntRange",
			"numrange" => "NpgsqlDbType.NumericRange",
			"tsrange" => "NpgsqlDbType.TimestampRange",
			"tstzrange" => "NpgsqlDbType.TimestampTzRange",
			"daterange" => "NpgsqlDbType.DateRange",

			// Built-in multirange types
			"int4multirange" => "NpgsqlDbType.IntegerMultirange",
			"int8multirange" => "NpgsqlDbType.BigIntMultirange",
			"nummultirange" => "NpgsqlDbType.NumericMultirange",
			"tsmultirange" => "NpgsqlDbType.TimestampMultirange",
			"tstzmultirange" => "NpgsqlDbType.TimestampTzMultirange",
			"datemultirange" => "NpgsqlDbType.DateMultirange",

			// Internal types
			"int2vector" => "NpgsqlDbType.Int2Vector",
			"oidvector" => "NpgsqlDbType.Oidvector",
			"pg_lsn" => "NpgsqlDbType.PgLsn",
			"tid" => "NpgsqlDbType.Tid",

			// Plugin types
			"citext" => "NpgsqlDbType.Citext",
			"lquery" => "NpgsqlDbType.LQuery",
			"ltree" => "NpgsqlDbType.LTree",
			"ltxtquery" => "NpgsqlDbType.LTxtQuery",
			"hstore" => "NpgsqlDbType.Hstore",
			"geometry" => "NpgsqlDbType.Geometry",
			"geography" => "NpgsqlDbType.Geography",
			_ => "NpgsqlDbType.Unknown"
		};
	}
}
