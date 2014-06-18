using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WebApiProxyGen.Metadata
{
	public class ControllerDefinition
	{
		public string Name { get; set; }

        public string Description { get; set; }

        public IEnumerable<ActionMethodDefinition> ActionMethods { get; set; }
 
    }
}
