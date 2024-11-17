using AspNetCore.Csr;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

namespace ApiExample.Models;

[FromJson]
public partial class DummyRequest
{
	[DataMember(Name = "user")]
	public User? User { get; set; }

	public int Id { get; set; } = 0;

	public List<ExUser> Users { get; set; } = new();

	public decimal Price { get; set; } = 0;

}
