using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Nucleo;
using Galileo_API.Models.ProGrX_Nucleo;

namespace Galileo_API.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysAplIntegradorBL
    {
        /// <summary>Estados válidos de los combos (primer carácter del texto en V6).</summary>
        private const string EstadosValidos = "RPADF";

        private readonly FrmSysAplIntegradorDB _DB;

        public FrmSysAplIntegradorBL(IConfiguration config)
        {
            _DB = new FrmSysAplIntegradorDB(config);
        }

        /// <summary>
        /// Valida que el estado sea de un solo carácter permitido y que exista usuario.
        /// </summary>
        private static string? ValidarLista(AplListaRequest request)
        {
            if (request.estado.Length != 1 || !EstadosValidos.Contains(request.estado))
            {
                return "El estado indicado no es válido.";
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return "El usuario es requerido.";
            }

            return null;
        }

        public ErrorDto<List<AplLineaDto>> SYS_APL_Integrador_Lineas_Lista(int codEmpresa)
        {
            return _DB.SYS_APL_Integrador_Lineas_Lista(codEmpresa);
        }

        public ErrorDto<List<AplTramiteDto>> SYS_APL_Integrador_Tramite_Lista(int codEmpresa, AplListaRequest request)
        {
            var error = ValidarLista(request);
            return error is null
                ? _DB.SYS_APL_Integrador_Tramite_Lista(codEmpresa, request)
                : DbHelper.CreateErrorResponse<List<AplTramiteDto>>(error);
        }

        public ErrorDto<List<AplSolicitudDto>> SYS_APL_Integrador_Solicitud_Lista(int codEmpresa, AplListaRequest request)
        {
            var error = ValidarLista(request);
            return error is null
                ? _DB.SYS_APL_Integrador_Solicitud_Lista(codEmpresa, request)
                : DbHelper.CreateErrorResponse<List<AplSolicitudDto>>(error);
        }

        public ErrorDto SYS_APL_Integrador_Tramite_Atiende(int codEmpresa, long apl_operacion, string usuario)
        {
            return string.IsNullOrWhiteSpace(usuario)
                ? DbHelper.ErrorResponse("El usuario es requerido.")
                : _DB.SYS_APL_Integrador_Tramite_Atiende(codEmpresa, apl_operacion, usuario);
        }

        public ErrorDto SYS_APL_Integrador_Solicitud_Atiende(int codEmpresa, long num_solicitud, string usuario)
        {
            return string.IsNullOrWhiteSpace(usuario)
                ? DbHelper.ErrorResponse("El usuario es requerido.")
                : _DB.SYS_APL_Integrador_Solicitud_Atiende(codEmpresa, num_solicitud, usuario);
        }

        public ErrorDto<AplMonitorDto?> SYS_APL_Integrador_Monitor(int codEmpresa)
        {
            return _DB.SYS_APL_Integrador_Monitor(codEmpresa);
        }
    }
}
