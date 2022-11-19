using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Redis.Data;

namespace Redis.Controllers
{
    /*
     Database names:

        db_arch_prod:
            address: "127.0.0.1:9001"
            dbNumber: DB0

        db_curr_prod:
            address: "127.0.0.1:9001"
            dbNumber: DB1

        db_arch_sales:
            address: "127.0.0.1:9001"
            dbNumber: DB2

        db_curr_sales:
            address: "127.0.0.1:9001"
            dbNumber: DB3

     */
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly IDatabaseCommunication _repo;

        public DataController(IDatabaseCommunication repo)
        {
            _repo = repo;
        }

        [HttpGet("{key}/{field}")]
        public ActionResult<string> GetField(string key, string field, [FromBody] JObject data)
        {
            string db = data["database"].ToObject<string>();
            string value = _repo.GetField(key, field, db);
            if(value != null)
            {
                return Ok(value);
            }
            return NotFound();
        }
        [HttpPost]
        public ActionResult<string> SetData([FromBody]JObject data)
        {
            string key = data["key"].ToObject<string>();
            string db = data["database"].ToObject<string>();
            string[] value = data["value"].ToObject<string[]>();
            string[] field = data["field"].ToObject<string[]>();
            _repo.SetData(key, value, field, db);
            return Ok();
        }
        [HttpDelete("{key}")]
        public ActionResult DeleteData(string key, [FromBody] JObject data)
        {
            string db = data["database"].ToObject<string>();
            _repo.DeleteData(key, db);
            return NoContent();
        }
    }
}
