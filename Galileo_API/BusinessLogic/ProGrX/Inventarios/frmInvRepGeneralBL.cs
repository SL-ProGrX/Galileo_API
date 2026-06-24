using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvRepGeneralBL
    {
        private readonly FrmInvRepGeneralDB _db;

        public FrmInvRepGeneralBL(IConfiguration config)
        {
            _db = new FrmInvRepGeneralDB(config);
        }

        public ErrorDto<List<BodegaReporteInvDto>> Obtener_Bodegas(int CodEmpresa)
        {
            return _db.Obtener_Bodegas(CodEmpresa);
        }

        public ErrorDto<List<UnidadesReporteInvDto>> Obtener_Unidades(int CodEmpresa)
        {
            return _db.Obtener_Unidades(CodEmpresa);
        }

        public ErrorDto<List<DepartamentoReporteInvDto>> Obtener_Departamento(int CodEmpresa)
        {
            return _db.Obtener_Departamento(CodEmpresa);
        }

        public ErrorDto<List<ProveedoresInvDto>> Obtener_Proveedor(int CodEmpresa)
        {
            return _db.Obtener_Proveedor(CodEmpresa);
        }

        public ErrorDto<List<LineasInvDto>> Obtener_Lineas(int CodEmpresa)
        {
            return _db.Obtener_Lineas(CodEmpresa);
        }

        public ErrorDto<List<CprUensLista>> CprUens_Obtener(int CodEmpresa, string usuario)
        {
            return _db.CprUens_Obtener(CodEmpresa, usuario);
        }
    }
}