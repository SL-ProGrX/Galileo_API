using System.Text;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrRetencionDeduccionesDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrRetencionDeduccionesDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la información inicial de la pantalla.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<CrRetencionDeduccionesPantallaData> Cr_RetencionDeducciones_Pantalla_Obtener(int codEmpresa)
        {
            var fechaServidor = ObtenerFechaServidor(codEmpresa);
            if (fechaServidor.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    fechaServidor.Description ?? "No fue posible obtener la fecha del servidor.",
                    fechaServidor.Code.GetValueOrDefault(-1),
                    new CrRetencionDeduccionesPantallaData());
            }

            var clientes = ObtenerClientes(codEmpresa);
            if (clientes.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    clientes.Description ?? "No fue posible obtener los clientes.",
                    clientes.Code.GetValueOrDefault(-1),
                    new CrRetencionDeduccionesPantallaData());
            }

            var instituciones = ObtenerInstituciones(codEmpresa);
            if (instituciones.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    instituciones.Description ?? "No fue posible obtener las instituciones.",
                    instituciones.Code.GetValueOrDefault(-1),
                    new CrRetencionDeduccionesPantallaData());
            }

            var salida = new CrRetencionDeduccionesPantallaData
            {
                clientes = clientes.Result ?? new List<DropDownListaGenericaModel>(),
                instituciones = instituciones.Result ?? new List<DropDownListaGenericaModel>(),
                fecha_servidor = fechaServidor.Result,
                proceso_default = $"{fechaServidor.Result.Year}{fechaServidor.Result.Month:00}",
                formato_default = "01",
                tipo_default = "P",
                formatos = new List<DropDownListaGenericaModel>
                {
                    new() { item = "01", descripcion = "Formato de Salida : Sistema ProGrX" },
                    new() { item = "02", descripcion = "Formato de Salida : INTEGRA" },
                    new() { item = "03", descripcion = "Formato de Salida : CCSS" },
                    new() { item = "04", descripcion = "Formato de Salida : SPA" }
                },
                tipos = new List<DropDownListaGenericaModel>
                {
                    new() { item = "F", descripcion = "Fechas" },
                    new() { item = "P", descripcion = "Proceso" }
                }
            };

            return DbHelper.CreateOkResponse(salida);
        }

        /// <summary>
        /// Obtiene la lista de deducciones aplicadas según filtros.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrRetencionDeduccionesResultadoData> Cr_RetencionDeducciones_Obtener(
            int codEmpresa,
            CrRetencionDeduccionesObtenerRequest request)
        {
            var validacion = ValidarFiltros(request);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    validacion.Description ?? "Filtros inválidos.",
                    validacion.Code.GetValueOrDefault(-1),
                    new CrRetencionDeduccionesResultadoData());
            }

            var usaPlanPagos = ObtenerUsaPlanPagos(codEmpresa);
            if (usaPlanPagos.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    usaPlanPagos.Description ?? "No fue posible determinar la configuración de plan de pagos.",
                    usaPlanPagos.Code.GetValueOrDefault(-1),
                    new CrRetencionDeduccionesResultadoData());
            }

            var listaPrincipal = usaPlanPagos.Result
                ? ObtenerDeduccionesConPlanPagos(codEmpresa, request)
                : ObtenerDeduccionesSinPlanPagos(codEmpresa, request);

            if (listaPrincipal.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    listaPrincipal.Description ?? "No fue posible obtener las deducciones.",
                    listaPrincipal.Code.GetValueOrDefault(-1),
                    new CrRetencionDeduccionesResultadoData());
            }

            List<CrRetencionDeduccionesData> salida = listaPrincipal.Result ?? new List<CrRetencionDeduccionesData>();

            if (!usaPlanPagos.Result)
            {
                var morosidad = ObtenerDeduccionesMorosidad(codEmpresa, request);
                if (morosidad.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        morosidad.Description ?? "No fue posible obtener la morosidad.",
                        morosidad.Code.GetValueOrDefault(-1),
                        new CrRetencionDeduccionesResultadoData());
                }

                salida = ConsolidarDeducciones(
                    salida,
                    morosidad.Result ?? new List<CrRetencionDeduccionesData>());
            }

            var resultado = new CrRetencionDeduccionesResultadoData
            {
                deducciones = salida
                    .OrderBy(x => x.cedula)
                    .ToList(),
                total_casos = salida.Count,
                total_monto = salida.Sum(x => x.monto)
            };

            return DbHelper.CreateOkResponse(resultado);
        }

        /// <summary>
        /// Genera el archivo de salida del formato solicitado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrRetencionDeduccionesArchivoData> Cr_RetencionDeducciones_Archivo_Generar(
            int codEmpresa,
            CrRetencionDeduccionesArchivoRequest request)
        {
            var consulta = Cr_RetencionDeducciones_Obtener(codEmpresa, new CrRetencionDeduccionesObtenerRequest
            {
                codigo = request.codigo,
                cod_institucion = request.cod_institucion,
                formato = request.formato,
                tipo = request.tipo,
                fecha_inicio = request.fecha_inicio,
                fecha_corte = request.fecha_corte,
                proceso = request.proceso
            });

            if (consulta.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    consulta.Description ?? "No fue posible obtener las deducciones para el archivo.",
                    consulta.Code.GetValueOrDefault(-1),
                    new CrRetencionDeduccionesArchivoData());
            }

            var lista = consulta.Result?.deducciones ?? new List<CrRetencionDeduccionesData>();
            if (!lista.Any())
            {
                return DbHelper.CreateErrorResponse(
                    "No existen datos para generar el archivo.",
                    -2,
                    new CrRetencionDeduccionesArchivoData());
            }

            var institucion = ObtenerInstitucionDescripcion(codEmpresa, request.cod_institucion ?? 0);
            if (institucion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    institucion.Description ?? "No fue posible obtener la institución.",
                    institucion.Code.GetValueOrDefault(-1),
                    new CrRetencionDeduccionesArchivoData());
            }

            var fechaServidor = ObtenerFechaServidor(codEmpresa);
            if (fechaServidor.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    fechaServidor.Description ?? "No fue posible obtener la fecha del servidor.",
                    fechaServidor.Code.GetValueOrDefault(-1),
                    new CrRetencionDeduccionesArchivoData());
            }

            string formato = NormalizarFormato(request.formato);
            string descripcionInstitucion = institucion.Result?.descripcion ?? "TODOS";

            string nombreArchivo = ConstruirNombreArchivo(
                formato,
                request,
                fechaServidor.Result,
                descripcionInstitucion,
                request.cod_institucion ?? 0);

            string contenido = formato switch
            {
                "02" => GenerarContenidoIntegra(lista, request),
                "03" => GenerarContenidoCCSS(lista),
                "04" => GenerarContenidoSPA(lista, request),
                _ => GenerarContenidoProGrX(lista)
            };

            return DbHelper.CreateOkResponse(new CrRetencionDeduccionesArchivoData
            {
                nombre_archivo = nombreArchivo,
                contenido = contenido,
                content_type = "text/plain"
            });
        }

        /// <summary>
        /// Obtiene los clientes habilitados para retención.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        private ErrorDto<List<DropDownListaGenericaModel>> ObtenerClientes(int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(codigo) as item,
                    rtrim(descripcion) as descripcion
                from catalogo
                where retencion = 'S'
                  and activo = 1
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene las instituciones activas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        private ErrorDto<List<DropDownListaGenericaModel>> ObtenerInstituciones(int codEmpresa)
        {
            const string sql = @"
                select cast(0 as int) as item, 'TODOS' as descripcion
                union all
                select cast(cod_institucion as int) as item, rtrim(descripcion) as descripcion
                from instituciones
                where activa = 1
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene la fecha actual del servidor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        private ErrorDto<DateTime> ObtenerFechaServidor(int codEmpresa)
        {
            const string sql = @"select dbo.MyGetdate();";

            return DbHelper.ExecuteSingleQuery<DateTime>(
                _portalDb,
                codEmpresa,
                sql,
                DateTime.Now);
        }

        /// <summary>
        /// Indica si la empresa utiliza plan de pagos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        private ErrorDto<bool> ObtenerUsaPlanPagos(int codEmpresa)
        {
            const string sql = @"select cast(isnull(SysPlanPagos, 0) as bit) from globales;";

            return DbHelper.ExecuteSingleQuery<bool>(
                _portalDb,
                codEmpresa,
                sql,
                false);
        }

        /// <summary>
        /// Obtiene la descripción de una institución.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codInstitucion"></param>
        /// <returns></returns>
        private ErrorDto<DropDownListaGenericaModel> ObtenerInstitucionDescripcion(int codEmpresa, int codInstitucion)
        {
            if (codInstitucion == 0)
            {
                return DbHelper.CreateOkResponse(new DropDownListaGenericaModel
                {
                    item = 0,
                    descripcion = "TODOS"
                });
            }

            const string sql = @"
                select
                    cast(cod_institucion as int) as item,
                    rtrim(descripcion) as descripcion
                from instituciones
                where cod_institucion = @CodInstitucion;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql,
                new DropDownListaGenericaModel(),
                new { CodInstitucion = codInstitucion });
        }

        /// <summary>
        /// Obtiene deducciones cuando la empresa usa plan de pagos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto<List<CrRetencionDeduccionesData>> ObtenerDeduccionesConPlanPagos(
            int codEmpresa,
            CrRetencionDeduccionesObtenerRequest request)
        {
            string filtroFecha = request.tipo == "F"
                ? " and D.MOV_FECHA between @FechaInicio and @FechaCorte "
                : " and D.NUM_COMPROBANTE like @Proceso ";

            string filtroInstitucion = request.cod_institucion.GetValueOrDefault(0) == 0
                ? string.Empty
                : " and S.cod_institucion = @CodInstitucion ";

            string sql = $@"
                select
                    rtrim(S.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    isnull(sum(D.MOV_MONTO), 0) as monto
                from CRD_OPERACION_TRANSAC D
                inner join reg_creditos R on D.id_solicitud = R.id_solicitud
                inner join socios S on R.cedula = S.cedula
                where D.TIPO_DOCUMENTO = 'PLA'
                  and D.codigo = @Codigo
                  {filtroFecha}
                  {filtroInstitucion}
                group by S.cedula, S.nombre;";

            return DbHelper.ExecuteListQuery<CrRetencionDeduccionesData>(
                _portalDb,
                codEmpresa,
                sql,
                CrearParametrosConsulta(request, true));
        }

        /// <summary>
        /// Obtiene deducciones cuando la empresa no usa plan de pagos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto<List<CrRetencionDeduccionesData>> ObtenerDeduccionesSinPlanPagos(
            int codEmpresa,
            CrRetencionDeduccionesObtenerRequest request)
        {
            string filtroFecha = request.tipo == "F"
                ? " and D.fechas between @FechaInicioSoloFecha and @FechaCorteSoloFecha "
                : " and D.NCon like @Proceso ";

            string filtroInstitucion = request.cod_institucion.GetValueOrDefault(0) == 0
                ? string.Empty
                : " and S.cod_institucion = @CodInstitucion ";

            string sql = $@"
                select
                    rtrim(S.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    isnull(sum(D.abono), 0) as monto
                from creditos_Dt D
                inner join reg_creditos R on D.id_solicitud = R.id_solicitud
                inner join socios S on R.cedula = S.cedula
                where D.tcon in ('PLA', '1')
                  and D.codigo = @Codigo
                  {filtroFecha}
                  {filtroInstitucion}
                group by S.cedula, S.nombre;";

            return DbHelper.ExecuteListQuery<CrRetencionDeduccionesData>(
                _portalDb,
                codEmpresa,
                sql,
                CrearParametrosConsulta(request, false));
        }

        /// <summary>
        /// Obtiene la morosidad para consolidar cuando no se usa plan de pagos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto<List<CrRetencionDeduccionesData>> ObtenerDeduccionesMorosidad(
            int codEmpresa,
            CrRetencionDeduccionesObtenerRequest request)
        {
            string filtroFecha = request.tipo == "F"
                ? " and D.Fecult between @FechaInicioSoloFecha and @FechaCorteSoloFecha "
                : " and D.NCon like @Proceso ";

            string filtroInstitucion = request.cod_institucion.GetValueOrDefault(0) == 0
                ? string.Empty
                : " and S.cod_institucion = @CodInstitucion ";

            string sql = $@"
                select
                    rtrim(S.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    isnull(sum(D.abAmortiza), 0) as monto
                from Morosidad D
                inner join reg_creditos R on D.id_solicitud = R.id_solicitud
                inner join socios S on R.cedula = S.cedula
                where D.tcon = '1'
                  and D.estado = 'C'
                  and D.codigo = @Codigo
                  {filtroFecha}
                  {filtroInstitucion}
                group by S.cedula, S.nombre;";

            return DbHelper.ExecuteListQuery<CrRetencionDeduccionesData>(
                _portalDb,
                codEmpresa,
                sql,
                CrearParametrosConsulta(request, false));
        }

        private static object CrearParametrosConsulta(CrRetencionDeduccionesObtenerRequest request, bool usaHora)
        {
            var fechaInicio = request.fecha_inicio ?? DateTime.Now;
            var fechaCorte = request.fecha_corte ?? fechaInicio;

            return new
            {
                Codigo = request.codigo.Trim().ToUpperInvariant(),
                CodInstitucion = request.cod_institucion ?? 0,
                FechaInicio = fechaInicio.Date,
                FechaCorte = usaHora
                    ? fechaCorte.Date.AddHours(23).AddMinutes(59).AddSeconds(59)
                    : fechaCorte.Date,
                FechaInicioSoloFecha = fechaInicio.Date,
                FechaCorteSoloFecha = fechaCorte.Date,
                Proceso = $"{(request.proceso ?? string.Empty).Trim()}%"
            };
        }

        private static List<CrRetencionDeduccionesData> ConsolidarDeducciones(
            List<CrRetencionDeduccionesData> baseLista,
            List<CrRetencionDeduccionesData> morosidad)
        {
            var salida = baseLista
                .Select(x => new CrRetencionDeduccionesData
                {
                    cedula = x.cedula,
                    nombre = x.nombre,
                    monto = x.monto
                })
                .ToList();

            foreach (var item in morosidad)
            {
                var actual = salida.FirstOrDefault(x => x.cedula == item.cedula);
                if (actual is null)
                {
                    salida.Add(item);
                    continue;
                }

                actual.monto += item.monto;
            }

            return salida;
        }

        private static ErrorDto ValidarFiltros(CrRetencionDeduccionesObtenerRequest request)
        {
            request.codigo = (request.codigo ?? string.Empty).Trim().ToUpperInvariant();
            request.formato = NormalizarFormato(request.formato);
            request.tipo = NormalizarTipo(request.tipo);
            request.proceso = (request.proceso ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return DbHelper.ErrorResponse("Debe indicar el cliente.", -1);
            }

            if (request.tipo == "F")
            {
                if (!request.fecha_inicio.HasValue || !request.fecha_corte.HasValue)
                {
                    return DbHelper.ErrorResponse("Debe indicar las fechas de inicio y corte.", -1);
                }

                if (request.fecha_corte.Value.Date < request.fecha_inicio.Value.Date)
                {
                    return DbHelper.ErrorResponse("La fecha corte no puede ser menor que la fecha inicio.", -1);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.proceso))
                {
                    return DbHelper.ErrorResponse("Debe indicar el proceso.", -1);
                }
            }

            return DbHelper.OkResponse("Ok");
        }

        private static string NormalizarFormato(string? formato)
        {
            return formato switch
            {
                "02" => "02",
                "03" => "03",
                "04" => "04",
                _ => "01"
            };
        }

        private static string NormalizarTipo(string? tipo)
        {
            return tipo == "F" ? "F" : "P";
        }

        private static string ConstruirNombreArchivo(
            string formato,
            CrRetencionDeduccionesArchivoRequest request,
            DateTime fechaServidor,
            string descripcionInstitucion,
            int codInstitucion)
        {
            string prefijo = formato switch
            {
                "02" => "INTEGRA",
                "03" => "CCSS",
                "04" => "SPA",
                _ => "SIF"
            };

            if (NormalizarTipo(request.tipo) == "F")
            {
                return $"{prefijo}-{fechaServidor:yyyyMMdd}-{codInstitucion:00}.{descripcionInstitucion}.txt";
            }

            return $"{request.proceso}_{codInstitucion:00} {descripcionInstitucion} [{prefijo}].txt";
        }

        private static string GenerarContenidoProGrX(List<CrRetencionDeduccionesData> lista)
        {
            var sb = new StringBuilder();

            foreach (var item in lista)
            {
                string linea =
                    Rellenar(item.cedula, 15, ' ', false) +
                    Rellenar(item.nombre, 50, ' ', false) +
                    item.monto.ToString("000000000.00");

                sb.AppendLine(linea);
            }

            return sb.ToString();
        }

        private static string GenerarContenidoIntegra(
            List<CrRetencionDeduccionesData> lista,
            CrRetencionDeduccionesArchivoRequest request)
        {
            var sb = new StringBuilder();
            DateTime fecha = request.fecha_corte ?? request.fecha_inicio ?? DateTime.Now;

            foreach (var item in lista)
            {
                string linea =
                    RellenarSoloNumeros(item.cedula, 10) + "\t102000115\t" +
                    Math.Round(item.monto, 0).ToString("0") + "\t" +
                    fecha.ToString("dd/MM/yyyy");

                sb.AppendLine(linea);
            }

            return sb.ToString();
        }

        private static string GenerarContenidoCCSS(List<CrRetencionDeduccionesData> lista)
        {
            var sb = new StringBuilder();

            foreach (var item in lista)
            {
                string monto = ((long)Math.Round(item.monto * 100, 0)).ToString();
                string linea =
                    RellenarSoloNumeros(item.cedula, 11) +
                    "3464025252      " +
                    Rellenar(monto, 13, '0', true) +
                    Rellenar(string.Empty, 47, '0', false) +
                    Rellenar(item.nombre, 31, ' ', false) +
                    ".";

                sb.AppendLine(linea);
            }

            return sb.ToString();
        }

        private static string GenerarContenidoSPA(
            List<CrRetencionDeduccionesData> lista,
            CrRetencionDeduccionesArchivoRequest request)
        {
            var sb = new StringBuilder();
            DateTime fecha = request.fecha_corte ?? request.fecha_inicio ?? DateTime.Now;

            foreach (var item in lista)
            {
                string monto = ((long)Math.Round(item.monto * 100, 0)).ToString();
                string linea =
                    RellenarSoloNumeros(item.cedula, 10) +
                    Rellenar(item.nombre, 28, ' ', false) +
                    "04573500439" +
                    Rellenar(monto, 8, '0', true) +
                    "Q01   000000000" +
                    fecha.ToString("yyyyMMdd") +
                    "5535511";

                sb.AppendLine(linea);
            }

            return sb.ToString();
        }

        private static string Rellenar(string valor, int largo, char caracter, bool izquierda)
        {
            valor ??= string.Empty;
            valor = valor.Trim();

            if (valor.Length > largo)
            {
                valor = valor[..largo];
            }

            return izquierda
                ? valor.PadLeft(largo, caracter)
                : valor.PadRight(largo, caracter);
        }

        private static string RellenarSoloNumeros(string valor, int largo)
        {
            string limpio = new string((valor ?? string.Empty).Where(char.IsDigit).ToArray());
            return Rellenar(limpio, largo, '0', true);
        }
    }
}