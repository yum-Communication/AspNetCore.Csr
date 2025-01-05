using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace AspNetCore.Csr.CodeGen;

[Generator]
public class Generator: IIncrementalGenerator {

	private static readonly string GenSignature = "\t[global::System.CodeDom.Compiler.GeneratedCode(\"AspNetCore.Csr.CodeGen\", \"1.0.2.0\")]";

	private static readonly Regex rgxController = new("^(Microsoft.AspNetCore.Mvc.)?ApiController(Attribute)?$");
	private static readonly Regex rgxService = new("^(AspNetCore.Csr.)?Service(Attribute)?$");
	private static readonly Regex rgxToJson = new("^(AspNetCore.Csr.)?ToJson(Attribute)?$");
	private static readonly Regex rgxFromJson = new("^(AspNetCore.Csr.)?FromJson(Attribute)?$");
	private static readonly Regex rgxEntity = new("^(AspNetCore.Csr.)?Entity(Attribute)?$");
	private static readonly Regex rgxDbMapper = new("^(AspNetCore.Csr.)?DbMapper(Attribute)?$");
	private static readonly Regex rgxSqlParam = new("#{[^}]+}", RegexOptions.Multiline);
	private static readonly Regex rgxWhiteSpace = new("\\s+", RegexOptions.Multiline);
	private static readonly Regex rgxComment = new("--[^\\n]*", RegexOptions.Multiline);

	/// <summary>
	/// 初期化。ハンドラ的なのを登録する
	/// </summary>
	/// <param name="context"></param>
	public void Initialize(IncrementalGeneratorInitializationContext context) {

		Debug.WriteLine("Initialize");
		IncrementalValuesProvider<(SyntaxNode, INamedTypeSymbol)> entitySyntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
			static (SyntaxNode node, CancellationToken token) =>
			{

				// 対象の属性がついているクラスを対象とする
				if (node is ClassDeclarationSyntax classDeclSyntax && classDeclSyntax.AttributeLists.Count > 0) {
					return classDeclSyntax.AttributeLists.Count((AttributeListSyntax attributeListSyntax) =>
					{
						foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes) {
							var txt = attributeSyntax.Name.ToFullString();

							if (rgxController.IsMatch(txt) || rgxService.IsMatch(txt) || rgxToJson.IsMatch(txt) || rgxFromJson.IsMatch(txt) || rgxEntity.IsMatch(txt)) {
								return true;
							}
						}
						return false;
					}) > 0;
				} else if (node is InterfaceDeclarationSyntax interfaceDeclSyntax && interfaceDeclSyntax.AttributeLists.Count > 0) {
					return interfaceDeclSyntax.AttributeLists.Count((AttributeListSyntax attributeListSyntax) =>
					{
						foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes) {
							var txt = attributeSyntax.Name.ToFullString();

							if (rgxDbMapper.IsMatch(txt)) {
								return true;
							}
						}
						return false;
					}) > 0;
				}
				return false;
			}, static (GeneratorSyntaxContext context, CancellationToken token) =>
			{
				token.ThrowIfCancellationRequested();
				if (context.Node is ClassDeclarationSyntax clsDeclSyntax && clsDeclSyntax.AttributeLists.Count > 0) {
					return (context.Node, Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetDeclaredSymbol(context.SemanticModel, clsDeclSyntax, token)!);
				} else if (context.Node is InterfaceDeclarationSyntax interfaceDeclSyntax && interfaceDeclSyntax.AttributeLists.Count > 0) {
					return (context.Node, Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetDeclaredSymbol(context.SemanticModel, interfaceDeclSyntax, token)!);
				}
				throw new NullReferenceException("Out of filtered syntax.");
			}
		);

		context.RegisterSourceOutput(entitySyntaxProvider, (SourceProductionContext spc, (SyntaxNode syntaxNode, INamedTypeSymbol clsSymbol) tpl) =>
		{
			spc.CancellationToken.ThrowIfCancellationRequested();

			string namespaceStr = tpl.clsSymbol.ContainingNamespace.ToDisplayString();
			string accessor = "";
			string interfaceName;
			string className;

			StringBuilder text = new($$"""
// Generated source
#pragma warning disable
#nullable enable
using AspNetCore.Csr;

namespace {{namespaceStr}} {

""");
			if (tpl.syntaxNode is ClassDeclarationSyntax clsSyntax) {
				// クラスの場合
				// Controller, Service, FromJson, ToJson, Entityとある
				className = tpl.clsSymbol.Name.ToString();
				interfaceName = "I" + className;
				string classFullName = GetGenericClassName(tpl.clsSymbol);

				bool isController = false;
				bool isService = false;
				bool isToJson = false;
				bool isFromJson = false;
				bool isEntity = false;
				string routePath = string.Empty;

				foreach (var attr in tpl.clsSymbol.GetAttributes()) {
					string attrName = attr.AttributeClass!.Name;
					switch (attrName) {
					case "ApiControllerAttribute":
						isController = true;
						break;
					case "ControllerAttribute":
						isController = true;
						break;
					case "ServiceAttribute":
						isService = true;
						break;
					case "ToJsonAttribute":
						isToJson = true;
						break;
					case "FromJsonAttribute":
						isFromJson = true;
						break;
					case "EntityAttribute":
						isEntity = true;
						break;
					case "RouteAttribute":
						var attrDataCns = attr.ConstructorArguments.First();
						if (attrDataCns.Value != null) {
							routePath = attrDataCns.Value.ToString();
						}
						break;
					}
				}

				foreach (SyntaxToken syntaxToken in clsSyntax.Modifiers) {
					switch (syntaxToken.Text) {
					case "internal":
					case "public":
						accessor = syntaxToken.Text + " ";
						break;
					}
				}
				Dictionary<string, MemberSyntaxInfo> miDic = new();
				Dictionary<string, PropertySyntaxInfo> piDic = new();
				GetMemberSyntaxInfo(clsSyntax, miDic, piDic);

				if (isService) {
					OutputServiceInterface(tpl.clsSymbol, accessor, miDic, piDic, className, interfaceName, text);
					OutputFactory(tpl.clsSymbol, accessor, className, interfaceName, namespaceStr, text);
					text.Append("\t\tinternal partial class DependencyInitializer {\n\t\tprivate readonly ").Append(className).Append("Factory m_").Append(className).Append("Factory = new ();\n}\n");
				} else if (isController) {
					OutputFactory(tpl.clsSymbol, accessor, className, interfaceName, namespaceStr, text);
					OutputControllerMapper(tpl.clsSymbol, className, namespaceStr, miDic, text);
				} else if (isFromJson && isToJson) {
					text.Append($$"""
	public partial class {{classFullName}}: IFromJsonData, IToJsonData {

""");
					OutputFromJson(tpl.clsSymbol, classFullName, text);
					OutputToJson(tpl.clsSymbol, piDic, text);
					if (isEntity) {
						OutputEntity(tpl.clsSymbol, classFullName, text);
					}
					text.Append("\n\t}\n");
				} else if (isFromJson) {
					text.Append($$"""
	public partial class {{classFullName}}: IFromJsonData {

""");
					OutputFromJson(tpl.clsSymbol, classFullName, text);
					if (isEntity) {
						OutputEntity(tpl.clsSymbol, classFullName, text);
					}
					text.Append("\n\t}\n");
				} else if (isToJson) {
					text.Append($$"""
	public partial class {{classFullName}}: IToJsonData {

""");
					OutputToJson(tpl.clsSymbol, piDic, text);
					if (isEntity) {
						OutputEntity(tpl.clsSymbol, classFullName, text);
					}
					text.Append("\n\t}\n");
				} else if (isEntity) {
					text.Append($$"""
	public partial class {{classFullName}} {

""");
					OutputEntity(tpl.clsSymbol, classFullName, text);
					text.Append("\n\t}\n");
				}
			} else if (tpl.syntaxNode is InterfaceDeclarationSyntax interfaceSyntax) {
				// インタフェースの場合
				interfaceName = tpl.clsSymbol.Name.ToString();
				className = interfaceName.Substring(1, interfaceName.Length - 1);

				// DbMapperしかない

				string dbType = string.Empty;
				foreach (var attr in tpl.clsSymbol.GetAttributes()) {
					string attrName = attr.AttributeClass!.Name;
					switch (attrName) {
					case "DbMapper":
					case "DbMapperAttribute":
						var attrDataCns = attr.ConstructorArguments.First();
						if (attrDataCns.Value != null) {
							dbType = attrDataCns.Value.ToString();
						}
						break;
					}
				}

				if (dbType != string.Empty) {
					foreach (SyntaxToken syntaxToken in interfaceSyntax.Modifiers) {
						switch (syntaxToken.Text) {
						case "internal":
						case "public":
							accessor = syntaxToken.Text + " ";
							break;
						}
					}
					OutDbMapper(tpl.clsSymbol, accessor, className, interfaceName, dbType, text);
					OutputFactory(tpl.clsSymbol, accessor, className, interfaceName, namespaceStr, text);
					text.Append("\tinternal partial class DependencyInitializer {\n\t\tprivate readonly ").Append(className).Append("Factory m_").Append(className).Append("Factory = new ();\n\t}\n");
				}
			} else {
				return;
			}

			// 最後にnamespaceを閉じる
			text.Append("\n}\n");
			spc.AddSource(className + ".g.cs", text.ToString());
		});
	}

	private static string GetGenericClassName(ITypeSymbol clsSymbol) {
		if (clsSymbol is INamedTypeSymbol namedTypeSymbol) {
			var x = namedTypeSymbol.TypeParameters;
			if (x.Length > 0) {
				StringBuilder sb = new(clsSymbol.Name.ToString());
				sb.Append('<');
				int i = 0;
				foreach (var tp in namedTypeSymbol.TypeParameters) {
					if (i > 0) { sb.Append(','); }
					sb.Append(tp.ToDisplayString());
					++i;
				}
				sb.Append('>');
				return sb.ToString();
			}
		}
		return clsSymbol.Name.ToString();
	}

	/// <summary>
	/// メンバの情報を一覧化
	/// </summary>
	/// <param name="tpl"></param>
	/// <param name="miDic"></param>
	private static void GetMemberSyntaxInfo(ClassDeclarationSyntax clsSyntax, Dictionary<string, MemberSyntaxInfo> miDic, Dictionary<string, PropertySyntaxInfo> piDic) {
		foreach (MemberDeclarationSyntax memberSyntax in clsSyntax.Members) {
			if (memberSyntax is MethodDeclarationSyntax methodDeclSymbol) {

				MemberSyntaxInfo mi = new();
				//foreach (var token in memberSyntax.ChildTokens()) {
				foreach (SyntaxToken token in methodDeclSymbol.Modifiers) {
					switch (token.Text) {
					case "internal":
						mi.IsInternal = true;
						break;
					case "partial":
						mi.IsPartial = true;
						break;
					case "public":
						mi.IsPublic = true;
						break;

					case "static":
						mi.IsStatic = true;
						break;

					case "override":
					case "private":
					case "protected":
					case "readonly":
					case "virtual":
						// partial メンバで非対応のトークンを検出した場合は生成対象としない
						goto NEXT_MEMBER;

					default:
						break;
					}
				}

				mi.Name = methodDeclSymbol.Identifier.Text;
				mi.ReturnType = methodDeclSymbol.ReturnType.ToFullString();
				miDic.Add(mi.Name, mi);
			} else if (memberSyntax is PropertyDeclarationSyntax propertyDeclSyntax) {
				PropertySyntaxInfo pi = new();
				foreach (SyntaxToken token in propertyDeclSyntax.Modifiers) {
					switch (token.Text) {
					case "internal":
						pi.IsInternal = true;
						break;
					case "public":
						pi.IsPublic = true;
						break;
					case "static":
						pi.IsStatic = true;
						break;
					case "override":
					case "partial":
					case "private":
					case "protected":
					case "readonly":
					case "virtual":
						// partial メンバで非対応のトークンを検出した場合は生成対象としない
						goto NEXT_MEMBER;

					default:
						break;
					}
				}
				pi.Name = propertyDeclSyntax.Identifier.Text;
				pi.ReturnType = propertyDeclSyntax.Type.ToFullString();
				piDic.Add(pi.Name, pi);
			}
NEXT_MEMBER:
			;
		}
	}


	/// <summary>
	/// サービス用。Interfaceを出力する
	/// </summary>
	/// <param name="tpl"></param>
	/// <param name="isInternal"></param>
	/// <param name="isPublic"></param>
	/// <param name="miDic"></param>
	/// <param name="className"></param>
	/// <param name="interfaceName"></param>
	/// <param name="text"></param>
	private void OutputServiceInterface(INamedTypeSymbol clsSymbol, string accessor, Dictionary<string, MemberSyntaxInfo> miDic, Dictionary<string, PropertySyntaxInfo> piDic, string className, string interfaceName, StringBuilder genText) {
		genText.Append($$"""

	{{accessor}}interface {{interfaceName}} {

""");
		int procIdx = 0;
		foreach (var it in clsSymbol.GetMembers()) {
			if ((it is IMethodSymbol methodSymbol) && miDic.TryGetValue(methodSymbol.Name, out var methodSytaxInfo)) {
				DoInterfaceMethod(genText, methodSymbol, methodSytaxInfo);
			} else if ((it is IPropertySymbol propertySymbol) && piDic.TryGetValue(propertySymbol.Name, out var propertySyntaxInfo)) {
				DoInterfaceProperty(genText, propertySymbol, propertySyntaxInfo);
			}

			++procIdx;
		}
		genText.Append($$"""
	}
	{{accessor}}partial class {{className}} : {{interfaceName}};

""");
	}


	/// <summary>
	/// サービスのInterface出力用。メソッドの出力
	/// </summary>
	/// <param name="genText"></param>
	/// <param name="symbol"></param>
	/// <param name="sytaxInfo"></param>
	private static void DoInterfaceMethod(StringBuilder genText, IMethodSymbol symbol, MemberSyntaxInfo sytaxInfo) {
		// 戻り値の型を取得
		sytaxInfo.SetReturnType(symbol.ReturnType);

		genText.Append(GenSignature);
		genText.Append("\n\t\t");
		genText.Append(sytaxInfo.ReturnFullType).Append(' ').Append(symbol.Name);

		if (!symbol.TypeArguments.IsEmpty) {
			genText.Append("<");
			int typeArgIdx = 0;
			foreach (var typeArg in symbol.TypeArguments) {
				if (typeArgIdx > 0) {
					genText.Append(',');
				}
				genText.Append(typeArg.MetadataName);
				++typeArgIdx;
			}
			genText.Append(">");
		}

		genText.Append("(");

		// 引数を出力
		Dictionary<string, MethodArgument> maDic = new();
		List<MethodArgument> maList = new();
		int methodParamCount = 0;
		foreach (var methodParam in symbol.Parameters) {
			if (methodParamCount > 0) {
				genText.Append(", ");
			}
			MethodArgument ma = new(methodParamCount, methodParam.Name, methodParam.Type);
			maList.Add(ma);
			maDic.Add(methodParam.Name, ma);

			genText.Append(methodParam.ToDisplayString());
			++methodParamCount;
		}
		genText.Append(");\n\n");

	}


	/// <summary>
	/// サービスのInterface出力用。プロパティの出力
	/// </summary>
	/// <param name="genText"></param>
	/// <param name="symbol"></param>
	/// <param name="syntaxInfo"></param>
	private static void DoInterfaceProperty(StringBuilder genText, IPropertySymbol symbol, PropertySyntaxInfo syntaxInfo) {
		// 戻り値の型を取得
		syntaxInfo.SetReturnType(symbol.Type);

		genText.Append(GenSignature);
		genText.Append("\n\t\t");
		genText.Append(syntaxInfo.ReturnFullType).Append(' ').Append(symbol.Name).Append(" { ");
		if (symbol.GetMethod != null) {
			genText.Append("get; ");
		}
		if (symbol.SetMethod != null) {
			genText.Append("set; ");
		}
		genText.Append("}\n\n");
	}


	/// <summary>
	/// ファクトリクラスの出力
	/// </summary>
	/// <param name="tpl"></param>
	/// <param name="accessor"></param>
	/// <param name="className"></param>
	/// <param name="interfaceName"></param>
	/// <param name="namespaceName"></param>
	/// <param name="genText"></param>
	private static void OutputFactory(INamedTypeSymbol clsSymbol, string accessor, string className, string interfaceName, string namespaceName, StringBuilder genText) {
		genText.Append($$"""
	{{accessor}}class {{className}}Factory: IDependencyFactory {
		private static {{className}}? instance = null;

		{{accessor}}{{className}}Factory() {
			Dependencies.Registration("{{namespaceName}}.{{interfaceName}}", this);
		}
		{{accessor}} object Create() {
			return Build();
		}

		internal static {{className}} Build() {
			if (instance == null) {

""");

		// コンストラクタパラメタを渡してnewするコードを出力
		StringBuilder argText = new();
		if (clsSymbol.Constructors.Length > 0) {
			var defConstructor = clsSymbol.Constructors[0];
			int argIdx = 0;
			foreach (var param in defConstructor.Parameters) {
				string paramType = param.Type.ToDisplayString();
				string paramName = param.Name.ToString();

				genText.Append($"\t\t\t\tvar o_{paramName} = ({paramType})Dependencies.Resolve(\"{paramType}\");\n");
				if (argIdx > 0) {
					argText.Append(", ");
				}
				argText.Append("o_").Append(paramName);
				++argIdx;
			}
		}
		genText.Append($$"""
				instance = new {{namespaceName}}.{{className}}({{argText}});
			}
			return instance;
		}
	}

""");
	}


	/// <summary>
	/// コントローラーのマッパー登録を出力
	/// </summary>
	/// <param name="tpl"></param>
	/// <param name="className"></param>
	/// <param name="namespaceName"></param>
	/// <param name="miDic"></param>
	/// <param name="genText"></param>
	private static void OutputControllerMapper(INamedTypeSymbol clsSymbol, string className, string namespaceName, Dictionary<string, MemberSyntaxInfo> miDic, StringBuilder genText) {

		Regex rgxRouteParam = new("{[_A-Za-z][_0-9A-Za-z]*}");

		genText.Append($$"""
	internal class {{className}}Mapper : IControllerMapper {

		internal {{className}}Mapper() {
			AspNetCore.Csr.Controllers.Registration(this);
		}

		public void Add(Microsoft.AspNetCore.Builder.WebApplication app) {

""");
		string mapGroupRoute = string.Empty;
		foreach (var attr in clsSymbol.GetAttributes()) {
			string attrName = attr.AttributeClass!.Name;
			if (attrName == "RouteAttribute") {
				var attrDataCns = attr.ConstructorArguments.First();
				if (attrDataCns.Value != null) {
					mapGroupRoute = attrDataCns.Value.ToString();
				}
			}
		}
		genText.Append("\t\t\tvar group = app.MapGroup(\"").Append(mapGroupRoute).Append("\");\n");

		foreach (var it in clsSymbol.GetMembers()) {
			// マッパーに登録されるのはメソッドのみ
			if (it is not IMethodSymbol methodSymbol) {
				continue;
			}
			// 対象として認識されているか
			if (!miDic.TryGetValue(methodSymbol.Name, out var methodSyntaxInfo)) {
				continue;
			}

			// メソッドの属性からGET/POST/PUT/DELETEの別とRouteを取得する
			string methodRoute = string.Empty;
			string httpMethod = string.Empty;
			foreach (var attr in methodSymbol.GetAttributes()) {
				string attrName = attr.AttributeClass!.Name;
				if (attrName == "HttpGetAttribute") {
					httpMethod = "Get";
				} else if (attrName == "HttpPostAttribute") {
					httpMethod = "Post";
				} else if (attrName == "HttpPutAttribute") {
					httpMethod = "Put";
				} else if (attrName == "HttpDeleteAttribute") {
					httpMethod = "Delete";
				}

				List<string> routeParamName = new();
				if (httpMethod != string.Empty) {
					if (attr.ConstructorArguments.Length > 0) {
						var attrDataCns = attr.ConstructorArguments.First();
						if (attrDataCns.Value != null) {
							methodRoute = attrDataCns.Value.ToString();
						}
					}

					genText.Append($$"""
			group.Map{{httpMethod}}("{{methodRoute}}", async (Microsoft.AspNetCore.Http.HttpContext context) =>
			{
				{{namespaceName}}.{{className}} controller = {{namespaceName}}.{{className}}Factory.Build();

""");

					// メソッドの引数一覧を処理する
					// 引数を出力
					StringBuilder argText = new();
					Dictionary<string, MethodArgument> maDic = new();
					List<MethodArgument> maList = new();
					int methodParamCount = 0;
					foreach (var methodParam in methodSymbol.Parameters) {
						//MethodArgument ma = new(methodParamCount, methodParam.Name, methodParam.Type);
						string paramName = methodParam.Name;
						ITypeSymbol paramType = methodParam.Type;
						string paramTypeName = paramType.ToDisplayString();

						bool isArray = false;
						if (paramTypeName.EndsWith("[]")) {
							isArray = true;
							paramTypeName = paramTypeName.Substring(0, paramTypeName.Length - 2);
						}

						// 引数の属性
						foreach (var a in methodParam.GetAttributes()) {
							string paramAttrName = a.AttributeClass!.Name;

							switch (paramAttrName) {
							case "FromRouteAttribute": {
									var pk = GetKeyName(a, paramName);
									genText.Append($"\t\t\t\tstring? o_{paramName} = (string?)context.Request.RouteValues[\"{pk}\"];\n");
									break;
								}

							case "FromHeaderAttribute": {
									var pk = GetKeyName(a, paramName);
									genText.Append($"\t\t\t\tvar o_{paramName} = context.Request.Headers[\"{pk}\"];\n");
									break;
								}

							case "FromQueryAttribute": {
									var pk = GetKeyName(a, paramName);
									genText.Append($"\t\t\t\tvar o_{paramName} = context.Request.Query[\"{pk}\"];\n");
									break;
								}

							case "FromExAttribute": {
									var pk = GetKeyName(a, paramName);
									genText.Append($"\t\t\t\tvar o_{paramName} = ({paramTypeName})context.Items[\"{pk}\"];\n");
									break;
								}

							case "FromBodyAttribute":
								genText.Append($"\t\t\t\t{paramTypeName} o_{paramName} = await HttpContextUtility.ReadBody<{paramTypeName}>(context);\n");
								break;
							}
						}

						if (methodParamCount > 0) {
							argText.Append(",");
						}

						switch (paramTypeName) {
						case "Int":
						case "int":
							if (isArray) {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToIntArray(o_{paramName}, \"{paramName}\")");
							} else if (paramType.NullableAnnotation != NullableAnnotation.None) {
								// NOT NULL
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToIntNotNull(o_{paramName}, \"{paramName}\")");
							} else {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToInt(o_{paramName})");
							}
							break;

						case "Long":
						case "long":
							if (isArray) {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToLongArray(o_{paramName}, \"{paramName}\")");
							} else if (paramType.NullableAnnotation != NullableAnnotation.None) {
								// NOT NULL
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToLongNotNull(o_{paramName}, \"{paramName}\")");
							} else {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToLong(o_{paramName})");
							}
							break;

						case "Float":
						case "float":
							if (isArray) {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToFloatArray(o_{paramName}, \"{paramName}\")");
							} else if (paramType.NullableAnnotation != NullableAnnotation.None) {
								// NOT NULL
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToFloatNotNull(o_{paramName}, \"{paramName}\")");
							} else {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToFloat(o_{paramName})");
							}
							break;

						case "Double":
						case "double":
							if (isArray) {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToDoubleArray(o_{paramName}, \"{paramName}\")");
							} else if (paramType.NullableAnnotation != NullableAnnotation.None) {
								// NOT NULL
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToDoubleNotNull(o_{paramName}, \"{paramName}\")");
							} else {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToDouble(o_{paramName})");
							}
							break;

						case "Decimal":
						case "decimal":
							if (isArray) {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToDecima;Array(o_{paramName}, \"{paramName}\")");
							} else if (paramType.NullableAnnotation != NullableAnnotation.None) {
								// NOT NULL
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToDecimalNotNull(o_{paramName}, \"{paramName}\")");
							} else {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToDecimal(o_{paramName})");
							}
							break;

						case "Bool":
						case "bool":
							if (isArray) {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToBoolArray(o_{paramName}, \"{paramName}\")");
							} else if (paramType.NullableAnnotation != NullableAnnotation.None) {
								// NOT NULL
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToBoolNotNull(o_{paramName}, \"{paramName}\")");
							} else {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToBool(o_{paramName})");
							}
							break;

						case "String":
						case "string":
							if (isArray) {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToStringArray(o_{paramName}, \"{paramName}\")");
							} else if (paramType.NullableAnnotation != NullableAnnotation.None) {
								// NOT NULL
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToStringNotNull(o_{paramName}, \"{paramName}\")");
							} else {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToString(o_{paramName})");
							}
							break;

						case "DateTime":
							if (isArray) {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToDateTimeArray(o_{paramName}, \"{paramName}\")");
							} else if (paramType.NullableAnnotation != NullableAnnotation.None) {
								// NOT NULL
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToDateTimeNotNull(o_{paramName}, \"{paramName}\")");
							} else {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToDateTime(o_{paramName})");
							}
							break;

						case "Guid":
							if (isArray) {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToGuidArray(o_{paramName}, \"{paramName}\")");
							} else if (paramType.NullableAnnotation != NullableAnnotation.None) {
								// NOT NULL
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToGuidNotNull(o_{paramName}, \"{paramName}\")");
							} else {
								argText.Append($"\n\t\t\t\tHttpContextUtility.ToGuid(o_{paramName})");
							}
							break;

						default:
							argText.Append("\n\t\t\t\to_").Append(paramName);
							break;
						}
						++methodParamCount;
					}

					genText.Append($$"""
				var res = await controller.{{methodSymbol.Name}}({{argText}});
				await HttpContextUtility.Output(context, res);
			});

""");
					break;
				}
			}
		}

		genText.Append($$"""
		}
	}

	internal partial class ControllerInitializer {
		private readonly {{className}}Mapper m_{{className}}Mapper = new();
	}

""");
	}


	private static string GetKeyName(AttributeData a, string paramName) {
		string pk = paramName;
		if (a.ConstructorArguments.Length > 0) {
			var nameArg = a.ConstructorArguments.SingleOrDefault(tt => tt.Type?.Name == "Name");
			if (nameArg.Value != null) {
				pk = (string)nameArg.Value;
			}
		} else if (a.NamedArguments.Length > 0) {
			var nameArg = a.NamedArguments.SingleOrDefault(tt => tt.Key == "Name");
			if (nameArg.Value.Value != null) {
				pk = nameArg.Value.Value.ToString();
			}
		}

		return pk;
	}


	/// <summary>
	/// ストリームから読み込み済みのJsonオブジェクトからモデルオブジェクトを構築するやつを出力
	/// </summary>
	/// <param name="tpl"></param>
	/// <param name="className"></param>
	/// <param name="piDic"></param>
	/// <param name="genText"></param>
	private static void OutputFromJson(INamedTypeSymbol clsSymbol, string className, StringBuilder genText) {
		genText.Append($$"""
		public static IFromJsonData Deserialize(System.Text.Json.JsonElement je) {

""");
		StringBuilder initializing = new();
		// メンバをループしてローカルに読み込む部分を出力
		INamedTypeSymbol? typ = clsSymbol;
		while (typ != null) {
			foreach (var it in typ.GetMembers()) {
				// public でも internalでもないメンバは関係ない
				if (it.DeclaredAccessibility != Accessibility.Public && it.DeclaredAccessibility != Accessibility.Internal) { continue; }

				if (it is IPropertySymbol symbol) {
					string dataMemberName = GetDataMemberName(symbol, it.Name);
					PropertySyntaxInfo propSyntaxInfo = new();
					propSyntaxInfo.SetReturnType(symbol.Type);

					string cast = string.Empty;
					switch (propSyntaxInfo.ReturnFullTypeN) {
					case "Bool":
					case "bool":
						genText.Append($"\t\t\tbool? a_{it.Name} = JsonHelper.ReadBool(je, \"{dataMemberName}\");\n");
						break;

					case "Decimal":
					case "decimal":
						genText.Append($"\t\t\tdecimal? a_{it.Name} = JsonHelper.ReadDecimal(je, \"{dataMemberName}\");\n");
						break;

					case "Double":
					case "double":
						genText.Append($"\t\t\tdouble? a_{it.Name} = JsonHelper.ReadDouble(je, \"{dataMemberName}\");\n");
						break;

					case "float":
					case "Float":
						genText.Append($"\t\t\tfloat? a_{it.Name} = (float?)JsonHelper.ReadDouble(je, \"{dataMemberName}\");\n");
						break;

					case "Int":
					case "int":
						genText.Append($"\t\t\tint? a_{it.Name} = JsonHelper.ReadInt32(je, \"{dataMemberName}\");\n");
						break;

					case "Long":
					case "long":
						genText.Append($"\t\t\tlong? a_{it.Name} = JsonHelper.ReadInt64(je, \"{dataMemberName}\");\n");
						break;

					case "String":
					case "string":
						genText.Append($"\t\t\tstring? a_{it.Name} = JsonHelper.ReadString(je, \"{dataMemberName}\");\n");
						break;

					case "System.DateTime":
					case "DateTime":
						genText.Append($"\t\t\tSystem.DateTime? a_{it.Name} = JsonHelper.ReadDateTime(je, \"{dataMemberName}\");\n");
						cast = "(System.DateTime)";
						break;

					case "System.Guid":
					case "Guid":
						genText.Append($"\t\t\tSystem.Guid? a_{it.Name} = JsonHelper.ReadGuid(je, \"{dataMemberName}\");\n");
						cast = "(System.Guid)";
						break;

					default:
						if (propSyntaxInfo.IsListReturnType) {
							// List の場合
							genText.Append($"\t\t\tList<{propSyntaxInfo.ReturnTypeElemN}>? a_{it.Name} = JsonHelper.ReadList(je, \"{dataMemberName}\", jc =>");
							ProcListElement(genText, propSyntaxInfo);
							genText.Append(");\n");

						} else if (propSyntaxInfo.IsListReturnType) {
							// 配列の場合
							genText.Append($"\t\t\t{propSyntaxInfo.ReturnTypeElemN}[]? a_{it.Name} = JsonHelper.ReadList(je, \"{dataMemberName}\", jc =>");
							ProcListElement(genText, propSyntaxInfo);
							genText.Append(")?.ToArray();\n");

						} else {
							// オブジェクトの場合
							genText.Append($"\t\t\t{propSyntaxInfo.ReturnFullTypeN}? a_{it.Name} = JsonHelper.ReadObject<{propSyntaxInfo.ReturnFullTypeN}>(je, \"{dataMemberName}\");\n");
						}
						break;
					}
					if (propSyntaxInfo.IsNullable) {
						initializing.Append("\t\t\tret.").Append(it.Name).Append(" = a_").Append(it.Name).Append(";\n");
					} else {
						initializing.Append("\t\t\tif (a_").Append(it.Name).Append(" != null) { ret.").Append(it.Name).Append(" = (").Append(propSyntaxInfo.ReturnFullTypeN).Append(")").Append(cast).Append("a_").Append(it.Name).Append("; }\n");
					}
				}
			}

			typ = typ.BaseType;
		}
		// オブジェクトを生成して値を渡す
		genText.Append("\t\t\t").Append(className).Append(" ret = new();\n");
		genText.Append(initializing);

		genText.Append($$"""
			return ret;
		}

""");
	}


	private static void ProcListElement(StringBuilder genText, PropertySyntaxInfo propSyntaxInfo) {
		switch (propSyntaxInfo.ReturnTypeElemN) {
		case "bool":
			genText.Append("jc.GetBool()");
			break;

		case "decimal":
			genText.Append("jc.GetDecimal()");
			break;

		case "double":
			genText.Append("jc.GetDouble()");
			break;

		case "float":
			genText.Append("(float)jc.GetDouble()");
			break;

		case "int":
			genText.Append("jc.GetInt32()");
			break;

		case "long":
			genText.Append("jc.GetInt64()");
			break;

		case "string":
			genText.Append("jc.GetString()");
			break;

		case "System.DateTime":
			genText.Append("jc.GetDateTime()");
			break;

		case "System.Guid":
			genText.Append("jc.GetGuid()");
			break;

		default:
			// オブジェクトの場合
			genText.Append('(').Append(propSyntaxInfo.ReturnTypeElemN).Append(')').Append(propSyntaxInfo.ReturnTypeElemN).Append(".Deserialize(jc)");
			break;
		}
	}


	/// <summary>
	/// Jsonをストリームに出力する
	/// </summary>
	/// <param name="tpl"></param>
	/// <param name="className"></param>
	/// <param name="piDic"></param>
	/// <param name="genText"></param>
	private static void OutputToJson(INamedTypeSymbol clsSymbol, Dictionary<string, PropertySyntaxInfo> piDic, StringBuilder genText) {
		genText.Append($$"""
		public void Serialize(System.IO.Stream s) {
			JsonHelper.Write(s, "{");
			int bx = 0;
			int cx = 0;

""");

		INamedTypeSymbol? typ = clsSymbol;
		while (typ != null) {
			foreach (var it in typ.GetMembers()) {
				// public でも internalでもないメンバは関係ない
				if (it.DeclaredAccessibility != Accessibility.Public && it.DeclaredAccessibility != Accessibility.Internal) { continue; }

				if (it is IPropertySymbol symbol) {
					string memberName = it.Name;
					string dataMemberName = GetDataMemberName(symbol, memberName);
					PropertySyntaxInfo propSyntaxInfo = new();
					propSyntaxInfo.SetReturnType(symbol.Type);

					switch (propSyntaxInfo.ReturnFullTypeN) {
					case "Bool":
					case "Decimal":
					case "Double":
					case "Float":
					case "Int":
					case "Long":
					case "bool":
					case "decimal":
					case "double":
					case "float":
					case "int":
					case "long":
						if (propSyntaxInfo.IsNullable) {
							genText.Append($$"""
			if ({{memberName}} != null) {
				if (bx > 0) { JsonHelper.Write(s, ","); }
				JsonHelper.Write(s, "\"{{dataMemberName}}\": ");
				JsonHelper.WriteValue(s, {{memberName}});
				++bx;
			}

""");
						} else {
							genText.Append($$"""
			if (bx > 0) { JsonHelper.Write(s, ","); }
			JsonHelper.Write(s, "\"{{dataMemberName}}\": ");
			JsonHelper.WriteValue(s, {{memberName}});
			++bx;

""");
						}
						break;

					case "String":
					case "string":
					case "System.DateTime":
					case "DateTime":
					case "System.Guid":
					case "Guid":
						if (propSyntaxInfo.IsNullable) {
							genText.Append($$"""
			if ({{memberName}} != null) {
				if (bx > 0) { JsonHelper.Write(s, ","); }
				JsonHelper.Write(s, "\"{{dataMemberName}}\": \"");
				JsonHelper.WriteValue(s, {{memberName}});
				JsonHelper.Write(s, "\"");
				++bx;
			}

""");
						} else {
							genText.Append($$"""
			if (bx > 0) { JsonHelper.Write(s, ","); }
			JsonHelper.Write(s, "\"{{dataMemberName}}\": \"");
			JsonHelper.WriteValue(s, {{memberName}});
			JsonHelper.Write(s, "\"");
			++bx;

""");
						}
						break;

					default:
						if (propSyntaxInfo.IsListReturnType || propSyntaxInfo.IsListReturnType) {
							// 配列 もしくは List の場合
							if (propSyntaxInfo.IsNullable) {
								genText.Append($$"""
			if ({{memberName}} != null) {
				if (bx > 0) { JsonHelper.Write(s, ","); }
				JsonHelper.Write(s, "\"{{dataMemberName}}\": [");
				cx = 0;
				foreach(var it in {{memberName}}) {
					if(cx > 0) {
						JsonHelper.Write(s, ",");
					}

""");
								if (propSyntaxInfo.IsBasicType) {
									genText.Append("					JsonHelper.WriteValue(s, it);\n");
								} else {
									genText.Append($"\t\t\t\t\tit.Serialize(s);;\n");
								}

								genText.Append($$"""
					++cx;
				}
				JsonHelper.Write(s, ']');
				++bx;
			}

""");
							} else {
								genText.Append($$"""
			if (bx > 0) { JsonHelper.Write(s, ","); }
			JsonHelper.Write(s, "\"{{dataMemberName}}\": [");
			cx = 0;
			foreach(var it in {{memberName}}) {
				if(cx > 0) {
					JsonHelper.Write(s, ",");
				}

""");
								if (propSyntaxInfo.IsBasicType) {
									genText.Append("				JsonHelper.WriteValue(s, it);\n");
								} else {
									genText.Append("\t\t\t\tit.Serialize(s);;\n");
								}
								genText.Append($$"""
				++cx;
			}
			JsonHelper.Write(s, ']');
			++bx;

""");
							}
						} else {
							// オブジェクトの場合
							if (propSyntaxInfo.IsNullable) {
								genText.Append($$"""
			if ({{memberName}} != null) {
				if (bx > 0) { JsonHelper.Write(s, ","); }
				JsonHelper.Write(s, "\"{{dataMemberName}}\": ");
				{{memberName}}.Serialize(s);
				++bx;
			}

""");
							} else {
								genText.Append($$"""
			if (bx > 0) { JsonHelper.Write(s, ","); }
			JsonHelper.Write(s, "\"{{dataMemberName}}\": ");
			{{memberName}}.Serialize(s);
			++bx;

""");
							}
						}
						break;
					}
				}
			}
			typ = typ.BaseType;
		}
		genText.Append("\t\t\tJsonHelper.Write(s, \"}\");\n");
		genText.Append("\t\t}\n");
	}


	private static string GetDataMemberName(IPropertySymbol symbol, string name) {
		foreach (var attr in symbol.GetAttributes()) {
			string attrName = attr.AttributeClass!.Name;
			if (attrName == "DataMemberAttribute") {
				if (attr.NamedArguments.Length > 0) {

					foreach (var ana in attr.NamedArguments) {
						if (ana.Key == "Name" && ana.Value.Value != null) {
							return (string)ana.Value.Value;
						}
					}
				}
			}
		}
		return name;

	}

	class EntityColumn {
		public string ColumnName = string.Empty;
		public string TypeName = string.Empty;
		public bool IsNullable = false;
		public string AttrName = string.Empty;
		public string SnakeCase1 = string.Empty;
		public string SnakeCase2 = string.Empty;

		public string ReaderTypeName {
			get {
				switch (TypeName) {
				case "Int":
					return "Int32";
				case "Long":
					return "Int64";
				case "Bool":
					return "Boolean";
				}
				return TypeName;
			}
		}
	}
	private static void OutputEntity(INamedTypeSymbol clsSymbol, string className, StringBuilder genText) {

		List<EntityColumn> entityColumns = new();

		bool withoutInternal = false;
		INamedTypeSymbol? typ = clsSymbol;
		while (typ != null) {
			foreach (var it in typ.GetMembers()) {
				if (it.DeclaredAccessibility != Accessibility.Public && (withoutInternal || it.DeclaredAccessibility != Accessibility.Internal)) { continue; }
				TypeSymbolInfo typeSymbol;
				if (it is IPropertySymbol propSymbol) {
					if (propSymbol.SetMethod == null) {
						continue;
					}
					typeSymbol = new(propSymbol.Type);

				} else if (it is IFieldSymbol fieldSymbol) {

					typeSymbol = new(fieldSymbol.Type);
				} else {
					continue;
				}
				if (typeSymbol.Name == "Object") { continue; }

				EntityColumn ec = new();
				ec.ColumnName = it.Name;
				ec.TypeName = typeSymbol.Name;
				ec.IsNullable = typeSymbol.IsNullable;
				object? o = GetAttribute(it, "ColumnNameAttribute");
				if (o == null) {
					ec.SnakeCase1 = ToSnakeCase(it.Name);
					ec.SnakeCase2 = ToSnakeCaseNum(ec.SnakeCase1);
				} else {
					ec.AttrName = o.ToString();
				}
				entityColumns.Add(ec);
			}

			withoutInternal = false;
			typ = typ.BaseType;
		}

		genText.Append($$"""
		public static {{className}} FromDbResult(System.Data.Common.DbDataReader r) {
			{{className}} v = new();
			int idx = 0;
			int z = r.FieldCount;
			Dictionary<string,int> cnd = new();
			for (int fx = 0; fx < z; ++fx) {
				cnd.Add(r.GetName(fx), fx);
			}

""");

		foreach (var it in entityColumns) {
			if (it.AttrName != string.Empty) {
				genText.Append("\t\t\tidx = DataHelper.GetOrdinal(cnd, \"").Append(it.AttrName).Append("\");\n");
			} else {
				if (it.ColumnName != it.SnakeCase1 && it.SnakeCase1 != it.SnakeCase2) {
					// 3つとも違う
					genText.Append("			idx = DataHelper.GetOrdinal(cnd, \"").Append(it.ColumnName).Append("\", \"").Append(it.SnakeCase1).Append("\", \"").Append(it.SnakeCase2).Append("\");\n");
				} else if (it.SnakeCase1 != it.ColumnName) {
					// snakeCase1 と snakeCase2 が同じ
					genText.Append("			idx = DataHelper.GetOrdinal(cnd, \"").Append(it.ColumnName).Append("\", \"").Append(it.SnakeCase1).Append("\");\n");
				} else if (it.SnakeCase1 != it.SnakeCase2) {
					// snakeCase1 と 元の名前 が同じ
					genText.Append("			idx = DataHelper.GetOrdinal(cnd, \"").Append(it.ColumnName).Append("\", \"").Append(it.SnakeCase2).Append("\");\n");
				} else {
					// ぜんぶ同じ
					genText.Append("			idx = DataHelper.GetOrdinal(cnd, \"").Append(it.ColumnName).Append("\");\n");
				}
			}

			if (it.IsNullable) {
				genText.Append("\t\t\tv.").Append(it.ColumnName).Append(" = idx < 0 ? null : r.Get").Append(it.ReaderTypeName).Append("(idx);\n");
			} else {
				genText.Append("\t\t\tif (idx >= 0 && !r.IsDBNull(idx)) { v.").Append(it.ColumnName).Append(" = r.Get").Append(it.ReaderTypeName).Append("(idx); }\n");
			}
		}
		genText.Append($$"""
			return v;
		}

		public static async System.Threading.Tasks.Task<System.Collections.Generic.List<{{className}}>> FromDbResultsAsync(System.Data.Common.DbDataReader r) {
			int z = r.FieldCount;
			Dictionary<string,int> cnd = new();
			for (int fx = 0; fx < z; ++fx) {
				cnd.Add(r.GetName(fx), fx);
			}

""");
		// 各カラムのインデックスを拾う処理
		foreach (var it in entityColumns) {
			if (it.AttrName != string.Empty) {
				genText.Append("\t\t\tint ").Append(it.ColumnName).Append("_idx = DataHelper.GetOrdinal(cnd, \"").Append(it.AttrName).Append("\");\n");
			} else {
				if (it.ColumnName != it.SnakeCase1 && it.SnakeCase1 != it.SnakeCase2) {
					// 3つとも違う
					genText.Append("			int ").Append(it.ColumnName).Append("_idx = DataHelper.GetOrdinal(cnd, \"").Append(it.ColumnName).Append("\", \"").Append(it.SnakeCase1).Append("\", \"").Append(it.SnakeCase2).Append("\");\n");
				} else if (it.SnakeCase1 != it.ColumnName) {
					// snakeCase1 と snakeCase2 が同じ
					genText.Append("			int ").Append(it.ColumnName).Append("_idx = DataHelper.GetOrdinal(cnd, \"").Append(it.ColumnName).Append("\", \"").Append(it.SnakeCase1).Append("\");\n");
				} else if (it.SnakeCase1 != it.SnakeCase2) {
					// snakeCase1 と 元の名前 が同じ
					genText.Append("			int ").Append(it.ColumnName).Append("_idx = DataHelper.GetOrdinal(cnd, \"").Append(it.ColumnName).Append("\", \"").Append(it.SnakeCase2).Append("\");\n");
				} else {
					// ぜんぶ同じ
					genText.Append("			int ").Append(it.ColumnName).Append("_idx = DataHelper.GetOrdinal(cnd, \"").Append(it.ColumnName).Append("\");\n");
				}
			}
		}

		genText.Append($$"""

			System.Collections.Generic.List<{{className}}> ret = new();
			while (await r.ReadAsync()) {
				{{className}} v = new();

""");

		// インデックスからカラムの値を拾う処理
		foreach (var it in entityColumns) {
			//if (it.IsNullable) {
			//	genText.Append("\t\t\t\tv.").Append(it.ColumnName).Append(" = (").Append(it.ColumnName).Append("_idx < 0 || r.IsDBNull(").Append(it.ColumnName).Append("_idx)) ? null : r.Get").Append(it.ReaderTypeName).Append('(').Append(it.ColumnName).Append("_idx);\n");
			//} else {
				genText.Append("\t\t\t\tif (").Append(it.ColumnName).Append("_idx >= 0 && !r.IsDBNull(").Append(it.ColumnName).Append("_idx)) { v.").Append(it.ColumnName).Append(" = r.Get").Append(it.ReaderTypeName).Append('(').Append(it.ColumnName).Append("_idx); }\n");
			//}
		}

		genText.Append("""
				ret.Add(v);
			}
			return ret;
		}

""");
	}


	private static object? GetAttribute(ISymbol symbol, string attributeName) {
		foreach (AttributeData ad in symbol.GetAttributes()) {
			if (ad.AttributeClass != null && ad.AttributeClass.Name == attributeName) {
				TypedConstant tc = ad.ConstructorArguments.First();
				return tc.Value;
			}
		}
		return null;
	}


	private static string ToSnakeCase(string camelCase) {
		var regex = new Regex("[A-Z]");

		// 先頭を小文字にする
		camelCase = camelCase.Substring(0, 1).ToLower() + camelCase.Substring(1);

		// 大文字の前にアンダースコアを入れて、全体を小文字化する
		return regex.Replace(camelCase, s => $"_{s.Groups[0].Value[0]}").ToLower();
	}


	private static string ToSnakeCaseNum(string camelCase) {
		var regex = new Regex("(?<=[A-Za-z])[0-9]");

		// 数字の前にアンダースコアを入れる
		return regex.Replace(camelCase, s => $"_{s.Groups[0].Value[0]}");
	}


	/// <summary>
	/// DbMapperインタフェースの実装クラスを出力
	/// </summary>
	/// <param name="clsSymbol"></param>
	/// <param name="className"></param>
	/// <param name="text"></param>
	private void OutDbMapper(INamedTypeSymbol clsSymbol, string accessor, string className, string interfaceName, string dbType, StringBuilder genText) {

		genText.Append($"\t{accessor}class {className}: {interfaceName} {{\n");

		int procIdx = 0;
		foreach (var it in clsSymbol.GetMembers()) {
			if (it is not IMethodSymbol methodSymbol) {
				continue;
			}

			MemberSyntaxInfo methodSyntaxInfo = new();
			methodSyntaxInfo.SetReturnType(methodSymbol.ReturnType);

			// Mapper系の属性を持っているか
			string? sql = GetAttrInfo(methodSymbol, out string attributeName);
			if (string.IsNullOrEmpty(sql)) {
				continue;
			}

			genText.Append($$"""
	{{GenSignature}}
		{{accessor}}async {{methodSyntaxInfo.ReturnFullType}} {{methodSymbol.Name}}(
""");

			// 引数を出力
			string argDbCommand = string.Empty;
			Dictionary<string, MethodArgument> maDic = new();
			List<MethodArgument> maList = new();
			int methodParamCount = 0;
			foreach (var methodParam in methodSymbol.Parameters) {
				if (methodParamCount > 0) {
					genText.Append(", ");
				}
				if (methodParam.Type.Name == "DbCommand") {
					argDbCommand = methodParam.Name;
				}
				MethodArgument ma = new(methodParamCount, methodParam.Name, methodParam.Type);
				maList.Add(ma);
				maDic.Add(methodParam.Name, ma);

				genText.Append(methodParam.ToDisplayString());
				++methodParamCount;
			}
			genText.Append(") {\n");

			// SQLに埋め込まれたパラメタの一覧を作る
			int cx = 0;
			var sqlBlocks = SqlBlock.SplitSql(sql!);
			foreach (var sqlBlock in sqlBlocks) {

				List<SqlParameter> spList = new();
				foreach (Match m in rgxSqlParam.Matches(sqlBlock.Sql)) {
					SqlParameter sp = new(m.Groups[0].Value, m.Index, m.Length, cx);
					spList.Add(sp);
					sqlBlock.Params.Add(sp);
					++cx;
				}
				if (spList.Count > 0) {
					// SQLを呼び出せる形に直す
					spList.Sort((a, b) => b.Index - a.Index);
					StringBuilder sbSql = new();
					foreach (var sp in spList) {
						string s1 = sqlBlock.Sql.Substring(0, sp.Index);
						string s2 = sp.Index + sp.Length < sqlBlock.Sql.Length ? sqlBlock.Sql.Substring(sp.Index + sp.Length, sqlBlock.Sql.Length - sp.Index - sp.Length) : string.Empty;
						sbSql.Append(s1);
						sbSql.Append("@_");
						sbSql.Append(sp.Seq);
						sbSql.Append(' ');
						sbSql.Append(s2);
						sqlBlock.Sql = sbSql.ToString();
						sbSql.Clear();
					}
				}
				// --コメントを除去
				sqlBlock.Sql = rgxComment.Replace(sqlBlock.Sql, "");
				// 連続した空白を除去
				sqlBlock.Sql = rgxWhiteSpace.Replace(sqlBlock.Sql, " ");

			}

			if (sqlBlocks.Count > 1) {
				// #ifあり
				// パラメータが見つかった場合
				if (argDbCommand != string.Empty) {
					// メソッドの引数にDbCommandがある
					genText.Append($$"""
			var gen_cmd = {{argDbCommand}};

""");
				} else {
					// コネクション・コマンド作成
					genText.Append($$"""
			await using var conn = await DbMapperConfigure.GetConnection();
			await using var gen_cmd = conn.CreateCommand();

""");
				}
				genText.Append("			System.Text.StringBuilder sbSql = new();\n");

				int sx = 0;
				foreach (var sqlBlock in sqlBlocks) {
					if (sqlBlock.Condition != string.Empty) {

						genText.Append("			if (" + sqlBlock.Condition + ") {\n");
						if (sqlBlock.Params.Count > 0) {
							foreach (var sp in sqlBlock.Params) {
								genText.Append("				AspNetCore.Csr.").Append(dbType).Append(".DataHelper.AddWithValue(gen_cmd, \"_").Append(sp.Seq).Append("\", ").Append(sp.Name).Append(");\n");
							}
						}
						genText.Append("				sbSql.Append(_sql").Append(procIdx).Append('_').Append(sx).Append(");\n");
						genText.Append("			}\n");

					} else {
						if (sqlBlock.Params.Count > 0) {
							foreach (var sp in sqlBlock.Params) {
								genText.Append("			AspNetCore.Csr.").Append(dbType).Append(".DataHelper.AddWithValue(gen_cmd, \"_").Append(sp.Seq).Append("\", ").Append(sp.Name).Append(");\n");
							}
						}
						genText.Append("			sbSql.Append(_sql").Append(procIdx).Append('_').Append(sx).Append(");\n");
					}
					++sx;
				}
				genText.Append("			gen_cmd.CommandText = sbSql.ToString();\n");
			} else {
				// #ifとか無い
				if (argDbCommand != string.Empty) {
					// メソッドの引数にDbCommandがある
					genText.Append($$"""
			var gen_cmd = {{argDbCommand}};
			gen_cmd.Parameters.Clear();
			gen_cmd.CommandText = _sql{{procIdx}};

""");
				} else {
					// コネクション・コマンド作成
					genText.Append($$"""
			await using var conn = await DbMapperConfigure.GetConnection();
			await using var gen_cmd = conn.CreateCommand();
			gen_cmd.CommandText = _sql{{procIdx}};

""");
				}
				var sqlBlock = sqlBlocks[0];
				if (sqlBlock.Params.Count > 0) {
					foreach (var sp in sqlBlock.Params) {
						genText.Append("			AspNetCore.Csr.").Append(dbType).Append(".DataHelper.AddWithValue(gen_cmd, \"_").Append(sp.Seq).Append("\", ").Append(sp.Name).Append(");\n");
					}
				}
			}

			// 実行して値をオブジェクトに入れるコードを出力する
			if (attributeName == "Select" || attributeName == "SelectAttribute") {
				genText.Append("			await using var res = await gen_cmd.ExecuteReaderAsync();\n");
				if (methodSyntaxInfo.IsListReturnType) {
					genText.Append("			return await ").Append(methodSyntaxInfo.ReturnTypeElemN).Append(".FromDbResultsAsync(res);\n");
				} else {
					genText.Append("			if (await res.ReadAsync()) {\n");
					genText.Append("				return ").Append(methodSyntaxInfo.ReturnTypeElemN).Append(".FromDbResult(res);\n");
					genText.Append("			}\n");
					genText.Append("			return null;\n");
				}
			} else if (attributeName == "Insert" || attributeName == "InsertAttribute" ||
					   attributeName == "Update" || attributeName == "UpdateAttribute" ||
					   attributeName == "Delete" || attributeName == "DeleteAttribute" ||
					   attributeName == "Execute" || attributeName == "ExecuteAttribute") {
				genText.Append("			return await gen_cmd.ExecuteNonQueryAsync();\n");
			}
			// メソッドブロックの終了
			genText.Append("		}\n\n");
			if (sqlBlocks.Count > 1) {
				int sx = 0;
				foreach (var sqlBlock in sqlBlocks) {
					genText.Append("		private static readonly string _sql").Append(procIdx).Append('_').Append(sx).Append(" = \"");
					genText.Append(sqlBlock.Sql);
					genText.Append("\";\n");
					++sx;
				}
				genText.Append("\n");
			} else {
				genText.Append("		private static readonly string _sql").Append(procIdx).Append(" = \"");
				genText.Append(sqlBlocks[0].Sql);
				genText.Append("\";\n\n");
			}
			++procIdx;
		}

		genText.Append("\t}\n");
	}

	private static string? GetAttrInfo(IMethodSymbol methodSymbol, out string attributeName) {

		// Mapperの属性を持っているか
		foreach (var a in methodSymbol.GetAttributes()) {
			if (a.AttributeClass != null) {
				var an = a.AttributeClass.Name;
				if (an == "SelectAttribute" || an == "UpdateAttribute" || an == "InsertAttribute" || an == "DeleteAttribute" || an == "ExecuteAttribute") {
					// ※ 複数属性持っていたら最初のやつをとる

					var attrDataCns = a.ConstructorArguments.First();
					attributeName = an;
					return attrDataCns.Value?.ToString();
				}
			}
		}

		attributeName = string.Empty;
		return null;

	}
}
