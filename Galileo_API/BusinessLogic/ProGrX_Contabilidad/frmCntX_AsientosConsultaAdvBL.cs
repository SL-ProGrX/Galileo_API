using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXAsientosConsultaAdvBl
    {
        private readonly FrmCntXAsientosConsultaAdvDb _db;

        public FrmCntXAsientosConsultaAdvBl(IConfiguration config)
        {
            _db = new FrmCntXAsientosConsultaAdvDb(config);
        }

        public ErrorDto<List<CntxMovimientoConsultaDto>> CntX_Movimientos_Consulta(
            int codEmpresa,
            int codContabilidad,
            CntxMovimientosFiltroDto filtros)
        {
            return _db.CntX_Movimientos_Consulta(codEmpresa, codContabilidad, filtros);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntX_TiposAsiento_Listar(int codEmpresa, int codContabilidad)
        {
            return _db.CntX_TiposAsiento_Listar(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Unidades_Listar(int codEmpresa, int codContabilidad)
        {
            return _db.CntX_Unidades_Listar(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntX_CentroCostos_Listar(int codEmpresa, int codContabilidad)
        {
            return _db.CntX_CentroCostos_Listar(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Divisas_Listar(int codEmpresa, int codContabilidad)
        {
            return _db.CntX_Divisas_Listar(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_TiposAsientos_Buscar(int codEmpresa, int cod_contabilidad)
        {
            return _db.Cntx_TiposAsientos_Buscar(codEmpresa, cod_contabilidad);
        }
    }
}