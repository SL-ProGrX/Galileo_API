namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    using Galileo.Models.ERROR;
    using Galileo_API.DataBaseTier.ProGrX.Creditos;
    using Galileo_API.Models.ProGrX.Creditos;

    public class FrmCrBitacoraEspecialBL
    {
        private readonly FrmCrBitacoraEspecialDB _db;

        public FrmCrBitacoraEspecialBL(IConfiguration config)
        {
            _db = new FrmCrBitacoraEspecialDB(config);
        }

        public ErrorDto<List<CrBitacoraEspecialSocioModel>> CrBitacoraEspecial_Socios_Obtener(int CodEmpresa)
        {
            return _db.CrBitacoraEspecial_Socios_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CrBitacoraEspecialUsuarioModel>> CrBitacoraEspecial_Usuarios_Obtener(int CodEmpresa)
        {
            return _db.CrBitacoraEspecial_Usuarios_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CrBitacoraEspecialMovimientoModel>> CrBitacoraEspecial_Movimientos_Obtener(int CodEmpresa)
        {
            return _db.CrBitacoraEspecial_Movimientos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CrBitacoraEspecialRegistroModel>> CrBitacoraEspecial_Registros_Obtener(int CodEmpresa, CrBitacoraEspecialRegistrosObtenerRequest request)
        {
            return _db.CrBitacoraEspecial_Registros_Obtener(CodEmpresa, request);
        }

        public ErrorDto CrBitacoraEspecial_Asignar(int CodEmpresa, CrBitacoraEspecialAsignarRequest request)
        {
            return _db.CrBitacoraEspecial_Asignar(CodEmpresa, request);
        }
    }
}
