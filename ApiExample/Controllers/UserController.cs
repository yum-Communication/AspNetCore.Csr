using ApiExample.Models;
using AspNetCore.Csr;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ApiExample.Controllers;

[Microsoft.AspNetCore.Mvc.ApiControllerAttribute]
[Route("/users")]
public class UserController(
	// ここはフルネームでの記述が必要
	ApiExample.Services.IUserService userService
	) {

	private readonly ApiExample.Services.IUserService service = userService;

	[HttpGet]
	public async Task<ApiResult> Get(
		[FromQuery] string code,
		[FromHeader] string[] cookie
		) {

		var user = await service.GetUser(code);

		ApiResult res = new ApiResult();
		if (user != null) {
			res.Code = 200;
			res.Data = user;
		} else {
			res.Code = 400;
			res.Data = null;
		}
		return res;
	}


	class DummyResponseData: IToJsonData {
		public void Serialize(Stream s) {
			string outStr = "{ \"result\": 0 }";
			s.Write(Encoding.UTF8.GetBytes(outStr));
		}
	}

	[HttpGet("/{id}")]
	public Task<ApiResult> GetById([FromRoute(Name = "id")][Required] string id) {
		return Task<ApiResult>.Run(() =>
		{
			ApiResult res = new ApiResult();
			res.Code = 200;
			res.Data = new DummyResponseData();

			return res;
		});
	}

	[HttpPost]
	public Task<ApiResult> PostAtId([FromBody] DummyRequest req) {
		return Task<ApiResult>.Run(() =>
		{
			ApiResult res = new ApiResult();
			res.Code = 200;
			res.Data = new DummyResponseData();

			return res;
		});
	}
}
