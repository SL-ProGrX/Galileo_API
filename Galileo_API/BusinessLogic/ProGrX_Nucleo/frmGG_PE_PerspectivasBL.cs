using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.GG_PE;

namespace Galileo.BusinessLogic
{
    public class FrmGgPePerspectivasBL(IConfiguration config)
    {
        private readonly FrmGgPePerspectivasDB _db = new(config);


        public ErrorDto<PePerspectivasDto> PePerspectiva_Obtener(int CodEmpresa, int perspectiva)
        {
            return _db.PePerspectiva_Obtener(CodEmpresa, perspectiva);
        }

        public ErrorDto<PePerspectivasDto> PePerspectiva_Scroll(int CodEmpresa, int scroll, int? perspectiva)
        {
            return _db.PePerspectiva_Scroll(CodEmpresa, scroll, perspectiva);
        }

        public ErrorDto PePerspectiva_Guardar(int CodEmpresa, PePerspectivasDto perspectiva)
        {
            return _db.PePerspectiva_Guardar(CodEmpresa, perspectiva);
        }

        public ErrorDto PePerspectiva_Eliminar(int CodEmpresa, int perspectiva)
        {
            return _db.PePerspectiva_Eliminar(CodEmpresa, perspectiva);
        }

        public ErrorDto<List<PePerspectivasDto>> PePlanesLista_Obtener(int CodEmpresa)
        {
            return _db.PePlanesLista_Obtener(CodEmpresa);
        }

        public ErrorDto<PePerspectivasDatosLista> PePerpectivasLista_Obtener(int CodEmpresa, string Jfiltros)
        {
            return _db.PePerpectivasLista_Obtener(CodEmpresa, Jfiltros);
        }

    }
}