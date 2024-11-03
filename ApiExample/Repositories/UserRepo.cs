using ApiExample.Entity;
using AspNetCore.Csr;
using System.Data.Common;

namespace ApiExample.Repositories;

[DbMapper("Npgsql")]
public interface IUserRepo {

	[Select("""
		SELECT user_id, authid, email, created_at, updated_at
		FROM users
		WHERE authid = #{authid}
		""")]
	public Task<UserEntity?> SelectOne(string authid);


	[Select("""
		SELECT user_id, authid, email, created_at, updated_at
		FROM users
		WHERE authid = #{authid}
		""")]
	public Task<List<UserEntity>> Select(string authid);

}
