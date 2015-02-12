using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Description;

namespace WebApiProxyGen.Metadata
{
    public class ApiMethodModel
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public List<ApiParameterModel> Parameters { get; set; }

        public ApiMethodModel(ApiDescription apiDescription)
        {
            Method = apiDescription.HttpMethod.Method;
            Url = apiDescription.RelativePath.Substring(0, apiDescription.RelativePath.IndexOf("{") > 0 ? apiDescription.RelativePath.IndexOf("{") : apiDescription.RelativePath.Length).TrimEnd('/');
            ControllerName = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName;
            ActionName = apiDescription.ActionDescriptor.ActionName;
            Parameters = apiDescription.ParameterDescriptions.Select(pd => new ApiParameterModel(pd)).ToList();
        }
    }
}
