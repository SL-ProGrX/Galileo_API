using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.CargaArchivos;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualEstadoModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualCargaArchivos;

namespace Galileo_API.BusinessLogic.ProGrX_Procesos.frmCC_ProcesoMensualBL
{
    public class CcProcesoMensualBL
    {

        private readonly CcProcesoMensualEstadoDB _dbEstado;
        private readonly CcProcesoMensualBitacoraDb _dbBitacora;
        private readonly CcProcesoMensualCargaArchivosDb _dbCargaArchivos;
        
        public CcProcesoMensualBL(IConfiguration config)
        {
            _dbEstado = new CcProcesoMensualEstadoDB(config);
            _dbBitacora = new CcProcesoMensualBitacoraDb(config);
            _dbCargaArchivos = new CcProcesoMensualCargaArchivosDb(config);
         
        }

        public ErrorDto<CcProcesoMensualInicialResponse> CcProcesoMensual_Inicial_Obtener(int codEmpresa, int gInstitucion, string usuario)
        {
            return _dbEstado.CcProcesoMensual_Inicial_Obtener(codEmpresa, gInstitucion, usuario);
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


            if (request.TipoCarga != CcProcesoMensualCargaDeduccionesTipo.sbCargaDeduc_ExcelNew
                && request.TipoCarga != CcProcesoMensualCargaDeduccionesTipo.sbCargaDeduc_Csv_Integra
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
    }
}
