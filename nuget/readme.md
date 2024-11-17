# AspNetCore.Csr

AspNetCore.Csr is a tiny framework for AoT on AspNetCore, supports Controller-Service-Repository model with Simplified DI.

AspNetCore.Csr はAspNetCoreでAoTかつController-Service-Repositoryを実現するための簡易フレームワークです


## Controller

Just like traditional AspNetCore.Mvc controllers, you use the ApiController and Route attributes. Don't forget that the services you will use will be received in the default constructor.  
従来のAspNetCore.Mvcと同様に、コントローラーにはApiControllerおよびRoute属性を使用します。使用するサービスはデフォルトコンストラクタで受け取るので、忘れないようにしてください。
```csharp
using AspNetCore.Csr;

namespace ApiExample.Controllers;

[ApiController]
[Route("/api/users")]
public class UserController(
    // Default constructor parameter types must be fully specified
    ApiExample.Services.IUserService userService
    ) {

    private readonly ApiExample.Services.IUserService service = userService;

    // Write some method here...
}
```
There isn't much a controller method can do. It just takes values ​​from the Query, Route, Request-Header, Request-Body, etc. and returns an Task&lt;ApiResult&gt;.
The attributes that indicate the HTTP method can be HttpGet, HttpPost, HttpPut, or HttpDelete.  
コントローラーのメソッドでできることは多くはありません。Query, Route, Request-Header, Request-Bodyなどから値を受け取り、Task&lt;AspNetCore.Csr.ApiResult&gt;を返すだけです。
HTTPメソッドを示す属性には、HttpGet, HttpPost, HttpPut, HttpDeleteが使用できます。

Example method for HTTP-GET
```csharp
    [HttpGet("/{id}")]
    public async Task<ApiResult> GetById(
        [FromRoute(Name = "id")] string id
      ) {

        ApiResult res = new();
        try {
            res.Data = await service.GetUser(id);
        } catch (Exception e) {
            res.Code = 500;
            res.Data = null;
        }
        return res;
    }
```
There is no need to be confused by the sudden appearance of a class you have never heard of, ApiResult. Usage is very simple.  
ApiResultなんて聞いたことがないクラスがいきなり出てきたことで戸惑う必要はありません。使い方は非常に簡単です

---
### Code
This is HTTP response code, specification integer value.  
HTTPレスポンスコードです。整数値を指定してください

### Data
Json output is achieved by using a class that inherits AspNetCore.Csr.IToJsonData.  
AspNetCore.Csr.IToJsonDataを継承したクラスを使用することで、簡単にJSON出力ができます。

### ContentType
You can specify the Content-Type response header. The default value for this is `application/json`.
デフォルトで`application/json`となっているContent-Typeレスポンスヘッダです。

### Headers
You can also output other response headers.  
その他のヘッダを指定できます。

---

次に、HTTP-PUTの例を見てみましょう。

```csharp
    [HttpPut]
    public async Task<ApiResult> Put(
        [FromBody] User user
        ) {

        ApiResult res = new();
        try {
            int retCnt = await service.Register(user);
            res.Data = retCnt > 0
                ? MyResponse.Ok(0, "Ok")
                : MyResponse.Ok(1, "Empty");
        } catch (Exception e) {
            res.Code = 500;
            res.Data = MyResponse.Exception(e);
        }
        return res;
    }
```

Don't get frustrated by the appearance of unknown classes. Both MyResponse and UserRegRequest are user-defined classes, and their examples are as follows:  
謎のクラスが増えていますね。MyResponseとUserです。どちらもユーザー定義のクラスですが作成例はそれぞれ次のようになります。

```csharp
using AspNetCore.Csr;
using System.Runtime.Serialization;

namespace ApiExample.Models;

[ToJson]
public partial class MyResponse(int code, string message) {

    [DataMember(Name = "result")]
    public int Result { get; set; } = code;

    [DataMember(Name = "message")]
    public string? Message { get; set; } = message;

    public static IToJsonData? Ok(int code, string message) {
        return (IToJsonData?)new EmptyResponse(code, message);
    }

    public static IToJsonData? Exception(int code, Exception x) {
        return (IToJsonData?)new EmptyResponse(code, x.ToString());
    }
}
```

```csharp
using AspNetCore.Csr;
using System.Runtime.Serialization;

namespace ApiExample.Models;

[ToJson]
[FromJson]
public partial class User
{
    [DataMember(Name = "id")]
    public string UserId { get; set; } = string.Empty;

    [DataMember(Name = "name")]
    public string Name { get; set; } = string.Empty;

    [DataMember(Name = "updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.MinValue;
}
```
By adding the `ToJson` attribute, the class will automatically implement `IToJsonData` interface and `public void Serialize(System.IO.Stream s)` method for JSON output.  
`ToJson`属性を付けることにで、クラスは`IFromJsonData`インタフェースが自動的に実装され、`Serialize`メソッドが生成されます。

By adding the `FromJson` attribute, the class will automatically implement `IFromJsonData` interface and `public static IFromJsonData Deserialize(System.Text.Json.JsonElement je)` method for JSON output.  
`FromJson`属性を付けることにで、クラスは`IFromJsonData`インタフェースが自動的に実装され、`Deserialize`メソッドが生成されます。

These do not use reflection and therefore work well in an AoT environment.  
これらはリフレクションを使用しないため、AoT環境でも不都合なく動作します。


---
## Service

Specifying a service is very simple, just add a `Service` attribute to a class.  
サービスの指定はとても単純で、`Service`属性をクラスにつけるだけです。

A service class can receive a services or repositories by default constructor.
サービスクラスは、デフォルトコンストラクタでサービスやレポジトリを受け取ることができます

```csharp
using ApiExample.Models;
using ApiExample.Repositories;
using AspNetCore.Csr;

namespace ApiExample.Services;

[Service]
public partial class UserService(IUserRepo userRepo) {

    private IUserRepo repo = userRepo;

    public async Task<User?> GetUser(string id) {
        return await repo.SelectOne(id);
    }

    public async Task<int> Register(User user) {
        return await repo.Insert(user);
    }
}
```
Please don't say that async/await is not necessary in this example. In fact, you don't need a very simple service class that just calls a single method on the repository. In that case, you can just use the repository directly from your controller.  
この例ではasync/awaitは無くても良いとか言わないでください。実はコントローラーはレポジトリを受け取れるので、レポジトリのメソッドを一つ呼び出すだけのサービスクラスというもの自体が不要です。ここでは、あくまでも例示のためだけなので処理が無いだけです！

---
## Repository
The repository creates an interface. If you add the `DbMapper` attribute, the class will be automatically implemented. For the methods, specify the Select, Update/Insert/Delete, and Execute attributes according to the type of SQL.   
Update/Insert/Delete are not different internally.  
レポジトリはインタフェースを作ります。`DbMapper`属性を付けると、自動的にクラスが実装されます。メソッドにはそれぞれSQLの種類に従ってSelect, Update/Insert/Delete, Executeの属性を指定してください。  
※ Update/Insert/Deleteは内部的には差がありません
```csharp
using ApiExample.Entity;
using AspNetCore.Csr;

namespace ApiExample.Repositories;

[DbMapper("Npgsql")]
public interface IUserRepo {

	[Select("""
		SELECT * FROM m_user
		WHERE user_id = #{id}
		""")]
	public Task<UserEntity?> SelectOne(string id);


	[Select("""
		INSERT INTO m_user(user_id, name, updated_at)
		VALUES(#{user.UserId}, #{user.Name}, #{user.UpdatedAt})
		""")]
	public int Insert(User user);
}
```
`#{argumentName}` passes the method's argument to the SQL parameter. You can also specify the argument's property, as in the example.  
For dynamic SQL (where the SQL form changes depending on conditions), use the `#if{expression} ...SQL string.. #endif` syntax. The expression part will be created as an if statement in C# code, so it is a good idea to specify a null check or a value range.  
`#{argumentName}`はメソッドの引数をSQLのパラメタに渡します。例のように、引数のプロパティを指定することもできます。  
動的SQL（条件によってSQLの形を一部変える）場合には`#if{expression} ...SQL string.. #endif`構文を用います。expression部分はそのままC#のコードとしてif文が作成されますので、nullチェックや値の範囲を指定するとよいでしょう。  

dynamic SQL example
```csharp
	[Select("""
		SELECT *
		FROM  t_order
		WHERE 1 = 1
		#if{req.OrderedAtFrom != null} AND ordered_at >= #{req.OrderedAtFrom} #endif
		#if{req.OrderedAtTo != null} AND ordered_at <= #{req.OrderedAtTo} #endif
		#if{req.PoId != null} AND po_number = #{req.PoId} #endif
		""")]
	public Task<List<Order>> SelectOrders(DbCommand cmd, OrderRequest req);
```
I forgot to mention that repository methods return a `Task<T>`.  
For SELECT, you must specify a single object or `List` and set the return type to `Task`.  
For UPDATE/INSERT/DELETE, do not specify anything other than `Task<int>`.  
説明するのを忘れていましたが、レポジトリメソッドの戻り値は`Task<T>`です。  
SELECTの場合は単体のオブジェクトまたは`List`を指定して`Task`を戻り型に指定してください。  
UPDATE/INSERT/DELETEの場合は`Task<int>`以外を指定しないでください。

## Program.Main

At first, initialize Tiny-DI for controllers, services, repositories. It's very easy, just create an instance of the automatically implemented class.  
Ex:
```csharp
    ApiExample.Controllers.ControllerInitializer ci = new();
    ApiExample.Services.DependencyInitializer di = new();
    ApiExample.Repositories.DependencyInitializer ri = new();
```

At next, registration and mapping controllers to Kestrel.
```csharp
    AspNetCore.Csr.Controllers.Mapping(app);
```

At last, Run Kestrel.

Simple example of Program.cs here:
```csharp
using AspNetCore.Csr;
using Npgsql;
using System.Data.Common;

namespace ApiExample; 

public class Program {

    private static string connStr = "Server=localhost;Port=5432;Username=***;Password=***;Database=***";
    public static WebApplication? App { get; private set; }

    public static void Main(string[] args) {

        // Initialize Tiny-DI for controllers, services, repositories
        // 依存性の初期化はオブジェクトを生成するだけ
        ApiExample.Controllers.ControllerInitializer ci = new();
        ApiExample.Services.DependencyInitializer di = new();
        ApiExample.Repositories.DependencyInitializer ri = new();

        // Settings for automatic DB connection used in the repository. Not required if you always manually code database access
        // リポジトリでのDB自動接続用。手動接続しかしないならば不要
        DbMapperConfigure.DbConnector(ConnectDb);

        var builder = WebApplication.CreateSlimBuilder(args);
        builder.WebHost.ConfigureKestrel(serverOptions => serverOptions.AddServerHeader = false);
        var app = builder.Build();
        App = app;

        app.Use(async (context, next) =>
        {
            // TODO: 共通処理の事前処理をここに書く
            await next.Invoke(context);
            // TODO: 共通の事後処理をここに書く
        });

        AspNetCore.Csr.Controllers.Mapping(app);
        app.Run();
    }


    public static async Task<DbConnection> ConnectDb() {
        NpgsqlConnection conn = new();
        conn.ConnectionString = connStr;
        await conn.OpenAsync();
        return conn;
    }
}
```
