using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXUnidadesBl
    {
        private readonly FrmCntXUnidadesDb _db;

        public FrmCntXUnidadesBl(IConfiguration config)
        {
            _db = new FrmCntXUnidadesDb(config);
        }

        public ErrorDto<List<CntXUnidadDto>> CntX_Unidades_Listar(int codEmpresa)
        {
            return _db.CntX_Unidades_Listar(codEmpresa);
        }

        public ErrorDto<bool> CntX_Unidades_Guardar(int codEmpresa, string usuario, CntXUnidadGuardarDto dto)
        {
            return _db.CntX_Unidades_Guardar(codEmpresa, usuario, dto);
        }

        public ErrorDto<bool> CntX_Unidades_Eliminar(int codEmpresa, string usuario, string codUnidad)
        {
            return _db.CntX_Unidades_Eliminar(codEmpresa, usuario, codUnidad);
        }

        public ErrorDto<List<CntXUnidadActivaDto>> CntX_Unidades_Activas_Listar(int codEmpresa)
        {
            return _db.CntX_Unidades_Activas_Listar(codEmpresa);
        }

        public ErrorDto<List<CntXCentroCostoDto>> CntX_CentrosCosto_PorUnidad(int codEmpresa, string codUnidad)
        {
            return _db.CntX_CentrosCosto_PorUnidad(codEmpresa, codUnidad);
        }

        public ErrorDto<bool> CntX_Unidades_CC_Guardar(int codEmpresa, string usuario, CntXUnidadCCGuardarDto dto)
        {
            return _db.CntX_Unidades_CC_Guardar(codEmpresa, usuario, dto);
        }

        public ErrorDto<List<CntXCentroCostoDto>> CntX_Unidades_CC_Consulta(
            int codEmpresa,
            string codUnidad
        )
        {
            return _db.CntX_Unidades_CC_Consulta(codEmpresa, codUnidad);
        }

    }
}
