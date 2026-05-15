using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndNotificacionesBL
    {
        private readonly FrmFndNotificacionesDB _db;

        public FrmFndNotificacionesBL(IConfiguration? config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _db = new FrmFndNotificacionesDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Notificaciones_Operadora_Obtener(int CodEmpresa)
        {
            return _db.Fnd_Notificaciones_Operadora_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Notificaciones_TipoMov_Obtener(int CodEmpresa)
        {
            return _db.Fnd_Notificaciones_TipoMov_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Notificaciones_Planes_Obtener(int CodEmpresa, string operadora)
        {
            return _db.Fnd_Notificaciones_Planes_Obtener(CodEmpresa, operadora);
        }

        public ErrorDto<string> Fnd_Notificaciones_Plan_Obtener(int CodEmpresa, string operadora, string plan)
        {
            return _db.Fnd_Notificaciones_Plan_Obtener(CodEmpresa, operadora, plan);
        }

        public ErrorDto<List<FndNotificacionData>> Fnd_Notificaciones_Scroll_Obtener(int codEmpresa, int codOperadora, string codPlanActual, bool siguiente)
        {
            return _db.Fnd_Notificaciones_Scroll_Obtener(codEmpresa, codOperadora, codPlanActual, siguiente);
        }

        public ErrorDto<FndNotificacionData> Fnd_Notificaciones_Obtener(int codEmpresa, string pNotifica)
        {
            return _db.Fnd_Notificaciones_Obtener(codEmpresa, pNotifica);
        }

        public ErrorDto<List<FndNotificacionData>> Fnd_Notifica_List(int codEmpresa, int codOperadora, string codigo)
        {
            return _db.Fnd_Notifica_List(codEmpresa, codOperadora, codigo);
        }

        public ErrorDto<int> Fnd_Notificaciones_Guardar(int CodEmpresa, FndNotificacionData data)
        {
            return _db.Fnd_Notificaciones_Guardar(CodEmpresa, data);
        }

    }
}