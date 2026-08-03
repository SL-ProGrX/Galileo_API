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

        public ErrorDto<List<CntXUnidadDto>> CntX_Unidades_Listar(int codEmpresa, int codContabilidad)
        {
            return _db.CntX_Unidades_Listar(codEmpresa, codContabilidad);
        }

        public ErrorDto<bool> CntX_Unidades_Guardar(int codEmpresa, int codContabilidad, string usuario, CntXUnidadGuardarDto dto)
        {
            return _db.CntX_Unidades_Guardar(codEmpresa, codContabilidad, usuario, dto);
        }

        public ErrorDto<bool> CntX_Unidades_Eliminar(int codEmpresa, int codContabilidad, string usuario, string codUnidad)
        {
            return _db.CntX_Unidades_Eliminar(codEmpresa, codContabilidad, usuario, codUnidad);
        }

        public ErrorDto<List<CntXUnidadActivaDto>> CntX_Unidades_Activas_Listar(int codEmpresa, int codContabilidad)
        {
            return _db.CntX_Unidades_Activas_Listar(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<CntXCentroCostoDto>> CntX_CentrosCosto_PorUnidad(int codEmpresa, int codContabilidad, string codUnidad)
        {
            return _db.CntX_CentrosCosto_PorUnidad(codEmpresa, codContabilidad, codUnidad);
        }

        public ErrorDto<bool> CntX_Unidades_CC_Guardar(int codEmpresa, int codContabilidad, string usuario, CntXUnidadCCGuardarDto dto)
        {
            return _db.CntX_Unidades_CC_Guardar(codEmpresa, codContabilidad, usuario, dto);
        }

        public ErrorDto<List<CntXCentroCostoDto>> CntX_Unidades_CC_Consulta(
            int codEmpresa,
            int codContabilidad,
            string codUnidad
        )
        {
            return _db.CntX_Unidades_CC_Consulta(codEmpresa, codContabilidad, codUnidad);
        }

    }
}
