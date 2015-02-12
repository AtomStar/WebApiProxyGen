using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Description;

namespace WebApiProxyGen.Metadata
{
    public class ApiControllerModel
    {
        public string ControllerName { get; set; }
        public List<ApiMethodModel> Methods { get; set; }

        public ApiControllerModel(string controllerName, List<ApiMethodModel> controllerMethods)
        {
            ControllerName = controllerName;
            Methods = controllerMethods;
        }
    }
}
