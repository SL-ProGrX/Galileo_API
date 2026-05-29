using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFBitacoraEspecialDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1;

        private const string SqlMovimientosBitacora = @"
                    SELECT MOVIMIENTO AS item,
                           DESCRIPCION AS descripcion
                    FROM dbo.US_MOVIMIENTOS_BE
                    WHERE MODULO = @Modulo
                    ORDER BY MOVIMIENTO;";

        private const string SqlRevisarBitacora = @"
                    UPDATE dbo.AFI_BITACORA_ESPECIAL
                    SET revisado_usuario = @Usuario,
                        revisado_fecha = dbo.MyGetdate()
                    WHERE id_Bitacora = @IdBitacora;";

        private const string SqlBusquedaSocios = @"
                    SELECT Cedula AS item,
                           Nombre AS descripcion
                    FROM dbo.Socios
                    ORDER BY Nombre;";

        private const string SqlBusquedaUsuarios = @"
                    SELECT Nombre AS item,
                           Descripcion AS descripcion
                    FROM dbo.Usuarios
                    ORDER BY Nombre;";

        private const string SqlBitacoraEspecialConsulta = @"
                    SELECT
                        C.*,
                        S.cedula,
                        S.nombre,
                        M.Descripcion AS MovimientoDesc,
                        CASE WHEN C.revisado_fecha IS NULL THEN 0 ELSE 1 END AS Revisado
                    FROM dbo.Afi_Bitacora_especial C
                    INNER JOIN dbo.Socios S
                        ON S.cedula = C.cedula
                    INNER JOIN dbo.US_MOVIMIENTOS_BE M
                        ON C.Movimiento = M.Movimiento
                    WHERE M.Modulo = @Modulo
                      AND (@Cedula = '' OR C.cedula LIKE @Cedula)
                      AND (@AplicarFechas = 0 OR
                          (@UsarFechaRevision = 1 AND C.Revisado_fecha BETWEEN @FechaInicio AND @FechaCorte) OR
                          (@UsarFechaRevision = 0 AND C.fecha BETWEEN @FechaInicio AND @FechaCorte))
                      AND (@AplicarMovimientos = 0 OR C.Movimiento IN @Movimientos)
                      AND (@AplicarUsuario = 0 OR
                          (@UsarFechaRevision = 1 AND C.Revisado_Usuario = @Usuario) OR
                          (@UsarFechaRevision = 0 AND C.Usuario = @Usuario))
                      AND (@Revision = '' OR
                          (@Revision = 'P' AND C.Revisado_Fecha IS NULL) OR
                          (@Revision = 'R' AND C.Revisado_Fecha IS NOT NULL) OR
                          (@Revision = 'T'))
                    ORDER BY
                        CASE WHEN @UsarFechaRevision = 1 THEN C.Revisado_fecha END,
                        CASE WHEN @UsarFechaRevision = 0 THEN C.fecha END;";

        public FrmAFBitacoraEspecialDB(IConfiguration? config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene los tipos de movimiento de la bitácora especial para el módulo de clientes.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de movimientos disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_BitacoraEspecialMov_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlMovimientosBitacora,
                new { Modulo = vModulo });
        }


        /// <summary>
        /// Marca como revisados los registros seleccionados de la bitácora especial.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que realiza la revisión.</param>
        /// <param name="bitacora">Registros de bitácora a revisar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AF_BitacoraEspecial_Revisar(int CodEmpresa, string usuario, List<AFBitacoraEspecialData> bitacora)
        {
            if (bitacora is null || bitacora.Count == 0)
            {
                return DbHelper.OkResponse("No hay registros para revisar.");
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                foreach (var item in bitacora)
                {
                    connection.Execute(SqlRevisarBitacora, new
                    {
                        Usuario = NormalizarTexto(usuario),
                        IdBitacora = item.id_bitacora
                    });
                }

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al revisar bitácora especial.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Obtiene los valores disponibles para búsquedas de bitácora especial.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="campo">Campo de búsqueda solicitado.</param>
        /// <returns>Listado de socios o usuarios para filtro.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_BitacoraEspecialBusquedas_Obtener(int CodEmpresa, string campo)
        {
            var sql = EsBusquedaSocios(campo) ? SqlBusquedaSocios : SqlBusquedaUsuarios;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                sql);
        }


        /// <summary>
        /// Obtiene las entradas de la bitácora especial según los filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de consulta.</param>
        /// <returns>Listado de registros de bitácora especial.</returns>
        public ErrorDto<List<AFBitacoraEspecialData>> AF_BitacoraEspecial_Obtener(int CodEmpresa, AFBitacoraEspecialFiltros filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de bitácora especial son requeridos.",
                    -2,
                    new List<AFBitacoraEspecialData>());
            }

            return DbHelper.ExecuteListQuery<AFBitacoraEspecialData>(
                CreatePortalDb(),
                CodEmpresa,
                SqlBitacoraEspecialConsulta,
                CrearParametrosBitacoraEspecial(filtros, vModulo));
        }


        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB configurada.</returns>
        private PortalDB CreatePortalDb() => new(_config);


        /// <summary>
        /// Indica si la búsqueda solicitada corresponde a socios.
        /// </summary>
        /// <param name="campo">Campo solicitado.</param>
        /// <returns>Verdadero si la búsqueda es de socios.</returns>
        private static bool EsBusquedaSocios(string campo)
        {
            return string.Equals(NormalizarTexto(campo), "SOCIOS", StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Crea parámetros seguros para consultar bitácora especial.
        /// </summary>
        /// <param name="filtros">Filtros de consulta.</param>
        /// <param name="modulo">Código de módulo.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosBitacoraEspecial(AFBitacoraEspecialFiltros filtros, int modulo)
        {
            var movimientos = filtros.movimientos?
                .Select(x => NormalizarTexto(x.item?.ToString()))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToArray() ?? Array.Empty<string>();

            var usarFechaRevision = filtros.chkRevisados == true;
            var aplicarFechas = filtros.chkFechas == false;
            var aplicarUsuario = filtros.chkUsuario == false && !string.IsNullOrWhiteSpace(filtros.usuario);

            return new
            {
                Modulo = modulo,
                Cedula = CrearLikeContiene(filtros.cedula),
                AplicarFechas = aplicarFechas ? 1 : 0,
                UsarFechaRevision = usarFechaRevision ? 1 : 0,
                FechaInicio = filtros.fecha_inicio.Date,
                FechaCorte = filtros.fecha_corte.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                AplicarMovimientos = movimientos.Length > 0 ? 1 : 0,
                Movimientos = movimientos,
                AplicarUsuario = aplicarUsuario ? 1 : 0,
                Usuario = NormalizarTexto(filtros.usuario),
                Revision = NormalizarRevision(filtros.revision)
            };
        }


        /// <summary>
        /// Normaliza el valor de revisión aceptado por la consulta.
        /// </summary>
        /// <param name="revision">Valor de revisión.</param>
        /// <returns>Valor normalizado para filtro.</returns>
        private static string NormalizarRevision(string? revision)
        {
            var valor = NormalizarTexto(revision).ToUpperInvariant();
            return valor is "P" or "R" or "T" ? valor : string.Empty;
        }


        /// <summary>
        /// Crea un valor LIKE de búsqueda parcial.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Valor preparado para LIKE o cadena vacía.</returns>
        private static string CrearLikeContiene(string? valor)
        {
            var texto = NormalizarTexto(valor);
            return string.IsNullOrWhiteSpace(texto) ? string.Empty : $"%{texto}%";
        }


        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Texto sin espacios externos o cadena vacía.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}