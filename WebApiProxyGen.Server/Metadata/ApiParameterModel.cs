using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Description;

namespace WebApiProxyGen.Metadata
{
    public class ApiParameterModel
    {
        public string Type { get; set; }
        public List<string> TypeProperties { get; set; }
        public List<string> TypePropertiesLower { get; set; }
        public string Name { get; set; }
        public bool IsUriParameter { get; set; }
        public bool IsClass { get; set; }
        public bool IsArray { get; set; }

        public ApiParameterModel(ApiParameterDescription apiParameterDescription)
        {
            Type = apiParameterDescription.ParameterDescriptor.ParameterType.Name;
            TypeProperties = apiParameterDescription.ParameterDescriptor.ParameterType.GetProperties().Select(k => k.Name).ToList();
            TypePropertiesLower = TypeProperties.Select(a => a[0].ToString().ToLower() + a.Substring(1)).ToList();
            Name = apiParameterDescription.Name;
            IsUriParameter = apiParameterDescription.Source == ApiParameterSource.FromUri;
            IsClass = !apiParameterDescription.ParameterDescriptor.ParameterType.FullName.Contains("System");
            IsArray = apiParameterDescription.ParameterDescriptor.ParameterType.IsArray;
        }
    }
}
