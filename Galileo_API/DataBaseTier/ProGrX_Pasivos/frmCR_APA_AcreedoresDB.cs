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
    }
}
