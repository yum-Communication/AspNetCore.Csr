using AspNetCore.Csr;
using System.Runtime.Serialization;

namespace ApiExample.Models;

[ToJson]
public partial class EmptyResponse {

	/// <summary>
	/// 結果コード
	/// </summary>
	[DataMember(Name = "result")]
	public int Result { get; set; }

	/// <summary>
	/// エラーメッセージ
	/// </summary>
	[DataMember(Name = "message")]
	public string? Message { get; set; }

	/// <summary>
	/// デフォルトコンストラクタ
	/// </summary>
	public EmptyResponse(int code, string message) {
		Result = code;
		Message = message;
	}

	public static IToJsonData? Ok(int code, string message) {
		return (IToJsonData?)new EmptyResponse(code, message);
	}

	public static IToJsonData? Exception(int code, Exception x) {
		return (IToJsonData?)new EmptyResponse(code, x.ToString());
	}
}
