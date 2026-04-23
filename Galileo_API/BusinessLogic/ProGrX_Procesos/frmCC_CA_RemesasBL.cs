using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Procesos;
using Galileo_API.Models.ProGrX_Procesos;

namespace Galileo_API.BusinessLogic.ProGrX_Procesos
{
    public class FrmCcCaRemesasBL
    {
        private readonly FrmCcCaRemesasDB _db;

        public FrmCcCaRemesasBL(IConfiguration config)
        {
            _db = new FrmCcCaRemesasDB(config);
        }

        public ErrorDto<CcCaRemesasCatalogosResponse> CcCaRemesas_Catalogos_Obtener(int codEmpresa)
        {
            return _db.CcCaRemesas_Catalogos_Obtener(codEmpresa);
        }

        public ErrorDto<List<CcCaRemesasEnvioConsultaData>> CcCaRemesas_Envio_Consulta(
            int codEmpresa,
            CcCaRemesasEnvioConsultaRequest request)
        {
            if (request == null)
                return DbHelper.CreateErrorResponse<List<CcCaRemesasEnvioConsultaData>>("El request es requerido.");

            if (request.cod_remesa <= 0)
                return DbHelper.CreateErrorResponse<List<CcCaRemesasEnvioConsultaData>>("El tipo de remesa es requerido.");

            if (string.IsNullOrWhiteSpace(request.cod_entidad))
                return DbHelper.CreateErrorResponse<List<CcCaRemesasEnvioConsultaData>>("La entidad es requerida.");

            if (request.cuotas <= 0)
                return DbHelper.CreateErrorResponse<List<CcCaRemesasEnvioConsultaData>>("La cantidad de cuotas debe ser mayor a cero.");

            return _db.CcCaRemesas_Envio_Consulta(codEmpresa, request);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CcCaRemesas_Recibe_Pendientes_Obtener(int codEmpresa)
        {
            return _db.CcCaRemesas_Recibe_Pendientes_Obtener(codEmpresa);
        }

        public ErrorDto<List<CcCaRemesasRecibeDetalleData>> CcCaRemesas_Recibe_Detalle_Obtener(int codEmpresa, long remesa)
        {
            if (remesa <= 0)
                return DbHelper.CreateErrorResponse<List<CcCaRemesasRecibeDetalleData>>("La remesa es requerida.");

            return _db.CcCaRemesas_Recibe_Detalle_Obtener(codEmpresa, remesa);
        }
    }
}
