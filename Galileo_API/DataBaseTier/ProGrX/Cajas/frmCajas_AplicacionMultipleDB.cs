using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasAplicacionMultipleDb
    {
        private readonly PortalDB _portalDb;

        public FrmCajasAplicacionMultipleDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }
        /// <summary>
        /// Obtiene los documentos permitidos para la caja.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_AM_Documentos_Obtener(
            int codEmpresa,
            string codCaja)
        {
            const string sql = @"
                SELECT 
                    RTRIM(C.tipo_documento) AS item,
                    RTRIM(D.Descripcion) AS descripcion
                FROM SIF_DOCUMENTOS D
                INNER JOIN CAJAS_DOCUMENTOS C
                    ON D.TIPO_DOCUMENTO = C.TIPO_DOCUMENTO
                WHERE C.cod_caja = @codCaja
                  AND D.Tipo_Movimiento IN ('A','C')
                ORDER BY C.tipo_documento";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { codCaja });
        }

        /// <summary>
        /// Obtiene el socio y la divisa local para preparar la aplicacion multiple.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<CajasAmClienteInicialDto> Cajas_AM_ClienteInicial_Obtener(
            int codEmpresa,
            string cedula)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
            {
                var socio = cn.QueryFirstOrDefault<CajasAmClienteInicialDto>(
                    "SELECT RTRIM(cedula) AS cedula, RTRIM(nombre) AS nombre FROM SOCIOS WHERE cedula = @cedula",
                    new { cedula });

                if (socio is null)
                {
                    throw new InvalidOperationException("No se encontro operacion para abonos, puede que se encuentre cancelada.");
                }

                socio.divisa = cn.QueryFirstOrDefault<string>(
                    "SELECT RTRIM(COD_DIVISA) FROM vSys_Divisas WHERE DIVISA_LOCAL = 1") ?? string.Empty;

                return socio;
            });
        }

        /// <summary>
        /// Validar Caja AM.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="codApertura"></param>
        /// <param name="sesionId"></param>
        /// <param name="usuario"></param>
        /// <param name="monto"></param>
        /// <param name="tiquete"></param>
        /// <returns></returns>
        public ErrorDto<CajasAmValidacionDto> Cajas_AM_Validar(
            int codEmpresa,
            string codCaja,
            int codApertura,
            int sesionId,
            string usuario,
            decimal monto,
            string tiquete)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
            {
                var estadoCaja = cn.QueryFirstOrDefault<string>(
                    @"SELECT Estado
                      FROM cajas_aperturas_main
                      WHERE cod_caja = @codCaja
                        AND cod_apertura = @codApertura",
                    new { codCaja, codApertura });

                if (string.IsNullOrWhiteSpace(estadoCaja) || estadoCaja.Trim() == "C")
                {
                    return new CajasAmValidacionDto
                    {
                        validacion = $"- La apertura ..:{codApertura} de esta caja ha sido cerrada!"
                    };
                }

                return
                cn.QueryFirstOrDefault<CajasAmValidacionDto>(
                    "spCajas_Transac_Validacion",
                    new
                    {
                        Caja = codCaja,
                        Usuario = usuario,
                        Apertura = codApertura,
                        SesionId = sesionId,
                        TipoProc = "Crd",
                        Producto = "-AM-",
                        Monto = monto,
                        Ticket = tiquete
                    },
                    commandType: CommandType.StoredProcedure
                ) ?? new CajasAmValidacionDto();
            });
        }

        /// <summary>
        /// Obtiene Créditos pendientes.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasCreditoPendienteDto>> Cajas_AM_Creditos_Pendientes(
            int codEmpresa,
            CajasAMCreditosPendientesRequestDto request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
                cn.Query<dynamic>(
                    "spCajas_Crd_Persona_Creditos_Pendientes_Lista",
                    new
                    {
                        Cedula = request.cedula,
                        Caja = request.codcaja,
                        Apertura = request.codapertura,
                        Token = request.tiquete,
                        Corte = request.fechacorte,
                        TipoMov = request.tipomovimiento,
                        PlanCorte = request.plancorte ?? request.fechapago
                    },
                    commandType: CommandType.StoredProcedure
                ).Select(MapCreditoPendiente).ToList());
        }

        /// <summary>
        /// Agrega Créditos al lote.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public ErrorDto<bool> Cajas_AM_Creditos_Agregar(
            int codEmpresa,
            List<CajasAmAgregarRequestDto> items)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
            {
                foreach (var item in items)
                {
                    cn.Execute(
                        "spCajas_AM_Creditos_Add",
                        new
                        {
                            Caja = item.codCaja,
                            Apertura = item.codApertura,
                            Ticket = item.tiquete,
                            Cedula = item.cedula,
                            Operacion = item.operacion,
                            Codigo = item.linea,
                            Tipo = item.tipoAbono,
                            Corte = item.fecha,
                            Abono = item.abono,
                            Compromiso = item.abono,
                            Saldo = item.saldo,
                            IntCor = item.intCor,
                            IntMor = item.intMor,
                            Principal = item.principal,
                            Cargos = item.cargos,
                            Polizas = item.polizas
                        },
                        commandType: CommandType.StoredProcedure
                    );
                }

                return true;
            });
        }

        /// <summary>
        /// Elimina Créditos del lote.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ErrorDto<bool> Cajas_AM_Eliminar(
            int codEmpresa,
            List<long> ids)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
            {
                foreach (var id in ids)
                {
                    cn.Execute(
                        "spCajas_AM_Selected_Del",
                        new { Tipo = "C", Id = id },
                        commandType: CommandType.StoredProcedure
                    );
                }

                return true;
            });
        }

        /// <summary>
        /// Aplica cajas AM.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<long> Cajas_AM_Aplicar(
            int codEmpresa,
            CajasAmAplicarRequestDto request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
            {
                long amId = cn.QuerySingle<long>(
                    "spCajas_AM_Registro_Control",
                    new
                    {
                        Caja = request.codCaja,
                        Apertura = request.codApertura,
                        Token = request.tiquete,
                        Usuario = request.usuario,
                        Cedula = request.cedula,
                        Monto = request.total,
                        Divisa = request.divisa,
                        TipoCambio = request.tipoCambio ?? 1,
                        Notas = request.notas,
                        SesionId = request.sesionId
                    },
                    commandType: CommandType.StoredProcedure
                );

                cn.Execute(
                    "spCajas_AM_Procesa",
                    new
                    {
                        Cedula = request.cedula,
                        Caja = request.codCaja,
                        Apertura = request.codApertura,
                        Token = request.tiquete,
                        Usuario = request.usuario,
                        TipoDoc = request.tipoDocumento,
                        Monto = request.total,
                        Divisa = request.divisa,
                        TipoCambio = request.tipoCambio ?? 1,
                        Notas = request.notas,
                        CajasAM_Id = amId
                    },
                    commandType: CommandType.StoredProcedure
                );

                return amId;
            });
        }

        /// <summary>
        /// Obtiene los Créditos ya seleccionados para el lote de aplicación múltiple.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="codCaja"></param>
        /// <param name="codApertura"></param>
        /// <param name="tiquete"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasAmSeleccionadoDto>> Cajas_AM_Seleccionados(
            int codEmpresa,
            string cedula,
            string codCaja,
            int codApertura,
            string tiquete)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
                cn.Query<dynamic>(
                    "spCajas_Crd_Persona_Creditos_En_Lista",
                    new
                    {
                        Cedula = cedula,
                        Caja = codCaja,
                        Apertura = codApertura,
                        Token = tiquete
                    },
                    commandType: CommandType.StoredProcedure
                ).Select(MapSeleccionado).ToList());
        }

        private static CajasCreditoPendienteDto MapCreditoPendiente(dynamic row)
        {
            var data = (IDictionary<string, object>)row;

            return new CajasCreditoPendienteDto
            {
                operacion = GetValue<long?>(data, "ID_SOLICITUD"),
                linea = GetValue<string>(data, "Codigo"),
                saldo = GetValue<decimal?>(data, "Saldo"),
                abono = GetValue<decimal?>(data, "Compromiso"),
                ultimoPago = GetValue<string>(data, "CtaFechaUltCorte"),
                garantia = GetValue<string>(data, "GarantiaX"),
                descripcion = GetValue<string>(data, "Descripcion"),
                intC = GetValue<decimal?>(data, "IntC"),
                intM = GetValue<decimal?>(data, "IntM"),
                principal = GetValue<decimal?>(data, "Principal"),
                cargos = GetValue<decimal?>(data, "Cargos"),
                polizas = GetValue<decimal?>(data, "Polizas")
            };
        }

        private static CajasAmSeleccionadoDto MapSeleccionado(dynamic row)
        {
            var data = (IDictionary<string, object>)row;

            return new CajasAmSeleccionadoDto
            {
                operacion = GetValue<long?>(data, "ID_SOLICITUD"),
                linea = GetValue<string>(data, "Codigo"),
                saldo = GetValue<decimal?>(data, "Saldo"),
                tipo = GetValue<string>(data, "Tipo_Abono") == "C" ? "Cancelacion" : "Pago Cuota",
                abono = GetValue<decimal>(data, "Abono"),
                garantia = GetValue<string>(data, "Garantia_Desc"),
                descripcion = GetValue<string>(data, "Linea_Desc"),
                creditos_id = GetValue<long?>(data, "Creditos_ID"),
                intCor = GetValue<decimal?>(data, "IntCor"),
                intMor = GetValue<decimal?>(data, "IntMor"),
                principal = GetValue<decimal?>(data, "Amortiza"),
                cargos = GetValue<decimal?>(data, "Cargos"),
                polizas = GetValue<decimal?>(data, "Polizas")
            };
        }

        private static T? GetValue<T>(IDictionary<string, object> data, string key)
        {
            if (!data.TryGetValue(key, out var value))
            {
                var matchingKey = data.Keys.FirstOrDefault(k =>
                    string.Equals(k, key, StringComparison.OrdinalIgnoreCase));

                if (matchingKey is null || !data.TryGetValue(matchingKey, out value))
                {
                    return default;
                }
            }

            if (value is null || value is DBNull)
            {
                return default;
            }

            return (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
        }
    }
}


