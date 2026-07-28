using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmFndTrasladoFondosDB
    {
        private readonly IConfiguration _config;

        private const string SqlOperadoras = @"
                    SELECT cod_operadora AS item,
                           RTRIM(descripcion) AS descripcion
                    FROM dbo.FND_Operadoras;";

        private const string SqlSociosPorCedula = @"
                    SELECT cedula,
                           nombre
                    FROM dbo.socios
                    ORDER BY cedula;";

        private const string SqlSociosPorNombre = @"
                    SELECT cedula,
                           nombre
                    FROM dbo.socios
                    ORDER BY nombre;";

        private const string SqlContratosDisponibles = @"
                    SELECT C.cod_contrato,
                           C.cod_plan,
                           C.aportes + C.rendimiento - ISNULL(C.Monto_Transito, 0) AS Disponible,
                           P.descripcion
                    FROM dbo.fnd_planes P
                    INNER JOIN dbo.fnd_contratos C
                        ON P.cod_plan = C.cod_plan
                       AND P.cod_operadora = C.cod_operadora
                    WHERE C.estado = 'A'
                      AND C.cod_operadora = @CodOperadora
                      AND C.cedula = @Cedula
                      AND ISNULL(P.MOV_ENTRE_FONDOS, 0) = 1;";

        private const string SpTrasladoFondos = "spFndTrasladosFondos";

        public FrmFndTrasladoFondosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la lista de operadoras para traslado de fondos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de operadoras.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Operadoras_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlOperadoras);
        }


        /// <summary>
        /// Obtiene la lista de socios, ordenada por cédula o nombre.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="ordenarPor">Campo por el que se ordena: cedula o nombre.</param>
        /// <returns>Listado de socios.</returns>
        public ErrorDto<List<FndTrasladoSocioSimple>> Fnd_Traslado_Socios_Obtener(int CodEmpresa, string ordenarPor)
        {
            var sql = DebeOrdenarPorNombre(ordenarPor)
                ? SqlSociosPorNombre
                : SqlSociosPorCedula;

            return DbHelper.ExecuteListQuery<FndTrasladoSocioSimple>(
                CreatePortalDb(),
                CodEmpresa,
                sql);
        }


        /// <summary>
        /// Obtiene los contratos disponibles para traslado de fondos por operadora y cédula.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="codOperadora">Código de la operadora.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <returns>Listado de contratos disponibles.</returns>
        public ErrorDto<List<FndTrasladoContratoDisponible>> Fnd_Traslado_ContratosDisponibles_Obtener(int CodEmpresa, string codOperadora, string? cedula)
        {
            return DbHelper.ExecuteListQuery<FndTrasladoContratoDisponible>(
                CreatePortalDb(),
                CodEmpresa,
                SqlContratosDisponibles,
                new
                {
                    CodOperadora = NormalizarTexto(codOperadora),
                    Cedula = NormalizarTexto(cedula)
                });
        }


        /// <summary>
        /// Ejecuta el traslado de fondos entre contratos y planes.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del traslado.</param>
        /// <returns>Resultado del traslado.</returns>
        public ErrorDto<FndTrasladoFondosResult> Fnd_TrasladoFondos_Ejecutar(int CodEmpresa, FndTrasladoFondosRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse<FndTrasladoFondosResult>(
                    "Los datos del traslado son requeridos.",
                    -2,
                    null);
            }

            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirst<FndTrasladoFondosResult>(
                    SpTrasladoFondos,
                    CrearParametrosTraslado(request),
                    commandType: System.Data.CommandType.StoredProcedure));
        }

        /// <summary>
        /// Indica si la consulta de socios debe ordenarse por nombre.
        /// </summary>
        private static bool DebeOrdenarPorNombre(string? ordenarPor)
        {
            return string.Equals(
                NormalizarTexto(ordenarPor),
                "nombre",
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Crea parámetros seguros para ejecutar el traslado de fondos.
        /// </summary>
        private static object CrearParametrosTraslado(FndTrasladoFondosRequest request)
        {
            return new
            {
                Plan = NormalizarTexto(request.PlanOrigen),
                Contrato = request.ContratoOrigen,
                Cedula = NormalizarTexto(request.Cedula),
                Monto = request.Monto,
                DestinoPln = NormalizarTexto(request.PlanDestino),
                DestinoCnt = request.ContratoDestino,
                DestinoCdl = NormalizarTexto(request.Cedula),
                Usuario = NormalizarTexto(request.Usuario),
                Nota = NormalizarTexto(request.Nota),
                App = string.IsNullOrWhiteSpace(request.App) ? "ProGrX" : NormalizarTexto(request.App)
            };
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}