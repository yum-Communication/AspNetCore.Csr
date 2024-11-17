using Microsoft.Extensions.Primitives;
using System.Text.Json;

namespace AspNetCore.Csr; 

public class EmptyResult {
	public int Code { get; set; } = 200;
	public Dictionary<string, StringValues> Headers = new();
	public string ContentType = string.Empty;
}

public class ApiResult : EmptyResult {
	public IToJsonData? Data { get; set; }

	public ApiResult() {
		ContentType = "application/json";
	}
}

public interface IToJsonData {
	void Serialize(Stream s);
}

public interface IFromJsonData {
	abstract static IFromJsonData Deserialize(JsonElement je);
}
