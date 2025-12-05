using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;


namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosSeccionesDb
    {
        private readonly int vModulo = 36; // Activos Fijos
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;

        public FrmActivosSeccionesDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }


        /// <summary>
        /// Lista paginada (lazy) de departamentos.
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<ActivosDepartamentosLista> Activos_DepartamentosLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<ActivosDepartamentosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosDepartamentosLista()
            };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);
                string where = "";
                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    var f = filtros.filtro.Replace("'", "''");
                    where = $@" WHERE  d.COD_DEPARTAMENTO LIKE '%{f}%'
                                OR    d.DESCRIPCION    LIKE '%{f}%'
                                OR    d.COD_UNIDAD     LIKE '%{f}%'
                                OR    ISNULL(u.DESCRIPCION,'') LIKE '%{f}%'";
                }

                // total
                string queryTotal = $@"SELECT COUNT(DISTINCT d.COD_DEPARTAMENTO)
                                    FROM dbo.ACTIVOS_DEPARTAMENTOS d
                                    LEFT JOIN dbo.CNTX_UNIDADES u
                                      ON u.COD_UNIDAD = d.COD_UNIDAD
                                       {where}";
                resp.Result.total = cn.QueryFirstOrDefault<int>(queryTotal);

                // sort y paginación
                string sortField = string.IsNullOrWhiteSpace(filtros?.sortField) ? "d.COD_DEPARTAMENTO" : filtros.sortField;
                string sortOrder = filtros?.sortOrder == 0 ? "DESC" : "ASC";
                int pagina = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 50;

                string query = $@"
                       SELECT DISTINCT
                        d.COD_DEPARTAMENTO            AS cod_departamento,
                        ISNULL(d.DESCRIPCION,'')      AS descripcion,
                        ISNULL(d.COD_UNIDAD,'')       AS cod_unidad,
                        ISNULL(u.DESCRIPCION,'')      AS unidad_desc,
                        ISNULL(d.REGISTRO_USUARIO,'') AS usuario
                    FROM dbo.ACTIVOS_DEPARTAMENTOS d
                    LEFT JOIN dbo.CNTX_UNIDADES u
                      ON u.COD_UNIDAD = d.COD_UNIDAD
                    {where}
                    ORDER BY {sortField} {sortOrder}
                    OFFSET {pagina} ROWS FETCH NEXT {paginacion} ROWS ONLY;";

                resp.Result.lista = cn.Query<ActivosDepartamentosData>(query).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result.total = 0;
                resp.Result.lista = [];
            }
            return resp;
        }
        /// <summary>
        /// Lista completa de departamentos (sin paginar) para exportar.
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<ActivosDepartamentosData>> Activos_Departamentos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<List<ActivosDepartamentosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosDepartamentosData>()
            };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);
                string where = "";
                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    var f = filtros.filtro.Replace("'", "''");
                    where = $@" WHERE  d.COD_DEPARTAMENTO LIKE '%{f}%'
                                OR    d.DESCRIPCION    LIKE '%{f}%'
                                OR    d.COD_UNIDAD     LIKE '%{f}%'
                                OR    ISNULL(u.DESCRIPCION,'') LIKE '%{f}%'
                                OR    ISNULL(d.REGISTRO_USUARIO,'') LIKE '%{f}%'";
                }

                string query = $@"
                    SELECT
                        d.COD_DEPARTAMENTO                         AS cod_departamento,
                        ISNULL(d.DESCRIPCION,'')                   AS descripcion,
                        ISNULL(d.COD_UNIDAD,'')                    AS cod_unidad,
                        ISNULL(u.DESCRIPCION,'')                   AS unidad_desc,
                        ISNULL(d.REGISTRO_USUARIO,'')              AS usuario
                    FROM dbo.ACTIVOS_DEPARTAMENTOS d
                    LEFT JOIN dbo.CNTX_UNIDADES u
                      ON u.COD_UNIDAD = d.COD_UNIDAD
                    {where}
                    ORDER BY d.COD_DEPARTAMENTO;";

                resp.Result = cn.Query<ActivosDepartamentosData>(query).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }
        /// <summary>
        /// Guardar un departamento.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="departamento"></param>
        /// </summary>
        public ErrorDto Activos_Departamentos_Guardar(int CodEmpresa, string usuario, ActivosDepartamentosData departamento)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                if (departamento == null)
                    return new ErrorDto { Code = -1, Description = "Datos no proporcionados." };

                if (string.IsNullOrWhiteSpace(departamento.cod_departamento))
                    return new ErrorDto { Code = -1, Description = "Debe indicar el código de departamento." };

                if (string.IsNullOrWhiteSpace(departamento.descripcion))
                    return new ErrorDto { Code = -1, Description = "Debe indicar la descripción." };

                if (string.IsNullOrWhiteSpace(departamento.cod_unidad))
                    return new ErrorDto { Code = -1, Description = "Debe indicar la unidad contable." };

                using var cn = _portalDB.CreateConnection(CodEmpresa);

                const string queryExiste = @"SELECT COUNT(1)
                                             FROM dbo.ACTIVOS_DEPARTAMENTOS
                                             WHERE COD_DEPARTAMENTO = @cod;";
                int existe = cn.QueryFirstOrDefault<int>(queryExiste, new { cod = departamento.cod_departamento.ToUpper() });

                if (departamento.isNew)
                {
                    if (existe > 0)
                        return new ErrorDto { Code = -2, Description = $"El departamento {departamento.cod_departamento.ToUpper()} ya existe." };

                    return Activos_Departamentos_Insertar(CodEmpresa, usuario, departamento);
                }
                else
                {
                    if (existe == 0)
                        return new ErrorDto { Code = -2, Description = $"El departamento {departamento.cod_departamento.ToUpper()} no existe." };

                    return Activos_Departamentos_Actualizar(CodEmpresa, usuario, departamento);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
        /// <summary>
        /// Inserta un nuevo departamento.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="departamento"></param>
        /// </summary>
        /// <returns></returns>
        private ErrorDto Activos_Departamentos_Insertar(int CodEmpresa, string usuario, ActivosDepartamentosData departamento)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);

                string query = @"
                    INSERT INTO dbo.ACTIVOS_DEPARTAMENTOS
                       (COD_DEPARTAMENTO, DESCRIPCION, COD_UNIDAD,
                        REGISTRO_FECHA, REGISTRO_USUARIO, MODIFICA_USUARIO, MODIFICA_FECHA)
                    VALUES
                       (@cod, @desc, @unidad,
                        SYSDATETIME(), @usr, NULL, NULL);";

                cn.Execute(query, new
                {
                    cod = departamento.cod_departamento.ToUpper(),
                    desc = departamento.descripcion?.ToUpper(),
                    unidad = departamento.cod_unidad.ToUpper(),
                    usr = string.IsNullOrWhiteSpace(usuario) ? null : usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? "",
                    DetalleMovimiento = $"Departamento: {departamento.cod_departamento} - {departamento.descripcion} / Unidad: {departamento.cod_unidad}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                resp.Description = "Departamento ingresado satisfactoriamente.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
        /// <summary>
        /// Actualiza un departamento existente.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="departamento"></param>
        /// </summary>
        /// <returns></returns>
        private ErrorDto Activos_Departamentos_Actualizar(int CodEmpresa, string usuario, ActivosDepartamentosData departamento)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);

                string query = @"
                    UPDATE dbo.ACTIVOS_DEPARTAMENTOS
                       SET DESCRIPCION      = @desc,
                           COD_UNIDAD       = @unidad,
                           MODIFICA_USUARIO = @usr,
                           MODIFICA_FECHA   = SYSDATETIME()
                     WHERE COD_DEPARTAMENTO = @cod;";

                cn.Execute(query, new
                {
                    cod = departamento.cod_departamento.ToUpper(),
                    desc = departamento.descripcion?.ToUpper(),
                    unidad = departamento.cod_unidad.ToUpper(),
                    usr = string.IsNullOrWhiteSpace(usuario) ? null : usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? "",
                    DetalleMovimiento = $"Departamento: {departamento.cod_departamento} - {departamento.descripcion} / Unidad: {departamento.cod_unidad}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });

                resp.Description = "Departamento actualizado satisfactoriamente.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Eliminar departamento por su código.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_departamento"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Activos_Departamentos_Eliminar(int CodEmpresa, string usuario, string cod_departamento)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                if (string.IsNullOrWhiteSpace(cod_departamento))
                    return new ErrorDto { Code = -1, Description = "Debe indicar el código de departamento." };

                using var cn = _portalDB.CreateConnection(CodEmpresa);

                string query = @"DELETE FROM dbo.ACTIVOS_DEPARTAMENTOS WHERE COD_DEPARTAMENTO = @cod;";
                int rows = cn.Execute(query, new { cod = cod_departamento.ToUpper() });

                if (rows == 0)
                    return new ErrorDto { Code = -2, Description = $"El departamento {cod_departamento.ToUpper()} no existe." };

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? "",
                    DetalleMovimiento = $"Departamento: {cod_departamento}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
        /// <summary>
        /// Valida si un código de departamento ya existe en la base de datos ACTIVOS_Departamentos.
        /// </summary>
        /// <param name="CodEmpresa">.</param>
        /// <param name="cod_departamento"></param>
        /// <returns></returns>
        public ErrorDto Activos_Departamentos_Valida(int CodEmpresa, string cod_departamento)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"SELECT COUNT(1)
                                       FROM dbo.ACTIVOS_DEPARTAMENTOS
                                       WHERE UPPER(COD_DEPARTAMENTO) = @cod;";
                int existe = cn.QueryFirstOrDefault<int>(query, new { cod = (cod_departamento ?? string.Empty).ToUpper() });

                if (existe > 0)
                {
                    resp.Code = -1;
                    resp.Description = "El código de departamento ya existe.";
                }
                else
                {
                    resp.Code = 0;
                    resp.Description = "El código de departamento es válido.";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
        /// <summary>
        /// Lista paginada (lazy) de secciones. Puede filtrar por cod_departamento.
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_departamento"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<ActivosSeccionesLista> Activos_SeccionesLista_Obtener(int CodEmpresa, string? cod_departamento, FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<ActivosSeccionesLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosSeccionesLista()
            };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);

                string where = " WHERE 1=1 ";
                if (!string.IsNullOrWhiteSpace(cod_departamento))
                    where += " AND s.COD_DEPARTAMENTO = @dept ";

                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    var f = filtros.filtro.Replace("'", "''");
                    where += $@" AND (  s.COD_SECCION LIKE '%{f}%'
                                     OR s.DESCRIPCION LIKE '%{f}%'
                                     OR s.COD_CENTRO_COSTO LIKE '%{f}%'
                                     OR ISNULL(cc.DESCRIPCION,'') LIKE '%{f}%'
                                     OR ISNULL(d.DESCRIPCION,'')  LIKE '%{f}%')";
                }

                string qTotal = $@"
                    SELECT COUNT(1)
                    FROM dbo.ACTIVOS_SECCIONES s
                    LEFT JOIN dbo.CNTX_CENTRO_COSTOS cc
                        ON cc.COD_CENTRO_COSTO = s.COD_CENTRO_COSTO
                    LEFT JOIN dbo.ACTIVOS_DEPARTAMENTOS d
                        ON d.COD_DEPARTAMENTO = s.COD_DEPARTAMENTO
                    {where};";
                resp.Result.total = cn.QueryFirstOrDefault<int>(qTotal, new { dept = cod_departamento?.ToUpper() });

                string sortField = string.IsNullOrWhiteSpace(filtros?.sortField) ? "s.COD_SECCION" : filtros.sortField;
                string sortOrder = filtros?.sortOrder == 0 ? "DESC" : "ASC";
                int pagina = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 50;

                string query = $@"
                    SELECT
                        s.COD_DEPARTAMENTO             AS cod_departamento,
                        ISNULL(d.DESCRIPCION,'')       AS departamento_desc,
                        s.COD_SECCION                  AS cod_seccion,
                        ISNULL(s.DESCRIPCION,'')       AS descripcion,
                        ISNULL(s.COD_CENTRO_COSTO,'')  AS cod_centro_costo,
                        ISNULL(cc.DESCRIPCION,'')      AS centro_costo_desc,
                        ISNULL(s.REGISTRO_USUARIO,'')  AS usuario
                    FROM dbo.ACTIVOS_SECCIONES s
                    LEFT JOIN dbo.CNTX_CENTRO_COSTOS cc
                        ON cc.COD_CENTRO_COSTO = s.COD_CENTRO_COSTO
                    LEFT JOIN dbo.ACTIVOS_DEPARTAMENTOS d
                        ON d.COD_DEPARTAMENTO = s.COD_DEPARTAMENTO
                    {where}
                    ORDER BY {sortField} {sortOrder}
                    OFFSET {pagina} ROWS
                    FETCH NEXT {paginacion} ROWS ONLY;";
                resp.Result.lista = cn.Query<ActivosSeccionesData>(query, new { dept = cod_departamento?.ToUpper() }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result.total = 0;
                resp.Result.lista = [];
            }
            return resp;
        }

        /// <summary>
        /// Lista completa de secciones (sin paginar) para exportar. Puede filtrar por cod_departamento.
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_departamento"></param>
        /// <param name="filtros"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<ActivosSeccionesData>> Activos_Secciones_Obtener(int CodEmpresa, string? cod_departamento, FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<List<ActivosSeccionesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosSeccionesData>()
            };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);

                string where = " WHERE 1=1 ";
                if (!string.IsNullOrWhiteSpace(cod_departamento))
                    where += " AND s.COD_DEPARTAMENTO = @dept ";

                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    var f = filtros.filtro.Replace("'", "''");
                    where += $@" AND (  s.COD_SECCION LIKE '%{f}%'
                                     OR s.DESCRIPCION LIKE '%{f}%'
                                     OR s.COD_CENTRO_COSTO LIKE '%{f}%'
                                     OR ISNULL(cc.DESCRIPCION,'') LIKE '%{f}%'
                                     OR ISNULL(d.DESCRIPCION,'')  LIKE '%{f}%')";
                }

                string query = $@"
                    SELECT
                        s.COD_DEPARTAMENTO             AS cod_departamento,
                        ISNULL(d.DESCRIPCION,'')       AS departamento_desc,
                        s.COD_SECCION                  AS cod_seccion,
                        ISNULL(s.DESCRIPCION,'')       AS descripcion,
                        ISNULL(s.COD_CENTRO_COSTO,'')  AS cod_centro_costo,
                        ISNULL(cc.DESCRIPCION,'')      AS centro_costo_desc,
                        ISNULL(s.REGISTRO_USUARIO,'')  AS usuario
                    FROM dbo.ACTIVOS_SECCIONES s
                    LEFT JOIN dbo.CNTX_CENTRO_COSTOS cc
                        ON cc.COD_CENTRO_COSTO = s.COD_CENTRO_COSTO
                    LEFT JOIN dbo.ACTIVOS_DEPARTAMENTOS d
                        ON d.COD_DEPARTAMENTO = s.COD_DEPARTAMENTO
                    {where}
                    ORDER BY s.COD_SECCION;";
                resp.Result = cn.Query<ActivosSeccionesData>(query, new { dept = cod_departamento?.ToUpper() }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }

        /// <summary>
        /// Guardar una sección.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="seccion"></param>
        /// </summary>
        public ErrorDto Activos_Secciones_Guardar(int CodEmpresa, string usuario, ActivosSeccionesData seccion)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                if (seccion == null)
                    return new ErrorDto { Code = -1, Description = "Datos no proporcionados." };
                if (string.IsNullOrWhiteSpace(seccion.cod_departamento))
                    return new ErrorDto { Code = -1, Description = "Debe indicar el código de departamento." };
                if (string.IsNullOrWhiteSpace(seccion.cod_seccion))
                    return new ErrorDto { Code = -1, Description = "Debe indicar el código de sección." };
                if (string.IsNullOrWhiteSpace(seccion.descripcion))
                    return new ErrorDto { Code = -1, Description = "Debe indicar la descripción." };

                using var cn = _portalDB.CreateConnection(CodEmpresa);
                const string qExiste = @"
                    SELECT COUNT(1)
                    FROM dbo.ACTIVOS_SECCIONES
                    WHERE COD_DEPARTAMENTO = @dept AND COD_SECCION = @sec;";
                int existe = cn.QueryFirstOrDefault<int>(qExiste, new { dept = seccion.cod_departamento.ToUpper(), sec = seccion.cod_seccion.ToUpper() });

                if (seccion.isNew)
                {
                    if (existe > 0)
                        return new ErrorDto { Code = -2, Description = $"La sección {seccion.cod_seccion.ToUpper()} ya existe para el departamento {seccion.cod_departamento.ToUpper()}." };

                    resp = Activos_Secciones_Insertar(CodEmpresa, usuario, seccion);
                }
                else
                {
                    if (existe == 0)
                        return new ErrorDto { Code = -2, Description = $"La sección {seccion.cod_seccion.ToUpper()} no existe en el departamento {seccion.cod_departamento.ToUpper()}." };

                    resp = Activos_Secciones_Actualizar(CodEmpresa, usuario, seccion);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Inserta una nueva sección.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="seccion"></param>
        /// </summary>
        /// <returns></returns>
        private ErrorDto Activos_Secciones_Insertar(int CodEmpresa, string usuario, ActivosSeccionesData seccion)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);
                string query = @"
                    INSERT INTO dbo.ACTIVOS_SECCIONES
                        (COD_DEPARTAMENTO, COD_SECCION, DESCRIPCION, COD_CENTRO_COSTO, REGISTRO_USUARIO, REGISTRO_FECHA, MODIFICA_USUARIO, MODIFICA_FECHA)
                    VALUES
                        (@dept, @sec, @desc, @cc, @usr, SYSDATETIME(), NULL, NULL);";

                cn.Execute(query, new
                {
                    dept = seccion.cod_departamento.ToUpper(),
                    sec = seccion.cod_seccion.ToUpper(),
                    desc = seccion.descripcion?.ToUpper(),
                    cc = string.IsNullOrWhiteSpace(seccion.cod_centro_costo) ? null : seccion.cod_centro_costo.ToUpper(),
                    usr = string.IsNullOrWhiteSpace(usuario) ? null : usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? "",
                    DetalleMovimiento = $"Sección: {seccion.cod_seccion} - Departamento: {seccion.cod_departamento} - {seccion.descripcion}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                resp.Description = "Sección ingresada satisfactoriamente.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Actualiza una sección existente.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="seccion"></param>
        /// </summary>
        /// <returns></returns>
        private ErrorDto Activos_Secciones_Actualizar(int CodEmpresa, string usuario, ActivosSeccionesData seccion)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);
                string query = @"
                    UPDATE dbo.ACTIVOS_SECCIONES
                       SET DESCRIPCION      = @desc,
                           COD_CENTRO_COSTO = @cc,
                           MODIFICA_USUARIO = @usr,
                           MODIFICA_FECHA   = SYSDATETIME()
                     WHERE COD_DEPARTAMENTO = @dept
                       AND COD_SECCION      = @sec;";

                cn.Execute(query, new
                {
                    dept = seccion.cod_departamento.ToUpper(),
                    sec = seccion.cod_seccion.ToUpper(),
                    desc = seccion.descripcion?.ToUpper(),
                    cc = string.IsNullOrWhiteSpace(seccion.cod_centro_costo) ? null : seccion.cod_centro_costo.ToUpper(),
                    usr = string.IsNullOrWhiteSpace(usuario) ? null : usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? "",
                    DetalleMovimiento = $"Sección: {seccion.cod_seccion} - Departamento: {seccion.cod_departamento} - {seccion.descripcion}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });

                resp.Description = "Sección actualizada satisfactoriamente.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Eliminar sección por su código y departamento.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_departamento"></param>
        /// <param name="cod_seccion"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto Activos_Secciones_Eliminar(int CodEmpresa, string usuario, string cod_departamento, string cod_seccion)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                if (string.IsNullOrWhiteSpace(cod_departamento) || string.IsNullOrWhiteSpace(cod_seccion))
                    return new ErrorDto { Code = -1, Description = "Debe indicar departamento y sección." };

                using var cn = _portalDB.CreateConnection(CodEmpresa);
                string query = @"
                    DELETE FROM dbo.ACTIVOS_SECCIONES
                    WHERE COD_DEPARTAMENTO = @dept AND COD_SECCION = @sec;";
                int rows = cn.Execute(query, new { dept = cod_departamento.ToUpper(), sec = cod_seccion.ToUpper() });

                if (rows == 0)
                    return new ErrorDto { Code = -2, Description = $"La sección {cod_seccion.ToUpper()} no existe para el departamento {cod_departamento.ToUpper()}." };

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario ?? "",
                    DetalleMovimiento = $"Sección: {cod_seccion} - Dept: {cod_departamento}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Valida si un código de sección ya existe en la base de datos ACTIVOS_SECCIONES.
        /// </summary>
        /// <param name="CodEmpresa">.</param>
        /// <param name="cod_departamento"></param>
        /// <param name="cod_seccion"></param>
        /// <returns></returns>
        public ErrorDto Activos_Secciones_Valida(int CodEmpresa, string cod_departamento, string cod_seccion)
        {
            var resp = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    SELECT COUNT(1)
                    FROM dbo.ACTIVOS_SECCIONES
                    WHERE UPPER(COD_DEPARTAMENTO) = @dept
                      AND UPPER(COD_SECCION)      = @sec;";
                int existe = cn.QueryFirstOrDefault<int>(query, new { dept = (cod_departamento ?? "").ToUpper(), sec = (cod_seccion ?? "").ToUpper() });

                if (existe > 0)
                {
                    resp.Code = -1;
                    resp.Description = "La sección ya existe para este departamento.";
                }
                else
                {
                    resp.Code = 0;
                    resp.Description = "La sección es válida.";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene una lista de centros de costo activos por contabilidad.
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Secciones_CentrosCostos_Obtener(int CodEmpresa, int contabilidad)
        {
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);
                string query = @"
                    SELECT COD_CENTRO_COSTO AS item, DESCRIPCION
                    FROM dbo.CNTX_CENTRO_COSTOS
                    WHERE COD_CONTABILIDAD = @contabilidad AND ACTIVO = 1
                    ORDER BY DESCRIPCION ASC;";
                resp.Result = cn.Query<DropDownListaGenericaModel>(query, new { contabilidad }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }
        /// <summary>
        /// Lista de Unidades Contables activas por contabilidad.
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Departamentos_Unidades_Obtener(int CodEmpresa, int contabilidad)
        {
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);
                string query = @"
            SELECT COD_UNIDAD AS item, DESCRIPCION
            FROM dbo.CNTX_UNIDADES
            WHERE COD_CONTABILIDAD = @contabilidad AND ACTIVA = 1
            ORDER BY DESCRIPCION ASC;";
                resp.Result = cn.Query<DropDownListaGenericaModel>(query, new { contabilidad }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }
            return resp;
        }
        /// <summary>
        /// Lista los departamentos.
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Departamentos_Dropdown_Obtener(int CodEmpresa)
        {
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Description = "Ok", Result = new() };
            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
            SELECT d.COD_DEPARTAMENTO AS item, d.DESCRIPCION
            FROM dbo.ACTIVOS_DEPARTAMENTOS d
            ORDER BY d.DESCRIPCION ASC;";
                resp.Result = cn.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1; resp.Description = ex.Message; resp.Result = null;
            }
            return resp;
        }

    }
}
