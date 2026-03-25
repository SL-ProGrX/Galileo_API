using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCatalogoDestinosBl
    {
        private readonly FrmCrCatalogoDestinosDb _db;

        public FrmCrCatalogoDestinosBl(IConfiguration config) 
            => _db = new FrmCrCatalogoDestinosDb(config);

        public ErrorDto<List<CrCatalogoDestinoData>> CrCatalogoDestinos_Obtener(int codEmpresa)
        {
            return _db.CrCatalogoDestinos_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrCatalogos_Obtener(int codEmpresa, string tipo)
        {
            return _db.CrCatalogos_Obtener(codEmpresa, tipo);
        }

        public ErrorDto<List<CrCatalogoDestinoData>> CrCatalogoDestinos_Asignados_Obtener(int codEmpresa, string codigo)
        {
            return _db.CrCatalogoDestinos_Asignados_Obtener(codEmpresa, codigo);
        }

        public ErrorDto CrCatalogoDestinos_Asignar(int codEmpresa, string codDestino, string catalogo, bool isChecked)
        {
            return _db.CrCatalogoDestinos_Asignar(codEmpresa, codDestino, catalogo, isChecked);
        }

        public ErrorDto CrCatalogoDestinos_Guardar(int codEmpresa, string usuario, CrCatalogoDestinoData request)
        {
            return _db.CrCatalogoDestinos_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto CrCatalogoDestinos_Eliminar(int codEmpresa, string codDestino, string usuario)
        {
            return _db.CrCatalogoDestinos_Eliminar(codEmpresa, codDestino, usuario);
        }
    }
}
