using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.DataBaseTier
{
    public class frmInvKardexDB
    {
        private readonly IConfiguration _config;

        public frmInvKardexDB(IConfiguration config)
        {
            _config = config;
        }


        public ErrorDTO<List<ConsultaMovimientoBodegaCDTO>> Obtener_Bodegas(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<List<ConsultaMovimientoBodegaCDTO>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "SELECT COD_BODEGA, DESCRIPCION FROM PV_BODEGAS";

                    response.Result = connection.Query<ConsultaMovimientoBodegaCDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }


        public ErrorDTO<MovimientosDTOList> consultarMovimientos_Obtener(int CodCliente, string filtroString)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            MovimientosInventarios_Filtros filtros = JsonConvert.DeserializeObject<MovimientosInventarios_Filtros>(filtroString) ?? new MovimientosInventarios_Filtros();

            var response = new ErrorDTO<MovimientosDTOList>
            {
                Code = 0,
                Result = new MovimientosDTOList(),
            };
            response.Result.Total = 0;

            try
            {
                string where = "", paginaActual = " ", paginacionActual = " ";

                // Convertir la cadena ISO a DateTimeOffset
                DateTimeOffset fecha_inicio = DateTimeOffset.Parse(filtros.fecha_inicio);
                string fechainicio = fecha_inicio.ToString("yyyy-MM-dd");

                DateTimeOffset fecha_corte = DateTimeOffset.Parse(filtros.fecha_corte);
                string fechacorte = fecha_corte.ToString("yyyy-MM-dd");

                where = $"where M.fecha BETWEEN '{fechainicio} 00:00:00' AND '{fechacorte} 23:59:59' ";

                if (filtros.Tipo != "Todos")
                {
                    if (filtros.Tipo == "E" || filtros.Tipo == "S")
                    {
                        where = where + $" AND M.Tipo = '{filtros.Tipo}' ";
                    }
                    else
                    {
                        where = where + $" AND M.Origen = '{filtros.Tipo}' ";
                    }
                }

                if (filtros.cod_Bodega != "Todos")
                {
                    where = where + $" AND m.COD_BODEGA = '{filtros.cod_Bodega}'";
                }

                if (filtros.cod_Producto != "Todos")
                {
                    where = where + $"AND M.cod_producto = '{filtros.cod_Producto}'";
                }

                if (filtros.vfiltro != null)
                {
                    where = where + "AND (M.cod_producto LIKE '%" + filtros.vfiltro + "%' OR P.descripcion LIKE '%" + filtros.vfiltro + "%' OR M.codigo LIKE '%" + filtros.vfiltro + "%' OR M.Fecha LIKE '%" + filtros.vfiltro + "%')";
                }

                if (filtros.pagina != null)
                {
                    paginaActual = "OFFSET " + filtros.pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT COUNT(M.cod_producto)
                                FROM pv_inventario_mov M
                                INNER JOIN pv_productos P ON M.cod_producto = P.cod_producto
                                INNER JOIN pv_Bodegas B ON M.cod_bodega = B.cod_bodega {where} ";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"SELECT
                                    M.Fecha,
                                    (RTRIM(M.cod_producto) + ' - ' + RTRIM(P.descripcion)) AS Producto,
                                    CASE M.tipo
                                        WHEN 'E' THEN 'ENTRADA'
                                        WHEN 'S' THEN 'SALIDA'
                                    END AS TipoX,
                                    M.origen,
                                    M.codigo,
                                    ISNULL(M.existencia, 0) AS Existencia,
                                    M.cantidad,
                                    CASE
                                        WHEN M.tipo = 'E' THEN ISNULL(M.existencia, 0) + M.Cantidad
                                        WHEN M.tipo = 'S' THEN ISNULL(M.existencia, 0) - M.Cantidad
                                    END AS ExistenciaX,
                                    M.precio,
                                    (M.cantidad * M.precio) AS TotalSinImp,
                                    (M.cantidad * M.precio) * (M.imp_ventas / 100) AS ImpVentas,
                                    (M.cantidad * M.precio) * (M.imp_consumo / 100) AS ImpConsumo,
                                    (M.cantidad * M.precio) + ((M.cantidad * M.precio) * (M.imp_ventas / 100)) + ((M.cantidad * M.precio) * (M.imp_consumo / 100)) AS TotalConImp,
                                    (RTRIM(M.cod_bodega) + ' - ' + RTRIM(B.descripcion)) AS Bodega,
                                    dbo.fxINVBodegaTraslado(M.Origen, M.Tipo, M.Linea) AS BodegaEnlace
                                FROM
                                    pv_inventario_mov M
                                    INNER JOIN pv_productos P ON M.cod_producto = P.cod_producto
                                    INNER JOIN pv_Bodegas B ON M.cod_bodega = B.cod_bodega 
                                    {where} ORDER BY M.Fecha desc
                                    {paginaActual} {paginacionActual}; ";

                    response.Result.Movimientos = connection.Query<MovimientosDTO>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Total = 0;
                response.Result.Movimientos = new List<MovimientosDTO>();
            }

            return response;

        }




    }
}