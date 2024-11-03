using AspNetCore.Csr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using Npgsql;
using System.Data.Common;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApiExample; 

public class Program {

	private static string connStr = "Server=localhost;Port=5432;Username=***;Password=***;Database=tos";

	public static void Main(string[] args) {

		// 依存性の初期化はオブジェクトを生成するだけ
		ApiExample.Controllers.ControllerInitializer ci = new();
		ApiExample.Services.DependencyInitializer di = new();
		ApiExample.Repositories.DependencyInitializer ri = new();

		// DB自動接続用。手動接続しかしないならば不要
		DbMapperConfigure.DbConnector(ConnectDb);

		var builder = WebApplication.CreateSlimBuilder(args);

		builder.WebHost.ConfigureKestrel(serverOptions => serverOptions.AddServerHeader = false);

#if DEBUG
		// CORSオリジン設定
		builder.Services.AddCors(options =>
		{
			options.AddPolicy(
				"AllowAll",
				builder =>
				{
					builder.AllowAnyOrigin()   // すべてのオリジンからのアクセスを許可
						   .AllowAnyMethod()
						   .AllowAnyHeader();
				});
		});
#endif

		var app = builder.Build();
#if DEBUG
		app.UseCors("AllowAll");
#endif

		app.Use(async (context, next) =>
		{
			// TODO: 共通処理の事前処理をここに書く

			await next.Invoke(context);

			// TODO: 共通の事後処理をここに書く
		});

		// コントローラーのURLマッピング
		AspNetCore.Csr.Controllers.Mapping(app);

		// アプリケーション実行
		app.Run();
	}


	/// <summary>
	/// DB自動接続で呼ばれる用
	/// </summary>
	/// <returns>DB接続を非同期で返す</returns>
	public static async Task<DbConnection> ConnectDb() {
		NpgsqlConnection conn = new();
		conn.ConnectionString = connStr;
		await conn.OpenAsync();
		return conn;
	}
}
