namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    using System.Data;
    using Dapper;
    using Galileo.DataBaseTier;
    using Galileo.Models.ERROR;
    using Galileo_API.Models.ProGrX_ControlTramites;

    public class FrmAfLiquidacionRevisionDB
    {
        private readonly PortalDB _portalDb;

        public FrmAfLiquidacionRevisionDB(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmAfLiquidacionRevisionDB(PortalDB portalDB)
        {
            _portalDb = portalDB;
        }

        /// <summary>
        /// Obtiene liquidaciones pendientes de revisión de analista.
        /// El filtro de cédula es opcional.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionRevisionListaModel>> AF_LiquidacionRevision_Obtener(
            int CodEmpresa,
            string? Cedula)
        {
            const string sqlQuery = @"
                -- Liquidaciones pendientes de revisión (frmAF_LiquidacionRevision).
                -- @Cedula: cédula opcional del socio para filtrar resultados.
                SELECT
                    'B' as Tipo,
                    L.cedula as Cedula,
                    S.nombre as Nombre,
                    L.usuario as Usuario_Registra,
                    convert(varchar(50), L.cod_remesa) as No_Remesa,
                    R.usuario as Usuario_Remesa,
                    convert(varchar(50), L.consec) as No_Boleta
                FROM Liquidacion L
                INNER JOIN Socios S
                    ON L.Cedula = S.cedula
                LEFT JOIN AFI_REMESAS_LIQ R
                    ON L.cod_remesa = R.cod_remesa
                WHERE L.ANALISTA_REVISION IS NULL
                  AND (@Cedula IS NULL OR L.cedula = @Cedula)";

            var cedulaFiltro = string.IsNullOrWhiteSpace(Cedula)
                ? null
                : Cedula.Trim();

            return DbHelper.ExecuteListQuery<AfLiquidacionRevisionListaModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                new { Cedula = cedulaFiltro });
        }

        /// <summary>
        /// Obtiene el encabezado de detalle de una liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Consec"></param>
        /// <returns></returns>
        public ErrorDto<AfLiquidacionRevisionDetalleModel?> AF_LiquidacionRevision_Detalle_Obtener(
            int CodEmpresa,
            int Consec)
        {
            const string sqlQuery = @"
                -- Detalle de liquidación (frmAF_LiquidacionRevision).
                -- @Consec: consecutivo de la liquidación.
                SELECT
                    L.Consec as Consec,
                    L.cedula as Cedula,
                    S.nombre as Nombre,
                    isnull(L.AHORRO_LIQ, 0) as Ahorro_Liq,
                    isnull(L.APORTE_LIQ, 0) as Aporte_Liq,
                    isnull(L.CAPITALIZADO_LIQ, 0) as Capitalizado_Liq,
                    isnull(L.TOTALBRUTO, 0) as Total_Bruto,
                    isnull(L.TNETO, 0) as T_Neto,
                    isnull(L.RETENIDO, 0) as Retenido,
                    convert(varchar(50), isnull(L.AC_BOLETA, '')) as Ac_Boleta,
                    L.AC_FECHA as Ac_Fecha,
                    L.FECLIQ as Fecliq,
                    convert(varchar(50), isnull(L.TDOCUMENTO, '')) as Tdocumento,
                    T.DESCRIPCION as Banco,
                    R.DESCRIPCION as Causa,
                    case
                        when L.estadoactliq = 'A' then 'Ren. Asociación'
                        else 'Ren. Partronal'
                    end as Tipo,
                    isnull(Cta.Cuenta, '') as Cuenta
                FROM Liquidacion L
                INNER JOIN Socios S
                    ON L.cedula = S.cedula
                INNER JOIN TES_BANCOS T
                    ON L.cod_banco = T.ID_BANCO
                INNER JOIN causas_renuncias R
                    ON L.ID_CAUSA = R.ID_CAUSA
                LEFT JOIN CUENTAS_AHORROS Cta
                    ON L.cod_banco = Cta.ID_BANCO
                    AND L.CEDULA = Cta.CEDULA
                WHERE L.CONSEC = @Consec";

            return DbHelper.ExecuteSingleQuery<AfLiquidacionRevisionDetalleModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                default,
                new { Consec });
        }

        /// <summary>
        /// Obtiene las operaciones del detalle de una liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Consec"></param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionRevisionOperacionModel>> AF_LiquidacionRevision_Operaciones_Obtener(
            int CodEmpresa,
            int Consec)
        {
            const string sqlQuery = @"
                -- Operaciones de liquidación (frmAF_LiquidacionRevision).
                -- @Consec: consecutivo de la liquidación.
                SELECT
                    convert(varchar(50), ID_SOLICITUD) as Id_Solicitud,
                    convert(varchar(50), CODIGO) as Codigo,
                    isnull(LIQ_ABONO, 0) as Abono,
                    isnull(LIQ_SALDO, 0) as Saldo,
                    isnull(LIQ_SALDO, 0) - isnull(LIQ_AMORTIZA, 0) as Resultante
                FROM LIQUIDA_DETALLE
                WHERE CONSEC = @Consec";

            return DbHelper.ExecuteListQuery<AfLiquidacionRevisionOperacionModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                new { Consec });
        }

        /// <summary>
        /// Obtiene el seguimiento de etiquetas de una liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cedula"></param>
        /// <param name="Documento"></param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionRevisionSeguimientoModel>> AF_LiquidacionRevision_Seguimiento_Obtener(
            int CodEmpresa,
            string Cedula,
            string Documento)
        {
            const string sqlQuery = @"
                -- Seguimiento de etiquetas de liquidación (frmAF_LiquidacionRevision).
                -- @Cedula: cédula del socio.
                -- @Documento: consecutivo/boleta de la liquidación.
                SELECT
                    isnull(rtrim(T.DESCRIPCION), '') as Descripcion,
                    isnull(rtrim(OT.NOTAS), '') as Notas,
                    OT.REGISTRO_FECHA as Registro_Fecha,
                    isnull(rtrim(OT.REGISTRO_USUARIO), '') as Registro_Usuario
                FROM SIF_CONTROL_TAGS OT
                INNER JOIN SIF_TAGS T
                    ON OT.TAG_CODIGO = T.TAG_CODIGO
                WHERE OT.codigo = @Cedula
                  AND OT.cod_Modulo = 'LIQ'
                  AND OT.Documento = @Documento
                ORDER BY OT.REGISTRO_FECHA DESC";

            return DbHelper.ExecuteListQuery<AfLiquidacionRevisionSeguimientoModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                new
                {
                    Cedula = Cedula.Trim(),
                    Documento = Documento.Trim(),
                });
        }

        /// <summary>
        /// Obtiene las etiquetas activas del usuario para el módulo LIQ.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionRevisionEtiquetaModel>> AF_LiquidacionRevision_Etiquetas_Obtener(
            int CodEmpresa,
            string Usuario)
        {
            const string sqlQuery = @"
                -- Etiquetas de revisión (frmAF_LiquidacionRevision).
                -- @Usuario: usuario de sesión para filtrar por grupo.
                SELECT DISTINCT
                    convert(varchar(50), CT.TAG_CODIGO) as Tag_Codigo,
                    convert(varchar(50), CT.TAG_CODIGO) + ' - ' + rtrim(CT.DESCRIPCION) as ItmX
                FROM SIF_TAGS CT
                INNER JOIN SIF_TAGS_GRUPOS CTG
                    ON CT.TAG_CODIGO = CTG.TAG_CODIGO
                INNER JOIN SIF_GRPUSERS CGU
                    ON CTG.COD_GRUPO = CGU.COD_GRUPO
                WHERE CT.ACTIVO = 1
                  AND CGU.USUARIO = @Usuario
                  AND CT.TAG_CODIGO IN
                  (
                      SELECT TAG_CODIGO
                      FROM SIF_TAGS_MODULOS
                      WHERE cod_modulo = 'LIQ'
                  )
                ORDER BY Tag_Codigo";

            return DbHelper.ExecuteListQuery<AfLiquidacionRevisionEtiquetaModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                new { Usuario = Usuario.Trim() });
        }

        /// <summary>
        /// Obtiene las omisiones del módulo LIQ para una liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cedula"></param>
        /// <param name="Documento"></param>
        /// <returns></returns>
        public ErrorDto<List<AfLiquidacionRevisionOmisionModel>> AF_LiquidacionRevision_Omisiones_Obtener(
            int CodEmpresa,
            string Cedula,
            string Documento)
        {
            const string sqlQuery = @"
                -- Omisiones de revisión (frmAF_LiquidacionRevision).
                -- @Cedula: cédula del socio.
                -- @Documento: consecutivo/boleta de la liquidación.
                SELECT
                    convert(varchar(50), E.ID_ERROR) as Id_Error,
                    isnull(rtrim(E.DESCRIPCION), '') as Descripcion,
                    convert(varchar(50), isnull(ER.ID_ERROR, '')) as Asignado,
                    isnull(ER.APLICADO, 'N') as Aplicado,
                    isnull(rtrim(E.MENSAJE), '') as Mensaje,
                    convert(varchar(50), isnull(ER.LINEA_ERR, '')) as Linea_Err
                FROM sif_Omisiones E
                LEFT JOIN SIF_OMISIONESG ER
                    ON E.ID_ERROR = ER.ID_ERROR
                    AND ER.cedula = @Cedula
                    AND ER.Modulo = 'LIQ'
                    AND ER.Codigo = @Cedula
                    AND ER.Documento = @Documento
                WHERE E.ACTIVO = '1'
                  AND E.ID_ERROR IN
                  (
                      SELECT ID_ERROR
                      FROM SIF_OMISIONES_MODULOS
                      WHERE cod_modulo = 'LIQ'
                  )
                ORDER BY E.ID_ERROR";

            var cedulaFiltro = Cedula.Trim();

            return DbHelper.ExecuteListQuery<AfLiquidacionRevisionOmisionModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                new
                {
                    Cedula = cedulaFiltro,
                    Documento = Documento.Trim(),
                });
        }

        /// <summary>
        /// Obtiene el mensaje/aviso de la etiqueta seleccionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="TagCodigo"></param>
        /// <returns></returns>
        public ErrorDto<AfLiquidacionRevisionAvisoModel?> AF_LiquidacionRevision_Aviso_Obtener(
            int CodEmpresa,
            string TagCodigo)
        {
            const string sqlQuery = @"
                -- Aviso de etiqueta (frmAF_LiquidacionRevision).
                -- @TagCodigo: código de la etiqueta seleccionada.
                SELECT
                    isnull(MENSAJE, '') as Mensaje
                FROM SIF_TAGS_AVISOS
                WHERE TAG_CODIGO = @TagCodigo";

            return DbHelper.ExecuteSingleQuery<AfLiquidacionRevisionAvisoModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                default,
                new { TagCodigo = TagCodigo.Trim() });
        }

        /// <summary>
        /// Inserta una omisión no asignada en SIF_OMISIONESG y retorna LINEA_ERR.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<AfLiquidacionRevisionOmisionInsertarModel?> AF_LiquidacionRevision_Omision_Insertar(
            int CodEmpresa,
            AfLiquidacionRevisionOmisionInsertarRequest request)
        {
            const string sqlQuery = @"
                -- Inserta omisión asignada (frmAF_LiquidacionRevision / ItemCheck).
                -- @Cedula: cédula del socio.
                -- @Id_Error: código de la omisión.
                -- @Documento: consecutivo/boleta de la liquidación.
                -- @Usuario: usuario de sesión.
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM SIF_OMISIONESG
                    WHERE cedula = @Cedula
                      AND ID_ERROR = @Id_Error
                      AND MODULO = 'LIQ'
                      AND CODIGO = @Cedula
                      AND DOCUMENTO = @Documento
                )
                BEGIN
                    INSERT INTO SIF_OMISIONESG
                    (
                        cedula,
                        ID_ERROR,
                        MODULO,
                        CODIGO,
                        DOCUMENTO,
                        REGISTRO_FECHA,
                        REGISTRO_USUARIO
                    )
                    VALUES
                    (
                        @Cedula,
                        @Id_Error,
                        'LIQ',
                        @Cedula,
                        @Documento,
                        dbo.MyGetdate(),
                        @Usuario
                    )
                END

                SELECT TOP 1
                    convert(varchar(50), LINEA_ERR) as Linea_Err
                FROM SIF_OMISIONESG
                WHERE cedula = @Cedula
                  AND ID_ERROR = @Id_Error
                  AND MODULO = 'LIQ'
                  AND CODIGO = @Cedula
                  AND DOCUMENTO = @Documento
                ORDER BY LINEA_ERR DESC";

            var cedula = request.Cedula.Trim();

            return DbHelper.ExecuteSingleQuery<AfLiquidacionRevisionOmisionInsertarModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                default,
                new
                {
                    Cedula = cedula,
                    Id_Error = request.Id_Error.Trim(),
                    Documento = request.Documento.Trim(),
                    Usuario = request.Usuario.Trim(),
                });
        }

        /// <summary>
        /// Elimina una omisión asignada por LINEA_ERR.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_LiquidacionRevision_Omision_Eliminar(
            int CodEmpresa,
            AfLiquidacionRevisionOmisionEliminarRequest request)
        {
            const string sqlQuery = @"
                -- Elimina omisión asignada (frmAF_LiquidacionRevision / ItemUncheck).
                -- @Linea_Err: identidad de SIF_OMISIONESG.
                DELETE FROM SIF_OMISIONESG
                WHERE LINEA_ERR = @Linea_Err";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                new { Linea_Err = request.Linea_Err.Trim() });
        }

        /// <summary>
        /// Marca como aplicadas las omisiones asignadas de la liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_LiquidacionRevision_Omisiones_Aplicar(
            int CodEmpresa,
            AfLiquidacionRevisionOmisionesAplicarRequest request)
        {
            const string sqlQuery = @"
                -- Aplica omisiones seleccionadas (frmAF_LiquidacionRevision / Aplicar).
                -- @Cedula: cédula del socio.
                -- @Documento: consecutivo/boleta de la liquidación.
                UPDATE SIF_OMISIONESG
                SET APLICADO = 'S'
                WHERE cedula = @Cedula
                  AND MODULO = 'LIQ'
                  AND CODIGO = @Cedula
                  AND DOCUMENTO = @Documento";

            var cedula = request.Cedula.Trim();

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                new
                {
                    Cedula = cedula,
                    Documento = request.Documento.Trim(),
                });
        }

        /// <summary>
        /// Registra la etiqueta de revisión y marca las omisiones como aplicadas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_LiquidacionRevision_Aplicar(
            int CodEmpresa,
            AfLiquidacionRevisionAplicarRequest request)
        {
            const string sqlOmisiones = @"
                -- Aplica omisiones seleccionadas (frmAF_LiquidacionRevision / sbAplicarErrores).
                -- @Cedula: cédula del socio.
                -- @Documento: consecutivo/boleta de la liquidación.
                UPDATE SIF_OMISIONESG
                SET APLICADO = 'S'
                WHERE cedula = @Cedula
                  AND MODULO = 'LIQ'
                  AND CODIGO = @Cedula
                  AND DOCUMENTO = @Documento";

            var cedula = request.Cedula.Trim();
            var documento = request.Documento.Trim();

            var resp = DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                using var transaction = connection.BeginTransaction();

                try
                {
                    connection.Execute(
                        "spSIFRegistraTags",
                        new
                        {
                            Codigo = cedula,
                            Tag = request.Tag.Trim(),
                            Usuario = request.Usuario.Trim(),
                            Notas = request.Observacion ?? string.Empty,
                            Documento = documento,
                            Modulo = "LIQ",
                            Llave_01 = documento,
                            Llave_02 = string.Empty,
                            Llave_03 = string.Empty,
                        },
                        transaction,
                        commandType: CommandType.StoredProcedure);

                    connection.Execute(
                        sqlOmisiones,
                        new
                        {
                            Cedula = cedula,
                            Documento = documento,
                        },
                        transaction);

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });

            if (resp.Code == -1)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = resp.Description,
                };
            }

            return DbHelper.CreateOkResponse();
        }
    }
}
