using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.Security;
using Galileo.Models.ERROR;
using System.Data;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    public class CcProcesoMensualRecepcionDb
    {
        private readonly PortalDB _portalDb;
        private readonly MCobroDb _mCobroDb;
        private readonly CcProcesoMensualGeneralDb _mGeneral;

        public CcProcesoMensualRecepcionDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mCobroDb = new MCobroDb(config);
            _mGeneral = new CcProcesoMensualGeneralDb(config);
        }

        #region sbCrDesgloce
        public void CrDesgloce(IDbConnection connection, IDbTransaction transaction, CcProcesoMensualDesgloseRequest request)
        {
            var configuracion = ObtenerConfiguracionCreditoDesglose(connection, transaction, request.CodInstitucion);

            var aplicaIncon = configuracion.Aplica;
            var historicoCobroEnvio = configuracion.HistoricoCobroEnvio;
            var fechaSistema = configuracion.FechaServer;

            decimal fechaAnterior = request.FechaProceso;

            for (var i = 1; i <= historicoCobroEnvio; i++)
            {
                fechaAnterior = _mCobroDb.fxFechaProcesoAnterior(request.CodEmpresa, fechaAnterior);
            }

            EliminarCreditosProceso(connection, transaction, request.CodInstitucion, request.FechaProceso, fechaAnterior);

            var resultado = EjecutarCreditoDesglose(
                connection, transaction,
                 new CcProcesoMensualCreditoDesgloseDbRequest
                 {
                     CodInstitucion = request.CodInstitucion,
                     FechaProceso = request.FechaProceso,
                     FechaSistema = fechaSistema,
                     AplicaIncon = aplicaIncon,
                     PrimeraVez = 1,
                     Cantidad = 50
                 });

            var total = resultado.Total + 1;
            var pendientes = total == 0 ? 0 : resultado.Pendientes;
            var procesados = total == 0 ? 0 : resultado.Procesados;

            while (pendientes > 0)
            {
                resultado = EjecutarCreditoDesglose(
                    connection, transaction,
                  new CcProcesoMensualCreditoDesgloseDbRequest
                  {
                      CodInstitucion = request.CodInstitucion,
                      FechaProceso = request.FechaProceso,
                      FechaSistema = fechaSistema,
                      AplicaIncon = aplicaIncon,
                      PrimeraVez = 0,
                      Cantidad = 350
                  });

                total = resultado.Total;
                pendientes = resultado.Pendientes;
                procesados = resultado.Procesados;
            }
        }
        private static CcProcesoMensualCreditoDesgloseConfigDbModel ObtenerConfiguracionCreditoDesglose(IDbConnection connection, IDbTransaction transaction, int codInstitucion)
        {
            const string query = @"
                SELECT 
                    ISNULL(pr_cr_aplica_incon, 0) AS Aplica,
                    ISNULL(historico_cobro_envio, 0) AS HistoricoCobroEnvio,
                    dbo.MyGetdate() AS FechaServer
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualCreditoDesgloseConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }, transaction)
                ?? new CcProcesoMensualCreditoDesgloseConfigDbModel();
        }
        private static void EliminarCreditosProceso(IDbConnection connection, IDbTransaction transaction, int codInstitucion, decimal fechaProceso, decimal fechaAnterior)
        {
            const string query = @"
                DELETE FROM prm_creditos
                WHERE cod_institucion = @CodInstitucion
                  AND (
                        Fecha_Proceso <= @FechaAnterior
                        OR Fecha_Proceso = @FechaProceso
                      )";

            connection.Execute(
                query,
                new
                {
                    CodInstitucion = codInstitucion,
                    FechaProceso = fechaProceso,
                    FechaAnterior = fechaAnterior
                }, transaction);
        }
        private static CcProcesoMensualCreditoDesgloseResultadoDbModel EjecutarCreditoDesglose(IDbConnection connection, IDbTransaction transaction, CcProcesoMensualCreditoDesgloseDbRequest request)
        {
            const string query = @"
        EXEC spPrmCreditoDesgloseNew
            @CodInstitucion,
            @FechaProceso,
            @FechaSistema,
            @AplicaIncon,
            @PrimeraVez,
            @Cantidad";

            return connection.QueryFirstOrDefault<CcProcesoMensualCreditoDesgloseResultadoDbModel>(
            query,
            new
            {
                request.CodInstitucion,
                request.FechaProceso,
                FechaSistema = request.FechaSistema.Date,
                request.AplicaIncon,
                request.PrimeraVez,
                request.Cantidad
            }, transaction)
            ?? new CcProcesoMensualCreditoDesgloseResultadoDbModel();
        }
        #endregion 

        #region  sbDesglocePlanilla
        public ErrorDto<CcProcesoMensualDesglosePlanillaResponse> CcProcesoMensual_DesglosarPlanilla_Ejecutar(CcProcesoMensualDesgloseRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualDesglosePlanillaResponse>(
               "La solicitud es requerida.",
                -1,
              new CcProcesoMensualDesglosePlanillaResponse());

            }

            using var connection = DbHelper.OpenConnection(_portalDb, request.CodEmpresa);
            _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, request.CodEmpresa, "04", "PRE", request.Usuario, request.CodInstitucion, request.FechaProceso);

            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                DetallarAportes(connection, transaction, request.FechaProceso, request.CodInstitucion);

                CrDesgloce(connection, transaction, request);

                MarcarInstitucionComoDesglosada(connection, transaction, request.CodInstitucion);

                MProcesoMensualDb.SbBitacoraPlanilla(connection,
                                                  new CcProcesoMensualBitacoraPlanillaDto
                                                  {
                                                      Transaccion = "04",
                                                      CodInstitucion = request.CodInstitucion,
                                                      Proceso = request.FechaProceso,
                                                      Gestion = "R",
                                                      Usuario = request.Usuario
                                                  }, transaction);
                transaction.Commit();

                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, request.CodEmpresa, "04", "POS", request.Usuario, request.CodInstitucion, request.FechaProceso);

                return DbHelper.CreateOkResponse(
                   new CcProcesoMensualDesglosePlanillaResponse
                   {
                       Desglosado = true,
                       Mensaje = "- Detalle de Aportes y Abonos realizado satisfactoriamente...- Puede Proceder a las Aplicaciones...\""
                   });


            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return DbHelper.CreateErrorResponse<CcProcesoMensualDesglosePlanillaResponse>(
                    ex.Message,
                    -1,
                    new CcProcesoMensualDesglosePlanillaResponse());
            }
        }
        private static void DetallarAportes(IDbConnection connection, IDbTransaction transaction, decimal fechaProceso, int codInstitucion)
        {
            const string query = @"
                EXEC spPrmAporteDetalla
                    @FechaProceso,
                    @CodInstitucion";

            connection.Execute(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion
                }, transaction: transaction);
        }
        private static void MarcarInstitucionComoDesglosada(IDbConnection connection, IDbTransaction transaction, int codInstitucion)
        {
            const string query = @"
                UPDATE instituciones
                SET pr_desgloza = 1
                WHERE cod_institucion = @CodInstitucion";

            connection.Execute(
                query,
                new
                {
                    CodInstitucion = codInstitucion
                }, transaction);
        }
        #endregion 
    }
}
