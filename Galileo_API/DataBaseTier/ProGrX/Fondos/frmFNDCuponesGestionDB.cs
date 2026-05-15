using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndCuponesGestionDb
    {
        private readonly IConfiguration _config;

        private const string SpConsultaVencimiento = "spFndCDPCuponesConsultaVencimiento";
        private const string SpLiquidaCupon = "spFndCDPCuponesLiquida";

        private const string SqlBancosUsuario = @"
                    SELECT
                        B.id_banco AS item,
                        B.descripcion AS descripcion
                    FROM dbo.tes_banco_asg T
                    INNER JOIN dbo.Tes_Bancos B
                        ON T.id_banco = B.id_banco
                    WHERE T.nombre = @Usuario
                      AND B.Estado = 'A';";

        private const string SqlConceptosActivos = @"
                    SELECT
                        RTRIM(RETENCION_CODIGO) AS item,
                        RTRIM(DESCRIPCION) AS descripcion
                    FROM dbo.FND_RETENCION_CONCEPTOS
                    WHERE ACTIVO = 1;";

        private const string SqlPlanExiste = @"
                    SELECT COUNT(1) AS Existe
                    FROM dbo.fnd_Planes
                    WHERE cod_Plan IN
                    (
                        SELECT valor
                        FROM dbo.Fnd_parametros
                        WHERE cod_parametro = '24'
                    );";

        public FrmFndCuponesGestionDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la lista de bancos asignados al usuario.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> FndCuponesGestion_Bancos_Obtener(int CodEmpresa, string usuario)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlBancosUsuario,
                new { Usuario = NormalizarTexto(usuario) });
        }

        /// <summary>
        /// Obtiene la lista de conceptos activos.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> FndCuponesGestion_Conceptos_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlConceptosActivos);
        }

        /// <summary>
        /// Valida si existen planes con cod_parametro = '24'.
        /// </summary>
        public ErrorDto<FndCuponesGestionPlanExisteResult> FndCuponesGestion_PlanExiste(int CodEmpresa)
        {
            var result = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlPlanExiste,
                0);

            return new ErrorDto<FndCuponesGestionPlanExisteResult>
            {
                Code = result.Code,
                Description = result.Description,
                Result = new FndCuponesGestionPlanExisteResult
                {
                    Existe = result.Result
                }
            };
        }

        /// <summary>
        /// Consulta los cupones según los parámetros y el estado de estos.
        /// </summary>
        public ErrorDto<List<FndCuponesGestionVencimientoResult>> FndCuponesGestion_ConsultaVencimiento(FndCuponesGestionVencimientoParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de consulta son requeridos.",
                    -2,
                    new List<FndCuponesGestionVencimientoResult>());
            }

            var result = DbHelper.WithConn(new PortalDB(_config), param.CodEmpresa, connection =>
                connection.Query<FndCuponesGestionVencimientoResult>(
                    SpConsultaVencimiento,
                    CrearParametrosConsultaVencimiento(param),
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return new ErrorDto<List<FndCuponesGestionVencimientoResult>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<FndCuponesGestionVencimientoResult>()
            };
        }

        /// <summary>
        /// Liquida los cupones según los parámetros proporcionados.
        /// </summary>
        public ErrorDto<FndCuponesGestionLiquidaResult> FndCuponesGestion_Liquida(FndCuponesGestionLiquidaParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de liquidación son requeridos.",
                    -2,
                    new FndCuponesGestionLiquidaResult());
            }

            var result = DbHelper.WithConn(new PortalDB(_config), param.CodEmpresa, connection =>
                connection.QueryFirstOrDefault<FndCuponesGestionLiquidaResult>(
                    SpLiquidaCupon,
                    CrearParametrosLiquida(param),
                    commandType: System.Data.CommandType.StoredProcedure));

            return new ErrorDto<FndCuponesGestionLiquidaResult>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndCuponesGestionLiquidaResult()
            };
        }
        
        private static object CrearParametrosConsultaVencimiento(FndCuponesGestionVencimientoParams param)
        {
            return new
            {
                Inicio = param.chkFechas ? new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc) : param.FechaInicio.Date,
                Corte = param.chkFechas ? new DateTime(2199, 12, 31, 23, 59, 59, DateTimeKind.Utc) : param.FechaCorte.Date.AddDays(1).AddTicks(-1),
                Operadora = param.CodOperadora,
                Plan = NormalizarTexto(param.CodPlan),
                Tipo = NormalizarTexto(param.Proceso),
                EmiteTipo = NormalizarTexto(param.TipoPago),
                EmiteBanco = param.BancoId
            };
        }

        private static object CrearParametrosLiquida(FndCuponesGestionLiquidaParams param)
        {
            return new
            {
                Operadora = param.CodOperadora,
                Plan = NormalizarTexto(param.CodPlan),
                Contrato = param.Contrato,
                CuponId = param.CuponId,
                Usuario = NormalizarTexto(param.Usuario),
                TipoGestion = NormalizarTexto(param.Proceso),
                Retencion = NormalizarTexto(param.RetencionCodigo),
                Emite = NormalizarTexto(param.TipoDoc),
                Banco = param.BancoId,
                CtaAhorro = NormalizarTexto(param.CuentaPersona),
                Tesoreria = param.TesoreriaFlag,
                Notas = NormalizarTexto(param.Descripcion),
                AppName = string.IsNullOrWhiteSpace(param.AppName) ? "ProGrX" : NormalizarTexto(param.AppName)
            };
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
