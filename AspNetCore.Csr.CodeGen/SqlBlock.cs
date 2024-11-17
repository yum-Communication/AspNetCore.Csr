using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AspNetCore.Csr.CodeGen;

internal class SqlBlock {
	public string Condition = string.Empty;
	public string Sql = string.Empty;
	public List<SqlParameter> Params = new();
	private static readonly Regex rgx = new("#if{(.+?)}([\\s\\S\\n\\r]+?)#endif", RegexOptions.Multiline);

	public static List<SqlBlock> SplitSql(string sql) {
		Regex rgxEnd = new("");
		List<SqlBlock> res = new();
		var m = rgx.Match(sql);
		if (m.Success) {
			SqlBlock s = new();
			do {
				// ifの手前をListに追加
				string ss = sql.Substring(0, m.Index);
				if (!string.IsNullOrWhiteSpace(ss)) {
					s.Sql = ss;
					res.Add(s);
					s = new();
				}

				s.Condition = m.Groups[1].Value;
				s.Sql = m.Groups[2].Value;
				res.Add(s);
				s = new();

				if (m.Index + m.Length >= sql.Length) { break; }
				sql = sql.Substring(m.Index + m.Length);
				if (string.IsNullOrWhiteSpace(sql)) { break; }

				m = rgx.Match(sql);
			} while (m.Success);

			if (!string.IsNullOrWhiteSpace(sql)) {
				s.Sql = sql.TrimEnd();
				res.Add(s);
			}

		} else {
			SqlBlock s = new();
			s.Sql = sql;
			res.Add(s);
		}

		return res;
	}
}
