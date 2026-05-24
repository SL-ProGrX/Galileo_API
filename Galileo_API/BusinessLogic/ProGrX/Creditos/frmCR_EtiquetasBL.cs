using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrEtiquetasBl
    {
        private readonly FrmCrEtiquetasDb _db;

        public FrmCrEtiquetasBl(IConfiguration config)
        {
            _db = new FrmCrEtiquetasDb(config);
        }

        public ErrorDto<List<CrEtiquetaData>> CrEtiquetas_Obtener(int codEmpresa)
            => _db.CrEtiquetas_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrEtiquetas_Requisitos_Obtener(int codEmpresa)
            => _db.CrEtiquetas_Requisitos_Obtener(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> CrEtiquetas_TagsCombo_Obtener(int codEmpresa)
            => _db.CrEtiquetas_TagsCombo_Obtener(codEmpresa);

        public ErrorDto<CrEtiquetaNotificacionData> CrEtiquetas_Notificacion_Obtener(int codEmpresa, string tag_codigo)
            => _db.CrEtiquetas_Notificacion_Obtener(codEmpresa, tag_codigo);

        public ErrorDto CrEtiquetas_Guardar(int codEmpresa, CrEtiquetaGuardarRequest request)
            => _db.CrEtiquetas_Guardar(codEmpresa, request);

        public ErrorDto CrEtiquetas_Eliminar(int codEmpresa, CrEtiquetaEliminarRequest request)
            => _db.CrEtiquetas_Eliminar(codEmpresa, request);

        public ErrorDto CrEtiquetas_Notificacion_Guardar(int codEmpresa, CrEtiquetaNotificacionGuardarRequest request)
            => _db.CrEtiquetas_Notificacion_Guardar(codEmpresa, request);

        public ErrorDto CrEtiquetas_Notificacion_Eliminar(int codEmpresa, CrEtiquetaNotificacionEliminarRequest request)
            => _db.CrEtiquetas_Notificacion_Eliminar(codEmpresa, request);
    }
}