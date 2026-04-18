using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmCrPreaConfigBL
    {
        private readonly FrmCrPreaConfigDB _db;

        public FrmCrPreaConfigBL(IConfiguration config)
        {
            _db = new FrmCrPreaConfigDB(config);
        }
        public ErrorDto<CrPreaConfigListaResult> CR_Prea_Config_Lista_Obtener(int CodEmpresa,string tipo,string filtros)
        {
            return _db.CR_Prea_Config_Lista_Obtener(CodEmpresa, tipo, filtros);
        }
        public ErrorDto<CrPreaConfigListaResult> CR_Prea_Config_Lista_Export(int CodEmpresa,string tipo,string filtros)
        {
            return _db.CR_Prea_Config_Lista_Export(CodEmpresa, tipo, filtros);
        }
        public ErrorDto CR_Prea_Config_Guardar(int CodEmpresa,string usuario,string tipo,CrPreaConfigGuardarRequest request)
        {
            return _db.CR_Prea_Config_Guardar(CodEmpresa, usuario, tipo, request);
        }
        public ErrorDto CR_Prea_Config_Eliminar(int CodEmpresa,string usuario,string tipo,int id)
        {
            return _db.CR_Prea_Config_Eliminar(CodEmpresa, usuario, tipo, id);
        }
        public ErrorDto<CrPreaAvaluoCfiaDto> CR_Prea_AvaluoCFIA_Obtener(int CodEmpresa)
        {
            return _db.CR_Prea_AvaluoCFIA_Obtener(CodEmpresa);
        }
        public ErrorDto CR_Prea_AvaluoCFIA_Guardar(int CodEmpresa,string usuario,CrPreaAvaluoCfiaGuardarRequest request)
        {
            return _db.CR_Prea_AvaluoCFIA_Guardar(CodEmpresa, usuario, request);
        }
    }
}