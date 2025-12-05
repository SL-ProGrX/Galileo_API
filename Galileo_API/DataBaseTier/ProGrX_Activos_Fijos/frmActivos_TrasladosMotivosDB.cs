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
    public class FrmActivosTrasladosMotivosDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;
        private const string _codMotivo = "cod_motivo";

        // Lista blanca para ORDER BY
        private static readonly Dictionary<string, string> SortFieldMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { _codMotivo, _codMotivo },
                { "descripcion", "descripcion" },
                { "activo", "activo" }
            };

        public FrmActivosTrasladosMotivosDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método para consultar la lista de motivos de traslados
        /// </summary>
        public ErrorDto<ActivosTrasladosMotivosDataLista> Activos_TrasladosMotivos_Consultar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<ActivosTrasladosMotivosDataLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosTrasladosMotivosDataLista()
                {
                    total = 0,
                    lista = new List<ActivosTrasladosMotivosData>()
                }
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var where = "";
                var p = new DynamicParameters();

                var filtroTexto = (filtros?.filtro ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(filtroTexto))
                {
                    where = @" WHERE ( cod_motivo   LIKE @filtro
                                   OR descripcion LIKE @filtro ) ";
                    p.Add("@filtro", "%" + filtroTexto + "%");
                }

                // Total
                var qTotal = $@"SELECT COUNT(cod_motivo)
                                FROM ACTIVOS_TRASLADOS_MOTIVOS
                                {where}";
                result.Result.total = connection.Query<int>(qTotal, p).FirstOrDefault();

                // Orden
                var sortKey = string.IsNullOrWhiteSpace(filtros?.sortField)
                    ? _codMotivo
                    : filtros.sortField!;
                if (!SortFieldMap.TryGetValue(sortKey, out var sortField))
                    sortField = _codMotivo;

                var sortOrder = (filtros?.sortOrder ?? 0) == 0 ? "DESC" : "ASC";

                // Paginación (asumo pagina 1-based)
                var pagina = filtros?.pagina ?? 1;
                var paginacion = filtros?.paginacion ?? 10;
                var offset = pagina <= 1 ? 0 : (pagina - 1) * paginacion;

                p.Add("@offset", offset);
                p.Add("@fetch", paginacion);

                var qDatos = $@"
                SELECT 
                    cod_motivo,
                    descripcion,
                    activo,
                    registro_usuario,
                    registro_fecha
                FROM ACTIVOS_TRASLADOS_MOTIVOS
                {where}
                ORDER BY {sortField} {sortOrder}
                OFFSET @offset ROWS 
                FETCH NEXT @fetch ROWS ONLY;";

                result.Result.lista = connection
                    .Query<ActivosTrasladosMotivosData>(qDatos, p)
                    .ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = [];
            }
            return result;
        }

        /// <summary>
        /// Método para consultar datos de motivos de Traslado a exportar
        /// </summary>
        public ErrorDto<List<ActivosTrasladosMotivosData>> Activos_TrasladosMotivos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<ActivosTrasladosMotivosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosTrasladosMotivosData>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var where = "";
                var p = new DynamicParameters();

                var filtroTexto = (filtros?.filtro ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(filtroTexto))
                {
                    where = @" WHERE ( cod_motivo   LIKE @filtro
                                   OR descripcion LIKE @filtro ) ";
                    p.Add("@filtro", "%" + filtroTexto + "%");
                }

                var query = $@"
                SELECT 
                    cod_motivo,
                    descripcion,
                    activo,
                    registro_usuario,
                    registro_fecha
                FROM ACTIVOS_TRASLADOS_MOTIVOS 
                {where}
                ORDER BY cod_motivo;";

                result.Result = connection
                    .Query<ActivosTrasladosMotivosData>(query, p)
                    .ToList();
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
        /// Método para insertar o actualizar un Motivo de Traslado
        /// </summary>
        public ErrorDto Activos_TrasladosMotivos_Guardar(int CodEmpresa, string usuario, ActivosTrasladosMotivosData datos)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string queryExiste = @"SELECT COALESCE(COUNT(*),0) as Existe 
                                             FROM ACTIVOS_TRASLADOS_MOTIVOS  
                                             WHERE cod_motivo = @codigo ";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { codigo = datos.cod_motivo });

                if (datos.isNew)
                {
                    if (existe > 0)
                    {
                        result.Code = -2;
                        result.Description = $"El Motivo con el código {datos.cod_motivo} ya existe.";
                    }
                    else
                    {
                        result = Activos_TrasladosMotivos_Insertar(CodEmpresa, usuario, datos);
                    }
                }
                else if (existe == 0 && !datos.isNew)
                {
                    result.Code = -2;
                    result.Description = $"El Motivo con el código {datos.cod_motivo} no existe.";
                }
                else
                {
                    result = Activos_TrasladosMotivos_Actualizar(CodEmpresa, usuario, datos);
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Método para actualizar un Motivo de Traslado
        /// </summary>
        private ErrorDto Activos_TrasladosMotivos_Actualizar(int CodEmpresa, string usuario, ActivosTrasladosMotivosData datos)
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
                    UPDATE ACTIVOS_TRASLADOS_MOTIVOS 
                       SET descripcion = @descripcion,
                           activo      = @activo
                     WHERE cod_motivo  = @cod_motivo";

                connection.Execute(query, new
                {
                    datos.cod_motivo,
                    datos.descripcion,
                    datos.activo
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Motivo de Traslado:  {datos.cod_motivo}",
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

        /// <summary>
        /// Método para insertar un Motivo de Traslado
        /// </summary>
        private ErrorDto Activos_TrasladosMotivos_Insertar(int CodEmpresa, string usuario, ActivosTrasladosMotivosData datos)
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
                    INSERT INTO ACTIVOS_TRASLADOS_MOTIVOS
                        (cod_motivo, descripcion, activo, registro_usuario, registro_fecha)
                    VALUES 
                        (@cod_motivo, @descripcion, @activo, @usuario, GETDATE());";

                connection.Execute(query, new
                {
                    datos.cod_motivo,
                    datos.descripcion,
                    datos.activo,
                    usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Motivo de Traslado: {datos.cod_motivo}",
                    Movimiento = "Registra - WEB",
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

        /// <summary>
        /// Método para eliminar un motivo
        /// </summary>
        public ErrorDto Activos_TrasladosMotivos_Eliminar(int CodEmpresa, string usuario, string cod_motivo)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"DELETE FROM ACTIVOS_TRASLADOS_MOTIVOS WHERE cod_motivo = @cod_motivo";
                connection.Execute(query, new { cod_motivo });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Motivo de Traslado: {cod_motivo}",
                    Movimiento = "Elimina - WEB",
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