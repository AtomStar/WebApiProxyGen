using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebApiProxyGen.Metadata;

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
                var param = request.Content.ReadAsStringAsync().Result;
                var classNamespace = param.Split('=').Length > 1 ? param.Split('=')[1] : "";
                _metadataProvider.Namespace = classNamespace;
                var metadata = _metadataProvider.GetMetadata(request);
                return request.CreateResponse(System.Net.HttpStatusCode.OK, metadata);
            });
        }
    }
}
