using Dapper;
using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// Guardado central del beneficio: aplica validaciones y decide inserción o actualización.
        /// </summary>
        public async Task<ErrorDto<BeneficioGeneralDatos>> BeneficioIntegralGeneral_Guardar(int CodCliente, string fuente, BeneficioGeneralDatos beneficioGeneral)
        {
            if (string.IsNullOrEmpty(beneficioGeneral.cedula))
            {
                return new ErrorDto<BeneficioGeneralDatos> { Code = -1, Description = "Cédula no puede ser nula" };
            }

            var estadoItem = beneficioGeneral.estado?.item ?? string.Empty;
            var codBeneficio = beneficioGeneral.cod_beneficio?.item ?? string.Empty;

            // 1. Permisos del usuario
            var permisosResult = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<string>(
                    "SELECT dbo.fxBeneficio_ValidacionPermisos(@usuario, @codCategoria, @estado)",
                    new { usuario = beneficioGeneral.modifica_usuario, codCategoria = beneficioGeneral.cod_categoria, estado = estadoItem }));

            if (permisosResult.Code != 0)
            {
                return new ErrorDto<BeneficioGeneralDatos> { Code = -1, Description = permisosResult.Description };
            }
            if (!string.IsNullOrEmpty(permisosResult.Result))
            {
                return new ErrorDto<BeneficioGeneralDatos> { Code = -1, Description = "\n " + permisosResult.Result };
            }

            // 2. Validación de persona
            var perError = _mBeneficiosDB.ValidarPersona(CodCliente, beneficioGeneral.cedula.Trim(), codBeneficio);
            if (perError.Code == -1)
            {
                return new ErrorDto<BeneficioGeneralDatos> { Code = -1, Description = perError.Description };
            }

            // 3. Parcialidad del beneficio (reutiliza frmAF_BeneficioAsg)
            var afiBeneficios = _frmAsgDb.AfiBeneficioDTO_Obtener(CodCliente, codBeneficio).Result;
            _bAplicaParcial = afiBeneficios?.aplica_parcial == 1;

            // 4. Requisitos
            if (beneficioGeneral.consec != null)
            {
                var respreq = _mBeneficiosDB.ValidaRequisitos(CodCliente, estadoItem, codBeneficio, (int)beneficioGeneral.consec);
                if (respreq.Code == -1)
                {
                    return new ErrorDto<BeneficioGeneralDatos> { Code = respreq.Code, Description = respreq.Description };
                }
            }

            // 5. Datos del beneficio
            var errBene = _mBeneficiosDB.ValidarBeneficioDato(CodCliente, beneficioGeneral);
            if (errBene.Code == -1)
            {
                return new ErrorDto<BeneficioGeneralDatos> { Code = errBene.Code, Description = errBene.Description };
            }

            return beneficioGeneral.id_beneficio == 0
                ? await Guarda_Beneficio(CodCliente, beneficioGeneral, "S", fuente)
                : Actualiza_Beneficio(CodCliente, beneficioGeneral, "S", fuente);
        }

        /// <summary>
        /// Inserta un nuevo beneficio (afi_bene_otorga + productos), deja bitácora, montos/motivos y notifica por correo.
        /// </summary>
        private async Task<ErrorDto<BeneficioGeneralDatos>> Guarda_Beneficio(int CodCliente, BeneficioGeneralDatos beneficio, string modificaMonto, string fuente)
        {
            var codBeneficio = beneficio.cod_beneficio?.item ?? string.Empty;
            var tipoItem = beneficio.tipo?.item ?? string.Empty;

            var justError = _mBeneficiosDB.ValidarBeneficioJustificaDato(CodCliente, beneficio, beneficio.requiere_justificacion);
            if (justError.Code == -1)
            {
                return new ErrorDto<BeneficioGeneralDatos> { Code = justError.Code, Description = justError.Description };
            }

            var empresa = _frmAsgDb.CargaOficinas(CodCliente, beneficio.registra_user).Result;
            if (empresa == null || empresa.Count == 0)
            {
                return new ErrorDto<BeneficioGeneralDatos> { Code = -1, Description = "No se encontró la oficina del usuario" };
            }

            var response = new ErrorDto<BeneficioGeneralDatos> { Code = 0 };

            try
            {
                var vBeneConsec = _mBeneficiosDB.fxConsec(CodCliente, codBeneficio);
                beneficio.consec = Convert.ToInt32(vBeneConsec);
                modificaMonto = tipoItem == "P" ? "N" : "S";

                int idGenerado;
                using (var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente))
                {
                    var estado = await connection.QueryFirstOrDefaultAsync<string>(
                        @"SELECT TOP 1 E.COD_ESTADO FROM AFI_BENE_ESTADOS E
                          WHERE E.P_INICIA = 1 AND E.PROCESO = 'T' AND E.COD_ESTADO IN (
                              SELECT G.COD_ESTADO FROM AFI_BENE_GRUPO_ESTADOS G WHERE G.COD_GRUPO IN (
                                  SELECT B.COD_GRUPO FROM AFI_GRUPO_BENEFICIO B WHERE COD_BENEFICIO = @codBeneficio))
                          ORDER BY E.COD_ESTADO DESC", new { codBeneficio });

                    await connection.ExecuteAsync(SqlInsertOtorga, new
                    {
                        consec = vBeneConsec,
                        codBeneficio,
                        cedula = beneficio.cedula!.Trim(),
                        monto = beneficio.monto,
                        modificaMonto,
                        registraUser = (beneficio.registra_user ?? string.Empty).ToUpper(),
                        estado,
                        notas = beneficio.notas,
                        solicita = beneficio.cedula!.Trim(),
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

                    idGenerado = await connection.QueryFirstOrDefaultAsync<int>("SELECT IDENT_CURRENT('afi_bene_otorga') AS id");
                    beneficio.id_beneficio = idGenerado;

                    if (tipoItem != "M" && beneficio.productos != null)
                    {
                        InsertarProductos(connection, beneficio);
                    }
                }

                var tipoDesc = tipoItem == "P" ? "Producto" : tipoItem == "M" ? "Monetario" : "Mixto";
                RegistrarBitacora(CodCliente, codBeneficio, vBeneConsec, InsertaVal,
                    $"Inserta Datos Generales - Beneficio {tipoDesc}: [{idGenerado} {codBeneficio} {vBeneConsec}]", beneficio.registra_user ?? string.Empty);

                if (beneficio.cod_motivo != null)
                {
                    InsertarActualizarMotivos(CodCliente, beneficio);
                }
                InsertarActualizarMontos(CodCliente, beneficio, modificaMonto);

                response.Description = idGenerado + "@" + beneficio.consec;

                return await NotificarSolicitud(CodCliente, beneficio, idGenerado, vBeneConsec, codBeneficio, response);
            }
            catch (Exception ex)
            {
                return new ErrorDto<BeneficioGeneralDatos> { Code = -1, Description = "Guarda_Beneficio - " + ex.Message };
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
        /// Notifica la solicitud por correo (excepto categoría B_CRECE) y valida el correo del socio.
        /// </summary>
        private async Task<ErrorDto<BeneficioGeneralDatos>> NotificarSolicitud(
            int CodCliente, BeneficioGeneralDatos beneficio, int idGenerado, long vBeneConsec, string codBeneficio, ErrorDto<BeneficioGeneralDatos> response)
        {
            var correoResult = _envioCorreoDB.BuscoDatosSocioBeneficio(CodCliente, beneficio.cedula ?? string.Empty, codBeneficio);
            var correo = correoResult?.Result;

            if (correo == null || string.IsNullOrEmpty(correo.email))
            {
                return new ErrorDto<BeneficioGeneralDatos>
                {
                    Code = -1,
                    Description = "El asociado no tiene un correo electrónico registrado en Datos Persona"
                };
            }

            var email = new AfiBeneDatosCorreo
            {
                nombre = correo.nombre,
                cedula = correo.cedula,
                email = correo.email,
                beneficio = correo.beneficio,
                expediente = idGenerado.ToString().PadLeft(5, '0') + codBeneficio.Trim() + vBeneConsec.ToString().PadLeft(5, '0')
            };

            var categoria = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<string>(
                    "SELECT COD_CATEGORIA FROM AFI_BENEFICIOS WHERE COD_BENEFICIO = @codBeneficio", new { codBeneficio })).Result;

            if (categoria != "B_CRECE")
            {
                await CorreoNotificacionSolicitud_Enviar(CodCliente, email, codBeneficio.Trim(),
                    (beneficio.consec ?? 0).ToString(), idGenerado, beneficio.registra_user ?? string.Empty);
            }

            return response;
        }

        /// <summary>
        /// Actualiza los datos generales del beneficio y, según la fuente, monto/estado; siempre motivos si aplica.
        /// </summary>
        private ErrorDto<BeneficioGeneralDatos> Actualiza_Beneficio(int CodCliente, BeneficioGeneralDatos beneficio, string modificaMonto, string fuente)
        {
            var response = new ErrorDto<BeneficioGeneralDatos> { Code = 0 };
            var tipoItem = beneficio.tipo?.item ?? string.Empty;

            try
            {
                if (beneficio.cod_motivo != null)
                {
                    InsertarActualizarMotivos(CodCliente, beneficio);
                }

                DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
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

                var tipoDesc = tipoItem == "P" ? "Producto" : tipoItem == "M" ? "Monetario" : "Mixto";

                var error = new ErrorDto { Code = 0 };
                switch (fuente)
                {
                    case "DB":
                        RegistrarBitacora(CodCliente, beneficio.cod_beneficio?.item ?? string.Empty, beneficio.consec, "Actualiza",
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
                    return new ErrorDto<BeneficioGeneralDatos> { Code = -1, Description = error.Description };
                }

                response.Description = beneficio.id_beneficio + "@" + beneficio.consec;
            }
            catch (Exception ex)
            {
                return new ErrorDto<BeneficioGeneralDatos> { Code = -1, Description = "Actualiza_Beneficio - " + ex.Message };
            }

            return response;
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
                    var montoAnterior = connection.QueryFirstOrDefault<float>(
                        "SELECT MONTO_NUEVO FROM AFI_BENE_REGISTRO_MONTOS WHERE CONSEC = @consec AND COD_BENEFICIO = @codBeneficio",
                        new { consec = beneficio.consec, codBeneficio });

                    connection.Execute(
                        @"UPDATE [dbo].[AFI_BENE_REGISTRO_MONTOS]
                             SET [MONTO_NUEVO] = @montoNuevo, [MONTO_ANTERIOR] = @montoAnterior, [NOTAS] = @notas,
                                 [REGISTRO_FECHA] = GETDATE(), [REGISTRO_USUARIO] = @usuario
                           WHERE CONSEC = @consec AND [COD_BENEFICIO] = @codBeneficio",
                        new { montoNuevo = beneficio.monto_aplicado, montoAnterior, notas = beneficio.observaciones_monto, usuario = beneficio.registra_user, consec = beneficio.consec, codBeneficio });

                    if (montoAnterior != beneficio.monto_aplicado)
                    {
                        RegistrarBitacora(CodCliente, codBeneficio, beneficio.consec, "Actualiza",
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
                        RegistrarBitacora(CodCliente, codBeneficio, beneficio.consec, "Actualiza",
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
        private ErrorDto InsertarActualizarMotivos(int CodCliente, BeneficioGeneralDatos beneficio)
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
                        RegistrarBitacora(CodCliente, codBeneficio, beneficio.consec, "Actualiza",
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

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = "InsertarActualizarMotivos - " + ex.Message };
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

        /// <summary>
        /// Envía el correo de notificación de solicitud del beneficio al asociado.
        /// </summary>
        private async Task CorreoNotificacionSolicitud_Enviar(int CodCliente, AfiBeneDatosCorreo socio, string cod_beneficio, string consec, int id_beneficio, string usuario)
        {
            var info = new ErrorDto { Code = 0 };

            try
            {
                var codCategoria = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                    connection.QueryFirstOrDefault<string>(
                        @"SELECT C.COD_SMTP FROM AFI_BENE_CATEGORIAS C
                          WHERE C.COD_CATEGORIA IN (
                              SELECT B.COD_CATEGORIA FROM AFI_BENEFICIOS B WHERE B.COD_BENEFICIO IN (
                                  SELECT DISTINCT H.COD_BENEFICIO FROM AFI_BENE_OTORGA H WHERE H.ID_BENEFICIO = @idBeneficio))",
                        new { idBeneficio = id_beneficio })).Result;

                var eConfig = ObtenerCorreoConfig(CodCliente, codCategoria);
                if (eConfig == null)
                {
                    return;
                }

                var body = codCategoria == "B_PSICO"
                    ? ArmarBodyHtml($"Confirmación de solicitud por Beneficio de {socio.beneficio} {socio.expediente}", BodyPsico(socio))
                    : ArmarBodyHtml($"Confirmación de solicitud por Beneficio de {socio.beneficio} {socio.expediente}", BodyGeneral(socio));

                if (_sendEmail == "Y")
                {
                    var emailRequest = new EmailRequest
                    {
                        To = socio.email,
                        From = eConfig.User,
                        Subject = "Notificación de Solicitud",
                        Body = body,
                        Attachments = new List<IFormFile>()
                    };

                    await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, info);
                }

                RegistrarBitacora(CodCliente, cod_beneficio, int.TryParse(consec, out var c) ? c : 0, "Notifica",
                    $"Notificación Solicitud de Beneficio enviada a {socio.email}", usuario);
            }
            catch
            {
                // El fallo de correo no interrumpe el guardado (comportamiento del sistema anterior).
            }
        }

        private static string BodyPsico(AfiBeneDatosCorreo socio) =>
            $@"<p>Estimado (a) asociado (a):</p>
               <p>Nos complace informarle que la solicitud de {socio.beneficio} fue registrada en nuestro sistema.</p>
               <p>Este servicio de atención psicológica en ASECCSS consta de 3 sesiones aproximadamente con la intención de brindar un acompañamiento emocional en el momento que te encuentres enfrentando actualmente.</p>
               <p>Pronto nuestro equipo se pondrá en contacto con usted para brindarle más detalles y las indicaciones del proceso, así como la fecha de inicio de la primera sesión.</p>
               <p>En la Gerencia de Bienestar Social y Sostenibilidad de ASECCSS estamos para servirle. ¡Nuestro compromiso solidario es con el asociado/a y su familia!</p>";

        private static string BodyGeneral(AfiBeneDatosCorreo socio) =>
            $@"<p>Estimado (a) asociado (a):</p>
               <p>Nos complace informarle que la solicitud de {socio.beneficio} fue registrada en nuestro sistema.</p>
               <p>Le recordamos algunos puntos fundamentales:</p>
               <p>1. De la manera más atenta, aclaramos que el llenado del formulario de solicitud y la presentación de los requisitos adjuntos no garantiza la aprobación del beneficio.</p>
               <p>2. Todas las solicitudes deben ser analizadas por el equipo de Bienestar Social y Sostenibilidad.</p>
               <p>3. Para finalizar, las resoluciones se estarán brindando luego de la verificación de los requisitos completos de acuerdo con los plazos estipulados en el Reglamento del PROBESOL.</p>
               <p>Puede consultar el Reglamento PROBESOL y la información de nuestros beneficios solidarios en: https://aseccss.com/beneficios-solidarios/</p>
               <p>En la Gerencia de Bienestar Social y Sostenibilidad de ASECCSS estamos para servirle. ¡Nuestro compromiso solidario es con el asociado/a y su familia!</p>";

        // ==================== SQL de otorga (sin datos de usuario) ====================

        private const string SqlInsertOtorga = @"
            INSERT afi_bene_otorga (consec, cod_beneficio, cedula, monto, modifica_monto, registra_user, registra_fecha,
                                    estado, notas, Solicita, nombre, tipo, cod_oficina, MONTO_APLICADO, FENA_NOMBRE, FENA_DESCRIPCION,
                                    SEPELIO_IDENTIFICACION, SEPELIO_NOMBRE, SEPELIO_FECHA_FALLECIMIENTO, CRECE_GRUPO, ID_PROFESIONAL,
                                    ID_APT_CATEGORIA, REQUIERE_JUSTIFICACION, APLICA_MORA, APLICA_PAGO_MASIVO)
            VALUES (@consec, @codBeneficio, @cedula, @monto, @modificaMonto, @registraUser, GETDATE(),
                    @estado, @notas, @solicita, @nombre, @tipo, @codOficina, @montoAplicado, @desaNombre, @desaDescripcion,
                    @sepelioIdent, @sepelioNombre, @sepelioFecha, @creceGrupo, @idProfesional,
                    @idAptCategoria, @requiereJustificacion, @aplicaMora, @aplicaPagoMasivo)";

        private const string SqlUpdateOtorga = @"
            UPDATE afi_bene_otorga
               SET notas = @notas, Solicita = @solicita, nombre = @nombre, FENA_NOMBRE = @desaNombre, FENA_DESCRIPCION = @desaDescripcion,
                   SEPELIO_IDENTIFICACION = @sepelioIdent, SEPELIO_NOMBRE = @sepelioNombre, SEPELIO_FECHA_FALLECIMIENTO = @sepelioFecha,
                   CRECE_GRUPO = @creceGrupo, TIPO = @tipo, ID_PROFESIONAL = @idProfesional, ID_APT_CATEGORIA = @idAptCategoria,
                   APLICA_MORA = @aplicaMora, APLICA_PAGO_MASIVO = @aplicaPagoMasivo, modifica_usuario = @modificaUsuario, modifica_fecha = GETDATE()
             WHERE id_beneficio = @idBeneficio";
    }
}
