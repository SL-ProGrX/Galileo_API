using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhExcedentesMensualesDB
    {
        /// <summary>
        /// Obtiene el último periodo cerrado para el tab Aplicaciones.
        /// </summary>
        public ErrorDto<ExcPeriodosDto?> AH_ExcedentesMensuales_Aplicaciones_UltimoPeriodoCerrado_Obtener(int codEmpresa)
        {
            const string sql = @"
select top 1
    CAST(IdX AS varchar(20)) as idx,
    RTRIM(ItmX) as itmx,
    RTRIM(ISNULL(Estado, '')) as estado,
    ISNULL(MODO_AUTOMATICO, 0) as modo_automatico
from vExc_Periodos
where idx in (select max(idx) from vExc_Periodos where estado = 'C');";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var result = conn.QueryFirstOrDefault<ExcPeriodosDto>(sql);
                return DbHelper.CreateOkResponse<ExcPeriodosDto?>(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<ExcPeriodosDto?>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la bitácora completa del periodo para el tab Aplicaciones,
        /// incluyendo los procesos necesarios para replicar el seguimiento visual de VB6.
        /// </summary>
        public ErrorDto<List<BitacoraExcedenteDto>> AH_ExcedentesMensuales_Aplicaciones_Log_Lista(
            int codEmpresa,
            int periodoId)
        {
            const string sql = @"
select
    ISNULL(Linea, 0) as linea,
    Registro_Fecha as registro_fecha,
    RTRIM(ISNULL(Registro_Usuario, '')) as registro_usuario,
    RTRIM(ISNULL(Cod_Proceso, '')) as cod_proceso,
    RTRIM(ISNULL(Proceso_Desc, '')) as proceso_desc,
    RTRIM(ISNULL(Detalle, '')) as detalle,
    RTRIM(ISNULL(Tipo_Documento, '')) as tipo_documento,
    RTRIM(ISNULL(Cod_Transaccion, '')) as cod_transaccion,
    ISNULL(Casos, 0) as casos,
    ISNULL(Monto, 0) as monto,
    RTRIM(ISNULL(Time_Inicio, '')) as time_inicio,
    RTRIM(ISNULL(Time_Corte, '')) as time_corte,
    RTRIM(ISNULL(Duracion, '')) as duracion,
    RTRIM(ISNULL(Notas, '')) as notas
from vExc_Periodos_Bitacora
where id_periodo = @PeriodoId
order by Registro_Fecha asc, Linea asc;";

            return DbHelper.ExecuteListQuery<BitacoraExcedenteDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { PeriodoId = periodoId });
        }

        /// <summary>
        /// Obtiene la lista de procesos pendientes de aplicaciones para el periodo.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AH_ExcedentesMensuales_Aplicaciones_ProcesosPendientes_Lista(
            int codEmpresa,
            int periodoId)
        {
            const string sql = @"
exec spExc_Aplicaciones_Procesos_Pendientes @PeriodoId;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var result = conn.Query<dynamic>(
                    sql,
                    new { PeriodoId = periodoId })
                    .Select(x =>
                    {
                        x.item = x.COD_PROCESO?.Trim();
                        x.descripcion = x.DESCRIPCION?.Trim();
                        return x;
                    })
                    .ToList();

                var lista = result.Select(x => new DropDownListaGenericaModel
                {
                    item = x.item,
                    descripcion = x.descripcion
                }).ToList();

                return DbHelper.CreateOkResponse<List<DropDownListaGenericaModel>>(lista);  
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta la separación de salidas del periodo.
        /// </summary>
        public ErrorDto AH_ExcedentesMensuales_Aplicaciones_Salidas_Separa(
            int codEmpresa,
            int periodoId,
            string usuario)
        {
            const string sql = @"
exec spExc_Procesos_Salidas_Separa @PeriodoId, @Usuario;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    PeriodoId = periodoId,
                    Usuario = usuario
                });
        }

        /// <summary>
        /// Obtiene las salidas pendientes de traslado a fondos del periodo.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AH_ExcedentesMensuales_Aplicaciones_SalidasPendientes_Lista(
            int codEmpresa,
            int periodoId)
        {
            const string sql = @"
select
    RTRIM(COD_SALIDA) as item,
    RTRIM(DESCRIPCION) as descripcion
from EXC_TIPOS_SALIDAS
where DESTINO_PLAN <> ''
  and ('Salida: ' + COD_SALIDA) not in
  (
      select DETALLE
      from EXC_PERIODOS_BITACORA
      where COD_PROCESO = '12'
        and ID_PERIODO = @PeriodoId
  )
  and COD_SALIDA in
  (
      select COD_SALIDA
      from vExc_Cierre_Salida_Rsm
      where ID_PERIODO = @PeriodoId
        and EXCEDENTE_FINAL > 0
  );";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { PeriodoId = periodoId });
        }

        /// <summary>
        /// Ejecuta el traslado a fondos de una salida del periodo.
        /// </summary>
        public ErrorDto AH_ExcedentesMensuales_Aplicaciones_Salidas_Fondos(
            int codEmpresa,
            int periodoId,
            string salida,
            string usuario)
        {
            const string sql = @"
exec spExc_Procesos_Salidas_Fondos @PeriodoId, @Salida, @Usuario;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    PeriodoId = periodoId,
                    Salida = salida,
                    Usuario = usuario
                });
        }


        /// <summary>
        /// Ejecuta un proceso del tab Aplicaciones según el radio seleccionado.
        /// Replica el flujo manual de VB6 en una sola entrada de API.
        /// </summary>
        public ErrorDto<FrmAhExcedentesMensualesAplicacionProcesoResponse?> AH_ExcedentesMensuales_Aplicaciones_Proceso_Ejecutar(
            int codEmpresa,
            FrmAhExcedentesMensualesAplicacionProcesoRequest request)
        {
            const string sqlSysPlanPagos = @"
select isnull(SysCrdPlanPago, 0)
from sif_empresa;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var procesoId = (request.procesoId ?? string.Empty).Trim().ToUpperInvariant();
                var aplicaCero = request.cargaInfoCero ? 1 : 0;
                var sysPlanPagos = conn.QueryFirstOrDefault<int>(sqlSysPlanPagos);

                string mensaje = procesoId switch
                {
                    "INSOLVENTES" => EjecutarInsolventes(conn, request),
                    "DONACIONES" => EjecutarDonaciones(conn, request),
                    "AJUSTES" => EjecutarAjustes(conn, request, aplicaCero),
                    "SALDOS_GARANTIA" => EjecutarSaldosGarantia(conn, request, aplicaCero),
                    "MORA" => EjecutarMora(codEmpresa, conn, request, aplicaCero, sysPlanPagos),
                    "MORA_OPCF" => EjecutarMoraOpcf(conn, request, aplicaCero, sysPlanPagos),
                    "CAPITALIZA_EXTRA" => EjecutarCapitalizacionExtra(conn, request),
                    "ABONO_PATRONAL" => EjecutarAbonoPatronal(conn, request),
                    "ACT_AHORROS_EXTRA" => EjecutarActualizaAhorrosExtra(conn, request),
                    "ACT_AHORROS_GENERAL" => EjecutarActualizaAhorrosGeneral(conn, request),
                    "ACT_INFO_AJUSTES" => EjecutarActualizaAjustes(conn, request),
                    "ACT_CREDITOS" => EjecutarActualizaCreditos(conn, request),
                    "ASIENTO_GENERAL" => EjecutarAsientoGeneral(conn, request),
                    "FACTURA_ELECTRONICA" => EjecutarFacturaElectronica(conn, request),
                    "SALIDAS_SEPARA" => EjecutarSalidasSepara(conn, request),
                    "SALIDAS_FONDOS" => EjecutarSalidasFondos(conn, request),
                    _ => throw new InvalidOperationException("El proceso seleccionado no es válido.")
                };

                return DbHelper.CreateOkResponse<FrmAhExcedentesMensualesAplicacionProcesoResponse?>(
                    new FrmAhExcedentesMensualesAplicacionProcesoResponse
                    {
                        mensaje = mensaje
                    });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmAhExcedentesMensualesAplicacionProcesoResponse?>(ex.Message);
            }
        }

        private enum ExcCierreLimpiezaTipo
        {
            Mora,
            Ajustes,
            SaldosGarantia,
            MoraOpcf
        }

        private static void LimpiarExcCierre(SqlConnection conn, int periodoId, ExcCierreLimpiezaTipo tipo)
        {
            string sql = tipo switch
            {
                ExcCierreLimpiezaTipo.Mora => @"
update exc_cierre
set mora_cargada = 0,
    mora_aplicada = 0,
    exc_posmora = exc_PosSaldos_ASE
where id_periodo = @PeriodoId;",

                ExcCierreLimpiezaTipo.Ajustes => @"
update exc_cierre
set Ajuste_cargado = 0,
    ajuste_aplicado = 0,
    excedente_posajuste = 0
where id_periodo = @PeriodoId;",

                ExcCierreLimpiezaTipo.SaldosGarantia => @"
update exc_cierre
set saldos_ase_cargado = 0,
    saldos_ase_aplicados = 0,
    exc_posSaldos_ASE = excedente_PosAjuste
where id_periodo = @PeriodoId;",

                ExcCierreLimpiezaTipo.MoraOpcf => @"
update exc_cierre
set moraopcf_cargada = 0,
    moraopcf_aplicada = 0,
    exc_posmoraopcf = 0
where id_periodo = @PeriodoId;",

                _ => throw new InvalidOperationException("Tipo de limpieza no soportado.")
            };

            conn.Execute(sql, new { PeriodoId = periodoId });
        }

        private static void EjecutarSpIterativo(SqlConnection conn, string sql, object parameters)
        {
            var result = conn.QueryFirstOrDefault(sql, parameters);

            while (result != null && Convert.ToInt32(result!.Pendientes) > 0)
            {
                result = conn.QueryFirstOrDefault(sql, parameters);
            }

        }

        private string EjecutarInsolventes(SqlConnection conn, FrmAhExcedentesMensualesAplicacionProcesoRequest request)
        {
            conn.Execute(
                "exec spExc_Insolventes @PeriodoId, @Usuario;",
                new { PeriodoId = request.periodoId, Usuario = request.usuario });

            return "Indicador de insolventes aplicado satisfactoriamente.";
        }

        private string EjecutarDonaciones(SqlConnection conn, FrmAhExcedentesMensualesAplicacionProcesoRequest request)
        {
            conn.Execute(
                "exec spExc_Procesos_Donacion_Aplica @PeriodoId, @Usuario;",
                new { PeriodoId = request.periodoId, Usuario = request.usuario });

            return "Donaciones aplicadas satisfactoriamente.";
        }

        private string EjecutarAjustes(
            SqlConnection conn,
            FrmAhExcedentesMensualesAplicacionProcesoRequest request,
            int aplicaCero)
        {
            if (request.limpiaAplicacionAnterior)
            {
                LimpiarExcCierre(
                    conn,
                    request.periodoId,
                    ExcCierreLimpiezaTipo.Ajustes);
            }

            conn.Execute(
                "exec spExc_Procesos_Ajustes_Cargado @PeriodoId, @AplicaCero, @Usuario;",
                new { PeriodoId = request.periodoId, AplicaCero = aplicaCero, Usuario = request.usuario });

            conn.Execute(
                "exec spExc_Procesos_Ajustes_Aplica @PeriodoId, @Usuario;",
                new { PeriodoId = request.periodoId, Usuario = request.usuario });

            return "Ajustes cargados y aplicados satisfactoriamente.";
        }

        private string EjecutarSaldosGarantia(
            SqlConnection conn,
            FrmAhExcedentesMensualesAplicacionProcesoRequest request,
            int aplicaCero)
        {
            if (request.limpiaAplicacionAnterior)
            {
                LimpiarExcCierre(
                    conn,
                    request.periodoId,
                    ExcCierreLimpiezaTipo.SaldosGarantia);
            }

            conn.Execute(
                "exec spExc_Procesos_Creditos_EXC_Cargado @PeriodoId, @AplicaCero, @Usuario;",
                new { PeriodoId = request.periodoId, AplicaCero = aplicaCero, Usuario = request.usuario });

            return "Créditos con garantía cargados y aplicados satisfactoriamente.";
        }

        private string EjecutarMora(
            int codEmpresa,
            SqlConnection conn,
            FrmAhExcedentesMensualesAplicacionProcesoRequest request,
            int aplicaCero,
            int sysPlanPagos)
        {
            if (request.limpiaAplicacionAnterior)
            {
                LimpiarExcCierre(
                    conn,
                    request.periodoId, ExcCierreLimpiezaTipo.Mora);
            }

            conn.Execute(
                "exec spExc_Procesos_Morosidad_Cargado @PeriodoId, @AplicaCero, @Usuario;",
                new { PeriodoId = request.periodoId, AplicaCero = aplicaCero, Usuario = request.usuario });

            if (request.cargaInfoCero)
            {
                AH_ExcedentesMensuales_Bitacora_Registrar(codEmpresa: codEmpresa, periodoId: request.periodoId, codProceso: "", detalle: "", usuario: request.usuario);
            }

            if (request.cargaInfoCero)
            {
                conn.Execute(@"
insert into EXC_PERIODOS_BITACORA
(
    ID_PERIODO, LINEA, COD_PROCESO, DETALLE, REGISTRO_FECHA, REGISTRO_USUARIO,
    TIPO_DOCUMENTO, COD_TRANSACCION, MONTO, CASOS, TIME_INICIO, TIME_CORTE
)
values
(
    @PeriodoId,
    (select isnull(max(LINEA), 0) + 1 from EXC_PERIODOS_BITACORA where ID_PERIODO = @PeriodoId),
    '04',
    'Actualiza',
    dbo.MyGetdate(),
    @Usuario,
    '',
    '',
    0,
    0,
    dbo.MyGetdate(),
    dbo.MyGetdate()
);", new { PeriodoId = request.periodoId, Usuario = request.usuario });

                return "Morosidad cargada en cero satisfactoriamente.";
            }

            if (sysPlanPagos != 1)
            {
                throw new InvalidOperationException("La empresa no usa plan de pagos. Falta migrar la rama VB6 de morosidad sin plan de pagos.");
            }

            EjecutarSpIterativo(
                conn,
                "exec spExc_Procesos_Morosidad_Pago @PeriodoId, @Usuario;",
                new { PeriodoId = request.periodoId, Usuario = request.usuario });

            return "Morosidad aplicada satisfactoriamente.";
        }

        private string EjecutarMoraOpcf(
            SqlConnection conn,
            FrmAhExcedentesMensualesAplicacionProcesoRequest request,
            int aplicaCero,
            int sysPlanPagos)
        {
            if (request.limpiaAplicacionAnterior)
            {
                LimpiarExcCierre(
                    conn,
                    request.periodoId, ExcCierreLimpiezaTipo.MoraOpcf);
            }

            conn.Execute(
                "exec spExc_Procesos_Morosidad_OPCF_Cargado @PeriodoId, @AplicaCero, @Usuario;",
                new { PeriodoId = request.periodoId, AplicaCero = aplicaCero, Usuario = request.usuario });

            if (request.cargaInfoCero)
            {
                conn.Execute(@"
insert into EXC_PERIODOS_BITACORA
(
    ID_PERIODO, LINEA, COD_PROCESO, DETALLE, REGISTRO_FECHA, REGISTRO_USUARIO,
    TIPO_DOCUMENTO, COD_TRANSACCION, MONTO, CASOS, TIME_INICIO, TIME_CORTE
)
values
(
    @PeriodoId,
    (select isnull(max(LINEA), 0) + 1 from EXC_PERIODOS_BITACORA where ID_PERIODO = @PeriodoId),
    '05',
    'Actualiza',
    dbo.MyGetdate(),
    @Usuario,
    '',
    '',
    0,
    0,
    dbo.MyGetdate(),
    dbo.MyGetdate()
);", new { PeriodoId = request.periodoId, Usuario = request.usuario });

                return "Morosidad OPCF cargada en cero satisfactoriamente.";
            }

            if (sysPlanPagos != 1)
            {
                throw new InvalidOperationException("La empresa no usa plan de pagos. Falta migrar la rama VB6 de morosidad OPCF sin plan de pagos.");
            }

            EjecutarSpIterativo(
                conn,
                "exec spExc_Procesos_Morosidad_OPCF_Pago @PeriodoId, @Usuario;",
                new { PeriodoId = request.periodoId, Usuario = request.usuario });

            return "Morosidad OPCF aplicada satisfactoriamente.";
        }

        private string EjecutarCapitalizacionExtra(SqlConnection conn, FrmAhExcedentesMensualesAplicacionProcesoRequest request)
        {
            conn.Execute(
                "exec spExc_Procesos_CAP_Individual_Cargado @PeriodoId, @Usuario;",
                new { PeriodoId = request.periodoId, Usuario = request.usuario });

            return "Capitalización extraordinaria aplicada satisfactoriamente.";
        }

        private string EjecutarAbonoPatronal(SqlConnection conn, FrmAhExcedentesMensualesAplicacionProcesoRequest request)
        {
            conn.Execute(
                "exec spExc_AbonosExtraordinariosRenunciaPatronal @PeriodoId, @Usuario;",
                new { PeriodoId = request.periodoId, Usuario = request.usuario });

            return "Abono extraordinario por renuncia patronal aplicado satisfactoriamente.";
        }

        private string EjecutarActualizaAhorrosExtra(SqlConnection conn, FrmAhExcedentesMensualesAplicacionProcesoRequest request)
        {
            conn.QueryFirstOrDefault(
                "exec spExc_Procesos_CAP_Individual_Fondos @PeriodoId, @Usuario;",
                new { PeriodoId = request.periodoId, Usuario = request.usuario });

            return "Ahorros con capitalizaciones extraordinarias actualizados satisfactoriamente.";
        }

        private string EjecutarActualizaAhorrosGeneral(SqlConnection conn, FrmAhExcedentesMensualesAplicacionProcesoRequest request)
        {
            conn.Execute(
                "exec spExc_Capitalizacion_General_Actualiza @PeriodoId, @Usuario;",
                new { PeriodoId = request.periodoId, Usuario = request.usuario });

            return "Capitalización general actualizada satisfactoriamente.";
        }

        private string EjecutarActualizaAjustes(SqlConnection conn, FrmAhExcedentesMensualesAplicacionProcesoRequest request)
        {
            conn.Execute(
                "exec spExc_Procesos_Ajustes_Actualiza @PeriodoId, @Usuario;",
                new { PeriodoId = request.periodoId, Usuario = request.usuario });

            return "Información de ajustes actualizada satisfactoriamente.";
        }

        private string EjecutarActualizaCreditos(SqlConnection conn, FrmAhExcedentesMensualesAplicacionProcesoRequest request)
        {
            EjecutarSpIterativo(
                conn,
                "exec spExc_Procesos_Creditos_EXC_Pago @PeriodoId, @Usuario;",
                new { PeriodoId = request.periodoId, Usuario = request.usuario });

            return "Créditos actualizados satisfactoriamente.";
        }

        private string EjecutarAsientoGeneral(SqlConnection conn, FrmAhExcedentesMensualesAplicacionProcesoRequest request)
        {
            conn.QueryFirstOrDefault(
                "exec spExc_Comprobante @PeriodoId, @Usuario, '';",
                new { PeriodoId = request.periodoId, Usuario = request.usuario });

            return "Asiento general de excedentes creado satisfactoriamente.";
        }

        private string EjecutarFacturaElectronica(SqlConnection conn, FrmAhExcedentesMensualesAplicacionProcesoRequest request)
        {
            conn.Execute(
                "exec spExc_FacturaElectronica @PeriodoId, @Usuario;",
                new { PeriodoId = request.periodoId, Usuario = request.usuario });

            return "Factura electrónica registrada satisfactoriamente.";
        }

        private string EjecutarSalidasSepara(SqlConnection conn, FrmAhExcedentesMensualesAplicacionProcesoRequest request)
        {
            conn.Execute(
                "exec spExc_Procesos_Salidas_Separa @PeriodoId, @Usuario;",
                new { PeriodoId = request.periodoId, Usuario = request.usuario });

            return "Salidas separadas satisfactoriamente.";
        }

        private string EjecutarSalidasFondos(SqlConnection conn, FrmAhExcedentesMensualesAplicacionProcesoRequest request)
        {
            conn.Execute(
                "exec spExc_Procesos_Salidas_Fondos @PeriodoId, @Salida, @Usuario;",
                new { PeriodoId = request.periodoId, Salida = request.salida, Usuario = request.usuario });

            conn.Execute(@"
insert into EXC_PERIODOS_BITACORA
(
    ID_PERIODO, LINEA, COD_PROCESO, DETALLE, REGISTRO_FECHA, REGISTRO_USUARIO,
    TIPO_DOCUMENTO, COD_TRANSACCION, MONTO, CASOS, TIME_INICIO, TIME_CORTE
)
values
(
    @PeriodoId,
    (select isnull(max(LINEA), 0) + 1 from EXC_PERIODOS_BITACORA where ID_PERIODO = @PeriodoId),
    '12',
    'Actualiza',
    dbo.MyGetdate(),
    @Usuario,
    '',
    '',
    0,
    0,
    dbo.MyGetdate(),
    dbo.MyGetdate()
);", new { PeriodoId = request.periodoId, Usuario = request.usuario });

            return "Excedentes trasladados a fondos satisfactoriamente.";
        }
    }
}
