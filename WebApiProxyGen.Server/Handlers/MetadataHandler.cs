using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebApiProxyGen.Metadata;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace WebApiProxyGen.Handlers
{
    public class MetadataHandler : DelegatingHandler
    {

        private MetadataProvider _metadataProvider;
        public MetadataHandler()
        {
            _metadataProvider = new MetadataProvider();
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var queryString = request.Content.ReadAsStringAsync().Result;
                var parameters = HttpUtility.ParseQueryString(queryString);
                var classNamespace = !String.IsNullOrWhiteSpace(parameters["Namespace"]) ? parameters["Namespace"] : "WebProxy";
                var type = !String.IsNullOrWhiteSpace(parameters["Type"]) ? parameters["Type"] : "CS";
                _metadataProvider.Namespace = classNamespace;
                if (type == "CS")
                {
                    var metadata = _metadataProvider.GetMetadata(request);
                    return request.CreateResponse(System.Net.HttpStatusCode.OK, metadata);
                }
                else
                {
                    var metadata = _metadataProvider.GetApiControllers();
                    return request.CreateResponse(System.Net.HttpStatusCode.OK, metadata);
                }

                    
            });
        }
    }
}
