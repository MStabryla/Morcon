using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Route("data")]
[ApiController]
public class DataController : ControllerBase
{
    public IActionResult Get()
    {
        var data = new
        {
            Message = "Hello from the DataController!",
            Timestamp = DateTime.UtcNow
        };
        return Ok(data);
    }
}
