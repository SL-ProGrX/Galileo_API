using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFComisionesAutorizaDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1;
        private readonly MSecurityMainDb _Security_MainDB;

        private const string SqlComisionesAutorizaBase = @"
                                SELECT
                                    S.id_Boleta_AF AS IdBoleta,
                                    S.Cedula,
                                    S.Nombre,
                                    S.id_promotor AS IdPromotor,
                                    ISNULL(S.Comision_Autoriza, 0) AS AutorizacionX,
                                    S.Comision_Autoriza,
                                    S.FechaIngreso,
                                    S.EstadoActual,
                                    S.reg_Fecha AS Fecha_Comision,
                                    S.reg_user AS Reg_User,
                                    S.AUTORIZA_COMISION_NOTAS AS Autoriza_Comision_Notas,
                                    P.Nombre AS PromotorX
                                FROM dbo.socios S
                                INNER JOIN dbo.promotores P
                                    ON S.id_promotor = P.id_promotor
                                WHERE S.estadoactual = 'S'
                                  AND S.Fecha_Comision IS NULL
                                  AND S.fechaIngreso BETWEEN @inicio AND @corte
                                  AND P.apl_comision = 1";

        private const string SqlComisionesAutorizaUpdate = @"
                                UPDATE dbo.socios
                                SET Comision_Autoriza = @Autoriza,
                                    AUTORIZA_COMISION_NOTAS = @Notas
                                WHERE cedula = @Cedula;";

        public FrmAFComisionesAutorizaDB(IConfiguration? config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }


        /// <summary>
        /// Obtiene la lista de socios para autorización de comisiones aplicando filtros opcionales.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtro">Filtros de consulta para autorización de comisiones.</param>
        /// <returns>Listado de socios pendientes o procesados para autorización.</returns>
        public ErrorDto<List<ComisionAutorizaData>> AF_ComisionesAutoriza_Obtener(int CodEmpresa, ComisionAutorizaFiltroDto filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de autorización son requeridos.",
                    -2,
                    new List<ComisionAutorizaData>());
            }

            var query = ConstruirConsultaComisiones(filtro);
            return DbHelper.ExecuteListQuery<ComisionAutorizaData>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                CrearParametrosComision(filtro));
        }


        /// <summary>
        /// Autoriza o desautoriza la comisión de un socio y registra la bitácora.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <param name="autoriza">Indicador de autorización.</param>
        /// <param name="notas">Notas asociadas a la autorización.</param>
        /// <param name="usuario">Usuario que ejecuta la acción.</param>
        /// <returns>Resultado de la actualización.</returns>
        public ErrorDto AF_ComisionesAutoriza_Autorizar(int CodEmpresa, string cedula, int autoriza, string? notas, string usuario)
        {
            var cedulaSegura = NormalizarTexto(cedula);
            if (string.IsNullOrWhiteSpace(cedulaSegura))
            {
                return DbHelper.ErrorResponse("La cédula es requerida.", -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlComisionesAutorizaUpdate,
                new
                {
                    Autoriza = autoriza,
                    Notas = NormalizarTexto(notas),
                    Cedula = cedulaSegura
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description ?? "Error al actualizar autorización de comisión.",
                    result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacoraAutorizacion(CodEmpresa, usuario, cedulaSegura, autoriza, notas);
            return DbHelper.OkResponse("Ok");
        }
        

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB configurada.</returns>
        private PortalDB CreatePortalDb() => new(_config);


        /// <summary>
        /// Construye la consulta de autorización de comisiones según los filtros aplicados.
        /// </summary>
        /// <param name="filtro">Filtros seleccionados.</param>
        /// <returns>Consulta SQL parametrizada.</returns>
        private static string ConstruirConsultaComisiones(ComisionAutorizaFiltroDto filtro)
        {
            var condiciones = new List<string>();

            if (filtro.ChkAportes)
            {
                condiciones.Add("dbo.fxAFIComisionAporte(S.FechaIngreso, S.Cedula) > 0");
            }

            if (filtro.ChkPromotor)
            {
                condiciones.Add("S.id_promotor = @idPromotor");
            }

            if (filtro.ChkUsuarios)
            {
                condiciones.Add("S.reg_user = @usuario");
            }

            if (filtro.Autorizado.HasValue)
            {
                if (filtro.Autorizado == 1 || filtro.Autorizado == 2)
                {
                    condiciones.Add("S.Comision_Autoriza = @autorizado");
                }
                else if (filtro.Autorizado == 0)
                {
                    condiciones.Add("ISNULL(S.Comision_Autoriza,0) = 0");
                }
            }

            var whereExtra = condiciones.Count > 0
                ? " AND " + string.Join(" AND ", condiciones)
                : string.Empty;

            return $@"{SqlComisionesAutorizaBase}
                       {whereExtra}
                       ORDER BY S.FechaIngreso";
        }


        /// <summary>
        /// Crea los parámetros seguros para la consulta de autorización.
        /// </summary>
        /// <param name="filtro">Filtros de consulta.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosComision(ComisionAutorizaFiltroDto filtro)
        {
            return new
            {
                inicio = filtro.Inicio.Date,
                corte = filtro.Corte.Date.AddDays(1).AddSeconds(-1),
                idPromotor = filtro.IdPromotor,
                usuario = NormalizarTexto(filtro.Usuario),
                autorizado = filtro.Autorizado
            };
        }


        /// <summary>
        /// Registra en bitácora la autorización o desautorización de comisión.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que ejecuta la acción.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <param name="autoriza">Indicador de autorización.</param>
        /// <param name="notas">Notas registradas.</param>
        private void RegistrarBitacoraAutorizacion(int codEmpresa, string usuario, string cedula, int autoriza, string? notas)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario),
                DetalleMovimiento = $"Comisión Autoriza: {cedula} - {NormalizarTexto(notas)}",
                Movimiento = autoriza == 1
                    ? "Autoriza Comisión - WEB"
                    : "Desautoriza Comisión - WEB",
                Modulo = vModulo
            });
        }


        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Texto sin espacios externos o cadena vacía.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}