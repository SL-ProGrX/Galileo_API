using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using System.Data;
using Galileo.Models.Security;


namespace Galileo.DataBaseTier
{
    public class FrmSifEmpresaDB
    {
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmSifEmpresaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        private ErrorDto TryBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            try
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                    Modulo = vModulo,
                    Movimiento = movimiento,
                    DetalleMovimiento = detalle
                });

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? "Error inesperado");
            }
        }
        
        public ErrorDto<FrmSifEmpresaModel> Sif_Empresa_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            const string sql = @"
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

            var r = DbHelper.ExecuteSingleQuery<FrmSifEmpresaModel>(_portalDB, CodEmpresa, sql, defaultValue: null, parameters: new { id = idEmpresa });

            if ((r.Code ?? -1) != 0)
                return new ErrorDto<FrmSifEmpresaModel> { Code = r.Code, Description = r.Description, Result = null };

            var model = r.Result;
            if (model == null)
            {
                return new ErrorDto<FrmSifEmpresaModel>
                {
                    Code = 1,
                    Description = "No existe SIF_EMPRESA",
                    Result = null
                };
            }

            return new ErrorDto<FrmSifEmpresaModel> { Code = 0, Description = "OK", Result = model };
        }


        public ErrorDto Sif_Empresa_Guardar(int CodEmpresa, FrmSifEmpresaModel dto, string usuario)
        {
            if (dto == null)
                return DbHelper.ErrorResponse("Datos inválidos");

            var usuarioLocal = usuario ?? string.Empty;

            var exec = DbHelper.WithConn(_portalDB, CodEmpresa, cn =>
            {
                cn.Open();
                using var tx = cn.BeginTransaction();

                var p = new
                {
                    dto.id_empresa,

                    dto.nombre,
                    dto.cedula_juridica,
                    dto.apto_postal,
                    dto.telefonoemp,
                    dto.fax,
                    dto.email,
                    dto.sitio_web,

                    dto.cod_empresa_enlace,
                    cod_cuenta_no_cfg = string.IsNullOrWhiteSpace(dto.cod_cuenta_no_cfg) ? null : dto.cod_cuenta_no_cfg,

                    dto.pag_nomlargo,
                    dto.pag_nomcorto,
                    dto.pag_cedjurle,
                    dto.pag_domicilio,
                    dto.representante_legal,
                    dto.representante_id,
                    dto.representante_calidades,
                    dto.pag_seccion_01,
                    dto.pag_seccion_02,

                    dto.usar_estado_comercial,
                    dto.ec_nota01,
                    dto.ec_nota02,
                    dto.ec_visible_patrimonio,
                    dto.ec_visible_fondos,
                    dto.ec_visible_creditos,
                    dto.ec_visible_fianzas,
                    dto.ec_visible_excedentes,
                    dto.ec_visible_disponible,

                    dto.liq_boleta_pie,

                    dto.mision,
                    dto.vision,
                    dto.slogan,

                    dto.consentimiento_contacto_titulo,
                    dto.consentimiento_contacto_texto,

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

                var esUpdate = exists > 0;

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

                var movimiento = esUpdate ? "Modifica - WEB" : "Registra - WEB";
                var detalle = $"Empresa: {dto.nombre} (ID: {dto.id_empresa})";
                var bit = TryBitacora(CodEmpresa, usuarioLocal, movimiento, detalle);

                if ((bit.Code ?? -1) != 0)
                    return bit;

                return DbHelper.CreateOkResponse();
            });

            return (exec.Code ?? -1) == 0
                ? DbHelper.CreateOkResponse()
                : DbHelper.ErrorResponse(exec.Description ?? "Error", exec.Code ?? -1);
        }


        public ErrorDto<byte[]> Sif_Empresa_Logo_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            const string sql = @"SELECT TOP 1 LOGO FROM SIF_EMPRESA
                            WHERE (@id IS NULL) OR ID_EMPRESA=@id
                            ORDER BY ID_EMPRESA;";

            var r = DbHelper.WithConn(_portalDB, CodEmpresa, cn => cn.ExecuteScalar<byte[]>(sql, new { id = idEmpresa }));

            if ((r.Code ?? -1) != 0)
                return new ErrorDto<byte[]> { Code = r.Code, Description = r.Description, Result = null };

            return new ErrorDto<byte[]> { Code = 0, Description = "OK", Result = r.Result ?? Array.Empty<byte>() };
        }


        public ErrorDto Sif_Empresa_Logo_Guardar(int CodEmpresa, int idEmpresa, byte[] contenido, string usuario)
        {
            const string sql = @"UPDATE SIF_EMPRESA SET LOGO=@contenido WHERE ID_EMPRESA=@id";

            var exec = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql, new { id = idEmpresa, contenido });
            if ((exec.Code ?? -1) != 0)
                return exec;

            var bit = TryBitacora(CodEmpresa, usuario, "Actualiza - WEB", $"Empresa: Logo actualizado (ID: {idEmpresa})");
            return (bit.Code ?? -1) == 0 ? DbHelper.CreateOkResponse() : bit;
        }


        public ErrorDto<byte[]> Sif_Empresa_Fondo_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            const string sql = @"SELECT TOP 1 FONDO_PANTALLA FROM SIF_EMPRESA
                            WHERE (@id IS NULL) OR ID_EMPRESA=@id
                            ORDER BY ID_EMPRESA;";

            var r = DbHelper.WithConn(_portalDB, CodEmpresa, cn => cn.ExecuteScalar<byte[]>(sql, new { id = idEmpresa }));

            if ((r.Code ?? -1) != 0)
                return new ErrorDto<byte[]> { Code = r.Code, Description = r.Description, Result = null };

            return new ErrorDto<byte[]> { Code = 0, Description = "OK", Result = r.Result ?? Array.Empty<byte>() };
        }


        public ErrorDto Sif_Empresa_Fondo_Guardar(int CodEmpresa, int idEmpresa, byte[] contenido, string usuario)
        {
            const string sql = @"UPDATE SIF_EMPRESA SET FONDO_PANTALLA=@contenido WHERE ID_EMPRESA=@id";

            var exec = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql, new { id = idEmpresa, contenido });
            if ((exec.Code ?? -1) != 0)
                return exec;

            var bit = TryBitacora(CodEmpresa, usuario, "Actualiza - WEB", $"Empresa: Fondo de pantalla actualizado (ID: {idEmpresa})");
            return (bit.Code ?? -1) == 0 ? DbHelper.CreateOkResponse() : bit;
        }


        public ErrorDto<List<ComboContabilidadDto>> Sif_Empresa_Contabilidades_Obtener(int CodEmpresa)
        {
            const string sql = @"SELECT COD_CONTABILIDAD AS idx, RTRIM(NOMBRE) AS itmx, RTRIM(NOMBRE) AS descripcion
                            FROM CNTX_CONTABILIDADES
                            ORDER BY NOMBRE;";

            return DbHelper.ExecuteListQuery<ComboContabilidadDto>(_portalDB, CodEmpresa, sql);
        }


        public ErrorDto<CuentaLookupDto> Sif_Empresa_CuentaPorCodigo_Obtener(int CodEmpresa, int codContabilidad, string codCuenta)
        {
            const string sql = @"SELECT TOP 1 Cod_Cuenta_Mask AS cod_cuenta_mask, Descripcion
                            FROM vCNTX_CUENTAS_LOCAL
                            WHERE COD_CONTABILIDAD=@conta AND COD_CUENTA=@cuenta;";

            var r = DbHelper.ExecuteSingleQuery<CuentaLookupDto>(_portalDB, CodEmpresa, sql, defaultValue: null, parameters: new { conta = codContabilidad, cuenta = codCuenta });

            if ((r.Code ?? -1) != 0)
                return new ErrorDto<CuentaLookupDto> { Code = r.Code, Description = r.Description, Result = null };

            if (r.Result == null)
                return new ErrorDto<CuentaLookupDto> { Code = 1, Description = "No existe la cuenta", Result = null };

            return new ErrorDto<CuentaLookupDto> { Code = 0, Description = "OK", Result = r.Result };
        }


        public ErrorDto<List<CuentaLookupDto>> Sif_Empresa_Cuentas_Buscar(int CodEmpresa, int codContabilidad, string? search)
        {
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

            return DbHelper.ExecuteListQuery<CuentaLookupDto>(_portalDB, CodEmpresa, sql, new { conta = codContabilidad, q = search });
        }


        public ErrorDto Sif_Empresa_BloqueoFecha_Aplicar(int CodEmpresa, DateTime fecha, char accion, string usuario)
        {
            var fechaHora = new DateTime(fecha.Year, fecha.Month, fecha.Day, 22, 0, 0, DateTimeKind.Unspecified);
            var usuarioLocal = usuario ?? string.Empty;

            var exec = DbHelper.WithConn(_portalDB, CodEmpresa, cn =>
            {
                cn.Execute(
                    "spSys_BloqueoFechaAuxiliar",
                    new { Fecha = fechaHora, Tipo = accion, Usuario = usuarioLocal },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 180);
                return 0;
            });

            if ((exec.Code ?? -1) != 0)
                return DbHelper.ErrorResponse(exec.Description ?? "Error", exec.Code ?? -1);

            var mov = "Aplica - WEB";
            var detalle = (char.ToUpperInvariant(accion) == 'B')
                ? $"Bloquea Fecha Auxiliar: {fechaHora:yyyy/MM/dd}"
                : "DES-Bloqueo Fecha Auxiliar";

            var bit = TryBitacora(CodEmpresa, usuarioLocal, mov, detalle);
            return (bit.Code ?? -1) == 0 ? DbHelper.CreateOkResponse() : bit;
        }


        public ErrorDto<DateTime?> Sif_Empresa_BloqueoFecha_Obtener(int CodEmpresa, int? idEmpresa = null)
        {
            const string sql = @"SELECT TOP 1 FECHA_CONGELA FROM SIF_EMPRESA 
                            WHERE (@id IS NULL) OR ID_EMPRESA=@id
                            ORDER BY ID_EMPRESA;";

            var r = DbHelper.WithConn(_portalDB, CodEmpresa, cn => cn.ExecuteScalar<DateTime?>(sql, new { id = idEmpresa }));

            if ((r.Code ?? -1) != 0)
                return new ErrorDto<DateTime?> { Code = r.Code, Description = r.Description, Result = null };

            return new ErrorDto<DateTime?> { Code = 0, Description = "OK", Result = r.Result };
        }
    }
}