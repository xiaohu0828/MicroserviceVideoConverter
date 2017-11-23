using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Results;
using MicroServiceCommon.Models;

namespace IntegrationMiddlewareMicroService.Controllers
{
    using System.Web.Http;

    [RoutePrefix("MicroserviceVideoConverter")]
    public class IntegrationMiddlewareController : ApiController
    {
        private readonly Context _context;
        private readonly PubAndSub _pubAndSub;

        public IntegrationMiddlewareController()
        {
            _context = new Context();
        }

        [Route("api/get/{id}")]
        public IEnumerable<string> Get(int id)
        {
            return new List<string>();
        }

        [Route("api/upload")]
        public IHttpActionResult Post([FromBody]VideoFile file)
        {
            if (file == null)
            {
                return BadRequest();
            }

            _context.Add(file);
            _pubAndSub.SendMessage(_context.VideoFiles.Last());

            return new ResponseMessageResult(new HttpResponseMessage(HttpStatusCode.Accepted));
        }
    }
}
