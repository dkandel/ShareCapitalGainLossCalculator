using Microsoft.AspNetCore.Mvc;

namespace ShareCapitalGainLossCalculator.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CalculatorsController: ControllerBase
{
    [HttpGet("version")]
    public IActionResult GetVersion()
    {
        return Ok("1.0");
    }
}
