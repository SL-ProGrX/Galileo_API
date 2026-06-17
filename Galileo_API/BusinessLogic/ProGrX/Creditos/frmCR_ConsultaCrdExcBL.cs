using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrConsultaCrdExcBL
    {
        private readonly FrmCrConsultaCrdExcDB _db;

        public FrmCrConsultaCrdExcBL(IConfiguration config)
        {
            _db = new FrmCrConsultaCrdExcDB(config);
        }

        public ErrorDto<CrConsultaCrdExcInicialDto> CR_ConsultaCrdExc_Inicial_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            return _db.CR_ConsultaCrdExc_Inicial_Obtener(CodEmpresa, cedula, usuario);
        }

        public ErrorDto<List<CrConsultaCrdExcCuentaBancoDto>> CR_ConsultaCrdExc_CuentasBanco_Obtener(int CodEmpresa, string cedula, int banco)
        {
            return _db.CR_ConsultaCrdExc_CuentasBanco_Obtener(CodEmpresa, cedula, banco);
        }

        public ErrorDto<CrConsultaCrdExcDisponibleRecursoDto> CR_ConsultaCrdExc_DisponibleRecurso_Obtener(int CodEmpresa, string recurso)
        {
            return _db.CR_ConsultaCrdExc_DisponibleRecurso_Obtener(CodEmpresa, recurso);
        }

        public ErrorDto<CrConsultaCrdExcFormalizarDto> CR_ConsultaCrdExc_Formalizar(int CodEmpresa, CrConsultaCrdExcFormalizarRequest request)
        {
            return _db.CR_ConsultaCrdExc_Formalizar(CodEmpresa, request);
        }
        public ErrorDto<CrConsultaCrdExcOficinaUsuarioDto> CR_ConsultaCrdExc_OficinaUsuario_Obtener(int CodEmpresa, string usuario)
        {
            return _db.CR_ConsultaCrdExc_OficinaUsuario_Obtener(CodEmpresa, usuario);
        }
    }
}