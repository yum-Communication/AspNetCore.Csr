using Microsoft.Extensions.Primitives;
using System.Text.Json;

namespace AspNetCore.Csr;


public class ApiResult: EmptyResult {
	public IToJsonData? Data { get; set; }

	/// <summary>
	/// コンストラクタ
	/// </summary>
	public ApiResult() : base("application/json") {
	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="code">HTTPステータスコード</param>
	public ApiResult(int code) : base(code, "application/json") {
	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="data">データ</param>
	public ApiResult(IToJsonData data) : base("application/json") {
		Data = data;
	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="code">HTTPステータスコード</param>
	/// <param name="data">データ</param>
	public ApiResult(int code, IToJsonData data) : base(code, "application/json") {
		Data = data;
	}
}



public interface IToJsonData {
	void Serialize(Stream s);
}

public interface IFromJsonData {
	abstract static IFromJsonData Deserialize(JsonElement je);
}
