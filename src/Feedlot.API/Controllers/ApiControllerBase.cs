using Feedlot.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Feedlot.API.Controllers;

/// <summary>
/// Controller base con helpers para convertir Result&lt;T&gt; en IActionResult.
/// Centraliza el mapa de ResultErrorType → código HTTP para que
/// los controllers no repitan la lógica de conversión.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.ErrorType switch
        {
            ResultErrorType.NotFound => NotFound(new { error = result.Error }),
            ResultErrorType.Conflict => Conflict(new { error = result.Error }),
            ResultErrorType.Validation => BadRequest(new { error = result.Error }),
            ResultErrorType.BusinessRule => UnprocessableEntity(new { error = result.Error }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error })
        };
    }

    protected IActionResult FromResult(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return result.ErrorType switch
        {
            ResultErrorType.NotFound => NotFound(new { error = result.Error }),
            ResultErrorType.Conflict => Conflict(new { error = result.Error }),
            ResultErrorType.BusinessRule => UnprocessableEntity(new { error = result.Error }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error })
        };
    }

    protected IActionResult CreatedFromResult<T>(Result<T> result, string routeName, object routeValues)
    {
        if (result.IsSuccess)
            return CreatedAtRoute(routeName, routeValues, result.Value);

        return FromResult(result);
    }
}
