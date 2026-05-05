using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cobros;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOCarteraBL
    {
        private readonly FrmCOCarteraDB _db;

        public FrmCOCarteraBL(IConfiguration config)
        {
            _db = new FrmCOCarteraDB(config);
        }

        public ErrorDto<COCarteraListaResult> Co_CarteraLista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            if (filtros == null)
            {
                filtros = new FiltrosLazyLoadData();
            }
            return _db.Co_CarteraLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<COCarteraListaResult> Co_Cartera_Export(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);

            if (filtros == null)
            {
                filtros = new FiltrosLazyLoadData();
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return _db.Co_CarteraLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto Co_Cartera_Guardar(int CodEmpresa, string usuario, COCarteraClasificacionData cartera)
        {
            return _db.Co_Cartera_Guardar(CodEmpresa, usuario, cartera);
        }

        public ErrorDto Co_Cartera_Eliminar(int CodEmpresa, string usuario, string cod_clasificacion)
        {
            return _db.Co_Cartera_Eliminar(CodEmpresa, usuario, cod_clasificacion);
        }

        public ErrorDto<List<COCarteraCatalogoData>> Co_Catalogo_Obtener(int CodEmpresa)
        {
            return _db.Co_Catalogo_Obtener(CodEmpresa);
        }

        public ErrorDto<List<COCarteraAsignacionCatItemData>> Co_Asignacion_Carteras_PorCodigo_Obtener(int CodEmpresa, string codigo)
        {
            return _db.Co_Asignacion_Carteras_PorCodigo_Obtener(CodEmpresa, codigo);
        }

        public ErrorDto<List<COCarteraAsignacionCodigoItemData>> Co_Asignacion_Codigos_PorCartera_Obtener(int CodEmpresa, string cod_clasificacion)
        {
            return _db.Co_Asignacion_Codigos_PorCartera_Obtener(CodEmpresa, cod_clasificacion);
        }

        public ErrorDto Co_Asignacion_Guardar(int CodEmpresa, string usuario, COCarteraAsignacionGuardarDto dto)
        {
            return _db.Co_Asignacion_Guardar(CodEmpresa, usuario, dto);
        }

        public ErrorDto Co_Asignacion_Bulk_Guardar(int CodEmpresa, string usuario, COCarteraAsignacionBulkDto dto)
        {
            return _db.Co_Asignacion_Bulk_Guardar(CodEmpresa, usuario, dto);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Co_Carteras_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.Co_Carteras_Dropdown_Obtener(CodEmpresa);
        }
    }
}