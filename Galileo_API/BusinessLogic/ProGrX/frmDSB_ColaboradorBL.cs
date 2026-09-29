using Galileo.DataBaseTier.ProGrX;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX;

namespace Galileo.BusinessLogic.ProGrX
{
    public class FrmDsbColaboradorBL
    {
        private readonly FrmDsbColaboradorDB _db;

        public FrmDsbColaboradorBL(IConfiguration config)
        {
            _db = new FrmDsbColaboradorDB(config);
        }

        public ErrorDto<ColaboradorVinculoData> Colaborador_Vinculado_Obtener(
            int CodEmpresa,
            string usuario)
        {
            return _db.Colaborador_Vinculado_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<ColaboradorPerfilData> Colaborador_Perfil_Obtener(
            int CodEmpresa,
            string usuario,
            string AppVersion)
        {
            return _db.Colaborador_Perfil_Obtener(
                CodEmpresa,
                usuario,
                AppVersion);
        }

        public ErrorDto<List<ColaboradorEmpleadoOpcionData>> Colaborador_Consulta_Id(
            int CodEmpresa,
            string Identificacion,
            string? EmpleadoId)
        {
            return _db.Colaborador_Consulta_Id(
                CodEmpresa,
                Identificacion,
                EmpleadoId);
        }

        public ErrorDto<ColaboradorAccesoData> Colaborador_Acceso(
            int CodEmpresa,
            string usuario,
            ColaboradorAccesoRequest request)
        {
            return _db.Colaborador_Acceso(CodEmpresa, usuario, request);
        }

        public ErrorDto<ColaboradorClaveReestableceData> Colaborador_Clave_Reestablece(
            int CodEmpresa,
            string usuario,
            ColaboradorClaveReestableceRequest request)
        {
            return _db.Colaborador_Clave_Reestablece(
                CodEmpresa,
                usuario,
                request);
        }

        public ErrorDto<List<Dictionary<string, object?>>> Colaborador_Menu_Obtener(
            int CodEmpresa,
            string usuario,
            ColaboradorMenuRequest request)
        {
            return _db.Colaborador_Menu_Obtener(
                CodEmpresa,
                usuario,
                request);
        }
    }
}
