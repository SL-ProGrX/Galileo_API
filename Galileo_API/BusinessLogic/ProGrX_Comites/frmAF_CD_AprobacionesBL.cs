using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdAprobacionesBl
    {
        private readonly FrmAfCdAprobacionesDb _db;

        public FrmAfCdAprobacionesBl(IConfiguration config)
        {
            _db = new FrmAfCdAprobacionesDb(config);
        }
        public ErrorDto<List<AfcdAprobacionDto>> Listar(int codEmpresa, int banco)
        {
            return _db.Listar(codEmpresa, banco);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Bancos(int codEmpresa)
        {
            return _db.Bancos(codEmpresa);
        }


        public ErrorDto<bool> Aprobar(AfcdAprobacionRequest req)
        {
            return _db.Aprobar(req);
        }


        public ErrorDto<bool> Rechazar(AfcdRechazoRequest req)
        {
            return _db.Rechazar(req);
        }

        public ErrorDto<OficinaUsuarioAprobacionDto> Oficina_ObtenerPorUsuario(int codEmpresa, string usuario)
        {
            return _db.Oficina_ObtenerPorUsuario(codEmpresa, usuario);
        }
    }
}