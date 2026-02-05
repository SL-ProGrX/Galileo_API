using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasCajaChicaBL
    {
        private readonly FrmCajasCajaChicaDB _db;
        public FrmCajasCajaChicaBL(IConfiguration config)
        {
            _db = new FrmCajasCajaChicaDB(config);
        }

        public ErrorDto<List<CajasCajaChicaServiciosDto>> Cajas_CajaChicaServicios_Buscar(
               int codEmpresa,
               string codCaja,
               string servicioBusqueda)
        {
            return _db.Cajas_CajaChicaServicios_Buscar(
                codEmpresa,
                codCaja,
                servicioBusqueda);
        }


        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_CajaChicaDocumentos_Obtener(
                int codEmpresa,
                string codCaja)
        {
            return _db.Cajas_CajaChicaDocumentos_Obtener(
                codEmpresa,
                codCaja);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas__CajaChicaDivisas_Obtener(
            int codEmpresa,
            int codContabilidad)
        {
            return _db.Cajas__CajaChicaDivisas_Obtener(
                codEmpresa,
                codContabilidad);
        }

        public ErrorDto<CajasCajaChicaTipoCambioRsDto> Cntx_Divisa_TipoCambio_Obtener(
                int codEmpresa,
                int codContabilidad,
                string codDivisa)
        {
            return _db.Cntx_Divisa_TipoCambio_Obtener(
                codEmpresa,
                codContabilidad,
                codDivisa);
        }

        public ErrorDto<List<CajasCajaChicaSociosBusquedaRsDto>> Socios_Buscar(
               int codEmpresa,
               string? filtroNombre)
        {
            return _db.Socios_Buscar(
                codEmpresa,
                filtroNombre);
        }

        public ErrorDto<CajasCajaChicaAplicarDbResponseDto> Cajas_Retiro_Aplicar_Db(
                 CajasCajaChicaAplicarDbRequestDto req)
        {
            return _db.Cajas_Retiro_Aplicar_Db(
                req);
        }
    }
}
