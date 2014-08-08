using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;


namespace WebApiProxyGen.Metadata
{

    public class ExcludeProxy : Attribute
    {
    }
    public static class TypeExtensions
    {
        public static bool IsExcluded(this Type type)
        {
            return type.GetCustomAttributes(true).OfType<ExcludeProxy>().Any();
        }
    }
    public class MetadataProvider
    {
        public string Namespace { get; set; }
        List<ModelDefinition> models = new List<ModelDefinition>();

        public Metadata GetMetadata(HttpRequestMessage request)
        {
            models = new List<ModelDefinition>();
            var host = request.RequestUri.Scheme + "://" + request.RequestUri.Authority;
            var descriptions = GlobalConfiguration.Configuration.Services.GetApiExplorer().ApiDescriptions;
            var documentationProvider = GlobalConfiguration.Configuration.Services.GetDocumentationProvider();

            ILookup<HttpControllerDescriptor, ApiDescription> apiGroups = descriptions.ToLookup(api => api.ActionDescriptor.ControllerDescriptor);

            var descripList = descriptions.ToList();
            var desc = descripList[0].ActionDescriptor.ActionName;
            var metadata = new Metadata
            {
                Definitions = from d in apiGroups
                              where !d.Key.ControllerType.IsExcluded()
                              select new ControllerDefinition
                              {
                                  Name = d.Key.ControllerName,
                                  Description = documentationProvider == null ? "" : documentationProvider.GetDocumentation(d.Key) ?? "",
                                  ActionMethods = from a in descriptions
                                                  where !a.ActionDescriptor.ControllerDescriptor.ControllerType.IsExcluded()
                                                  && a.ActionDescriptor.ControllerDescriptor.ControllerName == d.Key.ControllerName
                                                  select new ActionMethodDefinition
                                                  {
                                                      Name = a.ActionDescriptor.ActionName,
                                                      BodyParameter = (from b in a.ParameterDescriptions
                                                                       where b.Source == ApiParameterSource.FromBody
                                                                       select new ParameterDefinition
                                                                       {
                                                                           Name = b.ParameterDescriptor.ParameterName,
                                                                           Type = ParseType(b.ParameterDescriptor.ParameterType),
                                                                           Description = b.Documentation ?? ""
                                                                       }).FirstOrDefault(),
                                                      UrlParameters = from b in a.ParameterDescriptions
                                                                      where b.Source == ApiParameterSource.FromUri
                                                                      && b.ParameterDescriptor != null
                                                                      select new ParameterDefinition
                                                                      {
                                                                          Name = b.ParameterDescriptor.ParameterName ?? "",
                                                                          Type = ParseType(b.ParameterDescriptor.ParameterType ?? typeof(string)),
                                                                          Description = b.Documentation ?? ""
                                                                      },
                                                      Url = a.RelativePath,

                                                      Description = a.Documentation ?? "",
                                                      ReturnType = ParseType(a.ActionDescriptor.ReturnType),
                                                      Type = a.HttpMethod.Method
                                                  }
                              },
                Models = models,
                Host = host
            };


            return metadata;

        }

        private string ParseType(Type type)
        {
            string res;

            if (type == null)
                return "";

            //If the type is a generic type format to correct class name
            if (type.IsGenericType)
            {
                res = type.Name;

                int index = res.IndexOf('`');

                if (index > -1)
                    res = res.Substring(0, index);

                Type[] args = type.GetGenericArguments();

                res += "<";

                for (int i = 0; i < args.Length; i++)
                {
                    if (i > 0)
                        res += ", ";
                    //Recursivly find nested arguments
                    res += ParseType(args[i]);
                }
                res += ">";
            }
            else
            {
                if (type.ToString().StartsWith("System."))
                {
                    if (type.ToString().Equals("System.Void"))
                        res = "void";
                    else
                        res = type.Name;
                }
                else
                {
                    if (type.Name.EndsWith("Controller"))
                        res = this.Namespace + ".Clients." + type.Name;
                    else
                        res = this.Namespace + ".Models." + type.Name;
                    if (!models.Any(c => c.Name.Equals(type.Name)) && !type.IsInterface)
                        AddModelDefinition(type);
                }
            }

            return res;
        }
        public static string ParseType2(Type type, string nameSpace)
        {
            string res;

            if (type == null)
                return "";

            //If the type is a generic type format to correct class name
            if (type.IsGenericType)
            {
                res = type.Name;

                int index = res.IndexOf('`');

                if (index > -1)
                    res = res.Substring(0, index);

                Type[] args = type.GetGenericArguments();

                res += "<";

                for (int i = 0; i < args.Length; i++)
                {
                    if (i > 0)
                        res += ", ";
                    //Recursivly find nested arguments
                    res += ParseType2(args[i], nameSpace);
                }
                res += ">";
            }
            else
            {
                res = type.Name;
                if (type.ToString().Equals("System.Void"))
                    res = "void";
                if (!type.FullName.StartsWith("System"))
                {
                    if (type.Name.EndsWith("Controller"))
                        res = nameSpace + ".Clients." + type.Name;
                    else
                        res = nameSpace + ".Models." + type.Name;
                }
            }
            return res;
        }
        private void AddModelDefinition(Type classToDef)
        {
            //When the class is an array redefine the classToDef as the array type
            if (classToDef.IsArray)
            {
                classToDef = classToDef.GetElementType();
            }
            var interfaces = classToDef.GetInterfaces();
            foreach (var item in interfaces)
            {
                if (item.IsGenericType)
                {
                    var name = item.Name.Substring(0, item.Name.IndexOf('`'));
                    var methods = item.GetMethods();
                }
            }
            //If the class has not been mapped then map into metadata
            if (!models.Any(c => c.Name.Equals(classToDef.Name)))
            {

                ModelDefinition model = new ModelDefinition();

                var properties = classToDef.GetProperties();

                model.Name = classToDef.Name;

                model.Properties = from property in properties
                                   select new ModelProperty
                                   {
                                       Name = property.Name,
                                       Type = ParseType(property.PropertyType)
                                   };
                model.ControllerProperty = model.Properties.FirstOrDefault(p => p.Type.EndsWith("Controller"));
                var modelControllerProp = properties.FirstOrDefault(p => p.PropertyType.Name.EndsWith("Controller"));
                model.Methods = new List<MethodDefinition>();
                if (modelControllerProp != null)
                {
                    model.Methods = GetInterfaceMethods(modelControllerProp.PropertyType, model.Name, this.Namespace);

                    var controllerMethods = GetControllerMethods(modelControllerProp.PropertyType, this.Namespace);
                    foreach (var item in controllerMethods)
                    {
                        if (!model.Methods.Exists(p => p.Name == item.Name && p.ReturnType == item.ReturnType && p.ParameterSignature == p.ParameterSignature))
                            model.Methods.Add(item);
                    }
                }
                //Exclude controller classes to be added to the proxy
                if (!model.Name.EndsWith("Controller"))
                    models.Add(model);
            }
        }
        private static List<MethodDefinition> GetInterfaceMethods(Type t, string modelName, string nameSpace)
        {
            var methodDefs = new List<MethodDefinition>();
            var interfaces = t.GetInterfaces();
            foreach (var item in interfaces)
            {
                if (item.IsGenericType)
                {
                    var name = item.Name.Substring(0, item.Name.IndexOf('`'));
                    var methods = item.GetMethods();
                    foreach (var method in methods)
                    {
                        var methodDef = new MethodDefinition(method, modelName, nameSpace);
                        methodDefs.Add(methodDef);
                    }
                }
            }
            return methodDefs;
        }
        private static List<MethodDefinition> GetControllerMethods(Type t, string nameSpace)
        {
            var methodDefs = new List<MethodDefinition>();
            var methods = t.GetMethods().Where(p => p.DeclaringType.Name == t.Name);
            foreach (var method in methods)
            {
                var methodDef = new MethodDefinition(method, "", nameSpace);
                methodDef.Static = "static";
                methodDefs.Add(methodDef);
            }
            return methodDefs;
        }


    }

}