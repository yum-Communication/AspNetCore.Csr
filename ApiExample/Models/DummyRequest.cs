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
}
