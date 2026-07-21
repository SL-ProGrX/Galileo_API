using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ProGrX.Credito;
using System.Data;
using System.Linq;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Credito
{
    public partial class FrmCRConsultaCreditosDB
    {
        #region Créditos

        /// <summary>
        /// Obtiene la fecha del servidor y la versión de cálculo de créditos.
        /// </summary>
        public ErrorDto<CrConsultaCreditoContextoData> CR_ConsultaCrd_CreditoContexto_Obtener(int codEmpresa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.QueryFirstOrDefault<CrConsultaCreditoContextoData>(
                    @"select dbo.MyGetdate() as fechaServidor,
                             isnull(SysCrdPlanPago, 0) as sysPlanPagos
                        from SIF_EMPRESA"));

            return result.Code == 0 && result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "No fue posible obtener el contexto de créditos.",
                    result.Code.GetValueOrDefault(-1),
                    new CrConsultaCreditoContextoData());
        }

        /// <summary>
        /// Calcula los valores de cancelación de una operación a una fecha de corte.
        /// </summary>
        public ErrorDto<CrConsultaCancelacionData> CR_ConsultaCrd_Cancelacion_Obtener(
            int codEmpresa,
            int operacion,
            DateTime corte)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar una operación válida.",
                    -1,
                    new CrConsultaCancelacionData());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                var sysPlanPagos = connection.QueryFirstOrDefault<int>(
                    "select isnull(SysCrdPlanPago, 0) from SIF_EMPRESA");

                if (sysPlanPagos == 1)
                {
                    return connection.QueryFirstOrDefault<CrConsultaCancelacionData>(
                        "spCrdPlanPagosInfoCancelacion",
                        new
                        {
                            Operacion = operacion,
                            Fecha = corte.ToString("yyyy/MM/dd")
                        },
                        commandType: CommandType.StoredProcedure);
                }

                var credito = connection.QueryFirstOrDefault<CrConsultaCancelacionLegacyRow>(
                    @"select R.saldo,
                             R.interesv,
                             R.fecUlt,
                             isnull(V.intc + V.intm, 0) as intMora,
                             isnull(V.cargos, 0) as cargos,
                             isnull(V.cuota, 0) as moraCuota,
                             isnull(V.Amortiza, 0) as principalAtrasado,
                             R.PriDeduc
                        from REG_CREDITOS R
                        inner join CATALOGO C on R.codigo = C.codigo
                        left join VISTA_MOROSIDAD V
                          on R.id_solicitud = V.id_solicitud
                       where R.id_solicitud = @Operacion",
                    new { Operacion = operacion });

                if (credito is null)
                {
                    return null;
                }

                var ultimoProceso = credito.fecUlt;
                if (credito.moraCuota > 0)
                {
                    var procesoMora = connection.QueryFirstOrDefault<int?>(
                        @"select max(fechap)
                            from MOROSIDAD
                           where estado = 'A'
                             and id_solicitud = @Operacion",
                        new { Operacion = operacion });
                    if (procesoMora.GetValueOrDefault() > ultimoProceso)
                    {
                        ultimoProceso = procesoMora.GetValueOrDefault();
                    }
                }

                var procesoCorte = corte.Year * 100 + corte.Month;
                decimal interesCorriente = 0;

                if (procesoCorte == credito.priDeduc &&
                    ultimoProceso < procesoCorte)
                {
                    interesCorriente =
                        credito.saldo * credito.interesv / 36000m * corte.Day;
                }
                else if (procesoCorte > credito.priDeduc &&
                         ultimoProceso <= procesoCorte)
                {
                    var meses = -1;
                    var procesoTemporal = ultimoProceso;
                    while (procesoCorte > procesoTemporal)
                    {
                        meses++;
                        procesoTemporal = SiguienteProceso(procesoTemporal);
                    }

                    interesCorriente =
                        credito.saldo * credito.interesv / 36000m *
                        (corte.Day + meses * 30);
                }

                if (credito.principalAtrasado >= credito.saldo)
                {
                    interesCorriente = 0;
                }

                return new CrConsultaCancelacionData
                {
                    principal = credito.saldo,
                    intcor = interesCorriente,
                    intmor = credito.intMora,
                    cargos = credito.cargos
                };
            });

            return result.Code == 0 && result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "No se encontró la operación indicada.",
                    result.Code.GetValueOrDefault(-1),
                    new CrConsultaCancelacionData());
        }

        /// <summary>
        /// Obtiene el expediente de preanálisis relacionado con una operación.
        /// </summary>
        public ErrorDto<string> CR_ConsultaCrd_PreAnalisisOperacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar una operación válida.",
                    -1,
                    string.Empty);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.QueryFirstOrDefault<string>(
                    @"select top (1)
                             ltrim(rtrim(convert(varchar(50), cod_preAnalisis)))
                        from CRD_PREA_PREANALISIS
                       where id_solicitud = @Operacion",
                    new { Operacion = operacion }));

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? string.Empty)
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "No fue posible obtener el expediente.",
                    result.Code.GetValueOrDefault(-1),
                    string.Empty);
        }

        public ErrorDto<CrConsultaPlanillaAbonoDistInicialData> CR_ConsultaPlanillaAbonoDist_Inicializar(
            int CodEmpresa,
            string cedula)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var socio = connection.QueryFirstOrDefault<CrConsultaPlanillaAbonoDistInicialData>(
                    @"select cod_institucion,
                             rtrim(cedula) as cedula,
                             rtrim(nombre) as nombre,
                             dbo.MyGetdate() as fecha,
                             year(dbo.MyGetdate()) * 100 + month(dbo.MyGetdate()) as proceso
                        from socios
                       where cedula = @Cedula",
                    new { Cedula = (cedula ?? string.Empty).Trim() });

                if (socio is null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontró la persona indicada.",
                        -1,
                        new CrConsultaPlanillaAbonoDistInicialData());
                }

                socio.deductoras = connection.Query<PlanillaAbonoDeductoraRow>(
                        "exec spAFI_Institucion_Vinculadas @Institucion, 3",
                        new { Institucion = socio.cod_institucion })
                    .Select(item => new DropDownListaGenericaModel
                    {
                        item = item.Idx.ToString(),
                        descripcion = (item.ItmX ?? string.Empty).Trim()
                    })
                    .ToList();

                return DbHelper.CreateOkResponse(socio);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al inicializar la consulta preliminar.",
                    result.Code.GetValueOrDefault(-1),
                    new CrConsultaPlanillaAbonoDistInicialData());
        }

        public ErrorDto<CrConsultaPlanillaAbonoDistUltimoData> CR_ConsultaPlanillaAbonoDist_UltimoMonto(
            int CodEmpresa,
            string cedula,
            int codInstitucion,
            int proceso)
        {
            var result = DbHelper.ExecuteSingleQuery<CrConsultaPlanillaAbonoDistUltimoData>(
                CreatePortalDb(),
                CodEmpresa,
                @"select isnull(sum(Cuota), 0) as monto,
                         isnull(max(FecPro), @Proceso) as proceso
                    from PRM_ENVIADO_DETALLE
                   where COD_INSTITUCION = @CodInstitucion
                     and cedula = @Cedula
                     and FECPRO in (
                         select max(proceso)
                           from PRM_BITACORA
                          where COD_INSTITUCION = @CodInstitucion
                            and GESTION = 'E')",
                new CrConsultaPlanillaAbonoDistUltimoData { proceso = proceso },
                new
                {
                    Cedula = (cedula ?? string.Empty).Trim(),
                    CodInstitucion = codInstitucion,
                    Proceso = proceso
                });

            return result.Code == 0 && result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al consultar el último monto de planilla.",
                    result.Code.GetValueOrDefault(-1),
                    new CrConsultaPlanillaAbonoDistUltimoData { proceso = proceso });
        }

        public ErrorDto<List<CrConsultaPlanillaAbonoDistDetalleData>> CR_ConsultaPlanillaAbonoDist_Consultar(
            int CodEmpresa,
            string cedula,
            int codInstitucion,
            int proceso,
            decimal monto,
            DateTime corte)
        {
            return DbHelper.ExecuteListQuery<CrConsultaPlanillaAbonoDistDetalleData>(
                CreatePortalDb(),
                CodEmpresa,
                @"exec spPrmCreditoDetalleAbonos
                        @CodInstitucion,
                        @Proceso,
                        @Cedula,
                        @Monto,
                        @Corte,
                        'S',
                        1,
                        1,
                        1",
                new
                {
                    CodInstitucion = codInstitucion,
                    Proceso = proceso,
                    Cedula = (cedula ?? string.Empty).Trim(),
                    Monto = monto,
                    Corte = corte
                });
        }

        private sealed class PlanillaAbonoDeductoraRow
        {
            public int Idx { get; set; }
            public string? ItmX { get; set; }
        }

        /// <summary>
        /// Método para consultar Activos y Cancelados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCrdCreditosData>> CR_ConsultaCrd_Creditos_Obtener(int CodEmpresa, string cedula, string sheetName)
        {
            return EjecutarStoredProcedureList<CrConsultaCrdCreditosData>(
                CodEmpresa,
                "spSys_Consulta_Integrada_Creditos",
                new { Cedula = cedula, Estado = sheetName });
        }

        /// <summary>
        /// Consulta tramite credito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCrdSolicitudData>> CR_ConsultaCrd_Tramite_Obtener(int CodEmpresa, string cedula, string sheetName)
        {
            return EjecutarStoredProcedureList<CrConsultaCrdSolicitudData>(
                CodEmpresa,
                "spSIFEstadoSolicitud",
                new { Cedula = cedula });
        }

        /// <summary>
        /// Consulta tramite credito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCreditosData>> CR_ConsultaCrd_Tramite_Obtener(int CodEmpresa, string cedula)
        {
            return EjecutarStoredProcedureList<CrConsultaCreditosData>(
                CodEmpresa,
                "spSIFEstadoSolicitud",
                new { Cedula = cedula });
        }

        /// <summary>
        /// Obtiene Créditos en PreAnalisis
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCrdPreanalisisData>> CR_ConsultaCrd_PreAnalisis_Obtener(int CodEmpresa, string cedula)
        {
            return EjecutarStoredProcedureList<CrConsultaCrdPreanalisisData>(
                CodEmpresa,
                "spSIFEstadoPreAnalisis",
                new { Cedula = cedula });
        }

        /// <summary>
        /// Obtiene Créditos en Incobrable
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCrdIncobrableData>> CR_ConsultaCrd_Incobrable_Obtener(int CodEmpresa, string cedula)
        {
            return EjecutarStoredProcedureList<CrConsultaCrdIncobrableData>(
                CodEmpresa,
                "spSIFEstadoIncobrable",
                new { Cedula = cedula });
        }

        #endregion
    }
}
