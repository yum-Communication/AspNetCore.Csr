using ApiExample.Entity;
using AspNetCore.Csr;
using System.Data.Common;

namespace ApiExample.Repositories;

[DbMapper("Npgsql")]
public interface IUserRepo {

	[Select("""
		SELECT user_id, authid, email, created_at, updated_at
		FROM users
		WHERE 1 = 1
		#if{authid != null} authid = #{authid}#endif
		LIMIT 12
		""")]
	public Task<UserEntity?> SelectOne(string? authid);


	[Select("""
		SELECT user_id, authid, email, created_at, updated_at
		FROM users
		WHERE 1 = 1
		#if{authid != null} authid = #{authid}#endif
		LIMIT 12		
		""")]
	public Task<List<UserEntity>> Select(string authid);

	/// <summary>
	/// 売り先別商品情報・価格を登録・更新
	/// </summary>
	/// <param name="mm"></param>
	/// <param name="args"></param>
	/// <param name="itemCd"></param>
	/// <param name="ib"></param>
	/// <returns></returns>
	[Execute("call tkm.sp_item_base_price(#{itemCd})")]
	public Task<int> InsertItemPrice(string itemCd);


}
