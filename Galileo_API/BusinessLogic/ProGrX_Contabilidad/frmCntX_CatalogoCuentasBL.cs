using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXCatalogoCuentasBL
    {
        private readonly FrmCntXCatalogoCuentasDB _db;

        public FrmCntXCatalogoCuentasBL(IConfiguration config)
        {
            _db = new FrmCntXCatalogoCuentasDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXCatalogoDivisas(int codEmpresa, int codContabilidad)
            => _db.CntXCatalogoDivisas(codEmpresa, codContabilidad);

        public ErrorDto<List<DropDownListaGenericaModel>> CntXCatalogoTiposCuenta(int codEmpresa, int codContabilidad)
            => _db.CntXCatalogoTiposCuenta(codEmpresa, codContabilidad);

        public ErrorDto<List<DropDownListaGenericaModel>> CntXCatalogoUnidades(int codEmpresa, int codContabilidad)
            => _db.CntXCatalogoUnidades(codEmpresa, codContabilidad);

        public ErrorDto<List<DropDownListaGenericaModel>> CntXCatalogoCentrosCosto(int codEmpresa, int codContabilidad)
            => _db.CntXCatalogoCentrosCosto(codEmpresa, codContabilidad);

        public ErrorDto<List<CntXCatalogoCuentaDto>> CntXCatalogoConsulta(int codEmpresa, CntXCatalogoCuentasFiltroRequest filtro)
            => _db.CntXCatalogoConsulta(codEmpresa, filtro);

        public ErrorDto<CntXCatalogoCuentaDetalleResponse> CntXCatalogoDetalle(int codEmpresa, int codContabilidad, string cuenta)
            => _db.CntXCatalogoDetalle(codEmpresa, codContabilidad, cuenta);

        public ErrorDto<bool> CntXCatalogoDetalleGuardar(int codEmpresa, CntXCatalogoCuentaDetalleGuardarRequest request)
            => _db.CntXCatalogoDetalleGuardar(codEmpresa, request);

        public ErrorDto<bool> CntXCatalogoCuentaEstadoGuardar(int codEmpresa, CntXCatalogoCuentaEstadoRequest request)
            => _db.CntXCatalogoCuentaEstadoGuardar(codEmpresa, request);

        public ErrorDto<CntXCatalogoCuentaGuardarResponse> CntXCatalogoCuentaGuardar(int codEmpresa, CntXCatalogoCuentaGuardarRequest request)
            => _db.CntXCatalogoCuentaGuardar(codEmpresa, request);

        public ErrorDto<bool> CntXCatalogoMapeo(int codEmpresa, CntXCatalogoMapeoRequest request)
            => _db.CntXCatalogoMapeo(codEmpresa, request);

        public ErrorDto<CntXCatalogoBajaNivelDto> CntXCatalogoBajaNivel(int codEmpresa, CntXCatalogoBajaNivelRequest request)
            => _db.CntXCatalogoBajaNivel(codEmpresa, request);

        public ErrorDto<bool> CntXCatalogoTraduccionGuardar(int codEmpresa, CntXCatalogoTraduccionGuardarRequest request)
            => _db.CntXCatalogoTraduccionGuardar(codEmpresa, request);

        public ErrorDto<bool> CntXCatalogoTraduccionEliminar(int codEmpresa, CntXCatalogoTraduccionGuardarRequest request)
            => _db.CntXCatalogoTraduccionEliminar(codEmpresa, request);

        public ErrorDto<bool> CntXCatalogoProrrataGuardar(int codEmpresa, CntXCatalogoProrrataGuardarRequest request)
            => _db.CntXCatalogoProrrataGuardar(codEmpresa, request);

        public ErrorDto<bool> CntXCatalogoProrrataEliminar(int codEmpresa, CntXCatalogoProrrataGuardarRequest request)
            => _db.CntXCatalogoProrrataEliminar(codEmpresa, request);
    }
}
