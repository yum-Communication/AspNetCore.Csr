using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Csr; 

public static class Dependencies {
	private static readonly Dictionary<string, DependencyFactory> dependencies;

	static Dependencies() {
		dependencies = new();
	}


	public static object Resolve(string name) {
		if(dependencies.TryGetValue(name, out var factory)) {
			return factory.Create();
		}
		throw new KeyNotFoundException("Dependency name = " + name);
	}

	public static void Registration(string name, DependencyFactory df) {
		dependencies.Add(name, df);
	}
}


public static class Controllers {
	private static readonly List<ControllerMapper> controllers;

	static Controllers() {
		controllers = new();
	}


	public static void Mapping(WebApplication app) {
		foreach(var c in controllers) {
			c.Add(app);
		}
	}

	public static void Registration(ControllerMapper df) {
		controllers.Add(df);
	}
}
