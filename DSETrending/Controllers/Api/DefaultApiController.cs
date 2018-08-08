using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSETrending.Controllers.Api
{
    //[Route("api/[controller]")]
    public class DefaultApiController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetTrades()
        {
            var result = AppManager.GetTrades(1);
            return Ok(result);
        }

        [HttpGet]
        public IHttpActionResult Now()
        {
            var result = AppManager.GetNowTrades();
            return Ok(result);
        }
    }
}
