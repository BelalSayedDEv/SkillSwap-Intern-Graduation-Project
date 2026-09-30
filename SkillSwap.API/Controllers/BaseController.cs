namespace SkillSwap.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Common.Responses;

[Route("api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<T>.Success(result.Data!));
        }

        var errorResponse = ApiResponse<T>.Failure(result.ErrorMessage);

        return result.ErrorType switch
        {
            ErrorType.NotFound => NotFound(errorResponse),
            ErrorType.Conflict => Conflict(errorResponse),
            ErrorType.Validation => BadRequest(errorResponse),
            ErrorType.Unauthorized => Unauthorized(errorResponse),
            _ => BadRequest(errorResponse)
        };
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<string>.Success("Operation completed successfully"));
        }

        var errorResponse = ApiResponse<string>.Failure(result.ErrorMessage);

        return result.ErrorType switch
        {
            ErrorType.NotFound => NotFound(errorResponse),
            ErrorType.Conflict => Conflict(errorResponse),
            ErrorType.Validation => BadRequest(errorResponse),
            ErrorType.Unauthorized => Unauthorized(errorResponse),
            _ => BadRequest(errorResponse)
        };
    }
}
