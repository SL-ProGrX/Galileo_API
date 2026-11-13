using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreAnalisisBL
    {
        private readonly FrmPreAnalisisDB _db;

        public FrmPreAnalisisBL(IConfiguration config)
        {
            _db = new FrmPreAnalisisDB(config);
        }

        public ErrorDto<FrmPreAnalisisCargarResponse> Prea_frmPreAnalisis_Cargar(int codEmpresa, long operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operación.",
                    -1,
                    new FrmPreAnalisisCargarResponse());
            }

            return _db.Prea_frmPreAnalisis_Cargar(codEmpresa, operacion);
        }

        public ErrorDto<FrmPreAnalisisCalcularResponse> Prea_frmPreAnalisis_Calcular(
            int codEmpresa,
            FrmPreAnalisisCalcularRequest request)
        {
            if (request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la operación.",
                    -1,
                    new FrmPreAnalisisCalcularResponse());
            }

            return _db.Prea_frmPreAnalisis_Calcular(codEmpresa, request);
        }

        public ErrorDto<FrmPreAnalisisDeudorGuardarResponse> Prea_frmPreAnalisis_Deudor_Guardar(
            int codEmpresa,
            FrmPreAnalisisDeudorGuardarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -1,
                    new FrmPreAnalisisDeudorGuardarResponse());
            }

            return _db.Prea_frmPreAnalisis_Deudor_Guardar(codEmpresa, request);
        }

        public ErrorDto Prea_frmPreAnalisis_Observaciones_Guardar(
            int codEmpresa,
            FrmPreAnalisisObservacionesGuardarRequest request)
        {
            if (request.operacion <= 0)
            {
                return DbHelper.ErrorResponse("Faltan Datos");
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse("Debe indicar el usuario.");
            }

            return _db.Prea_frmPreAnalisis_Observaciones_Guardar(codEmpresa, request);
        }

        public ErrorDto<TablasListaGenericaModel> Prea_frmPreAnalisis_Operaciones_Lista(int codEmpresa, string filtros)
        {
            return _db.Prea_frmPreAnalisis_Operaciones_Lista(codEmpresa, filtros);
        }

        public ErrorDto<List<FrmPreAnalisisComiteData>> Prea_frmPreAnalisis_Comites_Lista(int codEmpresa)
        {
            return _db.Prea_frmPreAnalisis_Comites_Lista(codEmpresa);
        }

        public ErrorDto<List<long>> Prea_frmPreAnalisis_Reporte_Solicitudes(int codEmpresa, int idComite, int acta)
        {
            return _db.Prea_frmPreAnalisis_Reporte_Solicitudes(codEmpresa, idComite, acta);
        }

        public ErrorDto<Dictionary<string, string>> Prea_frmPreAnalisis_Reporte_Parametros(
            int codEmpresa,
            long operacion,
            string tipo)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "# Operación Incorrecto",
                    -1,
                    new Dictionary<string, string>());
            }

            return _db.Prea_frmPreAnalisis_Reporte_Parametros(codEmpresa, operacion, tipo);
        }
    }
}
