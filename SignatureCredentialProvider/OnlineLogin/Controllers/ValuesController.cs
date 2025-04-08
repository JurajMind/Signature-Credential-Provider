using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;



namespace OnlineLogin.Controllers
{
   // [Authorize]
    public class ValuesController : ApiController
    {

        // GET api/values
        public IEnumerable<string> Get()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\token.txt", true))
            {
                file.WriteLine("Fourth line");
            }
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }
        [HttpGet]
        public string Login(string token)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\token.txt", true))
            {
                file.WriteLine("Fourth line");
            }
            return "OK";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
