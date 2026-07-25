using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public partial class FrmFslRemesasPagoDB
    {
        /// <summary>
        /// Obtiene las remesas cerradas listas para trasladar.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Lista de remesas.</returns>
        public ErrorDto<List<FslRemesasListaDatos>> FslTraslados_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                const string sql = @"SELECT *, CONCAT(TESORERIA_REMESA, REGISTRO_USUARIO, REGISTRO_FECHA, FECHA_INICIO, FECHA_CORTE) AS DESCRIPCION
                                     FROM FSL_REMESAS_TESORERIA WHERE estado = 'C' ORDER BY REGISTRO_fecha DESC";
                return connection.Query<FslRemesasListaDatos>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene los expedientes de una remesa pendientes de traslado a tesorería.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="fecha_inicio">Fecha inicial.</param>
        /// <param name="fecha_corte">Fecha de corte.</param>
        /// <param name="cod_remesa">Código de la remesa.</param>
        /// <returns>Lista de expedientes.</returns>
        public ErrorDto<List<FslTrasladoListaData>> FslTrasladoLista_Obtener(int CodEmpresa, string fecha_inicio, string fecha_corte, int cod_remesa)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                const string sql = @"SELECT E.COD_EXPEDIENTE AS cod_expediente, E.CEDULA AS cedula, S.NOMBRE AS nombre,
                                            E.TOTAL_SOBRANTE AS total_sobrante, E.PRESENTA_CEDULA AS presenta_cedula, E.PRESENTA_NOMBRE AS presenta_nombre
                                     FROM FSL_EXPEDIENTES E
                                     INNER JOIN SOCIOS S ON E.CEDULA = S.CEDULA
                                     WHERE E.RESOLUCION_FECHA BETWEEN @fecha_inicio AND @fecha_corte
                                       AND E.TESORERIA_REMESA = @cod_remesa AND E.Tipo_Desembolso = 'T'
                                       AND E.Estado = 'X' AND E.TOTAL_SOBRANTE > 0 AND ISNULL(E.Tesoreria_Solicitud, 0) = 0
                                     ORDER BY E.CEDULA, S.NOMBRE";
                return connection.Query<FslTrasladoListaData>(sql, new { fecha_inicio, fecha_corte, cod_remesa }).ToList();
            });
        }

        /// <summary>
        /// Aplica el traslado a tesorería de los expedientes de una remesa: genera la solicitud de tesorería,
        /// crea el detalle contable, actualiza el expediente y cierra la remesa como trasladada.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="traslados">JSON con el traslado y los casos.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslTraslado_Aplicar(int CodEmpresa, string traslados)
        {
            var traslado = JsonConvert.DeserializeObject<FslTrasladoAplicar>(traslados) ?? new FslTrasladoAplicar();

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                var config = new TrasladoConfig
                {
                    Cuenta = _mBeneficiosDB.fxFSL_Parametros(CodEmpresa, "01"),
                    Concepto = _mBeneficiosDB.fxFSL_Parametros(CodEmpresa, "05"),
                    Unidad = _mBeneficiosDB.fxFSL_Parametros(CodEmpresa, "07"),
                    Token = connection.QueryFirstOrDefault<string>("SELECT TOP 1 id_token FROM tes_tokens WHERE estado = 'A' ORDER BY registro_fecha") ?? string.Empty
                };

                var casos = 0;
                foreach (var item in traslado.casos)
                {
                    ProcesarTrasladoCaso(CodEmpresa, connection, traslado, item, config);
                    casos++;
                }

                if (casos > 0)
                {
                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = traslado.usuario.ToUpper(),
                        DetalleMovimiento = "Carga Remesa Traslado a Tesoreria :" + traslado.codTraslado,
                        Movimiento = "Aplica - WEB",
                        Modulo = 7
                    });

                    connection.Execute("UPDATE FSL_REMESAS_TESORERIA SET estado = 'T' WHERE TESORERIA_REMESA = @codTraslado",
                        new { traslado.codTraslado });
                }

                return DbHelper.OkResponse("Traslado a Tesoreria realizado satisfactoriamente...");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Procesa el traslado de un expediente: resuelve el medio de pago, genera la solicitud de tesorería,
        /// el detalle contable, actualiza el expediente y deja traza.
        /// </summary>
        private void ProcesarTrasladoCaso(int CodEmpresa, SqlConnection connection, FslTrasladoAplicar traslado, FslTrasladoListaData item, TrasladoConfig config)
        {
            var cuentaAhorros = connection.QueryFirstOrDefault<FslCuentaAhorrosDatos>(
                "SELECT TOP 1 * FROM cuentas_Ahorros WHERE Tipo = 1 AND cedula = @cedula ORDER BY Prioridad", new { item.cedula });

            string tipo, cuenta, banco;
            if (cuentaAhorros != null)
            {
                tipo = "TE";
                cuenta = cuentaAhorros.cuenta;
                banco = cuentaAhorros.id_banco;
            }
            else
            {
                tipo = "CK";
                cuenta = string.Empty;
                banco = _mBeneficiosDB.fxFSL_Parametros(CodEmpresa, "04");
            }

            var solicitud = fxMaestroTesoreria(CodEmpresa, new TrasladoTesoreriaParams
            {
                TipoDocumento = tipo,
                Banco = int.Parse(banco),
                Monto = item.total_sobrante,
                Codigo = item.cod_expediente,
                Beneficiario = item.nombre,
                Detalle1 = "Exp.: " + item.cod_expediente,
                Cuenta = cuenta,
                Fecha = DateTime.Now,
                Unidad = config.Unidad,
                Token = config.Token,
                Usuario = traslado.usuario,
                CodTraslado = traslado.codTraslado,
                Concepto = config.Concepto
            });

            sbCreaDetalle(CodEmpresa, solicitud, fxTraeCuentaBanco(CodEmpresa, banco), item.total_sobrante, "H", 1, config.Unidad);
            sbCreaDetalle(CodEmpresa, solicitud, config.Cuenta, item.total_sobrante, "D", 1, config.Unidad);

            connection.Execute(@"UPDATE FSL_EXPEDIENTES
                                 SET Tesoreria_Solicitud = @solicitud, Tesoreria_Fecha = GETDATE(), Tesoreria_Usuario = @usuario
                                 WHERE TESORERIA_REMESA = @codTraslado AND cod_expediente = @cod_expediente",
                new { solicitud, usuario = traslado.usuario, traslado.codTraslado, item.cod_expediente });

            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = traslado.usuario.ToUpper(),
                DetalleMovimiento = "Traspaso a Tesoreria - Expediente :" + item.cod_expediente,
                Movimiento = "Registra - WEB",
                Modulo = 7
            });
        }

        /// <summary>
        /// Inserta el maestro de la solicitud de tesorería y devuelve su número de solicitud.
        /// </summary>
        private long fxMaestroTesoreria(int CodEmpresa, TrasladoTesoreriaParams p)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                var autoriza = p.TipoDocumento == "CK" ? "S" : "N";
                var userAutoriza = p.TipoDocumento == "CK" ? p.Usuario : null;

                const string sql = @"INSERT Tes_Transacciones
                                        (cod_concepto, cod_unidad, id_banco, tipo, codigo, beneficiario, monto, fecha_solicitud,
                                         estado, estadoi, modulo, submodulo, cta_ahorros, detalle1, detalle2, referencia, op,
                                         genera, actualiza, user_solicita, autoriza, user_autoriza, fecha_autorizacion,
                                         ID_TOKEN, REMESA_TIPO, REMESA_ID)
                                     VALUES
                                        (@concepto, @unidad, @banco, @tipoDocumento, @codigo, @beneficiario, @monto, @fecha,
                                         'P', 'P', 'CC', 'C', @cuenta, @detalle1, @detalle2, @referencia, @op,
                                         'S', 'S', @usuario, @autoriza, @userAutoriza, CASE WHEN @autoriza = 'S' THEN GETDATE() ELSE NULL END,
                                         @token, 'FSL', @codTraslado)";

                connection.Execute(sql, new
                {
                    p.Concepto,
                    p.Unidad,
                    p.Banco,
                    p.TipoDocumento,
                    p.Codigo,
                    p.Beneficiario,
                    p.Monto,
                    p.Fecha,
                    p.Cuenta,
                    p.Detalle1,
                    detalle2 = p.Detalle2,
                    p.Referencia,
                    p.Op,
                    p.Usuario,
                    autoriza,
                    userAutoriza,
                    p.Token,
                    p.CodTraslado
                });

                var solicitud = connection.QueryFirstOrDefault<long>("SELECT MAX(nsolicitud) AS Solicitud FROM Tes_Transacciones");
                var info = connection.QueryFirstOrDefault<FslTesTransaccionesData>(
                    "SELECT * FROM Tes_Transacciones WHERE nsolicitud = @solicitud", new { solicitud });

                if (info != null && info.codigo == p.Codigo.Trim())
                {
                    return info.nsolicitud;
                }

                return connection.QueryFirstOrDefault<long>(
                    "SELECT MAX(nsolicitud) AS Solicitud FROM Tes_Transacciones WHERE codigo = @codigo", new { codigo = p.Codigo });
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Inserta una línea de detalle contable de la solicitud de tesorería.
        /// </summary>
        private void sbCreaDetalle(int CodEmpresa, long vSolicitud, string vCtaConta, float vMonto, string vDH, int vLinea, string vUnidad)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                const string sql = @"INSERT Tes_Trans_Asiento (nsolicitud, cuenta_contable, monto, debehaber, linea, cod_unidad)
                                     VALUES (@vSolicitud, @cuenta, @vMonto, @vDH, @vLinea, @vUnidad)";
                connection.Execute(sql, new { vSolicitud, cuenta = vCtaConta.Trim(), vMonto, vDH, vLinea, vUnidad });
            }
            catch (Exception)
            {
                // El original ignora el error de detalle; se conserva ese comportamiento.
            }
        }

        /// <summary>
        /// Obtiene la cuenta contable de un banco.
        /// </summary>
        private string fxTraeCuentaBanco(int CodEmpresa, string vBanco)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<string>("SELECT ctaconta FROM tes_bancos WHERE id_banco = @vBanco", new { vBanco }));

            return result.Result ?? "0";
        }

        /// <summary>Configuración base del traslado (parámetros SIF y token).</summary>
        private sealed class TrasladoConfig
        {
            public string Cuenta { get; set; } = string.Empty;
            public string Concepto { get; set; } = string.Empty;
            public string Unidad { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
        }

        /// <summary>Parámetros para la creación del maestro de tesorería (Regla 31: agrupados en modelo).</summary>
        private sealed class TrasladoTesoreriaParams
        {
            public string TipoDocumento { get; set; } = string.Empty;
            public int Banco { get; set; }
            public float Monto { get; set; }
            public string Codigo { get; set; } = string.Empty;
            public string Beneficiario { get; set; } = string.Empty;
            public long Op { get; set; }
            public string Detalle1 { get; set; } = string.Empty;
            public long Referencia { get; set; }
            public string Detalle2 { get; set; } = string.Empty;
            public string Cuenta { get; set; } = string.Empty;
            public DateTime Fecha { get; set; }
            public string Unidad { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
            public long CodTraslado { get; set; }
            public string Concepto { get; set; } = string.Empty;
        }
    }
}
