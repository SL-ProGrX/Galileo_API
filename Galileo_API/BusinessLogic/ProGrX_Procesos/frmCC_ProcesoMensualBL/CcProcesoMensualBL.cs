using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR; 
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB; 
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualEstadoModels;

namespace Galileo_API.BusinessLogic.ProGrX_Procesos.frmCC_ProcesoMensualBL
{
    public class CcProcesoMensualBL
    {
         
        private readonly CcProcesoMensualEstadoDB _dbEstado;
        private readonly CcProcesoMensualBitacoraDb _dbBitacora;

        public CcProcesoMensualBL(IConfiguration config)
        {           
            _dbEstado = new CcProcesoMensualEstadoDB(config); 
            _dbBitacora = new CcProcesoMensualBitacoraDb(config);
        }

        public ErrorDto<CcProcesoMensualInicialResponse> CcProcesoMensual_Inicial_Obtener(int codEmpresa,int gInstitucion, string usuario)
        {
            return _dbEstado.CcProcesoMensual_Inicial_Obtener(codEmpresa, gInstitucion, usuario);
        }
        public ErrorDto<List<CcProcesoMensualBitacoraDbModel>> CcProcesoMensual_Bitacora_Obtener(int codEmpresa, int gInstitucion, int proceso)
        {
            return _dbBitacora.CcProcesoMensual_Bitacora_Obtener(codEmpresa, gInstitucion, proceso);
        }
        public ErrorDto<CcProcesoMensualValidaPasoResponse> CcProcesoMensual_ValidaPaso(int codEmpresa, int codInstitucion, decimal fechaProceso, string transaccion = "08")
        {
            return _dbEstado.CcProcesoMensual_ValidaPaso(codEmpresa, codInstitucion, fechaProceso, transaccion  );
        }

    }
}
