using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXPlantillaAsientosBl
    {
        private readonly FrmCntXPlantillaAsientosDb _db;

        public FrmCntXPlantillaAsientosBl(IConfiguration config)
        {
            _db = new FrmCntXPlantillaAsientosDb(config);
        }

        public ErrorDto<CntxPlantillaResponseDto> Consultar(int codEmpresa, int codPlantilla)
            => _db.Consultar(codEmpresa, codPlantilla);

        public ErrorDto<int> Insertar(int codEmpresa, CntxPlantillaSaveDto modelo)
            => _db.Insertar(codEmpresa, modelo);

        public ErrorDto<int> Actualizar(int codEmpresa, CntxPlantillaSaveDto modelo)
            => _db.Actualizar(codEmpresa, modelo);

        public ErrorDto<int> Borrar(int codEmpresa, int codPlantilla)
            => _db.Borrar(codEmpresa, codPlantilla);

        public ErrorDto<int?> Scroll(int codEmpresa, int? codigoActual, int direccion)
            => _db.Scroll(codEmpresa, codigoActual, direccion);

        public ErrorDto<List<CntxPlantillaDto>> BuscarPlantillas(int codEmpresa)
            => _db.BuscarPlantillas(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_TiposAsientos_Buscar(int codEmpresa,int cod_contabilidad)
        {

            return _db.Cntx_TiposAsientos_Buscar(codEmpresa,cod_contabilidad
            );

        }
    }
}