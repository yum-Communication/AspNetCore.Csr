using System.Net.Mime;
using System.Text;

namespace AspNetCore.Csr;

public class TextResult: EmptyResult {
	public string? Data { get; set; }

	/// <summary>
	/// コンストラクタ
	/// </summary>
	public TextResult() : base("text/html; charset=UTF-8") {
	}

	public TextResult(string data) : base("text/html; charset=UTF-8") {
		Data = data;
	}
}



public class StreamResult: EmptyResult {
	public Stream? Data { get; set; }

	/// <summary>
	/// コンストラクタ
	/// </summary>
	public StreamResult() : base("text/html; charset=UTF-8") {
	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="data">データ</param>
	public StreamResult(Stream data) : base("text/html; charset=UTF-8") {
		Data = data;
	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="contentType">コンテンツタイプ</param>
	public StreamResult(string contentType) : base(contentType) {
	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="contentType">コンテンツタイプ</param>
	/// <param name="data">データ</param>
	public StreamResult(string contentType, Stream data) : base(contentType) {
		Data = data;
	}
}



public class ByteArrayResult: EmptyResult {
	public byte[]? Data { get; set; }

	/// <summary>
	/// コンストラクタ
	/// </summary>
	public ByteArrayResult() : base("text/html; charset=UTF-8") {
	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="data">データ</param>
	public ByteArrayResult(byte[] data) : base("text/html; charset=UTF-8") {
		Data = data;
	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="contentType">コンテンツタイプ</param>
	public ByteArrayResult(string contentType) : base(contentType) {
	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="contentType">コンテンツタイプ</param>
	/// <param name="data">データ</param>
	public ByteArrayResult(string contentType, byte[] data) : base(contentType) {
		Data = data;
	}
}
