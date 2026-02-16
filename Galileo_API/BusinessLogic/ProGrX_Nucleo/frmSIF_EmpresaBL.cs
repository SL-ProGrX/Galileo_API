using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic
{
    public class FrmSifEmpresaBL(IConfiguration config)
    {
        private readonly FrmSifEmpresaDB _db = new FrmSifEmpresaDB(config);

        public ErrorDto<FrmSifEmpresaModel> Sif_Empresa_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            return _db.Sif_Empresa_Obtener(CodEmpresa, idEmpresa);
        }

        public ErrorDto Sif_Empresa_Guardar(int CodEmpresa, FrmSifEmpresaModel dto, string usuario)
        {
            return _db.Sif_Empresa_Guardar(CodEmpresa, dto, usuario);
        }

        public ErrorDto<byte[]> Sif_Empresa_Logo_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            return _db.Sif_Empresa_Logo_Obtener(CodEmpresa, idEmpresa);
        }

        public ErrorDto Sif_Empresa_Logo_Guardar(int CodEmpresa, int idEmpresa, byte[] contenido, string usuario)
        {
            return _db.Sif_Empresa_Logo_Guardar(CodEmpresa, idEmpresa, contenido, usuario);
        }

        public ErrorDto<byte[]> Sif_Empresa_Fondo_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            return _db.Sif_Empresa_Fondo_Obtener(CodEmpresa, idEmpresa);
        }

        public ErrorDto Sif_Empresa_Fondo_Guardar(int CodEmpresa, int idEmpresa, byte[] contenido, string usuario)
        {
            return _db.Sif_Empresa_Fondo_Guardar(CodEmpresa, idEmpresa, contenido, usuario);
        }

        public ErrorDto<List<ComboContabilidadDto>> Sif_Empresa_Contabilidades_Obtener(int CodEmpresa)
        {
            return _db.Sif_Empresa_Contabilidades_Obtener(CodEmpresa);
        }

        public ErrorDto<CuentaLookupDto> Sif_Empresa_CuentaPorCodigo_Obtener(int CodEmpresa, int codContabilidad, string codCuenta)
        {
            return _db.Sif_Empresa_CuentaPorCodigo_Obtener(CodEmpresa, codContabilidad, codCuenta);
        }

        public ErrorDto<List<CuentaLookupDto>> Sif_Empresa_Cuentas_Buscar(int CodEmpresa, int codContabilidad, string search)
        {
            return _db.Sif_Empresa_Cuentas_Buscar(CodEmpresa, codContabilidad, search);
        }

        public ErrorDto Sif_Empresa_BloqueoFecha_Aplicar(int CodEmpresa, DateTime fecha, char accion, string usuario)
        {
            return _db.Sif_Empresa_BloqueoFecha_Aplicar(CodEmpresa, fecha, accion, usuario);
        }

        public ErrorDto<DateTime?> Sif_Empresa_BloqueoFecha_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            return _db.Sif_Empresa_BloqueoFecha_Obtener(CodEmpresa, idEmpresa);
        }
    }
}