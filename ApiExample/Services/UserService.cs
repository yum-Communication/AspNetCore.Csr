using ApiExample.Models;
using ApiExample.Repositories;
using AspNetCore.Csr;

namespace ApiExample.Services;

[Service]
public partial class UserService(IUserRepo userRepo) {

	private IUserRepo repo = userRepo;

	public int IntValue { get; set; }

	public async Task<User?> GetUser(string code) {
		return await repo.SelectOne(code);
	}
}
