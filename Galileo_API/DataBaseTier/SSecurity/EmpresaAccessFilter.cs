using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Galileo.DataBaseTier
{
    public class EmpresaAccessFilter : IAsyncActionFilter
    {
        private readonly PerfilUsuarioDB _seguridad;

        public EmpresaAccessFilter(IConfiguration config)
        {
            _seguridad = new PerfilUsuarioDB(config);
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // si no está autenticado, lo gestiona [Authorize]
            if (!(context.HttpContext.User?.Identity?.IsAuthenticated ?? false))
            {
                await next();
                return;
            }

            // sacar CodEmpresa del request
            var codEmpresa = TryGetCodEmpresa(context);
            if (codEmpresa is null)
            {
                await next(); // endpoints que no usan empresa
                return;
            }

            // sacar userId del token (sub)
            var userIdStr = context.HttpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                         ?? context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
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

            await next();
        }

        private static int? TryGetCodEmpresa(ActionExecutingContext context)
        {
            // query/route params
            if (TryGetIntFromArguments(context, "empresaCod", out var e1)) return e1;
            if (TryGetIntFromArguments(context, "codEmpresa", out var e2)) return e2;
            if (TryGetIntFromArguments(context, "CodEmpresa", out var e3)) return e3;

            // DTOs (body)
            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg is null) continue;
                var val = TryGetIntFromProperties(arg, new[] { "CodEmpresa", "codEmpresa", "EmpresaCod", "empresaCod" });
                if (val.HasValue) return val.Value;
            }
            return null;
        }

        private static bool TryGetIntFromArguments(ActionExecutingContext context, string key, out int value)
        {
            if (context.ActionArguments.TryGetValue(key, out var v) && v is int i)
            {
                value = i;
                return true;
            }
            value = 0;
            return false;
        }

        private static int? TryGetIntFromProperties(object obj, string[] propertyNames)
        {
            if (obj == null) return null;

            var type = obj.GetType();

            foreach (var name in propertyNames)
            {
                var prop = type.GetProperty(name);
                if (prop == null) continue;

                var val = prop.GetValue(obj);
                if (val != null)
                {
                    var converted = TryConvertToInt(val);
                    if (converted.HasValue) return converted.Value;
                }
            }

            return null;
        }

        private static int? TryConvertToInt(object val)
        {
            if (val == null) return null;

            switch (val)
            {
                case int i:
                    return i;
                case long l:
                    return (int)l;
                case short s:
                    return s;
                case byte b:
                    return b;
                case string str when int.TryParse(str, out var parsed):
                    return parsed;
                default:
                    return null;
            }
        }
    }

}