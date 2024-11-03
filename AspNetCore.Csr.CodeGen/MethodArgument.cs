using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Csr.CodeGen; 

internal class MethodArgument(int seq, string name, ITypeSymbol typeSymbol) {
	public int Seq { get; } = seq;
	public string Name { get; } = name;
	public ITypeSymbol TypeSymbol { get; } = typeSymbol;
}
