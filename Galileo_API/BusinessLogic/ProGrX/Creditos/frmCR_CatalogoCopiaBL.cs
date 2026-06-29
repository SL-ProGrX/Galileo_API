using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCatalogoCopiaBL
    {
        private readonly FrmCrCatalogoCopiaDB _db;

        public FrmCrCatalogoCopiaBL(IConfiguration config)
        {
            _db = new FrmCrCatalogoCopiaDB(config);
        }

        public ErrorDto<List<CrCatalogoCopiaLineaDto>> CR_CatalogoCopia_Lineas_Obtener(int CodEmpresa)
        {
            return _db.CR_CatalogoCopia_Lineas_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CrCatalogoCopiaLineaDto>> CR_CatalogoCopia_Lineas_F4_Obtener(int CodEmpresa, string? texto)
        {
            return _db.CR_CatalogoCopia_Lineas_F4_Obtener(CodEmpresa, texto);
        }

        public ErrorDto<CrCatalogoCopiaDescripcionDto> CR_CatalogoCopia_Linea_Descripcion_Obtener(int CodEmpresa, string codigo)
        {
            return _db.CR_CatalogoCopia_Linea_Descripcion_Obtener(CodEmpresa, codigo);
        }

        public ErrorDto<CrCatalogoCopiaScrollDto> CR_CatalogoCopia_Linea_Scroll_Obtener(int CodEmpresa, int scroll, string? codigo)
        {
            return _db.CR_CatalogoCopia_Linea_Scroll_Obtener(CodEmpresa, scroll, codigo);
        }

        public ErrorDto<CrCatalogoCopiaResultadoDto> CR_CatalogoCopia_Copiar(int CodEmpresa, CrCatalogoCopiaRequest request)
        {
            return _db.CR_CatalogoCopia_Copiar(CodEmpresa, request);
        }
    }
}