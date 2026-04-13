using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using System.Data;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOControlListaDB
    {
        private readonly PortalDB _portalDB;

        public FrmCOControlListaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Consulta el listado principal del control de carteras de cobro.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Filtros de búsqueda del formulario.</param>
        /// <returns>Totales y lista principal.</returns>
        public ErrorDto<CoControlListaBuscarResponse> CoControlLista_Buscar(
            int codEmpresa,
            CoControlListaBuscarRequest request)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                var p = new DynamicParameters();
                p.Add("@usuario", request.usuario);
                p.Add("@todos_usuarios", request.todos_usuarios);
                p.Add("@fecha_inicio", request.fecha_inicio);
                p.Add("@fecha_corte", request.fecha_corte);
                p.Add("@todas_fechas", request.todas_fechas);
                p.Add("@casos_sin_asignar", request.casos_sin_asignar);
                p.Add("@cedula", request.cedula);
                p.Add("@nombre", request.nombre);
                p.Add("@estado", request.estado);
                p.Add("@cuotas_desde", request.cuotas_desde);
                p.Add("@cuotas_hasta", request.cuotas_hasta);
                p.Add("@cartera", request.cartera);
                p.Add("@oficina", request.oficina);
                p.Add("@institucion", request.institucion);
                p.Add("@tipo_casos", request.tipo_casos);
                p.Add("@dias_atencion", request.dias_atencion);
                p.Add("@gestion", request.gestion);
                p.Add("@causa", request.causa);
                p.Add("@arreglo", request.arreglo);
                p.Add("@todas_fechas_pago", request.todas_fechas_pago);
                p.Add("@fecha_pago_inicio", request.fecha_pago_inicio);
                p.Add("@fecha_pago_corte", request.fecha_pago_corte);
                p.Add("@incluir_info_contacto", request.incluir_info_contacto);
                p.Add("@lista_garantias", request.lista_garantias);
                p.Add("@lista_antiguedades", request.lista_antiguedades);
                p.Add("@orden", request.orden);
                p.Add("@orden_tipo", request.orden_tipo);
                p.Add("@filtro", request.filtro);
                p.Add("@pagina", request.pagina);
                p.Add("@paginacion", request.paginacion);
                p.Add("@sortOrder", request.sortOrder);
                p.Add("@sortField", request.sortField);

                using var multi = conn.QueryMultiple(
                    "spCBR_W_ControlLista_Buscar",
                    p,
                    commandType: CommandType.StoredProcedure);

                var totales = multi.ReadFirstOrDefault<CoControlListaTotales>() ?? new CoControlListaTotales();
                var lista = multi.Read<CoControlListaGridRow>().ToList();

                return DbHelper.CreateOkResponse(new CoControlListaBuscarResponse
                {
                    totales = totales,
                    lista = lista
                });
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlListaBuscarResponse>(ex.Message);
            }
        }
    }
}
