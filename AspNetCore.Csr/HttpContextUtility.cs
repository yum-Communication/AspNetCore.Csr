using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AspNetCore.Csr;

public delegate T FromJsonHandler<T>(JsonElement je);

public static class HttpContextUtility {

	/// <summary>
	/// APIの結果をレスポンスに出力する
	/// </summary>
	/// <param name="context"></param>
	/// <param name="result"></param>
	public static async Task Output(HttpContext context, ApiResult? result) {
		if (result != null) {
			var response = context.Response;
			response.StatusCode = result.Code;
			foreach (var it in result.Headers) {
				response.Headers.Add(it);
			}

			if (result.Data != null) {
				MemoryStream ms = new();
				result.Data.Serialize(ms);
				ms.Position = 0;
				int len = (int)ms.Length;
				response.Headers.ContentType = result.ContentType + "; charset=UTF-8";
				response.ContentLength = len;
				byte[] buffer = ms.GetBuffer();
				await response.Body.WriteAsync(buffer, 0, len);

				//await response.Body.WriteAsync(ms.GetBuffer(), 0, len);//, 0, len, CancellationToken.None);
			} else {
				response.ContentLength = 0;
			}
		}
	}

	/// <summary>
	/// HTMLをレスポンスに出力する
	/// </summary>
	/// <param name="context"></param>
	/// <param name="result"></param>
	public static async Task Output(HttpContext context, TextResult? result) {
		if (result != null) {
			var response = context.Response;
			response.StatusCode = result.Code;
			foreach (var it in result.Headers) {
				response.Headers.Add(it);
			}

			if (result.Data != null) {
				var bytes = Encoding.UTF8.GetBytes(result.Data);
				response.Headers.ContentType = result.ContentType + "; charset=UTF-8";
				response.ContentLength = bytes.Length;
				await response.Body.WriteAsync(bytes, 0, bytes.Length, CancellationToken.None);
			} else {
				response.ContentLength = 0;
			}
		}
	}

	/// <summary>
	/// ストリームをレスポンスに出力する
	/// </summary>
	/// <param name="context"></param>
	/// <param name="result"></param>
	public static async Task Output(HttpContext context, StreamResult? result) {
		if (result != null) {
			var response = context.Response;
			response.StatusCode = result.Code;
			foreach (var it in result.Headers) {
				response.Headers.Add(it);
			}

			if (result.Data != null) {
				Stream s = result.Data;
				if (s.CanSeek) {
					s.Position = 0;
				}
				response.Headers.ContentType = result.ContentType;
				response.ContentLength = s.Length;
				await s.CopyToAsync(response.Body);

				s.Close();
				s.Dispose();
			} else {
				response.ContentLength = 0;
			}
		}
	}

	public static async Task<T> ReadBody<T>(HttpContext context) where T : IFromJsonData {
		var body = context.Request.Body;
		var doc = await JsonDocument.ParseAsync(body);
		T obj = (T)T.Deserialize(doc.RootElement);
		return obj;
	}

	public static string? GetRequestHeader(this HttpContext context, string key) {
		var hdr = context.Request.Headers[key];
		if (hdr.Count > 0) {
			return hdr[0];
		}
		return null;
	}

	//public static List<string> GetRequestHeaderList(this HttpContext context, string key) {
	//	List<string> ret = new();
	//	var hdr = context.Request.Headers[key];
	//	if (hdr.Count > 1) {
	//		return hdr[0];
	//	}else if (hdr.Count > 0) {
	//		return hdr[0];
	//	}
	//	return ret;
	//}

	public static string? GetRequestQuery(this HttpContext context, string key) {
		var hdr = context.Request.Query[key];
		if (hdr.Count > 0) {
			return hdr[0];
		}
		return null;
	}


	public static int ToIntNotNull(this StringValues s, string name) {
		if (s.Count == 0) { throw new ArgumentNullException(name + " is NOT NULL"); }
		if (int.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required integer value. :'" + s[0] + "'");
	}

	public static long ToLongNotNull(this StringValues s, string name) {
		if (s.Count == 0) { throw new ArgumentNullException(name + " is NOT NULL"); }
		if (long.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required long integer value. :'" + s[0] + "'");
	}

	public static float ToFloatNotNull(this StringValues s, string name) {
		if (s.Count == 0) { throw new ArgumentNullException(name + " is NOT NULL"); }
		if (float.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required float value. :'" + s[0] + "'");
	}

	public static double ToDoubleNotNull(this StringValues s, string name) {
		if (s.Count == 0) { throw new ArgumentNullException(name + " is NOT NULL"); }
		if (double.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required double value. :'" + s[0] + "'");
	}

	public static decimal ToDecimalNotNull(this StringValues s, string name) {
		if (s.Count == 0) { throw new ArgumentNullException(name + " is NOT NULL"); }
		if (decimal.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required decimal value. :'" + s[0] + "'");
	}

	public static bool ToBoolNotNull(this StringValues s, string name) {
		if (s.Count == 0) { throw new ArgumentNullException(name + " is NOT NULL"); }
		if (bool.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required boolean value. :'" + s[0] + "'");
	}

	public static DateTime ToDateTimeNotNull(this StringValues s, string name) {
		if (s.Count == 0) { throw new ArgumentNullException(name + " is NOT NULL"); }
		if (DateTime.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required DateTime value. :'" + s[0] + "'");
	}

	public static Guid ToGuidNotNull(this StringValues s, string name) {
		if (s.Count == 0) { throw new ArgumentNullException(name + " is NOT NULL"); }
		if (Guid.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required GUID value. :'" + s[0] + "'");
	}

	public static string ToStringNotNull(this StringValues s, string name) {
		if (s.Count == 0) { throw new ArgumentNullException(name + " is NOT NULL"); }
		return s[0] ?? string.Empty;
	}



	public static int? ToInt(this StringValues s, string name) {
		if (s.Count == 0) { return null; }
		if (int.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required integer value. :'" + s[0] + "'");
	}

	public static long? ToLong(this StringValues s, string name) {
		if (s.Count == 0) { return null; }
		if (long.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required long integer value. :'" + s[0] + "'");
	}

	public static float? ToFloat(this StringValues s, string name) {
		if (s.Count == 0) { return null; }
		if (float.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required float value. :'" + s[0] + "'");
	}

	public static double? ToDouble(this StringValues s, string name) {
		if (s.Count == 0) { return null; }
		if (double.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required double value. :'" + s[0] + "'");
	}

	public static decimal? ToDecimal(this StringValues s, string name) {
		if (s.Count == 0) { return null; }
		if (decimal.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required decimal value. :'" + s[0] + "'");
	}

	public static bool? ToBool(this StringValues s, string name) {
		if (s.Count == 0) { return null; }
		if (bool.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required boolean value. :'" + s[0] + "'");
	}

	public static DateTime? ToDateTime(this StringValues s, string name) {
		if (s.Count == 0) { return null; }
		if (DateTime.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required DateTime value. :'" + s[0] + "'");
	}

	public static Guid? ToGuid(this StringValues s, string name) {
		if (s.Count == 0) { return null; }
		if (Guid.TryParse(s[0], out var ret)) {
			return ret;
		}
		throw new ArgumentException(name + " required GUID value. :'" + s[0] + "'");
	}

	public static string? ToString(this StringValues s, string name) {
		if (s.Count == 0) { return null; }
		return s[0];
	}



	public static int[] ToIntArray(this StringValues s, string name) {
		if (s.Count == 0) { return []; }
		int z = s.Count;
		int[] ret = new int[z];
		for (int i = 0; i < z; ++z) {
			if (!int.TryParse(s[i], out ret[i])) {
				throw new ArgumentException(name + " required integer value. :'" + s[i] + "'");
			}
		}
		return ret;
	}

	public static long[] ToLongArray(this StringValues s, string name) {
		if (s.Count == 0) { return []; }
		int z = s.Count;
		long[] ret = new long[z];
		for (int i = 0; i < z; ++z) {
			if (!long.TryParse(s[i], out ret[i])) {
				throw new ArgumentException(name + " required long value. :'" + s[i] + "'");
			}
		}
		return ret;
	}

	public static float[] ToFloatArray(this StringValues s, string name) {
		if (s.Count == 0) { return []; }
		int z = s.Count;
		float[] ret = new float[z];
		for (int i = 0; i < z; ++z) {
			if (!float.TryParse(s[i], out ret[i])) {
				throw new ArgumentException(name + " required float value. :'" + s[i] + "'");
			}
		}
		return ret;
	}

	public static double[] ToDoubleArray(this StringValues s, string name) {
		if (s.Count == 0) { return []; }
		int z = s.Count;
		double[] ret = new double[z];
		for (int i = 0; i < z; ++z) {
			if (!double.TryParse(s[i], out ret[i])) {
				throw new ArgumentException(name + " required double value. :'" + s[i] + "'");
			}
		}
		return ret;
	}

	public static decimal[] ToDecimalArray(this StringValues s, string name) {
		if (s.Count == 0) { return []; }
		int z = s.Count;
		decimal[] ret = new decimal[z];
		for (int i = 0; i < z; ++z) {
			if (!decimal.TryParse(s[i], out ret[i])) {
				throw new ArgumentException(name + " required decimal value. :'" + s[i] + "'");
			}
		}
		return ret;
	}

	public static bool[] ToBoolArray(this StringValues s, string name) {
		if (s.Count == 0) { return []; }
		int z = s.Count;
		bool[] ret = new bool[z];
		for (int i = 0; i < z; ++z) {
			if (!bool.TryParse(s[i], out ret[i])) {
				throw new ArgumentException(name + " required bool value. :'" + s[i] + "'");
			}
		}
		return ret;
	}

	public static DateTime[] ToDateTimeArray(this StringValues s, string name) {
		if (s.Count == 0) { return []; }
		int z = s.Count;
		DateTime[] ret = new DateTime[z];
		for (int i = 0; i < z; ++z) {
			if (!DateTime.TryParse(s[i], out ret[i])) {
				throw new ArgumentException(name + " required DateTime value. :'" + s[i] + "'");
			}
		}
		return ret;
	}

	public static Guid[] ToGuidArray(this StringValues s, string name) {
		int z = s.Count;
		Guid[] ret = new Guid[z];
		for (int i = 0; i < z; ++z) {
			if (!Guid.TryParse(s[i], out ret[i])) {
				throw new ArgumentException(name + " required Guid value. :'" + s[i] + "'");
			}
		}
		return ret;
	}

	public static string[] ToStringArray(this StringValues s, string name) {
		int z = s.Count;
		string[] ret = new string[z];
		for (int i = 0; i < z; ++z) {
			ret[i] = s[i] ?? string.Empty;
		}
		return ret;
	}



	public static int ToIntNotNull(this string? s, string name) {
		if (s == null) { throw new ArgumentNullException(name + " is NOT NULL"); }
		return int.Parse(s);
	}

	public static long ToLongNotNull(this string? s, string name) {
		if (s == null) { throw new ArgumentNullException(name + " is NOT NULL"); }
		return long.Parse(s);
	}

	public static float ToFloatNotNull(this string? s, string name) {
		if (s == null) { throw new ArgumentNullException(name + " is NOT NULL"); }
		return float.Parse(s);
	}

	public static double ToDoubleNotNull(this string? s, string name) {
		if (s == null) { throw new ArgumentNullException(name + " is NOT NULL"); }
		return double.Parse(s);
	}

	public static decimal ToDecimalNotNull(this string? s, string name) {
		if (s == null) { throw new ArgumentNullException(name + " is NOT NULL"); }
		return decimal.Parse(s);
	}

	public static bool ToBoolNotNull(this string? s, string name) {
		if (s == null) { throw new ArgumentNullException(name + " is NOT NULL"); }
		return bool.Parse(s);
	}

	public static DateTime ToDateTimeNotNull(this string? s, string name) {
		if (s == null) { throw new ArgumentNullException(name + " is NOT NULL"); }
		return DateTime.Parse(s);
	}

	public static Guid ToGuidNotNull(this string? s, string name) {
		if (s == null) { throw new ArgumentNullException(name + " is NOT NULL"); }
		return Guid.Parse(s);
	}

	public static string ToStringNotNull(this string? s, string name) {
		if (s == null) { throw new ArgumentNullException(name + " is NOT NULL"); }
		return s;
	}


	public static int? ToInt(this string? s) {
		return s == null ? null : int.Parse(s);
	}

	public static long? ToLong(this string? s) {
		return s == null ? null : long.Parse(s);
	}

	public static float? ToFloat(this string? s) {
		return s == null ? null : float.Parse(s);
	}

	public static double? ToDouble(this string? s) {
		return s == null ? null : double.Parse(s);
	}

	public static decimal? ToDecimal(this string? s) {
		return s == null ? null : decimal.Parse(s);
	}

	public static bool? ToBool(this string? s) {
		return s == null ? null : bool.Parse(s);
	}

	public static DateTime? ToDateTime(this string? s) {
		return s == null ? null : DateTime.Parse(s);
	}

	public static Guid? ToGuid(this string? s) {
		return s == null ? null : Guid.Parse(s);
	}

	public static string? ToString(this string? s) {
		return s;
	}
}
