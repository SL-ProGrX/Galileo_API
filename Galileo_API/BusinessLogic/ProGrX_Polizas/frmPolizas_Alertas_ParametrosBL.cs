using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmPolizasAlertasParametrosBL
    {
        private readonly FrmPolizasAlertasParametrosDb _db;

        public FrmPolizasAlertasParametrosBL(IConfiguration config)
        {
            _db = new FrmPolizasAlertasParametrosDb(config);
        }

        public ErrorDto<PolAlertasParametrosDto?> POL_Alertas_Parametros_Obtener(int CodEmpresa)
        {
            return _db.POL_Alertas_Parametros_Obtener(CodEmpresa);
        }

        public ErrorDto POL_Alertas_Parametros_Guardar(int CodEmpresa, string Usuario, PolAlertasParametrosGuardarDto param)
        { 
            return _db.POL_Alertas_Parametros_Guardar(CodEmpresa, Usuario, param);
        }

        public ErrorDto<List<PolAlertasEmailDto>> POL_Alertas_Email_Listar(int CodEmpresa)
        {
            return _db.POL_Alertas_Email_Listar(CodEmpresa);
        }

        public ErrorDto POL_Alertas_Email_Agregar(int CodEmpresa, string Usuario, PolAlertasEmailAgregarDto dto)
        {
            return _db.POL_Alertas_Email_Agregar(CodEmpresa, Usuario, dto);
        }

        public ErrorDto POL_Alertas_Email_Eliminar(int CodEmpresa, string Usuario, int ids)
        {
           
            return _db.POL_Alertas_Email_Eliminar(CodEmpresa, Usuario, ids);
        }
    }
}
