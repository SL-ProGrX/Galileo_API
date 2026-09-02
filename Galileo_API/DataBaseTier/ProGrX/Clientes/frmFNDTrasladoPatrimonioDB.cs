namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public partial class FrmFndTrasladoPatrimonioDB
    {
        private readonly IConfiguration _config;
        private const string TipoRecibo = "FRE";
        private const string TipoNotaCredito = "FNC";
        private const string TipoNotaDebito = "FND";
        private const string ExisteNoEncontrado = "NE";

        private const string SqlPlanesPatrimonio = @"
                    SELECT cod_plan AS [Plan],
                           descripcion AS Descripcion
                    FROM dbo.fnd_planes
                    WHERE cod_operadora = @IdOperadora
                      AND Patrimonio_Enlace = 1
                    ORDER BY cod_plan;";

        private const string SqlPlanDetalle = @"
                    SELECT Descripcion,
                           Patrimonio_Tipo,
                           CASE
                               WHEN Patrimonio_Tipo = 'P' THEN 'Aporte Patronal'
                               WHEN Patrimonio_Tipo = 'O' THEN 'Aporte Obrero'
                               WHEN Patrimonio_Tipo = 'C' THEN 'Capitalización'
                               ELSE 'No Identificado'
                           END AS Patrimonio,
                           LTRIM(RTRIM(cod_moneda)) AS Divisa,
                           LTRIM(RTRIM(cuenta_conta)) AS Cuenta_FND
                    FROM dbo.Fnd_Planes
                    WHERE Cod_Operadora = @IdOperadora
                      AND Cod_Plan = @CodPlan
                      AND Patrimonio_Enlace = 1;";

        private const string SqlContratosPatrimonio = @"
                    SELECT @Marcado AS [Marcado],
                           C.COD_CONTRATO,
                           C.CEDULA,
                           S.NOMBRE,
                           E.descripcion AS EstadoPersona,
                           (C.APORTES + C.RENDIMIENTO) AS MONTO,
                           S.EstadoActual
                    FROM dbo.fnd_contratos C
                    INNER JOIN dbo.Fnd_Planes P
                        ON C.cod_OPERADORA = P.COD_OPERADORA
                       AND C.COD_PLAN = P.COD_PLAN
                    INNER JOIN dbo.Socios S
                        ON C.cedula = S.cedula
                    INNER JOIN dbo.AHORRO_CONSOLIDADO A
                        ON S.cedula = A.cedula
                       AND A.COD_DIVISA = P.COD_MONEDA
                    INNER JOIN dbo.AFI_ESTADOS_PERSONA E
                        ON S.estadoActual = E.cod_Estado
                    WHERE C.cod_operadora = @IdOperadora
                      AND C.cod_plan = @CodPlan
                      AND C.estado = 'A'
                      AND (C.aportes + C.rendimiento) > 0
                      AND (@FiltroEstado = 0 OR S.estadoActual = 'S')
                      AND (@FiltroPatronal = 0 OR S.estadoActual IN ('A','S'))
                    ORDER BY C.cedula;";

        private const string SpDocsConsecutivo = "spSIFDocsConsecutivo";
        private const string SpDocsAsiento = "spSIFDocsAsiento";

        private const string SqlAseReciboSelect = "SELECT CS_RECIBO AS Consecutivo FROM dbo.ase_consecutivos;";
        private const string SqlAseReciboUpdate = "UPDATE dbo.ase_consecutivos SET CS_RECIBO = CS_RECIBO + 1;";
        private const string SqlAseDepositoSelect = "SELECT CS_DEPOSITO AS Consecutivo FROM dbo.ase_consecutivos;";
        private const string SqlAseDepositoUpdate = "UPDATE dbo.ase_consecutivos SET CS_DEPOSITO = CS_DEPOSITO + 1;";
        private const string SqlAseNotaDebitoSelect = "SELECT CS_NOTA_DEBITO AS Consecutivo FROM dbo.ase_consecutivos;";
        private const string SqlAseNotaDebitoUpdate = "UPDATE dbo.ase_consecutivos SET CS_NOTA_DEBITO = CS_NOTA_DEBITO + 1;";
        private const string SqlAseNotaCreditoSelect = "SELECT CS_NOTA_CREDITO AS Consecutivo FROM dbo.ase_consecutivos;";
        private const string SqlAseNotaCreditoUpdate = "UPDATE dbo.ase_consecutivos SET CS_NOTA_CREDITO = CS_NOTA_CREDITO + 1;";

        private const string SqlContratoDetalleInsert = @"
                    INSERT INTO dbo.fnd_contratos_detalle
                    (
                        cod_operadora,
                        cod_plan,
                        cod_contrato,
                        monto,
                        fecha,
                        fecha_proceso,
                        tcon,
                        ncon,
                        usuario,
                        cod_concepto,
                        cod_caja
                    )
                    VALUES
                    (
                        @CodOperadora,
                        @CodPlan,
                        @CodContrato,
                        @Monto,
                        dbo.MyGetdate(),
                        @FechaProceso,
                        @Tcon,
                        @Ncon,
                        @Usuario,
                        'FND003',
                        ''
                    );";

        private const string SqlContratoUpdateAportes = @"
                    UPDATE dbo.fnd_contratos
                    SET aportes = 0,
                        rendimiento = 0
                    WHERE cod_operadora = @CodOperadora
                      AND cod_plan = @CodPlan
                      AND cod_contrato = @CodContrato;";

        private const string SqlDocumentoInsert = @"
                    INSERT INTO dbo.fnd_documentos
                    (
                        tipo,
                        id_documento,
                        cod_operadora,
                        cliente,
                        concepto,
                        fecha,
                        monto,
                        usuario,
                        detalle1,
                        detalle2,
                        detalle3,
                        detalle4,
                        detalle,
                        dp
                    )
                    VALUES
                    (
                        @Tipo,
                        @IdDocumento,
                        @CodOperadora,
                        @Cliente,
                        @Concepto,
                        dbo.MyGetdate(),
                        @Monto,
                        @Usuario,
                        @Detalle1,
                        @Detalle2,
                        @Detalle3,
                        @Detalle4,
                        @Detalle,
                        @Dp
                    );";

        private const string SqlAsientoInsert = @"
                    INSERT INTO dbo.fnd_asientos
                    (
                        Cod_Operadora,
                        Tipo,
                        Id_documento,
                        Fnd_Cuenta,
                        Fnd_Monto,
                        Fnd_Debehaber
                    )
                    VALUES
                    (
                        @CodOperadora,
                        @Tipo,
                        @IdDocumento,
                        @FndCuenta,
                        @FndMonto,
                        @FndDebehaber
                    );";

        private const string SqlSifTransaccionInsert = @"
                    INSERT INTO dbo.SIF_TRANSACCIONES
                    (
                        COD_TRANSACCION,
                        TIPO_DOCUMENTO,
                        REGISTRO_FECHA,
                        REGISTRO_USUARIO,
                        CLIENTE_IDENTIFICACION,
                        CLIENTE_NOMBRE,
                        cod_concepto,
                        monto,
                        estado,
                        Referencia_01,
                        Referencia_02,
                        Referencia_03,
                        cod_oficina,
                        linea1,
                        linea2,
                        linea3,
                        linea4,
                        detalle,
                        documento
                    )
                    VALUES
                    (
                        @CodTransaccion,
                        @TipoDocumento,
                        dbo.MyGetdate(),
                        @RegistroUsuario,
                        @ClienteIdentificacion,
                        @ClienteNombre,
                        @CodConcepto,
                        @Monto,
                        @Estado,
                        @Referencia01,
                        @Referencia02,
                        @Referencia03,
                        @CodOficina,
                        @Linea1,
                        @Linea2,
                        @Linea3,
                        @Linea4,
                        @Detalle,
                        @Documento
                    );";

        private const string SqlSocioDetalle = @"
                    SELECT S.cedula,
                           S.estadoactual,
                           ABS(D.monto) AS Monto,
                           ISNULL(A.cedula, 'NE') AS Existe
                    FROM dbo.Socios S
                    INNER JOIN dbo.fnd_contratos C
                        ON S.cedula = C.cedula
                    INNER JOIN dbo.fnd_contratos_detalle D
                        ON C.cod_operadora = D.cod_operadora
                       AND C.cod_plan = D.cod_plan
                       AND C.cod_contrato = D.cod_contrato
                    LEFT JOIN dbo.Ahorro_Consolidado A
                        ON S.cedula = A.cedula
                    WHERE D.Tcon = @Tcon
                      AND D.Ncon = @Ncon;";

        private const string SqlAhorroObreroUpdate = "UPDATE dbo.ahorro_consolidado SET ahorro = ahorro + @Monto WHERE cedula = @Cedula;";
        private const string SqlAhorroObreroInsert = "INSERT INTO dbo.ahorro_consolidado(cedula, ahorro, aporte, capitaliza, custodia, extra) VALUES (@Cedula, @Monto, 0, 0, 0, 0);";
        private const string SqlAportePatronalUpdate = "UPDATE dbo.ahorro_consolidado SET aporte = aporte + @Monto WHERE cedula = @Cedula;";
        private const string SqlAportePatronalInsert = "INSERT INTO dbo.ahorro_consolidado(cedula, ahorro, aporte, capitaliza, custodia, extra) VALUES (@Cedula, 0, @Monto, 0, 0, 0);";
        private const string SqlCustodiaUpdate = "UPDATE dbo.ahorro_consolidado SET custodia = ISNULL(custodia,0) + @Monto WHERE cedula = @Cedula;";
        private const string SqlCustodiaInsert = "INSERT INTO dbo.ahorro_consolidado(cedula, ahorro, aporte, capitaliza, custodia, extra) VALUES (@Cedula, 0, 0, 0, @Monto, 0);";
        private const string SqlCapitalizaUpdate = "UPDATE dbo.ahorro_consolidado SET capitaliza = capitaliza + @Monto WHERE cedula = @Cedula;";
        private const string SqlCapitalizaInsert = "INSERT INTO dbo.ahorro_consolidado(cedula, ahorro, aporte, capitaliza, custodia, extra) VALUES (@Cedula, 0, 0, @Monto, 0, 0);";

        private const string SqlAhorroDetalladoInsert = @"
                    INSERT INTO dbo.ahorro_detallado
                    (
                        cedula,
                        tipo,
                        monto,
                        fecha,
                        fechaproc,
                        estado,
                        numcom,
                        Tcon,
                        Ncon,
                        usuario,
                        cod_Caja,
                        cod_concepto
                    )
                    VALUES
                    (
                        @Cedula,
                        @Tipo,
                        @Monto,
                        dbo.MyGetdate(),
                        @FechaProc,
                        'A',
                        @NumCom,
                        @Tcon,
                        @Ncon,
                        @Usuario,
                        '',
                        @Concepto
                    );";

        private const string SqlSifTransaccionPatrimonioInsert = @"
                    INSERT INTO dbo.SIF_TRANSACCIONES
                    (
                        COD_TRANSACCION,
                        TIPO_DOCUMENTO,
                        REGISTRO_FECHA,
                        REGISTRO_USUARIO,
                        Cliente_IDENTIFICACION,
                        CLIENTE_NOMBRE,
                        cod_concepto,
                        monto,
                        estado,
                        Referencia_01,
                        Referencia_02,
                        cod_oficina,
                        linea1,
                        linea2,
                        linea3,
                        detalle,
                        documento
                    )
                    VALUES
                    (
                        @NC_Pat,
                        @TipoDoc,
                        dbo.MyGetdate(),
                        @Usuario,
                        '',
                        'APLICACION GENERAL',
                        @Concepto,
                        0,
                        'P',
                        @Operadora,
                        @Plan,
                        @OficinaTitular,
                        @Linea1,
                        @Linea2,
                        @Linea3,
                        @Detalle,
                        @Documento
                    );";

        private const string SqlAhorroDetalladoResumen = @"
                    SELECT SUM(A.monto) AS Monto,
                           A.tipo AS Tipo,
                           S.estadoactual AS EstadoActual
                    FROM dbo.ahorro_detallado A
                    INNER JOIN dbo.socios S
                        ON A.cedula = S.cedula
                    WHERE A.tcon = @TipoDoc
                      AND A.Ncon = @NC_Pat
                    GROUP BY A.tipo, S.estadoactual;";

        private const string SqlParAfahCuentas = @"
                    SELECT cta_custodia AS Cta_Custodia,
                           cta_obrero AS Cta_Obrero,
                           cta_patronal AS Cta_Patronal,
                           cta_capitaliza AS Cta_Capitaliza,
                           cta_devoluciones AS Cta_Devoluciones
                    FROM dbo.par_afah;";

        public FrmFndTrasladoPatrimonioDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mProGrx = new MProGrxMain(config);
        }

        private readonly MProGrxMain _mProGrx;
    }
}