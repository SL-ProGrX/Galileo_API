using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosTrasladosMotivosDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;

        private const string OkMessage              = "Ok";
        private const string TableMotivos           = "ACTIVOS_TRASLADOS_MOTIVOS";
        private const string ColCodMotivo           = "cod_motivo";
        private const string ColDescripcion         = "descripcion";
        private const string ColActivo              = "activo";
        private const string ColRegistroUsuario     = "registro_usuario";
        private const string ColRegistroFecha       = "registro_fecha";
        private const string ParamFiltro            = "@filtro";
        private const string MovimientoDetalleLabel = "Motivo de Traslado: ";

        private const string _codMotivo = ColCodMotivo;

        // Lista blanca para ORDER BY
        private static readonly Dictionary<string, string> SortFieldMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { _codMotivo,       ColCodMotivo   },
                { ColDescripcion,   ColDescripcion },
                { ColActivo,        ColActivo      }
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
            var result = new ErrorDto<ActivosTrasladosMotivosDataLista>
            {
                Code = 0,
                Description = OkMessage,
                Result = new ActivosTrasladosMotivosDataLista
                {
                    total = 0,
                    lista = new List<ActivosTrasladosMotivosData>()
                }
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var where = string.Empty;
                var p = new DynamicParameters();

                var filtroTexto = (filtros?.filtro ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(filtroTexto))
                {
                    where = $@" WHERE ( {ColCodMotivo}   LIKE {ParamFiltro}
                                   OR {ColDescripcion} LIKE {ParamFiltro} ) ";
                    p.Add(ParamFiltro, "%" + filtroTexto + "%");
                }

                // Total
                var qTotal = $@"
                    SELECT COUNT({ColCodMotivo})
                    FROM {TableMotivos}
                    {where}";
                result.Result.total = connection.Query<int>(qTotal, p).FirstOrDefault();

                // Orden
                var sortKey = string.IsNullOrWhiteSpace(filtros?.sortField)
                    ? _codMotivo
                    : filtros.sortField!;
                if (!SortFieldMap.TryGetValue(sortKey, out var sortField))
                    sortField = _codMotivo;

                var sortOrder = (filtros?.sortOrder ?? 0) == 0 ? "DESC" : "ASC";

                // Paginación (pagina 1-based)
                var pagina = filtros?.pagina ?? 1;
                var paginacion = filtros?.paginacion ?? 10;
                var offset = pagina <= 1 ? 0 : (pagina - 1) * paginacion;

                p.Add("@offset", offset);
                p.Add("@fetch", paginacion);

                var qDatos = $@"
                    SELECT 
                        {ColCodMotivo}       AS cod_motivo,
                        {ColDescripcion}     AS descripcion,
                        {ColActivo}          AS activo,
                        {ColRegistroUsuario} AS registro_usuario,
                        {ColRegistroFecha}   AS registro_fecha
                    FROM {TableMotivos}
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
            var result = new ErrorDto<List<ActivosTrasladosMotivosData>>
            {
                Code = 0,
                Description = OkMessage,
                Result = new List<ActivosTrasladosMotivosData>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var where = string.Empty;
                var p = new DynamicParameters();

                var filtroTexto = (filtros?.filtro ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(filtroTexto))
                {
                    where = $@" WHERE ( {ColCodMotivo}   LIKE {ParamFiltro}
                                   OR {ColDescripcion} LIKE {ParamFiltro} ) ";
                    p.Add(ParamFiltro, "%" + filtroTexto + "%");
                }

                var query = $@"
                    SELECT 
                        {ColCodMotivo}       AS cod_motivo,
                        {ColDescripcion}     AS descripcion,
                        {ColActivo}          AS activo,
                        {ColRegistroUsuario} AS registro_usuario,
                        {ColRegistroFecha}   AS registro_fecha
                    FROM {TableMotivos}
                    {where}
                    ORDER BY {ColCodMotivo};";

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
            var result = new ErrorDto
            {
                Code = 0,
                Description = OkMessage
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var queryExiste = $@"
                    SELECT COALESCE(COUNT(*),0) as Existe 
                    FROM {TableMotivos}  
                    WHERE {ColCodMotivo} = @codigo ";
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
            var result = new ErrorDto
            {
                Code = 0,
                Description = OkMessage
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"
                    UPDATE {TableMotivos} 
                       SET {ColDescripcion} = @{ColDescripcion},
                           {ColActivo}      = @{ColActivo}
                     WHERE {ColCodMotivo}  = @{ColCodMotivo}";

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
                    DetalleMovimiento = $"{MovimientoDetalleLabel}{datos.cod_motivo}",
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
            var result = new ErrorDto
            {
                Code = 0,
                Description = OkMessage
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"
                    INSERT INTO {TableMotivos}
                        ({ColCodMotivo}, {ColDescripcion}, {ColActivo}, {ColRegistroUsuario}, {ColRegistroFecha})
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
                    DetalleMovimiento = $"{MovimientoDetalleLabel}{datos.cod_motivo}",
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
            var result = new ErrorDto
            {
                Code = 0,
                Description = OkMessage
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"DELETE FROM {TableMotivos} WHERE {ColCodMotivo} = @{ColCodMotivo}";
                connection.Execute(query, new { cod_motivo });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"{MovimientoDetalleLabel}{cod_motivo}",
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