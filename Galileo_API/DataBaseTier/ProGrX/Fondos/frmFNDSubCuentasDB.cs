using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndSubCuentasDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 18; // Módulo de Fondo de Inversión
        private readonly MSecurityMainDb _Security_MainDB;
        private const string SqlActualizarCuotaContrato = @"
                    EXEC dbo.spFnd_SubCuentas_Maestro_Update
                        @Operadora,
                        @Plan,
                        @Contrato,
                        @Usuario;";

        private const string SqlSubCuentas = @"
                    SELECT
                        B.IdX AS idx,
                        B.cedula,
                        B.Nombre AS nombre,
                        B.parentesco,
                        B.fechaNac AS fechanac,
                        B.cuota,
                        B.APORTES AS aportes,
                        B.RENDIMIENTO AS rendimiento,
                        B.direccion,
                        B.notas,
                        B.telefono1,
                        B.telefono2,
                        B.email,
                        B.apto_postal,
                        B.cod_operadora,
                        B.cod_plan,
                        B.cod_contrato,
                        B.cod_beneficiario,
                        B.estado,
                        ISNULL(P.Descripcion, '') AS Parentesco_Desc
                    FROM dbo.FND_SubCUENTAS B
                    LEFT JOIN dbo.SYS_PARENTESCOS P
                        ON B.Parentesco = P.cod_Parentesco
                    WHERE B.cod_Operadora = @Operadora
                      AND B.cod_Plan = @Plan
                      AND B.cod_Contrato = @Contrato
                    ORDER BY B.IdX;";

        private const string SqlParentescos = @"
                    SELECT
                        RTRIM(cod_Parentesco) AS item,
                        RTRIM(Descripcion) AS descripcion
                    FROM dbo.sys_Parentescos
                    WHERE activo = 1
                    ORDER BY Descripcion;";

        private const string SqlExisteBeneficiario = @"
                    SELECT ISNULL(COUNT(1), 0)
                    FROM dbo.FND_SubCUENTAS
                    WHERE cedula = @Cedula
                      AND IDX <> @Idx
                      AND cod_Operadora = @Operadora
                      AND cod_Plan = @Plan
                      AND cod_Contrato = @Contrato;";

        private const string SqlLimiteSubCuentasPlan = @"
                    SELECT
                        ISNULL(P.SubCuentasMax, 0) AS Maximo,
                        COUNT(S.IdX) AS Registradas
                    FROM dbo.FND_Planes P
                    LEFT JOIN dbo.FND_SubCUENTAS S
                        ON S.cod_operadora = P.cod_operadora
                       AND S.cod_plan = P.cod_plan
                       AND S.cod_contrato = @Contrato
                    WHERE P.cod_operadora = @Operadora
                      AND P.cod_plan = @Plan
                    GROUP BY P.SubCuentasMax;";

        private const string SqlConfiguracionSubCuentasPlan = @"
                    SELECT
                        ISNULL(P.MONTO_MINIMO, 0) AS monto_minimo,
                        ISNULL(P.SubCuentasMax, 0) AS subcuentasmax,
                        CONVERT(bit, ISNULL(P.CUENTA_MAESTRA, 0)) AS cuenta_maestra,
                        (
                            SELECT COUNT(1)
                            FROM dbo.FND_SubCUENTAS S
                            WHERE S.cod_operadora = P.cod_operadora
                              AND S.cod_plan = P.cod_plan
                              AND S.cod_contrato = @Contrato
                        ) AS registradas
                    FROM dbo.FND_Planes P
                    WHERE P.cod_operadora = @Operadora
                      AND P.cod_plan = @Plan;";

        private const string SqlInsertSubCuenta = @"
                    INSERT INTO dbo.FND_SubCUENTAS
                    (
                        IdX,
                        cedula,
                        Nombre,
                        parentesco,
                        fechaNac,
                        cuota,
                        APORTES,
                        RENDIMIENTO,
                        direccion,
                        notas,
                        telefono1,
                        telefono2,
                        email,
                        apto_postal,
                        cod_operadora,
                        cod_plan,
                        cod_contrato,
                        cod_beneficiario,
                        estado
                    )
                    OUTPUT INSERTED.IdX
                    SELECT
                        ISNULL(MAX(S.IdX), 0) + 1,
                        @Cedula,
                        @Nombre,
                        @Parentesco,
                        @FechaNac,
                        @Cuota,
                        @Aportes,
                        @Rendimiento,
                        @Direccion,
                        @Notas,
                        @Telefono1,
                        @Telefono2,
                        @Email,
                        @AptoPostal,
                        @CodOperadora,
                        @CodPlan,
                        @CodContrato,
                        @CodBeneficiario,
                        @Estado
                    FROM dbo.FND_SubCUENTAS S WITH (UPDLOCK, HOLDLOCK)
                    WHERE S.cod_operadora = @CodOperadora
                      AND S.cod_plan = @CodPlan
                      AND S.cod_contrato = @CodContrato
                    HAVING COUNT(S.IdX) <
                    (
                        SELECT ISNULL(P.SubCuentasMax, 0)
                        FROM dbo.FND_Planes P
                        WHERE P.cod_operadora = @CodOperadora
                          AND P.cod_plan = @CodPlan
                    );";

        private const string SqlUpdateSubCuenta = @"
                    UPDATE dbo.FND_SubCUENTAS
                    SET Nombre = @Nombre,
                        CEDULA = @Cedula,
                        parentesco = @Parentesco,
                        notas = @Notas,
                        direccion = @Direccion,
                        apto_postal = @AptoPostal,
                        email = @Email,
                        telefono1 = @Telefono1,
                        telefono2 = @Telefono2,
                        fechaNac = @FechaNac,
                        cuota = @Cuota
                    WHERE IdX = @IdX
                      AND cod_operadora = @CodOperadora
                      AND cod_plan = @CodPlan
                      AND cod_contrato = @CodContrato;";

        private const string SqlSubCuentaPorIdx = @"
                    SELECT
                        IdX AS idx,
                        cedula,
                        Nombre AS nombre,
                        parentesco,
                        fechaNac AS fechanac,
                        cuota,
                        APORTES AS aportes,
                        RENDIMIENTO AS rendimiento,
                        direccion,
                        notas,
                        telefono1,
                        telefono2,
                        email,
                        apto_postal,
                        cod_operadora,
                        cod_plan,
                        cod_contrato,
                        cod_beneficiario,
                        estado
                    FROM dbo.FND_SubCUENTAS
                    WHERE IDX = @Idx
                      AND cod_operadora = @Operadora
                      AND cod_plan = @Plan
                      AND cod_contrato = @Contrato;";

        private const string SqlExisteDetalleSubCuenta = @"
                    SELECT COUNT(1)
                    FROM dbo.FND_SUBCUENTAS_DETALLE
                    WHERE IDX = @Idx
                      AND cod_operadora = @Operadora
                      AND cod_plan = @Plan
                      AND cod_contrato = @Contrato;";

        private const string SqlDeleteSubCuenta = @"
                    DELETE FROM dbo.FND_SubCUENTAS
                    WHERE IDX = @Idx
                      AND cod_operadora = @Operadora
                      AND cod_plan = @Plan
                      AND cod_contrato = @Contrato;";

        private const string SqlCedulaConsecutivo = @"
                    SELECT CONVERT(varchar(20), ISNULL(MAX(IDX), 0) + 1)
                    FROM dbo.FND_SubCUENTAS
                    WHERE cod_plan = @Plan
                      AND cod_contrato = @Contrato
                      AND cod_operadora = @Operadora;";

        public FrmFndSubCuentasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Método que obtiene la lista de SubCuentas de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="plan"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<FndSubCuentasData>> FND_SubCuentas_Lista_Obtener(int CodEmpresa, int operadora, string plan, long contrato)
        {
            return DbHelper.ExecuteListQuery<FndSubCuentasData>(
                new PortalDB(_config),
                CodEmpresa,
                SqlSubCuentas,
                new
                {
                    Operadora = operadora,
                    Plan = NormalizarTexto(plan),
                    Contrato = contrato
                });
        }

        public ErrorDto<FndSubCuentasConfiguracionData?> FND_SubCuentas_Configuracion_Obtener(
            int CodEmpresa,
            int operadora,
            string plan,
            long contrato)
        {
            return DbHelper.ExecuteSingleQuery<FndSubCuentasConfiguracionData>(
                new PortalDB(_config),
                CodEmpresa,
                SqlConfiguracionSubCuentasPlan,
                default,
                new
                {
                    Operadora = operadora,
                    Plan = NormalizarTexto(plan),
                    Contrato = contrato
                });
        }


        /// <summary>
        /// Método para obtener la lista de parentescos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_SubCuentas_Parentescos_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlParentescos);
        }

        /// <summary>
        /// Guarda una subcuenta nueva o actualiza una existente después de validar
        /// la configuración del plan y los datos requeridos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa sobre la cual se ejecuta la operación.</param>
        /// <param name="usuario">Usuario que realiza el registro o la modificación.</param>
        /// <param name="data">Datos de la subcuenta que se deben guardar.</param>
        /// <returns>Resultado de la operación con el detalle de éxito o error.</returns>
        public ErrorDto FND_SubCuentas_Guardar(int CodEmpresa, string usuario, FndSubCuentasData data)
        {
            if (data is null)
            {
                return DbHelper.ErrorResponse("Los datos de la subcuenta son requeridos.", -2);
            }

            var valida = fxValida(CodEmpresa, data);
            if (valida.Code != 0)
            {
                return DbHelper.ErrorResponse(valida.Description ?? "La subcuenta no es válida.", valida.Code.GetValueOrDefault(-1));
            }

            var result = data.isNew
                ? FNDSubCuentas_Insertar(CodEmpresa, usuario, data)
                : FNDSubCuentas_Actualizar(CodEmpresa, usuario, data);

            return result.Code == 0
                ? ActualizarCuotaContrato(CodEmpresa, usuario, data)
                : result;
        }

        /// <summary>
        /// Valida la información de la subcuenta, la identificación duplicada y el
        /// máximo de subcuentas configurado para el plan cuando se trata de un alta.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa que contiene el contrato.</param>
        /// <param name="data">Datos de la subcuenta por validar.</param>
        /// <returns>Resultado booleano de la validación y su mensaje de negocio.</returns>
        private ErrorDto<bool> fxValida(int CodEmpresa, FndSubCuentasData data)
        {
            var response = DbHelper.CreateOkResponse(true, string.Empty);

            if (data is null)
            {
                return DbHelper.CreateErrorResponse("Los datos de la subcuenta son requeridos.", -2, false);
            }

            var existe = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlExisteBeneficiario,
                0,
                new
                {
                    Cedula = NormalizarTexto(data.cedula),
                    Idx = data.idx,
                    Operadora = data.cod_operadora,
                    Plan = NormalizarTexto(data.cod_plan),
                    Contrato = data.cod_contrato
                });

            if (existe.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    existe.Description ?? "Error al validar subcuenta.",
                    existe.Code.GetValueOrDefault(-1),
                    false);
            }

            if (data.isNew)
            {
                var limite = ValidarLimiteSubCuentas(CodEmpresa, data);
                if (limite.Code != 0 || !limite.Result)
                {
                    return limite;
                }
            }

            var mensajes = new List<string>();
            if (existe.Result > 0)
            {
                mensajes.Add("Ya existe un beneficiario registrado con el mismo número de identificación.");
            }

            AgregarValidacionesRequeridas(data, mensajes);
            if (mensajes.Count == 0)
            {
                var montoValido = FND_SubCuentas_MontoMinimo_Validar(CodEmpresa, data);
                if (montoValido.Code != 0)
                {
                    return montoValido;
                }

                return response;
            }

            response.Code = -1;
            response.Description = " - " + string.Join(" - ", mensajes);
            response.Result = false;
            return response;
        }

        /// <summary>
        /// Inserta una subcuenta después de obtener el consecutivo correspondiente
        /// al contrato y registra el movimiento en la bitácora cuando finaliza con éxito.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa sobre la cual se ejecuta la inserción.</param>
        /// <param name="usuario">Usuario que registra la subcuenta.</param>
        /// <param name="data">Datos validados de la subcuenta que se debe insertar.</param>
        /// <returns>Resultado de la inserción con el detalle de éxito o error.</returns>
        private ErrorDto FNDSubCuentas_Insertar(int CodEmpresa, string usuario, FndSubCuentasData data)
        {
            var insertResult = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlInsertSubCuenta,
                0,
                CrearParametrosSubCuenta(data, true));

            if (insertResult.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    insertResult.Description ?? "Error al registrar la subcuenta.",
                    insertResult.Code.GetValueOrDefault(-1));
            }

            if (insertResult.Result <= 0)
            {
                return DbHelper.ErrorResponse(
                    "No puede ingresar más subcuentas; se alcanzó el máximo configurado para el plan.",
                    -2);
            }

            data.idx = insertResult.Result;
            RegistrarBitacora(CodEmpresa, usuario, data, "Registra - WEB");

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Actualiza los datos de una subcuenta y registra el movimiento en la
        /// bitácora cuando finaliza con éxito.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa sobre la cual se ejecuta la actualización.</param>
        /// <param name="usuario">Usuario que modifica la subcuenta.</param>
        /// <param name="data">Datos validados de la subcuenta que se debe actualizar.</param>
        /// <returns>Resultado de la actualización con el detalle de éxito o error.</returns>
        private ErrorDto FNDSubCuentas_Actualizar(int CodEmpresa, string usuario, FndSubCuentasData data)
        {
            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlUpdateSubCuenta,
                CrearParametrosSubCuenta(data, false));

            if (result.Code == 0)
            {
                RegistrarBitacora(CodEmpresa, usuario, data, "Modifica - WEB");
            }

            return result;
        }

        /// <summary>
        /// Elimina una subcuenta siempre que no tenga detalle asociado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa que contiene la subcuenta.</param>
        /// <param name="operadora">Código de la operadora del contrato.</param>
        /// <param name="plan">Código del plan del contrato.</param>
        /// <param name="contrato">Número de contrato al que pertenece la subcuenta.</param>
        /// <param name="consec">Consecutivo interno de la subcuenta.</param>
        /// <param name="usuario">Usuario que solicita la eliminación.</param>
        /// <returns>Resultado de la eliminación o el mensaje de negocio correspondiente.</returns>
        public ErrorDto FNDSubCuentas_Borrar(
            int CodEmpresa,
            int operadora,
            string plan,
            long contrato,
            int consec,
            string usuario)
        {
            var parametros = new
            {
                Idx = consec,
                Operadora = operadora,
                Plan = NormalizarTexto(plan),
                Contrato = contrato
            };

            var existe = DbHelper.ExecuteSingleQuery<FndSubCuentasData>(
                new PortalDB(_config),
                CodEmpresa,
                SqlSubCuentaPorIdx,
                default,
                parametros);

            if (existe.Code != 0)
            {
                return DbHelper.ErrorResponse(existe.Description ?? "Error al consultar subcuenta.", existe.Code.GetValueOrDefault(-1));
            }

            if (existe.Result is null)
            {
                return DbHelper.ErrorResponse("La subcuenta no existe.", -2);
            }

            var detalle = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlExisteDetalleSubCuenta,
                0,
                parametros);

            if (detalle.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    detalle.Description ?? "Error al validar los movimientos de la subcuenta.",
                    detalle.Code.GetValueOrDefault(-1));
            }

            if (detalle.Result > 0)
            {
                return DbHelper.ErrorResponse(
                    "No se puede eliminar la subcuenta porque tiene movimientos o detalles asociados.",
                    -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlDeleteSubCuenta,
                parametros);

            if (result.Code != 0)
            {
                var descripcion = result.Description ?? string.Empty;
                var tieneDetalleAsociado =
                    descripcion.Contains("RefFND_SUBCUENTAS3337", StringComparison.OrdinalIgnoreCase) ||
                    descripcion.Contains("FND_SUBCUENTAS_DETALLE", StringComparison.OrdinalIgnoreCase);

                var mensajeError = descripcion;
                if (tieneDetalleAsociado)
                {
                    mensajeError = "No se puede eliminar la subcuenta porque tiene movimientos o detalles asociados.";
                }
                else if (string.IsNullOrWhiteSpace(mensajeError))
                {
                    mensajeError = "No fue posible eliminar la subcuenta.";
                }

                return DbHelper.ErrorResponse(
                    mensajeError,
                    result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, usuario, existe.Result, "Elimina - WEB");

            return ActualizarCuotaContrato(CodEmpresa, usuario, existe.Result);
        }

        /// <summary>
        /// Método para obtener el consecutivo de un beneficiario por cedula
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="plan"></param>
        /// <param name="contrato"></param>
        /// <param name="operadora"></param>
        /// <returns></returns>
        public ErrorDto<string> FNDDSubCuentas_Cedula_Obtener(int CodEmpresa, string plan, long contrato, int operadora)
        {
            var result = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlCedulaConsecutivo,
                string.Empty,
                new
                {
                    Plan = NormalizarTexto(plan),
                    Contrato = contrato,
                    Operadora = operadora
                });

            return new ErrorDto<string>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? string.Empty
            };
        }

        private ErrorDto ActualizarCuotaContrato(
            int CodEmpresa,
            string usuario,
            FndSubCuentasData data)
        {
            return DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlActualizarCuotaContrato,
                new
                {
                    Operadora = data.cod_operadora,
                    Plan = NormalizarTexto(data.cod_plan),
                    Contrato = data.cod_contrato,
                    Usuario = NormalizarTexto(usuario)
                });
        }

        private static void AgregarValidacionesRequeridas(FndSubCuentasData data, List<string> mensajes)
        {
            if (string.IsNullOrWhiteSpace(data.parentesco))
            {
                mensajes.Add("No se ha seleccionado ningún parentesco.");
            }

            if (string.IsNullOrWhiteSpace(data.nombre))
            {
                mensajes.Add("Nombre del beneficiario no es válido.");
            }

            if (string.IsNullOrWhiteSpace(data.apellido1))
            {
                mensajes.Add("Primer apellido del beneficiario no es válido.");
            }

            if (string.IsNullOrWhiteSpace(data.apellido2))
            {
                mensajes.Add("Segundo apellido del beneficiario no es válido.");
            }
        }

        /// <summary>
        /// Verifica que el contrato no haya alcanzado el máximo de subcuentas definido
        /// en la configuración de su plan.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa que contiene el contrato.</param>
        /// <param name="data">Datos que identifican la operadora, el plan y el contrato.</param>
        /// <returns>Resultado positivo cuando todavía se permite registrar otra subcuenta.</returns>
        private ErrorDto<bool> ValidarLimiteSubCuentas(int CodEmpresa, FndSubCuentasData data)
        {
            var configuracion = DbHelper.ExecuteSingleQuery<LimiteSubCuentasPlan>(
                new PortalDB(_config),
                CodEmpresa,
                SqlLimiteSubCuentasPlan,
                default,
                new
                {
                    Operadora = data.cod_operadora,
                    Plan = NormalizarTexto(data.cod_plan),
                    Contrato = data.cod_contrato
                });

            if (configuracion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    configuracion.Description ?? "Error al validar el límite de subcuentas del plan.",
                    configuracion.Code.GetValueOrDefault(-1),
                    false);
            }

            if (configuracion.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontró la configuración del plan para validar las subcuentas.",
                    -2,
                    false);
            }

            if (configuracion.Result.Maximo <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El plan seleccionado no permite registrar subcuentas.",
                    -2,
                    false);
            }

            if (configuracion.Result.Registradas >= configuracion.Result.Maximo)
            {
                return DbHelper.CreateErrorResponse(
                    $"No puede ingresar más subcuentas; el máximo permitido para el plan es de {configuracion.Result.Maximo}.",
                    -2,
                    false);
            }

            return DbHelper.CreateOkResponse(true, string.Empty);
        }

        /// <summary>
        /// Construye los parámetros utilizados por las sentencias de inserción y actualización.
        /// </summary>
        /// <param name="data">Datos de la subcuenta.</param>
        /// <param name="esNuevo">Indica si los parámetros corresponden a una inserción.</param>
        /// <returns>Objeto de parámetros normalizados para acceso a datos.</returns>
        private static object CrearParametrosSubCuenta(FndSubCuentasData data, bool esNuevo)
        {
            return new
            {
                IdX = data.idx,
                Cedula = NormalizarTexto(data.cedula),
                Nombre = CrearNombreCompleto(data),
                Parentesco = NormalizarTexto(data.parentesco),
                FechaNac = MProGrXAuxiliarDB.validaFechaGlobal(data.fechanac, esNuevo ? "yyyyMMdd" : "yyyy-MM-dd"),
                Cuota = data.cuota,
                Aportes = 0,
                Rendimiento = 0,
                Direccion = NormalizarTexto(data.direccion),
                Notas = NormalizarTexto(data.notas),
                Telefono1 = NormalizarTexto(data.telefono1),
                Telefono2 = NormalizarTexto(data.telefono2),
                Email = NormalizarTexto(data.email),
                AptoPostal = NormalizarTexto(data.apto_postal),
                CodOperadora = data.cod_operadora,
                CodPlan = NormalizarTexto(data.cod_plan),
                CodContrato = data.cod_contrato,
                CodBeneficiario = 0,
                Estado = "A"
            };
        }

        /// <summary>
        /// Registra en bitácora el movimiento realizado sobre una subcuenta.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario que realiza el movimiento.</param>
        /// <param name="data">Datos que identifican la subcuenta.</param>
        /// <param name="movimiento">Descripción del movimiento realizado.</param>
        private void RegistrarBitacora(int codEmpresa, string usuario, FndSubCuentasData data, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario),
                DetalleMovimiento = $"Sub-Cuenta de Plan: Op. {data.cod_operadora}..Pln: {NormalizarTexto(data.cod_plan)}..Cnt:{data.cod_contrato}..Id:{data.idx}..Ced.{NormalizarTexto(data.cedula)}, Mnt.{data.cuota}",
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        /// <summary>
        /// Valida que el plan admita subcuentas y que la mensualidad alcance
        /// el monto mínimo configurado, conforme al formulario VB6.
        /// </summary>
        /// <param name="CodEmpresa">Empresa del contrato.</param>
        /// <param name="data">Subcuenta que se intenta guardar.</param>
        /// <returns>Resultado de la validación de la mensualidad.</returns>
        private ErrorDto<bool> FND_SubCuentas_MontoMinimo_Validar(
            int CodEmpresa,
            FndSubCuentasData data)
        {
            var configuracion = DbHelper.ExecuteSingleQuery<FndSubCuentasConfiguracionData>(
                new PortalDB(_config),
                CodEmpresa,
                SqlConfiguracionSubCuentasPlan,
                default,
                new
                {
                    Operadora = data.cod_operadora,
                    Plan = NormalizarTexto(data.cod_plan),
                    Contrato = data.cod_contrato
                });

            if (configuracion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    configuracion.Description ?? "No fue posible validar el monto mínimo del plan.",
                    configuracion.Code.GetValueOrDefault(-1),
                    false);
            }

            if (configuracion.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontró la configuración del plan.",
                    -2,
                    false);
            }

            if (!configuracion.Result.cuenta_maestra)
            {
                return DbHelper.CreateErrorResponse(
                    "Este plan no acepta subcuentas.",
                    -2,
                    false);
            }

            if (data.cuota is null ||
                data.cuota.Value < configuracion.Result.monto_minimo)
            {
                return DbHelper.CreateErrorResponse(
                    $"La mensualidad debe ser mayor o igual a {configuracion.Result.monto_minimo:N2}.",
                    -2,
                    false);
            }

            return DbHelper.CreateOkResponse(true, string.Empty);
        }

        /// <summary>
        /// Forma el nombre completo en el orden APELLIDO1 APELLIDO2 NOMBRE(S),
        /// eliminando apellidos repetidos que hayan llegado dentro del campo nombre.
        /// </summary>
        /// <param name="data">Datos personales de la subcuenta.</param>
        /// <returns>Nombre completo normalizado.</returns>
        private static string CrearNombreCompleto(FndSubCuentasData data)
        {
            var apellidos = SepararPalabras(data.apellido1)
                .Concat(SepararPalabras(data.apellido2))
                .ToList();
            var nombres = SepararPalabras(data.nombre).ToList();

            // Algunas integraciones envían el nombre completo en el campo nombre.
            // Se eliminan únicamente las apariciones que ya llegaron como apellidos
            // para evitar guardarlas dos veces en FND_SubCUENTAS.Nombre.
            foreach (var apellido in apellidos)
            {
                var indice = nombres.FindIndex(nombre =>
                    string.Equals(nombre, apellido, StringComparison.OrdinalIgnoreCase));
                if (indice >= 0)
                {
                    nombres.RemoveAt(indice);
                }
            }

            return string.Join(" ", apellidos.Concat(nombres));
        }

        /// <summary>
        /// Normaliza un texto y lo divide en palabras no vacías.
        /// </summary>
        /// <param name="valor">Texto por separar.</param>
        /// <returns>Palabras normalizadas en mayúsculas.</returns>
        private static IEnumerable<string> SepararPalabras(string? valor) =>
            NormalizarTexto(valor)
                .ToUpperInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        private sealed class LimiteSubCuentasPlan
        {
            public int Maximo { get; set; }
            public int Registradas { get; set; }
        }

    }
}
