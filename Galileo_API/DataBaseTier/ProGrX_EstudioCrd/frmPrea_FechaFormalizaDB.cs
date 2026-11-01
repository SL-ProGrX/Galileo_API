using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd.Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaFechaFormalizaDB
    {
        private static readonly string[] FormatosFecha = { "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss", "yyyy-MM-dd" };

        private readonly PortalDB _portalDb;

        public FrmPreaFechaFormalizaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Convierte el valor de una columna de fecha del SP (DateTime o texto dd/MM/yyyy)
        /// sin depender de la cultura del servidor (en-US).
        /// </summary>
        private static DateTime? ConvertirFecha(object? valor)
        {
            if (valor is DateTime fecha)
            {
                return fecha;
            }

            if (valor is string texto && DateTime.TryParseExact(texto.Trim(), FormatosFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parseada))
            {
                return parseada;
            }

            return null;
        }

        /// <summary>
        /// Carga la información inicial de frmPrea_FechaFormaliza para que Angular
        /// solo pinte fechas y valores calculados base del expediente.
        /// </summary>
        public ErrorDto<FrmPreaFechaFormalizaCargarResponse> Prea_frmPreaFechaFormaliza_Cargar(
            int codEmpresa,
            FrmPreaFechaFormalizaCargarRequest request)
        {
           
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var baseData = connection.QueryFirstOrDefault<FrmPreaFechaFormalizaBaseData>(
                    "spCrdPrea_FechaCalIntereses",
                    new { Expediente = request.cod_preanalisis.Trim() },
                    commandType: CommandType.StoredProcedure
                ) ?? new FrmPreaFechaFormalizaBaseData();

                var Result = new FrmPreaFechaFormalizaCargarResponse
                {
                    cod_preanalisis = request.cod_preanalisis.Trim(),
                    planilla_aplica = ConvertirFecha(baseData.planilla_aplica),
                    planilla_envio = ConvertirFecha(baseData.planilla_envio),
                    fecha_corte = ConvertirFecha(baseData.fecha_corte),
                    formalizacion = ConvertirFecha(baseData.formalizacion),
                    monto = baseData.monto,
                    tasa = baseData.tasa,
                    dias = 0,
                    monto_interes = 0
                };

                return DbHelper.CreateOkResponse<FrmPreaFechaFormalizaCargarResponse>(Result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaFechaFormalizaCargarResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Calcula intereses proyectando la fecha de formalización a partir del expediente.
        /// El API resuelve monto, tasa y fecha de corte para no depender del cliente.
        /// </summary>
        public ErrorDto<FrmPreaFechaFormalizaCalcularResponse> Prea_frmPreaFechaFormaliza_Calcular(
            int codEmpresa,
            FrmPreaFechaFormalizaCalcularRequest request)
        {
            
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                var baseData = connection.QueryFirstOrDefault<FrmPreaFechaFormalizaBaseData>(
                    "spCrdPrea_FechaCalIntereses",
                    new { Expediente = request.cod_preanalisis.Trim() },
                    commandType: CommandType.StoredProcedure
                ) ?? new FrmPreaFechaFormalizaBaseData();

                var calculo = connection.QueryFirstOrDefault<FrmPreaFechaFormalizaCalculoSpData>(
                    "spCrdPrea_InteresesFormaliza_Calculo",
                    new
                    {
                        Expediente = request.cod_preanalisis.Trim(),
                        Monto = baseData.monto,
                        Tasa = baseData.tasa,
                        Formalizacion = request.fecha_formaliza,
                        Corte = ConvertirFecha(baseData.fecha_corte)
                    },
                    commandType: CommandType.StoredProcedure
                ) ?? new FrmPreaFechaFormalizaCalculoSpData();

                var Result = new FrmPreaFechaFormalizaCalcularResponse
                {
                    fecha_corte = ConvertirFecha(baseData.fecha_corte),
                    fecha_formaliza = request.fecha_formaliza,
                    dias = calculo.dias,
                    monto_interes = calculo.monto_interes
                };

                return DbHelper.CreateOkResponse<FrmPreaFechaFormalizaCalcularResponse>(Result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaFechaFormalizaCalcularResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Recalcula y guarda el monto de interés proyectado de frmPrea_FechaFormaliza.
        /// Angular solo envía expediente y fecha; el API valida, calcula y persiste.
        /// </summary>
        public ErrorDto<FrmPreaFechaFormalizaCambiarResponse> Prea_frmPreaFechaFormaliza_Cambiar(
            int codEmpresa,
            FrmPreaFechaFormalizaCambiarRequest request)
        {
            
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                connection.Execute(
                       "spCrdPreaGuardaMontoInteresProyectado",
                       new
                       {
                           COD_PREANALISIS = request.cod_preanalisis.Trim(),
                           MONTO_INTERES = request.monto_interes,
                           INDICADOR_CALC = 1
                       },
                       commandType: CommandType.StoredProcedure
                   );

                var Result = new FrmPreaFechaFormalizaCambiarResponse
                {
                    fecha_formaliza = request.fecha_formaliza,
                    monto_interes = request.monto_interes,
                    mensaje = "Se ha realizado la acción correctamente."
                };

                return DbHelper.CreateOkResponse<FrmPreaFechaFormalizaCambiarResponse>(Result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaFechaFormalizaCambiarResponse>(ex.Message);
            }
        }
    }
}
