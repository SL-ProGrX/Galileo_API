using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXConAsientosBl
    {
        private readonly FrmCntXConAsientosDb _db;

        public FrmCntXConAsientosBl(IConfiguration config)
        {
            _db = new FrmCntXConAsientosDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Consolidaciones_Listar(int codEmpresa, int codContabilidad)
        {
            return _db.Consolidaciones_Listar(codEmpresa, codContabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Asientos_Buscar(int codEmpresa, int codContabilidad, int? codConsolida)
        {
            return _db.Asientos_Buscar(codEmpresa, codContabilidad, codConsolida);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Obtener(int codEmpresa, int cod_contabilidad)
        {
            return _db.Unidades_Obtener(codEmpresa, cod_contabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Divisas_Obtener(int codEmpresa, int cod_contabilidad)
        {
            return _db.Divisas_Obtener(codEmpresa, cod_contabilidad);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CentroCosto_Obtener(int codEmpresa, int cod_contabilidad, string codUnidad)
        {
            return _db.CentroCosto_Obtener(codEmpresa, cod_contabilidad, codUnidad);
        }

        public ErrorDto<List<CntxConAsientoDetalleDto>> AsientoDetalle_Obtener(int codEmpresa,int codContabilidad,
        int? codConsolida,string? codAsiento)
        {
            return _db.AsientoDetalle_Obtener(codEmpresa,codContabilidad,codConsolida,codAsiento
            );
        }

        public ErrorDto<bool> GuardarAsiento(CntxConAsientoGuardarDto request)
        {
            return _db.GuardarAsiento(request);
        }

        public ErrorDto<bool> EliminarAsiento(int codEmpresa, int codContabilidad,int codConsolida,string codAsiento,string usuario)
        {
            return _db.EliminarAsiento(codEmpresa, codContabilidad, codConsolida, codAsiento, usuario);
        }
    }
}