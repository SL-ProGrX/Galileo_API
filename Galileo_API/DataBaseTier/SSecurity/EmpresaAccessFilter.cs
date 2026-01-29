using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Galileo.DataBaseTier
{
    public class EmpresaAccessFilter : IAsyncActionFilter
    {
        private readonly PerfilUsuarioDB _seguridad;
        private const string CodEmpresaKey = "CodEmpresa";

        public EmpresaAccessFilter(IConfiguration config)
        {
            _seguridad = new PerfilUsuarioDB(config);
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var isAuthenticated = context.HttpContext.User?.Identity?.IsAuthenticated ?? false;

            // sacar CodEmpresa del request (query/route/body)
            var codEmpresa = TryGetCodEmpresa(context);
            if (codEmpresa is null)
            {
                await next(); // endpoints que no usan empresa
                return;
            }

            // Si el endpoint usa empresa, no permitas acceso sin autenticación
            if (!isAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // sacar userId del token (sub)
            string? userIdStr = null;
            var user = context.HttpContext.User;
            if (user != null)
            {
                userIdStr = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            Console.WriteLine($"EmpresaAccessFilter -> sub={userIdStr}, codEmpresa={codEmpresa}");

            if (!int.TryParse(userIdStr, out var userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            // validar permiso
            if (!_seguridad.UsuarioTieneAccesoAEmpresa(userId, codEmpresa.Value))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Persistir el valor validado para uso en controllers/servicios
            context.HttpContext.Items[CodEmpresaKey] = codEmpresa.Value;
            context.HttpContext.Items["UserId"] = userId;

            // Opcional pero recomendado: sobreescribir argumentos para evitar que se use el valor original del request
            OverwriteEmpresaArgument(context, codEmpresa.Value);

            await next();
        }

        private static int? TryGetCodEmpresa(ActionExecutingContext context)
        {
            // query/route params
            if (TryGetIntFromArguments(context, "empresaCod", out var e1)) return e1;
            if (TryGetIntFromArguments(context, "codEmpresa", out var e2)) return e2;
            if (TryGetIntFromArguments(context, "CodEmpresa", out var e3)) return e3;

            // DTOs (body)
            var propertyNames = new[] { "CodEmpresa", "codEmpresa", "EmpresaCod", "empresaCod" };
            var val = context.ActionArguments.Values
                .Where(arg => arg is not null)
                .Select(arg => TryGetIntFromProperties(arg!, propertyNames))
                .FirstOrDefault(v => v.HasValue);
            if (val.HasValue) return val.Value;
            return null;
        }

        private static void OverwriteEmpresaArgument(ActionExecutingContext context, int codEmpresa)
        {
            // Keys comunes en actions
            var keys = new[] { "empresaCod", "codEmpresa", CodEmpresaKey };

            keys
                .Where(key => context.ActionArguments.ContainsKey(key))
                .ToList()
                .ForEach(key => context.ActionArguments[key] = codEmpresa);

            // Si viene dentro de un DTO, no lo reescribimos (evitamos reflection set). En ese caso usa HttpContext.Items[CodEmpresaKey].
        }

        private static bool TryGetIntFromArguments(ActionExecutingContext context, string key, out int value)
        {
            if (context.ActionArguments.TryGetValue(key, out var v) && v is not null)
            {
                var converted = TryConvertToInt(v);
                if (converted.HasValue)
                {
                    value = converted.Value;
                    return true;
                }
            }

            value = 0;
            return false;
        }

        private static int? TryGetIntFromProperties(object obj, string[] propertyNames)
        {
            if (obj == null) return null;

            var type = obj.GetType();

            var convertedValue = propertyNames
                 .Select(name => type.GetProperty(name))
                 .Where(prop => prop != null)
                 .Select(prop => prop != null ? prop.GetValue(obj) : null)
                 .Where(val => val != null)
                 .Select(val => TryConvertToInt(val!))
                 .FirstOrDefault(converted => converted.HasValue);


            if (convertedValue.HasValue)
            {
                return convertedValue.Value;
            }
            return null;
        }

        private static int? TryConvertToInt(object val)
        {
            if (val == null) return null;

            return val switch
            {
                int i => i,
                long l => (int)l,
                short s => s,
                byte b => b,
                string str when int.TryParse(str, out var parsed) => parsed,
                _ => null,
            };
        }
    }

}