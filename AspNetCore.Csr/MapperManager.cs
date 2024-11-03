namespace AspNetCore.Csr; 

public static class MapperManager {
	public static int Dummy;
	private static List<AddMapper> am;

	static MapperManager() {
		Dummy = 0;
		am = new();
	}

	public static void Initialize() {
		Dummy = 1;
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
