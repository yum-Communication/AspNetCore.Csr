using System.Net.Mime;
using System.Text;

namespace AspNetCore.Csr;

public class TextResult: EmptyResult {
	public string? Data { get; set; }

	public TextResult() {
		ContentType = "text/html";
	}
}



public class StreamResult: EmptyResult {
	public Stream? Data { get; set; }

	public StreamResult() {
		ContentType = "text/html; charset=UTF-8";
	}
}
