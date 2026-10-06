using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public sealed class FrmInvUnidadesConvDb
    {
        private const int CodigoValidacion = -2;

        private const string MensajeEmpresaRequerida =
            "El c&oacute;digo de la empresa es requerido.";

        private const string MensajeUnidadRequerida =
            "El c&oacute;digo de la unidad es requerido.";

        private const string MensajeUnidadDestinoRequerida =
            "El c&oacute;digo de la unidad equivalente es requerido.";

        private const string MensajeSolicitudRequerida =
            "La informaci&oacute;n de la conversi&oacute;n es requerida.";

        private const string MensajeUnidadesIguales =
            "La unidad equivalente debe ser diferente de la unidad base.";

        private const string MensajeFactorInvalido =
            "El factor de conversi&oacute;n debe ser mayor que cero.";

        private const string MensajeConversionNoEncontrada =
            "No se encontr&oacute; la conversi&oacute;n de unidades indicada.";

        private const string MensajeUnidadesError =
            "Ocurri&oacute; un error al consultar las unidades de medida.";

        private const string MensajeConversionesError =
            "Ocurri&oacute; un error al consultar las conversiones de unidades.";

        private const string MensajeGuardarError =
            "Ocurri&oacute; un error al guardar la conversi&oacute;n de unidades.";

        private const string MensajeEliminarError =
            "Ocurri&oacute; un error al eliminar la conversi&oacute;n de unidades.";

        private const string MensajeGuardarExito =
            "Conversi&oacute;n de unidades guardada correctamente.";

        private const string MensajeEliminarExito =
            "Conversi&oacute;n de unidades eliminada correctamente.";

        private readonly PortalDB _portalDb;

        public FrmInvUnidadesConvDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las unidades de medida disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de unidades de medida.</returns>
        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            INV_UnidadesConv_Unidades_Obtener(
                int CodEmpresa)
        {
            var lista =
                new List<
                    DropDownListaGenericaModel<string>>();

            if (CodEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion,
                    lista);
            }

            const string query = """
                SELECT
                    RTRIM(COD_UNIDAD) AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM PV_UNIDADES
                ORDER BY COD_UNIDAD;
                """;

            var resultado =
                DbHelper.ExecuteListQuery<
                    DropDownListaGenericaModel<string>>(
                        _portalDb,
                        CodEmpresa,
                        query);

            return INV_UnidadesConv_Lista_Resultado_Procesar(
                resultado,
                MensajeUnidadesError);
        }

        /// <summary>
        /// Obtiene las conversiones registradas para una unidad base.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodUnidad">Código de la unidad base.</param>
        /// <returns>Listado de conversiones de la unidad.</returns>
        public ErrorDto<UnidadesConvLista>
            INV_UnidadesConv_Lista_Obtener(
                int CodEmpresa,
                string CodUnidad)
        {
            var listaVacia =
                INV_UnidadesConv_Lista_Vacia_Crear();

            if (CodEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion,
                    listaVacia);
            }

            string codUnidad =
                CodUnidad?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(codUnidad))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeUnidadRequerida,
                    CodigoValidacion,
                    listaVacia);
            }

            const string query = """
                SELECT
                    RTRIM(COD_UNIDAD) AS cod_unidad,
                    RTRIM(COD_UNIDAD_D) AS cod_unidad_d,
                    ISNULL(FACTOR, 0) AS factor
                FROM PV_UNIDADES_CONV
                WHERE COD_UNIDAD = @CodUnidad;
                """;

            var resultado =
                DbHelper.ExecuteListQuery<
                    UnidadMedicionConvData>(
                        _portalDb,
                        CodEmpresa,
                        query,
                        new
                        {
                            CodUnidad = codUnidad
                        });

            if (resultado.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    INV_UnidadesConv_Error_Descripcion_Crear(
                        MensajeConversionesError,
                        resultado.Description),
                    resultado.Code.GetValueOrDefault(-1),
                    listaVacia);
            }

            var conversiones =
                resultado.Result ??
                new List<UnidadMedicionConvData>();

            return DbHelper.CreateOkResponse(
                new UnidadesConvLista
                {
                    total = conversiones.Count,
                    lista = conversiones
                });
        }

        /// <summary>
        /// Registra o actualiza una conversión de unidades.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="equivalencia">Información de la conversión.</param>
        /// <returns>Resultado del guardado.</returns>
        public ErrorDto INV_UnidadesConv_Guardar(
            int CodEmpresa,
            UnidadMedicionConvData? equivalencia)
        {
            ErrorDto? validacion =
                INV_UnidadesConv_Equivalencia_Validar(
                    CodEmpresa,
                    equivalencia);

            if (validacion is not null)
            {
                return validacion;
            }

            var equivalenciaValidada = equivalencia!;
            string codUnidad =
                equivalenciaValidada.cod_unidad.Trim();

            string codUnidadDestino =
                equivalenciaValidada.cod_unidad_d.Trim();

            const string query = """
                IF EXISTS
                (
                    SELECT 1
                    FROM PV_UNIDADES_CONV
                    WHERE COD_UNIDAD = @CodUnidad
                      AND COD_UNIDAD_D = @CodUnidadDestino
                )
                BEGIN
                    UPDATE PV_UNIDADES_CONV
                    SET FACTOR = @Factor
                    WHERE COD_UNIDAD = @CodUnidad
                      AND COD_UNIDAD_D = @CodUnidadDestino;
                END
                ELSE
                BEGIN
                    INSERT INTO PV_UNIDADES_CONV
                    (
                        COD_UNIDAD,
                        COD_UNIDAD_D,
                        FACTOR
                    )
                    VALUES
                    (
                        @CodUnidad,
                        @CodUnidadDestino,
                        @Factor
                    );
                END;
                """;

            var resultado =
                DbHelper.ExecuteNonQuery(
                    _portalDb,
                    CodEmpresa,
                    query,
                    new
                    {
                        CodUnidad = codUnidad,
                        CodUnidadDestino =
                            codUnidadDestino,
                        Factor = equivalencia.factor
                    });

            return INV_UnidadesConv_NonQuery_Resultado_Procesar(
                resultado,
                MensajeGuardarExito,
                MensajeGuardarError);
        }

        /// <summary>
        /// Elimina una conversión de unidades.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodUnidad">Código de la unidad base.</param>
        /// <param name="CodUnidadDestino">Código de la unidad equivalente.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto INV_UnidadesConv_Eliminar(
            int CodEmpresa,
            string CodUnidad,
            string CodUnidadDestino)
        {
            ErrorDto? validacion =
                INV_UnidadesConv_Codigos_Validar(
                    CodEmpresa,
                    CodUnidad,
                    CodUnidadDestino);

            if (validacion is not null)
            {
                return validacion;
            }

            const string query = """
                DELETE FROM PV_UNIDADES_CONV
                WHERE COD_UNIDAD = @CodUnidad
                  AND COD_UNIDAD_D = @CodUnidadDestino;
                """;

            var resultado =
                DbHelper.ExecuteNonQueryWithResult(
                    _portalDb,
                    CodEmpresa,
                    query,
                    new
                    {
                        CodUnidad = CodUnidad.Trim(),
                        CodUnidadDestino =
                            CodUnidadDestino.Trim()
                    });

            if (resultado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    INV_UnidadesConv_Error_Descripcion_Crear(
                        MensajeEliminarError,
                        resultado.Description),
                    resultado.Code.GetValueOrDefault(-1));
            }

            if (resultado.Result <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeConversionNoEncontrada,
                    CodigoValidacion);
            }

            return DbHelper.OkResponse(
                MensajeEliminarExito);
        }

        private static ErrorDto?
            INV_UnidadesConv_Equivalencia_Validar(
                int CodEmpresa,
                UnidadMedicionConvData? equivalencia)
        {
            if (CodEmpresa <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion);
            }

            if (equivalencia is null)
            {
                return DbHelper.ErrorResponse(
                    MensajeSolicitudRequerida,
                    CodigoValidacion);
            }

            ErrorDto? validacion =
                INV_UnidadesConv_Codigos_Validar(
                    CodEmpresa,
                    equivalencia.cod_unidad,
                    equivalencia.cod_unidad_d);

            if (validacion is not null)
            {
                return validacion;
            }

            if (string.Equals(
                equivalencia.cod_unidad.Trim(),
                equivalencia.cod_unidad_d.Trim(),
                StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.ErrorResponse(
                    MensajeUnidadesIguales,
                    CodigoValidacion);
            }

            if (equivalencia.factor <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeFactorInvalido,
                    CodigoValidacion);
            }

            return null;
        }

        private static ErrorDto?
            INV_UnidadesConv_Codigos_Validar(
                int CodEmpresa,
                string? CodUnidad,
                string? CodUnidadDestino)
        {
            if (CodEmpresa <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(CodUnidad))
            {
                return DbHelper.ErrorResponse(
                    MensajeUnidadRequerida,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                CodUnidadDestino))
            {
                return DbHelper.ErrorResponse(
                    MensajeUnidadDestinoRequerida,
                    CodigoValidacion);
            }

            return null;
        }

        private static ErrorDto<List<T>>
            INV_UnidadesConv_Lista_Resultado_Procesar<T>(
                ErrorDto<List<T>> resultado,
                string mensajeError)
        {
            if (resultado.Code == 0)
            {
                return DbHelper.CreateOkResponse(
                    resultado.Result ??
                    new List<T>());
            }

            return DbHelper.CreateErrorResponse(
                INV_UnidadesConv_Error_Descripcion_Crear(
                    mensajeError,
                    resultado.Description),
                resultado.Code.GetValueOrDefault(-1),
                new List<T>());
        }

        private static ErrorDto
            INV_UnidadesConv_NonQuery_Resultado_Procesar(
                ErrorDto resultado,
                string mensajeExito,
                string mensajeError)
        {
            if (resultado.Code == 0)
            {
                return DbHelper.OkResponse(
                    mensajeExito);
            }

            return DbHelper.ErrorResponse(
                INV_UnidadesConv_Error_Descripcion_Crear(
                    mensajeError,
                    resultado.Description),
                resultado.Code.GetValueOrDefault(-1));
        }

        private static UnidadesConvLista
            INV_UnidadesConv_Lista_Vacia_Crear()
        {
            return new UnidadesConvLista
            {
                total = 0,
                lista =
                    new List<UnidadMedicionConvData>()
            };
        }

        private static string
            INV_UnidadesConv_Error_Descripcion_Crear(
                string mensaje,
                string? detalle)
        {
            return string.IsNullOrWhiteSpace(detalle)
                ? mensaje
                : $"{mensaje} {detalle}";
        }
    }
}
