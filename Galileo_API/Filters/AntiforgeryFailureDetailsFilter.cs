using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Galileo_API.Filters;

public sealed class AntiforgeryFailureDetailsFilter : IAsyncAlwaysRunResultFilter
{
    public const int FilterOrder = -3000;

    public Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        if (context.Result is IAntiforgeryValidationFailedResult)
        {
            var problem = new ProblemDetails
            {
                Title = "Solicitud rechazada",
                Detail = "No se pudo validar el token anti-CSRF. Recargue la página y vuelva a intentarlo.",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.HttpContext.Request.Path,
            };
            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

            context.Result = new ObjectResult(problem)
            {
                StatusCode = StatusCodes.Status400BadRequest,
            };
        }

        return next();
    }
}
