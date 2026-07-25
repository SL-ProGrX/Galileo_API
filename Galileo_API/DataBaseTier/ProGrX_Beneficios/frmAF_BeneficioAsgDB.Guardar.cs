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
                if (string.IsNullOrEmpty(datos.cedula))
                {
                    return new ErrorDto { Code = -1, Description = "Cedula no puede ser nula" };
                }

                var afiBeneficios = AfiBeneficioDTO_Obtener(CodCliente, datos.cod_beneficio ?? string.Empty).Result;
                _bAplicaParcial = afiBeneficios?.aplica_parcial == 1;

                if (afiBeneficios?.aplica_beneficiarios == 1 && (datos.solicita == null || datos.solicita_nombre == null))
                {
                    return new ErrorDto { Code = -1, Description = "Verifique los datos del Fallecido" };
                }

                var info = datos.tipoBeneficio switch
                {
                    "M" => GuardarBeneficioMonetario(CodCliente, datos, usuario),
                    "P" => GuardarBeneficioProducto(CodCliente, datos, usuario),
                    _ => new ErrorDto { Code = 0 }
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
                var vBeneConsec = fxConsec(CodCliente, datos.cod_beneficio ?? string.Empty);

                return InsertarBeneficioMonetario(CodCliente, datos, modificaMonto, usuario, vBeneConsec, titular);
            }

            return ActualizarBeneficioMonetario(CodCliente, datos, modificaMonto, usuario);
        }

        private ErrorDto InsertarBeneficioMonetario(int CodCliente, AfiBeneficioAsgInsertar datos, string modificaMonto, string usuario, long vBeneConsec, string titular)
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
                });

                if (filas <= 0)
                {
                    return 0;
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodCliente,
                    Usuario = usuario.ToUpper(),
                    DetalleMovimiento = $"Registra, Beneficio:{vBeneConsec}-{datos.cod_beneficio}, Cedula [{datos.cedula.Trim()}]",
                    Movimiento = "REGISTRA - WEB",
                    Modulo = 7
                });

                connection.Execute(sqlPago, new
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

                return filas;
            });

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = result.Description };
            }

            return result.Result > 0
                ? new ErrorDto { Code = 0, Description = "Informacion Guardada Satisfactoriamente" }
                : new ErrorDto { Code = -1, Description = "Error al insertar el registro" };
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
                 WHERE cod_beneficio = @codBeneficio AND cedula = @solicita AND consec = @consec";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
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
                });

                if (filas <= 0)
                {
                    return 0;
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodCliente,
                    Usuario = usuario.ToUpper(),
                    DetalleMovimiento = $"Modifica, Beneficio:{datos.consec}-{datos.cod_beneficio}, Cedula [{datos.cedula.Trim()}]",
                    Movimiento = "MODIFICA - WEB",
                    Modulo = 7
                });

                connection.Execute(sqlPago, new
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
                });

                return filas;
            });

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = result.Description };
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
            var productos = datos.productos ?? new List<AfBeneAsgProductoData>();

            const string sqlInsertOtorga = @"
                INSERT afi_bene_otorga (consec, cod_beneficio, cedula, monto, modifica_monto, registra_user, registra_fecha,
                                        estado, notas, Solicita, nombre, tipo)
                VALUES (@consec, @codBeneficio, @cedula, @monto, @modificaMonto, @usuario, GETDATE(),
                        @estado, @notas, @solicita, @nombre, @tipo)";

            const string sqlUpdateOtorga = @"
                UPDATE afi_bene_otorga
                   SET notas = @notas, estado = @estado, modifica_monto = @modificaMonto, solicita = @solicita,
                       monto = @monto, nombre = @nombre, TIPO = @tipo
                 WHERE cod_beneficio = @codBeneficio AND cedula = @cedula AND consec = @consec";

            const string sqlInsertProd = @"INSERT afi_bene_prodasg (consec, cod_beneficio, cod_producto, cantidad, costo_unidad)
                                            VALUES (@consec, @codBeneficio, @codProducto, @cantidad, @costoUnidad)";

            const string sqlUpdateProd = @"UPDATE afi_bene_prodasg SET cantidad = @cantidad
                                            WHERE cod_beneficio = @codBeneficio AND consec = @consec AND cod_producto = @codProducto";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                if (esNuevo)
                {
                    var vBeneConsec = fxConsec(CodCliente, datos.cod_beneficio ?? string.Empty);

                    var filas = connection.Execute(sqlInsertOtorga, new
                    {
                        consec = vBeneConsec,
                        codBeneficio = datos.cod_beneficio,
                        cedula = datos.cedula.Trim(),
                        monto = datos.monto,
                        modificaMonto,
                        usuario,
                        estado = datos.estado,
                        notas = datos.notas,
                        solicita = datos.solicita,
                        nombre = (datos.solicita_nombre ?? string.Empty).ToUpper(),
                        tipo = datos.tipoBeneficio
                    });

                    if (filas <= 0)
                    {
                        return 0;
                    }

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodCliente,
                        Usuario = usuario.ToUpper(),
                        DetalleMovimiento = $"Registra, Beneficio:{vBeneConsec}-{datos.cod_beneficio}, Cedula [{datos.cedula.Trim()}]",
                        Movimiento = "REGISTRA - WEB",
                        Modulo = 7
                    });

                    foreach (var prod in productos)
                    {
                        connection.Execute(sqlInsertProd, new
                        {
                            consec = vBeneConsec,
                            codBeneficio = datos.cod_beneficio,
                            codProducto = prod.cod_producto,
                            cantidad = prod.cantidad,
                            costoUnidad = prod.costo_unidad
                        });
                    }

                    return filas;
                }

                var filasUpd = connection.Execute(sqlUpdateOtorga, new
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
                });

                if (filasUpd <= 0)
                {
                    return 0;
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodCliente,
                    Usuario = usuario.ToUpper(),
                    DetalleMovimiento = $"Modifica, Beneficio:{datos.consec}-{datos.cod_beneficio}, Cedula [{datos.cedula.Trim()}]",
                    Movimiento = "MODIFICA - WEB",
                    Modulo = 7
                });

                foreach (var prod in productos)
                {
                    connection.Execute(sqlUpdateProd, new
                    {
                        cantidad = prod.cantidad,
                        codBeneficio = datos.cod_beneficio,
                        consec = datos.consec,
                        codProducto = prod.cod_producto
                    });
                }

                return filasUpd;
            });

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = result.Description };
            }

            var mensajeError = esNuevo ? "Error al insertar el registro" : "Error al actualizar el registro";

            return result.Result > 0
                ? new ErrorDto { Code = 0, Description = "Informacion Guardada Satisfactoriamente" }
                : new ErrorDto { Code = -1, Description = mensajeError };
        }

        /// <summary>
        /// Obtiene el siguiente consecutivo del beneficio.
        /// </summary>
        private long fxConsec(int CodCliente, string cod_beneficio)
        {
            const string sql = "SELECT ISNULL(MAX(consec), 0) AS consecutivo FROM afi_bene_otorga WHERE cod_beneficio = @codBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<long>(sql, new { codBeneficio = cod_beneficio }));

            return result.Code == 0 ? result.Result + 1 : 0;
        }
    }
}
