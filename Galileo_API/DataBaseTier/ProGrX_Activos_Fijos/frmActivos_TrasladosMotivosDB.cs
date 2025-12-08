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

        private const string MsgOk = "Ok";

        private const string MovimientoLabel   = "Motivo de Traslado: ";
        private const string MsgMotivoExiste   = "El Motivo {0} ya existe.";
        private const string MsgMotivoNoExiste = "El Motivo {0} no existe.";

        private const string SortAsc  = "ASC";
        private const string SortDesc = "DESC";

        private const string LikeWildcard = "%";

        public FrmActivosTrasladosMotivosDb(IConfiguration config)
        {
            _security = new MSecurityMainDb(config);
            _portalDb = new PortalDB(config);
        }

        // --------------------------------------------------------
        // Resolución segura del campo de ordenamiento (whitelist)
        // --------------------------------------------------------
        private static string ResolveSortField(string input)
        {
            var key = (input ?? string.Empty).Trim().ToLowerInvariant();

            return key switch
            {
                "descripcion" => "descripcion",
                "activo"      => "activo",
                _             => "cod_motivo"
            };
        }

        // ===============================================================
        // SQL CONSTANTES (sin interpolación)  -> no S2077
        // ===============================================================

        // Filtro común: si @filtro es NULL no filtra, si no, aplica LIKE
        private const string SqlWhereFiltro = @"
WHERE (@filtro IS NULL
       OR cod_motivo  LIKE @filtro
       OR descripcion LIKE @filtro)";

        // Total de filas
        private const string SqlTotal = @"
SELECT COUNT(cod_motivo)
FROM ACTIVOS_TRASLADOS_MOTIVOS
" + SqlWhereFiltro + ";";

        // Select paginado con ORDER BY dinámico vía CASE
        private const string SqlSelectPaginado = @"
SELECT 
    cod_motivo        AS cod_motivo,
    descripcion       AS descripcion,
    activo            AS activo,
    registro_usuario  AS registro_usuario,
    registro_fecha    AS registro_fecha
FROM ACTIVOS_TRASLADOS_MOTIVOS
" + SqlWhereFiltro + @"
ORDER BY
    -- orden ascendente
    CASE 
        WHEN @sortOrder = 'ASC' AND @sortField = 'cod_motivo'  THEN cod_motivo
        WHEN @sortOrder = 'ASC' AND @sortField = 'descripcion' THEN descripcion
        WHEN @sortOrder = 'ASC' AND @sortField = 'activo'      THEN activo
    END ASC,
    -- orden descendente
    CASE 
        WHEN @sortOrder = 'DESC' AND @sortField = 'cod_motivo'  THEN cod_motivo
        WHEN @sortOrder = 'DESC' AND @sortField = 'descripcion' THEN descripcion
        WHEN @sortOrder = 'DESC' AND @sortField = 'activo'      THEN activo
    END DESC
OFFSET @offset ROWS
FETCH NEXT @fetch ROWS ONLY;";

        // Select para exportar (sin paginar)
        private const string SqlSelectAll = @"
SELECT 
    cod_motivo        AS cod_motivo,
    descripcion       AS descripcion,
    activo            AS activo,
    registro_usuario  AS registro_usuario,
    registro_fecha    AS registro_fecha
FROM ACTIVOS_TRASLADOS_MOTIVOS
" + SqlWhereFiltro + @"
ORDER BY cod_motivo;";

        // Existe motivo
        private const string SqlExiste = @"
SELECT COUNT(*) 
FROM ACTIVOS_TRASLADOS_MOTIVOS
WHERE cod_motivo = @codigo;";

        // UPDATE
        private const string SqlUpdate = @"
UPDATE ACTIVOS_TRASLADOS_MOTIVOS
   SET descripcion = @descripcion,
       activo      = @activo
 WHERE cod_motivo  = @cod_motivo;";

        // INSERT
        private const string SqlInsert = @"
INSERT INTO ACTIVOS_TRASLADOS_MOTIVOS
    (cod_motivo, descripcion, activo, registro_usuario, registro_fecha)
VALUES
    (@cod_motivo, @descripcion, @activo, @usuario, GETDATE());";

        // DELETE
        private const string SqlDelete = @"
DELETE FROM ACTIVOS_TRASLADOS_MOTIVOS
WHERE cod_motivo = @cod_motivo;";

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

                var filtroTxt = (filtros?.filtro ?? string.Empty).Trim();
                string? filtroParam = string.IsNullOrWhiteSpace(filtroTxt)
                    ? null
                    : LikeWildcard + filtroTxt + LikeWildcard;

                // Ordenamiento seguro
                string sortField = ResolveSortField(filtros?.sortField ?? string.Empty);
                string sortOrder = (filtros?.sortOrder ?? 0) == 0 ? SortDesc : SortAsc;

                // Paginación
                int pagina   = filtros?.pagina     ?? 1;
                int pageSize = filtros?.paginacion ?? 10;
                int offset   = pagina <= 1 ? 0 : (pagina - 1) * pageSize;

                var p = new DynamicParameters();
                p.Add("@filtro",    filtroParam);
                p.Add("@sortField", sortField);
                p.Add("@sortOrder", sortOrder);
                p.Add("@offset",    offset);
                p.Add("@fetch",     pageSize);

                // TOTAL
                result.Result.total = cn.Query<int>(SqlTotal, p).FirstOrDefault();

                // LISTA PAGINADA
                result.Result.lista = cn
                    .Query<ActivosTrasladosMotivosData>(SqlSelectPaginado, p)
                    .ToList();
            }
            catch (Exception ex)
            {
                result.Code         = -1;
                result.Description  = ex.Message;
                result.Result.lista = new List<ActivosTrasladosMotivosData>();
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

                var filtroTxt = (filtros?.filtro ?? string.Empty).Trim();
                string? filtroParam = string.IsNullOrWhiteSpace(filtroTxt)
                    ? null
                    : LikeWildcard + filtroTxt + LikeWildcard;

                var p = new DynamicParameters();
                p.Add("@filtro", filtroParam);

                resp.Result = cn
                    .Query<ActivosTrasladosMotivosData>(SqlSelectAll, p)
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

                int existe = cn.QueryFirstOrDefault<int>(
                    SqlExiste,
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

                cn.Execute(SqlUpdate, new
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

                cn.Execute(SqlInsert, new
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

                cn.Execute(SqlDelete, new { cod_motivo });

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