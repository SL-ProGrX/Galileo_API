using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class MComprasBl
    {
        private readonly IConfiguration _config;

        public MComprasBl(IConfiguration config)
        {
            _config = config;
        }

        public List<CargoPeriodicoDto> sbCprCboCargosPer(int CodEmpresa)
        {
            return new MComprasDB(_config).sbCprCboCargosPer(CodEmpresa);
        }

        public bool fxCprCambiaFecha(int CodEmpresa, string vUsuario)
        {
            return new MComprasDB(_config).fxCprCambiaFecha(CodEmpresa, vUsuario);
        }

        public ErrorDto sbCprOrdenesDespacho(int CodEmpresa, string vOrden)
        {
            return new MComprasDB(_config).sbCprOrdenesDespacho(CodEmpresa, vOrden);
        }

        public List<TipoOrdenDto> sbCprCboTiposOrden(int CodEmpresa)
        {
            return new MComprasDB(_config).sbCprCboTiposOrden(CodEmpresa);
        }

        public ErrorDto<UnidadesDtoList> UnidadesObtener(int CodEmpresa, string? filtros)
        {
            return new MComprasDB(_config).UnidadesObtener(CodEmpresa, filtros);
        }

        public ErrorDto<CentroCostoDtoList> CentroCostosObtener(int CodEmpresa, string? filtros)
        {
            return new MComprasDB(_config).CentroCostosObtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<CatalogoDto>> CatalogoCompras_Obtener(int CodEmpresa, string tipo)
        {
            return new MComprasDB(_config).CatalogoCompras_Obtener(CodEmpresa, tipo);
        }
    }
}
