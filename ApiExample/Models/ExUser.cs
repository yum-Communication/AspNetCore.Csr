using AspNetCore.Csr;

namespace ApiExample.Models;

[ToJson]
[FromJson]
public partial class ExUser : User{
	public string Shine { get; set; } = string.Empty;
	public string Buta { get; set; } = string.Empty;
}
