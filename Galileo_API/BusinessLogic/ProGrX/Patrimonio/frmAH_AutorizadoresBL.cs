using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAhAutorizadoresBL
    {
        private readonly FrmAhAutorizadoresDB _db;

        public FrmAhAutorizadoresBL(IConfiguration config)
        {
            _db = new FrmAhAutorizadoresDB(config);
        }

        public ErrorDto<AutorizadorePatrimonioDto> Patrimonio_frmAH_Autorizadores_Obtener(
            int codEmpresa,
            string usuario)
        {
            return _db.Patrimonio_frmAH_Autorizadores_Obtener(codEmpresa, usuario);
        }

        public ErrorDto<string> Patrimonio_frmAH_Autorizadores_ConsultaAscDesc(
            int codEmpresa,
            string usuario,
            string tipo)
        {
            return _db.Patrimonio_frmAH_Autorizadores_ConsultaAscDesc(codEmpresa, usuario, tipo);
        }

        public ErrorDto<List<AutorizadorePatrimonioDto>> Patrimonio_frmAH_Autorizadores_Lista(
            int codEmpresa,
            string? filtro)
        {
            return _db.Patrimonio_frmAH_Autorizadores_Lista(codEmpresa, filtro);
        }

        public ErrorDto<FrmAhAutorizadoresGuardarResponse> Patrimonio_frmAH_Autorizadores_Insertar(
            int codEmpresa,
            FrmAhAutorizadoresGuardarRequest request)
        {
            return _db.Patrimonio_frmAH_Autorizadores_Insertar(codEmpresa, request);
        }

        public ErrorDto<FrmAhAutorizadoresGuardarResponse> Patrimonio_frmAH_Autorizadores_Actualizar(
            int codEmpresa,
            FrmAhAutorizadoresGuardarRequest request)
        {
            return _db.Patrimonio_frmAH_Autorizadores_Actualizar(codEmpresa, request);
        }

        public ErrorDto<bool> Patrimonio_frmAH_Autorizadores_Eliminar(
            int codEmpresa,
            string usuario,
            string registroUsuario)
        {
            return _db.Patrimonio_frmAH_Autorizadores_Eliminar(codEmpresa, usuario, registroUsuario);
        }
    }
}
