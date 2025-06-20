using Microsoft.Extensions.Primitives;

namespace AspNetCore.Csr;

public class EmptyResult {
	public int Code { get; set; }
	public Dictionary<string, StringValues> Headers { get; }
	public string ContentType { get; set; }

	/// <summary>
	/// コンストラクタ
	/// </summary>
	public EmptyResult() {
		Code = 200;
		Headers = new();
		ContentType = string.Empty;
	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="code">HTTPステータスコード</param>
	public EmptyResult(int code) {
		Code = code;
		Headers = new();
		ContentType = string.Empty;
	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="contentType">コンテンツタイプ</param>
	public EmptyResult(string contentType) {
		Code = 200;
		Headers = new();
		ContentType = contentType;
	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="code">HTTPステータスコード</param>
	/// <param name="contentType">コンテンツタイプ</param>
	public EmptyResult(int code, string contentType) {
		Code = code;
		Headers = new();
		ContentType = contentType;
	}
}
