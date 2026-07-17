namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    internal static class FrmTesAutorizacionSql
    {
        public const string SQL_UPDATE_EMISION =
     @"UPDATE Tes_Transacciones 
            SET Autoriza='S',
                Fecha_Autorizacion = dbo.MyGetdate(), 
                User_Autoriza = @usuario,
                ESTADO_SINPE = @estado_sinpe,
                TIPO_GIROSINPE = @tipo_giro_sinpe,
                USUARIO_AUTORIZA_ESPECIAL = @usuarioEspecial
          WHERE Nsolicitud = @nsolicitud";

        public const string SQL_UPDATE_FIRMAS =
            @"UPDATE Tes_Transacciones 
            SET FIRMAS_AUTORIZA_FECHA = dbo.MyGetdate(),
                FIRMAS_AUTORIZA_USUARIO = @usuario
          WHERE Nsolicitud = @nsolicitud";

        public const string SQL_TES_AUTORIZACIONES_RANGOS = @"
SELECT rango_gen_Inicio, rango_gen_corte, firmas_gen_inicio, firmas_gen_corte
FROM TES_AUTORIZACIONES
WHERE NOMBRE = @usuario";

        public const string SQL_BITACORA_EMISION = "EXEC spTesBitacora @nsolicitud,'02','',@usuario";
        public const string SQL_BITACORA_FIRMAS = "EXEC spTesBitacora @nsolicitud,'04','',@usuario";

        public const string SQL_AUTORIZACION_LOTE = @"
            DECLARE @nsolicitud INT;
            DECLARE solicitudes_cursor CURSOR LOCAL FAST_FORWARD FOR
                SELECT TRY_CONVERT(INT, [value])
                FROM OPENJSON(@solicitudesJson)
                WHERE TRY_CONVERT(INT, [value]) IS NOT NULL;

            OPEN solicitudes_cursor;
            FETCH NEXT FROM solicitudes_cursor INTO @nsolicitud;

            WHILE @@FETCH_STATUS = 0
            BEGIN
                BEGIN TRY
                    IF @tipoAutorizacion = 0
                    BEGIN
                        UPDATE Tes_Transacciones
                        SET Autoriza = 'S',
                            Fecha_Autorizacion = dbo.MyGetdate(),
                            User_Autoriza = @usuario,
                            ESTADO_SINPE = @estadoSinpe,
                            TIPO_GIROSINPE = @tipoGiroSinpe,
                            USUARIO_AUTORIZA_ESPECIAL = @usuarioEspecial
                        WHERE Nsolicitud = @nsolicitud;

                        EXEC spTesBitacora @nsolicitud, '02', '', @usuario;
                    END
                    ELSE
                    BEGIN
                        UPDATE Tes_Transacciones
                        SET FIRMAS_AUTORIZA_FECHA = dbo.MyGetdate(),
                            FIRMAS_AUTORIZA_USUARIO = @usuario
                        WHERE Nsolicitud = @nsolicitud;

                        EXEC spTesBitacora @nsolicitud, '04', '', @usuario;
                    END;
                END TRY
                BEGIN CATCH
                    -- Se conserva el comportamiento actual: una solicitud con error
                    -- no detiene el resto del lote.
                    PRINT CONCAT('Solicitud no autorizada: ', @nsolicitud);
                END CATCH;

                FETCH NEXT FROM solicitudes_cursor INTO @nsolicitud;
            END;

            CLOSE solicitudes_cursor;
            DEALLOCATE solicitudes_cursor;";

        public const string SP_TRANSACCIONES_PENDIENTES = @"
                SELECT
                        T.nsolicitud, T.codigo, T.beneficiario, T.monto, T.fecha_solicitud, T.cta_Ahorros,
                        CASE WHEN @Duplicados = 1
                             THEN dbo.fxTesSupervisa(CODIGO,BENEFICIARIO,monto,0,'T')
                             ELSE 0
                        END AS duplicado,
                        dbo.fxTes_Cuenta_Verifica(T.id_banco,T.codigo,T.cta_ahorros) AS Cta_Verifica,
                        T.Detalle1 + T.detalle2 AS Detalle, ISNULL(T.cod_App,'') AS AppId,
                        IIF(T.user_hold IS NULL, 0, 1) AS Bloqueo, S.ESTADOACTUAL
                    FROM Tes_Transacciones T
                    LEFT JOIN Tes_Bancos B ON T.id_banco = B.id_banco
                    LEFT JOIN Socios S
                        ON T.CODIGO = S.CEDULA
                    WHERE T.estado = 'P'
                      AND B.id_banco = @Banco
                      AND T.Tipo = @TipoDoc

                      -- Fechas
                      AND (
                            @TodasFechas = 1
                            OR (T.fecha_solicitud BETWEEN @FechaInicio AND @FechaFin)
                          )

                      -- Solicitudes
                      AND (
                            @TodasSolicitudes = 1
                            OR (T.nsolicitud >= @SolicitudInicio AND T.nsolicitud <= @SolicitudCorte)
                          )

                      -- Bloqueo
                      AND (
                            @IncluirBloqueados = 1
                            OR T.fecha_hold IS NULL
                          )
                    ";

        public const string SP_TRANSACCIONES_PENDIENTES_OLD = @"
                SELECT 
                        T.nsolicitud, T.codigo, T.beneficiario, T.monto, T.fecha_solicitud, T.cta_Ahorros,
                        CASE WHEN @Duplicados = 1
                             THEN dbo.fxTesSupervisa(CODIGO,BENEFICIARIO,monto,0,'T')
                             ELSE 0
                        END AS duplicado,
                        dbo.fxTes_Cuenta_Verifica(T.id_banco,T.codigo,T.cta_ahorros) AS Cta_Verifica,
                        T.Detalle1 + T.detalle2 AS Detalle, ISNULL(T.cod_App,'') AS AppId,
                        IIF(T.user_hold IS NULL, 0, 1) AS Bloqueo, S.ESTADOACTUAL
                    FROM Tes_Transacciones T 
                    LEFT JOIN Tes_Bancos B ON T.id_banco = B.id_banco
                    LEFT JOIN Socios S 
                        ON SUBSTRING(REPLACE(, '-', ''),
                                     PATINDEX('%[^0]%', REPLACE(T.CODIGO, '-', '')),
                                     LEN(REPLACE(T.CODIGO, '-', '')))
                        =
                           SUBSTRING(REPLACE(S.CEDULA, '-', ''),
                                     PATINDEX('%[^0]%', REPLACE(S.CEDULA, '-', '')),
                                     LEN(REPLACE(S.CEDULA, '-', '')))
                    WHERE T.estado = 'P'
                      AND B.id_banco = @Banco
                      AND T.Tipo = @TipoDoc

                      -- Fechas
                      AND (
                            @TodasFechas = 1
                            OR (T.fecha_solicitud BETWEEN @FechaInicio AND @FechaFin)
                          )

                      -- Solicitudes
                      AND (
                            @TodasSolicitudes = 1
                            OR (T.nsolicitud >= @SolicitudInicio AND T.nsolicitud <= @SolicitudCorte)
                          )

                      -- Bloqueo
                      AND (
                            @IncluirBloqueados = 1
                            OR T.fecha_hold IS NULL
                          )
                    ";

        // SIN FILTRO
        public const string QueryTransac_Base = @"
Select TOP (@top) *
From Tes_Transacciones
Where Estado = 'P' And Tipo = @tipoDoc
  And ID_Banco= @banco And Autoriza='S' and fecha_hold is null
Order by Nsolicitud";

        public const string BaseQuery_Base = @"
(SELECT TOP (@top) nsolicitud
 FROM Tes_Transacciones
 WHERE Estado = 'P' AND Tipo = @tipoDoc
   AND ID_Banco = @banco AND Autoriza = 'S' AND fecha_hold IS NULL
 Order by Nsolicitud)";

        // POR SOLICITUDES
        public const string QueryTransac_Solicitudes = @"
Select TOP (@top) *
From Tes_Transacciones
Where Estado = 'P' And Tipo = @tipoDoc
  And ID_Banco= @banco And Autoriza='S' and fecha_hold is null
  And NSolicitud Between @minimo And @maximo
Order by Nsolicitud";

        // POR FECHAS
        public const string QueryTransac_Fechas = @"
Select TOP (@top) *
From Tes_Transacciones
Where Estado = 'P' And Tipo = @tipoDoc
  And ID_Banco= @banco And Autoriza='S' and fecha_hold is null
  And Fecha_Solicitud Between @fechaInicio And @fechaCorte
Order by Nsolicitud";


        public const string Query_UpdateTransacciones = @"
UPDATE Tes_Transacciones
SET Estado = 'T',
    Fecha_Emision = @fechaEmision,
    Ubicacion_Actual = 'T',
    FECHA_TRASLADO = @fechaEmision,
    NDocumento = @nDocumento,
    user_genera = @usuario,
    documento_base = @bancoConsec,
    COD_PLAN = @plan
WHERE NSolicitud = @nSolicitud;";


        public const string Query_ConteoPendientes = @"
            SELECT COUNT(T.nsolicitud) 
            FROM Tes_Transacciones T 
            INNER JOIN Tes_Bancos B ON T.id_banco = B.id_banco
            WHERE T.estado = 'P' AND B.id_banco = @Banco AND T.Tipo = @TipoDoc";

        public const string Query_Interbancaria = @"
            SELECT Bg.LCTA_INTERBANCARIA 
            FROM TES_BANCOS Tb 
            INNER JOIN TES_BANCOS_GRUPOS Bg ON Tb.COD_GRUPO = Bg.COD_GRUPO
            WHERE Tb.ID_BANCO = @Banco";

        public const string Query_Autorizaciones = @"Select * From Tes_Autorizaciones Where Clave = @clave and nombre = @usuario and estado = 'A'";

    }
}
