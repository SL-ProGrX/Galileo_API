using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Pasivos;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Pasivos
{
    public partial class FrmCrApaAcreedoresDB
    {
        #region Helpers

        /// <summary>
        /// Crea los parámetros lazy comunes para listas por acreedor.
        /// </summary>
        private static DynamicParameters CR_APA_CrearParametrosLazyPorAcreedor(
            string codAcreedor,
            FiltrosLazyLoadData filtros,
            string defaultSortField,
            Func<string, int> resolveSortCode)
        {
            filtros ??= new FiltrosLazyLoadData();

            var hasFilter = !string.IsNullOrWhiteSpace(filtros.filtro);
            var filtroLike = hasFilter ? $"%{filtros.filtro.Trim()}%" : null;

            var offset = filtros.pagina < 0 ? 0 : filtros.pagina;
            var pageSize = filtros.paginacion <= 0 ? 30 : filtros.paginacion;

            var sortField = (filtros.sortField ?? defaultSortField).Trim().ToLowerInvariant();
            var sortOrder = filtros.sortOrder;
            var sortCode = resolveSortCode(sortField);
            var isAsc = sortOrder != -1 && sortOrder != 2;

            var parameters = new DynamicParameters();
            parameters.Add("@cod_acreedor", codAcreedor, DbType.String);
            parameters.Add("@hasFilter", hasFilter ? 1 : 0, DbType.Int32);
            parameters.Add("@filtro", filtroLike, DbType.String);
            parameters.Add("@offset", offset, DbType.Int32);
            parameters.Add("@pageSize", pageSize, DbType.Int32);
            parameters.Add("@sortCode", sortCode, DbType.Int32);
            parameters.Add("@isAsc", isAsc ? 1 : 0, DbType.Int32);

            return parameters;
        }

        /// <summary>
        /// Ejecuta una lista lazy común por acreedor.
        /// </summary>
        private ErrorDto<TResult> CR_APA_ObtenerListaPorAcreedor<TResult, TItem>(
          CrApaAcreedoresListaParameters param,
         Func<TResult> createResult ,
         Action<TResult, int> setTotal,
         Action<TResult, List<TItem>> setLista
           )
        {
            using var conn = DbHelper.OpenConnection(_portalDb, param.codEmpresa);

            var result = createResult();

            try
            {
                var codAcreedor = (param.cod_acreedor ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(codAcreedor))
                {
                    return DbHelper.CreateErrorResponse<TResult>(CrApaAcreedoresVariables.vCodAcreedor);
                }

                var parameters = CR_APA_CrearParametrosLazyPorAcreedor(
                    codAcreedor,
                    param.filtros,
                    param.defaultSortField,
                    param.resolveSortCode);

                var total = conn.ExecuteScalar<int>(param.sqlCount, parameters);
                var lista = conn.Query<TItem>(param.sqlData, parameters).ToList();

                setTotal(result, total);
                setLista(result, lista);

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TResult>(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta un guardado simple nuevo/edición para tablas hijas de acreedor.
        /// </summary>
        private ErrorDto<int> CR_APA_GuardarRegistroSimple(
            CrApaAcreedorDatosParameters param)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, param.codEmpresa);

            try
            {
                if (string.IsNullOrWhiteSpace(param.codAcreedor))
                {
                    return DbHelper.CreateErrorResponse<int>(CrApaAcreedoresVariables.vCodAcreedor);
                }

                if (string.IsNullOrWhiteSpace(param.identificador))
                {
                    return DbHelper.CreateErrorResponse<int>(param.mensajeIdentificadorRequerido);
                }

                if (string.IsNullOrWhiteSpace(param.nombre))
                {
                    return DbHelper.CreateErrorResponse<int>(param.mensajeNombreRequerido);
                }

                if (param.isNew)
                {
                    var existe = conn.ExecuteScalar<int>(param.sqlExiste, param.parametrosExiste);

                    if (existe > 0)
                    {
                        return DbHelper.CreateErrorResponse<int>(param.mensajeDuplicado);
                    }

                    conn.Execute(param.sqlInsert, param.parametros);
                }
                else
                {
                    var rows = conn.Execute(param.sqlUpdate, param.parametros);

                    if (rows == 0)
                    {
                        return DbHelper.CreateErrorResponse<int>(param.mensajeNoEncontrado);
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
        /// Ejecuta un borrado simple por acreedor e identificador.
        /// </summary>
        private ErrorDto<int> CR_APA_EliminarRegistroSimple(
            int codEmpresa,
            string cod_acreedor,
            string identificador,
            string mensajeIdentificadorRequerido,
            string mensajeNoEncontrado,
            string sqlDelete,
            object parametrosDelete)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var codAcreedor = (cod_acreedor ?? string.Empty).Trim();
                var id = (identificador ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(codAcreedor) || string.IsNullOrWhiteSpace(id))
                {
                    return DbHelper.CreateErrorResponse<int>(mensajeIdentificadorRequerido);
                }

                var rows = conn.Execute(sqlDelete, parametrosDelete);

                if (rows == 0)
                {
                    return DbHelper.CreateErrorResponse<int>(mensajeNoEncontrado);
                }

                return DbHelper.CreateOkResponse(1);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }

        #endregion

        #region Contactos

        /// <summary>
        /// Obtiene la lista lazy de contactos de un acreedor APA.
        /// </summary>
        public ErrorDto<FrmCrApaContactosListaDto> CR_APA_Contactos_Obtener(
            int codEmpresa,
            string cod_acreedor,
            FiltrosLazyLoadData filtros)
        {
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

            return CR_APA_ObtenerListaPorAcreedor<FrmCrApaContactosListaDto, FrmCrApaContactoGridDto>(
                new CrApaAcreedoresListaParameters
                {
                    codEmpresa = codEmpresa,
                    cod_acreedor = cod_acreedor,
                    filtros = filtros,
                    defaultSortField = "codigo",
                    resolveSortCode = sortField => sortField switch
                    {
                        "nombre" => 2,
                        "tel_celular" => 3,
                        "tel_trabajo" => 4,
                        "fax" => 5,
                        "email" => 6,
                        _ => 1
                    },
                    sqlCount = sqlCount,
                    sqlData = sqlData
                },

                () => new FrmCrApaContactosListaDto(),
                (result, total) => result.total = total,
                (result, lista) => result.lista = lista
                );
        }

        /// <summary>
        /// Guarda un contacto de acreedor APA.
        /// </summary>
        public ErrorDto<int> CR_APA_Contacto_Guardar(
            int codEmpresa,
            FrmCrApaContactoGuardarRequest request)
        {
            var codAcreedor = (request.cod_acreedor ?? string.Empty).Trim();
            var codigo = (request.codigo ?? string.Empty).Trim();
            var nombre = (request.nombre ?? string.Empty).Trim();

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

            const string sqlExiste = @"
SELECT ISNULL(COUNT(*), 0)
FROM CRD_APA_CONTACTOS
WHERE COD_ACREEDOR = @cod_acreedor
  AND COD_CONTACTO = @codigo";

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

            return CR_APA_GuardarRegistroSimple(
                new CrApaAcreedorDatosParameters
                {
                    codEmpresa = codEmpresa,
                    codAcreedor = codAcreedor,
                    identificador = codigo,
                    nombre = nombre,
                    isNew = request.isNew,
                    parametros = parametros,
                    mensajeIdentificadorRequerido = "El código del contacto es requerido.",
                    mensajeNombreRequerido = "El nombre del contacto es requerido.",
                    mensajeDuplicado = "Ya existe un contacto con ese código para este acreedor.",
                    mensajeNoEncontrado = "No se encontró el contacto.",
                    sqlExiste = sqlExiste,
                    sqlInsert = sqlInsert,
                    sqlUpdate = sqlUpdate,
                    parametrosExiste = new { cod_acreedor = codAcreedor, codigo }
                });
        }

        /// <summary>
        /// Elimina un contacto de acreedor APA.
        /// </summary>
        public ErrorDto<int> CR_APA_Contacto_Eliminar(
            int codEmpresa,
            string cod_acreedor,
            string codigo)
        {
            const string sqlDelete = @"
DELETE FROM CRD_APA_CONTACTOS
WHERE COD_ACREEDOR = @cod_acreedor
  AND COD_CONTACTO = @codigo";

            return CR_APA_EliminarRegistroSimple(
                codEmpresa,
                cod_acreedor,
                codigo,
                "El acreedor y el código del contacto son requeridos.",
                "No se encontró el contacto.",
                sqlDelete,
                new
                {
                    cod_acreedor = (cod_acreedor ?? string.Empty).Trim(),
                    codigo = (codigo ?? string.Empty).Trim()
                });
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
            const string sqlCount = @"
SELECT COUNT(1)
FROM CRD_APA_AUTORIZADOSCK
WHERE COD_ACREEDOR = @cod_acreedor
  AND (
      @hasFilter = 0
      OR CEDULA LIKE @filtro
      OR NOMBRE LIKE @filtro
  );";

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

            return CR_APA_ObtenerListaPorAcreedor<FrmCrApaAutorizadosListaDto, FrmCrApaAutorizadoGridDto>(
                new CrApaAcreedoresListaParameters
                {
                    codEmpresa = codEmpresa,
                    cod_acreedor = cod_acreedor,
                    filtros = filtros,
                    defaultSortField = "cedula",
                    resolveSortCode = sortField => sortField switch
                    {
                        "nombre" => 2,
                        _ => 1
                    },
                    sqlCount = sqlCount,
                    sqlData = sqlData
                },
                () => new FrmCrApaAutorizadosListaDto(),
                (result, total) => result.total = total,
                (result, lista) => result.lista = lista);
        }

        /// <summary>
        /// Guarda un autorizado de acreedor APA.
        /// </summary>
        public ErrorDto<int> CR_APA_Autorizado_Guardar(
            int codEmpresa,
            FrmCrApaAutorizadoGuardarRequest request)
        {
            var codAcreedor = (request.cod_acreedor ?? string.Empty).Trim();
            var cedula = (request.cedula ?? string.Empty).Trim();
            var nombre = (request.nombre ?? string.Empty).Trim();

            var parametros = new
            {
                cod_acreedor = codAcreedor,
                cedula,
                nombre,
            };

            const string sqlExiste = @"
SELECT ISNULL(COUNT(*), 0)
FROM CRD_APA_AUTORIZADOSCK
WHERE COD_ACREEDOR = @cod_acreedor
  AND CEDULA = @cedula";

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

            const string sqlUpdate = @"
UPDATE CRD_APA_AUTORIZADOSCK
SET
    NOMBRE = @nombre
WHERE COD_ACREEDOR = @cod_acreedor
  AND CEDULA = @cedula;";

            return CR_APA_GuardarRegistroSimple(
                 new CrApaAcreedorDatosParameters
                 {
                     codEmpresa = codEmpresa,
                     codAcreedor = codAcreedor,
                     identificador = cedula,
                     nombre = nombre,
                     isNew = request.isNew,
                     parametros = parametros,
                     mensajeIdentificadorRequerido = "La cédula del autorizado es requerida.",
                     mensajeNombreRequerido = "El nombre del autorizado es requerido.",
                     mensajeDuplicado = "Ya existe un autorizado con esa cédula para este acreedor.",
                     mensajeNoEncontrado = "No se encontró el autorizado.",
                     sqlExiste = sqlExiste,
                     sqlInsert = sqlInsert,
                     sqlUpdate = sqlUpdate,
                     parametrosExiste = new { cod_acreedor = codAcreedor, cedula }
                 });
        }

        /// <summary>
        /// Elimina un autorizado de acreedor APA.
        /// </summary>
        public ErrorDto<int> CR_APA_Autorizado_Eliminar(
            int codEmpresa,
            string cod_acreedor,
            string cedula)
        {
            const string sqlDelete = @"
DELETE FROM CRD_APA_AUTORIZADOSCK
WHERE COD_ACREEDOR = @cod_acreedor
  AND CEDULA = @cedula";

            return CR_APA_EliminarRegistroSimple(
                codEmpresa,
                cod_acreedor,
                cedula,
                "El acreedor y la cédula del autorizado son requeridos.",
                "No se encontró el autorizado.",
                sqlDelete,
                new
                {
                    cod_acreedor = (cod_acreedor ?? string.Empty).Trim(),
                    cedula = (cedula ?? string.Empty).Trim()
                });
        }

        #endregion
    }
}
