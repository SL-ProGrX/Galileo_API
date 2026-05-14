using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndCalculadoraInversionesDB
    {
        private readonly IConfiguration _config;

        public FrmFndCalculadoraInversionesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtener dropdown planes 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="TipoInv"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Calculadora_Planes_Obtener(int CodEmpresa, int TipoInv)
        {
            return EjecutarDropdownCalculadora(
                CodEmpresa,
                "spFnd_Calculadora_Planes",
                new { Tipo = TipoInv == 1 ? "CDP" : "APL" });
        }

        /// <summary>
        /// Obtener datos del plan seleccionado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodPlan"></param>
        /// <returns></returns>
        public ErrorDto<FndCalculadoraPlanes> Fnd_Calculadora_ConsultaPlan_Obtener(int CodEmpresa, string CodPlan)
        {
            const string query = @"
                    SELECT
                        TIPO_DEDUC,
                        PORC_DEDUC,
                        TIPO_CDP,
                        PAGO_CUPONES,
                        WEB_VENCE,
                        CAPITALIZA_RENDIMIENTOS,
                        TASA_MARGEN_NEGOCIACION
                    FROM dbo.fnd_Planes
                    WHERE cod_operadora = 1
                      AND cod_plan = @CodPlan;";

            var result = DbHelper.ExecuteSingleQuery<FndCalculadoraPlanes>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new FndCalculadoraPlanes(),
                new { CodPlan = NormalizarTexto(CodPlan) });

            return new ErrorDto<FndCalculadoraPlanes>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndCalculadoraPlanes()
            };
        }

        /// <summary>
        /// Obtener dropdown de los plazos de inversión disponibles
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodPlan"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Calculadora_PlazosInv_Obtener(int CodEmpresa, string CodPlan)
        {
            return EjecutarDropdownCalculadora(
                CodEmpresa,
                "spFnd_Inversion_Plazos",
                new { CodPlan = NormalizarTexto(CodPlan) });
        }

        /// <summary>
        /// Obtener dropdown de los cupones disponibles
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Plazo"></param>
        /// <param name="CodPlan"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Calculadora_Cupones_Obtener(int CodEmpresa, int Plazo, string CodPlan)
        {
            return EjecutarDropdownCalculadora(
                CodEmpresa,
                "spFnd_Cupon_Frecuencia",
                new { Plazo, CodPlan = NormalizarTexto(CodPlan) });
        }

        /// <summary>
        /// Obtener dropdown de los plazos en días disponibles
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Plazo"></param>
        /// <param name="CodPlan"></param>
        /// <returns></returns>
        public ErrorDto<int> Fnd_Calculadora_PlazosDias_Obtener(int CodEmpresa, int Plazo, string CodPlan)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
            {
                var data = connection.QueryFirstOrDefault<dynamic>(
                    "spFnd_Inversion_Plazos_Dias",
                    new { Plazo },
                    commandType: System.Data.CommandType.StoredProcedure);

                return ObtenerDiasPlazo(data, CodPlan);
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener plazo en días.", result.Code ?? 0, 0);
            }

            return result.Result > 0
                ? DbHelper.CreateOkResponse(result.Result)
                : DbHelper.CreateErrorResponse("No se encontraron resultados para el plazo especificado.", -1, 0);
        }

        /// <summary>
        /// Obtener tasa de referencia
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="PlazoDias"></param>
        /// <param name="Tipo"></param>
        /// <param name="Plan"></param>
        /// <param name="Operadora"></param>
        /// <param name="chkCupon"></param>
        /// <param name="rpTipo"></param>
        /// <param name="PlazoInv"></param>
        /// <param name="CuponId"></param>
        /// <returns></returns>
        public ErrorDto<decimal> Fnd_Calculadora_TasaRef_Obtener(int CodEmpresa, int PlazoDias, string Tipo, string Plan, int Operadora, bool chkCupon, int rpTipo, int PlazoInv, int? CuponId )
        {
            if (!chkCupon || rpTipo == 0)
            {
                const string query = @"SELECT dbo.fxFNDCalcularTasaRefContrato(@Operadora, @Plan, @PlazoDias, @Tipo, NULL, NULL, 0) AS TASA;";

                return DbHelper.ExecuteSingleQuery(
                    new PortalDB(_config),
                    CodEmpresa,
                    query,
                    0m,
                    new { Operadora, Plan = NormalizarTexto(Plan), PlazoDias, Tipo = NormalizarTexto(Tipo) });
            }

            return DbHelper.ExecuteStoredProcedureSingle<decimal>(
                new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa),
                "dbo.spFnd_Inversion_Tasas_Condiciones",
                0m,
                new { Operadora, Plan = NormalizarTexto(Plan), PlazoInv, CuponId });
        }

        /// <summary>
        /// Calcular flujo de inversión
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="FiltrosCalculadora"></param>
        /// <returns></returns>
        public ErrorDto<List<FndCalculadoraInversionesFlujoData>> Fnd_Calculadora_Inversiones_Calcular(int CodEmpresa, string FiltrosCalculadora)
        {
            var filtros = DeserializeFiltros(FiltrosCalculadora);
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Error: FiltrosCalculadora deserialization returned null.",
                    -1,
                    new List<FndCalculadoraInversionesFlujoData>());
            }

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
            {
                var calculoId = RegistrarCalculo(connection, filtros);
                var flujo = ObtenerFlujoCalculo(connection, calculoId);

                return new CalculadoraInversionResultado
                {
                    CalculoId = calculoId,
                    Flujo = flujo
                };
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al calcular inversión.",
                    -1,
                    new List<FndCalculadoraInversionesFlujoData>());
            }

            return new ErrorDto<List<FndCalculadoraInversionesFlujoData>>
            {
                Code = result.Result?.Flujo.Count > 0 ? result.Result.CalculoId : 0,
                Description = "Ok",
                Result = result.Result?.Flujo ?? new List<FndCalculadoraInversionesFlujoData>()
            };
        }

        /// <summary>
        /// Enviar email del cálculo procesado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CalculoId"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Calculadora_Inversiones_EmailEnviar(int CodEmpresa, int CalculoId, string Usuario)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Execute(
                    "dbo.spFnd_Calculadora_Inversiones_Email",
                    new
                    {
                        CalculoId,
                        Usuario = NormalizarTexto(Usuario)
                    },
                    commandType: System.Data.CommandType.StoredProcedure));

            return result.Code == 0
                ? DbHelper.OkResponse("Correo enviado a la persona!")
                : DbHelper.ErrorResponse(result.Description ?? "Error al enviar correo del cálculo.", result.Code ?? -1);
        }

        private ErrorDto<List<DropDownListaGenericaModel>> EjecutarDropdownCalculadora(
            int codEmpresa,
            string procedimiento,
            object parametros)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), codEmpresa, connection =>
                connection.Query(
                    procedimiento,
                    parametros,
                    commandType: System.Data.CommandType.StoredProcedure)
                    .Select(row => new DropDownListaGenericaModel
                    {
                        item = Convert.ToString(row.IdX) ?? string.Empty,
                        descripcion = Convert.ToString(row.ItmX ?? row.itmX) ?? string.Empty
                    }).ToList());

            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<DropDownListaGenericaModel>()
            };
        }

        private static int ObtenerDiasPlazo(dynamic? data, string codPlan)
        {
            if (data == null)
            {
                return 0;
            }

            return EsPlanDias(codPlan)
                ? Convert.ToInt32(data.PLAZO_DIAS)
                : Convert.ToInt32(data.PLAZO_MESES);
        }

        private static bool EsPlanDias(string? codPlan)
        {
            var plan = NormalizarTexto(codPlan);
            return plan.Length > 0 && string.Equals(plan[..1], "D", StringComparison.OrdinalIgnoreCase);
        }

        private static FiltrosCalculadora? DeserializeFiltros(string filtrosCalculadora)
        {
            return System.Text.Json.JsonSerializer.Deserialize<FiltrosCalculadora>(filtrosCalculadora);
        }

        private static int RegistrarCalculo(SqlConnection connection, FiltrosCalculadora filtros)
        {
            return connection.QueryFirstOrDefault<int>(
                "dbo.spFnd_Calculadora_Inversiones_Registro",
                CrearParametrosRegistroCalculo(filtros),
                commandType: System.Data.CommandType.StoredProcedure);
        }

        private static List<FndCalculadoraInversionesFlujoData> ObtenerFlujoCalculo(SqlConnection connection, int calculoId)
        {
            return connection.Query<FndCalculadoraInversionesFlujoData>(
                "dbo.spFnd_Calculadora_Inversiones_Flujo",
                new { pCalculoId = calculoId },
                commandType: System.Data.CommandType.StoredProcedure).ToList();
        }

        private static object CrearParametrosRegistroCalculo(FiltrosCalculadora filtros)
        {
            return new
            {
                pCalculoId = filtros.pCalculoId == 0 ? (int?)null : filtros.pCalculoId,
                txtInversion = filtros.txtInversion,
                vFecha = DateTime.Now,
                plazo = filtros.Plazo,
                pTP_Sol = filtros.pTP_Sol,
                pFrecuenciaPago = filtros.pFrecuenciaPago,
                txtMonto = filtros.txtMonto,
                baseCalculo = 360,
                capitaliza = filtros.chkCapitaliza ? 1 : 0,
                cedula = NormalizarTexto(filtros.Cedula),
                plan = NormalizarTexto(filtros.Plan),
                origen = "ProGrX",
                usuario = NormalizarTexto(filtros.Usuario).ToUpper(),
                pTP_Indica = filtros.pTP_Sol > filtros.pTasa ? 1 : 0
            };
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }

    internal sealed class CalculadoraInversionResultado
    {
        public int CalculoId { get; init; }
        public List<FndCalculadoraInversionesFlujoData> Flujo { get; init; } = new();
    }
}
