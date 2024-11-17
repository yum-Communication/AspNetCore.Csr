namespace AspNetCore.Csr; 

public static class MapperManager {

	private static List<AddMapper> am;

	static MapperManager() {
		am = new();
	}

	public static void AddMapper(WebApplication app) {
		foreach(var it in am) {
			it.Add(app);
		}
	}

	public static void Registration(AddMapper m) {
		am.Add(m);
	}
}
