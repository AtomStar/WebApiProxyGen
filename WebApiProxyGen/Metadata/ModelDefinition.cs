using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiProxyGen.Metadata
{
    public class ModelDefinition
    {
        public string Name { get; set; }
        public ModelProperty ControllerProperty { get; set; }
        public IEnumerable<ModelProperty> Properties { get; set; }
        public List<MethodDefinition> Methods { get; set; }
       
    }

    public class ModelProperty
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
