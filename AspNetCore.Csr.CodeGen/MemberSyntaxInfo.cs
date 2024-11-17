using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Csr.CodeGen; 

internal class MemberSyntaxInfo {
	public bool IsPublic { get; set; } = false;
	public bool IsInternal { get; set; } = false;
	public string Accessor { get => IsPublic ? "public " : IsInternal ? "internal " : ""; }
	public bool IsPartial { get; set; } = false;
	public bool IsStatic { get; set; } = false;
	public string ReturnType { get; set; } = "void";
	public bool IsArrayReturnType { get; set; } = false;
	public bool IsListReturnType { get; set; } = false;
	public bool IsTaskReturnType { get; set; } = false;
	public string ReturnFullType { get; set; } = string.Empty;
	public string ReturnTypeElem { get; set; } = string.Empty;
	public string ReturnTypeN { get; set; } = string.Empty;
	public string ReturnTypeElemN { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;

	public void SetReturnType(ITypeSymbol typeSymbol) {
		ReturnFullType = typeSymbol.ToDisplayString();
		if (ReturnFullType.EndsWith("?")) {
			ReturnTypeN = ReturnFullType.Substring(0, ReturnFullType.Length - 1);
		} else {
			ReturnTypeN = ReturnFullType;
		}

		if (typeSymbol is IArrayTypeSymbol aryTypeSymbol) {
			// 配列
			IsArrayReturnType = true;
			ReturnTypeElem = aryTypeSymbol.ElementType.ToDisplayString();
		} else if (typeSymbol is INamedTypeSymbol namedTypeSymbol) {
			var x = namedTypeSymbol.TypeParameters;
			if (x.Length > 0) {
				var paramType = namedTypeSymbol.TypeArguments.Last();
				// タイプパラメタを持つ
				string name = namedTypeSymbol.Name;
				if(name == "Task") {
					IsTaskReturnType = true;
					if (paramType is INamedTypeSymbol namedTypeSymbol2) {
						if (namedTypeSymbol2.TypeArguments.Length > 0) {
							if (namedTypeSymbol2.Name == "List") {
								IsListReturnType = true;
							}
							paramType = namedTypeSymbol2.TypeArguments.Last();
						}
					}
				}else if(name == "List") {
					IsListReturnType = true;
				}
				ReturnTypeElem = paramType.ToDisplayString();
			}
		}

		if (ReturnTypeElem.EndsWith("?")) {
			ReturnTypeElemN = ReturnTypeElem.Substring(0, ReturnTypeElem.Length - 1);
		} else {
			ReturnTypeElemN = ReturnTypeElem;
		}
	}
}

internal class PropertySyntaxInfo {
	public bool IsPublic { get; set; } = false;
	public bool IsInternal { get; set; } = false;
	public bool IsStatic { get; set; } = false;

	/// <summary>
	/// プロパティ名
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// プロパティの型（単体名）
	/// </summary>
	public string ReturnType { get; set; } = "void";

	/// <summary>
	/// 入力されたプロパティの型（フル）
	/// </summary>
	public string ReturnFullType { get; set; } = string.Empty;

	/// <summary>
	/// 入力されたプロパティの型（NULL不可版）
	/// </summary>
	public string ReturnFullTypeN { get; set; } = string.Empty;

	/// <summary>
	/// プロパティの型が配列であるか
	/// </summary>
	public bool IsArrayReturnType { get; set; } = false;
	/// <summary>
	/// プロパティの型がListであるか
	/// </summary>
	public bool IsListReturnType { get; set; } = false;
	/// <summary>
	/// プロパティの型が配列またはListであったときの要素の型
	/// </summary>
	public string ReturnTypeElem { get; set; } = string.Empty;
	/// <summary>
	/// プロパティの型が配列またはListであったときの要素の型（NULL不可版）
	/// </summary>
	public string ReturnTypeElemN { get; set; } = string.Empty;

	public bool IsNullable { get; set; } = false;

	/// <summary>
	/// プロパティがゲッターを持つか
	/// </summary>
	public bool HasGetter { get; set; } = false;

	/// <summary>
	/// プロパティがセッターを持つか
	/// </summary>
	public bool HasSetter { get; set; } = false;

	/// <summary>
	/// 基本型であるか
	/// </summary>
	public bool IsBasicType { get; set; } = false;

	/// <summary>
	/// プロパティの型の処理
	/// </summary>
	/// <param name="typeSymbol">プロパティの型</param>
	public void SetReturnType(ITypeSymbol typeSymbol) {

		ReturnFullType = typeSymbol.ToDisplayString();
		// プロパティの型の末尾に'?'があればNULL許容型。
		if (ReturnFullType.EndsWith("?")) {
			ReturnFullTypeN = ReturnFullType.Substring(0, ReturnFullType.Length - 1);
			IsNullable = true;
		} else {
			ReturnFullTypeN = ReturnFullType;
		}

		if (typeSymbol is IArrayTypeSymbol aryTypeSymbol) {
			// 配列だった場合！
			IsArrayReturnType = true;
			ReturnTypeElem = aryTypeSymbol.ElementType.ToDisplayString();
		} else if (typeSymbol is INamedTypeSymbol namedTypeSymbol) {
			var x = namedTypeSymbol.TypeParameters;
			if (x.Length > 0) {
				// ジェネリック型！

				if (namedTypeSymbol.Name == "Nullable") {
					// null許容型はNullableジェネリッククラスであるため、その処理が必要！

					// NULL許容となっている元の型。これがnull許容型であるはずがない。
					var typeSymbol2 = namedTypeSymbol.TypeArguments.Last();
					ReturnFullTypeN = typeSymbol2.ToDisplayString();

					if (typeSymbol2 is INamedTypeSymbol namedTypeSymbol2) {
						x = namedTypeSymbol2.TypeParameters;
						if (x.Length > 0) {
							// NULL許容であるジェネリック型の場合はここに来る
							IsListReturnType = true;
							var paramType = namedTypeSymbol2.TypeArguments.Last();
							ReturnTypeElem = paramType.ToDisplayString();
						}
					}
				} else {
					IsListReturnType = true;
					var paramType = namedTypeSymbol.TypeArguments.Last();
					ReturnTypeElem = paramType.ToDisplayString();
				}
			}
		}

		if (ReturnTypeElem.EndsWith("?")) {
			ReturnTypeElemN = ReturnTypeElem.Substring(0, ReturnTypeElem.Length - 1);
			IsNullable = true;
		} else {
			ReturnTypeElemN = ReturnTypeElem;
		}

		switch (ReturnTypeElemN) {
		case "Bool":
		case "DateTime":
		case "Decimal":
		case "Double":
		case "Float":
		case "Guid":
		case "Int":
		case "Long":
		case "String":
		case "System.DateTime":
		case "System.Guid":
		case "System.String":
		case "bool":
		case "decimal":
		case "double":
		case "float":
		case "int":
		case "long":
		case "string":
			IsBasicType = true;
			break;
		}
	}
}
