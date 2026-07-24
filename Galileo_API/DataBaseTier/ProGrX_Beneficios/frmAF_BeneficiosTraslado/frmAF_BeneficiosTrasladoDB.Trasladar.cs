using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosTrasladoDB
    {
        /// <summary>
        /// Obtiene las remesas cerradas listas para trasladar.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de remesas.</returns>
        public ErrorDto<List<AfiBeneficiosRemesasDto>> AfiTraslados_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT *, CONCAT(COD_REMESA, USUARIO, FECHA, FECHA_INICIO, FECHA_CORTE) AS DESCRIPCION
                                     FROM AFI_BENEFICIOS_REMESAS WHERE estado = 'C' ORDER BY fecha DESC";
                return connection.Query<AfiBeneficiosRemesasDto>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene los beneficios de una remesa lista para traslado con filtro y paginación.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con los filtros.</param>
        /// <returns>Lista de beneficios y total.</returns>
        public ErrorDto<AfiBeneficiosCargasDataLista> AfiTraslado_Obtener(int CodCliente, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<AfiBeneficiosTrasladoDto>(filtros) ?? new AfiBeneficiosTrasladoDto();

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new AfiBeneficiosCargasDataLista();

                const string sqlCount = @"SELECT COUNT(B.Cedula)
                                          FROM afi_bene_pago B
                                          INNER JOIN socios S ON B.cedula = S.cedula
                                          INNER JOIN afi_bene_otorga O ON B.cod_beneficio = O.cod_beneficio AND B.consec = O.consec
                                          INNER JOIN Afi_Estados_Persona E ON S.EstadoActual = E.Cod_Estado
                                          INNER JOIN Tes_Bancos Ban ON B.cod_Banco = Ban.id_Banco
                                          WHERE O.cod_remesa = @cod_remesa
                                            AND O.registra_fecha BETWEEN @fecha_inicio AND @fecha_corte
                                            AND O.ESTADO IN (SELECT COD_ESTADO FROM AFI_BENE_ESTADOS WHERE P_FINALIZA = 1 AND PROCESO = 'A')
                                            AND B.tesoreria IS NULL";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount, new { filtro.cod_remesa, filtro.fecha_inicio, filtro.fecha_corte });

                var parametros = new DynamicParameters();
                parametros.Add("cod_remesa", filtro.cod_remesa);
                parametros.Add("fecha_inicio", filtro.fecha_inicio);
                parametros.Add("fecha_corte", filtro.fecha_corte);

                var vfiltro = string.Empty;
                if (!string.IsNullOrEmpty(filtro.vfiltro))
                {
                    vfiltro = @" AND (B.cedula LIKE @vfiltro OR B.cta_Bancaria LIKE @vfiltro OR O.Nombre LIKE @vfiltro OR Ban.Descripcion LIKE @vfiltro)";
                    parametros.Add("vfiltro", $"{filtro.vfiltro}%");
                }

                var paginado = string.Empty;
                if (filtro.pagina != null)
                {
                    paginado = " ORDER BY B.cod_Beneficio OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY ";
                    parametros.Add("offset", filtro.pagina);
                    parametros.Add("fetch", filtro.paginacion);
                }

                var sql = $@"SELECT B.*, S.Nombre, E.Descripcion AS 'EstadoPersona', Ban.Descripcion AS 'BancoDesc',
                                    O.cod_remesa, O.registra_fecha, O.ID_BENEFICIO,
                                    (SELECT DESCRIPCION FROM AFI_BENEFICIOS WHERE COD_BENEFICIO = B.COD_BENEFICIO) AS BENEFICIO_DESC, B.id_pago
                             FROM afi_bene_pago B
                             INNER JOIN socios S ON B.cedula = S.cedula
                             INNER JOIN afi_bene_otorga O ON B.cod_beneficio = O.cod_beneficio AND B.consec = O.consec
                             INNER JOIN Afi_Estados_Persona E ON S.EstadoActual = E.Cod_Estado
                             INNER JOIN Tes_Bancos Ban ON B.cod_Banco = Ban.id_Banco
                             WHERE O.cod_remesa = @cod_remesa
                               AND B.registro_fecha BETWEEN @fecha_inicio AND @fecha_corte
                               AND O.ESTADO IN (SELECT COD_ESTADO FROM AFI_BENE_ESTADOS WHERE P_FINALIZA = 1 AND PROCESO = 'A')
                               AND B.tesoreria IS NULL {vfiltro} {paginado}";

                response.Beneficios = connection.Query<AfiBeneficiosCargasData>(sql, parametros).ToList();
                return response;
            });
        }

        /// <summary>
        /// Aplica el traslado a tesorería de los beneficios de una remesa: genera maestros, actualiza estados,
        /// crea el detalle contable (con o sin comisión), deja traza y cierra la remesa como trasladada.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="traslado">JSON con la remesa, usuario y casos.</param>
        /// <returns>Resultado de la operación.</returns>
        public Task<ErrorDto> AfiTraslado_Aplicar(int CodCliente, string traslado)
        {
            var infoCarga = JsonConvert.DeserializeObject<AfiTrasladoAplicar>(traslado) ?? new AfiTrasladoAplicar();

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var vToken = ObtenerTokenTraslado(connection, CodCliente, infoCarga);
                var parametros = ObtenerParametrosTraslado(connection);

                foreach (var item in infoCarga.casos)
                {
                    ProcesarTrasladoCaso(connection, CodCliente, infoCarga, item, vToken, parametros);
                }

                connection.Execute("UPDATE AFI_BENEFICIOS_REMESAS SET Estado = 'T' WHERE cod_remesa = @cod_remesa", new { infoCarga.cod_remesa });

                return Task.FromResult(new ErrorDto { Code = 0 });
            }
            catch (Exception ex)
            {
                return Task.FromResult(DbHelper.ErrorResponse(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene el token de tesorería (recibido, activo existente o nuevo).
        /// </summary>
        private string ObtenerTokenTraslado(SqlConnection connection, int CodCliente, AfiTrasladoAplicar infoCarga)
        {
            if (infoCarga.token != null)
            {
                return infoCarga.token;
            }

            var existe = connection.QueryFirstOrDefault<string>("SELECT TOP 1 id_token FROM tes_tokens WHERE estado = 'A' ORDER BY registro_fecha");
            return existe ?? _mTes.fxTesToken(CodCliente, infoCarga.usuario);
        }

        /// <summary>
        /// Obtiene los parámetros SIF de comisión, concepto y cuenta de comisión.
        /// </summary>
        private ParametrosTraslado ObtenerParametrosTraslado(SqlConnection connection)
        {
            const string sql = "SELECT VALOR FROM SIF_PARAMETROS WHERE COD_PARAMETRO = @cod";
            return new ParametrosTraslado
            {
                ValorComision = float.Parse(connection.QueryFirstOrDefault<string>(sql, new { cod = _codComision }) ?? "0"),
                ValorConcepto = connection.QueryFirstOrDefault<string>(sql, new { cod = "AFIBENE" }) ?? string.Empty,
                CtaComision = connection.QueryFirstOrDefault<string>(sql, new { cod = _ctaComision }) ?? string.Empty
            };
        }

        /// <summary>
        /// Procesa el traslado de un caso: maestro de tesorería, actualización de pago/otorga, detalle y bitácora.
        /// </summary>
        private void ProcesarTrasladoCaso(SqlConnection connection, int CodCliente, AfiTrasladoAplicar infoCarga,
            AfiBeneTrasladoAplciar item, string vToken, ParametrosTraslado parametros)
        {
            var vCtaConcepto = connection.QueryFirstOrDefault<string>("SELECT CTACONTA FROM TES_BANCOS WHERE ID_BANCO = @cod_banco", new { item.cod_banco });
            var beneficio = connection.QueryFirstOrDefault<AfiBeneficiosTraslado>(
                "SELECT descripcion, cod_cuenta FROM afi_beneficios WHERE cod_beneficio = @cod_beneficio", new { item.cod_beneficio })
                ?? new AfiBeneficiosTraslado();

            var vTesoreria = _mTes.fxgTesoreriaMaestro(CodCliente, infoCarga.usuario, new TesoreriaMaestroModel
            {
                vTipoDocumento = item.tipo_emision,
                vBanco = item.cod_banco,
                vMonto = item.monto,
                vBeneficiario = item.nombre,
                vCodigo = item.cedula,
                vOP = 0,
                vDetalle1 = item.cod_beneficio,
                vReferencia = 0,
                vDetalle2 = beneficio.descripcion,
                vCuenta = item.cta_bancaria,
                vConcepto = parametros.ValorConcepto,
                vUnidad = "OC",
                vFecha = DateTime.Now.ToString(FechaFormat),
                vRemesa = infoCarga.cod_remesa,
                vRemesaTipo = "BEN",
                vCodApp = "ProGrX-Web",
                vToken = vToken
            });

            ActualizarPagoTraslado(connection, infoCarga, item, vTesoreria, vToken);
            ActualizarOtorgaTraslado(connection, infoCarga, item);
            CrearDetallesTraslado(CodCliente, infoCarga, item, vTesoreria, vCtaConcepto ?? string.Empty, beneficio.cod_cuenta, parametros);

            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
            {
                EmpresaId = CodCliente,
                cod_beneficio = item.cod_beneficio,
                consec = item.consec,
                movimiento = "Actualiza",
                detalle = $"Envio pago a tesoreria Cod.Remesa: [{infoCarga.cod_remesa}]",
                registro_usuario = infoCarga.usuario
            });
        }

        /// <summary>
        /// Actualiza el pago a estado 'E' con la tesorería y el token.
        /// </summary>
        private static void ActualizarPagoTraslado(SqlConnection connection, AfiTrasladoAplicar infoCarga, AfiBeneTrasladoAplciar item, long vTesoreria, string vToken)
        {
            const string sql = @"UPDATE afi_bene_pago
                                 SET estado = 'E', tesoreria = @vTesoreria, envio_user = @usuario, envio_fecha = GETDATE(),
                                     ID_TOKEN = @vToken, cod_remesa = @cod_remesa
                                 WHERE cedula = @cedula AND id_pago = @id_pago AND cod_beneficio = @cod_beneficio AND consec = @consec";
            connection.Execute(sql, new { vTesoreria, infoCarga.usuario, vToken, infoCarga.cod_remesa, item.cedula, item.id_pago, item.cod_beneficio, item.consec });
        }

        /// <summary>
        /// Actualiza el otorgamiento a estado 'A' si aún no tiene remesa asignada.
        /// </summary>
        private static void ActualizarOtorgaTraslado(SqlConnection connection, AfiTrasladoAplicar infoCarga, AfiBeneTrasladoAplciar item)
        {
            const string sqlRemesa = @"SELECT COALESCE((SELECT COD_REMESA FROM afi_bene_otorga
                                                        WHERE cedula = @cedula AND cod_beneficio = @cod_beneficio AND consec = @consec), 0)";
            var existeRemesa = connection.QueryFirstOrDefault<int>(sqlRemesa, new { item.cedula, item.cod_beneficio, item.consec });

            if (existeRemesa == 0)
            {
                const string sql = @"UPDATE afi_bene_otorga
                                     SET estado = 'A', autoriza_user = @usuario, autoriza_fecha = GETDATE(), cod_remesa = @cod_remesa
                                     WHERE cedula = @cedula AND cod_beneficio = @cod_beneficio AND consec = @consec";
                connection.Execute(sql, new { infoCarga.usuario, infoCarga.cod_remesa, item.cedula, item.cod_beneficio, item.consec });
            }
        }

        /// <summary>
        /// Crea el detalle contable de la tesorería, aplicando la comisión cuando corresponde.
        /// </summary>
        private void CrearDetallesTraslado(int CodCliente, AfiTrasladoAplicar infoCarga, AfiBeneTrasladoAplciar item,
            long vTesoreria, string vCtaConcepto, string vCtaBene, ParametrosTraslado parametros)
        {
            if (item.cod_banco != 58 && infoCarga.aplicaComision)
            {
                CrearDetalle(CodCliente, vTesoreria, vCtaConcepto, item.monto - parametros.ValorComision, "H", 1);
                CrearDetalle(CodCliente, vTesoreria, parametros.CtaComision, parametros.ValorComision, "H", 2);
                CrearDetalle(CodCliente, vTesoreria, vCtaBene, item.monto, "D", 3);
            }
            else
            {
                CrearDetalle(CodCliente, vTesoreria, vCtaConcepto, item.monto, "H", 1);
                CrearDetalle(CodCliente, vTesoreria, vCtaBene, item.monto, "D", 2);
            }
        }

        /// <summary>
        /// Inserta una línea de detalle de tesorería.
        /// </summary>
        private void CrearDetalle(int CodCliente, long vTesoreria, string vCtaConta, float vMonto, string vDH, int vLinea)
        {
            _mTes.sbgTesoreriaDetalle(CodCliente, new TesoreriaDetalleModel
            {
                vSolicitud = vTesoreria,
                vCtaConta = vCtaConta,
                vMonto = vMonto,
                vDH = vDH,
                vLinea = vLinea
            });
        }

        /// <summary>
        /// Obtiene los tokens disponibles para la liquidación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario.</param>
        /// <returns>Lista de tokens.</returns>
        public ErrorDto<List<TokenConsultaModel>> Afi_LiqAsientosToken_Obtener(int CodEmpresa, string usuario)
        {
            return _mTesoreria.spTes_Token_Consulta(CodEmpresa, usuario);
        }

        /// <summary>
        /// Genera un nuevo token para la liquidación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Afi_LiqAsientoToken_Nuevo(int CodEmpresa, string usuario)
        {
            return _mTesoreria.spTes_Token_New(CodEmpresa, usuario);
        }

        /// <summary>
        /// Parámetros SIF usados en el traslado (comisión, concepto y cuenta de comisión).
        /// </summary>
        private sealed class ParametrosTraslado
        {
            public float ValorComision { get; set; }
            public string ValorConcepto { get; set; } = string.Empty;
            public string CtaComision { get; set; } = string.Empty;
        }
    }
}
