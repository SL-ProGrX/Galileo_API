using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Pasivos;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Pasivos
{
    public class FrmCrApaAcreedoresDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrApaAcreedoresDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la grilla de acreedores de APA con paginación lazy, filtro y orden seguro.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaAcreedoresGridLista> CR_APA_Acreedores_Obtener(
            int codEmpresa,
            FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var Result = new FrmCrApaAcreedoresGridLista();

            try
            {
                filtros ??= new FiltrosLazyLoadData();

                var hasFilter = !string.IsNullOrWhiteSpace(filtros.filtro);
                var filtroLike = hasFilter ? $"%{filtros.filtro!.Trim()}%" : null;

                var offset = filtros.pagina < 0 ? 0 : filtros.pagina;
                var pageSize = filtros.paginacion <= 0 ? 30 : filtros.paginacion;

                var sortField = (filtros.sortField ?? "cod_acreedor").Trim().ToLowerInvariant();
                var sortOrder = filtros.sortOrder;

                var sortCode = sortField switch
                {
                    "descripcion" => 2,
                    "estado" => 3,
                    _ => 1
                };

                var isAsc = sortOrder != -1 && sortOrder != 2;

                var parameters = new DynamicParameters();
                parameters.Add("@hasFilter", hasFilter ? 1 : 0, DbType.Int32);
                parameters.Add("@filtro", filtroLike, DbType.String);
                parameters.Add("@offset", offset, DbType.Int32);
                parameters.Add("@pageSize", pageSize, DbType.Int32);
                parameters.Add("@sortCode", sortCode, DbType.Int32);
                parameters.Add("@isAsc", isAsc ? 1 : 0, DbType.Int32);

                const string sqlCount = @"
SELECT COUNT(1)
FROM CRD_APA_ACREEDORES
WHERE @hasFilter = 0
   OR COD_ACREEDOR LIKE @filtro
   OR DESCRIPCION LIKE @filtro
   OR CASE ESTADO
         WHEN 'A' THEN 'Activo'
         WHEN 'I' THEN 'Inactivo'
         ELSE ISNULL(ESTADO, '')
      END LIKE @filtro;";

                Result.total = conn.ExecuteScalar<int>(sqlCount, parameters);

                const string sqlData = @"
WITH base AS
(
    SELECT
        RTRIM(COD_ACREEDOR) AS cod_acreedor,
        RTRIM(DESCRIPCION) AS descripcion,
        CASE ESTADO
            WHEN 'A' THEN 'Activo'
            WHEN 'I' THEN 'Inactivo'
            ELSE ISNULL(ESTADO, '')
        END AS estado
    FROM CRD_APA_ACREEDORES
    WHERE @hasFilter = 0
       OR COD_ACREEDOR LIKE @filtro
       OR DESCRIPCION LIKE @filtro
       OR CASE ESTADO
             WHEN 'A' THEN 'Activo'
             WHEN 'I' THEN 'Inactivo'
             ELSE ISNULL(ESTADO, '')
          END LIKE @filtro
)
SELECT cod_acreedor, descripcion, estado
FROM base t
ORDER BY
    CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN t.cod_acreedor END ASC,
    CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN t.cod_acreedor END DESC,
    CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN t.descripcion END ASC,
    CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN t.descripcion END DESC,
    CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN t.estado END ASC,
    CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN t.estado END DESC
OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";

                Result.lista = conn.Query<FrmCrApaAcreedoresGridItem>(
                    sqlData,
                    parameters).ToList();

                return DbHelper.CreateOkResponse<FrmCrApaAcreedoresGridLista>(Result);
            }
            catch (Exception ex)
            {
               return DbHelper.CreateErrorResponse<FrmCrApaAcreedoresGridLista>(ex.Message);
            }

          
        }


        /// <summary>
        /// Obtiene los datos principales de un acreedor APA por código.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaAcreedorDatosDto> CR_APA_Acreedor_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var codAcreedor = (cod_acreedor ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(codAcreedor))
                {
                    return DbHelper.CreateErrorResponse<FrmCrApaAcreedorDatosDto>(
                        CrApaAcreedoresVariables.vCodAcreedor);
                }



                const string sql = @"
                    SELECT
                        RTRIM(A.COD_ACREEDOR) AS cod_acreedor,
                        RTRIM(ISNULL(A.DESCRIPCION, '')) AS descripcion,
                        RTRIM(ISNULL(A.ESTADO, '')) AS estado,
                        RTRIM(ISNULL(A.TELEFONO1, '')) AS telefono1,
                        RTRIM(ISNULL(A.TELEFONO2, '')) AS telefono2,
                        RTRIM(ISNULL(A.DIRECCION, '')) AS direccion,
                        RTRIM(ISNULL(A.WEBSITE, '')) AS website,
                        RTRIM(ISNULL(A.COD_CUENTA, '')) AS cod_cuenta,
                        RTRIM(ISNULL(CtaAcreedor.DESCRIPCION, '')) AS cod_cuenta_desc,

                        RTRIM(ISNULL(A.COD_CUENTA_TRANSITORIA, '')) AS cod_cuenta_transitoria,
                        RTRIM(ISNULL(CtaTransitoria.DESCRIPCION, '')) AS cod_cuenta_transitoria_desc,

                        RTRIM(ISNULL(A.COD_CUENTA_GASTOS, '')) AS cod_cuenta_gastos,
                        RTRIM(ISNULL(CtaGastos.DESCRIPCION, '')) AS cod_cuenta_gastos_desc,

                        RTRIM(ISNULL(A.COD_CUENTA_CARGOS, '')) AS cod_cuenta_cargos,
                        RTRIM(ISNULL(CtaCargos.DESCRIPCION, '')) AS cod_cuenta_cargos_desc,

                        RTRIM(ISNULL(A.COD_CUENTA_COMISION, '')) AS cod_cuenta_comision,
                        RTRIM(ISNULL(CtaComision.DESCRIPCION, '')) AS cod_cuenta_comision_desc,

                        A.BANCO_CK AS banco_ck,
                        RTRIM(ISNULL(Bck.DESCRIPCION, '')) AS banco_ck_desc,

                        A.BANCO_DC AS banco_dc,
                        RTRIM(ISNULL(Bdc.DESCRIPCION, '')) AS banco_dc_desc
                    FROM CRD_APA_ACREEDORES A
                    LEFT JOIN vCNTX_CUENTAS_LOCAL CtaAcreedor
                        ON A.COD_CUENTA = CtaAcreedor.Cod_Cuenta
                    LEFT JOIN vCNTX_CUENTAS_LOCAL CtaTransitoria
                        ON A.COD_CUENTA_TRANSITORIA = CtaTransitoria.Cod_Cuenta
                    LEFT JOIN vCNTX_CUENTAS_LOCAL CtaGastos
                        ON A.COD_CUENTA_GASTOS = CtaGastos.Cod_Cuenta
                    LEFT JOIN vCNTX_CUENTAS_LOCAL CtaCargos
                        ON A.COD_CUENTA_CARGOS = CtaCargos.Cod_Cuenta
                    LEFT JOIN vCNTX_CUENTAS_LOCAL CtaComision
                        ON A.COD_CUENTA_COMISION = CtaComision.Cod_Cuenta
                    LEFT JOIN BANCOS Bck
                        ON A.BANCO_CK = Bck.ID_BANCO
                    LEFT JOIN BANCOS Bdc
                        ON A.BANCO_DC = Bdc.ID_BANCO
                    WHERE A.COD_ACREEDOR = @cod_acreedor;";

                var result = conn.QueryFirstOrDefault<FrmCrApaAcreedorDatosDto>(
                    sql,
                    new { cod_acreedor = codAcreedor });

                if (result == null)
                {
                    return DbHelper.CreateErrorResponse<FrmCrApaAcreedorDatosDto>(
                        "No se encontró el acreedor.");
                }

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaAcreedorDatosDto>(ex.Message);
            }
        }

        /// <summary>
        /// Guarda un acreedor APA nuevo.
        /// </summary>
        public ErrorDto<int> CR_APA_Acreedor_Insertar(
            int codEmpresa,
            FrmCrApaAcreedorGuardarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var existe = conn.ExecuteScalar<int>(
                    @"SELECT ISNULL(COUNT(*), 0)
              FROM CRD_APA_ACREEDORES
              WHERE COD_ACREEDOR = @cod_acreedor",
                    new { cod_acreedor = request.cod_acreedor });

                if (existe > 0)
                {
                    return DbHelper.CreateErrorResponse<int>(
                        "Ya existe un acreedor con ese código.");
                }

                const string sql = @"
INSERT INTO CRD_APA_ACREEDORES
(
    COD_ACREEDOR,
    DESCRIPCION,
    ESTADO,
    TELEFONO1,
    TELEFONO2,
    DIRECCION,
    WEBSITE,
    COD_CUENTA,
    COD_CUENTA_TRANSITORIA,
    COD_CUENTA_GASTOS,
    COD_CUENTA_CARGOS,
    COD_CUENTA_COMISION,
    BANCO_CK,
    BANCO_DC
)
VALUES
(
    @cod_acreedor,
    @descripcion,
    @estado,
    @telefono1,
    @telefono2,
    @direccion,
    @website,
    @cod_cuenta,
    @cod_cuenta_transitoria,
    @cod_cuenta_gastos,
    @cod_cuenta_cargos,
    @cod_cuenta_comision,
    @banco_ck,
    @banco_dc
);";

                var parametros = CR_APA_Acreedor_CrearParametrosGuardar(request);

                conn.Execute(sql, parametros);

                return DbHelper.CreateOkResponse(1);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un acreedor APA existente.
        /// </summary>
        public ErrorDto<int> CR_APA_Acreedor_Actualizar(
            int codEmpresa,
            FrmCrApaAcreedorGuardarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string sql = @"
UPDATE CRD_APA_ACREEDORES
SET
    DESCRIPCION = @descripcion,
    ESTADO = @estado,
    TELEFONO1 = @telefono1,
    TELEFONO2 = @telefono2,
    DIRECCION = @direccion,
    WEBSITE = @website,
    COD_CUENTA = @cod_cuenta,
    COD_CUENTA_TRANSITORIA = @cod_cuenta_transitoria,
    COD_CUENTA_GASTOS = @cod_cuenta_gastos,
    COD_CUENTA_CARGOS = @cod_cuenta_cargos,
    COD_CUENTA_COMISION = @cod_cuenta_comision,
    BANCO_CK = @banco_ck,
    BANCO_DC = @banco_dc
WHERE COD_ACREEDOR = @cod_acreedor;";

                var parametros = CR_APA_Acreedor_CrearParametrosGuardar(request);
                var rows = conn.Execute(sql, parametros);

                if (rows == 0)
                {
                    return DbHelper.CreateErrorResponse<int>("No se encontró el acreedor.");
                }

                return DbHelper.CreateOkResponse(1);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de bancos para búsquedas del mantenimiento de acreedores APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmCrApaBancoDto>> CR_APA_Bancos_Obtener(int codEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string sql = @"
SELECT
    ID_BANCO AS item,
    RTRIM(ISNULL(DESCRIPCION, '')) AS descripcion
FROM BANCOS
ORDER BY ID_BANCO;";

                var result = conn.Query<FrmCrApaBancoDto>(sql).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<FrmCrApaBancoDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene un banco por código para el mantenimiento de acreedores APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaBancoDto> CR_APA_Banco_Obtener(
            int codEmpresa,
            int id_banco)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                if (id_banco <= 0)
                {
                    return DbHelper.CreateErrorResponse<FrmCrApaBancoDto>(
                        "El código del banco es requerido.");
                }

                const string sql = @"
SELECT
    ID_BANCO AS item,
    RTRIM(ISNULL(DESCRIPCION, '')) AS descripcion
FROM BANCOS
WHERE ID_BANCO = @id_banco;";

                var result = conn.QueryFirstOrDefault<FrmCrApaBancoDto>(
                    sql,
                    new { id_banco });

                if (result == null)
                {
                    return DbHelper.CreateErrorResponse<FrmCrApaBancoDto>(
                        "No se encontró el banco.");
                }

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaBancoDto>(ex.Message);
            }
        }

        /// <summary>
        /// Construye los parámetros normalizados para insertar o actualizar acreedores APA.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private object CR_APA_Acreedor_CrearParametrosGuardar(
            FrmCrApaAcreedorGuardarRequest request)
        {
            return new
            {
                cod_acreedor = (request.cod_acreedor ?? string.Empty).Trim(),
                descripcion = (request.descripcion ?? string.Empty).Trim(),
                estado = (request.estado ?? string.Empty).Trim(),
                telefono1 = (request.telefono1 ?? string.Empty).Trim(),
                telefono2 = (request.telefono2 ?? string.Empty).Trim(),
                direccion = (request.direccion ?? string.Empty).Trim(),
                website = (request.website ?? string.Empty).Trim(),
                cod_cuenta = (request.cod_cuenta ?? string.Empty).Trim(),
                cod_cuenta_transitoria = (request.cod_cuenta_transitoria ?? string.Empty).Trim(),
                cod_cuenta_gastos = (request.cod_cuenta_gastos ?? string.Empty).Trim(),
                cod_cuenta_cargos = (request.cod_cuenta_cargos ?? string.Empty).Trim(),
                cod_cuenta_comision = (request.cod_cuenta_comision ?? string.Empty).Trim(),
                banco_ck = request.banco_ck,
                banco_dc = request.banco_dc
            };
        }


        #region contactos

        /// <summary>
        /// Obtiene la lista lazy de contactos de un acreedor APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaContactosListaDto> CR_APA_Contactos_Obtener(
            int codEmpresa,
            string cod_acreedor,
            FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var result = new FrmCrApaContactosListaDto();

            try
            {
                var codAcreedor = (cod_acreedor ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(codAcreedor))
                {
                    return DbHelper.CreateErrorResponse<FrmCrApaContactosListaDto>(
                        CrApaAcreedoresVariables.vCodAcreedor);
                }

                filtros ??= new FiltrosLazyLoadData();

                var hasFilter = !string.IsNullOrWhiteSpace(filtros.filtro);
                var filtroLike = hasFilter ? $"%{filtros.filtro!.Trim()}%" : null;

                var offset = filtros.pagina < 0 ? 0 : filtros.pagina;
                var pageSize = filtros.paginacion <= 0 ? 30 : filtros.paginacion;

                var sortField = (filtros.sortField ?? "codigo").Trim().ToLowerInvariant();
                var sortOrder = filtros.sortOrder;

                var sortCode = sortField switch
                {
                    "nombre" => 2,
                    "tel_celular" => 3,
                    "tel_trabajo" => 4,
                    "fax" => 5,
                    "email" => 6,
                    _ => 1
                };

                var isAsc = sortOrder != -1 && sortOrder != 2;

                var parameters = new DynamicParameters();
                parameters.Add("@cod_acreedor", codAcreedor, DbType.String);
                parameters.Add("@hasFilter", hasFilter ? 1 : 0, DbType.Int32);
                parameters.Add("@filtro", filtroLike, DbType.String);
                parameters.Add("@offset", offset, DbType.Int32);
                parameters.Add("@pageSize", pageSize, DbType.Int32);
                parameters.Add("@sortCode", sortCode, DbType.Int32);
                parameters.Add("@isAsc", isAsc ? 1 : 0, DbType.Int32);

                const string sqlCount = @"
SELECT COUNT(1)
FROM CRD_APA_CONTACTOS
WHERE COD_ACREEDOR = @cod_acreedor
  AND (
      @hasFilter = 0
      OR COD_CONTACTO LIKE @filtro
      OR NOMBRE LIKE @filtro
      OR TEL_CEL LIKE @filtro
      OR TEL_TRABAJO LIKE @filtro
      OR TEL_FAX LIKE @filtro
      OR EMAIL LIKE @filtro
  );";

                result.total = conn.ExecuteScalar<int>(sqlCount, parameters);

                const string sqlData = @"
WITH base AS
(
    SELECT
        RTRIM(ISNULL(COD_CONTACTO, '')) AS codigo,
        RTRIM(ISNULL(NOMBRE, '')) AS nombre,
        RTRIM(ISNULL(TEL_CEL, '')) AS tel_celular,
        RTRIM(ISNULL(TEL_TRABAJO, '')) AS tel_trabajo,
        RTRIM(ISNULL(TEL_FAX, '')) AS fax,
        RTRIM(ISNULL(EMAIL, '')) AS email
    FROM CRD_APA_CONTACTOS
    WHERE COD_ACREEDOR = @cod_acreedor
      AND (
          @hasFilter = 0
          OR COD_CONTACTO LIKE @filtro
          OR NOMBRE LIKE @filtro
          OR TEL_CEL LIKE @filtro
          OR TEL_TRABAJO LIKE @filtro
          OR TEL_FAX LIKE @filtro
          OR EMAIL LIKE @filtro
      )
)
SELECT codigo, nombre, tel_celular, tel_trabajo, fax, email
FROM base t
ORDER BY
    CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN t.codigo END ASC,
    CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN t.codigo END DESC,
    CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN t.nombre END ASC,
    CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN t.nombre END DESC,
    CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN t.tel_celular END ASC,
    CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN t.tel_celular END DESC,
    CASE WHEN @sortCode = 4 AND @isAsc = 1 THEN t.tel_trabajo END ASC,
    CASE WHEN @sortCode = 4 AND @isAsc = 0 THEN t.tel_trabajo END DESC,
    CASE WHEN @sortCode = 5 AND @isAsc = 1 THEN t.fax END ASC,
    CASE WHEN @sortCode = 5 AND @isAsc = 0 THEN t.fax END DESC,
    CASE WHEN @sortCode = 6 AND @isAsc = 1 THEN t.email END ASC,
    CASE WHEN @sortCode = 6 AND @isAsc = 0 THEN t.email END DESC
OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";

                result.lista = conn.Query<FrmCrApaContactoGridDto>(sqlData, parameters).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaContactosListaDto>(ex.Message);
            }
        }

        /// <summary>
        /// Guarda un contacto de acreedor APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int> CR_APA_Contacto_Guardar(
            int codEmpresa,
            FrmCrApaContactoGuardarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var codAcreedor = (request.cod_acreedor ?? string.Empty).Trim();
                var codigo = (request.codigo ?? string.Empty).Trim();
                var nombre = (request.nombre ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(codAcreedor))
                {
                    return DbHelper.CreateErrorResponse<int>(CrApaAcreedoresVariables.vCodAcreedor);
                }

                if (string.IsNullOrWhiteSpace(codigo))
                {
                    return DbHelper.CreateErrorResponse<int>("El código del contacto es requerido.");
                }

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return DbHelper.CreateErrorResponse<int>("El nombre del contacto es requerido.");
                }

                var parametros = new
                {
                    cod_acreedor = codAcreedor,
                    codigo,
                    nombre,
                    tel_celular = (request.tel_celular ?? string.Empty).Trim(),
                    tel_trabajo = (request.tel_trabajo ?? string.Empty).Trim(),
                    fax = (request.fax ?? string.Empty).Trim(),
                    email = (request.email ?? string.Empty).Trim(),
                };

                if (request.isNew)
                {
                    var existe = conn.ExecuteScalar<int>(
                        @"SELECT ISNULL(COUNT(*), 0)
                  FROM CRD_APA_CONTACTOS
                  WHERE COD_ACREEDOR = @cod_acreedor
                    AND COD_CONTACTO = @codigo",
                        new { cod_acreedor = codAcreedor, codigo });

                    if (existe > 0)
                    {
                        return DbHelper.CreateErrorResponse<int>(
                            "Ya existe un contacto con ese código para este acreedor.");
                    }

                    const string sqlInsert = @"
INSERT INTO CRD_APA_CONTACTOS
(
    COD_ACREEDOR,
    COD_CONTACTO,
    NOMBRE,
    TEL_CEL,
    TEL_TRABAJO,
    TEL_FAX,
    EMAIL
)
VALUES
(
    @cod_acreedor,
    @codigo,
    @nombre,
    @tel_celular,
    @tel_trabajo,
    @fax,
    @email
);";

                    conn.Execute(sqlInsert, parametros);
                }
                else
                {
                    const string sqlUpdate = @"
UPDATE CRD_APA_CONTACTOS
SET
    NOMBRE = @nombre,
    TEL_CEL = @tel_celular,
    TEL_TRABAJO = @tel_trabajo,
    TEL_FAX = @fax,
    EMAIL = @email
WHERE COD_ACREEDOR = @cod_acreedor
  AND COD_CONTACTO = @codigo;";

                    var rows = conn.Execute(sqlUpdate, parametros);

                    if (rows == 0)
                    {
                        return DbHelper.CreateErrorResponse<int>("No se encontró el contacto.");
                    }
                }

                return DbHelper.CreateOkResponse(1);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un contacto de acreedor APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<int> CR_APA_Contacto_Eliminar(
            int codEmpresa,
            string cod_acreedor,
            string codigo)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var codAcreedor = (cod_acreedor ?? string.Empty).Trim();
                var codContacto = (codigo ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(codAcreedor) || string.IsNullOrWhiteSpace(codContacto))
                {
                    return DbHelper.CreateErrorResponse<int>(
                        "El acreedor y el código del contacto son requeridos.");
                }

                var rows = conn.Execute(
                    @"DELETE FROM CRD_APA_CONTACTOS
              WHERE COD_ACREEDOR = @cod_acreedor
                AND COD_CONTACTO = @codigo",
                    new { cod_acreedor = codAcreedor, codigo = codContacto });

                if (rows == 0)
                {
                    return DbHelper.CreateErrorResponse<int>("No se encontró el contacto.");
                }

                return DbHelper.CreateOkResponse(1);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }

        #endregion

        #region Autorizados

        /// <summary>
        /// Obtiene la lista lazy de autorizados de un acreedor APA.
        /// </summary>
        public ErrorDto<FrmCrApaAutorizadosListaDto> CR_APA_Autorizados_Obtener(
            int codEmpresa,
            string cod_acreedor,
            FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            var result = new FrmCrApaAutorizadosListaDto();

            try
            {
                var codAcreedor = (cod_acreedor ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(codAcreedor))
                {
                    return DbHelper.CreateErrorResponse<FrmCrApaAutorizadosListaDto>(
                        CrApaAcreedoresVariables.vCodAcreedor);
                }

                filtros ??= new FiltrosLazyLoadData();

                var hasFilter = !string.IsNullOrWhiteSpace(filtros.filtro);
                var filtroLike = hasFilter ? $"%{filtros.filtro!.Trim()}%" : null;

                var offset = filtros.pagina < 0 ? 0 : filtros.pagina;
                var pageSize = filtros.paginacion <= 0 ? 30 : filtros.paginacion;

                var sortField = (filtros.sortField ?? "cedula").Trim().ToLowerInvariant();
                var sortOrder = filtros.sortOrder;

                var sortCode = sortField switch
                {
                    "nombre" => 2,
                    _ => 1
                };

                var isAsc = sortOrder != -1 && sortOrder != 2;

                var parameters = new DynamicParameters();
                parameters.Add("@cod_acreedor", codAcreedor, DbType.String);
                parameters.Add("@hasFilter", hasFilter ? 1 : 0, DbType.Int32);
                parameters.Add("@filtro", filtroLike, DbType.String);
                parameters.Add("@offset", offset, DbType.Int32);
                parameters.Add("@pageSize", pageSize, DbType.Int32);
                parameters.Add("@sortCode", sortCode, DbType.Int32);
                parameters.Add("@isAsc", isAsc ? 1 : 0, DbType.Int32);

                const string sqlCount = @"
SELECT COUNT(1)
FROM CRD_APA_AUTORIZADOSCK
WHERE COD_ACREEDOR = @cod_acreedor
  AND (
      @hasFilter = 0
      OR CEDULA LIKE @filtro
      OR NOMBRE LIKE @filtro
  );";

                result.total = conn.ExecuteScalar<int>(sqlCount, parameters);

                const string sqlData = @"
WITH base AS
(
    SELECT
        RTRIM(ISNULL(CEDULA, '')) AS cedula,
        RTRIM(ISNULL(NOMBRE, '')) AS nombre
    FROM CRD_APA_AUTORIZADOSCK
    WHERE COD_ACREEDOR = @cod_acreedor
      AND (
          @hasFilter = 0
          OR CEDULA LIKE @filtro
          OR NOMBRE LIKE @filtro
      )
)
SELECT cedula, nombre
FROM base t
ORDER BY
    CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN t.cedula END ASC,
    CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN t.cedula END DESC,
    CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN t.nombre END ASC,
    CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN t.nombre END DESC
OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";

                result.lista = conn.Query<FrmCrApaAutorizadoGridDto>(sqlData, parameters).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaAutorizadosListaDto>(ex.Message);
            }
        }

        /// <summary>
        /// Guarda un autorizado de acreedor APA.
        /// </summary>
        public ErrorDto<int> CR_APA_Autorizado_Guardar(
            int codEmpresa,
            FrmCrApaAutorizadoGuardarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var codAcreedor = (request.cod_acreedor ?? string.Empty).Trim();
                var cedula = (request.cedula ?? string.Empty).Trim();
                var nombre = (request.nombre ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(codAcreedor))
                {
                    return DbHelper.CreateErrorResponse<int>(CrApaAcreedoresVariables.vCodAcreedor);
                }

                if (string.IsNullOrWhiteSpace(cedula))
                {
                    return DbHelper.CreateErrorResponse<int>("La cédula es requerida.");
                }

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return DbHelper.CreateErrorResponse<int>("El nombre es requerido.");
                }

                var parametros = new
                {
                    cod_acreedor = codAcreedor,
                    cedula,
                    nombre,
                };

                if (request.isNew)
                {
                    var existe = conn.ExecuteScalar<int>(
                        @"SELECT ISNULL(COUNT(*), 0)
                  FROM CRD_APA_AUTORIZADOSCK
                  WHERE COD_ACREEDOR = @cod_acreedor
                    AND CEDULA = @cedula",
                        new { cod_acreedor = codAcreedor, cedula });

                    if (existe > 0)
                    {
                        return DbHelper.CreateErrorResponse<int>(
                            "Ya existe un autorizado con esa cédula para este acreedor.");
                    }

                    const string sqlInsert = @"
INSERT INTO CRD_APA_AUTORIZADOSCK
(
    COD_ACREEDOR,
    CEDULA,
    NOMBRE
)
VALUES
(
    @cod_acreedor,
    @cedula,
    @nombre
);";

                    conn.Execute(sqlInsert, parametros);
                }
                else
                {
                    const string sqlUpdate = @"
UPDATE CRD_APA_AUTORIZADOSCK
SET
    NOMBRE = @nombre
WHERE COD_ACREEDOR = @cod_acreedor
  AND CEDULA = @cedula;";

                    var rows = conn.Execute(sqlUpdate, parametros);

                    if (rows == 0)
                    {
                        return DbHelper.CreateErrorResponse<int>("No se encontró el autorizado.");
                    }
                }

                return DbHelper.CreateOkResponse(1);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un autorizado de acreedor APA.
        /// </summary>
        public ErrorDto<int> CR_APA_Autorizado_Eliminar(
            int codEmpresa,
            string cod_acreedor,
            string cedula)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var codAcreedor = (cod_acreedor ?? string.Empty).Trim();
                var id = (cedula ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(codAcreedor) || string.IsNullOrWhiteSpace(id))
                {
                    return DbHelper.CreateErrorResponse<int>(
                        "El acreedor y la cédula del autorizado son requeridos.");
                }

                var rows = conn.Execute(
                    @"DELETE FROM CRD_APA_AUTORIZADOSCK
              WHERE COD_ACREEDOR = @cod_acreedor
                AND CEDULA = @cedula",
                    new { cod_acreedor = codAcreedor, cedula = id });

                if (rows == 0)
                {
                    return DbHelper.CreateErrorResponse<int>("No se encontró el autorizado.");
                }

                return DbHelper.CreateOkResponse(1);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }

        #endregion

    }
}
