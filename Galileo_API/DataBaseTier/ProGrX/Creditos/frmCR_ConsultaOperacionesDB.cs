using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrConsultaOperacionesDb
    {
        private const string MsgCedulaRequerida = "Debe indicar la c&eacute;dula.";
        private const string MsgOperacionRequerida = "Debe indicar la operaci&oacute;n.";
        private const string MsgOperacionNoExiste = "No se encontr&oacute; la operaci&oacute;n.";
        private const string MsgConsultaError = "No fue posible consultar las operaciones.";
        private const string MsgDetalleError = "No fue posible obtener el detalle de la operaci&oacute;n.";
        private const string MsgBusquedaOperacionError = "No fue posible obtener la lista de operaciones.";
        private const string MsgBusquedaSociosError = "No fue posible obtener la lista de socios.";

        private readonly PortalDB _portalDb;
        private readonly MCntLinkDB _mCntLinkDb;

        public FrmCrConsultaOperacionesDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mCntLinkDb = new MCntLinkDB(config);
        }

        /// <summary>
        /// Obtiene la lista base para búsqueda por operación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaOperacionesBusquedaOperacionDto>> CrConsultaOperaciones_BusquedaOperaciones_Obtener(
            int codEmpresa)
        {
            var response = DbHelper.ExecuteListQuery<CrConsultaOperacionesBusquedaOperacionDto>(
                _portalDb,
                codEmpresa,
                @"
                select
                    id_solicitud,
                    codigo,
                    cedula
                from REG_CREDITOS
                order by id_solicitud desc");

            return response.Code == 0
                ? DbHelper.CreateOkResponse(response.Result ?? new List<CrConsultaOperacionesBusquedaOperacionDto>())
                : DbHelper.CreateErrorResponse(
                    response.Description ?? MsgBusquedaOperacionError,
                    response.Code ?? -1,
                    new List<CrConsultaOperacionesBusquedaOperacionDto>());
        }

        /// <summary>
        /// Obtiene la lista base para búsqueda por cédula.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaOperacionesBusquedaSocioDto>> CrConsultaOperaciones_BusquedaSocios_Obtener(
            int codEmpresa)
        {
            var response = DbHelper.ExecuteListQuery<CrConsultaOperacionesBusquedaSocioDto>(
                _portalDb,
                codEmpresa,
                @"
                select
                    cedula,
                    nombre
                from socios
                order by cedula");

            return response.Code == 0
                ? DbHelper.CreateOkResponse(response.Result ?? new List<CrConsultaOperacionesBusquedaSocioDto>())
                : DbHelper.CreateErrorResponse(
                    response.Description ?? MsgBusquedaSociosError,
                    response.Code ?? -1,
                    new List<CrConsultaOperacionesBusquedaSocioDto>());
        }

        /// <summary>
        /// Obtiene las operaciones en trámite de una persona.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaOperacionesListaDto>> CrConsultaOperaciones_Cedula_Obtener(
            int codEmpresa,
            string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return DbHelper.CreateErrorResponse(
                    MsgCedulaRequerida,
                    -2,
                    new List<CrConsultaOperacionesListaDto>());
            }

            var response = DbHelper.ExecuteListQuery<CrConsultaOperacionesListaDto>(
                _portalDb,
                codEmpresa,
                @"
                select
                    id_solicitud,
                    codigo,
                    cedula,
                    fechasol,
                    montosol,
                    estadosol,
                    estado,
                    proceso
                from REG_CREDITOS
                where cedula = @Cedula
                order by id_solicitud desc",
                new { Cedula = cedula.Trim() });

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    response.Description ?? MsgConsultaError,
                    response.Code ?? -1,
                    new List<CrConsultaOperacionesListaDto>());
            }

            List<CrConsultaOperacionesListaDto> lista = response.Result ?? new List<CrConsultaOperacionesListaDto>();
            foreach (CrConsultaOperacionesListaDto item in lista)
            {
                item.estadosol_desc = ObtenerEstadoSolicitudDescripcion(item.estadosol);
                item.estado_desc = ObtenerEstadoEcDescripcion(item.estado);
                item.proceso_desc = ObtenerProcesoDescripcion(item.proceso);
            }

            return DbHelper.CreateOkResponse(lista);
        }

        /// <summary>
        /// Obtiene el detalle completo de una operación en formato texto.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaOperacionesDetalleDto> CrConsultaOperaciones_Detalle_Obtener(
            int codEmpresa,
            int operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MsgOperacionRequerida,
                    -2,
                    new CrConsultaOperacionesDetalleDto());
            }

            try
            {
                var mainResponse = DbHelper.ExecuteSingleQuery<CrConsultaOperacionesMainData>(
                    _portalDb,
                    codEmpresa,
                    @"
                    Select
                        R.*,
                        S.nombre,
                        C.Descripcion as DescCod
                    from reg_creditos R
                    inner join Socios S on R.cedula = S.cedula
                    inner join Catalogo C on R.codigo = C.codigo
                    where R.ID_SOLICITUD = @Operacion",
                    new CrConsultaOperacionesMainData(),
                    new { Operacion = operacion });

                if (mainResponse.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        mainResponse.Description ?? MsgDetalleError,
                        mainResponse.Code ?? -1,
                        new CrConsultaOperacionesDetalleDto());
                }

                CrConsultaOperacionesMainData main = mainResponse.Result ?? new CrConsultaOperacionesMainData();
                if (main.id_solicitud <= 0)
                {
                    return DbHelper.CreateErrorResponse(
                        MsgOperacionNoExiste,
                        -2,
                        new CrConsultaOperacionesDetalleDto());
                }

                StringBuilder sb = new();
                string salto = Environment.NewLine;
                string dobleSalto = salto + salto + salto;

                sb.Append(dobleSalto);
                sb.AppendLine($"OPERACION : {main.id_solicitud}");
                sb.AppendLine($"CEDULA    : {main.cedula}\t{main.nombre}");
                sb.AppendLine($"CODIGO    : {main.codigo}\t\t{main.desccod}");
                sb.AppendLine($"GARANTIA  : {ObtenerGarantiaDescripcion(main.garantia)}\tESTADO : {ObtenerEstadoSolicitudDescripcion(main.estadosol).ToUpperInvariant()}");
                sb.Append(dobleSalto);

                AppendRecepcion(sb, main);
                sb.Append(dobleSalto);

                AppendResolucion(sb, main);
                sb.Append(dobleSalto);

                AppendFormalizacion(sb, main);
                sb.Append(dobleSalto);

                AppendFiadores(codEmpresa, sb, main.id_solicitud);
                sb.Append(dobleSalto);

                AppendRefundicionesCartera(codEmpresa, sb, main.id_solicitud);
                sb.Append(dobleSalto);

                AppendRefundicionesRetencion(codEmpresa, sb, main.id_solicitud);
                sb.Append(dobleSalto);

                AppendDesembolsos(codEmpresa, sb, main.id_solicitud);
                sb.Append(dobleSalto);

                AppendDatosAdicionales(codEmpresa, sb, main);
                sb.Append(dobleSalto);

                AppendDocumentoEmitido(codEmpresa, sb, main);

                return DbHelper.CreateOkResponse(new CrConsultaOperacionesDetalleDto
                {
                    operacion = main.id_solicitud,
                    cedula = main.cedula,
                    nombre = main.nombre,
                    detalle = sb.ToString()
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    $"Ocurri&oacute; un error al obtener el detalle: {ex.Message}",
                    -1,
                    new CrConsultaOperacionesDetalleDto());
            }
        }

        private static void AppendRecepcion(StringBuilder sb, CrConsultaOperacionesMainData main)
        {
            sb.AppendLine("*****************  RECEPCION *****************");
            sb.AppendLine();
            sb.AppendLine($"USUARIO : {main.userrec}\tFECHA : {FormatoFecha(main.fechasol)}");
            sb.AppendLine(
                $"MONTO   : {FormatoMonto(main.montosol)}\tPLAZO   : {main.plazo}\tINT : {main.@int}");
            sb.AppendLine($"CUOTA   : {main.cuota}");
            sb.AppendLine();
        }

        private static void AppendResolucion(StringBuilder sb, CrConsultaOperacionesMainData main)
        {
            sb.AppendLine("***************** RESOLUCION *****************");
            sb.AppendLine();
            sb.AppendLine($"USUARIO : {main.userres}\tFECHA : {FormatoFecha(main.fechares)}");
            sb.AppendLine(
                $"MONTO   : {FormatoMonto(main.montoapr)}\tPLAZO   : {main.plazo}\tINT : {main.@int}");
            sb.AppendLine($"CUOTA   : {main.cuota}");
            sb.AppendLine();
        }

        private static void AppendFormalizacion(StringBuilder sb, CrConsultaOperacionesMainData main)
        {
            sb.AppendLine("***************** FORMALIZACION *****************");
            sb.AppendLine();
            sb.AppendLine($"USUARIO : {main.userfor}\tFECHA : {FormatoFecha(main.fechaforp)}");
            sb.AppendLine(
                $"FEC.CALCULO : {FormatoFecha(main.fecha_calculo_int)}\tMONTO GIRADO : {FormatoMonto(main.monto_girado)}");
            sb.AppendLine($"DOCUMENTO   : {main.tdocumento}-{main.ndocumento}");
            sb.AppendLine($"DESEMBOLSOS : {ObtenerTexto(main.documento_referido, "N/A")}");
            sb.AppendLine();
        }

        private void AppendFiadores(int codEmpresa, StringBuilder sb, int operacion)
        {
            var response = DbHelper.ExecuteListQuery<CrConsultaOperacionesFiadorData>(
                _portalDb,
                codEmpresa,
                @"
                select
                    F.cedulaf,
                    S.nombre as nomb
                from fiadores F
                inner join Socios S on F.cedulaf = S.cedula
                where F.id_solicitud = @Operacion",
                new { Operacion = operacion });

            sb.AppendLine("***************** REGISTRO DE FIADORES *****************");
            sb.AppendLine();

            List<CrConsultaOperacionesFiadorData> lista = response.Result ?? new List<CrConsultaOperacionesFiadorData>();
            if (response.Code != 0 || lista.Count == 0)
            {
                sb.AppendLine(" ** NO EXISTEN FIADORES REGISTRADOS PARA ESTA SOLICITUD ** ");
                return;
            }

            foreach (CrConsultaOperacionesFiadorData item in lista)
            {
                sb.AppendLine($"CEDULA : {item.cedulaf}\tNOMBRE : {item.nomb}");
            }
        }

        private void AppendRefundicionesCartera(int codEmpresa, StringBuilder sb, int operacion)
        {
            var response = DbHelper.ExecuteListQuery<CrConsultaOperacionesRefundicionCarteraData>(
                _portalDb,
                codEmpresa,
                @"
                select
                    id_solicitud,
                    codigo,
                    monto,
                    intcor,
                    intmor
                from refundiciones
                where id_solicitudr = @Operacion",
                new { Operacion = operacion });

            sb.AppendLine("***************** REFUNDICIONES A CARTERA *****************");
            sb.AppendLine();

            List<CrConsultaOperacionesRefundicionCarteraData> lista = response.Result ?? new List<CrConsultaOperacionesRefundicionCarteraData>();
            if (response.Code != 0 || lista.Count == 0)
            {
                sb.AppendLine(" ** NO EXISTEN REFUNDICIONES A CARTERA REGISTRADAS PARA ESTA SOLICITUD ** ");
                return;
            }

            foreach (CrConsultaOperacionesRefundicionCarteraData item in lista)
            {
                sb.AppendLine(
                    $"OPERACION : {item.id_solicitud}\tCODIGO : {item.codigo}\tMONTO : {FormatoMonto(item.monto)} INTC.Atr. {FormatoMonto(item.intcor)}\t INT.MORO : {FormatoMonto(item.intmor)}");
            }
        }

        private void AppendRefundicionesRetencion(int codEmpresa, StringBuilder sb, int operacion)
        {
            var response = DbHelper.ExecuteListQuery<CrConsultaOperacionesRefundicionRetencionData>(
                _portalDb,
                codEmpresa,
                @"
                select
                    id_solicitud,
                    codigo,
                    monto,
                    mora
                from refunde_retencion
                where id_solicitudr = @Operacion",
                new { Operacion = operacion });

            sb.AppendLine("***************** REFUNDICIONES DE RETENCIONES *****************");
            sb.AppendLine();

            List<CrConsultaOperacionesRefundicionRetencionData> lista = response.Result ?? new List<CrConsultaOperacionesRefundicionRetencionData>();
            if (response.Code != 0 || lista.Count == 0)
            {
                sb.AppendLine(" ** NO EXISTEN REFUNDICIONES DE RETENCIONES REGISTRADAS PARA ESTA SOLICITUD ** ");
                return;
            }

            foreach (CrConsultaOperacionesRefundicionRetencionData item in lista)
            {
                sb.AppendLine(
                    $"OPERACION : {item.id_solicitud}\tCODIGO : {item.codigo}\tMONTO : {FormatoMonto(item.monto)} MOROSIDAD : {FormatoMonto(item.mora)}");
            }
        }

        private void AppendDesembolsos(int codEmpresa, StringBuilder sb, int operacion)
        {
            var response = DbHelper.ExecuteListQuery<CrConsultaOperacionesDesembolsoData>(
                _portalDb,
                codEmpresa,
                @"
                select
                    monto,
                    cuenta_conta,
                    concepto
                from desembolsos
                where id_solicitud = @Operacion",
                new { Operacion = operacion });

            sb.AppendLine("***************** DESEMBOLSOS *****************");
            sb.AppendLine();

            List<CrConsultaOperacionesDesembolsoData> lista = response.Result ?? new List<CrConsultaOperacionesDesembolsoData>();
            if (response.Code != 0 || lista.Count == 0)
            {
                sb.AppendLine(" ** NO EXISTEN DESEMBOLSOS REGISTRADOS PARA ESTA SOLICITUD ** ");
                return;
            }

            foreach (CrConsultaOperacionesDesembolsoData item in lista)
            {
                string cuenta = item.cuenta_conta?.Trim() ?? string.Empty;
                string cuentaFormato = string.IsNullOrWhiteSpace(cuenta)
                    ? string.Empty
                    : _mCntLinkDb.fxgCntCuentaFormato(codEmpresa, true, cuenta, 0);

                sb.AppendLine(
                    $"MONTO : {FormatoMonto(item.monto)}\tCUENTA : {cuentaFormato}\tBENEFICIARIO : {item.concepto}");
            }
        }

        private void AppendDatosAdicionales(int codEmpresa, StringBuilder sb, CrConsultaOperacionesMainData main)
        {
            string descripcionComite = ObtenerDescripcionComite(codEmpresa, main.id_comite);

            sb.AppendLine("***************** DATOS ADICIONALES *****************");
            sb.AppendLine();
            sb.AppendLine($"COMITE : {descripcionComite.ToUpperInvariant()}\tACTA : {main.acta}");
            sb.AppendLine("OBSERVACIONES : ");

            foreach (string linea in PartirTextoMayuscula(main.observacion, 70, 6))
            {
                sb.AppendLine(linea);
            }

            if (string.Equals(main.estado, "A", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("ESTE CREDITO SE ENCUENTRA ACTIVO");
            }
            else if (string.Equals(main.estado, "C", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("ESTE CREDITO SE ENCUENTRA CANCELADO");
            }
        }

        private void AppendDocumentoEmitido(int codEmpresa, StringBuilder sb, CrConsultaOperacionesMainData main)
        {
            string tipoDoc = (main.tdocumento ?? string.Empty).Trim().ToUpperInvariant();
            string numDoc = (main.ndocumento ?? string.Empty).Trim();
            string segmento = $"FRM-{main.fechaforp:yyyyMMdd}";

            sb.AppendLine("***************** DOCUMENTO EMITIDO *****************");
            sb.AppendLine();

            if (string.IsNullOrWhiteSpace(tipoDoc) || string.IsNullOrWhiteSpace(numDoc))
            {
                sb.AppendLine(" **** NO SE ENCONTRO DOCUMENTO ***");
                return;
            }

            if (tipoDoc == "TE" || tipoDoc == "CK")
            {
                AppendDocumentoTesoreria(codEmpresa, sb, tipoDoc, numDoc);
                return;
            }

            if (tipoDoc == "ND")
            {
                AppendDocumentoAse(sb, tipoDoc, numDoc);
                return;
            }

            if (tipoDoc == "FR")
            {
                AppendDocumentoTmp(codEmpresa, sb, tipoDoc, numDoc, segmento);
            }
        }

        private void AppendDocumentoTesoreria(int codEmpresa, StringBuilder sb, string tipoDoc, string numDoc)
        {
            var transResponse = DbHelper.ExecuteSingleQuery<CrConsultaOperacionesTesTransaccionData>(
                _portalDb,
                codEmpresa,
                @"
                select top 1
                    NSolicitud as nsolicitud
                from Tes_Transacciones
                where tipo = @Tipo
                  and Ndocumento = @NumDoc",
                new CrConsultaOperacionesTesTransaccionData(),
                new { Tipo = tipoDoc, NumDoc = numDoc });

            CrConsultaOperacionesTesTransaccionData trans = transResponse.Result ?? new CrConsultaOperacionesTesTransaccionData();

            if (trans.nsolicitud <= 0 && numDoc.Length >= 3)
            {
                string alterno = $"{numDoc.Substring(1, Math.Min(11, numDoc.Length - 1))}{numDoc[^2..]}";
                transResponse = DbHelper.ExecuteSingleQuery<CrConsultaOperacionesTesTransaccionData>(
                    _portalDb,
                    codEmpresa,
                    @"
                    select top 1
                        NSolicitud as nsolicitud
                    from Tes_Transacciones
                    where tipo = @Tipo
                      and Ndocumento = @NumDoc",
                    new CrConsultaOperacionesTesTransaccionData(),
                    new { Tipo = tipoDoc, NumDoc = alterno });

                trans = transResponse.Result ?? new CrConsultaOperacionesTesTransaccionData();
            }

            if (trans.nsolicitud <= 0)
            {
                sb.AppendLine(" **** NO SE ENCONTRO DOCUMENTO ***");
                return;
            }

            var asientoResponse = DbHelper.ExecuteListQuery<CrConsultaOperacionesTesAsientoData>(
                _portalDb,
                codEmpresa,
                @"
                select
                    CUENTA_CONTABLE as cuenta_contable,
                    Monto as monto,
                    DebeHaber as debehaber
                from Tes_Trans_Asiento
                where NSolicitud = @NSolicitud",
                new { NSolicitud = trans.nsolicitud });

            sb.AppendLine($" DOCUMENTO : {tipoDoc}-{numDoc}");
            sb.AppendLine();
            sb.AppendLine("CUENTA\t\t\t\t    DEBITOS\t   CREDITOS\tDESCRIPCION");
            sb.AppendLine();

            foreach (CrConsultaOperacionesTesAsientoData item in asientoResponse.Result ?? new List<CrConsultaOperacionesTesAsientoData>())
            {
                string cuentaFormato = _mCntLinkDb.fxgCntCuentaFormato(codEmpresa, true, item.cuenta_contable, 0);
                string monto = FormatoMonto(item.monto).PadLeft(18, ' ');
                string descripcion = _mCntLinkDb.fxgCntCuentaDesc(codEmpresa, item.cuenta_contable, 0);

                if (string.Equals(item.debehaber, "D", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine($"{cuentaFormato}\t\t{monto}\t\t\t{descripcion}");
                }
                else
                {
                    sb.AppendLine($"{cuentaFormato}\t\t\t\t{monto}\t{descripcion}");
                }
            }
        }

        private void AppendDocumentoAse(StringBuilder sb, string tipoDoc, string numDoc)
        {
            if (!int.TryParse(numDoc, out int idDocumento))
            {
                sb.AppendLine(" **** NO SE ENCONTRO DOCUMENTO ***");
                return;
            }

            var asientoResponse = DbHelper.ExecuteListQuery<CrConsultaOperacionesAseAsientoData>(
                _portalDb,
                0,
                @"
                select
                    A.recas_cuenta,
                    A.recas_monto,
                    A.recas_debehaber,
                    B.Descripcion
                from ase_asientos A
                inner join cuentas B on A.recas_cuenta = B.cod_cuenta
                where A.tipo = @Tipo
                  and A.id_documento = @IdDocumento",
                new { Tipo = tipoDoc, IdDocumento = idDocumento });

            sb.AppendLine($" DOCUMENTO : {tipoDoc}-{numDoc}");
            sb.AppendLine();
            sb.AppendLine("CUENTA\t\t\t\t    DEBITOS\t   CREDITOS\tDESCRIPCION");
            sb.AppendLine();

            foreach (CrConsultaOperacionesAseAsientoData item in asientoResponse.Result ?? new List<CrConsultaOperacionesAseAsientoData>())
            {
                string monto = FormatoMonto(item.recas_monto).PadLeft(18, ' ');

                if (string.Equals(item.recas_debehaber, "D", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine($"{item.recas_cuenta}\t\t{monto}\t\t\t{item.descripcion}");
                }
                else
                {
                    sb.AppendLine($"{item.recas_cuenta}\t\t\t\t{monto}\t{item.descripcion}");
                }
            }
        }

        private void AppendDocumentoTmp(int codEmpresa, StringBuilder sb, string tipoDoc, string numDoc, string segmento)
        {
            if (!int.TryParse(numDoc, out int operacion))
            {
                sb.AppendLine(" **** NO SE ENCONTRO DOCUMENTO ***");
                return;
            }

            var asientoResponse = DbHelper.ExecuteListQuery<CrConsultaOperacionesTmpAsientoData>(
                _portalDb,
                codEmpresa,
                @"
                select
                    A.tmp_cuenta,
                    A.tmp_monto,
                    A.tmp_debehaber,
                    B.Descripcion
                from asientos_TMP A
                inner join cuentas B on A.tmp_cuenta = B.cod_cuenta
                where A.tmp_tipo = 'FRM'
                  and A.tmp_operacion = @Operacion",
                new { Operacion = operacion });

            sb.AppendLine($" DOCUMENTO : {tipoDoc}-{numDoc}");
            sb.AppendLine($" SEGMENTO  : {segmento}");
            sb.AppendLine();
            sb.AppendLine("CUENTA\t\t\t\t    DEBITOS\t   CREDITOS\tDESCRIPCION");
            sb.AppendLine();

            foreach (CrConsultaOperacionesTmpAsientoData item in asientoResponse.Result ?? new List<CrConsultaOperacionesTmpAsientoData>())
            {
                string cuentaFormato = _mCntLinkDb.fxgCntCuentaFormato(codEmpresa, true, item.tmp_cuenta, 0);
                string monto = FormatoMonto(item.tmp_monto).PadLeft(18, ' ');

                if (string.Equals(item.tmp_debehaber, "D", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine($"{cuentaFormato}\t\t{monto}\t\t\t{item.descripcion}");
                }
                else
                {
                    sb.AppendLine($"{cuentaFormato}\t\t\t\t{monto}\t{item.descripcion}");
                }
            }
        }

        private string ObtenerDescripcionComite(int codEmpresa, int idComite)
        {
            if (idComite <= 0)
            {
                return string.Empty;
            }

            var response = DbHelper.ExecuteSingleQuery<CrConsultaOperacionesDescripcionData>(
                _portalDb,
                codEmpresa,
                "select descripcion as describe from comites where id_comite = @IdComite",
                new CrConsultaOperacionesDescripcionData(),
                new { IdComite = idComite });

            return response.Result?.describe ?? string.Empty;
        }

        private static IEnumerable<string> PartirTextoMayuscula(string? texto, int largo, int maxLineas)
        {
            string valor = (texto ?? string.Empty).Trim();
            for (int i = 0; i < maxLineas; i++)
            {
                int inicio = i * largo;
                if (inicio >= valor.Length)
                {
                    yield return string.Empty;
                    continue;
                }

                int cantidad = Math.Min(largo, valor.Length - inicio);
                yield return valor.Substring(inicio, cantidad).ToUpperInvariant();
            }
        }

        private static string ObtenerGarantiaDescripcion(string? garantia)
        {
            return (garantia ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static string ObtenerEstadoSolicitudDescripcion(string? estadoSol)
        {
            return (estadoSol ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "R" => "Recibida",
                "P" => "Pendiente",
                "A" => "Aprobada",
                "D" => "Denegada",
                "F" => "Formalizada",
                "N" => "Anulada",
                _ => string.Empty
            };
        }

        private static string ObtenerEstadoEcDescripcion(string? estado)
        {
            return (estado ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "A" => "Activa",
                "C" => "Cancelada",
                _ => "En Tramite"
            };
        }

        private static string ObtenerProcesoDescripcion(string? proceso)
        {
            return (proceso ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "J" => "Cobro Jud",
                "N" => "Normal",
                "T" => "Traspaso",
                _ => "------"
            };
        }

        private static string FormatoMonto(decimal valor)
        {
            return valor.ToString("###,###,###,##0.00");
        }

        private static string FormatoFecha(DateTime? fecha)
        {
            return fecha.HasValue ? fecha.Value.ToString("dd/MM/yyyy") : string.Empty;
        }

        private static string ObtenerTexto(string? valor, string porDefecto)
        {
            return string.IsNullOrWhiteSpace(valor) ? porDefecto : valor.Trim();
        }
    }
}