using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta los créditos en tránsito del expediente (cancelados y por cobrar).
        /// Fiel a VB6 sbCreditosTransito_Load (frmPreaEstudiov2.frm línea ~17171):
        /// SELECT id_solicitud, detalle, cuota FROM CRD_PREA_DETALLE_CUOTAS_EN_TRANSITO
        /// WHERE estado = &lt;tipo&gt; AND cod_PreAnalisis = &lt;expediente&gt;
        /// tipo 'C' = Cancelados (gCuotasCancela), tipo 'A' = Por Cobrar (gCuotasCobrar).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CreditosResponse> Prea_frmPreaEstudiov2_Creditos_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            var result = new ErrorDto<FrmPreaEstudiov2CreditosResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2CreditosResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var cancelados = ConsultarCuotasTransito(connection, cod_preanalisis, "C");
                var porCobrar = ConsultarCuotasTransito(connection, cod_preanalisis, "A");

                result.Result = new FrmPreaEstudiov2CreditosResponse
                {
                    cancelados = cancelados,
                    por_cobrar = porCobrar,
                    total_cancelados = cancelados.Sum(c => c.cuota),
                    total_por_cobrar = porCobrar.Sum(c => c.cuota)
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2CreditosResponse();
            }

            return result;
        }

        private static List<FrmPreaEstudiov2CreditoTransitoDto> ConsultarCuotasTransito(
            IDbConnection connection,
            string cod_preanalisis,
            string tipo)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Estado", tipo, DbType.String);
            parameters.Add("@Expediente", cod_preanalisis.Trim(), DbType.String);

            return connection.Query<FrmPreaEstudiov2CreditoTransitoDto>(
                "select id_solicitud, detalle, cuota from CRD_PREA_DETALLE_CUOTAS_EN_TRANSITO" +
                " where estado = @Estado and cod_PreAnalisis = @Expediente",
                parameters,
                commandType: CommandType.Text
            ).ToList();
        }

        /// <summary>
        /// Registra una cuota en tránsito. Fiel a VB6 sbCreditos_Cuotas_Registra
        /// (frmPreaEstudiov2.frm línea ~15625): exec spCrdPreaRegistrarCreditosCuotasCxC
        /// '&lt;expediente&gt;', &lt;cuota&gt;, '&lt;tipo&gt;', '&lt;detalle&gt;'
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CreditosResponse> Prea_frmPreaEstudiov2_Creditos_Registrar(
            int codEmpresa,
            FrmPreaEstudiov2CreditoTransitoRegistrarRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var exp = request.cod_preanalisis.Trim().Replace("'", "''");
                var detalle = (request.detalle ?? string.Empty).Trim().Replace("'", "''");
                var cuota = Dec(request.cuota);

                var strSQL = $"exec spCrdPreaRegistrarCreditosCuotasCxC '{exp}', {cuota}, '{request.tipo}', '{detalle}'";
                connection.Execute(strSQL, commandType: CommandType.Text);
            }
            catch (Exception ex)
            {
                return new ErrorDto<FrmPreaEstudiov2CreditosResponse>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = new FrmPreaEstudiov2CreditosResponse()
                };
            }

            return Prea_frmPreaEstudiov2_Creditos_Consultar(codEmpresa, request.cod_preanalisis);
        }

        /// <summary>
        /// Elimina TODAS las cuotas en tránsito del tipo indicado (no una fila individual).
        /// Fiel a VB6 sbCreditos_Cuotas_Borrar (frmPreaEstudiov2.frm línea ~15593):
        /// exec spCrdPreaEliminarCreditosCuotasCxC '&lt;expediente&gt;', '&lt;tipo&gt;'
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CreditosResponse> Prea_frmPreaEstudiov2_Creditos_Borrar(
            int codEmpresa,
            FrmPreaEstudiov2CreditoTransitoBorrarRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var exp = request.cod_preanalisis.Trim().Replace("'", "''");
                var strSQL = $"exec spCrdPreaEliminarCreditosCuotasCxC '{exp}', '{request.tipo}'";
                connection.Execute(strSQL, commandType: CommandType.Text);
            }
            catch (Exception ex)
            {
                return new ErrorDto<FrmPreaEstudiov2CreditosResponse>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = new FrmPreaEstudiov2CreditosResponse()
                };
            }

            return Prea_frmPreaEstudiov2_Creditos_Consultar(codEmpresa, request.cod_preanalisis);
        }
    }
}
