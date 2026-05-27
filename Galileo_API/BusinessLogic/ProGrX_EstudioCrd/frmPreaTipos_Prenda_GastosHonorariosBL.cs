using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaTiposPrendaGastosHonorariosBL
    {
        private readonly FrmPreaTiposPrendaGastosHonorariosDB _db;

        public FrmPreaTiposPrendaGastosHonorariosBL(IConfiguration config)
        {
            _db = new FrmPreaTiposPrendaGastosHonorariosDB(config);
        }

        public ErrorDto<CrPreaConfigListaResult> CR_PreaTipos_Prenda_GastosHonorarios_Lista_Obtener(int CodEmpresa, string tipo, string filtros)
        {
            return _db.CR_PreaTipos_Prenda_GastosHonorarios_Lista_Obtener(CodEmpresa, tipo, filtros);
        }

        public ErrorDto<CrPreaConfigListaResult> CR_PreaTipos_Prenda_GastosHonorarios_Lista_Export(int CodEmpresa, string tipo, string filtros)
        {
            return _db.CR_PreaTipos_Prenda_GastosHonorarios_Lista_Export(CodEmpresa, tipo, filtros);
        }

        public ErrorDto CR_PreaTipos_Prenda_GastosHonorarios_Guardar(int CodEmpresa, string usuario, string tipo, CrPreaConfigGuardarRequest request)
        {
            return _db.CR_PreaTipos_Prenda_GastosHonorarios_Guardar(CodEmpresa, usuario, tipo, request);
        }

        public ErrorDto CR_PreaTipos_Prenda_GastosHonorarios_Eliminar(int CodEmpresa, string usuario, string tipo, int id)
        {
            return _db.CR_PreaTipos_Prenda_GastosHonorarios_Eliminar(CodEmpresa, usuario, tipo, id);
        }
    }
}


