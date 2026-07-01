using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCatalogoCreditosDb
    {

        /// <summary>
        /// Obtiene los rangos base de monto, plazo y garantias de la linea.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<CrCatalogoCreditoRangosBaseData> CrCatalogoCreditos_RangosBase_Obtener(int codEmpresa, string codigo)
        {
            codigo = codigo.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<CrCatalogoCreditoRangosBaseData>
                {
                    Code = -1,
                    Description = "Debe consultar una linea de credito."
                };
            }

            const string rangosQuery = @"
                SELECT
                    consec,
                    ISNULL(de, 0) AS de,
                    ISNULL(hasta, 0) AS hasta,
                    ISNULL(plazo, 0) AS plazo,
                    ISNULL(intc_soc, 0) AS intc_soc,
                    ISNULL(intm_soc, 0) AS intm_soc,
                    ISNULL(intc_nsoc, 0) AS intc_nsoc,
                    ISNULL(intm_nsoc, 0) AS intm_nsoc
                FROM Rangos
                WHERE codigo = @Codigo
                ORDER BY consec;";

            const string plazosQuery = @"
                SELECT
                    consec,
                    ISNULL(desde, 0) AS desde,
                    ISNULL(hasta, 0) AS hasta,
                    ISNULL(tasa, 0) AS tasa
                FROM Rangos_plazo
                WHERE codigo = @Codigo
                ORDER BY consec;";

            const string garantiasQuery = @"
                SELECT
                    G.garantia,
                    ISNULL(G.descripcion, '') AS descripcion,
                    ISNULL(A.utiliza_tasa_garantia, 0) AS utiliza_tasa_garantia,
                    ISNULL(A.tasa_garantia, 0) AS tasa_garantia,
                    ISNULL(A.utiliza_tasa_piso, 0) AS utiliza_tasa_piso,
                    ISNULL(A.tasa_piso, 0) AS tasa_piso,
                    ISNULL(A.utiliza_tasa_techo, 0) AS utiliza_tasa_techo,
                    ISNULL(A.tasa_techo, 0) AS tasa_techo,
                    ISNULL(A.utiliza_maximos, 0) AS utiliza_maximos,
                    ISNULL(A.max_monto, 0) AS max_monto,
                    ISNULL(A.liquidez_minima, 0) AS liquidez_minima
                FROM crd_garantia_Tipos G
                INNER JOIN crd_catalogo_garantias A
                    ON G.garantia = A.garantia
                WHERE A.codigo = @Codigo
                ORDER BY G.garantia;";

            var parametros = new { Codigo = codigo };
            var rangos = DbHelper.ExecuteListQuery<CrCatalogoCreditoRangoBaseData>(_portalDb, codEmpresa, rangosQuery, parametros);
            if (rangos.Code < 0) return ErrorRangosBase(rangos.Description);

            var plazos = DbHelper.ExecuteListQuery<CrCatalogoCreditoRangoPlazoData>(_portalDb, codEmpresa, plazosQuery, parametros);
            if (plazos.Code < 0) return ErrorRangosBase(plazos.Description);

            var garantias = DbHelper.ExecuteListQuery<CrCatalogoCreditoRangoGarantiaData>(_portalDb, codEmpresa, garantiasQuery, parametros);
            if (garantias.Code < 0) return ErrorRangosBase(garantias.Description);

            return new ErrorDto<CrCatalogoCreditoRangosBaseData>
            {
                Code = 0,
                Description = "OK",
                Result = new CrCatalogoCreditoRangosBaseData
                {
                    rangos = rangos.Result ?? [],
                    tasasPlazos = plazos.Result ?? [],
                    garantias = garantias.Result ?? []
                }
            };
        }


        /// <summary>
        /// Guarda un rango base por monto.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int> CrCatalogoCreditos_RangoBase_Guardar(int codEmpresa, CrCatalogoCreditoRangoBaseGuardarRequest request)
        {
            NormalizarRangoRequest(request);
            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return new ErrorDto<int> { Code = -1, Description = "Debe consultar una linea de credito." };
            }

            const string query = @"
                IF ISNULL(@Consec, 0) = 0
                BEGIN
                    INSERT INTO Rangos(codigo, de, hasta, plazo, intc_soc, intm_soc, intc_nsoc, intm_nsoc)
                    VALUES(@Codigo, @De, @Hasta, @Plazo, @IntcSoc, @IntmSoc, @IntcNsoc, @IntmNsoc);

                    SELECT ISNULL(MAX(consec), 0)
                    FROM Rangos
                    WHERE codigo = @Codigo;
                END
                ELSE
                BEGIN
                    UPDATE Rangos
                    SET de = @De,
                        hasta = @Hasta,
                        plazo = @Plazo,
                        intc_soc = @IntcSoc,
                        intm_soc = @IntmSoc,
                        intc_nsoc = @IntcNsoc,
                        intm_nsoc = @IntmNsoc
                    WHERE consec = @Consec;

                    SELECT @Consec;
                END";

            var respuesta = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                query,
                0,
                new
                {
                    Codigo = request.codigo,
                    Consec = request.rango.consec,
                    De = request.rango.de,
                    Hasta = request.rango.hasta,
                    Plazo = request.rango.plazo,
                    IntcSoc = request.rango.intc_soc,
                    IntmSoc = request.rango.intm_soc,
                    IntcNsoc = request.rango.intc_nsoc,
                    IntmNsoc = request.rango.intm_nsoc
                });

            if (respuesta.Code >= 0)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    request.rango.consec == 0 ? "Registra - WEB" : "Modifica - WEB",
                    $"Rango para el Codigo: {request.codigo} ID:{respuesta.Result}");
            }

            return respuesta;
        }


        /// <summary>
        /// Guarda un rango de tasa por plazo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int> CrCatalogoCreditos_RangoPlazo_Guardar(int codEmpresa, CrCatalogoCreditoRangoPlazoGuardarRequest request)
        {
            NormalizarRangoRequest(request);
            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return new ErrorDto<int> { Code = -1, Description = "Debe consultar una linea de credito." };
            }

            const string query = @"
                IF ISNULL(@Consec, 0) = 0
                BEGIN
                    INSERT INTO Rangos_Plazo(codigo, desde, hasta, tasa)
                    VALUES(@Codigo, @Desde, @Hasta, @Tasa);

                    SELECT ISNULL(MAX(consec), 0)
                    FROM Rangos_Plazo
                    WHERE codigo = @Codigo;
                END
                ELSE
                BEGIN
                    UPDATE Rangos_Plazo
                    SET desde = @Desde,
                        hasta = @Hasta,
                        tasa = @Tasa
                    WHERE consec = @Consec;

                    SELECT @Consec;
                END";

            var respuesta = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                query,
                0,
                new
                {
                    Codigo = request.codigo,
                    Consec = request.rango.consec,
                    Desde = request.rango.desde,
                    Hasta = request.rango.hasta,
                    Tasa = request.rango.tasa
                });

            if (respuesta.Code >= 0)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    request.rango.consec == 0 ? "Registra - WEB" : "Modifica - WEB",
                    $"Rango Plazo para el Codigo: {request.codigo} ID:{respuesta.Result}");
            }

            return respuesta;
        }


        /// <summary>
        /// Guarda la configuracion de tasas y maximos por garantia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoCreditos_RangoGarantia_Guardar(int codEmpresa, CrCatalogoCreditoRangoGarantiaGuardarRequest request)
        {
            NormalizarRangoGarantiaRequest(request);
            if (string.IsNullOrWhiteSpace(request.codigo) || string.IsNullOrWhiteSpace(request.garantia.garantia))
            {
                return new ErrorDto { Code = -1, Description = "Debe indicar la linea y la garantia." };
            }

            const string query = @"
                UPDATE crd_catalogo_garantias
                SET utiliza_tasa_garantia = @UtilizaTasaGarantia,
                    tasa_garantia = @TasaGarantia,
                    utiliza_tasa_piso = @UtilizaTasaPiso,
                    tasa_piso = @TasaPiso,
                    utiliza_tasa_techo = @UtilizaTasaTecho,
                    tasa_techo = @TasaTecho,
                    utiliza_maximos = @UtilizaMaximos,
                    max_monto = @MaxMonto,
                    liquidez_minima = @LiquidezMinima,
                    actualiza_fecha = dbo.MyGetdate(),
                    actualiza_usuario = @Usuario
                WHERE codigo = @Codigo
                    AND garantia = @Garantia;";

            var respuesta = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Codigo = request.codigo,
                    Garantia = request.garantia.garantia,
                    UtilizaTasaGarantia = request.garantia.utiliza_tasa_garantia ? 1 : 0,
                    TasaGarantia = request.garantia.tasa_garantia,
                    UtilizaTasaPiso = request.garantia.utiliza_tasa_piso ? 1 : 0,
                    TasaPiso = request.garantia.tasa_piso,
                    UtilizaTasaTecho = request.garantia.utiliza_tasa_techo ? 1 : 0,
                    TasaTecho = request.garantia.tasa_techo,
                    UtilizaMaximos = request.garantia.utiliza_maximos ? 1 : 0,
                    MaxMonto = request.garantia.max_monto,
                    LiquidezMinima = request.garantia.liquidez_minima,
                    Usuario = request.usuario
                });

            if (respuesta.Code >= 0)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    "Modifica - WEB",
                    $"Garantia: {request.garantia.descripcion} Linea: {request.codigo}");
            }

            return respuesta;
        }
    }
}
