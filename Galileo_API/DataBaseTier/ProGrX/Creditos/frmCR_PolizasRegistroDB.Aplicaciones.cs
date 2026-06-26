using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrPolizasRegistroDb
    {
        /// <summary>
        /// Aplica o remueve un acreedor asociado a la poliza.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<bool> CrPolizasRegistro_Acreedor_Aplicar(
            int codEmpresa,
            CrPolizasRegistroAcreedorAplicarRequest request)
        {
            if (request.operacion <= 0 || request.num_poliza <= 0 || string.IsNullOrWhiteSpace(request.cod_acreedor))
            {
                return DbHelper.CreateErrorResponse(
                    "Los datos del acreedor no son validos.",
                    -2,
                    false);
            }

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                const string sqlDelete = @"
                delete from CRD_OPERACION_POLIZAS_ACREEDORES
                where id_solicitud = @Operacion
                  and num_poliza = @NumPoliza
                  and cod_acreedor = @CodAcreedor;";

                conn.Execute(sqlDelete, new
                {
                    Operacion = request.operacion,
                    NumPoliza = request.num_poliza,
                    CodAcreedor = request.cod_acreedor.Trim()
                }, tx);

                if (request.checked_item)
                {
                    const string sqlInsert = @"
                    insert into CRD_OPERACION_POLIZAS_ACREEDORES
                    (
                        num_poliza,
                        cod_acreedor,
                        codigo,
                        id_solicitud,
                        registro_fecha,
                        registro_usuario
                    )
                    values
                    (
                        @NumPoliza,
                        @CodAcreedor,
                        @Codigo,
                        @Operacion,
                        Getdate(),
                        @Usuario
                    );";

                    conn.Execute(sqlInsert, new
                    {
                        NumPoliza = request.num_poliza,
                        CodAcreedor = request.cod_acreedor.Trim(),
                        Codigo = request.codigo.Trim(),
                        Operacion = request.operacion,
                        Usuario = request.usuario.Trim()
                    }, tx);
                }

                tx.Commit();
                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return DbHelper.CreateErrorResponse(
                    $"No fue posible aplicar el acreedor. {ex.Message}",
                    -1,
                    false);
            }
        }

        /// <summary>
        /// Calcula el detalle del plan de pagos para la poliza integrada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrPolizasRegistroPlanPagoDetalleData> CrPolizasRegistro_PlanPago_Detalle_Obtener(
            int codEmpresa,
            CrPolizasRegistroPlanPagoDetalleRequest request)
        {
            if (request.operacion <= 0 || request.id_seq <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operacion y la linea inicial del plan.",
                    -2,
                    new CrPolizasRegistroPlanPagoDetalleData());
            }

            if (!request.poliza_fecha_pago.HasValue ||
                !request.poliza_cobertura_inicio.HasValue ||
                !request.poliza_cobertura_corte.HasValue)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar las fechas de pago y cobertura.",
                    -2,
                    new CrPolizasRegistroPlanPagoDetalleData());
            }

            const string sqlMesesPendientes = @"
            select isnull(
                dbo.fxCrdPolizaMesesPendientes(
                    @Operacion,
                    @IdSeq,
                    @FechaInicio,
                    @FechaCorte
                ),
                0
            ) as meses;";

            var mesesPendientesResp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlMesesPendientes,
                0,
                new
                {
                    Operacion = request.operacion,
                    IdSeq = request.id_seq,
                    FechaInicio = request.poliza_cobertura_inicio.Value.Date,
                    FechaCorte = request.poliza_cobertura_corte.Value.Date
                });

            if (mesesPendientesResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    mesesPendientesResp.Description ?? "No fue posible calcular los meses pendientes del plan.",
                    mesesPendientesResp.Code.GetValueOrDefault(-1),
                    new CrPolizasRegistroPlanPagoDetalleData());
            }

            int mesesPendientes = mesesPendientesResp.Result;
            if (mesesPendientes <= 0)
            {
                mesesPendientes = 1;
            }

            int divisorFrecuencia = CrPolizasRegistro_FrecuenciaPagosDivisor_Obtener(request.poliza_pago_frecuencia);

            int mesesVigenciaPago = CrPolizasRegistro_DiferenciaMeses_Obtener(
                request.poliza_fecha_pago.Value,
                request.poliza_cobertura_corte.Value) + 1;

            if (mesesVigenciaPago <= 0)
            {
                mesesVigenciaPago = 1;
            }

            int pagosNum = mesesVigenciaPago / divisorFrecuencia;
            if (pagosNum <= 0)
            {
                pagosNum = 1;
            }

            int coberturaMeses = CrPolizasRegistro_DiferenciaMeses_Obtener(
                request.poliza_cobertura_inicio.Value,
                request.poliza_cobertura_corte.Value) + 1;

            if (coberturaMeses <= 0)
            {
                coberturaMeses = 1;
            }

            decimal pagoMonto = request.poliza_monto / pagosNum;
            decimal cuota = request.poliza_monto / mesesPendientes;
            decimal cuotaRestoPlazo = request.poliza_monto / coberturaMeses;

            return DbHelper.CreateOkResponse(new CrPolizasRegistroPlanPagoDetalleData
            {
                poliza_cobertura_meses = coberturaMeses,
                poliza_pagos_num = pagosNum,
                poliza_pago_monto = pagoMonto,
                poliza_cuota = cuota,
                poliza_ctas_deduce = mesesPendientes,
                poliza_cuota_resto_plazo = cuotaRestoPlazo,
                id_seq = request.id_seq
            });
        }
    }
}