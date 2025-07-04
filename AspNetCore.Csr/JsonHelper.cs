﻿using System.Text;
using System.Text.Json;

namespace AspNetCore.Csr;

public static class JsonHelper {
	public static int ReadInt32(this JsonElement je, string key, int defValue) {
		if (je.TryGetProperty(key, out var val)) {

			if (val.TryGetInt32(out var result)) {
				return result;
			}
		}
		return defValue;
	}

	public static int? ReadInt32(this JsonElement je, string key) {
		if (je.TryGetProperty(key, out var val)) {

			if (val.TryGetInt32(out var result)) {
				return result;
			}
		}
		return null;
	}

	public static long? ReadInt64(this JsonElement je, string key) {
		if (je.TryGetProperty(key, out var val)) {

			if (val.TryGetInt64(out var result)) {
				return result;
			}
		}
		return null;
	}

	public static double? ReadDouble(this JsonElement je, string key) {
		if (je.TryGetProperty(key, out var val)) {

			if (val.TryGetDouble(out var result)) {
				return result;
			}
		}
		return null;
	}

	public static decimal? ReadDecimal(this JsonElement je, string key) {
		if (je.TryGetProperty(key, out var val)) {

			if (val.TryGetDecimal(out var result)) {
				return result;
			}
		}
		return null;
	}


	public static string ReadString(this JsonElement je, string key, string defValue) {
		if (je.TryGetProperty(key, out var val)) {
			string? result = val.GetString();
			if (result != null) {
				return result;
			}
		}
		return defValue;
	}

	public static string? ReadString(this JsonElement je, string key) {
		if (je.TryGetProperty(key, out var val)) {
			string? result = val.GetString();
			if (result != null) {
				return result;
			}
		}
		return null;
	}

	public static bool ReadBool(this JsonElement je, string key, bool defValue) {
		if (je.TryGetProperty(key, out var val)) {

			return val.GetBoolean();
		}
		return defValue;
	}

	public static bool? ReadBool(this JsonElement je, string key) {
		if (je.TryGetProperty(key, out var val)) {

			return val.GetBoolean();
		}
		return null;
	}

	public static DateTime ReadDateTime(this JsonElement je, string key, DateTime defValue) {
		if (je.TryGetProperty(key, out var val)) {

			if (val.TryGetDateTime(out var result)) {
				return result;
			}
			string? str = val.GetString();
			if (str != null) {
				if (DateTime.TryParse(str, out var dt)) {
					return dt;
				}
			}
		}
		return defValue;
	}

	public static DateTime? ReadDateTime(this JsonElement je, string key) {
		if (je.TryGetProperty(key, out var val)) {

			if (val.TryGetDateTime(out var result)) {
				return result;
			}
			string? str = val.GetString();
			if (str != null) {
				if (DateTime.TryParse(str, out var dt)) {
					return dt;
				}
			}
		}
		return null;
	}

	public static Guid? ReadGuid(this JsonElement je, string key) {
		if (je.TryGetProperty(key, out var val)) {
			if (val.TryGetGuid(out var result)) {
				return result;
			}
		}
		return null;
	}

	public static List<T>? ReadList<T>(this JsonElement je, string key, FromJsonHandler<T> fromJson) {
		if (je.TryGetProperty(key, out var val)) {
			List<T> ret = new();
			foreach (var it in val.EnumerateArray()) {
				ret.Add(fromJson(it));
			}
			return ret;
		}
		return null;
	}

	public static T? ReadObject<T>(this JsonElement je, string key) where T : IFromJsonData {
		T? obj = default;
		if (je.TryGetProperty(key, out var jc)) {
			obj = (T)T.Deserialize(jc);
		}
		return obj;
	}

	public static string Escape(string str) {
		str = str.Replace("\\", "\\\\");
		str = str.Replace("\"", "\\\"");
		str = str.Replace("\b", "\\b");
		str = str.Replace("\f", "\\f");
		str = str.Replace("\n", "\\n");
		str = str.Replace("\r", "\\r");
		str = str.Replace("\t", "\\t");
		return str;
	}


	public static void Write(Stream s, string str) {
		if (str != null) {
			byte[] b = Encoding.UTF8.GetBytes(str);
			s.Write(b, 0, b.Length);
		}
	}


	public static void Write(Stream s, char c) {
		byte[] b = Encoding.UTF8.GetBytes([c]);
		s.Write(b, 0, b.Length);
	}


	public static void WritePrimitive(Stream s, ValueType? p) {
		if (p != null) {
			string? str = p.ToString();
			if (str != null && str.Length > 0) {
				byte[] b = Encoding.UTF8.GetBytes(str);
				s.Write(b, 0, b.Length);
			}
		}
	}


	public static void WriteValue(Stream s, string? str) {
		if (str != null) {
			byte[] b = Encoding.UTF8.GetBytes(Escape(str));
			s.Write(strQuote, 0, 1);
			s.Write(b, 0, b.Length);
			s.Write(strQuote, 0, 1);
		}
	}

	public static void WriteValue(Stream s, char? c) {
		if (c != null) {
			byte[] b = Encoding.UTF8.GetBytes(Escape(c.Value.ToString()!));
			s.Write(strQuote, 0, 1);
			s.Write(b, 0, b.Length);
			s.Write(strQuote, 0, 1);
		}
	}

	public static void WriteValue(Stream s, char c) {
		byte[] b = Encoding.UTF8.GetBytes(Escape(c.ToString()));
		s.Write(strQuote, 0, 1);
		s.Write(b, 0, b.Length);
		s.Write(strQuote, 0, 1);
	}

	private static readonly byte[] strQuote = [0x22];
	private static readonly byte[] strTrue = [0x74, 0x72, 0x75, 0x65];
	private static readonly byte[] strFalse = [0x66, 0x61, 0x6C, 0x73, 0x65];

	public static void WriteValue(Stream s, bool? v) {
		if (v != null) {
			byte[] b = v.Value ? strTrue : strFalse;
			s.Write(b, 0, b.Length);
		}
	}

	public static void WriteValue(Stream s, bool v) {
		byte[] b = v ? strTrue : strFalse;
		s.Write(b, 0, b.Length);
	}

	public static void WriteValue(Stream s, int? v) {
		if (v != null) {
			byte[] b = Encoding.UTF8.GetBytes(v.Value.ToString()!);
			s.Write(b, 0, b.Length);
		}
	}

	public static void WriteValue(Stream s, int v) {
		byte[] b = Encoding.UTF8.GetBytes(v.ToString());
		s.Write(b, 0, b.Length);
	}

	public static void WriteValue(Stream s, long? v) {
		if (v != null) {
			byte[] b = Encoding.UTF8.GetBytes(v.Value.ToString()!);
			s.Write(b, 0, b.Length);
		}
	}

	public static void WriteValue(Stream s, long v) {
		byte[] b = Encoding.UTF8.GetBytes(v.ToString());
		s.Write(b, 0, b.Length);
	}

	public static void WriteValue(Stream s, float? v) {
		if (v != null) {
			byte[] b = Encoding.UTF8.GetBytes(v.Value.ToString()!);
			s.Write(b, 0, b.Length);
		}
	}

	public static void WriteValue(Stream s, float v) {
		byte[] b = Encoding.UTF8.GetBytes(v.ToString());
		s.Write(b, 0, b.Length);
	}

	public static void WriteValue(Stream s, double? v) {
		if (v != null) {
			byte[] b = Encoding.UTF8.GetBytes(v.Value.ToString()!);
			s.Write(b, 0, b.Length);
		}
	}

	public static void WriteValue(Stream s, double v) {
		byte[] b = Encoding.UTF8.GetBytes(v.ToString());
		s.Write(b, 0, b.Length);
	}

	public static void WriteValue(Stream s, decimal? v) {
		if (v != null) {
			byte[] b = Encoding.UTF8.GetBytes(v.Value.ToString()!);
			s.Write(b, 0, b.Length);
		}
	}

	public static void WriteValue(Stream s, decimal v) {
		byte[] b = Encoding.UTF8.GetBytes(v.ToString());
		s.Write(b, 0, b.Length);
	}

	public static void WriteValue(Stream s, Guid? v) {
		if (v != null) {
			byte[] b = Encoding.UTF8.GetBytes(v.Value.ToString()!);

			s.Write(strQuote, 0, 1);
			s.Write(b, 0, b.Length);
			s.Write(strQuote, 0, 1);
		}
	}

	public static void WriteValue(Stream s, Guid v) {
		byte[] b = Encoding.UTF8.GetBytes(v.ToString());
		s.Write(strQuote, 0, 1);
		s.Write(b, 0, b.Length);
		s.Write(strQuote, 0, 1);
	}

	public static void WriteValue(Stream s, DateTime? v) {
		if (v != null) {
			byte[] b = Encoding.UTF8.GetBytes(v.Value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz"));
			s.Write(strQuote, 0, 1);
			s.Write(b, 0, b.Length);
			s.Write(strQuote, 0, 1);
		}
	}

	public static void WriteValue(Stream s, DateTime v) {
		byte[] b = Encoding.UTF8.GetBytes(v.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz"));
		s.Write(strQuote, 0, 1);
		s.Write(b, 0, b.Length);
		s.Write(strQuote, 0, 1);
	}
}
