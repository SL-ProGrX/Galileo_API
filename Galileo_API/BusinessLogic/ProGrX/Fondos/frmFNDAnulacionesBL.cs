using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndAnulacionesBl
    {
        private readonly FrmFndAnulacionesDb DbFndAnulaciones;

        public FrmFndAnulacionesBl(IConfiguration config)
        {
            DbFndAnulaciones = new FrmFndAnulacionesDb(config);
        }

        public ErrorDto<FndAnulacionesDto> FND_Anulaciones_Obtener(int CodEmpresa, string Params)
        {
            FndAnulacionesParams Parametros = JsonConvert.DeserializeObject<FndAnulacionesParams>(Params) ?? new FndAnulacionesParams();
            return DbFndAnulaciones.FND_Anulaciones_Obtener(CodEmpresa, Parametros);
        }

        public ErrorDto<List<FndAnulacionesSubCuentasDto>> FND_Anulaciones_SubCuentas_Obtener(int CodEmpresa, string Params)
        {
            FndAnulacionesParams Parametros = JsonConvert.DeserializeObject<FndAnulacionesParams>(Params) ?? new FndAnulacionesParams();
            return DbFndAnulaciones.FND_Anulaciones_SubCuentas_Obtener(CodEmpresa, Parametros);
        }

        public ErrorDto<FndAutorizaDto> FND_Anulaciones_Autoriza_Obtener(int CodEmpresa, string Plan, string Usuario)
        {
            return DbFndAnulaciones.FND_Anulaciones_Autoriza_Obtener(CodEmpresa, Plan, Usuario);
        }

        public ErrorDto<FndAnulacionesEstadoGestionDto> FND_Anulaciones_SolicitaAutorizacion_Obtener(int CodEmpresa, string Params)
        {
            FndAnulacionesParams Parametros = JsonConvert.DeserializeObject<FndAnulacionesParams>(Params) ?? new FndAnulacionesParams();
            return DbFndAnulaciones.FND_Anulaciones_SolicitaAutorizacion_Obtener(CodEmpresa, Parametros);
        }

        public ErrorDto<FndAnulacionesEstadoGestionDto> FND_Anulaciones_AutorizacionRefresh_Obtener(int CodEmpresa, int GestionId)
        {
            return DbFndAnulaciones.FND_Anulaciones_AutorizacionRefresh_Obtener(CodEmpresa, GestionId);
        }

        public ErrorDto<object> FND_Anulaciones_Anular(int CodEmpresa, string Params, string Accion, string Notas)
        {
            FndAnulacionesParams Parametros = JsonConvert.DeserializeObject<FndAnulacionesParams>(Params) ?? new FndAnulacionesParams();
            return DbFndAnulaciones.FND_Anulaciones_Anular(CodEmpresa, Parametros, Accion, Notas);
        }
    }
}