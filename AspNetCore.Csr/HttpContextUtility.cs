using Microsoft.AspNetCore.Mvc.RazorPages;
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

			if (result.Data != null) {
				MemoryStream ms = new();
				result.Data.Serialize(ms);
				int len = (int)ms.Length;
				response.Headers.ContentType = "application/json; charset=UTF-8";
				response.ContentLength = len;
				await response.Body.WriteAsync(ms.GetBuffer());//, 0, len, CancellationToken.None);
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

			if (result.Data != null) {
				Stream s = result.Data;
				response.Headers.ContentType = result.ContentType;
				response.ContentLength = s.Length;
				await s.CopyToAsync(s, 8192, CancellationToken.None);
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

	public static string? GetRequestQuery(this HttpContext context, string key) {
		var hdr = context.Request.Query[key];
		if (hdr.Count > 0) {
			return hdr[0];
		}
		return null;
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

	public static bool? ToBool(this string? s) {
		return s == null ? null : bool.Parse(s);
	}

	public static DateTime? ToDateTime(this string? s) {
		return s == null ? null : DateTime.Parse(s);
	}
}
