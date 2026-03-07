using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosRenumeracionDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;

        private const string ColNumPlaca      = "num_placa";
        private const string ColPlacaAlterna  = "Placa_Alterna";
        private const string ColNombre        = "Nombre";
        private const string MensajeOk        = "Ok";
        private const string MensajeRenumeracionConRelacion = "No se puede renumerar el activo porque el activo tiene registros relacionados.";
        private const string MensajeNuevoNumeroExiste = "El nuevo número de placa ya existe. Verifique la información e intente nuevamente.";

        // Lista blanca para ORDER BY
        private static readonly Dictionary<string, string> SortFieldMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { ColNumPlaca,     ColNumPlaca },
                { ColPlacaAlterna, ColPlacaAlterna },
                { ColNombre,       ColNombre }
            };

        public FrmActivosRenumeracionDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método para consultar el listado de placas
        /// </summary>
        public ErrorDto<ActivosDataLista> Activos_Buscar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var response = new ErrorDto<ActivosDataLista>
            {
                Code = 0,
                Description = MensajeOk,
                Result = new ActivosDataLista
                {
                    total = 0,
                    lista = new List<ActivosData>()
                }
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();

                // Filtro texto parametrizado (sin concatenar WHERE)
                string? filtroTexto = filtros?.filtro;
                bool tieneFiltro = !string.IsNullOrWhiteSpace(filtroTexto);
                p.Add("@tieneFiltro", tieneFiltro ? 1 : 0);
                p.Add("@filtro", tieneFiltro ? $"%{filtroTexto!.Trim()}%" : null);

                // Total
                string qTotal = $@"
                    SELECT COUNT({ColNumPlaca}) 
                    FROM Activos_Principal
                    WHERE (@tieneFiltro = 0
                           OR {ColNumPlaca}     LIKE @filtro
                           OR {ColPlacaAlterna} LIKE @filtro
                           OR {ColNombre}       LIKE @filtro);";

                response.Result.total = connection.QueryFirstOrDefault<int>(qTotal, p);

                // ORDER BY con lista blanca -> índice de columna
                var sortKey = string.IsNullOrWhiteSpace(filtros?.sortField)
                    ? ColNumPlaca
                    : filtros.sortField!;

                if (!SortFieldMap.TryGetValue(sortKey, out var sortFieldCanonical))
                    sortFieldCanonical = ColNumPlaca;

                int sortIndex = sortFieldCanonical switch
                {
                    var s when s == ColPlacaAlterna => 2,
                    var s when s == ColNombre       => 3,
                    _                               => 1 // num_placa
                };
                p.Add("@sortIndex", sortIndex);

                int sortDir = (filtros?.sortOrder ?? 0) == 0 ? 0 : 1; // 0 = DESC, 1 = ASC
                p.Add("@sortDir", sortDir);

                // Paginación (pagina 1-based)
                int pagina = filtros?.pagina ?? 1;
                int paginacion = filtros?.paginacion ?? 10;
                int offset = pagina <= 1 ? 0 : (pagina - 1) * paginacion;

                p.Add("@offset", offset);
                p.Add("@fetch", paginacion);

                string query = $@"
                    SELECT  
                        {ColNumPlaca}     AS num_placa, 
                        {ColPlacaAlterna} AS Placa_Alterna, 
                        {ColNombre}       AS Nombre 
                    FROM Activos_Principal  
                    WHERE (@tieneFiltro = 0
                           OR {ColNumPlaca}     LIKE @filtro
                           OR {ColPlacaAlterna} LIKE @filtro
                           OR {ColNombre}       LIKE @filtro)
                    ORDER BY
                        -- ASC
                        CASE @sortDir WHEN 1 THEN
                            CASE @sortIndex
                                WHEN 1 THEN {ColNumPlaca}
                                WHEN 2 THEN {ColPlacaAlterna}
                                WHEN 3 THEN {ColNombre}
                            END
                        END ASC,
                        -- DESC
                        CASE @sortDir WHEN 0 THEN
                            CASE @sortIndex
                                WHEN 1 THEN {ColNumPlaca}
                                WHEN 2 THEN {ColPlacaAlterna}
                                WHEN 3 THEN {ColNombre}
                            END
                        END DESC
                    OFFSET @offset ROWS 
                    FETCH NEXT @fetch ROWS ONLY;";

                response.Result.lista = connection
                    .Query<ActivosData>(query, p)
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Método para consultar el detalle del numero de placa
        /// </summary>
        public ErrorDto<ActivosRenumeracionData> Activos_Renumeracion_Obtener(int CodEmpresa, string num_placa)
        {
            var result = new ErrorDto<ActivosRenumeracionData>()
            {
                Code = 0,
                Description = MensajeOk,
                Result = new ActivosRenumeracionData()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    SELECT 
                        A.num_placa,
                        A.nombre,
                        T.descripcion
                    FROM Activos_Principal A 
                    INNER JOIN Activos_tipo_Activo T
                        ON A.tipo_activo = T.tipo_activo
                    WHERE A.num_placa = @num_placa;";

                result.Result = connection
                    .Query<ActivosRenumeracionData>(query, new { num_placa })
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Método para actualizar el numero de placa
        /// </summary>
        public ErrorDto Activos_Renumeracion_Actualizar(int CodEmpresa, string usuario, string num_placa, string nuevo_num)
        {
            var result = new ErrorDto
            {
                Code = 0,
                Description = MensajeOk
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string queryExisteNuevoNumero = @"
                    SELECT COUNT(1)
                    FROM Activos_Principal
                    WHERE num_placa = @nuevo_num;";

                int existeNuevoNumero = connection.QueryFirstOrDefault<int>(queryExisteNuevoNumero, new
                {
                    nuevo_num
                });

                if (existeNuevoNumero > 0)
                {
                    result.Code = -1;
                    result.Description = MensajeNuevoNumeroExiste;
                    return result;
                }

                const string query = @"
                    UPDATE Activos_Principal
                       SET num_placa = @nuevo_num                                      
                     WHERE num_placa = @num_placa;";

                int filasAfectadas = connection.Execute(query, new
                {
                    num_placa,
                    nuevo_num
                });

                if (filasAfectadas == 0)
                {
                    result.Code = -1;
                    result.Description = "No se encontró el activo a renumerar.";
                    return result;
                }

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Renumeración: {num_placa} a {nuevo_num}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                result.Code = -1;
                result.Description = MensajeRenumeracionConRelacion;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }
    }
}