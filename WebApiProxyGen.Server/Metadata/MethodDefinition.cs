
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace WebApiProxyGen.Metadata
{
     public class MethodDefinition
    {
        public string ModelName { get; set; }
        public string Name { get; set; }
        public string Static { get; set; }
        public List<ParameterDefinition> Parameters { get; set; }
        public string ReturnType { get; set; }
        public string ParameterSignature { get; set; }
        public string ParameterNameSignature { get; set; }

        public MethodDefinition(MethodInfo mi, string modelName)
        {
            this.ModelName = modelName;
            this.Name = mi.Name;
            this.Static = "";
            if (mi.Name.StartsWith("Get"))
                this.Static = "static";
            this.ReturnType = MetadataProvider.ParseType2(mi.ReturnType);
            this.Parameters = new List<ParameterDefinition>();
            var parameters = mi.GetParameters();
            foreach (var item in parameters)
            {
                var parameter = new ParameterDefinition();
                parameter.Type = MetadataProvider.ParseType2(item.ParameterType);
                parameter.Name = item.Name;
                if (parameter.Type.ToLower() == this.ModelName.ToLower())
                    parameter.Name = "this";
                parameter.Description = "";
                this.Parameters.Add(parameter);
               
            }
            BuildParamSignatures();
        }
        public void BuildParamSignatures()
        {
            this.ParameterSignature = "";
            this.ParameterNameSignature = "";
            foreach (var item in this.Parameters)
            {
                if (item.Name == "this")
                    this.ParameterSignature += item.Name;
                else
                {
                    this.ParameterSignature += item.Type + " " + item.Name + ",";
                    this.ParameterNameSignature += item.Name + ",";
                }
            }
            this.ParameterSignature = this.ParameterSignature.TrimEnd(',');
            this.ParameterNameSignature = this.ParameterNameSignature.TrimEnd(',');
        }

    }
   
}