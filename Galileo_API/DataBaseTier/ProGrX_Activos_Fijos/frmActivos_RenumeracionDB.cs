using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosRenumeracionDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;
        private const string _numPlaca = "num_placa";

        // Lista blanca para ORDER BY
        private static readonly Dictionary<string, string> SortFieldMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { _numPlaca, _numPlaca },
                { "Placa_Alterna", "Placa_Alterna" },
                { "Nombre", "Nombre" },
                // por si vienen en minúsculas desde el front:
                { "placa_alterna", "Placa_Alterna" },
                { "nombre", "Nombre" }
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
                Description = "Ok",
                Result = new ActivosDataLista
                {
                    total = 0,
                    lista = new List<ActivosData>()
                }
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var where = "";
                var p = new DynamicParameters();

                // Filtro texto
                var filtroTexto = (filtros?.filtro ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(filtroTexto))
                {
                    where = @" WHERE ( num_placa     LIKE @filtro
                                    OR Placa_Alterna LIKE @filtro
                                    OR Nombre        LIKE @filtro )";
                    p.Add("@filtro", "%" + filtroTexto + "%");
                }

                // Total
                var qTotal = $@"SELECT COUNT(num_placa) 
                                FROM Activos_Principal
                                {where}";
                response.Result.total = connection.Query<int>(qTotal, p).FirstOrDefault();

                // ORDER BY con lista blanca
                var sortKey = string.IsNullOrWhiteSpace(filtros?.sortField)
                    ? _numPlaca
                    : filtros.sortField!;
                if (!SortFieldMap.TryGetValue(sortKey, out var sortField))
                    sortField = _numPlaca;

                var sortOrder = (filtros?.sortOrder ?? 0) == 0 ? "DESC" : "ASC";

                // Paginación (asumimos pagina 1-based)
                var pagina = filtros?.pagina ?? 1;
                var paginacion = filtros?.paginacion ?? 10;
                var offset = pagina <= 1 ? 0 : (pagina - 1) * paginacion;

                p.Add("@offset", offset);
                p.Add("@fetch", paginacion);

                var query = $@"
                    SELECT  
                        num_placa, 
                        Placa_Alterna, 
                        Nombre 
                    FROM Activos_Principal  
                    {where}
                    ORDER BY {sortField} {sortOrder}
                    OFFSET @offset ROWS 
                    FETCH NEXT @fetch ROWS ONLY;";

                response.Result.lista = connection.Query<ActivosData>(query, p).ToList();
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
                Description = "Ok",
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
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
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