using System.Text.Json;

namespace AspNetCore.Csr; 

public class ApiResult {
	public int Code { get; set; } = 200;
	public IToJsonData? Data { get; set; }
}

public interface IToJsonData {
	void Serialize(Stream s);
}

public interface IFromJsonData {
	abstract static IFromJsonData Deserialize(JsonElement je);
}
