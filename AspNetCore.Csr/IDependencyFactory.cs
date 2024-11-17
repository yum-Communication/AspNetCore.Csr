using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Csr; 

public interface IDependencyFactory {
	object Create();
}

public interface IControllerMapper {
	void Add(WebApplication app);
}
