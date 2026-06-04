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
        private readonly MProGrxMain _mProGrxMain;

        public FrmCrRetencionDeduccionesDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene la información inicial de la pantalla.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrRetencionDeduccionesPantallaData> Cr_RetencionDeducciones_Pantalla_Obtener(
            int codEmpresa,
            string usuario)
        {
            var globalesResp = ObtenerGlobales(codEmpresa, usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? "No fue posible obtener los parámetros globales.",
                    globalesResp.Code.GetValueOrDefault(-1),
                    new CrRetencionDeduccionesPantallaData());
            }

            var globales = globalesResp.Result;

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

            var fechaServidor = globales.fxFechaServidor ?? DateTime.Now;
            var procesoDefault = ObtenerProcesoDefault(globales, fechaServidor);

            var salida = new CrRetencionDeduccionesPantallaData
            {
                clientes = clientes.Result ?? new List<DropDownListaGenericaModel>(),
                instituciones = instituciones.Result ?? new List<DropDownListaGenericaModel>(),
                fecha_servidor = fechaServidor,
                proceso_default = procesoDefault,
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

            var globalesResp = ObtenerGlobales(codEmpresa, request.usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? "No fue posible obtener la configuración global.",
                    globalesResp.Code.GetValueOrDefault(-1),
                    new CrRetencionDeduccionesResultadoData());
            }

            var globales = globalesResp.Result;
            var usaPlanPagos = globales.SysPlanPagos == 1;

            var listaPrincipal = usaPlanPagos
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

            if (!usaPlanPagos)
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
                usuario = request.usuario,
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

            var globalesResp = ObtenerGlobales(codEmpresa, request.usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? "No fue posible obtener los parámetros globales.",
                    globalesResp.Code.GetValueOrDefault(-1),
                    new CrRetencionDeduccionesArchivoData());
            }

            var fechaServidor = globalesResp.Result.fxFechaServidor ?? DateTime.Now;
            string formato = NormalizarFormato(request.formato);
            string descripcionInstitucion = institucion.Result?.descripcion ?? "TODOS";

            string nombreArchivo = ConstruirNombreArchivo(
                formato,
                request,
                fechaServidor,
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
        /// Obtiene los parámetros globales reutilizables del formulario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private ErrorDto<Globales> ObtenerGlobales(int codEmpresa, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario para obtener los parámetros globales.",
                    -1,
                    new Globales());
            }

            return _mProGrxMain.sbSifParametrosInicializa(codEmpresa, usuario.Trim());
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
                select item, descripcion
                from
                (
                    select
                        0 as orden,
                        cast(0 as int) as item,
                        'TODOS' as descripcion

                    union all

                    select
                        1 as orden,
                        cast(cod_institucion as int) as item,
                        rtrim(descripcion) as descripcion
                    from instituciones
                    where activa = 1
                ) as x
                order by x.orden, x.descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);
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

            var response = DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql,
                new DropDownListaGenericaModel(),
                new { CodInstitucion = codInstitucion });

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    response.Description ?? "No fue posible obtener la institución.",
                    response.Code.GetValueOrDefault(-1),
                    new DropDownListaGenericaModel());
            }

            return DbHelper.CreateOkResponse(
                response.Result ?? new DropDownListaGenericaModel());
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

        private static string ObtenerProcesoDefault(Globales globales, DateTime fechaServidor)
        {
            if (globales.GlngFechaCR > 0)
            {
                return Convert.ToInt64(globales.GlngFechaCR).ToString();
            }

            return $"{fechaServidor.Year}{fechaServidor.Month:00}";
        }

        private static ErrorDto ValidarFiltros(CrRetencionDeduccionesObtenerRequest request)
        {
            request.usuario = (request.usuario ?? string.Empty).Trim();
            request.codigo = (request.codigo ?? string.Empty).Trim().ToUpperInvariant();
            request.formato = NormalizarFormato(request.formato);
            request.tipo = NormalizarTipo(request.tipo);
            request.proceso = (request.proceso ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse("Debe indicar el usuario.", -1);
            }

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
            string prefijoFecha = formato switch
            {
                "02" => "INTEGRA",
                "03" => "CCSS",
                "04" => "SPA",
                _ => "SIF"
            };

            string prefijoProceso = formato switch
            {
                "02" => "INTEGRA",
                "03" => "CCSS",
                "04" => "SPA",
                _ => "ProGrX"
            };

            if (NormalizarTipo(request.tipo) == "F")
            {
                return $"{prefijoFecha}-{fechaServidor:yyyyMMdd}-{codInstitucion:00}.{descripcionInstitucion}.txt";
            }

            return $"{request.proceso}_{codInstitucion:00} {descripcionInstitucion} [{prefijoProceso}].txt";
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