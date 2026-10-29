using System.Security.Claims;

namespace Galileo_API.Extensions;

/// <summary>
/// Lectura de la identidad autenticada a partir de los claims del token de acceso.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    private const string UserNameClaim = "UserName";

    /// <summary>
    /// Devuelve el usuario de la sesión autenticada. Los endpoints deben usar este valor
    /// y no el enviado por el cliente para registrar quién ejecuta la operación.
    /// </summary>
    /// <param name="user">Identidad de la solicitud (<c>ControllerBase.User</c>).</param>
    /// <returns>Nombre de usuario del claim UserName; vacío si no existe.</returns>
    public static string ObtenerUsuario(this ClaimsPrincipal user)
    {
        return user.FindFirst(UserNameClaim)?.Value ?? string.Empty;
    }
}
