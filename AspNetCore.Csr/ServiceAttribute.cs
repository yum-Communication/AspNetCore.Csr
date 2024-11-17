namespace AspNetCore.Csr;

/// <summary>
/// サービスであることを表す属性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ServiceAttribute: Attribute {}

/// <summary>
/// Json化出力するオブジェクトであることを表す属性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ToJsonAttribute: Attribute { }

/// <summary>
/// Jsonから値を受け取るオブジェクトであることを表す属性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class FromJsonAttribute: Attribute { }


/// <summary>
/// Entityクラスであることを表す属性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class EntityAttribute: Attribute { }

/// <summary>
/// 列名指定の属性
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ColumnNameAttribute(string name): Attribute {
	public string Name { get; } = name;
}

/// <summary>
/// DBマッパーであることを表す属性
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class DbMapperAttribute(string dbType): Attribute {
	public string DbType { get; } = dbType;
}

/// <summary>
/// SELECT文のマッパーを表す属性
/// </summary>
/// <param name="sql">実行するSQL</param>
[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class SelectAttribute(string sql): Attribute {
	public string Sql { get; } = sql;
}

/// <summary>
/// UPDATE文のマッパーを表す属性
/// </summary>
/// <param name="sql">実行するSQL</param>
[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class UpdateAttribute(string sql): Attribute {
	public string Sql { get; } = sql;
}

/// <summary>
/// INSERT文のマッパーを表す属性
/// </summary>
/// <param name="sql">実行するSQL</param>
[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class InsertAttribute(string sql): Attribute {
	public string Sql { get; } = sql;
}

/// <summary>
/// DELETE文のマッパーを表す属性
/// </summary>
/// <param name="sql">実行するSQL</param>
[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class DeleteAttribute(string sql): Attribute {
	public string Sql { get; } = sql;
}

/// <summary>
/// プロシージャ実行のマッパーを表す属性
/// </summary>
/// <param name="sql">実行するSQL</param>
[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class ExecuteAttribute(string sql): Attribute {
	public string Sql { get; } = sql;
}

/// <summary>
/// コンテキストのItemから値を取得する（ミドルウェアで入れているものを取得する用）
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
public class FromExAttribute: Attribute {
	public string? Name { get; set; }
}