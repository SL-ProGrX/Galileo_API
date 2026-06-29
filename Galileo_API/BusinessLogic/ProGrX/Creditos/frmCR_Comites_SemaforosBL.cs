using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Creditos;
using Galileo_API.DataBaseTier.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrComitesSemaforoBL
    {
        private readonly FrmCrComitesSemaforoDB _db;

        public FrmCrComitesSemaforoBL(IConfiguration config)
        {
            _db = new FrmCrComitesSemaforoDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesSemaforo_Comites_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.CR_ComitesSemaforo_Comites_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<CrComitesSemaforoData> CR_ComitesSemaforo_Obtener(int CodEmpresa, int idComite)
        {
            return _db.CR_ComitesSemaforo_Obtener(CodEmpresa, idComite);
        }

        public ErrorDto CR_ComitesSemaforo_Guardar(int CodEmpresa, CrComitesSemaforoGuardarRequest request)
        {
            return _db.CR_ComitesSemaforo_Guardar(CodEmpresa, request);
        }

        public ErrorDto<CrComitesSemaforoEmailLista> CR_ComitesSemaforo_Email_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return _db.CR_ComitesSemaforo_Email_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CrComitesSemaforoEmailLista> CR_ComitesSemaforo_Email_Lista_Export(int CodEmpresa, string parametros)
        {
            return _db.CR_ComitesSemaforo_Email_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto CR_ComitesSemaforo_Email_Agregar(int CodEmpresa, CrComitesSemaforoEmailAgregarRequest request)
        {
            return _db.CR_ComitesSemaforo_Email_Agregar(CodEmpresa, request);
        }

        public ErrorDto CR_ComitesSemaforo_Email_Eliminar(int CodEmpresa, CrComitesSemaforoEmailEliminarRequest request)
        {
            return _db.CR_ComitesSemaforo_Email_Eliminar(CodEmpresa, request);
        }
    }
}