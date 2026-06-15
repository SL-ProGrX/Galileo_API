
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.CargaArchivos;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualEstadoModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualCargaArchivos;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using Galileo.Models.Security;
using System.Globalization;
using Galileo_API.DataBaseTier;

namespace Galileo_API.BusinessLogic.ProGrX_Procesos.frmCC_ProcesoMensualBL
{
    public class CcProcesoMensualBL
    {
        private readonly PortalDB _portalDb;
        private readonly CcProcesoMensualEstadoDB _dbEstado;
        private readonly CcProcesoMensualBitacoraDb _dbBitacora;
        private readonly CcProcesoMensualCargaArchivosDb _dbCargaArchivos;
        private readonly CcProcesoMensualEnvioDb _dbEnvio;
        private readonly CcProcesoMensualRecepcionDb _dbRecepcion;
        private readonly CcProcesoMensualAplicacionAhorrosDb _dbAplicacionAhorros;
        private readonly int vModulo = 3;
        private readonly MSecurityMainDb _Security_MainDB;


        public CcProcesoMensualBL(IConfiguration config)
        {
            _dbEstado = new CcProcesoMensualEstadoDB(config);
            _dbBitacora = new CcProcesoMensualBitacoraDb(config);
            _dbCargaArchivos = new CcProcesoMensualCargaArchivosDb(config);
            _dbEnvio = new CcProcesoMensualEnvioDb(config);
            _portalDb = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
            _dbRecepcion = new CcProcesoMensualRecepcionDb(config);
            _dbAplicacionAhorros = new CcProcesoMensualAplicacionAhorrosDb(config);
        }
        public ErrorDto<CcProcesoMensualInicialResponse> CcProcesoMensual_Inicial_Obtener(int codEmpresa, int gInstitucion)
        {
            return _dbEstado.CcProcesoMensual_Inicial_Obtener(codEmpresa, gInstitucion);
        }
        public ErrorDto<List<CcProcesoMensualBitacoraDbModel>> CcProcesoMensual_Bitacora_Obtener(int codEmpresa, int gInstitucion, int proceso)
        {
            return _dbBitacora.CcProcesoMensual_Bitacora_Obtener(codEmpresa, gInstitucion, proceso);
        }
        public ErrorDto<CcProcesoMensualValidaPasoResponse> CcProcesoMensual_ValidaPaso(int codEmpresa, int codInstitucion, decimal fechaProceso, string transaccion = "08")
        {
            return _dbEstado.CcProcesoMensual_ValidaPaso(codEmpresa, codInstitucion, fechaProceso, transaccion);
        }
        public ErrorDto<CcProcesoMensualCargaDeduccionesResponse> CcProcesoMensual_CargarDeducciones(CcProcesoMensualCargaDeduccionesRequest request)
        {
            var reglas = CcProcesoMensualCargaDeduccionesConfig.ObtenerReglas(request.TipoCarga);


            if (request.TipoCarga != "30"
                && request.TipoCarga != "02"
                && reglas.Count == 0)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualCargaDeduccionesResponse>(
                    "El tipo de carga no tiene reglas configuradas.",
                    -1,
                    new CcProcesoMensualCargaDeduccionesResponse());
            }

            return _dbCargaArchivos.CargarDeduccionesGenerico(request, reglas);
        }
        public ErrorDto<CcProcesoMensualEstadoResponse> CcProcesoMensual_EstadoActualProceso_Obtener(int codEmpresa, int gInstitucion)
        {
            return _dbEstado.CcProcesoMensual_EstadoActualProceso_Obtener(codEmpresa, gInstitucion);
        }
        public ErrorDto<CcProcesoMensualCargaConfigDbModel> CcProcesoMensual_DatosInstitucion_Obtener(int codEmpresa, int codInstitucion)
        {
            return _dbEstado.DatosInstitucion_Obtener(codEmpresa, codInstitucion);
        }
        public ErrorDto<CcProcesoMensualCambiarFechaResponse> CcProcesoMensual_CambiarFechaProceso_Ejecutar(int codEmpresa, CcProcesoMensualCambiarFechaRequest request)
        {
            try
            {
                if (request is null)
                {
                    return DbHelper.CreateErrorResponse<CcProcesoMensualCambiarFechaResponse>(
                   "La solicitud es requerida.",
                   -1,
                   new CcProcesoMensualCambiarFechaResponse());

                }


                if (request.Anio.Trim() == "" || request.Mes is < 1 or > 12 || request.Quincena < 0)
                {
                    return DbHelper.CreateErrorResponse<CcProcesoMensualCambiarFechaResponse>(
                        "Los datos del período no son válidos.",
                        -1,
                        new CcProcesoMensualCambiarFechaResponse());

                }

                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var fechaProcesoTexto = $"{request.Anio}{request.Mes:00}.{request.Quincena}";
                var fechaProceso = decimal.Parse(fechaProcesoTexto, CultureInfo.InvariantCulture);

                var fechaCorte = _dbEnvio.ObtenerFechaCorteProceso(
                    connection,
                    fechaProceso);

                _dbEnvio.ActualizarInstitucionCambioFechaProceso(
                    connection,
                    request.CodInstitucion,
                    fechaCorte);

                var cambiaFechaFormalizaciones =
                    _dbEnvio.ObtenerIndicadorCambioFechaProceso(
                        connection,
                        request.CodInstitucion);

                if (cambiaFechaFormalizaciones)
                {
                    _dbEnvio.ActualizarFechaFormalizaciones(
                        connection,
                        fechaCorte);
                }

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = request.Usuario,
                    DetalleMovimiento = $"PRM-CREDITO Cambia Fecha Proceso Inst:{request.CodInstitucion}",
                    Movimiento = "Aplica - WEB",
                    Modulo = vModulo
                });

                MProcesoMensualDb.SbBitacoraPlanilla(connection,
                                                    new CcProcesoMensualBitacoraPlanillaDto
                                                    {
                                                        Transaccion = "01",
                                                        CodInstitucion = request.CodInstitucion,
                                                        Proceso = fechaProceso,
                                                        Gestion = "E",
                                                        Usuario = request.Usuario
                                                    });


                return DbHelper.CreateOkResponse<CcProcesoMensualCambiarFechaResponse>(
                   new CcProcesoMensualCambiarFechaResponse
                   {
                       FechaProceso = fechaProceso,
                       FechaCorte = fechaCorte,
                       Mensaje = $"La fecha de proceso fue cambiada a :  {request.Anio}{request.Mes:00} "
                   });


            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualCambiarFechaResponse>(
                   ex.Message,
                  -1,
                  new CcProcesoMensualCambiarFechaResponse());

            }
        }
        public ErrorDto<CcProcesoMensualDesglosePlanillaResponse> CcProcesoMensual_DesglosarPlanilla_Ejecutar(CcProcesoMensualDesgloseRequest request)
        {
            return _dbRecepcion.CcProcesoMensual_DesglosarPlanilla_Ejecutar(request);  
        }
        public ErrorDto<CcProcesoMensualAhorros> CcProcesoMensual_Ahorros_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario)
        {
            return _dbAplicacionAhorros.CcProcesoMensual_Ahorros_Aplicar(codEmpresa, codInstitucion, fechaProceso, usuario);

        }
        public ErrorDto<CcProcesoMensualAhorroReporteModel> CcProcesoMensual_ParametrosAhorroReporte_Obtener( int codEmpresa,int codInstitucion)
        {
            return _dbAplicacionAhorros.CcProcesoMensual_ParametrosAhorroReporte_Obtener(codEmpresa, codInstitucion);

        }
    }
}
