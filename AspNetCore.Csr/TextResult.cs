using System.Text;

namespace AspNetCore.Csr;

public class TextResult {
	public int Code { get; set; } = 200;
	public string ContentType = "text/html";
	public string? Data { get; set; }
}



public class StreamResult {
	public int Code { get; set; } = 200;
	public string ContentType = "text/html";
	public Stream? Data { get; set; }
}
