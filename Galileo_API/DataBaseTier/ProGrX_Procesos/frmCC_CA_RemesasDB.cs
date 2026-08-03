using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.Models.ProGrX_Procesos;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos
{
    public class FrmCcCaRemesasDB
    {
        private readonly PortalDB _portalDB;
        private readonly IConfiguration _config;
        private readonly MRecibos _mRecibos;

        public FrmCcCaRemesasDB(IConfiguration config)
        {
            _config = config;
            _portalDB = new PortalDB(config);
            _mRecibos = new MRecibos(config);
        }

        #region Envio
        /// <summary>
        /// Obtiene los catálogos base de la pantalla de remesas:
        /// líneas, entidades, procesos, cuotas y tipos de filtro.
        /// </summary>
        public ErrorDto<CcCaRemesasCatalogosResponse> CcCaRemesas_Catalogos_Obtener(int codEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            var response = new CcCaRemesasCatalogosResponse();

            try
            {
                const string sqlLineas = @"
                    SELECT
                        COD_REMESA AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM PRM_CA_TIPOS_REMESA
                    WHERE activo = 1
                    ORDER BY COD_REMESA;";

                const string sqlEntidades = @"
                    SELECT
                        RTRIM(cod_entidad) AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM PRM_CA_ENTIDAD
                    WHERE activo = 1
                    ORDER BY cod_entidad;";

                response.lineas = conn.Query<DropDownListaGenericaModel>(sqlLineas).ToList();
                response.entidades = conn.Query<DropDownListaGenericaModel>(sqlEntidades).ToList();

                for (var i = 1; i <= 5; i++)
                {
                    response.cuotas.Add(new DropDownListaGenericaModel
                    {
                        item = i.ToString(),
                        descripcion = i.ToString()
                    });
                }

                response.filtros.Add(new DropDownListaGenericaModel { item = "C", descripcion = "Cédula" });
                response.filtros.Add(new DropDownListaGenericaModel { item = "N", descripcion = "Nombre" });
                response.filtros.Add(new DropDownListaGenericaModel { item = "O", descripcion = "Operación" });

                var hoy = DateTime.Today;
                var periodo = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Local);
                for (var i = 0; i <= 6; i++)
                {
                    var valor = periodo.AddMonths(i).ToString("yyyyMM");
                    response.procesos.Add(new DropDownListaGenericaModel
                    {
                        item = valor,
                        descripcion = valor
                    });
                }

                return DbHelper.CreateOkResponse(response);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CcCaRemesasCatalogosResponse>(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Consulta los casos candidatos a enviar en remesa de cargos automáticos.
        /// </summary>
        public ErrorDto<List<CcCaRemesasEnvioConsultaData>> CcCaRemesas_Envio_Consulta(
            int codEmpresa,
            CcCaRemesasEnvioConsultaRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var lista = conn.Query<CcCaRemesasEnvioConsultaData>(
                    "spPrm_CA_Remesa_Envia_Consulta",
                    new
                    {
                        RemesaTipo = request.cod_remesa,
                        Entidad = request.cod_entidad.Trim(),
                        Fecha = request.fecha_vence.Date,
                        NCuotas = request.cuotas
                    },
                    commandType: CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CcCaRemesasEnvioConsultaData>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene las remesas pendientes de recibir/aplicar.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CcCaRemesas_Recibe_Pendientes_Obtener(int codEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var lista = conn.Query(
                        @"EXEC spPrm_CA_Remesa_Envia_Pendiente")
                        .Select(x => new DropDownListaGenericaModel
                        {
                            item = x.IdX,
                            descripcion = x.ItmX
                        })
                        .ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el detalle de una remesa pendiente o procesada para la pestaña recibe/aplica.
        /// </summary>
        public ErrorDto<List<CcCaRemesasRecibeDetalleData>> CcCaRemesas_Recibe_Detalle_Obtener(int codEmpresa, long remesa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var lista = conn.Query<CcCaRemesasRecibeDetalleData>(
                    "spPrm_CA_Remesa_Consultas",
                    new { NumGeneracion = remesa },
                    commandType: CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CcCaRemesasRecibeDetalleData>>(ex.Message);
            }
        }

        /// <summary>
        /// Valida si existe una remesa pendiente de procesar.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<CcCaRemesasEnvioPendienteData?> CcCaRemesas_Envio_Pendiente_Validar(int codEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var item = conn.QueryFirstOrDefault<CcCaRemesasEnvioPendienteData>(
                    "spPrm_CA_Remesa_Envia_Pendiente",
                    commandType: CommandType.StoredProcedure);

                return DbHelper.CreateOkResponse(item);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CcCaRemesasEnvioPendienteData?>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el siguiente número de generación para registrar una remesa.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<long> CcCaRemesas_Envio_NumeroGeneracion_Obtener(int codEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var item = conn.QueryFirstOrDefault<dynamic>(
                    "spPrm_CA_Remesa_Envia_Numero_Genracion",
                    commandType: CommandType.StoredProcedure);

                long numero = item == null ? 0 : Convert.ToInt64(item.IdGenera);
                return DbHelper.CreateOkResponse(numero);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<long>(ex.Message);
            }
        }

        /// <summary>
        /// Registra la remesa de envío con los casos seleccionados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CcCaRemesas_Envio_Registrar(
            int codEmpresa,
            string usuario,
            CcCaRemesasEnvioRegistrarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                if (request.seleccionados == null || request.seleccionados.Count == 0)
                    return DbHelper.ErrorResponse("No existen registros seleccionados.", -2);

                var procesoTexto = (request.proceso ?? string.Empty).Trim();
                DateTime fechaProceso;

                if (!DateTime.TryParseExact(
                        procesoTexto + "01",
                        "yyyyMMdd",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out fechaProceso))
                {
                    return DbHelper.ErrorResponse("El proceso indicado no es válido.", -2);
                }

                /**
                Linea de codigo anterior
                var fechaCorte = new DateTime(
                    fechaProceso.Year,
                    fechaProceso.Month,
                    DateTime.DaysInMonth(fechaProceso.Year, fechaProceso.Month),
                    0,
                    0,
                    0,
                    DateTimeKind.Unspecified);
                **/

                foreach (var item in request.seleccionados)
                {
                    conn.Execute(
                        "spPrm_CA_Remesa_Envia_Add",
                        new
                        {
                            NumGen = request.numero_generacion,
                            RemesaTipo = request.cod_remesa,
                            Entidad = request.cod_entidad.Trim(),
                            Fecha = request.fecha_vence.Date,
                            Cedula = item.cedula?.Trim() ?? string.Empty,
                            Nombre = item.nombre ?? string.Empty,
                            Tarjeta_Numero = item.tarjeta_numero ?? string.Empty,
                            Tarjeta_Vence = item.tarjeta_vence ?? string.Empty,
                            Tarjeta_Tipo = item.tarjeta_tipo ?? string.Empty,
                            Linea = item.codigo ?? string.Empty,
                            Monto = item.compromiso,
                            Correo = item.correo ?? string.Empty,
                            Operacion = item.id_solicitud,
                            FecUlt = item.fecult ?? string.Empty,
                            TipoCuota = item.tipo ?? string.Empty
                        },
                        commandType: CommandType.StoredProcedure);
                }

                return DbHelper.OkResponse("Información para cobro automático realizada satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }


        /// <summary>
        /// Genera el archivo de salida bancario de la remesa enviada,
        /// según el número de generación indicado.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="numeroGeneracion">Número de generación de la remesa.</param>
        /// <returns>Archivo listo para descarga en base64.</returns>
        public ErrorDto<ArchivoDto> CcCaRemesas_Envio_ArchivoBanco_Obtener(int codEmpresa, long numeroGeneracion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var lista = conn.Query<CcCaRemesasArchivoBancoRow>(
                    "spPrm_CA_Remesa_Archivo_Envia",
                    new { Remesa = numeroGeneracion },
                    commandType: CommandType.StoredProcedure).ToList();

                if (lista.Count == 0)
                    return DbHelper.CreateErrorResponse<ArchivoDto>(
                        "No se encontraron datos para generar el archivo bancario.");

                var formato = (lista[0].Formato ?? string.Empty).Trim().ToUpperInvariant();
                var extension = formato == "BAC" ? "csv" : "txt";
                var contentType = formato == "BAC" ? "text/csv" : "text/plain";

                var sb = new StringBuilder();

                foreach (var item in lista)
                {
                    sb.AppendLine(ConstruirLineaArchivoBanco(formato, item));
                }

                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                var archivo = new ArchivoDto
                {
                    FileName = $"Cargo_Automatico_{numeroGeneracion}_{formato}.{extension}",
                    ContentType = contentType,
                    FileContentsBase64 = Convert.ToBase64String(bytes)
                };

                return DbHelper.CreateOkResponse(archivo);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<ArchivoDto>(ex.Message);
            }
        }

        private static string ConstruirLineaArchivoBanco(string formato, CcCaRemesasArchivoBancoRow item)
        {
            return formato switch
            {
                "BAC" => ConstruirLineaBac(item),
                "BNCR" => ConstruirLineaBncr(item),
                _ => ConstruirLineaDefault(item),
            };
        }

        private static string ConstruirLineaBac(CcCaRemesasArchivoBancoRow item)
        {
            return string.Join(",",
                "1",
                Left(item.Cedula, 25),
                Left(item.Referencia, 25),
                (item.Tarjeta ?? string.Empty).Trim(),
                item.Fecha_Vence.ToString("MMyyyy", CultureInfo.InvariantCulture),
                item.Monto.ToString("########0.##", CultureInfo.InvariantCulture),
                item.Fecha_Transaccion.ToString("ddMMyyyy", CultureInfo.InvariantCulture),
                Left((item.Email ?? string.Empty).Trim(), 30),
                Left((item.Nombre ?? string.Empty).Trim(), 30));
        }

        private static string ConstruirLineaBncr(CcCaRemesasArchivoBancoRow item)
        {
            return PadLeft(item.Tarjeta, '0', 16)
                + item.Monto.ToString("0000000.00", CultureInfo.InvariantCulture)
                + PadLeft(item.NUMERO_AFILIADO, '0', 10)
                + item.Fecha_Transaccion.ToString("ddMMyyyy", CultureInfo.InvariantCulture)
                + item.Fecha_Vence.ToString("MMyyyy", CultureInfo.InvariantCulture)
                + PadRight(item.Referencia, ' ', 40);
        }

        private static string ConstruirLineaDefault(CcCaRemesasArchivoBancoRow item)
        {
            return PadLeft(item.Tarjeta, '0', 16)
                + item.Monto.ToString("0000000.00", CultureInfo.InvariantCulture)
                + PadLeft(item.NUMERO_AFILIADO, '0', 10)
                + item.Fecha_Transaccion.ToString("ddMMyyyy", CultureInfo.InvariantCulture)
                + item.Fecha_Vence.ToString("MMyyyy", CultureInfo.InvariantCulture)
                + PadRight(item.Referencia, ' ', 40);
        }

        private static string Left(string? value, int length)
        {
            var texto = value ?? string.Empty;
            return texto.Length <= length ? texto : texto[..length];
        }

        private static string PadLeft(string? value, char fill, int length)
        {
            return (value ?? string.Empty).Trim().PadLeft(length, fill)[^length..];
        }

        private static string PadRight(string? value, char fill, int length)
        {
            var texto = (value ?? string.Empty).Trim();
            return texto.Length >= length ? texto[..length] : texto.PadRight(length, fill);
        }

        #endregion

        #region Recibo / Aplica

        /// <summary>
        /// Carga las autorizaciones del archivo Excel para una remesa en trámite.
        /// Equivale funcionalmente a sbCargaAutorizaciones del VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Número de generación, usuario y autorizaciones.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto CcCaRemesas_Recibe_Autorizaciones_Cargar(
            int codEmpresa,
            CcCaRemesasRecibeAutorizacionesRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                if (request.autorizaciones == null || request.autorizaciones.Count == 0)
                    return DbHelper.ErrorResponse("No existen autorizaciones para procesar.", -2);

                foreach (var item in request.autorizaciones)
                {
                    var referencia = (item.documento ?? string.Empty).Trim();
                    var autorizacion = (item.transaccion ?? string.Empty).Trim();
                    var estado = string.IsNullOrWhiteSpace(item.estado)
                        ? string.Empty
                        : item.estado.Trim()[0].ToString();

                    conn.Execute(
                        "spPrm_CA_Remesa_Autorizaciones",
                        new
                        {
                            Remesa = request.numero_generacion,
                            Tarjeta = referencia,
                            Autorizacion = autorizacion,
                            Estado = estado,
                            Usuario = (request.usuario ?? string.Empty).Trim()
                        },
                        commandType: CommandType.StoredProcedure);
                }

                return DbHelper.OkResponse("Autorizaciones cargadas correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Cierra una remesa luego de cargar sus autorizaciones.
        /// Equivale al paso spPrm_CA_Remesa_Cierra del VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="numeroGeneracion">Número de generación de la remesa.</param>
        /// <param name="usuario">Usuario que ejecuta el cierre.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto CcCaRemesas_Recibe_Cierra(
            int codEmpresa,
            long numeroGeneracion,
            string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                conn.Execute(
                    "spPrm_CA_Remesa_Cierra",
                    new
                    {
                        Remesa = numeroGeneracion,
                        Usuario = usuario.Trim()
                    },
                    commandType: CommandType.StoredProcedure);

                return DbHelper.OkResponse("Remesa cerrada correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }


        /// <summary>
        /// Aplica los abonos de una remesa cerrada en bloques.
        /// Equivale al paso spPrm_CA_Abonos_Aplica del VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de aplicación.</param>
        /// <returns>Cantidad de pendientes luego del bloque aplicado.</returns>
        public ErrorDto<CcCaRemesasRecibeAplicaResponse> CcCaRemesas_Recibe_Aplica(
            int codEmpresa,
            CcCaRemesasRecibeAplicaRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            var response = new CcCaRemesasRecibeAplicaResponse();

            try
            {
                var row = conn.QueryFirstOrDefault<dynamic>(
                    "spPrm_CA_Abonos_Aplica",
                    new
                    {
                        Remesa = request.numero_generacion,
                        Usuario = request.usuario.Trim(),
                        TipoDoc = request.tipo_documento.Trim(),
                        Documento = request.numero_documento.Trim(),
                        Proceso = request.lote
                    },
                    commandType: CommandType.StoredProcedure);

                response.pendientes = row == null ? 0 : Convert.ToInt32(row.Procesado ?? 0);

                return DbHelper.CreateOkResponse(response);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CcCaRemesasRecibeAplicaResponse>(ex.Message, -1, response);
            }
        }

        #endregion


        #region Asiento y Recibo

        /// <summary>
        /// Crea el comprobante y asiento contable de la aplicación de una remesa.
        /// Equivalente a spPrm_CA_Aplica_Asiento del VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos: tipo_documento, numero_documento, usuario, numero_generacion.</param>
        /// <returns>true si exitoso.</returns>
        public ErrorDto<bool> CcCaRemesas_Recibe_Asiento(
            int codEmpresa,
            CcCaRemesasRecibeAsientoRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                conn.Execute(
                    "spPrm_CA_Aplica_Asiento",
                    new
                    {
                        TipoDoc = request.tipo_documento.Trim(),
                        Documento = request.numero_documento.Trim(),
                        Usuario = request.usuario.Trim(),
                        Remesa = request.numero_generacion
                    },
                    commandType: CommandType.StoredProcedure);

                return DbHelper.CreateOkResponse(true);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<bool>(ex.Message);
            }
        }

        /// <summary>
        /// Genera y retorna el recibo en PDF de un documento de la remesa.
        /// Delega en MRecibos que resuelve el RDLC según SysDocVersion.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos: numero_documento, tipo_documento, usuario, reimprimir.</param>
        /// <returns>Objeto con FileContents (base64 PDF) y fileDownloadName.</returns>
        public ErrorDto<object> CcCaRemesas_Recibe_ImprimeRecibo(
            int codEmpresa,
            CcCaRemesasRecibeImprimeReciboRequest request)
        {
            return _mRecibos.sbImprimeRecibo(
                codEmpresa,
                request.numero_documento,
                request.tipo_documento,
                request.usuario,
                request.reimprimir);
        }

        #endregion

    }
}
