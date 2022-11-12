using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Redis.Data;

namespace Redis.Controllers
{
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
        public ActionResult<string> GetField(string key, string field)
        {
            var value = _repo.GetField(key, field);

            if(value != null)
            {
                return Ok(value);
            }
            return NotFound();
        }
        [HttpPost]
        public ActionResult<string> SetField([FromBody]JObject data)
        {
            string key = data["key"].ToObject<string>();
            string value = data["value"].ToObject<string>();
            string field = data["field"].ToObject<string>();
            _repo.SetField(key, value, field);
            return Ok();
        }
    }
}
