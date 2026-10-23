using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed class FrmIvrCatComisionesDb : InversionesCatalogoDbBase
    {
        private readonly MCntLinkDB _cntLinkDb;

        public FrmIvrCatComisionesDb(IConfiguration config)
            : base(
                config,
                "El código del tipo de comisión es requerido.",
                "Tipo de Comisiones:  ")
        {
            _cntLinkDb = new MCntLinkDB(config);
        }

        /// <summary>
        /// Obtiene el catálogo de tipos de comisiones ordenado por código.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa cuyos tipos de comisiones se consultan.
        /// </param>
        /// <returns>
        /// Lista de tipos de comisiones o el detalle del error de consulta.
        /// </returns>
        public ErrorDto<List<IvrCatComisionDto>> IVR_Cat_Comisiones_Lista_Obtener(
            int codEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(V.COD_COMISION) AS Cod_Comision,
                    RTRIM(ISNULL(V.DESCRIPCION, '')) AS Descripcion,
                    RTRIM(ISNULL(V.COD_CUENTA_MASK, '')) AS Cod_Cuenta_Mask,
                    RTRIM(ISNULL(V.COD_CUENTA_DESC, '')) AS Cod_Cuenta_Desc,
                    RTRIM(ISNULL(C.TIPO, '')) AS Tipo,
                    RTRIM(ISNULL(V.Tipo_Valor, '')) AS Tipo_Valor,
                    ISNULL(V.VALOR, 0) AS Valor,
                    RTRIM(ISNULL(C.APLICA_EN, '')) AS Aplica_En,
                    RTRIM(ISNULL(V.Aplica_Desc, '')) AS Aplica_Desc,
                    RTRIM(ISNULL(C.SUMAN_EN, '')) AS Suman_En,
                    RTRIM(ISNULL(V.Sumar_Desc, '')) AS Sumar_Desc,
                    CAST(ISNULL(V.ACTIVO, 0) AS bit) AS Activo,
                    V.REGISTRO_USUARIO AS Registro_Usuario,
                    V.REGISTRO_FECHA AS Registro_Fecha
                FROM vIVR_COMISIONES_TIPOS V
                INNER JOIN IVR_COMISIONES_TIPOS C ON C.COD_COMISION = V.COD_COMISION
                ORDER BY V.COD_COMISION;
                """;

            return DbHelper.ExecuteListQuery<IvrCatComisionDto>(
                _portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Registra un tipo de comisión nuevo y su movimiento en la bitácora de Inversiones.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa donde se registra el tipo de comisión.
        /// </param>
        /// <param name="request">
        /// Código, descripción, cuenta, tipo, valor, aplicación, suma, estado y usuario del registro.
        /// </param>
        /// <returns>
        /// Resultado del registro, validación de código duplicado o detalle del error.
        /// </returns>
        public ErrorDto IVR_Cat_Comisiones_Registrar(
            int codEmpresa,
            IvrCatComisionGuardarRequest? request)
        {
            const string sql = """
                INSERT INTO IVR_COMISIONES_TIPOS
                (
                    COD_COMISION, DESCRIPCION, COD_CUENTA, TIPO, VALOR,
                    APLICA_EN, SUMAN_EN, ACTIVO,
                    REGISTRO_USUARIO, REGISTRO_FECHA
                )
                SELECT
                    @cod_comision, @descripcion, @cod_cuenta, @tipo, @valor,
                    @aplica_en, @suman_en, @activo,
                    @usuario, dbo.MyGetdate()
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM IVR_COMISIONES_TIPOS
                    WHERE COD_COMISION = @cod_comision
                );
                """;

            return Guardar(codEmpresa, request, sql, true);
        }

        /// <summary>
        /// Actualiza los datos del tipo de comisión, conservando su código y datos de registro.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa donde se actualiza el tipo de comisión.
        /// </param>
        /// <param name="request">
        /// Código del tipo de comisión, datos que deben actualizarse y usuario del movimiento.
        /// </param>
        /// <returns>
        /// Resultado de la actualización, validación de existencia o detalle del error.
        /// </returns>
        public ErrorDto IVR_Cat_Comisiones_Actualizar(
            int codEmpresa,
            IvrCatComisionGuardarRequest? request)
        {
            const string sql = """
                UPDATE IVR_COMISIONES_TIPOS
                SET DESCRIPCION = @descripcion,
                    COD_CUENTA = @cod_cuenta,
                    TIPO = @tipo,
                    VALOR = @valor,
                    APLICA_EN = @aplica_en,
                    SUMAN_EN = @suman_en,
                    ACTIVO = @activo
                WHERE COD_COMISION = @cod_comision;
                """;

            return Guardar(codEmpresa, request, sql, false);
        }

        /// <summary>
        /// Elimina el tipo de comisión indicado y registra el movimiento en la bitácora de Inversiones.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa a la que pertenece el tipo de comisión.
        /// </param>
        /// <param name="codComision">
        /// Código del tipo de comisión que debe eliminarse.
        /// </param>
        /// <param name="usuario">
        /// Usuario que realiza la eliminación y se registra en la bitácora.
        /// </param>
        /// <returns>
        /// Resultado de la eliminación, validación de existencia o detalle del error.
        /// </returns>
        public ErrorDto IVR_Cat_Comisiones_Eliminar(
            int codEmpresa,
            string? codComision,
            string? usuario)
        {
            var validacion = ValidarIdentificacion(
                codComision, usuario);
            if (validacion != null)
            {
                return validacion;
            }

            const string sql = """
                DELETE FROM IVR_COMISIONES_TIPOS
                WHERE COD_COMISION = @cod_comision;
                """;

            var resultado = DbHelper.ExecuteNonQueryWithResult(
                _portalDb, codEmpresa, sql,
                new { cod_comision = codComision!.Trim() });

            return ResultadoMovimiento(
                codEmpresa, resultado, usuario!, "Elimina", codComision,
                "El tipo de comisión indicado no existe.");
        }

        private ErrorDto Guardar(
            int codEmpresa,
            IvrCatComisionGuardarRequest? request,
            string sql,
            bool nuevo)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(
                    "Los datos del tipo de comisión son requeridos.", CodigoMensajeControlado);
            }

            var req = request;
            var validacion = ValidarIdentificacion(
                req.Cod_Comision, req.Usuario);
            if (validacion != null)
            {
                return validacion;
            }

            if (req.Valor == null)
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar un valor numérico.", CodigoMensajeControlado);
            }

            var codigo = nuevo
                ? req.Cod_Comision.Trim().ToUpperInvariant()
                : req.Cod_Comision.Trim();

            var resultado = DbHelper.ExecuteNonQueryWithResult(
                _portalDb, codEmpresa, sql,
                new
                {
                    cod_comision = codigo,
                    descripcion = req.Descripcion ?? string.Empty,
                    cod_cuenta = _cntLinkDb.fxgCntCuentaFormato(
                        codEmpresa, false, req.Cod_Cuenta, 0),
                    tipo = req.Tipo?.Trim() ?? string.Empty,
                    valor = req.Valor.Value,
                    aplica_en = req.Aplica_En?.Trim() ?? string.Empty,
                    suman_en = req.Suman_En?.Trim().ToUpperInvariant() ?? string.Empty,
                    activo = req.Activo,
                    usuario = req.Usuario.Trim()
                });

            return ResultadoMovimiento(
                codEmpresa, resultado, req.Usuario,
                nuevo ? "Registra" : "Modifica", codigo,
                nuevo ? "El tipo de comisión indicado ya existe."
                    : "El tipo de comisión indicado no existe.");
        }
    }
}
