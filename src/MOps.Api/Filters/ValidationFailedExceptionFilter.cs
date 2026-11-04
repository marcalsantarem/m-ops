using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MOps.Core.Services;

namespace MOps.Api.Filters;

/// <summary>Converte <see cref="ValidationFailedException"/> dos serviços em 400 com ValidationProblemDetails.</summary>
public sealed class ValidationFailedExceptionFilter(ProblemDetailsFactory problemDetailsFactory) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ValidationFailedException ex) return;

        var modelState = new ModelStateDictionary();
        foreach (var (field, messages) in ex.Errors)
            foreach (var message in messages)
                modelState.AddModelError(field, message);

        var problem = problemDetailsFactory.CreateValidationProblemDetails(
            context.HttpContext, modelState, StatusCodes.Status400BadRequest);
        context.Result = new BadRequestObjectResult(problem);
        context.ExceptionHandled = true;
    }
}
