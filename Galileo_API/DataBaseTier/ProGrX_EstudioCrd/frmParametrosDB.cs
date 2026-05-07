using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmParametrosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmParametrosDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmParametrosDb(PortalDB portalDb, MSecurityMainDb bitacora)
        {
            _portalDb = portalDb;
            _bitacora = bitacora;
        }

        /// <summary>
        /// Obtiene parametros
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<ParametrosOtrosData?> Parametros_ObtenerOtros(int codEmpresa)
        {
            const string query = @"SELECT TOP 1 * FROM Pra_Parametros;";

            return DbHelper.ExecuteSingleQuery<ParametrosOtrosData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                null);
        }

        /// <summary>
        /// Guarda parametros
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Parametros_GuardarOtros(
            int codEmpresa, string usuario, ParametrosOtrosGuardarRequest request)
        {
            const string sql = @"
                IF EXISTS (SELECT 1 FROM Pra_Parametros)
                BEGIN
                    UPDATE Pra_Parametros
                    SET
                        Meses_Transcurridos = @MesesTranscurridos,
                        Porc_Fiduciarios = @PorcFiduciarios,
                        Porc_Cancelado = @PorcCancelado,
                        ACTIVAR_SGT = @ActivarSgt
                END
                ELSE
                BEGIN
                    INSERT INTO Pra_Parametros
                    (
                        Meses_Transcurridos,
                        Porc_Fiduciarios,
                        Porc_Cancelado,
                        ACTIVAR_SGT
                    )
                    VALUES
                    (
                        @MesesTranscurridos,
                        @PorcFiduciarios,
                        @PorcCancelado,
                        @ActivarSgt
                    )
                END;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    MesesTranscurridos = request.meses_transcurridos,
                    PorcFiduciarios = request.porc_fiduciarios,
                    PorcCancelado = request.porc_cancelado,
                    ActivarSgt = request.activar_sgt ? 1 : 0
                });

            if (resp.Code < 0)
            {
                return resp;
            }

            RegistrarBitacora(codEmpresa, usuario, "Modifica", "Modifico Parametros De PreAnalisis");

            return new ErrorDto
            {
                Code = 0,
                Description = "Registro actualizado"
            };
        }

        /// <summary>
        /// Obtiene lista de codigos por garantia
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="garantia"></param>
        /// <param name="orden"></param>
        /// <returns></returns>
        public ErrorDto<List<ParametrosCodigoData>> Parametros_ObtenerCodigos(
            int codEmpresa, string garantia, string orden)
        {
            var filtroGarantia = ObtenerFiltroCatalogo(garantia);
            if (string.IsNullOrWhiteSpace(filtroGarantia))
            {
                return ErrorResultado<List<ParametrosCodigoData>>("Garantía inválida.");
            }

            var orderBy = NormalizarTexto(orden).ToLower() == "descripcion"
                ? "C.Descripcion"
                : "C.Codigo";

            var query = $@"
                SELECT
                    RTRIM(ISNULL(C.Codigo, '')) AS codigo,
                    RTRIM(ISNULL(C.Descripcion, '')) AS descripcion,
                    CAST(CASE WHEN PC.Codigo IS NULL THEN 0 ELSE 1 END AS bit) AS [checked]
                FROM Catalogo AS C
                LEFT JOIN Pra_Codigos AS PC
                    ON PC.Codigo = C.Codigo
                    AND PC.Garantia = @Garantia
                WHERE {filtroGarantia}
                  AND C.Retencion = 'N'
                  AND C.Poliza = 'N'
                ORDER BY {orderBy};";

            return DbHelper.ExecuteListQuery<ParametrosCodigoData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Garantia = NormalizarGarantia(garantia)
                });
        }

        /// <summary>
        /// Actualiza estado de un codigo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Parametros_ActualizarCodigo(
            int codEmpresa, string usuario, ParametrosCodigoActualizarRequest request)
        {
            var garantia = NormalizarGarantia(request.garantia);
            if (string.IsNullOrWhiteSpace(garantia) || string.IsNullOrWhiteSpace(request.codigo))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Datos inválidos para actualizar el código."
                };
            }

            var sql = request.@checked
                ? @"
                    IF NOT EXISTS (
                        SELECT 1
                        FROM Pra_Codigos
                        WHERE Garantia = @Garantia
                          AND Codigo = @Codigo
                    )
                    BEGIN
                        INSERT INTO Pra_Codigos (Garantia, Codigo)
                        VALUES (@Garantia, @Codigo)
                    END;"
                : @"
                    DELETE FROM Pra_Codigos
                    WHERE Garantia = @Garantia
                      AND Codigo = @Codigo;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Garantia = garantia,
                    Codigo = NormalizarTexto(request.codigo)
                });

            if (resp.Code < 0)
            {
                return resp;
            }

            RegistrarBitacora(
                codEmpresa,
                usuario,
                request.@checked ? "Registra" : "Borra",
                $"{(request.@checked ? "Registra" : "Borra")} Codigo {request.codigo.Trim()} Bajo Garantia {ObtenerDescripcionGarantia(garantia)}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = "Informacion registrada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Obtiene lista de membresias por garantia
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="garantia"></param>
        /// <returns></returns>
        public ErrorDto<List<ParametrosMembresiaData>> Parametros_ObtenerMembresias(
            int codEmpresa,
            string garantia)
        {
            var garantiaNormalizada = NormalizarGarantia(garantia);
            if (string.IsNullOrWhiteSpace(garantiaNormalizada))
            {
                return ErrorResultado<List<ParametrosMembresiaData>>("Garantía inválida.");
            }

            const string query = @"
                SELECT
                    Desde AS desde,
                    Hasta AS hasta,
                    Monto AS monto
                FROM Pra_Membresias
                WHERE Garantia = @Garantia
                ORDER BY Desde;";

            return DbHelper.ExecuteListQuery<ParametrosMembresiaData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Garantia = garantiaNormalizada
                });
        }

        /// <summary>
        /// Guarda lista de membresias por garantia
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Parametros_GuardarMembresias(
            int codEmpresa,
            string usuario,
            ParametrosMembresiasGuardarRequest request)
        {
            var garantia = NormalizarGarantia(request.garantia);
            if (string.IsNullOrWhiteSpace(garantia))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Garantía inválida."
                };
            }

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                "DELETE FROM Pra_Membresias WHERE Garantia = @Garantia;",
                new
                {
                    Garantia = garantia
                });

            if (respDelete.Code < 0)
            {
                return respDelete;
            }

            foreach (var item in request.membresias ?? new List<ParametrosMembresiaData>())
            {
                if (item.desde == null || item.hasta == null || item.monto == null)
                {
                    continue;
                }

                var respInsert = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    @"
                        INSERT INTO Pra_Membresias (Garantia, Desde, Hasta, Monto)
                        VALUES (@Garantia, @Desde, @Hasta, @Monto);",
                    new
                    {
                        Garantia = garantia,
                        Desde = item.desde,
                        Hasta = item.hasta,
                        Monto = item.monto
                    });

                if (respInsert.Code < 0)
                {
                    return respInsert;
                }
            }

            RegistrarBitacora(
                codEmpresa,
                usuario,
                "Modifica",
                $"Modifico Tabla de Membresia Bajo Garantia {ObtenerDescripcionGarantia(garantia)}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = "Tabla actualizada"
            };
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string NormalizarTexto(string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }

        private static string NormalizarGarantia(string? garantia)
        {
            return NormalizarTexto(garantia).ToUpper();
        }

        private static string ObtenerFiltroCatalogo(string garantia)
        {
            return NormalizarGarantia(garantia) switch
            {
                "F" => "C.GAR_FIADORES = 'S'",
                "V" => "C.GAR_HIPOTECA = 'S'",
                "E" => "C.GAR_NO = 'N'",
                "S" => "C.GAR_NO = 'S'",
                _ => string.Empty
            };
        }

        private static string ObtenerDescripcionGarantia(string garantia)
        {
            return NormalizarGarantia(garantia) switch
            {
                "F" => "Fiduciaria",
                "V" => "Vivienda",
                "E" => "Especial",
                "S" => "Sin Garantía",
                _ => string.Empty
            };
        }

        private static ErrorDto<T> ErrorResultado<T>(string descripcion)
        {
            return new ErrorDto<T>
            {
                Code = -1,
                Description = descripcion,
                Result = default
            };
        }
    }
}