using Galileo.DataBaseTier;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCategoriasCreditoDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private const int ModuloBitacora = 3;

        public FrmCrCategoriasCreditoDb(
            IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene el catálogo de probabilidades
        /// de incumplimiento o default.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<
            CrCategoriasCreditoProbabilidadDefaultData>>
            CR_frmCR_Categorias_Credito_ProbabilidadDefault_Obtener(
                int codEmpresa)
        {
            const string sql = """
                select
                    ID_PROBABILIDAD_DEF
                        as id_probabilidad_def,
                    rtrim(DESCRIPCION)
                        as descripcion,
                    rtrim(CATEGORIA)
                        as categoria,
                    VALOR_INICIAL
                        as valor_inicial,
                    VALOR_FINAL
                        as valor_final,
                    rtrim(
                        isnull(
                            USUARIO_REGISTRA,
                            ''
                        )
                    ) as usuario_registra,
                    FEC_REGISTRA
                        as fec_registra,
                    rtrim(
                        isnull(
                            USUARIO_MODIFICA,
                            ''
                        )
                    ) as usuario_modifica,
                    FEC_MODIFICA
                        as fec_modifica
                from CRD_CATALOGO_PROBABILIDAD_DEF
                order by
                    ID_PROBABILIDAD_DEF
                """;

            return DbHelper.ExecuteListQuery<
                CrCategoriasCreditoProbabilidadDefaultData>(
                    _portalDb,
                    codEmpresa,
                    sql);
        }

        /// <summary>
        /// Obtiene el catálogo de probabilidades de mora.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<
            CrCategoriasCreditoProbabilidadMoraData>>
            CR_frmCR_Categorias_Credito_ProbabilidadMora_Obtener(
                int codEmpresa)
        {
            const string sql = """
                select
                    ID_PROBABILIDAD_MORA
                        as id_probabilidad_mora,
                    rtrim(DESCRIPCION)
                        as descripcion,
                    rtrim(TIPO_MORA)
                        as tipo_mora,
                    PORC_PROBABILIDAD
                        as porc_probabilidad,
                    rtrim(
                        isnull(
                            USUARIO_REGISTRA,
                            ''
                        )
                    ) as usuario_registra,
                    FEC_REGISTRA
                        as fec_registra,
                    rtrim(
                        isnull(
                            USUARIO_MODIFICA,
                            ''
                        )
                    ) as usuario_modifica,
                    FEC_MODIFICA
                        as fec_modifica
                from CRD_CATALOGO_PROBABILIDAD_MORA
                order by
                    ID_PROBABILIDAD_MORA
                """;

            return DbHelper.ExecuteListQuery<
                CrCategoriasCreditoProbabilidadMoraData>(
                    _portalDb,
                    codEmpresa,
                    sql);
        }

        /// <summary>
        /// Obtiene los segmentos configurados
        /// para probabilidad de crédito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<
            CrCategoriasCreditoSegmentoData>>
            CR_frmCR_Categorias_Credito_Segmentos_Obtener(
                int codEmpresa)
        {
            const string sql = """
                select
                    ID_SEGMENTO
                        as id_segmento,
                    rtrim(COD_SEGMENTO)
                        as cod_segmento,
                    rtrim(DESCRIPCION)
                        as descripcion,
                    PORC_SEGMENTO
                        as porc_segmento,
                    rtrim(
                        isnull(
                            USUARIO_REGISTRA,
                            ''
                        )
                    ) as usuario_registra,
                    FEC_REGISTRA
                        as fec_registra,
                    rtrim(
                        isnull(
                            USUARIO_MODIFICA,
                            ''
                        )
                    ) as usuario_modifica,
                    FEC_MODIFICA
                        as fec_modifica
                from CRD_SEGMENTOS_PROBABILIDAD
                order by
                    ID_SEGMENTO
                """;

            return DbHelper.ExecuteListQuery<
                CrCategoriasCreditoSegmentoData>(
                    _portalDb,
                    codEmpresa,
                    sql);
        }

        /// <summary>
        /// Obtiene los registros de la carga de probabilidades.
        /// </summary>
        public ErrorDto<List<CrCategoriasCreditoCargaProbabilidadData>>
            CR_frmCR_Categorias_Credito_CargaProbabilidades_Obtener(
                int codEmpresa)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    codEmpresa);

                const string sql = """
                    SELECT
                        COD_SEGMENTO AS segmento,
                        MES_PERIODO AS mes,
                        METRICA AS probabilidad,
                        FEC_REGISTRA AS fec_registra,
                        RTRIM(ISNULL(USUARIO_REGISTRA, '')) AS usuario_registra
                    FROM CRD_CATALOGO_PROBABILIDAD_DEF_CARGA
                    ORDER BY COD_SEGMENTO, MES_PERIODO;
                    """;

                var registros = connection.Query<
                    CrCategoriasCreditoCargaProbabilidadData>(sql).ToList();

                return new ErrorDto<List<
                    CrCategoriasCreditoCargaProbabilidadData>>
                {
                    Code = 0,
                    Description = string.Empty,
                    Result = registros
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<List<
                    CrCategoriasCreditoCargaProbabilidadData>>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = []
                };
            }
        }

        /// <summary>
        /// Registra las filas del archivo y ejecuta la actualización final.
        /// </summary>
        public ErrorDto
            CR_frmCR_Categorias_Credito_CargaProbabilidades_Procesar(
                int codEmpresa,
                string usuario,
                CrCategoriasCreditoCargaProbabilidadRequest request)
        {
            request.usuario = usuario;
            var validacion = ValidarCarga(request);

            if (validacion != null)
            {
                return validacion;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    codEmpresa);
                using var transaction = connection.BeginTransaction();

                connection.Execute(
                    "DELETE FROM CRD_CATALOGO_PROBABILIDAD_DEF_CARGA;",
                    transaction: transaction);

                const string insertar = """
                    INSERT INTO CRD_CATALOGO_PROBABILIDAD_DEF_CARGA(
                        COD_SEGMENTO,
                        MES_PERIODO,
                        METRICA,
                        USUARIO_REGISTRA,
                        FEC_REGISTRA
                    ) VALUES (
                        @segmento,
                        @mes,
                        @probabilidad,
                        @Usuario,
                        GETDATE()
                    );
                    """;

                foreach (var registro in request.registros)
                {
                    connection.Execute(
                        insertar,
                        new
                        {
                            registro.segmento,
                            registro.mes,
                            registro.probabilidad,
                            Usuario = usuario
                        },
                        transaction);
                }

                transaction.Commit();

                RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    request.nombre_archivo,
                    request.registros.Count);

                return new ErrorDto
                {
                    Code = 0,
                    Description =
                        $"Carga procesada satisfactoriamente. Registros: {request.registros.Count}."
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        public ErrorDto CR_frmCR_Categorias_Credito_Guardar(
            int codEmpresa,
            string usuario,
            string tipo,
            CrCategoriasCreditoCrudRequest request)
        {
            var tipoNormalizado = NormalizarTipo(tipo);
            if (tipoNormalizado == null)
            {
                return ErrorValidacion("El tipo de catálogo no es válido.");
            }

            var validacion = ValidarCrud(tipoNormalizado, request);
            if (validacion != null)
            {
                return validacion;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    codEmpresa);

                var sql = ObtenerSqlGuardar(tipoNormalizado, request.es_nuevo);
                var parametros = new
                {
                    request.id,
                    Descripcion = request.descripcion.Trim(),
                    Categoria = request.categoria.Trim().ToUpperInvariant(),
                    request.valor_inicial,
                    request.valor_final,
                    request.porc_probabilidad,
                    CodSegmento = request.cod_segmento.Trim().ToUpperInvariant(),
                    request.porc_segmento,
                    Usuario = usuario
                };
                var idRegistro = request.id;
                var filas = 1;

                if (request.es_nuevo)
                {
                    idRegistro = connection.ExecuteScalar<int>(sql, parametros);
                }
                else
                {
                    filas = connection.Execute(sql, parametros);
                }

                if (!request.es_nuevo && filas == 0)
                {
                    return ErrorValidacion("El registro indicado ya no existe.");
                }

                RegistrarBitacoraCrud(
                    codEmpresa,
                    usuario,
                    request.es_nuevo ? "Registra - WEB" : "Modifica - WEB",
                    tipoNormalizado,
                    idRegistro);

                return new ErrorDto
                {
                    Code = 0,
                    Description = request.es_nuevo
                        ? "Información registrada satisfactoriamente."
                        : "Información actualizada satisfactoriamente."
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        public ErrorDto CR_frmCR_Categorias_Credito_Eliminar(
            int codEmpresa,
            string usuario,
            string tipo,
            int id)
        {
            var tipoNormalizado = NormalizarTipo(tipo);
            if (tipoNormalizado == null || id <= 0)
            {
                return ErrorValidacion("El registro indicado no es válido.");
            }

            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    codEmpresa);

                var sql = tipoNormalizado switch
                {
                    "P" => "DELETE FROM CRD_CATALOGO_PROBABILIDAD_DEF WHERE ID_PROBABILIDAD_DEF = @Id;",
                    "M" => "DELETE FROM CRD_CATALOGO_PROBABILIDAD_MORA WHERE ID_PROBABILIDAD_MORA = @Id;",
                    _ => "DELETE FROM CRD_SEGMENTOS_PROBABILIDAD WHERE ID_SEGMENTO = @Id;"
                };

                var filas = connection.Execute(sql, new { Id = id });
                if (filas == 0)
                {
                    return ErrorValidacion("El registro indicado ya no existe.");
                }

                RegistrarBitacoraCrud(
                    codEmpresa,
                    usuario,
                    "Elimina - WEB",
                    tipoNormalizado,
                    id);

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Información eliminada satisfactoriamente."
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        private static string? NormalizarTipo(string tipo)
        {
            var valor = (tipo ?? string.Empty).Trim().ToUpperInvariant();
            return valor is "P" or "M" or "S" ? valor : null;
        }

        private static ErrorDto? ValidarCrud(
            string tipo,
            CrCategoriasCreditoCrudRequest request)
        {
            request.descripcion = (request.descripcion ?? string.Empty).Trim();
            request.categoria = (request.categoria ?? string.Empty).Trim();
            request.cod_segmento = (request.cod_segmento ?? string.Empty).Trim();

            if (!request.es_nuevo && request.id <= 0)
            {
                return ErrorValidacion("El registro indicado no es válido.");
            }

            if (string.IsNullOrWhiteSpace(request.descripcion))
            {
                return ErrorValidacion("La descripción es requerida.");
            }

            return tipo switch
            {
                "P" => ValidarProbabilidadDefault(request),
                "M" => ValidarProbabilidadMora(request),
                "S" => ValidarSegmento(request),
                _ => null
            };
        }

        private static ErrorDto? ValidarProbabilidadDefault(
            CrCategoriasCreditoCrudRequest request)
        {
            if (request.descripcion.Length > 250)
            {
                return ErrorValidacion(
                    "La descripción no puede superar 250 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(request.categoria) ||
                request.categoria.Length > 5)
            {
                return ErrorValidacion(
                    "La categoría es requerida y permite hasta 5 caracteres.");
            }

            var rangoInvalido = request.valor_inicial < 0 ||
                request.valor_final < 0 ||
                request.valor_inicial > 999.9m ||
                request.valor_final > 999.9m ||
                request.valor_inicial > request.valor_final;

            return rangoInvalido
                ? ErrorValidacion(
                    "El porcentaje mínimo debe ser menor o igual al porcentaje máximo.")
                : null;
        }

        private static ErrorDto? ValidarProbabilidadMora(
            CrCategoriasCreditoCrudRequest request)
        {
            var datosInvalidos = request.descripcion.Length > 150 ||
                request.porc_probabilidad < 0 ||
                request.porc_probabilidad > 999.99m;

            return datosInvalidos
                ? ErrorValidacion(
                    "Verifique la descripción y el porcentaje de probabilidad.")
                : null;
        }

        private static ErrorDto? ValidarSegmento(
            CrCategoriasCreditoCrudRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.cod_segmento) ||
                request.cod_segmento.Length > 10)
            {
                return ErrorValidacion(
                    "El código de segmento es requerido y permite hasta 10 caracteres.");
            }

            var datosInvalidos = request.descripcion.Length > 150 ||
                request.porc_segmento < 0 ||
                request.porc_segmento >= 100m;

            return datosInvalidos
                ? ErrorValidacion(
                    "Verifique la descripción y el porcentaje del segmento.")
                : null;
        }

        private static string ObtenerSqlGuardar(string tipo, bool nuevo)
        {
            return (tipo, nuevo) switch
            {
                ("P", true) => """
                    INSERT INTO CRD_CATALOGO_PROBABILIDAD_DEF(
                        DESCRIPCION, CATEGORIA, VALOR_INICIAL, VALOR_FINAL,
                        USUARIO_REGISTRA, FEC_REGISTRA
                    ) VALUES (
                        @Descripcion, @Categoria, @valor_inicial, @valor_final,
                        @Usuario, GETDATE()
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS INT);
                    """,
                ("P", false) => """
                    UPDATE CRD_CATALOGO_PROBABILIDAD_DEF
                    SET DESCRIPCION = @Descripcion,
                        CATEGORIA = @Categoria,
                        VALOR_INICIAL = @valor_inicial,
                        VALOR_FINAL = @valor_final,
                        USUARIO_MODIFICA = @Usuario,
                        FEC_MODIFICA = GETDATE()
                    WHERE ID_PROBABILIDAD_DEF = @id;
                    """,
                ("M", true) => """
                    INSERT INTO CRD_CATALOGO_PROBABILIDAD_MORA(
                        TIPO_MORA, DESCRIPCION, PORC_PROBABILIDAD,
                        USUARIO_REGISTRA, FEC_REGISTRA
                    ) VALUES (
                        (SELECT ISNULL(MAX(TIPO_MORA), 0) + 1
                         FROM CRD_CATALOGO_PROBABILIDAD_MORA),
                        @Descripcion, @porc_probabilidad,
                        @Usuario, GETDATE()
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS INT);
                    """,
                ("M", false) => """
                    UPDATE CRD_CATALOGO_PROBABILIDAD_MORA
                    SET DESCRIPCION = @Descripcion,
                        PORC_PROBABILIDAD = @porc_probabilidad,
                        USUARIO_MODIFICA = @Usuario,
                        FEC_MODIFICA = GETDATE()
                    WHERE ID_PROBABILIDAD_MORA = @id;
                    """,
                ("S", true) => """
                    INSERT INTO CRD_SEGMENTOS_PROBABILIDAD(
                        COD_SEGMENTO, DESCRIPCION, PORC_SEGMENTO,
                        USUARIO_REGISTRA, FEC_REGISTRA
                    ) VALUES (
                        @CodSegmento, @Descripcion, @porc_segmento,
                        @Usuario, GETDATE()
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS INT);
                    """,
                _ => """
                    UPDATE CRD_SEGMENTOS_PROBABILIDAD
                    SET COD_SEGMENTO = @CodSegmento,
                        DESCRIPCION = @Descripcion,
                        PORC_SEGMENTO = @porc_segmento,
                        USUARIO_MODIFICA = @Usuario,
                        FEC_MODIFICA = GETDATE()
                    WHERE ID_SEGMENTO = @id;
                    """
            };
        }

        private void RegistrarBitacoraCrud(
            int codEmpresa,
            string usuario,
            string movimiento,
            string tipo,
            int id)
        {
            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                Movimiento = movimiento,
                DetalleMovimiento =
                    $"Categorías crediticias - Catálogo {tipo}, registro: {id}.",
                Modulo = ModuloBitacora
            });
        }

        private static ErrorDto? ValidarCarga(
            CrCategoriasCreditoCargaProbabilidadRequest? request)
        {
            if (request?.registros == null || request.registros.Count == 0)
            {
                return ErrorValidacion(
                    "El archivo no contiene registros para procesar.");
            }

            request.usuario = (request.usuario ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
            request.nombre_archivo = (request.nombre_archivo ?? string.Empty)
                .Trim();

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return ErrorValidacion(
                    "No se encontró el usuario que realiza la carga.");
            }

            var llaves = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            for (var indice = 0; indice < request.registros.Count; indice++)
            {
                var registro = request.registros[indice];
                var validacionRegistro = ValidarRegistroCarga(
                    registro,
                    indice + 2,
                    llaves);
                if (validacionRegistro != null)
                {
                    return validacionRegistro;
                }
            }

            return null;
        }

        private static ErrorDto? ValidarRegistroCarga(
            CrCategoriasCreditoCargaProbabilidadData registro,
            int fila,
            HashSet<string> llaves)
        {
            registro.segmento = (registro.segmento ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(registro.segmento))
            {
                return ErrorValidacion(
                    $"El segmento de la fila {fila} es obligatorio.");
            }

            if (registro.segmento.Length > 10)
            {
                return ErrorValidacion(
                    $"El segmento de la fila {fila} permite hasta 10 caracteres.");
            }

            if (registro.mes is < 1 or > 12)
            {
                return ErrorValidacion(
                    $"El mes de la fila {fila} debe estar entre 1 y 12.");
            }

            if (registro.probabilidad < 0 || registro.probabilidad >= 100m)
            {
                return ErrorValidacion(
                    $"La probabilidad de la fila {fila} debe estar entre 0 y 99.999999999999999.");
            }

            var llave =
                $"{registro.segmento.ToUpperInvariant()}|{registro.mes}";

            return llaves.Add(llave)
                ? null
                : ErrorValidacion(
                    $"El segmento {registro.segmento} y el mes " +
                    $"{registro.mes} están repetidos en el archivo.");
        }

        private static ErrorDto ErrorValidacion(string mensaje)
        {
            return new ErrorDto
            {
                Code = -2,
                Description = mensaje
            };
        }

        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string nombreArchivo,
            int cantidad)
        {
            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                Movimiento = "Carga - WEB",
                DetalleMovimiento =
                    "Categorías crediticias - Carga masiva de probabilidades. " +
                    $"Archivo: {nombreArchivo}. Registros: {cantidad}.",
                Modulo = ModuloBitacora
            });
        }
    }
}
