using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprSuspensionTiposBL
    {
       private readonly FrmCprSuspensionTiposDB _db;

        public FrmCprSuspensionTiposBL(IConfiguration config)
        {
            _db = new FrmCprSuspensionTiposDB(config);
        }

        public ErrorDto<TiposSuspensionDtoList> TiposSuspension_ObtenerTodos(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            return _db.TiposSuspension_ObtenerTodos(CodEmpresa, pagina, paginacion, filtro);
        }

        public ErrorDto TiposSuspension_Guardar(int CodEmpresa, TiposSuspensionDto tiposSuspensionDto)
        {
            return _db.TiposSuspension_Guardar(CodEmpresa, tiposSuspensionDto);
        }

        public ErrorDto TiposSuspension_Eliminar(int CodEmpresa, string codSuspension)
        {
            return _db.TiposSuspension_Eliminar(CodEmpresa, codSuspension);
        }
    }
}
