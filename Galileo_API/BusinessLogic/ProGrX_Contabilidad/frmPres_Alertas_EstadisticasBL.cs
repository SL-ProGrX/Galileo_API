

using Galileo.DataBaseTier.ProGrX_Contabilidad;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using PgxAPI.Models.ProGrX_Contabilidad;

namespace Galileo.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmPresAlertasEstadisticasBL
    {
        private readonly FrmPresAlertasEstadisticasDB _DB;
        public FrmPresAlertasEstadisticasBL(IConfiguration config)
        {
            _DB = new FrmPresAlertasEstadisticasDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> PresAlertasEstadisticasTipos_Obtener(int CodEmpresa)
        {
            return _DB.PresAlertasEstadisticasTipos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<PresVistaPresupuestoAlertasData>> PresPlanning_Obtener(int CodCliente, string datos)
        {
            return _DB.PresPlanning_Obtener(CodCliente, datos);
        }

        public ErrorDto PresAlertaJustificacion_Guardar(int codEmpresa, PresAlertaJustificacionGuardarRequest data)
        {
            return _DB.PresAlertaJustificacion_Guardar(codEmpresa, data);
        }

        public ErrorDto<List<PresAlertaJustificacionBitacoraData>> PresAlertaJustificacionBitacora_Obtener(
            int codEmpresa,
            int codConta,
            string codModelo,
            string codUnidad,
            string codCentroCosto,
            string codCuenta,
            int anio,
            int mes,
            string tipoAlerta)
        {
            return _DB.PresAlertaJustificacionBitacora_Obtener(
                codEmpresa, codConta, codModelo, codUnidad, codCentroCosto, codCuenta, anio, mes, tipoAlerta
            );
        }
    }
}
