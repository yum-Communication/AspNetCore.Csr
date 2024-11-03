using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Csr.CodeGen;

public class TypeSymbolInfo {
	public string Name { get; }
	public bool IsNullable { get; }

	public TypeSymbolInfo(ITypeSymbol typeSymbol) {
		string name = typeSymbol.ToString();
		if (name.EndsWith("?")) {
			IsNullable = true;
			name = name.Substring(0, name.Length - 1);
		} else {
			IsNullable = false;
		}

		Name = GetTypeName(name);
	}

	private static string GetTypeName(string name) {
		switch (name) {
		case "Boolean":
		case "bool":
			return "Bool";

		case "Int32":
		case "int":
			return "Int";

		case "Int64":
		case "long":
			return "Long";

		case "Float":
		case "float":
			return "Float";

		case "Double":
		case "double":
			return "Double";

		case "Decimal":
		case "decimal":
			return "Decimal";

		case "String":
		case "string":
			return "String";

		case "DateTime":
		case "System.DateTime":
			return "DateTime";

		case "Guid":
		case "System.Guid":
			return "Guid";
		}
		return "Object";
	}
}

