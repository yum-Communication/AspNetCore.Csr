using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Csr; 

public interface DependencyFactory {
	object Create();
}

public interface ControllerMapper {
	void Add(WebApplication app);
}
