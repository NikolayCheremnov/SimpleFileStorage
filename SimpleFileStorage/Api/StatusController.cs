using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SimpleFileStorage.Api
{
    [Route("api")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpGet]
        public StringMessage Status()
        {
            return new StringMessage(Message: "server is running");
        }

        [HttpGet("ping")]
        public StringMessage Ping()
        {
            return new StringMessage(Message: "pong");
        }
    }
}
