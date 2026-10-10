using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrPeriodosGraciaDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrPeriodosGraciaDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el catalogo de garantias.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Garantias_Obtener(int codEmpresa)
        {
            const string sqlGarantias = @"
                select
                    rtrim(Garantia) as item,
                    rtrim(descripcion) as descripcion
                from crd_garantia_tipos
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlGarantias
            );
        }

        /// <summary>
        /// Obtiene el catalogo de divisas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Divisas_Obtener(int codEmpresa)
        {
            const string sqlDivisas = @"
                select
                    rtrim(cod_divisa) as item,
                    rtrim(descripcion) as descripcion
                from vsys_divisas;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlDivisas
            );
        }

        /// <summary>
        /// Obtiene el catalogo de recursos (grupos).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="lineas"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Recursos_Obtener(
            int codEmpresa,
            bool lineas,
            string? codigo)
        {
            if (!lineas && string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "Debe enviar el codigo cuando lineas es false.",
                    Result = []
                };
            }

            const string sqlRecursosLineas = @"
                select
                    rtrim(cod_grupo) as item,
                    rtrim(descripcion) as descripcion
                from catalogo_grupos
                order by descripcion;";

            const string sqlRecursosCodigo = @"
                select
                    rtrim(R.cod_grupo) as item,
                    rtrim(R.descripcion) as descripcion
                from catalogo_grupos R
                inner join catalogo_AsignaGrp A
                    on R.cod_grupo = A.cod_grupo
                where A.codigo = @Codigo
                order by R.descripcion;";

            if (lineas)
            {
                return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                    _portalDb,
                    codEmpresa,
                    sqlRecursosLineas
                );
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlRecursosCodigo,
                new
                {
                    Codigo = codigo!.Trim()
                }
            );
        }

        /// <summary>
        /// Obtiene el catalogo de destinos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="lineas"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Destinos_Obtener(
            int codEmpresa,
            bool lineas,
            string? codigo)
        {
            if (!lineas && string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "Debe enviar el codigo cuando lineas es false.",
                    Result = []
                };
            }

            const string sqlDestinosLineas = @"
                select
                    rtrim(cod_destino) as item,
                    rtrim(descripcion) as descripcion
                from catalogo_destinos
                order by descripcion;";

            const string sqlDestinosCodigo = @"
                select
                    rtrim(R.cod_destino) as item,
                    rtrim(R.descripcion) as descripcion
                from catalogo_destinos R
                inner join catalogo_destinosAsg A
                    on R.cod_destino = A.cod_destino
                where A.codigo = @Codigo
                order by R.descripcion;";

            if (lineas)
            {
                return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                    _portalDb,
                    codEmpresa,
                    sqlDestinosLineas
                );
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlDestinosCodigo,
                new
                {
                    Codigo = codigo!.Trim()
                }
            );
        }

        /// <summary>
        /// Obtiene el catalogo de instituciones.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Instituciones_Obtener(int codEmpresa)
        {
            const string sqlInstituciones = @"
                select
                    rtrim(cod_institucion) as item,
                    rtrim(descripcion) as descripcion
                from instituciones
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlInstituciones
            );
        }

        /// <summary>
        /// Obtiene instituciones deductoras.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="todos"></param>
        /// <param name="codInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Deductoras_Obtener(
            int codEmpresa,
            bool todos,
            string? codInstitucion)
        {
            if (todos)
                return CrPeriodosGracia_Instituciones_Obtener(codEmpresa);

            if (string.IsNullOrWhiteSpace(codInstitucion))
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "Debe enviar codInstitucion cuando todos es false.",
                    Result = []
                };
            }

            const string sqlDeductoras = @"
                exec spAFI_Institucion_Vinculadas
                    @CodInstitucion,
                    3;";

            var resp = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.Query<(string? IdX, string? ItmX)>(
                    sqlDeductoras,
                    new
                    {
                        CodInstitucion = codInstitucion.Trim()
                    }
                ).ToList()
            );

            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = resp.Code,
                Description = resp.Description,
                Result =
                [
                    .. (resp.Result ?? [])
                        .Select(x => new DropDownListaGenericaModel
                        {
                            item = (x.IdX ?? string.Empty).Trim(),
                            descripcion = (x.ItmX ?? string.Empty).Trim()
                        })
                ]
            };
        }

        /// <summary>
        /// Obtiene el catalogo de estados de persona.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_EstadosPersona_Obtener(int codEmpresa)
        {
            const string sqlEstadosPersona = @"
                select
                    rtrim(cod_estado) as item,
                    rtrim(descripcion) as descripcion
                from afi_estados_persona
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlEstadosPersona
            );
        }

        /// <summary>
        /// Obtiene el catalogo de estado laboral activo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_EstadosLaborales_Obtener(int codEmpresa)
        {
            const string sqlEstadosLaborales = @"
                select
                    rtrim(Estado_Laboral) as item,
                    rtrim(Descripcion) as descripcion
                from AFI_ESTADO_LABORAL
                where Activo = 1
                order by Descripcion asc;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlEstadosLaborales
            );
        }

        /// <summary>
        /// Obtiene el catalogo de lineas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPeriodosGracia_Lineas_Obtener(int codEmpresa)
        {
            const string sqlLineas = @"
                select
                    rtrim(codigo) as item,
                    rtrim(descripcion) as descripcion
                from catalogo
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlLineas
            );
        }

        /// <summary>
        /// Ejecuta consulta masiva de periodo de gracia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<dynamic>> CrPeriodosGracia_Consulta_Obtener(
            int codEmpresa,
            CrPeriodosGraciaConsultaRequest request)
        {
            const string sqlConsulta = @"
                exec spCrd_Masivo_Periodo_Gracia_Consulta
                    @Linea = @Linea,
                    @Garantia = @Garantia,
                    @Destino = @Destino,
                    @Recurso = @Recurso,
                    @Institucion = @Institucion,
                    @Deductora = @Deductora,
                    @Divisa = @Divisa,
                    @EstadoPersona = @EstadoPersona,
                    @EstadoLaboral = @EstadoLaboral,
                    @FormalizaInicio = @FormalizaInicio,
                    @FormalizaCorte = @FormalizaCorte,
                    @FechaInicio = @FechaInicio,
                    @FechaCorte = @FechaCorte,
                    @PlazoRng = @PlazoRng,
                    @PlazoInicio = @PlazoInicio,
                    @PlazoCorte = @PlazoCorte,
                    @TasaRng = @TasaRng,
                    @TasaInicio = @TasaInicio,
                    @TasaCorte = @TasaCorte,
                    @CobroTipo = @CobroTipo,
                    @OperacionTipo = @OperacionTipo,
                    @PriDeducApl = @PriDeducApl,
                    @PriDeducFiltro = @PriDeducFiltro,
                    @PriDeducValor = @PriDeducValor,
                    @UltDeducApl = @UltDeducApl,
                    @UltDeducFiltro = @UltDeducFiltro,
                    @UltDeducValor = @UltDeducValor,
                    @Tipo = @Tipo,
                    @PlazoAdj = @PlazoAdj,
                    @Apl_Retroactivo = @Apl_Retroactivo,
                    @Apl_IntCor = @Apl_IntCor,
                    @Apl_Cargos = @Apl_Cargos,
                    @Apl_Poliza = @Apl_Poliza,
                    @Usuario = @Usuario,
                    @Detalle = @Detalle;";

            return DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.Query<dynamic>(
                    sqlConsulta,
                    CrearParametrosMasivo(request)
                ).ToList()
            );
        }

        /// <summary>
        /// Ejecuta la aplicacion masiva de periodo de gracia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrPeriodosGracia_Aplicar_Ejecutar(
            int codEmpresa,
            CrPeriodosGraciaConsultaRequest request)
        {
            const string sqlAplicar = @"
                exec spCrd_Masivo_Periodo_Gracia
                    @Linea = @Linea,
                    @Garantia = @Garantia,
                    @Destino = @Destino,
                    @Recurso = @Recurso,
                    @Institucion = @Institucion,
                    @Deductora = @Deductora,
                    @Divisa = @Divisa,
                    @EstadoPersona = @EstadoPersona,
                    @EstadoLaboral = @EstadoLaboral,
                    @FormalizaInicio = @FormalizaInicio,
                    @FormalizaCorte = @FormalizaCorte,
                    @FechaInicio = @FechaInicio,
                    @FechaCorte = @FechaCorte,
                    @PlazoRng = @PlazoRng,
                    @PlazoInicio = @PlazoInicio,
                    @PlazoCorte = @PlazoCorte,
                    @TasaRng = @TasaRng,
                    @TasaInicio = @TasaInicio,
                    @TasaCorte = @TasaCorte,
                    @CobroTipo = @CobroTipo,
                    @OperacionTipo = @OperacionTipo,
                    @PriDeducApl = @PriDeducApl,
                    @PriDeducFiltro = @PriDeducFiltro,
                    @PriDeducValor = @PriDeducValor,
                    @UltDeducApl = @UltDeducApl,
                    @UltDeducFiltro = @UltDeducFiltro,
                    @UltDeducValor = @UltDeducValor,
                    @Tipo = @Tipo,
                    @PlazoAdj = @PlazoAdj,
                    @Apl_Retroactivo = @Apl_Retroactivo,
                    @Apl_IntCor = @Apl_IntCor,
                    @Apl_Cargos = @Apl_Cargos,
                    @Apl_Poliza = @Apl_Poliza,
                    @Usuario = @Usuario,
                    @Detalle = @Detalle;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlAplicar,
                CrearParametrosMasivo(request)
            );

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Proceso aplicado satisfactoriamente..."
            };
        }

        private static object CrearParametrosMasivo(CrPeriodosGraciaConsultaRequest request)
        {
            return new
            {
                request.Linea,
                request.Garantia,
                request.Destino,
                request.Recurso,
                request.Institucion,
                request.Deductora,
                request.Divisa,
                request.EstadoPersona,
                request.EstadoLaboral,
                FormalizaInicio = request.FormalizaInicio?.Date,
                FormalizaCorte = request.FormalizaCorte?.Date,
                FechaInicio = request.AplInicio?.Date,
                FechaCorte = request.AplCorte?.Date,
                PlazoRng = BoolToSmallInt(request.PlazoRng),
                request.PlazoInicio,
                request.PlazoCorte,
                TasaRng = BoolToSmallInt(request.TasaRng),
                request.TasaInicio,
                request.TasaCorte,
                request.CobroTipo,
                request.OperacionTipo,
                PriDeducApl = BoolToSmallInt(request.PriDeducApl),
                request.PriDeducFiltro,
                PriDeducValor = request.PriDeduc,
                UltDeducApl = BoolToSmallInt(request.UltDeducApl),
                request.UltDeducFiltro,
                UltDeducValor = request.UltDeduc,
                Tipo = request.TipoAplicacion,
                PlazoAdj = BoolToSmallInt(request.AplAjustaPlazo),
                Apl_Retroactivo = BoolToSmallInt(request.AplRetroactivo),
                Apl_IntCor = BoolToSmallInt(request.AplIntereses),
                Apl_Cargos = BoolToSmallInt(request.AplCargos),
                Apl_Poliza = BoolToSmallInt(request.AplPolizas),
                request.Usuario,
                Detalle = request.Nota
            };
        }

        private static short? BoolToSmallInt(bool? valor)
        {
            if (!valor.HasValue)
            {
                return null;
            }

            short resultado = valor.Value ? (short)1 : (short)0;
            return resultado;
        }

    }
}
