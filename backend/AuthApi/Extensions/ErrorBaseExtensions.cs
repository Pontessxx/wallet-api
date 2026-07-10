namespace AuthApi.Extensions;

public static class ErrorBaseExtensions
{
    public static IActionResult UnauthorizedError(this ControllerBase controller, string message)
        => controller.Unauthorized(new ResponseError(message));

    public static IActionResult BadRequestError(this ControllerBase controller, string message)
        => controller.BadRequest(new ResponseError(message));

    public static IActionResult NotFoundError(this ControllerBase controller, string message)
        => controller.NotFound(new ResponseError(message));
}
