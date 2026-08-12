using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficioAsgDB
    {
        /// <summary>
        /// Guarda la asignación del beneficio (monetario o de productos), validando datos y parcialidad.
        /// </summary>
        public ErrorDto AfBeneficioAsg_Guardar(int CodCliente, string usuario, AfiBeneficioAsgInsertar datos)
        {
            try
            {
                var validacion = AF_BeneficioAsg_ValidarSolicitud(datos);
                if (validacion != null)
                {
                    return validacion;
                }

                var afiBeneficios = AfiBeneficioDTO_Obtener(CodCliente, datos.cod_beneficio ?? string.Empty).Result;
                if (afiBeneficios == null)
                {
                    return new ErrorDto { Code = -1, Description = "No se encontró la definición del beneficio" };
                }

                _bAplicaParcial = afiBeneficios.aplica_parcial == 1;

                if (afiBeneficios.aplica_beneficiarios == 1
                    && (string.IsNullOrWhiteSpace(datos.solicita) || string.IsNullOrWhiteSpace(datos.solicita_nombre)))
                {
                    return new ErrorDto { Code = -1, Description = "Verifique los datos del Fallecido" };
                }

                var info = datos.tipoBeneficio switch
                {
                    "M" => GuardarBeneficioMonetario(CodCliente, datos, usuario),
                    "P" => GuardarBeneficioProducto(CodCliente, datos, usuario),
                    _ => new ErrorDto { Code = -1, Description = "El tipo de beneficio no es válido" }
                };

                if (info.Code == -1)
                {
                    return info;
                }

                info.Description = "Información guardada Satisfactoriamente";
                return info;
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Valida los datos mínimos y los montos de la solicitud antes de iniciar operaciones de base de datos.
        /// </summary>
        private static ErrorDto? AF_BeneficioAsg_ValidarSolicitud(AfiBeneficioAsgInsertar datos)
        {
            if (string.IsNullOrWhiteSpace(datos.cedula))
            {
                return new ErrorDto { Code = -1, Description = "La cédula es requerida" };
            }

            if (string.IsNullOrWhiteSpace(datos.cod_beneficio)
                || string.IsNullOrWhiteSpace(datos.tipoBeneficio)
                || string.IsNullOrWhiteSpace(datos.estado))
            {
                return new ErrorDto { Code = -1, Description = "Complete beneficio, tipo y estado" };
            }

            if (datos.monto.GetValueOrDefault() < 0
                || datos.montoGira.GetValueOrDefault() < 0
                || datos.disponible.GetValueOrDefault() < 0)
            {
                return new ErrorDto { Code = -1, Description = "Los montos del beneficio no pueden ser negativos" };
            }

            if (datos.tipoBeneficio == "P")
            {
                return AF_BeneficioAsg_ValidarProductos(datos);
            }

            return null;
        }

        /// <summary>
        /// Valida cantidad, costo y tope de los productos incluidos en la asignación.
        /// </summary>
        private static ErrorDto? AF_BeneficioAsg_ValidarProductos(AfiBeneficioAsgInsertar datos)
        {
            var productos = datos.productos ?? new List<AfBeneAsgProductoData>();
            if (productos.Count == 0)
            {
                return new ErrorDto { Code = -1, Description = "Agregue al menos un producto" };
            }

            if (productos.Any(producto => producto.cantidad <= 0 || producto.costo_unidad < 0))
            {
                return new ErrorDto { Code = -1, Description = "Revise la cantidad y el costo de los productos" };
            }

            var totalProductos = productos.Sum(producto => producto.cantidad * producto.costo_unidad);
            return totalProductos > datos.monto.GetValueOrDefault()
                ? new ErrorDto { Code = -1, Description = "El total de productos excede el monto del beneficio" }
                : null;
        }

        /// <summary>
        /// Resuelve el flujo del beneficio monetario según parcialidad y disponible.
        /// </summary>
        private ErrorDto GuardarBeneficioMonetario(int CodCliente, AfiBeneficioAsgInsertar datos, string usuario)
        {
            if (_bAplicaParcial)
            {
                if (datos.disponible > 0)
                {
                    datos.monto = datos.montoGira;
                }
                return Guardar_Beneficio(CodCliente, datos, "S", usuario);
            }

            if (datos.disponible == 0 && datos.solicita != null)
            {
                return Guardar_Beneficio(CodCliente, datos, "N", usuario);
            }

            return new ErrorDto { Code = -1, Description = "No ha distribuido el disponible" };
        }

        /// <summary>
        /// Resuelve el flujo del beneficio de productos.
        /// </summary>
        private ErrorDto GuardarBeneficioProducto(int CodCliente, AfiBeneficioAsgInsertar datos, string usuario)
        {
            if (datos.productos != null && datos.productos.Count > 0)
            {
                return Guarda_Productos(CodCliente, datos, "N", usuario);
            }

            return new ErrorDto { Code = -1, Description = "No se almacenó la información" };
        }

        /// <summary>
        /// Inserta o actualiza el beneficio monetario (afi_bene_otorga + afi_bene_pago) y deja traza/tags.
        /// </summary>
        private ErrorDto Guardar_Beneficio(int CodCliente, AfiBeneficioAsgInsertar datos, string modificaMonto, string usuario)
        {
            var esNuevo = string.IsNullOrEmpty(datos.txtBeneficioId);

            if (esNuevo)
            {
                var empresa = CargaOficinas(CodCliente, usuario);
                if (empresa.Result == null || empresa.Result.Count == 0)
                {
                    return new ErrorDto { Code = -1, Description = "No se encontró la oficina del usuario" };
                }

                var titular = empresa.Result[0].Titular;
                return InsertarBeneficioMonetario(CodCliente, datos, modificaMonto, usuario, titular);
            }

            return ActualizarBeneficioMonetario(CodCliente, datos, modificaMonto, usuario);
        }

        private ErrorDto InsertarBeneficioMonetario(int CodCliente, AfiBeneficioAsgInsertar datos, string modificaMonto, string usuario, string titular)
        {
            const string sqlOtorga = @"
                INSERT afi_bene_otorga (consec, cod_beneficio, cedula, monto, modifica_monto, registra_user, registra_fecha,
                                        estado, notas, Solicita, nombre, tipo, cod_oficina)
                VALUES (@consec, @codBeneficio, @cedula, @monto, @modificaMonto, @usuario, GETDATE(),
                        @estado, @notas, @solicita, @nombre, @tipo, @codOficina)";

            const string sqlPago = @"
                INSERT afi_bene_pago (cedula, consec, cod_beneficio, tipo, monto, cod_banco, tipo_emision, cta_bancaria, estado)
                VALUES (@solicita, @consec, @codBeneficio, @tipo, @monto, @codBanco, @emitir, @codCuenta, @estado)";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.Serializable);

                try
                {
                    var vBeneConsec = connection.QueryFirstOrDefault<long>(
                        @"SELECT ISNULL(MAX(consec), 0) + 1
                            FROM afi_bene_otorga WITH (UPDLOCK, HOLDLOCK)
                           WHERE cod_beneficio = @codBeneficio",
                        new { codBeneficio = datos.cod_beneficio }, transaction);

                    var filas = connection.Execute(sqlOtorga, new
                    {
                        consec = vBeneConsec,
                        codBeneficio = datos.cod_beneficio,
                        cedula = datos.cedula.Trim(),
                        monto = datos.monto,
                        modificaMonto,
                        usuario = usuario.ToUpper(),
                        estado = datos.estado,
                        notas = datos.notas,
                        solicita = datos.solicita,
                        nombre = (datos.solicita_nombre ?? string.Empty).ToUpper(),
                        tipo = datos.tipoBeneficio,
                        codOficina = titular
                    }, transaction);

                    if (filas <= 0)
                    {
                        transaction.Rollback();
                        return 0L;
                    }

                    var filasPago = connection.Execute(sqlPago, new
                    {
                        solicita = datos.solicita,
                        consec = vBeneConsec,
                        codBeneficio = datos.cod_beneficio,
                        tipo = datos.tipoBeneficio,
                        monto = datos.monto,
                        codBanco = datos.cod_banco,
                        emitir = datos.emitir,
                        codCuenta = datos.cod_cuenta,
                        estado = datos.estado
                    }, transaction);

                    if (filasPago <= 0)
                    {
                        transaction.Rollback();
                        return 0L;
                    }

                    transaction.Commit();
                    return vBeneConsec;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });

            if (result.Code != 0 || result.Result <= 0)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = result.Code != 0 ? result.Description : "Error al insertar el registro"
                };
            }

            var vBeneConsec = result.Result;
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodCliente,
                    Usuario = usuario.ToUpper(),
                    DetalleMovimiento = $"Registra, Beneficio:{vBeneConsec}-{datos.cod_beneficio}, Cedula [{datos.cedula.Trim()}]",
                    Movimiento = "REGISTRA - WEB",
                    Modulo = 7
                });

                SbSIFRegistraTags(new SifRegistraTagsRequestDto
                {
                    Codigo = vBeneConsec.ToString(),
                    Tag = "S.BEN.01",
                    Usuario = usuario.ToUpper(),
                    Observacion = "Reg. Ben",
                    Documento = datos.cod_beneficio ?? string.Empty,
                    Modulo = "BEN"
                });

            return new ErrorDto { Code = 0, Description = "Información guardada satisfactoriamente" };
        }

        private ErrorDto ActualizarBeneficioMonetario(int CodCliente, AfiBeneficioAsgInsertar datos, string modificaMonto, string usuario)
        {
            const string sqlOtorga = @"
                UPDATE afi_bene_otorga
                   SET notas = @notas, estado = @estado, modifica_monto = @modificaMonto, solicita = @solicita,
                       monto = @monto, nombre = @nombre, TIPO = @tipo
                 WHERE cod_beneficio = @codBeneficio AND cedula = @cedula AND consec = @consec";

            const string sqlPago = @"
                UPDATE afi_bene_pago
                   SET monto = @monto, tipo = @tipo, tipo_emision = @emitir, cta_bancaria = @codCuenta, cod_banco = @codBanco, estado = @estado
                 WHERE cod_beneficio = @codBeneficio AND consec = @consec";

            const string sqlInsertPago = @"
                INSERT afi_bene_pago (cedula, consec, cod_beneficio, tipo, monto, cod_banco, tipo_emision, cta_bancaria, estado)
                VALUES (@solicita, @consec, @codBeneficio, @tipo, @monto, @codBanco, @emitir, @codCuenta, @estado)";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                using var transaction = connection.BeginTransaction();

                try
                {
                    var filas = connection.Execute(sqlOtorga, new
                    {
                        notas = datos.notas,
                        estado = datos.estado,
                        modificaMonto,
                        solicita = datos.solicita,
                        monto = datos.monto,
                        nombre = datos.solicita_nombre,
                        tipo = datos.tipoBeneficio,
                        codBeneficio = datos.cod_beneficio,
                        cedula = datos.cedula.Trim(),
                        consec = datos.consec
                    }, transaction);

                    if (filas <= 0)
                    {
                        transaction.Rollback();
                        return 0;
                    }

                    var parametrosPago = new
                    {
                        monto = datos.monto,
                        tipo = datos.tipoBeneficio,
                        emitir = datos.emitir,
                        codCuenta = datos.cod_cuenta,
                        codBanco = datos.cod_banco,
                        estado = datos.estado,
                        codBeneficio = datos.cod_beneficio,
                        solicita = (datos.solicita ?? string.Empty).Trim(),
                        consec = datos.consec
                    };

                    var filasPago = connection.Execute(sqlPago, parametrosPago, transaction);
                    if (filasPago == 0)
                    {
                        filasPago = connection.Execute(sqlInsertPago, parametrosPago, transaction);
                    }

                    if (filasPago <= 0)
                    {
                        transaction.Rollback();
                        return 0;
                    }

                    transaction.Commit();
                    return filas;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = result.Description };
            }

            if (result.Result > 0)
            {
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodCliente,
                    Usuario = usuario.ToUpper(),
                    DetalleMovimiento = $"Modifica, Beneficio:{datos.consec}-{datos.cod_beneficio}, Cedula [{datos.cedula.Trim()}]",
                    Movimiento = "MODIFICA - WEB",
                    Modulo = 7
                });
            }

            return result.Result > 0
                ? new ErrorDto { Code = 0 }
                : new ErrorDto { Code = -1, Description = "Error al actualizar el registro" };
        }

        /// <summary>
        /// Inserta o actualiza el beneficio de productos (afi_bene_otorga + afi_bene_prodasg) y deja traza.
        /// </summary>
        private ErrorDto Guarda_Productos(int CodCliente, AfiBeneficioAsgInsertar datos, string modificaMonto, string usuario)
        {
            var esNuevo = string.IsNullOrEmpty(datos.txtBeneficioId);
            var solicitud = new BeneficioProductosGuardarRequest
            {
                Datos = datos,
                ModificaMonto = modificaMonto,
                Usuario = usuario,
                EsNuevo = esNuevo
            };
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente,
                connection => AF_BeneficioAsg_Productos_Procesar(connection, solicitud));

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = result.Description };
            }

            var mensajeError = esNuevo ? "Error al insertar el registro" : "Error al actualizar el registro";

            if (result.Result > 0)
            {
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodCliente,
                    Usuario = usuario.ToUpper(),
                    DetalleMovimiento = $"{(esNuevo ? "Registra" : "Modifica")}, Beneficio:{result.Result}-{datos.cod_beneficio}, Cedula [{(datos.cedula ?? string.Empty).Trim()}]",
                    Movimiento = esNuevo ? "REGISTRA - WEB" : "MODIFICA - WEB",
                    Modulo = 7
                });
            }

            return result.Result > 0
                ? new ErrorDto { Code = 0, Description = "Información guardada satisfactoriamente" }
                : new ErrorDto { Code = -1, Description = mensajeError };
        }
    }
}
