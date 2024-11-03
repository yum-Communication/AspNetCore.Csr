using AspNetCore.Csr;
using System.Runtime.Serialization;

namespace ApiExample.Models;

[ToJson]
public partial class Response {
	[DataMember(Name = "result")]
	public int Result { get; set; }

	[DataMember(Name = "body")]
	public User? User { get; set; }
}
