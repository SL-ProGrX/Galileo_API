using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXUtilEliminaAsientosBl
    {
        private readonly FrmCntXUtilEliminaAsientosDb _db;

        public FrmCntXUtilEliminaAsientosBl(IConfiguration config)
        {
            _db = new FrmCntXUtilEliminaAsientosDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_TiposAsientos_Buscar(int codEmpresa, int cod_contabilidad)
        {
            return _db.Cntx_TiposAsientos_Buscar(codEmpresa, cod_contabilidad);
        }

        public ErrorDto<int> Cntx_Util_Asientos_Calcular(int codEmpresa, int cod_contabilidad, string tipo_asiento, string desde,
            string hasta, int anio, int mes)
        {
            return _db.Cntx_Util_Asientos_Calcular(codEmpresa, cod_contabilidad, tipo_asiento, desde, hasta, anio, mes);
        }

        public ErrorDto<bool> Cntx_Util_Asientos_Eliminar(CntxEliminarAsientosRequestDto request)
        {
            return _db.Cntx_Util_Asientos_Eliminar(request);
        }

        public ErrorDto<CntxPeriodoActualDto> Cntx_PeriodoActual_Obtener(int codEmpresa, int cod_contabilidad)
        {
            return _db.Cntx_PeriodoActual_Obtener(codEmpresa, cod_contabilidad);
        }
    }


}
