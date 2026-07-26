using System.Diagnostics.CodeAnalysis;
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
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <returns>Contexto de configuración requerido por la consulta de créditos.</returns>
        public ErrorDto<CrConsultaCreditoContextoData> CR_ConsultaCrd_CreditoContexto_Obtener(int codEmpresa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.QueryFirstOrDefault<CrConsultaCreditoContextoData>(
                    @"select dbo.MyGetdate() as fechaServidor,
                             isnull(SysCrdPlanPago, 0) as sysPlanPagos,
                             -- fxCajasParametros('01') ya no aplica: los abonos siempre usan Cajas.
                             isnull((
                                 select top (1) ltrim(rtrim(valor))
                                 from CAJAS_PARAMETROS
                                 where cod_parametro = '03'
                             ), 'N') as cajasParametro03,
                             isnull(Portal_ID, 0) as portalId
                        from SIF_EMPRESA"));

            return result.Code == 0 && result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "No fue posible obtener el contexto de créditos.",
                    result.Code.GetValueOrDefault(-1),
                    new CrConsultaCreditoContextoData());
        }

        /// <summary>
        /// Obtiene el resumen de salidas SoS de la persona consultada.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="usuario">Usuario que realiza la consulta.</param>
        /// <returns>Resumen de pagos del programa SoS.</returns>
        public ErrorDto<List<CrConsultaSoSResumenData>> CR_ConsultaCrd_SoSResumen_Obtener(
            int codEmpresa,
            string cedula,
            string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Query<CrConsultaSoSResumenData>(
                    "exec spSOS_Consulta_Resumen @cedula, @usuario",
                    new { cedula, usuario }).AsList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<CrConsultaSoSResumenData>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "No fue posible obtener el resumen SoS.",
                    result.Code.GetValueOrDefault(-1),
                    new List<CrConsultaSoSResumenData>());
        }

        /// <summary>
        /// Obtiene las operaciones relacionadas con un proceso SoS.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="proceso">Proceso SoS seleccionado.</param>
        /// <param name="usuario">Usuario que realiza la consulta.</param>
        /// <returns>Operaciones asociadas al proceso SoS.</returns>
        public ErrorDto<List<CrConsultaSoSOperacionData>> CR_ConsultaCrd_SoSOperaciones_Obtener(
            int codEmpresa,
            string cedula,
            decimal proceso,
            string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Query<CrConsultaSoSOperacionData>(
                    "exec spSOS_Consulta_Operaciones @cedula, @proceso, @usuario",
                    new { cedula, proceso, usuario }).AsList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<CrConsultaSoSOperacionData>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "No fue posible obtener las operaciones SoS.",
                    result.Code.GetValueOrDefault(-1),
                    new List<CrConsultaSoSOperacionData>());
        }

        /// <summary>
        /// Obtiene el estado de exclusión de la persona en el proceso de devolución SoS.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="usuario">Usuario que realiza la consulta.</param>
        /// <returns>Estado de inclusión o exclusión de la persona.</returns>
        public ErrorDto<CrConsultaSoSExclusionData> CR_ConsultaCrd_SoSExclusion_Obtener(
            int codEmpresa,
            string cedula,
            string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.QueryFirstOrDefault<CrConsultaSoSExclusionData>(
                    "exec spSOS_Exclusiones_Consulta @cedula, @usuario",
                    new { cedula = cedula.Trim(), usuario = usuario.Trim() })
                ?? new CrConsultaSoSExclusionData());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new CrConsultaSoSExclusionData())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "No fue posible consultar la exclusión SoS.",
                    result.Code.GetValueOrDefault(-1),
                    new CrConsultaSoSExclusionData());
        }

        /// <summary>
        /// Incluye o excluye a la persona del proceso de devolución SoS.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="excluir">Indica si la persona debe quedar excluida.</param>
        /// <param name="usuario">Usuario que registra el movimiento.</param>
        /// <returns>Resultado del registro de la exclusión.</returns>
        public ErrorDto CR_ConsultaCrd_SoSExclusion_Guardar(
            int codEmpresa,
            string cedula,
            bool excluir,
            string usuario)
        {
            var cedulaNormalizada = (cedula ?? string.Empty).Trim();
            var usuarioNormalizado = (usuario ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cedulaNormalizada))
            {
                return DbHelper.ErrorResponse("Debe indicar la cédula.", -1);
            }

            var accion = excluir ? "A" : "I";
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                connection.Execute(
                    "exec spSOS_Exclusiones_Registro @cedula, @accion, @usuario",
                    new { cedula = cedulaNormalizada, accion, usuario = usuarioNormalizado });
                return true;
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description ?? "No fue posible actualizar la exclusión SoS.",
                    result.Code.GetValueOrDefault(-1));
            }

            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuarioNormalizado,
                Modulo = 3,
                Movimiento = excluir ? "Registra" : "Elimina",
                DetalleMovimiento = $"Exclusión del Programa SOS -> Cédula: {cedulaNormalizada}"
            });

            return DbHelper.OkResponse("Operación realizada correctamente.");
        }

        /// <summary>
        /// Calcula los valores de cancelación de una operación a una fecha de corte.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="operacion">Número de operación de crédito.</param>
        /// <param name="corte">Fecha de corte del cálculo.</param>
        /// <returns>Valores calculados para cancelar la operación.</returns>
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
                ObtenerCancelacion(connection, operacion, corte));

            return result.Code == 0 && result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "No se encontró la operación indicada.",
                    result.Code.GetValueOrDefault(-1),
                    new CrConsultaCancelacionData());
        }

        /// <summary>
        /// Obtiene la cancelación según la configuración de plan de pagos de la empresa.
        /// </summary>
        /// <param name="connection">Conexión activa de la empresa.</param>
        /// <param name="operacion">Número de operación de crédito.</param>
        /// <param name="corte">Fecha de corte del cálculo.</param>
        /// <returns>Valores de cancelación o nulo cuando la operación no existe.</returns>
        private static CrConsultaCancelacionData? ObtenerCancelacion(
            SqlConnection connection,
            int operacion,
            DateTime corte)
        {
            var sysPlanPagos = connection.QueryFirstOrDefault<int>(
                "select isnull(SysCrdPlanPago, 0) from SIF_EMPRESA");

            return sysPlanPagos == 1
                ? ObtenerCancelacionConPlanPagos(connection, operacion, corte)
                : ObtenerCancelacionLegacy(connection, operacion, corte);
        }

        /// <summary>
        /// Obtiene la cancelación calculada por el procedimiento de plan de pagos.
        /// </summary>
        /// <param name="connection">Conexión activa de la empresa.</param>
        /// <param name="operacion">Número de operación de crédito.</param>
        /// <param name="corte">Fecha de corte del cálculo.</param>
        /// <returns>Valores calculados por el procedimiento configurado.</returns>
        private static CrConsultaCancelacionData? ObtenerCancelacionConPlanPagos(
            SqlConnection connection,
            int operacion,
            DateTime corte)
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

        /// <summary>
        /// Calcula la cancelación con la lógica histórica para empresas sin plan de pagos.
        /// </summary>
        /// <param name="connection">Conexión activa de la empresa.</param>
        /// <param name="operacion">Número de operación de crédito.</param>
        /// <param name="corte">Fecha de corte del cálculo.</param>
        /// <returns>Valores de cancelación o nulo cuando la operación no existe.</returns>
        private static CrConsultaCancelacionData? ObtenerCancelacionLegacy(
            SqlConnection connection,
            int operacion,
            DateTime corte)
        {
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

            var ultimoProceso = ObtenerUltimoProcesoCancelacion(
                connection,
                operacion,
                credito);
            var interesCorriente = CalcularInteresCorrienteCancelacion(
                credito,
                corte,
                ultimoProceso);

            return new CrConsultaCancelacionData
            {
                principal = credito.saldo,
                intcor = interesCorriente,
                intmor = credito.intMora,
                cargos = credito.cargos
            };
        }

        /// <summary>
        /// Determina el último proceso que debe utilizar el cálculo de cancelación.
        /// </summary>
        /// <param name="connection">Conexión activa de la empresa.</param>
        /// <param name="operacion">Número de operación de crédito.</param>
        /// <param name="credito">Datos históricos de la operación.</param>
        /// <returns>Último proceso registrado para la operación.</returns>
        private static int ObtenerUltimoProcesoCancelacion(
            SqlConnection connection,
            int operacion,
            CrConsultaCancelacionLegacyRow credito)
        {
            if (credito.moraCuota <= 0)
            {
                return credito.fecUlt;
            }

            var procesoMora = connection.QueryFirstOrDefault<int?>(
                @"select max(fechap)
                    from MOROSIDAD
                   where estado = 'A'
                     and id_solicitud = @Operacion",
                new { Operacion = operacion }).GetValueOrDefault();

            return Math.Max(credito.fecUlt, procesoMora);
        }

        /// <summary>
        /// Calcula el interés corriente de la cancelación histórica.
        /// </summary>
        /// <param name="credito">Datos históricos de la operación.</param>
        /// <param name="corte">Fecha de corte del cálculo.</param>
        /// <param name="ultimoProceso">Último proceso aplicado a la operación.</param>
        /// <returns>Interés corriente calculado.</returns>
        private static decimal CalcularInteresCorrienteCancelacion(
            CrConsultaCancelacionLegacyRow credito,
            DateTime corte,
            int ultimoProceso)
        {
            if (credito.principalAtrasado >= credito.saldo)
            {
                return 0;
            }

            var procesoCorte = corte.Year * 100 + corte.Month;
            if (procesoCorte == credito.priDeduc && ultimoProceso < procesoCorte)
            {
                return credito.saldo * credito.interesv / 36000m * corte.Day;
            }

            if (procesoCorte <= credito.priDeduc || ultimoProceso > procesoCorte)
            {
                return 0;
            }

            var meses = CalcularMesesCancelacion(ultimoProceso, procesoCorte);
            return credito.saldo * credito.interesv / 36000m *
                   (corte.Day + meses * 30m);
        }

        /// <summary>
        /// Cuenta los meses completos entre dos procesos AAAAMM para la cancelación.
        /// </summary>
        /// <param name="ultimoProceso">Proceso inicial en formato AAAAMM.</param>
        /// <param name="procesoCorte">Proceso final en formato AAAAMM.</param>
        /// <returns>Cantidad de meses completos entre ambos procesos.</returns>
        private static int CalcularMesesCancelacion(int ultimoProceso, int procesoCorte)
        {
            var meses = -1;
            var procesoTemporal = ultimoProceso;
            while (procesoCorte > procesoTemporal)
            {
                meses++;
                procesoTemporal = SiguienteProceso(procesoTemporal);
            }

            return meses;
        }

        /// <summary>
        /// Obtiene el expediente de preanálisis relacionado con una operación.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="operacion">Número de operación de crédito.</param>
        /// <returns>Identificador del expediente de preanálisis.</returns>
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

        /// <summary>
        /// Inicializa los datos requeridos por la consulta preliminar de distribución de abonos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <returns>Datos de la persona y deductoras disponibles.</returns>
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

        /// <summary>
        /// Obtiene el último monto enviado para la distribución de abonos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="codInstitucion">Código de la institución deductora.</param>
        /// <param name="proceso">Proceso utilizado como valor predeterminado.</param>
        /// <returns>Último monto y proceso encontrados.</returns>
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

        /// <summary>
        /// Consulta el detalle preliminar de distribución de abonos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa activa.</param>
        /// <param name="cedula">Identificación de la persona.</param>
        /// <param name="codInstitucion">Código de la institución deductora.</param>
        /// <param name="proceso">Proceso de planilla consultado.</param>
        /// <param name="monto">Monto que se debe distribuir.</param>
        /// <param name="corte">Fecha de corte de la consulta.</param>
        /// <returns>Detalle de operaciones y montos distribuidos.</returns>
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
            [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "Dapper asigna esta propiedad por reflexión desde una columna de consulta.")]
            public int Idx { get; set; }
            [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "Dapper asigna esta propiedad por reflexión desde una columna de consulta.")]
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
