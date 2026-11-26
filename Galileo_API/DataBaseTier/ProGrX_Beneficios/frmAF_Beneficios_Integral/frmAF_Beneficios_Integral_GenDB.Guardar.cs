using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralGenDB
    {
        private const string InsertaVal = "Inserta";
        private const string ActualizaVal = "Actualiza";
        private const float ToleranciaMonto = 0.005f;

        /// <summary>
        /// Guardado central del beneficio: aplica validaciones y decide inserción o actualización.
        /// </summary>
        public async Task<ErrorDto<BeneficioGeneralDatos>> BeneficioIntegralGeneral_Guardar(int CodCliente, string fuente, BeneficioGeneralDatos beneficioGeneral)
        {
            if (string.IsNullOrWhiteSpace(beneficioGeneral.cedula))
            {
                return RespuestaError("Cédula no puede ser nula");
            }

            try
            {
                var rechazo = ValidarGuardado(CodCliente, beneficioGeneral);
                if (rechazo != null)
                {
                    return RespuestaError(rechazo);
                }

                return beneficioGeneral.id_beneficio == 0
                    ? await Guarda_Beneficio(CodCliente, beneficioGeneral)
                    : Actualiza_Beneficio(CodCliente, beneficioGeneral, "S", fuente);
            }
            catch (Exception ex)
            {
                return RespuestaError("BeneficioIntegralGeneral_Guardar - " + ex.Message);
            }
        }

        private static ErrorDto<BeneficioGeneralDatos> RespuestaError(string? mensaje) =>
            new() { Code = -1, Description = mensaje };

        /// <summary>
        /// Respuesta de guardado correcto: "id@consecutivo" y, opcionalmente, "@advertencia".
        /// </summary>
        private static ErrorDto<BeneficioGeneralDatos> RespuestaGuardado(int idBeneficio, long? consec, string? advertencia = null) =>
            new()
            {
                Code = 0,
                Description = advertencia == null
                    ? $"{idBeneficio}@{consec}"
                    : $"{idBeneficio}@{consec}@{advertencia.Replace('@', ' ')}"
            };

        /// <summary>
        /// Aplica permisos y validaciones. Devuelve el motivo del rechazo o null si se puede guardar.
        /// </summary>
        private string? ValidarGuardado(int CodCliente, BeneficioGeneralDatos beneficioGeneral)
        {
            var estadoItem = beneficioGeneral.estado?.item ?? string.Empty;
            var codBeneficio = beneficioGeneral.cod_beneficio?.item ?? string.Empty;

            // 1. Permisos del usuario
            var permisosResult = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<string>(
                    "SELECT dbo.fxBeneficio_ValidacionPermisos(@usuario, @codCategoria, @estado)",
                    new { usuario = beneficioGeneral.modifica_usuario, codCategoria = beneficioGeneral.cod_categoria, estado = estadoItem }));

            if (permisosResult.Code != 0)
            {
                return permisosResult.Description;
            }
            if (!string.IsNullOrEmpty(permisosResult.Result))
            {
                return permisosResult.Result;
            }

            // 2. Validación de persona
            var perError = _mBeneficiosDB.ValidarPersona(CodCliente, beneficioGeneral.cedula!.Trim(), codBeneficio);
            if (perError.Code == -1)
            {
                return perError.Description;
            }

            // 3. Requisitos
            if (beneficioGeneral.consec != null)
            {
                var respreq = _mBeneficiosDB.ValidaRequisitos(CodCliente, estadoItem, codBeneficio, (int)beneficioGeneral.consec);
                if (respreq.Code == -1)
                {
                    return respreq.Description;
                }
            }

            // 4. Datos del beneficio y, si es nuevo, reglas de persona con justificación.
            // Se evalúan juntas para informar todos los motivos en un solo mensaje.
            var mensajes = new List<string?>();

            var errBene = _mBeneficiosDB.ValidarBeneficioDato(CodCliente, beneficioGeneral);
            if (errBene.Code == -1)
            {
                mensajes.Add(errBene.Description);
            }

            if (beneficioGeneral.id_beneficio == 0)
            {
                var justError = _mBeneficiosDB.ValidarBeneficioJustificaDato(CodCliente, beneficioGeneral, beneficioGeneral.requiere_justificacion);
                if (justError.Code == -1)
                {
                    mensajes.Add(justError.Description);
                }
            }

            return mensajes.Count == 0 ? null : string.Concat(mensajes);
        }

        /// <summary>
        /// Inserta un nuevo beneficio (afi_bene_otorga + productos), deja bitácora, montos/motivos y notifica por correo.
        /// </summary>
        private async Task<ErrorDto<BeneficioGeneralDatos>> Guarda_Beneficio(
            int CodCliente,
            BeneficioGeneralDatos beneficio)
        {
            var codBeneficio = beneficio.cod_beneficio?.item ?? string.Empty;
            var tipoItem = beneficio.tipo?.item ?? string.Empty;
            var modificaMonto = tipoItem == "P" ? "N" : "S";

            var empresa = _frmAsgDb.CargaOficinas(CodCliente, beneficio.registra_user).Result;
            if (empresa == null || empresa.Count == 0)
            {
                return RespuestaError("No se encontró la oficina del usuario");
            }

            var idGenerado = 0;

            try
            {
                var vBeneConsec = _mBeneficiosDB.fxConsec(CodCliente, codBeneficio);
                if (vBeneConsec <= 0)
                {
                    return RespuestaError("No fue posible generar el consecutivo del beneficio. El expediente no se creó.");
                }
                beneficio.consec = Convert.ToInt32(vBeneConsec);

                using (var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente))
                {
                    var estado = await connection.QueryFirstOrDefaultAsync<string>(
                        @"SELECT TOP 1 E.COD_ESTADO FROM AFI_BENE_ESTADOS E
                          WHERE E.P_INICIA = 1 AND E.PROCESO = 'T' AND E.COD_ESTADO IN (
                              SELECT G.COD_ESTADO FROM AFI_BENE_GRUPO_ESTADOS G WHERE G.COD_GRUPO IN (
                                  SELECT B.COD_GRUPO FROM AFI_GRUPO_BENEFICIO B WHERE COD_BENEFICIO = @codBeneficio))
                          ORDER BY E.COD_ESTADO DESC", new { codBeneficio });

                    var nuevoId = await connection.QuerySingleOrDefaultAsync<int?>(SqlInsertOtorga, new
                    {
                        consec = vBeneConsec,
                        codBeneficio,
                        cedula = beneficio.cedula!.Trim(),
                        monto = beneficio.monto,
                        modificaMonto,
                        registraUser = (beneficio.registra_user ?? string.Empty).ToUpper(),
                        estado,
                        notas = beneficio.notas,
                        solicita = beneficio.cedula.Trim(),
                        nombre = beneficio.nombre,
                        tipo = tipoItem,
                        codOficina = empresa[0].Titular,
                        montoAplicado = beneficio.monto_aplicado,
                        desaNombre = beneficio.desa_nombre,
                        desaDescripcion = beneficio.desa_descripcion,
                        sepelioIdent = beneficio.sepelio_identificacion,
                        sepelioNombre = beneficio.sepelio_nombre,
                        sepelioFecha = beneficio.sepelio_fecha_fallecimiento,
                        creceGrupo = beneficio.crece_grupo,
                        idProfesional = beneficio.id_profesional?.item ?? "0",
                        idAptCategoria = beneficio.id_apt_categoria?.item ?? "0",
                        requiereJustificacion = beneficio.requiere_justificacion ? 1 : 0,
                        aplicaMora = beneficio.aplica_mora ? 1 : 0,
                        aplicaPagoMasivo = beneficio.aplica_pago_masivo ? 1 : 0
                    });

                    if (nuevoId is not > 0)
                    {
                        return RespuestaError("No fue posible crear el expediente del beneficio.");
                    }
                    idGenerado = nuevoId.Value;
                    beneficio.id_beneficio = idGenerado;

                    if (tipoItem != "M" && beneficio.productos != null)
                    {
                        InsertarProductos(connection, beneficio);
                    }
                }

                var tipoDesc = ObtenerDescripcionTipo(tipoItem);
                RegistrarBitacora(CodCliente, codBeneficio, vBeneConsec, InsertaVal,
                    $"Inserta Datos Generales - Beneficio {tipoDesc}: [{idGenerado} {codBeneficio} {vBeneConsec}]", beneficio.registra_user ?? string.Empty);

                if (beneficio.cod_motivo != null)
                {
                    InsertarActualizarMotivos(CodCliente, beneficio);
                }
                InsertarActualizarMontos(CodCliente, beneficio, modificaMonto);

                var advertencia = await NotificarSolicitud(CodCliente, beneficio, idGenerado, vBeneConsec, codBeneficio);
                return RespuestaGuardado(idGenerado, beneficio.consec, advertencia);
            }
            catch (Exception ex)
            {
                // Si el expediente ya se insertó se informa como creado, para que no se registre dos veces.
                return idGenerado > 0
                    ? RespuestaGuardado(idGenerado, beneficio.consec, "El expediente se creó, pero no se completó el proceso: " + ex.Message)
                    : RespuestaError("Guarda_Beneficio - " + ex.Message);
            }
        }

        /// <summary>
        /// Inserta los productos del beneficio recién creado.
        /// </summary>
        private static void InsertarProductos(SqlConnection connection, BeneficioGeneralDatos beneficio)
        {
            const string sql = @"INSERT afi_bene_prodasg(consec, cod_beneficio, cod_producto, cantidad, costo_unidad, REGISTRO_FECHA, REGISTRO_USUARIO)
                                 VALUES(@consec, @codBeneficio, @codProducto, @cantidad, @costoUnidad, GETDATE(), @usuario)";

            foreach (var prod in beneficio.productos!)
            {
                connection.Execute(sql, new
                {
                    consec = beneficio.consec,
                    codBeneficio = beneficio.cod_beneficio?.item ?? string.Empty,
                    codProducto = prod.cod_producto,
                    cantidad = prod.cantidad,
                    costoUnidad = prod.costo_unidad,
                    usuario = beneficio.registra_user
                });
            }
        }

        /// <summary>
        /// Notifica la solicitud por correo (excepto categoría B_CRECE).
        /// Devuelve una advertencia si no se pudo notificar; el expediente ya está creado.
        /// </summary>
        private async Task<string?> NotificarSolicitud(
            int CodCliente, BeneficioGeneralDatos beneficio, int idGenerado, long vBeneConsec, string codBeneficio)
        {
            var correoResult = _envioCorreoDB.BuscoDatosSocioBeneficio(CodCliente, beneficio.cedula ?? string.Empty, codBeneficio);
            var correo = correoResult?.Result;

            if (correo == null || string.IsNullOrEmpty(correo.email))
            {
                return "El asociado no tiene un correo electrónico registrado en Datos Persona; no se envió la notificación.";
            }

            var categoria = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<string>(
                    "SELECT COD_CATEGORIA FROM AFI_BENEFICIOS WHERE COD_BENEFICIO = @codBeneficio", new { codBeneficio })).Result?.Trim();

            if (categoria == "B_CRECE")
            {
                return null;
            }

            var consec = vBeneConsec.ToString().PadLeft(5, '0');
            var envio = await _beneCorreo.EncolarAsync(new CorreoBeneficio
            {
                CodEmpresa = CodCliente,
                CodPlantilla = categoria == "B_PSICO" ? BeneficiosPlantillas.SolicitudPsicologia : BeneficiosPlantillas.Solicitud,
                CodSmtp = _beneCorreo.CodSmtpPorCategoria(CodCliente, categoria ?? string.Empty),
                Para = correo.email,
                Usuario = beneficio.registra_user ?? string.Empty,
                Datos = new Dictionary<string, string>
                {
                    ["BENEFICIO"] = correo.beneficio,
                    ["EXPEDIENTE"] = idGenerado.ToString().PadLeft(5, '0') + codBeneficio.Trim() + consec
                }
            });

            if (envio.Code != 0)
            {
                return "No se registró el correo de notificación: " + envio.Description;
            }

            RegistrarBitacora(CodCliente, codBeneficio.Trim(), beneficio.consec, "Notifica",
                $"Notificación Solicitud de Beneficio enviada a {correo.email}", beneficio.registra_user ?? string.Empty);

            return null;
        }

        /// <summary>
        /// Actualiza los datos generales del beneficio y, según la fuente, monto/estado; siempre motivos si aplica.
        /// </summary>
        private ErrorDto<BeneficioGeneralDatos> Actualiza_Beneficio(int CodCliente, BeneficioGeneralDatos beneficio, string modificaMonto, string fuente)
        {
            var tipoItem = beneficio.tipo?.item ?? string.Empty;

            try
            {
                if (!CorrespondeAlExpediente(CodCliente, beneficio))
                {
                    return RespuestaError("Los datos en pantalla no corresponden al expediente que se intenta actualizar. " +
                                          "Vuelva a seleccionarlo en la consulta o limpie la pantalla para crear uno nuevo.");
                }

                if (beneficio.cod_motivo != null)
                {
                    InsertarActualizarMotivos(CodCliente, beneficio);
                }

                var actualizado = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                    connection.Execute(SqlUpdateOtorga, new
                    {
                        notas = beneficio.notas,
                        solicita = beneficio.cedula!.Trim(),
                        nombre = beneficio.nombre,
                        desaNombre = beneficio.desa_nombre,
                        desaDescripcion = beneficio.desa_descripcion,
                        sepelioIdent = beneficio.sepelio_identificacion,
                        sepelioNombre = beneficio.sepelio_nombre,
                        sepelioFecha = beneficio.sepelio_fecha_fallecimiento,
                        creceGrupo = beneficio.crece_grupo,
                        tipo = tipoItem,
                        idProfesional = beneficio.id_profesional?.item ?? "0",
                        idAptCategoria = beneficio.id_apt_categoria?.item ?? "0",
                        aplicaMora = beneficio.aplica_mora ? 1 : 0,
                        aplicaPagoMasivo = beneficio.aplica_pago_masivo ? 1 : 0,
                        modificaUsuario = beneficio.modifica_usuario,
                        idBeneficio = beneficio.id_beneficio
                    }));

                if (actualizado.Code != 0)
                {
                    return RespuestaError(actualizado.Description);
                }

                var tipoDesc = ObtenerDescripcionTipo(tipoItem);

                var error = new ErrorDto { Code = 0 };
                switch (fuente)
                {
                    case "DB":
                        RegistrarBitacora(CodCliente, beneficio.cod_beneficio?.item ?? string.Empty, beneficio.consec, ActualizaVal,
                            $"Actualiza Datos Generales - Beneficio {tipoDesc}: [{beneficio.id_beneficio} {beneficio.cod_beneficio?.item} {beneficio.consec}]",
                            beneficio.registra_user ?? string.Empty);
                        break;
                    case "M":
                        error = InsertarActualizarMontos(CodCliente, beneficio, modificaMonto);
                        break;
                    case "E":
                        error = InsertarActualizarEstados(CodCliente, beneficio);
                        break;
                }

                if (error.Code == -1)
                {
                    return RespuestaError(error.Description);
                }

                return RespuestaGuardado(beneficio.id_beneficio, beneficio.consec);
            }
            catch (Exception ex)
            {
                return RespuestaError("Actualiza_Beneficio - " + ex.Message);
            }
        }

        private bool CorrespondeAlExpediente(int CodCliente, BeneficioGeneralDatos beneficio)
        {
            const string sql = @"
                SELECT COUNT(*) FROM AFI_BENE_OTORGA
                WHERE ID_BENEFICIO = @idBeneficio AND CONSEC = @consec
                  AND RTRIM(COD_BENEFICIO) = @codBeneficio AND RTRIM(CEDULA) = @cedula";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<int>(sql, new
                {
                    idBeneficio = beneficio.id_beneficio,
                    consec = beneficio.consec,
                    codBeneficio = (beneficio.cod_beneficio?.item ?? string.Empty).Trim(),
                    cedula = (beneficio.cedula ?? string.Empty).Trim()
                }));

            return result.Code == 0 && result.Result > 0;
        }

        /// <summary>
        /// Inserta o actualiza el registro de montos del beneficio y sincroniza productos si aplica.
        /// </summary>
        private ErrorDto InsertarActualizarMontos(int CodCliente, BeneficioGeneralDatos beneficio, string modificaMonto)
        {
            var codBeneficio = beneficio.cod_beneficio?.item ?? string.Empty;
            var tipoItem = beneficio.tipo?.item ?? string.Empty;

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);

                if (ExisteRegitro(CodCliente, beneficio.consec, codBeneficio, "MON"))
                {
                    var montoAnterior = connection.QueryFirstOrDefault<float?>(
                        "SELECT MONTO_NUEVO FROM AFI_BENE_REGISTRO_MONTOS WHERE CONSEC = @consec AND COD_BENEFICIO = @codBeneficio",
                        new { consec = beneficio.consec, codBeneficio }) ?? 0;

                    connection.Execute(
                        @"UPDATE [dbo].[AFI_BENE_REGISTRO_MONTOS]
                             SET [MONTO_NUEVO] = @montoNuevo, [MONTO_ANTERIOR] = @montoAnterior, [NOTAS] = @notas,
                                 [REGISTRO_FECHA] = GETDATE(), [REGISTRO_USUARIO] = @usuario
                           WHERE CONSEC = @consec AND [COD_BENEFICIO] = @codBeneficio",
                        new { montoNuevo = beneficio.monto_aplicado, montoAnterior, notas = beneficio.observaciones_monto, usuario = beneficio.registra_user, consec = beneficio.consec, codBeneficio });

                    if (MontoFueModificado(montoAnterior, beneficio.monto_aplicado))
                    {
                        RegistrarBitacora(CodCliente, codBeneficio, beneficio.consec, ActualizaVal,
                            $"Actualiza Monto de {montoAnterior} a {beneficio.monto_aplicado} ", beneficio.registra_user ?? string.Empty);
                    }
                }
                else
                {
                    connection.Execute(
                        @"INSERT INTO [dbo].[AFI_BENE_REGISTRO_MONTOS] ([COD_BENEFICIO],[CONSEC],[MONTO_NUEVO],[MONTO_ANTERIOR],[NOTAS],[REGISTRO_FECHA],[REGISTRO_USUARIO])
                          VALUES (@codBeneficio, @consec, @montoNuevo, 0, @notas, GETDATE(), @usuario)",
                        new { codBeneficio, consec = beneficio.consec, montoNuevo = beneficio.monto_aplicado, notas = beneficio.observaciones_monto, usuario = beneficio.registra_user });

                    RegistrarBitacora(CodCliente, codBeneficio, beneficio.consec, InsertaVal,
                        $"Inserta Monto {beneficio.monto_aplicado}", beneficio.registra_user ?? string.Empty);
                }

                connection.Execute(
                    "UPDATE afi_bene_otorga SET monto = @monto, modifica_monto = @modificaMonto, MONTO_APLICADO = @montoAplicado WHERE id_beneficio = @idBeneficio",
                    new { monto = beneficio.monto, modificaMonto, montoAplicado = beneficio.monto_aplicado, idBeneficio = beneficio.id_beneficio });

                if (tipoItem != "M" && beneficio.productos != null)
                {
                    SincronizarProductos(connection, beneficio);
                }

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = "InsertarActualizarMontos - " + ex.Message };
            }
        }

        private static string ObtenerDescripcionTipo(string tipoItem) =>
            tipoItem switch
            {
                "P" => "Producto",
                "M" => "Monetario",
                _ => "Mixto"
            };

        private static bool MontoFueModificado(
            float montoAnterior,
            float? montoNuevo) =>
            !montoNuevo.HasValue ||
            MathF.Abs(montoAnterior - montoNuevo.Value) > ToleranciaMonto;

        /// <summary>
        /// Elimina y reinserta los productos del beneficio; recalcula el monto aplicado cuando el tipo es Producto.
        /// </summary>
        private static void SincronizarProductos(SqlConnection connection, BeneficioGeneralDatos beneficio)
        {
            var codBeneficio = beneficio.cod_beneficio?.item ?? string.Empty;

            connection.Execute("DELETE FROM afi_bene_prodasg WHERE cod_beneficio = @codBeneficio AND consec = @consec",
                new { codBeneficio, consec = beneficio.consec });

            InsertarProductos(connection, beneficio);

            if ((beneficio.tipo?.item ?? string.Empty) == "P")
            {
                var montoTotal = connection.QueryFirstOrDefault<decimal>(
                    "SELECT ISNULL(SUM(ISNULL(CANTIDAD,0) * COSTO_UNIDAD),0) AS MONTO FROM AFI_BENE_PRODASG WHERE CONSEC = @consec AND COD_BENEFICIO = @codBeneficio",
                    new { consec = beneficio.consec, codBeneficio });

                connection.Execute("UPDATE afi_bene_otorga SET MONTO_APLICADO = @monto WHERE id_beneficio = @idBeneficio",
                    new { monto = montoTotal, idBeneficio = beneficio.id_beneficio });
            }
        }

        /// <summary>
        /// Inserta o actualiza el estado del beneficio, valida pagos y dispara el registro documental si es Aprobado.
        /// </summary>
        private ErrorDto InsertarActualizarEstados(int CodCliente, BeneficioGeneralDatos beneficio)
        {
            var codBeneficio = beneficio.cod_beneficio?.item ?? string.Empty;
            var estadoItem = beneficio.estado?.item ?? string.Empty;

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);

                var codEstadoAnterior = connection.QueryFirstOrDefault<string>(
                    "SELECT COD_ESTADO FROM AFI_BENE_REGISTRO_ESTADOS WHERE CONSEC = @consec AND COD_BENEFICIO = @codBeneficio",
                    new { consec = beneficio.consec, codBeneficio });

                var estadoAnterior = connection.QueryFirstOrDefault<string>(
                    "SELECT [DESCRIPCION] FROM [AFI_BENE_ESTADOS] WHERE COD_ESTADO = @codEstado", new { codEstado = codEstadoAnterior });

                var estadoActual = connection.QueryFirstOrDefault<string>(
                    "SELECT [DESCRIPCION] FROM [AFI_BENE_ESTADOS] WHERE COD_ESTADO = @codEstado", new { codEstado = estadoItem });

                var ordenPagada = connection.QueryFirstOrDefault<int>(
                    @"SELECT COUNT(ID_PAGO) FROM AFI_BENE_PAGO
                      WHERE COD_BENEFICIO = @codBeneficio AND CONSEC = @consec AND CEDULA = @cedula AND ESTADO = 'P'",
                    new { codBeneficio, consec = beneficio.consec, cedula = beneficio.cedula });

                var estadoAprob = connection.QueryFirstOrDefault<string>(
                    "SELECT COD_ESTADO FROM [dbo].[AFI_BENE_ESTADOS] WHERE COD_ESTADO = @codEstado AND P_FINALIZA = 1 AND PROCESO = 'A'",
                    new { codEstado = estadoItem });

                if (ordenPagada > 0 && estadoAprob == null)
                {
                    return new ErrorDto { Code = -1, Description = "No se permite cambiar al estado indicado debido a que este expediente ya tiene un registro de solicitud de pago" };
                }

                if (ExisteRegitro(CodCliente, beneficio.consec, codBeneficio, "E"))
                {
                    connection.Execute(
                        @"UPDATE [dbo].[AFI_BENE_REGISTRO_ESTADOS]
                             SET [COD_ESTADO] = @estado, [NOTAS] = @notas, [REGISTRO_FECHA] = GETDATE(), [REGISTRO_USUARIO] = @usuario
                           WHERE CONSEC = @consec AND [COD_BENEFICIO] = @codBeneficio",
                        new { estado = estadoItem, notas = beneficio.estadoObservaciones, usuario = beneficio.registra_user, consec = beneficio.consec, codBeneficio });

                    if (estadoItem == "A" || (beneficio.estado?.descripcion ?? string.Empty).ToUpper() == "APROBADO")
                    {
                        DispararRegistroDocumental(connection, beneficio);
                    }

                    if (codEstadoAnterior != estadoItem)
                    {
                        RegistrarBitacora(CodCliente, codBeneficio, beneficio.consec, ActualizaVal,
                            $"Actualiza Estado de {estadoAnterior} a {estadoActual}", beneficio.registra_user ?? string.Empty);
                    }
                }
                else
                {
                    connection.Execute(
                        @"INSERT INTO [dbo].[AFI_BENE_REGISTRO_ESTADOS] ([COD_BENEFICIO],[CONSEC],[COD_ESTADO],[NOTAS],[REGISTRO_FECHA],[REGISTRO_USUARIO])
                          VALUES (@codBeneficio, @consec, @estado, @notas, GETDATE(), @usuario)",
                        new { codBeneficio, consec = beneficio.consec, estado = estadoItem, notas = beneficio.estadoObservaciones, usuario = beneficio.registra_user });

                    RegistrarBitacora(CodCliente, codBeneficio, beneficio.consec, InsertaVal,
                        $"Inserta Estado {estadoActual}", beneficio.registra_user ?? string.Empty);
                }

                connection.Execute("UPDATE afi_bene_otorga SET estado = @estado WHERE id_beneficio = @idBeneficio",
                    new { estado = estadoItem, idBeneficio = beneficio.id_beneficio });

                var finaliza = connection.QueryFirstOrDefault<string>(
                    "SELECT COD_ESTADO FROM [dbo].[AFI_BENE_ESTADOS] WHERE COD_ESTADO = @codEstado AND P_FINALIZA = 1", new { codEstado = estadoItem });

                if (finaliza != null)
                {
                    connection.Execute(
                        "UPDATE afi_bene_otorga SET autoriza_fecha = GETDATE(), autoriza_user = @usuario WHERE id_beneficio = @idBeneficio",
                        new { usuario = beneficio.modifica_usuario, idBeneficio = beneficio.id_beneficio });
                }

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = "InsertarActualizarEstados - " + ex.Message };
            }
        }

        /// <summary>
        /// Dispara el registro documental (spTrdDocumentosIns) cuando el beneficio se aprueba.
        /// </summary>
        private static void DispararRegistroDocumental(SqlConnection connection, BeneficioGeneralDatos beneficio)
        {
            var trd = new TrdDocumentosModel
            {
                CodDocumento = "10",
                Consecutivo = beneficio.id_beneficio.ToString(),
                IdSobre = null,
                IdEstado = 1,
                ConfirmaRecepcion = 2,
                FechaActualiza = null,
                UsuarioActualiza = null,
                FechaInserta = DateTime.Now,
                UsuarioInserta = beneficio.registra_user,
                CodBarras = beneficio.id_beneficio.ToString(),
                Descripcion = null
            };

            connection.Execute("dbo.spTrdDocumentosIns", new
            {
                trd.CodDocumento,
                trd.Consecutivo,
                trd.IdSobre,
                trd.IdEstado,
                trd.ConfirmaRecepcion,
                trd.FechaActualiza,
                trd.UsuarioActualiza,
                trd.FechaInserta,
                trd.UsuarioInserta,
                trd.CodBarras,
                trd.Descripcion,
                Resultado = 0
            }, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Inserta o actualiza el motivo del beneficio.
        /// </summary>
        private void InsertarActualizarMotivos(int CodCliente, BeneficioGeneralDatos beneficio)
        {
            var codBeneficio = beneficio.cod_beneficio?.item ?? string.Empty;
            var motivoItem = beneficio.cod_motivo?.item ?? string.Empty;

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);

                if (ExisteRegitro(CodCliente, beneficio.consec, codBeneficio, "MOT"))
                {
                    var motivoAnterior = connection.QueryFirstOrDefault<string>(
                        "SELECT COD_MOTIVO FROM AFI_BENE_REGISTRO_MOTIVOS WHERE CONSEC = @consec AND COD_BENEFICIO = @codBeneficio",
                        new { consec = beneficio.consec, codBeneficio });

                    connection.Execute(
                        @"UPDATE [dbo].[AFI_BENE_REGISTRO_MOTIVOS]
                             SET [COD_MOTIVO] = @motivo, [REGISTRO_FECHA] = GETDATE(), [REGISTRO_USUARIO] = @usuario
                           WHERE CONSEC = @consec AND [COD_BENEFICIO] = @codBeneficio",
                        new { motivo = motivoItem, usuario = beneficio.registra_user, consec = beneficio.consec, codBeneficio });

                    if (motivoAnterior != motivoItem)
                    {
                        RegistrarBitacora(CodCliente, codBeneficio, beneficio.consec, ActualizaVal,
                            $"Actualiza Motivo de {motivoAnterior}a {motivoItem}", beneficio.registra_user ?? string.Empty);
                    }
                }
                else
                {
                    connection.Execute(
                        @"INSERT INTO [dbo].[AFI_BENE_REGISTRO_MOTIVOS] ([COD_BENEFICIO],[CONSEC],[COD_MOTIVO],[REGISTRO_FECHA],[REGISTRO_USUARIO])
                          VALUES (@codBeneficio, @consec, @motivo, GETDATE(), @usuario)",
                        new { codBeneficio, consec = beneficio.consec, motivo = motivoItem, usuario = beneficio.registra_user });

                    RegistrarBitacora(CodCliente, codBeneficio, beneficio.consec, InsertaVal,
                        $"Inserta Motivo {beneficio.cod_motivo?.descripcion}", beneficio.registra_user ?? string.Empty);
                }
            }
            catch (Exception)
            {
                // El motivo es metadata secundaria: si falla su registro no se aborta el guardado principal (paridad con el comportamiento original).
            }
        }

        /// <summary>
        /// Indica si existe un registro relacionado (SP spAFI_Bene_ExisteRegistros). tipo: MON, E, MOT.
        /// </summary>
        private bool ExisteRegitro(int CodCliente, int? consec, string cod_beneficio, string tipo)
        {
            const string sql = "EXEC spAFI_Bene_ExisteRegistros @consec, @codBeneficio, @tipo";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<int>(sql, new { consec, codBeneficio = cod_beneficio, tipo }));

            return result.Code == 0 && result.Result > 0;
        }

        // ==================== SQL de otorga (sin datos de usuario) ====================

        private const string SqlInsertOtorga = @"
            INSERT afi_bene_otorga (consec, cod_beneficio, cedula, monto, modifica_monto, registra_user, registra_fecha,
                                    estado, notas, Solicita, nombre, tipo, cod_oficina, MONTO_APLICADO, FENA_NOMBRE, FENA_DESCRIPCION,
                                    SEPELIO_IDENTIFICACION, SEPELIO_NOMBRE, SEPELIO_FECHA_FALLECIMIENTO, CRECE_GRUPO, ID_PROFESIONAL,
                                    ID_APT_CATEGORIA, REQUIERE_JUSTIFICACION, APLICA_MORA, APLICA_PAGO_MASIVO)
            VALUES (@consec, @codBeneficio, @cedula, @monto, @modificaMonto, @registraUser, GETDATE(),
                    @estado, @notas, @solicita, @nombre, @tipo, @codOficina, @montoAplicado, @desaNombre, @desaDescripcion,
                    @sepelioIdent, @sepelioNombre, @sepelioFecha, @creceGrupo, @idProfesional,
                    @idAptCategoria, @requiereJustificacion, @aplicaMora, @aplicaPagoMasivo);
            SELECT CAST(SCOPE_IDENTITY() AS int);";

        private const string SqlUpdateOtorga = @"
            UPDATE afi_bene_otorga
               SET notas = @notas, Solicita = @solicita, nombre = @nombre, FENA_NOMBRE = @desaNombre, FENA_DESCRIPCION = @desaDescripcion,
                   SEPELIO_IDENTIFICACION = @sepelioIdent, SEPELIO_NOMBRE = @sepelioNombre, SEPELIO_FECHA_FALLECIMIENTO = @sepelioFecha,
                   CRECE_GRUPO = @creceGrupo, TIPO = @tipo, ID_PROFESIONAL = @idProfesional, ID_APT_CATEGORIA = @idAptCategoria,
                   APLICA_MORA = @aplicaMora, APLICA_PAGO_MASIVO = @aplicaPagoMasivo, modifica_usuario = @modificaUsuario, modifica_fecha = GETDATE()
             WHERE id_beneficio = @idBeneficio";
    }
}
