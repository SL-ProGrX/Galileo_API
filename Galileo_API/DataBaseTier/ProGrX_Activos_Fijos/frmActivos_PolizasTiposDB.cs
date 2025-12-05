using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosPolizasTiposDb
    {
        private readonly int vModulo = 36; // Modulo de Activos Fijos
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;
        private const string _tipoPolizaCol = "tipo_poliza";

        public FrmActivosPolizasTiposDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene una lista de tipos de póliza de activos fijos con paginación y filtros.
        /// </summary>
        public ErrorDto<ActivosPolizasTiposLista> Activos_PolizasTiposLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<ActivosPolizasTiposLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosPolizasTiposLista()
                {
                    total = 0,
                    lista = new List<ActivosPolizasTiposData>()
                }
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();

                // Filtro
                string whereSql = string.Empty;
                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    whereSql = @"
WHERE ( tipo_poliza LIKE @filtro
    OR  descripcion LIKE @filtro )";
                    p.Add("@filtro", $"%{filtros.filtro.Trim()}%");
                }

                // Total con los mismos filtros
                var countSql = $@"
SELECT COUNT(tipo_poliza)
FROM   activos_polizas_tipos
{whereSql};";

                result.Result.total = connection.QueryFirstOrDefault<int>(countSql, p);

                // Paginación
                int pagina     = filtros?.pagina     ?? 0;
                int paginacion = filtros?.paginacion ?? 50;
                p.Add("@offset", pagina);
                p.Add("@rows",   paginacion);

                // Sort seguro (whitelist)
                var sortFieldRaw  = filtros?.sortField ?? _tipoPolizaCol;
                var sortFieldNorm = sortFieldRaw.Trim().ToLowerInvariant();

                string orderByCol = sortFieldNorm switch
                {
                    "tipo_poliza" => "tipo_poliza",
                    "descripcion" => "descripcion",
                    "activo"      => "ACTIVO",
                    _             => _tipoPolizaCol
                };

                string sortOrder = (filtros?.sortOrder ?? 0) == 0 ? "DESC" : "ASC";

                var dataSql = $@"
SELECT tipo_poliza,
       descripcion,
       CASE WHEN ISNULL(ACTIVO, 0) = 0 THEN 0 ELSE 1 END AS ACTIVO
FROM   activos_polizas_tipos
{whereSql}
ORDER BY {orderByCol} {sortOrder}
OFFSET @offset ROWS 
FETCH NEXT @rows ROWS ONLY;";

                result.Result.lista = connection
                    .Query<ActivosPolizasTiposData>(dataSql, p)
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
        /// Obtiene una lista de tipos de pólizas de activos fijos sin paginación, con filtros aplicados.
        /// </summary>
        public ErrorDto<List<ActivosPolizasTiposData>> Activos_PolizasTipos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<ActivosPolizasTiposData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosPolizasTiposData>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                string whereSql = string.Empty;

                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    whereSql = @"
WHERE ( tipo_poliza LIKE @filtro
    OR  descripcion LIKE @filtro )";
                    p.Add("@filtro", $"%{filtros.filtro.Trim()}%");
                }

                var query = $@"
SELECT tipo_poliza,
       descripcion,
       CASE WHEN ISNULL(ACTIVO, 0) = 0 THEN 0 ELSE 1 END AS ACTIVO
FROM   activos_polizas_tipos
{whereSql}
ORDER BY tipo_poliza;";

                result.Result = connection
                    .Query<ActivosPolizasTiposData>(query, p)
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
        /// Inserta o actualiza un tipo de póliza de activos fijos.
        /// </summary>
        public ErrorDto Activos_PolizasTipos_Guardar(int CodEmpresa, string usuario, ActivosPolizasTiposData tipoPoliza)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                // Verifico si existe usuario (activo)
                const string qUsuario = @"
SELECT COUNT(Nombre) 
FROM   usuarios 
WHERE  estado = 'A' 
  AND  UPPER(Nombre) LIKE '%' + @usr + '%'";
                int existeuser = connection.QueryFirstOrDefault<int>(qUsuario, new { usr = usuario.ToUpper() });
                if (existeuser == 0)
                {
                    result.Code = -2;
                    result.Description = $"El usuario {usuario.ToUpper()} no existe o no está activo.";
                    return result;
                }

                // Verifico si existe tipoPoliza
                const string qExiste = @"
SELECT ISNULL(COUNT(*),0) as Existe 
FROM   activos_polizas_tipos  
WHERE  UPPER(tipo_poliza) = @tipoPoliza";
                var existe = connection.QueryFirstOrDefault<int>(qExiste, new { tipoPoliza = tipoPoliza.tipo_poliza.ToUpper() });

                if (tipoPoliza.isNew)
                {
                    if (existe > 0)
                    {
                        result.Code = -2;
                        result.Description = $"El tipo de póliza con el código {tipoPoliza.tipo_poliza} ya existe.";
                    }
                    else
                    {
                        result = Activos_PolizasTipos_Insertar(CodEmpresa, usuario, tipoPoliza);
                    }
                }
                else if (existe == 0 && !tipoPoliza.isNew)
                {
                    result.Code = -2;
                    result.Description = $"El tipo de póliza con el código {tipoPoliza.tipo_poliza} no existe.";
                }
                else
                {
                    result = Activos_PolizasTipos_Actualizar(CodEmpresa, usuario, tipoPoliza);
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
        /// Actualiza un tipo de póliza existente.
        /// </summary>
        private ErrorDto Activos_PolizasTipos_Actualizar(int CodEmpresa, string usuario, ActivosPolizasTiposData tipoPoliza)
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
UPDATE activos_polizas_tipos
   SET descripcion      = @descripcion,
       activo           = @activo,
       modifica_usuario = @modifica_usuario,
       modifica_fecha   = SYSDATETIME()
 WHERE tipo_poliza      = @tipo_poliza";

                connection.Execute(query, new
                {
                    tipo_poliza       = tipoPoliza.tipo_poliza.ToUpper(),
                    descripcion       = tipoPoliza.descripcion?.ToUpper(),
                    activo            = tipoPoliza.activo,
                    modifica_usuario  = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipos de Pólizas Doc. : {tipoPoliza.tipo_poliza} - {tipoPoliza.descripcion}",
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
        /// Inserta un tipo de póliza.
        /// </summary>
        private ErrorDto Activos_PolizasTipos_Insertar(int CodEmpresa, string usuario, ActivosPolizasTiposData tipoPoliza)
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
INSERT INTO activos_polizas_tipos 
    (tipo_poliza, descripcion, activo, REGISTRO_USUARIO, REGISTRO_FECHA, MODIFICA_USUARIO, MODIFICA_FECHA)
VALUES 
    (@tipo_poliza, @descripcion, @activo, @registro_usuario, SYSDATETIME(), NULL, NULL)";

                connection.Execute(query, new
                {
                    tipo_poliza      = tipoPoliza.tipo_poliza.ToUpper(),
                    descripcion      = tipoPoliza.descripcion?.ToUpper(),
                    activo           = tipoPoliza.activo,
                    registro_usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipos de Pólizas Doc.. : {tipoPoliza.tipo_poliza} - {tipoPoliza.descripcion}",
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
        /// Elimina un tipo de póliza de activos fijos por su código.
        /// </summary>
        public ErrorDto Activos_PolizasTipos_Eliminar(int CodEmpresa, string usuario, string tipo_poliza)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string query = @"DELETE FROM activos_polizas_tipos WHERE tipo_poliza = @tipo_poliza";
                connection.Execute(query, new { tipo_poliza = tipo_poliza.ToUpper() });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipos de Pólizas Doc. : {tipo_poliza}",
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

        /// <summary>
        /// Valida si un código de tipo de póliza ya existe en la base de datos.
        /// </summary>
        public ErrorDto Activos_PolizasTipos_Valida(int CodEmpresa, string tipo_poliza)
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
SELECT COUNT(tipo_poliza) 
FROM   activos_polizas_tipos 
WHERE  UPPER(tipo_poliza) = @tipo_poliza";
                var existe = connection.QueryFirstOrDefault<int>(query, new { tipo_poliza = tipo_poliza.ToUpper() });

                if (existe > 0)
                {
                    result.Code = -1;
                    result.Description = "El código de tipo de póliza ya existe.";
                }
                else
                {
                    result.Code = 0;
                    result.Description = "El código de tipo de póliza es válido.";
                }

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