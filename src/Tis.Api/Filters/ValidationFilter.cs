using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Tis.Api.Filters;

// Hər action çağırılmazdan əvvəl onun DTO parametrlərini uyğun doğrulayıcı ilə yoxlayır.
// Xəta varsa 400 qaytarır: { "message": "ilk xəta", "errors": { "sahə": ["xəta", ...] } }.
// Controller-də ayrıca yoxlama yazmağa ehtiyac yoxdur, DTO üçün doğrulayıcı yazmaq kifayətdir.
public class ValidationFilter(IServiceProvider services) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new Dictionary<string, List<string>>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (services.GetService(validatorType) is not IValidator validator) continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument), context.HttpContext.RequestAborted);
            foreach (var failure in result.Errors)
            {
                var field = char.ToLowerInvariant(failure.PropertyName[0]) + failure.PropertyName[1..];
                if (!errors.TryGetValue(field, out var list))
                    errors[field] = list = new List<string>();
                list.Add(failure.ErrorMessage);
            }
        }

        if (errors.Count > 0)
        {
            var firstMessage = errors.Values.First().First();
            context.Result = new BadRequestObjectResult(new { statusCode = 400, message = firstMessage, errors });
            return;
        }

        await next();
    }
}
