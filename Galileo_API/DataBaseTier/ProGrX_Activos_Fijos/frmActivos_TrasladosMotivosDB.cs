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
        private readonly MSecurityMainDb _security;
        private readonly PortalDB _portalDb;

        private const string MsgOk  = "Ok";
        private const string Table  = "ACTIVOS_TRASLADOS_MOTIVOS";

        private const string ColCodMotivo   = "cod_motivo";
        private const string ColDescripcion = "descripcion";
        private const string ColActivo      = "activo";
        private const string ColRegUsuario  = "registro_usuario";
        private const string ColRegFecha    = "registro_fecha";

        private const string ParamFiltro = "@filtro";
        private const string ParamOffset = "@offset";
        private const string ParamFetch  = "@fetch";

        private const string MovimientoLabel   = "Motivo de Traslado: ";
        private const string MsgMotivoExiste   = "El Motivo {0} ya existe.";
        private const string MsgMotivoNoExiste = "El Motivo {0} no existe.";

        private const string SortAsc  = "ASC";
        private const string SortDesc = "DESC";

        private const string LikeWildcard = "%";

        // SELECT base común para evitar duplicar el literal (S1192)
        private static readonly string SelectBase = $@"
                    SELECT 
                        {ColCodMotivo}   AS cod_motivo,
                        {ColDescripcion} AS descripcion,
                        {ColActivo}      AS activo,
                        {ColRegUsuario}  AS registro_usuario,
                        {ColRegFecha}    AS registro_fecha
                    FROM {Table}";

        public FrmActivosTrasladosMotivosDb(IConfiguration config)
        {
            _security = new MSecurityMainDb(config);
            _portalDb = new PortalDB(config);
        }

        // ------------------------------
        // VALIDACIÓN DE ORDEN (S2077)
        // ------------------------------
        private static string ResolveSortField(string input)
        {
            var key = (input ?? string.Empty).Trim().ToLowerInvariant();

            return key switch
            {
                ColDescripcion => ColDescripcion,
                ColActivo      => ColActivo,
                _              => ColCodMotivo
            };
        }

        // WHERE común para filtro de texto (evita duplicar literal, S1192)
        private static string BuildWhereFiltro(string filtroTxt, DynamicParameters p)
        {
            filtroTxt = (filtroTxt ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(filtroTxt))
            {
                return string.Empty;
            }

            p.Add(ParamFiltro, LikeWildcard + filtroTxt + LikeWildcard);

            return $@"
                    WHERE ({ColCodMotivo}   LIKE {ParamFiltro}
                       OR {ColDescripcion} LIKE {ParamFiltro}) ";
        }

        // ===============================================================
        // CONSULTAR LISTA PAGINADA
        // ===============================================================
        public ErrorDto<ActivosTrasladosMotivosDataLista> Activos_TrasladosMotivos_Consultar(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<ActivosTrasladosMotivosDataLista>
            {
                Code        = 0,
                Description = MsgOk,
                Result      = new ActivosTrasladosMotivosDataLista
                {
                    total = 0,
                    lista = new List<ActivosTrasladosMotivosData>()
                }
            };

            try
            {
                using var cn = _portalDb.CreateConnection(CodEmpresa);

                var p     = new DynamicParameters();
                var where = BuildWhereFiltro(filtros?.filtro ?? string.Empty, p);

                // TOTAL
                string sqlTotal = $@"
                    SELECT COUNT({ColCodMotivo})
                    FROM {Table}
                    {where}";
                result.Result.total = cn.Query<int>(sqlTotal, p).FirstOrDefault();

                // ORDENAMIENTO seguro (S2077 compliant)
                string sortField = ResolveSortField(filtros?.sortField ?? string.Empty);
                string sortOrder = (filtros?.sortOrder ?? 0) == 0 ? SortDesc : SortAsc;

                // PAGINACIÓN
                int pagina   = filtros?.pagina     ?? 1;
                int pageSize = filtros?.paginacion ?? 10;
                int offset   = pagina <= 1 ? 0 : (pagina - 1) * pageSize;

                p.Add(ParamOffset, offset);
                p.Add(ParamFetch,  pageSize);

                string sql = $@"
                    {SelectBase}
                    {where}
                    ORDER BY {sortField} {sortOrder}
                    OFFSET {ParamOffset} ROWS
                    FETCH NEXT {ParamFetch} ROWS ONLY;";

                result.Result.lista = cn
                    .Query<ActivosTrasladosMotivosData>(sql, p)
                    .ToList();
            }
            catch (Exception ex)
            {
                result.Code         = -1;
                result.Description  = ex.Message;
                result.Result.lista = [];
            }

            return result;
        }

        // ===============================================================
        // CONSULTA PARA EXPORTAR (SIN PAGINAR)
        // ===============================================================
        public ErrorDto<List<ActivosTrasladosMotivosData>> Activos_TrasladosMotivos_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<List<ActivosTrasladosMotivosData>>
            {
                Code        = 0,
                Description = MsgOk,
                Result      = new List<ActivosTrasladosMotivosData>()
            };

            try
            {
                using var cn = _portalDb.CreateConnection(CodEmpresa);

                var p     = new DynamicParameters();
                var where = BuildWhereFiltro(filtros?.filtro ?? string.Empty, p);

                string sql = $@"
                    {SelectBase}
                    {where}
                    ORDER BY {ColCodMotivo};";

                resp.Result = cn
                    .Query<ActivosTrasladosMotivosData>(sql, p)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        // ===============================================================
        // GUARDAR (INSERTAR / ACTUALIZAR)
        // ===============================================================
        public ErrorDto Activos_TrasladosMotivos_Guardar(
            int CodEmpresa,
            string usuario,
            ActivosTrasladosMotivosData datos)
        {
            var resp = new ErrorDto { Code = 0, Description = MsgOk };

            try
            {
                using var cn = _portalDb.CreateConnection(CodEmpresa);

                string sqlExiste = $@"
                    SELECT COUNT(*) 
                    FROM {Table}
                    WHERE {ColCodMotivo} = @codigo";

                int existe = cn.QueryFirstOrDefault<int>(
                    sqlExiste,
                    new { codigo = datos.cod_motivo });

                if (datos.isNew)
                {
                    if (existe > 0)
                    {
                        return new ErrorDto
                        {
                            Code        = -2,
                            Description = string.Format(MsgMotivoExiste, datos.cod_motivo)
                        };
                    }

                    return Activos_TrasladosMotivos_Insertar(CodEmpresa, usuario, datos);
                }

                if (existe == 0)
                {
                    return new ErrorDto
                    {
                        Code        = -2,
                        Description = string.Format(MsgMotivoNoExiste, datos.cod_motivo)
                    };
                }

                return Activos_TrasladosMotivos_Actualizar(CodEmpresa, usuario, datos);
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        // ===============================================================
        // ACTUALIZAR
        // ===============================================================
        private ErrorDto Activos_TrasladosMotivos_Actualizar(
            int CodEmpresa,
            string usuario,
            ActivosTrasladosMotivosData datos)
        {
            var resp = new ErrorDto { Code = 0, Description = MsgOk };

            try
            {
                using var cn = _portalDb.CreateConnection(CodEmpresa);

                string sql = $@"
                    UPDATE {Table}
                       SET {ColDescripcion} = @{ColDescripcion},
                           {ColActivo}      = @{ColActivo}
                     WHERE {ColCodMotivo}  = @{ColCodMotivo}";

                cn.Execute(sql, new
                {
                    datos.cod_motivo,
                    datos.descripcion,
                    datos.activo
                });

                _security.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId         = CodEmpresa,
                    Usuario           = usuario,
                    DetalleMovimiento = $"{MovimientoLabel}{datos.cod_motivo}",
                    Movimiento        = "Modifica - WEB",
                    Modulo            = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        // ===============================================================
        // INSERTAR
        // ===============================================================
        private ErrorDto Activos_TrasladosMotivos_Insertar(
            int CodEmpresa,
            string usuario,
            ActivosTrasladosMotivosData datos)
        {
            var resp = new ErrorDto { Code = 0, Description = MsgOk };

            try
            {
                using var cn = _portalDb.CreateConnection(CodEmpresa);

                string sql = $@"
                    INSERT INTO {Table}
                        ({ColCodMotivo}, {ColDescripcion}, {ColActivo}, {ColRegUsuario}, {ColRegFecha})
                    VALUES
                        (@cod_motivo, @descripcion, @activo, @usuario, GETDATE());";

                cn.Execute(sql, new
                {
                    datos.cod_motivo,
                    datos.descripcion,
                    datos.activo,
                    usuario
                });

                _security.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId         = CodEmpresa,
                    Usuario           = usuario,
                    DetalleMovimiento = $"{MovimientoLabel}{datos.cod_motivo}",
                    Movimiento        = "Registra - WEB",
                    Modulo            = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        // ===============================================================
        // ELIMINAR
        // ===============================================================
        public ErrorDto Activos_TrasladosMotivos_Eliminar(
            int CodEmpresa,
            string usuario,
            string cod_motivo)
        {
            var resp = new ErrorDto { Code = 0, Description = MsgOk };

            try
            {
                using var cn = _portalDb.CreateConnection(CodEmpresa);

                string sql = $@"DELETE FROM {Table} WHERE {ColCodMotivo} = @cod_motivo";
                cn.Execute(sql, new { cod_motivo });

                _security.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId         = CodEmpresa,
                    Usuario           = usuario,
                    DetalleMovimiento = $"{MovimientoLabel}{cod_motivo}",
                    Movimiento        = "Elimina - WEB",
                    Modulo            = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }
    }
}