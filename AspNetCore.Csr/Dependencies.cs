using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Csr; 

public static class Dependencies {
	private static readonly Dictionary<string, IDependencyFactory> dependencies;

	static Dependencies() {
		dependencies = new();
	}


	public static object Resolve(string name) {
		if(dependencies.TryGetValue(name, out var factory)) {
			return factory.Create();
		}
		throw new KeyNotFoundException("Dependency name = " + name);
	}

	public static void Registration(string name, IDependencyFactory df) {
		dependencies.Add(name, df);
	}
}


public static class Controllers {
	private static readonly List<IControllerMapper> controllers;

	static Controllers() {
		controllers = new();
	}


	public static void Mapping(WebApplication app) {
		foreach(var c in controllers) {
			c.Add(app);
		}
	}

	public static void Registration(IControllerMapper df) {
		controllers.Add(df);
	}
}
