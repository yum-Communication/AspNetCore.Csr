using AspNetCore.Csr;
using System.Runtime.Serialization;

namespace ApiExample.Models;

[ToJson]
public partial class Response<T> where T: IToJsonData {

	/// <summary>
	/// 結果コード
	/// </summary>
	[DataMember(Name = "result")]
	public int Result { get; set; } = 0;

	/// <summary>
	/// エラーメッセージ
	/// </summary>
	[DataMember(Name = "message")]
	public string? Message { get; set; }


	/// <summary>
	/// 値
	/// </summary>
	[DataMember(Name = "body")]
	public T? Body { get; init; }

	/// <summary>
	/// デフォルトコンストラクタ
	/// </summary>
	public Response() {
		Result = 0;
		Message = null;
		Body = default;
	}

	/// <summary>
	/// OKの場合のコンストラクタ
	/// </summary>
	/// <param name="body"></param>
	public Response(T body) {
		Result = 200;
		Message = "Ok";
		Body = body;
	}
}
