using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCatalogoRequisitosBl
    {
        private readonly FrmCrCatalogoRequisitosDb _db;

        public FrmCrCatalogoRequisitosBl(IConfiguration config)
            => _db = new FrmCrCatalogoRequisitosDb(config);

        public ErrorDto<List<CrRequisitosData>> CrCatalogoRequisitos_Obtener(int codEmpresa)
        {
            return _db.CrCatalogoRequisitos_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrCatalogosTipos_Obtener(int codEmpresa, string nivel)
        {
            return _db.CrCatalogosTipos_Obtener(codEmpresa, nivel);
        }

        public ErrorDto<List<CrRequisitosData>> CrRequisitos_Asignados_Obtener(int codEmpresa, string nivel, string codigo)
        {
            return _db.CrRequisitos_Asignados_Obtener(codEmpresa, nivel, codigo);
        }

        public ErrorDto CrCatalogoRequisitos_Guardar(int codEmpresa, string usuario, CrRequisitosData request)
        {
            return _db.CrCatalogoRequisitos_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto CrCatalogoRequisitos_Eliminar(int codEmpresa, string codigo, string usuario)
        {
            return _db.CrCatalogoRequisitos_Eliminar(codEmpresa, codigo, usuario);
        }

        public ErrorDto CrCatalogoRequisitos_Asignar(int codEmpresa, CrRequisitoAsignacionRequest request)
        {
            return _db.CrCatalogoRequisitos_Asignar(codEmpresa, request);
        }
    }
}
