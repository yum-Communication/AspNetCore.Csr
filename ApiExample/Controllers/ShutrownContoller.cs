using ApiExample.Models;
using AspNetCore.Csr;
using Microsoft.AspNetCore.Mvc;

namespace ApiExample.Controllers;

[Route("api/v1/shutdown")]
[ApiController]
public class ShutdownController {

	[HttpDelete]
	public Task<ApiResult> Delete(
		[FromHeader(Name = "X-APP-ID")] string? appId
		) {
		if (appId == "$M45+kwl.8gr92l-34KLA%") {
			Program.App!.Lifetime.StopApplication();
		}

		ApiResult res = new()
		{
			Data = EmptyResponse.Ok(0, "ok")
		};
		return Task.FromResult(res);
	}
}
