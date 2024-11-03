using AspNetCore.Csr;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

namespace ApiExample.Models;

[ToJson]
[FromJson]
public partial class User
{
	[DataMember(Name = "id")]
	public Guid UserId { get; set; } = Guid.Empty;

	[DataMember(Name = "code")]
	public string Authid { get; set; } = string.Empty;

	[DataMember(Name = "password")]
	public string? Password { get; set; }

	[DataMember(Name = "salt")]
	public string? Salt { get; set; }

	[DataMember(Name = "email")]
	public string Email { get; set; } = string.Empty;

	[DataMember(Name = "created_at")]
	public DateTime CreatedAt { get; set; } = DateTime.MinValue;

	[DataMember(Name = "updated_at")]
	public DateTime UpdatedAt { get; set; } = DateTime.MinValue;
}
