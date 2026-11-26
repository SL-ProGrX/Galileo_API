using System.Data;
using Dapper;

namespace Galileo.DataBaseTier
{
    /// <summary>
    /// Reglas de permisos para resolver órdenes de compra, compartidas por las pantallas que las autorizan.
    /// </summary>
    internal static class CprOrdenPermisos
    {
        public const int CodigoSinPermiso = -2;
        public const string RutaNivelesAutorizacion = "Compras > Niveles de Autorización para el Proceso de Compras";

        public static string? ObtenerCodUnidad(IDbConnection conn, IDbTransaction? tx, string codOrden)
        {
            return conn.QueryFirstOrDefault<string>(
                @"SELECT TOP 1 COD_UNIDAD
                  FROM CPR_SOLICITUD
                  WHERE ADJUDICA_ORDEN = @CodOrden",
                new { CodOrden = codOrden },
                transaction: tx);
        }

        public static decimal ObtenerMontoOrden(IDbConnection conn, IDbTransaction tx, string codOrden)
        {
            return conn.QueryFirstOrDefault<decimal>(
                @"SELECT TOTAL
                  FROM CPR_ORDENES
                  WHERE COD_ORDEN = @CodOrden",
                new { CodOrden = codOrden },
                transaction: tx);
        }

        public static decimal ObtenerTipoCambio(IDbConnection conn, IDbTransaction tx)
        {
            var valor = conn.QueryFirstOrDefault<string>(
                @"SELECT VALOR
                  FROM SIF_PARAMETROS
                  WHERE COD_PARAMETRO = 'TC'",
                transaction: tx);

            return decimal.TryParse(valor, out var tipoCambio) ? tipoCambio : 0m;
        }

        /// <summary>
        /// Con UEN se exige un rango que cubra el monto; sin solicitud/UEN basta con ser autorizador.
        /// Devuelve null cuando el usuario cumple.
        /// </summary>
        public static string? FaltantePorUen(IDbConnection conn, IDbTransaction tx, string usuario, string? codUnidad, decimal montoDolares)
        {
            if (string.IsNullOrWhiteSpace(codUnidad))
            {
                return EsUsuarioAutorizador(conn, tx, usuario)
                    ? null
                    : "no está registrado como autorizador, requerido para órdenes sin solicitud ni UEN (pestaña Autorizadores)";
            }

            if (UsuarioEnRango(conn, tx, usuario, codUnidad, montoDolares))
                return null;

            var nombreUen = conn.QueryFirstOrDefault<string>(
                @"SELECT DESCRIPCION FROM CORE_UENS WHERE COD_UNIDAD = @CodUnidad",
                new { CodUnidad = codUnidad },
                transaction: tx);

            return $"falta un rango en la UEN {codUnidad} - {nombreUen} que cubra USD {montoDolares:N2} (pestañas Rangos y Usuarios por Rango)";
        }

        /// <summary>
        /// Valida que quien generó la orden esté a cargo del usuario. Devuelve null cuando cumple.
        /// </summary>
        public static string? FaltanteUsuarioACargo(IDbConnection conn, IDbTransaction tx, string usuario, string codOrden)
        {
            var generador = conn.QueryFirstOrDefault<string>(
                @"SELECT genera_user FROM cpr_ordenes WHERE cod_orden = @CodOrden",
                new { CodOrden = codOrden },
                transaction: tx);

            var aCargo = conn.QueryFirstOrDefault<int>(
                @"SELECT COUNT(*)
                  FROM cpr_orden_autousers
                  WHERE UPPER(usuario) = UPPER(@Usuario)
                    AND UPPER(usuario_asignado) = UPPER(@Generador)",
                new { Usuario = usuario, Generador = generador },
                transaction: tx);

            return aCargo > 0
                ? null
                : $"falta tener a cargo a {generador} (pestaña Usuarios a Cargo)";
        }

        public static bool EsUsuarioAutorizador(IDbConnection conn, IDbTransaction? tx, string usuario)
        {
            var count = conn.QueryFirstOrDefault<int>(
                @"SELECT COUNT(*)
                  FROM cpr_orden_autorizadores
                  WHERE usuario = @Usuario",
                new { Usuario = usuario },
                transaction: tx);

            return count > 0;
        }

        private static bool UsuarioEnRango(IDbConnection conn, IDbTransaction tx, string usuario, string uen, decimal montoDolares)
        {
            var rangos = conn.Query<(decimal MONTO_MINIMO, decimal MONTO_MAXIMO)>(
                @"SELECT r.MONTO_MINIMO, r.MONTO_MAXIMO
                  FROM cpr_orden_rangos r
                  JOIN CPR_RANGO_USUARIO u ON r.cod_rango = u.cod_rango
                  WHERE u.USUARIO = @Usuario AND u.ACTIVO = 1 AND u.UEN = @UEN",
                new { Usuario = usuario, UEN = uen },
                transaction: tx).ToList();

            return rangos.Any(r => montoDolares >= r.MONTO_MINIMO && montoDolares <= r.MONTO_MAXIMO);
        }
    }
}
