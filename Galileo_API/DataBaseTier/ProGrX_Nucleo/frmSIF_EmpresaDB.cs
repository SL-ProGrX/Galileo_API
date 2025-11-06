using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Nucleo;
using PgxAPI.Models.Security;
using System.Data;
namespace PgxAPI.DataBaseTier
{
    public class frmSIF_EmpresaDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;
        public frmSIF_EmpresaDB(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }
        /// <summary>
        /// Obtiene la configuraci�n de empresa desde SIF_EMPRESA.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<FrmSifEmpresaModel> Sif_Empresa_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            var r = new ErrorDto<FrmSifEmpresaModel> { Code = 0 };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var cn = new SqlConnection(conn);
                var sql = @"
                SELECT TOP (1)
                    e.ID_EMPRESA              AS id_empresa,
                    e.NOMBRE                  AS nombre,
                    RTRIM(LTRIM(e.CEDULA_JURIDICA)) AS cedula_juridica,
                    RTRIM(LTRIM(e.APTO_POSTAL))     AS apto_postal,
                    RTRIM(LTRIM(e.TELEFONOEMP))     AS telefonoemp,
                    RTRIM(LTRIM(e.FAX))             AS fax,
                    e.EMAIL                   AS email,
                    e.SITIO_WEB               AS sitio_web,

                    e.COD_EMPRESA_ENLACE      AS cod_empresa_enlace,
                    RTRIM(LTRIM(e.COD_CUENTA_NO_CFG)) AS cod_cuenta_no_cfg,
                    ISNULL(c.Descripcion,'')  AS cuenta_desc,

                    e.PAG_NOMLARGO            AS pag_nomlargo,
                    e.PAG_NOMCORTO            AS pag_nomcorto,
                    e.PAG_CEDJURLE            AS pag_cedjurle,
                    e.PAG_DOMICILIO           AS pag_domicilio,
                    e.REPRESENTANTE_LEGAL     AS representante_legal,
                    e.REPRESENTANTE_ID        AS representante_id,
                    e.REPRESENTANTE_CALIDADES AS representante_calidades,
                    e.PAG_SECCION_01          AS pag_seccion_01,
                    e.PAG_SECCION_02          AS pag_seccion_02,

                    CAST(CASE WHEN e.ESTADOCUENTA = 'C' THEN 1 ELSE 0 END AS bit) AS usar_estado_comercial,
                    e.EC_NOTA01               AS ec_nota01,
                    e.EC_NOTA02               AS ec_nota02,
                    CAST(e.EC_VISIBLE_PATRIMONIO AS bit) AS ec_visible_patrimonio,
                    CAST(e.EC_VISIBLE_FONDOS     AS bit) AS ec_visible_fondos,
                    CAST(e.EC_VISIBLE_CREDITOS   AS bit) AS ec_visible_creditos,
                    CAST(e.EC_VISIBLE_FIANZAS    AS bit) AS ec_visible_fianzas,
                    CAST(e.EC_VISIBLE_EXCEDENTES AS bit) AS ec_visible_excedentes,
                    CAST(e.EC_VISIBLE_DISPONIBLE AS bit) AS ec_visible_disponible,
                    e.LIQ_BOLETA_PIE          AS liq_boleta_pie,

                    e.MISION                  AS mision,
                    e.VISION                  AS vision,
                    e.SLOGAN                  AS slogan,

                    e.CONSENTIMIENTO_CONTACTO_TITULO AS consentimiento_contacto_titulo,
                    e.CONSENTIMIENTO_CONTACTO_TEXTO  AS consentimiento_contacto_texto,

                    e.CONSTANCIA_CRD_ENCABEZADO AS constancia_crd_encabezado,
                    e.CONSTANCIA_CRD_PIE        AS constancia_crd_pie,
                    e.CONSTANCIA_PAT_ENCABEZADO AS constancia_pat_encabezado,
                    e.CONSTANCIA_PAT_PIE        AS constancia_pat_pie,
                    CAST(e.CONSTANCIA_FECHA_VINCULACION AS bit) AS constancia_fecha_vinculacion,

                    e.FECHA_CONGELA            AS fecha_congela,

                    CAST(ISNULL(e.SINPE_ACTIVO, 0) AS bit) AS sinpe_activo
                FROM SIF_EMPRESA e
                LEFT JOIN vCNTX_CUENTAS_LOCAL c
                       ON e.COD_EMPRESA_ENLACE = c.COD_CONTABILIDAD
                      AND (
                            RTRIM(LTRIM(e.COD_CUENTA_NO_CFG)) = RTRIM(LTRIM(c.COD_CUENTA))
                         OR RTRIM(LTRIM(e.COD_CUENTA_NO_CFG)) = RTRIM(LTRIM(c.Cod_Cuenta_Mask))
                      )
                WHERE (@id IS NULL OR e.ID_EMPRESA = @id)
                ORDER BY e.ID_EMPRESA;";

                r.Result = cn.QueryFirstOrDefault<FrmSifEmpresaModel>(sql, new { id = idEmpresa });
                r.Description = r.Result == null ? "No existe SIF_EMPRESA" : "OK";
                r.Code = r.Result == null ? 1 : 0;
            }
            catch (Exception ex)
            {
                r.Code = -1;
                r.Description = ex.Message;
            }
            return r;
        }

        /// <summary>
        /// Guarda la configuraci�n de SIF_EMPRESA.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Sif_Empresa_Guardar(int CodEmpresa, FrmSifEmpresaModel dto, string usuario)
        {
            var r = new ErrorDto { Code = 0, Description = "OK" };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var cn = new SqlConnection(conn);
                cn.Open();
                using var tx = cn.BeginTransaction();

                var p = new
                {
                    dto.id_empresa,

                    // datos empresa
                    dto.nombre,
                    dto.cedula_juridica,
                    dto.apto_postal,
                    dto.telefonoemp,
                    dto.fax,
                    dto.email,
                    dto.sitio_web,

                    // contabilidad
                    dto.cod_empresa_enlace,
                    cod_cuenta_no_cfg = string.IsNullOrWhiteSpace(dto.cod_cuenta_no_cfg) ? null : dto.cod_cuenta_no_cfg,

                    // pagar�
                    dto.pag_nomlargo,
                    dto.pag_nomcorto,
                    dto.pag_cedjurle,
                    dto.pag_domicilio,
                    dto.representante_legal,
                    dto.representante_id,
                    dto.representante_calidades,
                    dto.pag_seccion_01,
                    dto.pag_seccion_02,

                    // flags estado de cuenta
                    dto.usar_estado_comercial,
                    dto.ec_nota01,
                    dto.ec_nota02,
                    dto.ec_visible_patrimonio,
                    dto.ec_visible_fondos,
                    dto.ec_visible_creditos,
                    dto.ec_visible_fianzas,
                    dto.ec_visible_excedentes,
                    dto.ec_visible_disponible,

                    // pie de boleta
                    dto.liq_boleta_pie,

                    // misi�n/visi�n
                    dto.mision,
                    dto.vision,
                    dto.slogan,

                    // consentimiento
                    dto.consentimiento_contacto_titulo,
                    dto.consentimiento_contacto_texto,

                    // constancias
                    dto.constancia_crd_encabezado,
                    dto.constancia_crd_pie,
                    dto.constancia_pat_encabezado,
                    dto.constancia_pat_pie,
                    dto.constancia_fecha_vinculacion,

                    dto.sinpe_activo
                };

                var exists = cn.ExecuteScalar<int>(
                    "SELECT COUNT(1) FROM SIF_EMPRESA WHERE ID_EMPRESA=@id_empresa",
                    new { dto.id_empresa }, tx);

                bool esUpdate = exists > 0;
                if (esUpdate)
                {
                    var updateSql = @"
                    UPDATE SIF_EMPRESA SET
                        NOMBRE=@nombre, CEDULA_JURIDICA=@cedula_juridica, APTO_POSTAL=@apto_postal,
                        TELEFONOEMP=@telefonoemp, FAX=@fax, EMAIL=@email, SITIO_WEB=@sitio_web,

                        COD_EMPRESA_ENLACE=@cod_empresa_enlace, COD_CUENTA_NO_CFG=@cod_cuenta_no_cfg,

                        PAG_NOMLARGO=@pag_nomlargo, PAG_NOMCORTO=@pag_nomcorto, PAG_CEDJURLE=@pag_cedjurle, PAG_DOMICILIO=@pag_domicilio,
                        REPRESENTANTE_LEGAL=@representante_legal, REPRESENTANTE_ID=@representante_id, REPRESENTANTE_CALIDADES=@representante_calidades,
                        PAG_SECCION_01=@pag_seccion_01, PAG_SECCION_02=@pag_seccion_02,

                        ESTADOCUENTA = CASE WHEN @usar_estado_comercial=1 THEN 'C' ELSE 'S' END,
                        EC_NOTA01=@ec_nota01, EC_NOTA02=@ec_nota02,
                        EC_VISIBLE_PATRIMONIO=@ec_visible_patrimonio, EC_VISIBLE_FONDOS=@ec_visible_fondos,
                        EC_VISIBLE_CREDITOS=@ec_visible_creditos, EC_VISIBLE_FIANZAS=@ec_visible_fianzas,
                        EC_VISIBLE_EXCEDENTES=@ec_visible_excedentes, EC_VISIBLE_DISPONIBLE=@ec_visible_disponible,
                        LIQ_BOLETA_PIE=@liq_boleta_pie,

                        MISION=@mision, VISION=@vision, SLOGAN=@slogan,

                        CONSENTIMIENTO_CONTACTO_TITULO=@consentimiento_contacto_titulo,
                        CONSENTIMIENTO_CONTACTO_TEXTO=@consentimiento_contacto_texto,

                        CONSTANCIA_CRD_ENCABEZADO=@constancia_crd_encabezado,
                        CONSTANCIA_CRD_PIE=@constancia_crd_pie,
                        CONSTANCIA_PAT_ENCABEZADO=@constancia_pat_encabezado,
                        CONSTANCIA_PAT_PIE=@constancia_pat_pie,
                        CONSTANCIA_FECHA_VINCULACION=@constancia_fecha_vinculacion,

                        SINPE_ACTIVO = CAST(@sinpe_activo AS smallint)
                    WHERE ID_EMPRESA=@id_empresa;";
                                        cn.Execute(updateSql, p, tx);
                                    }
                                    else
                                    {
                                        var insertSql = @"
                    INSERT INTO SIF_EMPRESA(
                        NOMBRE, CEDULA_JURIDICA, APTO_POSTAL, TELEFONOEMP, FAX, EMAIL, SITIO_WEB,
                        COD_EMPRESA_ENLACE, COD_CUENTA_NO_CFG,
                        PAG_NOMLARGO, PAG_NOMCORTO, PAG_CEDJURLE, PAG_DOMICILIO,
                        REPRESENTANTE_LEGAL, REPRESENTANTE_ID, REPRESENTANTE_CALIDADES,
                        PAG_SECCION_01, PAG_SECCION_02,
                        ESTADOCUENTA, EC_NOTA01, EC_NOTA02,
                        EC_VISIBLE_PATRIMONIO, EC_VISIBLE_FONDOS, EC_VISIBLE_CREDITOS, EC_VISIBLE_FIANZAS,
                        EC_VISIBLE_EXCEDENTES, EC_VISIBLE_DISPONIBLE, LIQ_BOLETA_PIE,
                        MISION, VISION, SLOGAN,
                        CONSENTIMIENTO_CONTACTO_TITULO, CONSENTIMIENTO_CONTACTO_TEXTO,
                        CONSTANCIA_CRD_ENCABEZADO, CONSTANCIA_CRD_PIE, CONSTANCIA_PAT_ENCABEZADO, CONSTANCIA_PAT_PIE,
                        CONSTANCIA_FECHA_VINCULACION,
                        SINPE_ACTIVO
                    ) VALUES (
                        @nombre, @cedula_juridica, @apto_postal, @telefonoemp, @fax, @email, @sitio_web,
                        @cod_empresa_enlace, @cod_cuenta_no_cfg,
                        @pag_nomlargo, @pag_nomcorto, @pag_cedjurle, @pag_domicilio,
                        @representante_legal, @representante_id, @representante_calidades,
                        @pag_seccion_01, @pag_seccion_02,
                        CASE WHEN @usar_estado_comercial=1 THEN 'C' ELSE 'S' END, @ec_nota01, @ec_nota02,
                        @ec_visible_patrimonio, @ec_visible_fondos, @ec_visible_creditos, @ec_visible_fianzas,
                        @ec_visible_excedentes, @ec_visible_disponible, @liq_boleta_pie,
                        @mision, @vision, @slogan,
                        @consentimiento_contacto_titulo, @consentimiento_contacto_texto,
                        @constancia_crd_encabezado, @constancia_crd_pie, @constancia_pat_encabezado, @constancia_pat_pie,
                        @constancia_fecha_vinculacion,
                        CAST(@sinpe_activo AS smallint)
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    var newId = cn.ExecuteScalar<int>(insertSql, p, tx);
                    dto.id_empresa = newId;
                }

                tx.Commit();
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    Modulo = vModulo,
                    Movimiento = esUpdate ? "Modifica - WEB" : "Registra - WEB",
                    DetalleMovimiento = $"Empresa: {dto.nombre} (ID: {dto.id_empresa})"
                });
            }
            catch (Exception ex)
            {
                r.Code = -1;
                r.Description = ex.Message;
            }
            return r;
        }

        /// <summary>
        /// Devuelve el LOGO almacenado en SIF_EMPRESA como varbinary.
        /// <param name="CodEmpresa"></param>
        /// <param name="idEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<byte[]> Sif_Empresa_Logo_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            var r = new ErrorDto<byte[]> { Code = 0 };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var cn = new SqlConnection(conn);
                var sql = @"SELECT TOP 1 LOGO FROM SIF_EMPRESA
                            WHERE (@id IS NULL) OR ID_EMPRESA=@id
                            ORDER BY ID_EMPRESA;";
                r.Result = cn.ExecuteScalar<byte[]>(sql, new { id = idEmpresa });
                r.Description = "OK";
            }
            catch (Exception ex) { r.Code = -1; r.Description = ex.Message; }
            return r;
        }

        /// <summary>
        /// Guarda el LOGO en SIF_EMPRESA (columna LOGO).
        /// </summary>
        /// <param name="CodEmpresa">/param>
        /// <param name="idEmpresa"></param>
        /// <param name="contenido"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Sif_Empresa_Logo_Guardar(int CodEmpresa, int idEmpresa, byte[] contenido, string usuario)
        {
            var r = new ErrorDto { Code = 0, Description = "OK" };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var cn = new SqlConnection(conn);
                cn.Execute("UPDATE SIF_EMPRESA SET LOGO=@contenido WHERE ID_EMPRESA=@id",
                           new { id = idEmpresa, contenido });
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    Modulo = vModulo,
                    Movimiento = "Actualiza - WEB",
                    DetalleMovimiento = $"Empresa: Logo actualizado (ID: {idEmpresa})"
                });
            }
            catch (Exception ex) { r.Code = -1; r.Description = ex.Message; }
            return r;
        }

        /// <summary>
        /// Devuelve el FONDO_PANTALLA almacenado en SIF_EMPRESA como varbinary.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<byte[]> Sif_Empresa_Fondo_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            var r = new ErrorDto<byte[]> { Code = 0 };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var cn = new SqlConnection(conn);
                var sql = @"SELECT TOP 1 FONDO_PANTALLA FROM SIF_EMPRESA
                            WHERE (@id IS NULL) OR ID_EMPRESA=@id
                            ORDER BY ID_EMPRESA;";
                r.Result = cn.ExecuteScalar<byte[]>(sql, new { id = idEmpresa });
                r.Description = "OK";
            }
            catch (Exception ex) { r.Code = -1; r.Description = ex.Message; }
            return r;
        }

        /// <summary>
        /// Guarda el FONDO_PANTALLA en SIF_EMPRESA (columna FONDO_PANTALLA).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idEmpresa"></param>
        /// <param name="contenido"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Sif_Empresa_Fondo_Guardar(int CodEmpresa, int idEmpresa, byte[] contenido, string usuario)
        {
            var r = new ErrorDto { Code = 0, Description = "OK" };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var cn = new SqlConnection(conn);
                cn.Execute("UPDATE SIF_EMPRESA SET FONDO_PANTALLA=@contenido WHERE ID_EMPRESA=@id",
                           new { id = idEmpresa, contenido });
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    Modulo = vModulo,
                    Movimiento = "Actualiza - WEB",
                    DetalleMovimiento = $"Empresa: Fondo de pantalla actualizado (ID: {idEmpresa})"
                });
            }
            catch (Exception ex) { r.Code = -1; r.Description = ex.Message; }
            return r;
        }

        /// <summary>
        /// Lista las contabilidades disponibles para el combo (CNTX_CONTABILIDADES).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ComboContabilidadDto>> Sif_Empresa_Contabilidades_Obtener(int CodEmpresa)
        {
            var r = new ErrorDto<List<ComboContabilidadDto>> { Code = 0, Result = new() };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var cn = new SqlConnection(conn);
                var sql = @"SELECT COD_CONTABILIDAD AS idx, RTRIM(NOMBRE) AS itmx, RTRIM(NOMBRE) AS descripcion
                            FROM CNTX_CONTABILIDADES
                            ORDER BY NOMBRE;";
                r.Result = cn.Query<ComboContabilidadDto>(sql).ToList();
                r.Description = "OK";
            }
            catch (Exception ex) { r.Code = -1; r.Description = ex.Message; r.Result = null; }
            return r;
        }

        /// <summary>
        /// Busca una cuenta exacta en la vista local por contabilidad y c�digo de cuenta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codCuenta"></param>
        /// <returns></returns>
        public ErrorDto<CuentaLookupDto> Sif_Empresa_CuentaPorCodigo_Obtener(int CodEmpresa, int codContabilidad, string codCuenta)
        {
            var r = new ErrorDto<CuentaLookupDto> { Code = 0 };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var cn = new SqlConnection(conn);
                var sql = @"SELECT TOP 1 Cod_Cuenta_Mask AS cod_cuenta_mask, Descripcion
                            FROM vCNTX_CUENTAS_LOCAL
                            WHERE COD_CONTABILIDAD=@conta AND COD_CUENTA=@cuenta;";
                r.Result = cn.QueryFirstOrDefault<CuentaLookupDto>(sql, new { conta = codContabilidad, cuenta = codCuenta });
                r.Description = r.Result == null ? "No existe la cuenta" : "OK";
                r.Code = r.Result == null ? 1 : 0;
            }
            catch (Exception ex) { r.Code = -1; r.Description = ex.Message; }
            return r;
        }

        /// <summary>
        /// Busca cuentas por texto (m�scara o descripci�n) en la vista local.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public ErrorDto<List<CuentaLookupDto>> Sif_Empresa_Cuentas_Buscar(
         int CodEmpresa, int codContabilidad, string? search)
        {
            var r = new ErrorDto<List<CuentaLookupDto>> { Code = 0, Result = new() };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var cn = new SqlConnection(conn);

                const string sql = @"
            SELECT TOP 50 Cod_Cuenta_Mask AS cod_cuenta_mask, Descripcion
            FROM vCNTX_CUENTAS_LOCAL
            WHERE COD_CONTABILIDAD = @conta
              AND (
                    @q IS NULL OR LTRIM(RTRIM(@q)) = ''
                    OR Cod_Cuenta_Mask LIKE '%' + @q + '%'
                    OR Descripcion      LIKE '%' + @q + '%'
                  )
            ORDER BY Cod_Cuenta_Mask;";
                var q = search;

                r.Result = cn.Query<CuentaLookupDto>(sql, new { conta = codContabilidad, q }).ToList();
                r.Description = "OK";
            }
            catch (Exception ex)
            {
                r.Code = -1;
                r.Description = ex.Message;
                r.Result = null;
            }
            return r;
        }

        /// <summary>
        /// Aplica bloqueo o desbloqueo de fecha de auxiliares mediante el SP spSys_BloqueoFechaAuxiliar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="fecha"></param>
        /// <param name="accion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Sif_Empresa_BloqueoFecha_Aplicar(int CodEmpresa, DateTime fecha, char accion, string usuario)
        {
            var r = new ErrorDto { Code = 0, Description = "OK" };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var cn = new SqlConnection(conn);

                var fechaHora = new DateTime(fecha.Year, fecha.Month, fecha.Day, 22, 0, 0);

                cn.Execute(
                    "spSys_BloqueoFechaAuxiliar",
                    new { Fecha = fechaHora, Tipo = accion, Usuario = usuario },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 180
                );
                // Bit�cora
                var mov = "Aplica - WEB";
                var detalle = (char.ToUpperInvariant(accion) == 'B')
                    ? $"Bloquea Fecha Auxiliar: {fechaHora:yyyy/MM/dd}"
                    : "DES-Bloqueo Fecha Auxiliar";

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    Modulo = vModulo,
                    Movimiento = mov,
                    DetalleMovimiento = detalle
                });
            }
            catch (Exception ex)
            {
                r.Code = -1;
                r.Description = ex.Message;
            }

            return r;
        }


        /// <summary>
        /// Obtiene la fecha de bloqueo almacenada (FECHA_CONGELA) desde SIF_EMPRESA.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<DateTime?> Sif_Empresa_BloqueoFecha_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            var r = new ErrorDto<DateTime?> { Code = 0 };
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var cn = new SqlConnection(conn);
                var sql = @"SELECT TOP 1 FECHA_CONGELA FROM SIF_EMPRESA 
                            WHERE (@id IS NULL) OR ID_EMPRESA=@id
                            ORDER BY ID_EMPRESA;";
                r.Result = cn.ExecuteScalar<DateTime?>(sql, new { id = idEmpresa });
                r.Description = "OK";
            }
            catch (Exception ex) { r.Code = -1; r.Description = ex.Message; }
            return r;
        }
    }
}