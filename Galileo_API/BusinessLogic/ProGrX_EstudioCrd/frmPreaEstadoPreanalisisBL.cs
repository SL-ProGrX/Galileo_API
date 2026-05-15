using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaEstadoPreanalisisBL
    {
        private readonly FrmPreaEstadoPreanalisisDB _db;

        public FrmPreaEstadoPreanalisisBL(IConfiguration config)
        {
            _db = new FrmPreaEstadoPreanalisisDB(config);
        }

        public ErrorDto<FrmPreaEstadoPreanalisisCargarResponse> Prea_frmPreaEstadoPreanalisis_Cargar(
            int codEmpresa,
            string usuario,
            string cod_preanalisis,
            string tipo)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -1,
                    new FrmPreaEstadoPreanalisisCargarResponse());
            }

            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el expediente.",
                    -1,
                    new FrmPreaEstadoPreanalisisCargarResponse());
            }

            if (!string.IsNullOrWhiteSpace(tipo) && !EsEstadoValido(tipo))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar un tipo valido.",
                    -1,
                    new FrmPreaEstadoPreanalisisCargarResponse());
            }

            return _db.Prea_frmPreaEstadoPreanalisis_Cargar(codEmpresa, usuario, cod_preanalisis, tipo);
        }

        public ErrorDto<FrmPreaEstadoPreanalisisGuardarResponse> Prea_frmPreaEstadoPreanalisis_Guardar(
            int codEmpresa,
            FrmPreaEstadoPreanalisisGuardarRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la informacion del estado.",
                    -1,
                    new FrmPreaEstadoPreanalisisGuardarResponse());
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -1,
                    new FrmPreaEstadoPreanalisisGuardarResponse());
            }

            if (string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el expediente.",
                    -1,
                    new FrmPreaEstadoPreanalisisGuardarResponse());
            }

            if (!EsEstadoValido(request.estado))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar un estado valido.",
                    -1,
                    new FrmPreaEstadoPreanalisisGuardarResponse());
            }

            return _db.Prea_frmPreaEstadoPreanalisis_Guardar(codEmpresa, request);
        }

        public ErrorDto<FrmPreaEstadoPreanalisisCausaRegistrarResponse> Prea_frmPreaEstadoPreanalisis_Causa_Registrar(
            int codEmpresa,
            FrmPreaEstadoPreanalisisCausaRegistrarRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la causa.",
                    -1,
                    new FrmPreaEstadoPreanalisisCausaRegistrarResponse());
            }

            if (string.IsNullOrWhiteSpace(request.usuario)
                || string.IsNullOrWhiteSpace(request.cod_preanalisis)
                || string.IsNullOrWhiteSpace(request.tipo)
                || string.IsNullOrWhiteSpace(request.codigo)
                || string.IsNullOrWhiteSpace(request.cod_causas))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la informacion completa de la causa.",
                    -1,
                    new FrmPreaEstadoPreanalisisCausaRegistrarResponse());
            }

            if (request.tipo.Trim().ToUpperInvariant() is not ("P" or "D"))
            {
                return DbHelper.CreateErrorResponse(
                    "Las causas solo aplican para estados Pendiente o Denegado.",
                    -1,
                    new FrmPreaEstadoPreanalisisCausaRegistrarResponse());
            }

            return _db.Prea_frmPreaEstadoPreanalisis_Causa_Registrar(codEmpresa, request);
        }

        private static bool EsEstadoValido(string estado)
            => estado.Trim().ToUpperInvariant() is "R" or "P" or "A" or "D";
    }
}
