using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_ARF
{
    public class FrmArfAcreedoresDb
    {
        private readonly PortalDB _portalDb;

        public FrmArfAcreedoresDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Consulta un acreedor y sus descripciones relacionadas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codigo">Código del acreedor.</param>
        /// <returns>Resultado de la consulta del acreedor.</returns>
        public ErrorDto<ArfAcreedorDto?> Consultar(int codEmpresa, int codigo)
        {
            const string sql = """
                SELECT A.*,
                       P.descripcion AS proveedor_desc,
                       ISNULL(Cta.Cod_Cuenta_Mask, A.cod_Cuenta) AS cod_cuenta_mask,
                       ISNULL(Cta.Descripcion, '') AS cuenta_desc
                FROM ARF_ACREEDORES A
                LEFT JOIN CxP_Proveedores P
                       ON A.cod_proveedor = P.cod_proveedor
                LEFT JOIN vCNTX_CUENTAS_LOCAL Cta
                       ON A.cod_Cuenta = Cta.Cod_Cuenta
                WHERE A.COD_ACREEDOR = @codigo
                """;

            return DbHelper.ExecuteSingleQuery<ArfAcreedorDto?>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { codigo });
        }

        /// <summary>
        /// Inserta un acreedor asignando el siguiente código disponible.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="m">Datos del acreedor.</param>
        /// <returns>Código generado para el acreedor.</returns>
        public ErrorDto<int> Insertar(int codEmpresa, ArfAcreedorDto m)
        {
            const string siguienteCodigoSql =
                "SELECT ISNULL(MAX(COD_ACREEDOR), 0) + 1 FROM ARF_ACREEDORES WITH (UPDLOCK, HOLDLOCK)";
            const string insertarSql = """
                INSERT INTO ARF_ACREEDORES
                (
                    cod_acreedor, descripcion, tipo_id, identificacion,
                    telefono_01, telefono_02, activo, apto_postal,
                    email_01, email_02, website, provincia, canton,
                    distrito, direccion, contacto_nombre, cod_banco,
                    cod_cuenta, cod_proveedor, registro_fecha, registro_usuario
                )
                VALUES
                (
                    @cod_acreedor, @descripcion, @tipo_id, @identificacion,
                    @telefono_01, @telefono_02, @activo, @apto_postal,
                    @email_01, @email_02, @website, @provincia, @canton,
                    @distrito, @direccion, @contacto_nombre, @cod_banco,
                    @cod_cuenta, @cod_proveedor, GETDATE(), @usuario
                )
                """;

            return DbHelper.WithConn(_portalDb, codEmpresa, connection =>
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();
                var nuevoCodigo = connection.QuerySingle<int>(
                    siguienteCodigoSql,
                    transaction: transaction);

                m.cod_acreedor = nuevoCodigo;
                connection.Execute(insertarSql, m, transaction);
                transaction.Commit();
                return nuevoCodigo;
            });
        }

        /// <summary>
        /// Actualiza los datos de un acreedor.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="m">Datos actualizados del acreedor.</param>
        /// <returns>Cantidad de filas actualizadas.</returns>
        public ErrorDto<int> Actualizar(int codEmpresa, ArfAcreedorDto m)
        {
            const string sql = """
                UPDATE ARF_ACREEDORES
                   SET descripcion = @descripcion,
                       tipo_id = @tipo_id,
                       identificacion = @identificacion,
                       telefono_01 = @telefono_01,
                       telefono_02 = @telefono_02,
                       website = @website,
                       apto_postal = @apto_postal,
                       email_01 = @email_01,
                       email_02 = @email_02,
                       direccion = @direccion,
                       provincia = @provincia,
                       canton = @canton,
                       distrito = @distrito,
                       contacto_nombre = @contacto_nombre,
                       activo = @activo,
                       cod_banco = @cod_banco,
                       cod_cuenta = @cod_cuenta,
                       cod_proveedor = @cod_proveedor,
                       modifica_fecha = GETDATE(),
                       modifica_usuario = @usuario
                 WHERE cod_acreedor = @cod_acreedor
                """;

            return DbHelper.ExecuteNonQueryWithResult(_portalDb, codEmpresa, sql, m);
        }

        /// <summary>
        /// Elimina un acreedor.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codigo">Código del acreedor.</param>
        /// <returns>Cantidad de filas eliminadas.</returns>
        public ErrorDto<int> Borrar(int codEmpresa, int codigo)
        {
            const string sql =
                "DELETE FROM ARF_ACREEDORES WHERE COD_ACREEDOR = @codigo";
            return DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                codEmpresa,
                sql,
                new { codigo });
        }

        /// <summary>
        /// Obtiene el código anterior o siguiente de acuerdo con la dirección.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codigoActual">Código desde el que se navega.</param>
        /// <param name="direccion">Dirección positiva para siguiente y negativa para anterior.</param>
        /// <returns>Código del acreedor encontrado.</returns>
        public ErrorDto<int?> Scroll(int codEmpresa, int? codigoActual, int direccion)
        {
            var sql = direccion > 0
                ? """
                  SELECT TOP 1 COD_ACREEDOR
                  FROM ARF_ACREEDORES
                  WHERE COD_ACREEDOR > @codigoActual
                  ORDER BY COD_ACREEDOR ASC
                  """
                : """
                  SELECT TOP 1 COD_ACREEDOR
                  FROM ARF_ACREEDORES
                  WHERE COD_ACREEDOR < @codigoActual
                  ORDER BY COD_ACREEDOR DESC
                  """;

            var codigoBase = codigoActual ?? (direccion > 0 ? 0 : int.MaxValue);
            return DbHelper.ExecuteSingleQuery<int?>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { codigoActual = codigoBase });
        }

        /// <summary>
        /// Obtiene las cuentas bancarias registradas para el acreedor.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="identificacion">Identificación del acreedor.</param>
        /// <returns>Lista detallada de cuentas bancarias.</returns>
        public ErrorDto<List<CuentaBancariaAcreedorDto>> CuentasBancarias(
            int codEmpresa,
            string identificacion)
        {
            const string sql = """
                SELECT C.CUENTA_INTERNA AS cuenta,
                       RTRIM(B.Descripcion) AS banco,
                       CASE WHEN C.tipo = 'A' THEN 'Ahorros' ELSE 'Corriente' END AS tipo,
                       C.cod_divisa AS divisa,
                       CASE WHEN C.CUENTA_INTERBANCA = 1 THEN 'Sí' ELSE 'No' END AS interbanca,
                       C.DESTINO AS destino,
                       CASE WHEN C.ACTIVA = 1 THEN 'Activa' ELSE 'Cerrada' END AS estado,
                       C.REGISTRO_FECHA AS registro_fecha,
                       C.REGISTRO_USUARIO AS registro_usuario
                FROM SYS_CUENTAS_BANCARIAS C
                INNER JOIN TES_BANCOS_GRUPOS B
                        ON C.cod_banco = B.cod_grupo
                WHERE C.identificacion = @identificacion
                  AND C.modulo = 'ARF'
                """;

            return DbHelper.ExecuteListQuery<CuentaBancariaAcreedorDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { identificacion });
        }

        /// <summary>
        /// Obtiene las provincias disponibles.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de provincias.</returns>
        public ErrorDto<List<ProvinciaAcreedorDto>> ObtenerProvincias(int codEmpresa)
        {
            const string sql =
                "SELECT provincia, RTRIM(descripcion) AS descripcion FROM PROVINCIAS ORDER BY descripcion";
            return DbHelper.ExecuteListQuery<ProvinciaAcreedorDto>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene los cantones de una provincia.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="provincia">Código de provincia.</param>
        /// <returns>Lista de cantones.</returns>
        public ErrorDto<List<CantonAcreedorDto>> ObtenerCantones(
            int codEmpresa,
            string provincia)
        {
            const string sql = """
                SELECT canton, RTRIM(descripcion) AS descripcion
                FROM CANTONES
                WHERE provincia = @provincia
                ORDER BY descripcion
                """;
            return DbHelper.ExecuteListQuery<CantonAcreedorDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { provincia });
        }

        /// <summary>
        /// Obtiene los distritos de un cantón.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="provincia">Código de provincia.</param>
        /// <param name="canton">Código de cantón.</param>
        /// <returns>Lista de distritos.</returns>
        public ErrorDto<List<DistritoAcreedorDto>> ObtenerDistritos(
            int codEmpresa,
            string provincia,
            string canton)
        {
            const string sql = """
                SELECT provincia, canton, distrito, RTRIM(descripcion) AS descripcion
                FROM DISTRITOS
                WHERE provincia = @provincia
                  AND canton = @canton
                ORDER BY descripcion
                """;
            return DbHelper.ExecuteListQuery<DistritoAcreedorDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { provincia, canton });
        }

        /// <summary>
        /// Obtiene los tipos de identificación.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de tipos de identificación.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ObtenerTiposIdentificacion(
            int codEmpresa)
        {
            const string sql = """
                SELECT tipo_id AS item, RTRIM(descripcion) AS descripcion
                FROM AFI_TIPOS_IDS
                ORDER BY descripcion
                """;
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Busca acreedores por código, descripción o identificación.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Texto opcional de búsqueda.</param>
        /// <returns>Lista de acreedores coincidentes.</returns>
        public ErrorDto<List<ArfAcreedorDto>> BuscarAcreedores(
            int codEmpresa,
            string? filtro)
        {
            const string sql = """
                SELECT cod_acreedor, descripcion, identificacion
                FROM ARF_ACREEDORES
                WHERE @filtro IS NULL
                   OR CONVERT(VARCHAR(20), cod_acreedor) LIKE '%' + @filtro + '%'
                   OR descripcion LIKE '%' + @filtro + '%'
                   OR identificacion LIKE '%' + @filtro + '%'
                ORDER BY descripcion
                """;
            return DbHelper.ExecuteListQuery<ArfAcreedorDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { filtro });
        }

        /// <summary>
        /// Busca proveedores por código o descripción.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Texto opcional de búsqueda.</param>
        /// <returns>Lista de proveedores coincidentes.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> BuscarProveedores(
            int codEmpresa,
            string? filtro)
        {
            const string sql = """
                SELECT cod_proveedor AS item, RTRIM(descripcion) AS descripcion
                FROM CxP_Proveedores
                WHERE @filtro IS NULL
                   OR CONVERT(VARCHAR(20), cod_proveedor) LIKE '%' + @filtro + '%'
                   OR descripcion LIKE '%' + @filtro + '%'
                ORDER BY descripcion
                """;
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { filtro });
        }

        /// <summary>
        /// Obtiene los bancos autorizados para el usuario.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario de la sesión.</param>
        /// <returns>Lista de bancos disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ObtenerBancos(
            int codEmpresa,
            string usuario)
        {
            const string sql = "EXEC spCrd_SGT_Bancos @usuario";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { usuario });
        }
    }
}
