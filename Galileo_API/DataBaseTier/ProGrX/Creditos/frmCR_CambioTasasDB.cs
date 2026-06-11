using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;
using System.Data;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCambioTasasDb
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;
        private const string SpConsulta = "spCrd_Masivo_Cambio_Tasa_Consulta";
        private const string SpAplicar = "spCrd_Masivo_Cambio_Tasa_Operacion";

        public FrmCrCambioTasasDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene los catalogos iniciales usados por frmCR_CambioTasas.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="usuario">Usuario usado para cargar Globales.</param>
        /// <returns>Catalogos, fecha servidor, GlngFechaCR y valor TBP.</returns>
        public ErrorDto<CrCambioTasasInicialResponse> CR_CambioTasas_Inicializar(int codEmpresa, string usuario)
        {
            var response = new CrCambioTasasInicialResponse();
            var globales = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, usuario ?? string.Empty);
            if (globales.Code < 0)
                return DbHelper.CreateErrorResponse<CrCambioTasasInicialResponse>(globales.Description ?? "No fue posible obtener globales.", result: response);

            response.fechaServidor = _mProGrxMain.fxFechaServidor(codEmpresa, 0);
            response.glngFechaCR = globales.Result?.GlngFechaCR ?? 0;
            response.tbp = ObtenerTbp(codEmpresa);
            response.garantias = ObtenerLista(codEmpresa, QryGarantias);
            response.divisas = ObtenerLista(codEmpresa, QryDivisas);
            response.estadosPersona = ObtenerLista(codEmpresa, QryEstadosPersona);
            response.instituciones = ObtenerLista(codEmpresa, QryInstituciones);
            response.deductoras = response.instituciones;
            response.estadosLaboral = ObtenerLista(codEmpresa, QryEstadosLaboral);
            response.recursos = ObtenerLista(codEmpresa, QryRecursosTodos);
            response.destinos = ObtenerLista(codEmpresa, QryDestinosTodos);

            return DbHelper.CreateOkResponse(response);
        }

        /// <summary>
        /// Obtiene deductoras filtradas por institucion, replicando spAFI_Institucion_Vinculadas.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codInstitucion">Institucion seleccionada; null obtiene todas.</param>
        /// <returns>Lista de deductoras.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_CambioTasas_Deductoras(int codEmpresa, int? codInstitucion)
        {
            if (!codInstitucion.HasValue)
                return DbHelper.CreateOkResponse(ObtenerLista(codEmpresa, QryInstituciones));

            const string sql = "exec spAFI_Institucion_Vinculadas @CodInstitucion, 3";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { CodInstitucion = codInstitucion.Value });
        }

        /// <summary>
        /// Obtiene recursos y destinos, todos o asociados a una linea especifica.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codigo">Codigo de linea cuando no se consultan todas.</param>
        /// <param name="todas">Indica si deben cargarse todos los recursos/destinos.</param>
        /// <returns>Catalogos de recursos y destinos.</returns>
        public ErrorDto<CrCambioTasasCatalogosLineaResponse> CR_CambioTasas_Catalogos_Linea(
            int codEmpresa,
            string? codigo,
            bool todas)
        {
            var parametros = new { Codigo = (codigo ?? string.Empty).Trim() };
            var response = new CrCambioTasasCatalogosLineaResponse
            {
                recursos = ObtenerLista(codEmpresa, todas ? QryRecursosTodos : QryRecursosLinea, parametros),
                destinos = ObtenerLista(codEmpresa, todas ? QryDestinosTodos : QryDestinosLinea, parametros)
            };

            return DbHelper.CreateOkResponse(response);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_CambioTasas_Lineas_F4(int codEmpresa)
        {
            const string sql = @"
                select rtrim(codigo) as item, rtrim(descripcion) as descripcion
                from catalogo
                order by descripcion";

            return DbHelper.CreateOkResponse(ObtenerListaSinTodos(codEmpresa, sql));
        }

        public ErrorDto<string> CR_CambioTasas_Linea_Describir(int codEmpresa, string codigo)
        {
            const string sql = @"
                select top 1 rtrim(descripcion)
                from catalogo
                where codigo = @Codigo";

            var resp = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                sql,
                string.Empty,
                new { Codigo = (codigo ?? string.Empty).Trim() });

            return DbHelper.CreateOkResponse(resp.Result ?? string.Empty);
        }

        /// <summary>
        /// Consulta las operaciones candidatas para cambio masivo de tasa.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="request">Filtros y parametros de calculo.</param>
        /// <returns>Operaciones y resumen calculado.</returns>
        public ErrorDto<CrCambioTasasConsultaResponse> CR_CambioTasas_Consultar(
            int codEmpresa,
            CrCambioTasasConsultaRequest request)
        {
            var validacion = ValidarConsulta(request);
            if (validacion.Code < 0)
                return DbHelper.CreateErrorResponse<CrCambioTasasConsultaResponse>(validacion.Description ?? "Request invalido.", result: new());

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                conn =>
                {
                    var operaciones = conn.Query<CrCambioTasasOperacionRow>(
                        SpConsulta,
                        CrearParametrosConsulta(request),
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 0).ToList();

                    return new CrCambioTasasConsultaResponse
                    {
                        operaciones = operaciones,
                        resumen = CalcularResumen(operaciones)
                    };
                });
        }

        /// <summary>
        /// Aplica el cambio masivo de tasa sobre las operaciones consultadas.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="request">Operaciones y parametros de aplicacion.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto CR_CambioTasas_Aplicar(int codEmpresa, CrCambioTasasAplicarRequest request)
        {
            if (request is null || request.operaciones.Count == 0)
                return DbHelper.ErrorResponse("No hay operaciones para aplicar el cambio de tasas.");

            var usuario = (request.usuario ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.ErrorResponse("Debe indicar el usuario que aplica el cambio.");

            if (!TasaValida(request.tasaAplRef))
                return DbHelper.ErrorResponse("La tasa indicada no es valida, verifique.");

            var operacionInvalida = request.operaciones.Any(OperacionAplicarInvalida);
            if (operacionInvalida)
                return DbHelper.ErrorResponse("Hay operaciones con datos incompletos para aplicar el cambio de tasas.");

            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);
                conn.Open();
                using var tx = conn.BeginTransaction();

                foreach (var operacion in request.operaciones)
                {
                    conn.Execute(
                        SpAplicar,
                        CrearParametrosAplicar(request, operacion, usuario),
                        tx,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 0);
                }

                tx.Commit();
                return DbHelper.OkResponse("Cambio de Tasas Realizado Satisfactoriamente...");
            }
            catch (DbException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private List<DropDownListaGenericaModel> ObtenerLista(int codEmpresa, string sql, object? parametros = null)
        {
            var resp = DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, parametros);
            return AgregarTodos(resp.Result ?? []);
        }

        private List<DropDownListaGenericaModel> ObtenerListaSinTodos(int codEmpresa, string sql, object? parametros = null)
        {
            var resp = DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, parametros);
            return resp.Result ?? [];
        }

        private decimal ObtenerTbp(int codEmpresa)
        {
            const string sql = "select try_convert(decimal(10,4), valor) from CRD_PARAMETROS where COD_PARAMETRO = '07'";
            var resp = DbHelper.ExecuteSingleQuery<decimal?>(_portalDb, codEmpresa, sql, 0);
            return resp.Result ?? 0;
        }

        private static ErrorDto ValidarConsulta(CrCambioTasasConsultaRequest request)
        {
            if (request is null)
                return DbHelper.ErrorResponse("Request invalido.");

            if (!TasaValida(request.tasaAplRef))
                return DbHelper.ErrorResponse("La tasa indicada no es valida, verifique.");

            return DbHelper.CreateOkResponse();
        }

        private static bool TasaValida(decimal? tasa)
            => tasa.HasValue && tasa.Value >= 0 && tasa.Value <= 100;

        private static DynamicParameters CrearParametrosConsulta(CrCambioTasasConsultaRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Linea", ValorTexto(request.linea));
            parameters.Add("@Garantia", ValorTexto(request.garantia));
            parameters.Add("@Destino", ValorTexto(request.destino));
            parameters.Add("@Recurso", ValorTexto(request.recurso));
            parameters.Add("@Institucion", request.institucion);
            parameters.Add("@Deductora", request.deductora);
            parameters.Add("@Divisa", ValorTexto(request.divisa));
            parameters.Add("@EstadoPersona", ValorTexto(request.estadoPersona));
            parameters.Add("@EstadoLaboral", ValorTexto(request.estadoLaboral));
            parameters.Add("@FormalizaInicio", InicioDia(request.formalizaInicio));
            parameters.Add("@FormalizaCorte", FinDia(request.formalizaCorte));
            parameters.Add("@PlazoRng", request.aplicaPlazo == true ? 1 : 0);
            parameters.Add("@PlazoInicio", request.aplicaPlazo == true ? request.plazoInicio : null);
            parameters.Add("@PlazoCorte", request.aplicaPlazo == true ? request.plazoCorte : null);
            parameters.Add("@TasaRng", request.aplicaTasa == true ? 1 : 0);
            parameters.Add("@TasaInicio", request.aplicaTasa == true ? request.tasaInicio : null);
            parameters.Add("@TasaCorte", request.aplicaTasa == true ? request.tasaCorte : null);
            parameters.Add("@CobroTipo", ValorTexto(request.cobroTipo));
            parameters.Add("@OperacionTipo", ValorTexto(request.operacionTipo));
            parameters.Add("@PriDeducApl", request.aplicaPriDeduc == true ? 1 : 0);
            parameters.Add("@PriDeducFiltro", request.aplicaPriDeduc == true ? ValorTexto(request.priDeducFiltro) : null);
            parameters.Add("@PriDeducValor", request.aplicaPriDeduc == true ? request.priDeduc : null);
            parameters.Add("@UltDeducApl", request.aplicaUltDeduc == true ? 1 : 0);
            parameters.Add("@UltDeducFiltro", request.aplicaUltDeduc == true ? ValorTexto(request.ultDeducFiltro) : null);
            parameters.Add("@UltDeducValor", request.aplicaUltDeduc == true ? request.ultDeduc : null);
            parameters.Add("@TasaTipo", NormalizarCodigo(request.tasaTipo, "R"));
            parameters.Add("@TasaAplTipo", NormalizarCodigo(request.tasaAplTipo, "N"));
            parameters.Add("@TasaAplCtas", NormalizarCodigo(request.tasaAplCtas, "R"));
            parameters.Add("@TasaAplRef", request.tasaAplRef);
            parameters.Add("@Usuario", (request.usuario ?? string.Empty).Trim());
            parameters.Add("@Detalle", (request.detalle ?? string.Empty).Trim());
            return parameters;
        }

        private static bool OperacionAplicarInvalida(CrCambioTasasOperacionAplicar operacion)
            => operacion.id_solicitud is null
               || string.IsNullOrWhiteSpace(operacion.codigo)
               || operacion.tasa is null
               || operacion.tasa_nueva is null
               || operacion.cuota_nueva is null
               || operacion.plazo_restante is null;

        private static DynamicParameters CrearParametrosAplicar(
            CrCambioTasasAplicarRequest request,
            CrCambioTasasOperacionAplicar operacion,
            string usuario)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Operacion", operacion.id_solicitud);
            parameters.Add("@Codigo", (operacion.codigo ?? string.Empty).Trim());
            parameters.Add("@Tipo", NormalizarCodigo(request.tasaTipo, "R"));
            parameters.Add("@TasaActual", operacion.tasa);
            parameters.Add("@TasaNew", operacion.tasa_nueva);
            parameters.Add("@Cuota", operacion.cuota_nueva);
            parameters.Add("@PlazoRst", operacion.plazo_restante);
            parameters.Add("@TasaTipo", NormalizarCodigo(request.tasaTipo, "R"));
            parameters.Add("@TasaAplTipo", NormalizarCodigo(request.tasaAplTipo, "N"));
            parameters.Add("@TasaAplCtas", NormalizarCodigo(request.tasaAplCtas, "R"));
            parameters.Add("@TasaAplRef", request.tasaAplRef);
            parameters.Add("@Usuario", usuario);
            parameters.Add("@Detalle", (request.detalle ?? string.Empty).Trim());
            return parameters;
        }

        private static CrCambioTasasResumen CalcularResumen(List<CrCambioTasasOperacionRow> operaciones)
        {
            return new CrCambioTasasResumen
            {
                casos = operaciones.Count,
                cuotasActuales = operaciones.Sum(x => x.cuota),
                cuotasNuevas = operaciones.Sum(x => x.cuota_nueva),
                diferenciaInteres = operaciones.Sum(x => (x.saldo * x.tasa_nueva / 1200) - (x.saldo * x.tasa / 1200))
            };
        }

        private static List<DropDownListaGenericaModel> AgregarTodos(List<DropDownListaGenericaModel> lista)
        {
            lista.Insert(0, new DropDownListaGenericaModel { item = null, descripcion = "TODOS" });
            return lista;
        }

        private static string? ValorTexto(string? valor)
        {
            var limpio = (valor ?? string.Empty).Trim();
            return string.IsNullOrWhiteSpace(limpio) ? null : limpio;
        }

        private static string NormalizarCodigo(string? valor, string predeterminado)
            => (string.IsNullOrWhiteSpace(valor) ? predeterminado : valor.Trim().Substring(0, 1)).ToUpperInvariant();

        private static DateTime? InicioDia(DateTime? fecha)
            => fecha?.Date;

        private static DateTime? FinDia(DateTime? fecha)
            => fecha?.Date.AddDays(1).AddTicks(-1);

        private const string QryGarantias = @"
            select rtrim(Garantia) as item, rtrim(descripcion) as descripcion
            from crd_garantia_tipos
            order by descripcion";

        private const string QryDivisas = @"
            select cod_divisa as item, descripcion
            from vsys_divisas";

        private const string QryEstadosPersona = @"
            select rtrim(cod_estado) as item, rtrim(descripcion) as descripcion
            from afi_estados_persona
            order by descripcion";

        private const string QryInstituciones = @"
            select cod_institucion as item, rtrim(descripcion) as descripcion
            from instituciones
            order by descripcion";

        private const string QryEstadosLaboral = @"
            select Estado_Laboral as item, Descripcion as descripcion
            from AFI_ESTADO_LABORAL
            where Activo = 1
            order by Descripcion asc";

        private const string QryRecursosTodos = @"
            select rtrim(cod_grupo) as item, rtrim(descripcion) as descripcion
            from catalogo_grupos
            order by descripcion";

        private const string QryDestinosTodos = @"
            select rtrim(cod_destino) as item, rtrim(descripcion) as descripcion
            from catalogo_destinos
            order by descripcion";

        private const string QryRecursosLinea = @"
            select R.cod_grupo as item, rtrim(R.descripcion) as descripcion
            from catalogo_grupos R
            inner join catalogo_AsignaGrp A on R.cod_grupo = A.cod_grupo
            where A.codigo = @Codigo
            order by R.descripcion";

        private const string QryDestinosLinea = @"
            select R.cod_destino as item, rtrim(R.descripcion) as descripcion
            from catalogo_destinos R
            inner join catalogo_destinosAsg A on R.cod_destino = A.cod_destino
            where A.codigo = @Codigo
            order by R.descripcion";
    }
}
