using Microsoft.AspNetCore.Mvc;

namespace MyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { 
            message = "Hello from .NET 9!", 
            timestamp = DateTime.UtcNow 
        });
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        return Ok(new { 
            id = id, 
            name = $"Item {id}" 
        });
    }

    [HttpPost]
    public IActionResult Post([FromBody] object data)
    {
        return Ok(new { 
            success = true, 
            receivedData = data 
        });
    }
}