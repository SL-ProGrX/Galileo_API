using Dapper;
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

        // Lista blanca para ORDER BY
        private static readonly Dictionary<string, string> SortFieldMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { ColNumPlaca,     ColNumPlaca },
                { ColPlacaAlterna, ColPlacaAlterna },
                { ColNombre,       ColNombre },
                // por si vienen en minúsculas desde el front:
                { "placa_alterna", ColPlacaAlterna },
                { "nombre",        ColNombre }
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
                const string query = @"
                    UPDATE Activos_Principal
                       SET num_placa = @nuevo_num                                      
                     WHERE num_placa = @num_placa;";

                connection.Execute(query, new
                {
                    num_placa,
                    nuevo_num
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Renumeración: {num_placa} a {nuevo_num}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
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