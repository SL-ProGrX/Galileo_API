using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Newtonsoft.Json;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesGeneraAsientosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb DBBitacora;

        public FrmTesGeneraAsientosDB(IConfiguration config)
        {
            DBBitacora = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Obtiene la cuenta de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaBancosGA>> Tes_Bancos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = "SELECT id_banco, descripcion FROM Tes_Bancos WHERE estado = 'A'";

                return conn.Query<DropDownListaBancosGA>(query).ToList();
            });
        }

        /// <summary>
        /// Obtiene los tipos de documentos de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_Banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaTiposGA>> Tes_Tipos_Obtener(int CodEmpresa, string cod_Banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"SELECT 
                                RTRIM(T.Tipo) + ' - ' + RTRIM(T.Descripcion) AS ItmY,
                                T.Tipo AS IdX,
                                RTRIM(T.Descripcion) AS ItmX
                            FROM 
                                tes_banco_docs A
                            INNER JOIN 
                                Tes_Tipos_Doc T ON A.tipo = T.tipo
                            WHERE 
                                A.ID_BANCO = @CodBanco
                                AND (
                                    (@CodBanco IN (1, 3) AND A.REG_EMISION = 1) OR
                                    (@CodBanco NOT IN (1, 3) AND A.REG_EMISION = 0))
                                        ORDER BY t.Tipo asc";

                return conn.Query<DropDownListaTiposGA>(query, new { CodBanco = cod_Banco }).ToList();
            });
        }



        /// <summary>
        /// Obtener informaci�n de las transacciones con asiento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosTransacciones"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_transaccionesAsientos_Obtener(
     int CodEmpresa,
     string filtrosTransacciones,
     FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var filtro = JsonConvert.DeserializeObject<TesTransaccionesFiltros>(filtrosTransacciones)
                         ?? new TesTransaccionesFiltros();

            filtros ??= new FiltrosLazyLoadData();

            try
            {
                // Texto libre (parametrizado)
                var texto = filtros.filtro?.Trim();
                var hasTexto = !string.IsNullOrWhiteSpace(texto);
                var like = hasTexto ? $"%{texto}%" : null;

                // Fechas (mejor enviar DateTime, no strings)
                var fechaInicio = filtro.fecha_desde.Date;
                var fechaFin = filtro.fecha_hasta.Date.AddDays(1).AddTicks(-1); // 23:59:59.9999999

                // Paginación segura
                var usarPaginacion = filtros.pagina >= 0; // Use value check instead of null check
                var offset = (filtros.pagina < 0) ? 0 : filtros.pagina;
                var fetch = (filtros.paginacion <= 0) ? 50 : filtros.paginacion;

                // Parámetros base
                var parameters = new
                {
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin,

                    // En vez de concatenar "IN (...)" según tipo_mov, controlamos con OR
                    TipoMov = (filtro.tipo_mov ?? "1").Trim(), // "1" => T/I ; otro => A

                    TodasCuentas = filtro.chk_todasCuentas ? 1 : 0,
                    Banco = filtro.cod_banco,

                    TodosDocumentos = filtro.chk_todosDocumentos ? 1 : 0,
                    Tipo = filtro.tipo_doc,

                    FiltroTexto = hasTexto ? texto : null,
                    Like = like,

                    UsarPaginacion = usarPaginacion ? 1 : 0,
                    Offset = offset,
                    Fetch = fetch
                };

                const string whereSql = @"
WHERE
    T.Estado_Asiento = 'P'
    AND (
        (@TipoMov = '1' AND T.Estado IN ('T','I'))
        OR (@TipoMov <> '1' AND T.Estado IN ('A'))
    )
    AND T.Fecha_Emision BETWEEN @FechaInicio AND @FechaFin
    AND (
        @TodasCuentas = 1 OR T.id_Banco = @Banco
    )
    AND (
        @TodosDocumentos = 1 OR T.Tipo = @Tipo
    )
    AND (
        @FiltroTexto IS NULL
        OR CAST(T.nsolicitud AS NVARCHAR(50)) LIKE @Like
        OR CAST(T.ndocumento AS NVARCHAR(50)) LIKE @Like
        OR T.beneficiario LIKE @Like
    )";

                const string sqlCount = @"
SELECT COUNT(T.nsolicitud)
FROM Tes_Transacciones T
INNER JOIN Tes_Bancos B ON T.id_Banco = B.id_Banco
" + whereSql + @";";

                const string sqlList = @"
SELECT
    T.nsolicitud,
    T.ndocumento,
    T.monto,
    T.fecha_emision,
    T.beneficiario,
    T.tipo,
    B.descripcion AS bancodesc,
    SUM(T.monto) OVER () AS monto_total
FROM Tes_Transacciones T
INNER JOIN Tes_Bancos B ON T.id_Banco = B.id_Banco
" + whereSql + @"
ORDER BY T.nsolicitud DESC
OFFSET (CASE WHEN @UsarPaginacion = 1 THEN @Offset ELSE 0 END) ROWS
FETCH NEXT (CASE WHEN @UsarPaginacion = 1 THEN @Fetch ELSE 2147483647 END) ROWS ONLY;";

                var result = new TablasListaGenericaModel
                {
                    total = conn.QuerySingle<int>(sqlCount, parameters),
                    lista = conn.Query<TesTrasladoTransaccionDto>(sqlList, parameters).ToList()
                };

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(ex.Message);
            }
        }


        /// <summary>
        /// Genera traslado de asientos a Contabilidad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="trasladoLista"></param>
        /// <returns></returns>
        public ErrorDto TES_Traslado_Generar(int CodEmpresa,string trasladoLista)
        {
            List<int> lista = JsonConvert.DeserializeObject<List<int>>(trasladoLista) ?? new List<int>();
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                foreach (var solicitud in lista)
                {
                    var querySP = "exec spTES_Asientos_Traslado_Individual @nsolicitud";
                    conn.Execute(querySP, new { nsolicitud = solicitud });
                }
                return DbHelper.OkResponse("Traslado procesado correctamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al procesar el traslado: {ex.Message}");
            }
        }

    }
}