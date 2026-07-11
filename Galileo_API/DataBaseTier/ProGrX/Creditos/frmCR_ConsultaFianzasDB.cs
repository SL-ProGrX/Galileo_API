using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrConsultaFianzasDb
    {
        private const string MsgCedulaRequerida = "Debe indicar la c&eacute;dula.";
        private const string MsgOperacionRequerida = "Debe indicar la operaci&oacute;n.";
        private const string MsgConsultaFianzasError = "No fue posible consultar las fianzas.";
        private const string MsgNombreError = "No fue posible obtener el nombre de la persona.";
        private const string MsgEstadoDeudorError = "No fue posible consultar el estado del deudor.";
        private const string MsgMoraError = "No fue posible consultar la mora de la operaci&oacute;n.";

        private const string TipoTodos = "X";
        private const string TipoFianzas = "F";
        private const string TipoTraslados = "T";
        private const string ProcesoNormal = "N";
        private const string EstadoAsociado = "S";

        private readonly PortalDB _portalDb;

        public FrmCrConsultaFianzasDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<CrConsultaFianzasConsultaData> CrConsultaFianzas_Consulta_Obtener(
            int codEmpresa,
            CrConsultaFianzasConsultaRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.CreateErrorResponse(
                    MsgCedulaRequerida,
                    -2,
                    new CrConsultaFianzasConsultaData());
            }

            try
            {
                string cedula = request.cedula.Trim();
                string tipo = NormalizarTipo(request.tipo);
                string estado = request.canceladas ? "C" : "A";

                ErrorDto<List<CrConsultaFianzasItemDto>> listaResponse =
                    DbHelper.ExecuteListQuery<CrConsultaFianzasItemDto>(
                        _portalDb,
                        codEmpresa,
                        @"exec spCrd_Consulta_Fianzas_Rsm @Cedula, @Estado, @Tipo",
                        new
                        {
                            Cedula = cedula,
                            Estado = estado,
                            Tipo = tipo
                        });

                if (listaResponse.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        listaResponse.Description ?? MsgConsultaFianzasError,
                        listaResponse.Code ?? -1,
                        new CrConsultaFianzasConsultaData());
                }

                List<CrConsultaFianzasItemDto> lista = PrepararLista(listaResponse.Result);

                ErrorDto<string?> nombreResponse = DbHelper.ExecuteSingleQuery<string>(
                    _portalDb,
                    codEmpresa,
                    "select isnull(dbo.fxNombre(@Cedula), '')",
                    string.Empty,
                    new { Cedula = cedula });

                if (nombreResponse.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        nombreResponse.Description ?? MsgNombreError,
                        nombreResponse.Code ?? -1,
                        new CrConsultaFianzasConsultaData());
                }

                decimal totalSaldos = lista.Sum(item => item.saldo);
                decimal totalCuotas = lista.Sum(item => item.cuota);

                CrConsultaFianzasConsultaData data = new()
                {
                    cedula = cedula,
                    nombre = nombreResponse.Result ?? string.Empty,
                    tipo = tipo,
                    titulo_resumen = ObtenerTituloResumen(tipo),
                    titulo_lista = ObtenerTituloLista(tipo),
                    subtitulo = ConstruirSubtitulo(tipo, lista.Count, totalSaldos),
                    total_casos = lista.Count,
                    total_saldos = totalSaldos,
                    total_cuotas = totalCuotas,
                    lista = lista
                };

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    $"Ocurri&oacute; un error al consultar fianzas: {ex.Message}",
                    -1,
                    new CrConsultaFianzasConsultaData());
            }
        }

        public ErrorDto<CrConsultaFianzasDetalleData> CrConsultaFianzas_Detalle_Obtener(
            int codEmpresa,
            CrConsultaFianzasDetalleRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cedula_deudor))
            {
                return DbHelper.CreateErrorResponse(
                    MsgCedulaRequerida,
                    -2,
                    new CrConsultaFianzasDetalleData());
            }

            if (request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MsgOperacionRequerida,
                    -2,
                    new CrConsultaFianzasDetalleData());
            }

            try
            {
                string cedulaDeudor = request.cedula_deudor.Trim();

                ErrorDto<CrConsultaFianzasEstadoDeudorQueryDto?> estadoResponse =
                    DbHelper.ExecuteSingleQuery<CrConsultaFianzasEstadoDeudorQueryDto>(
                        _portalDb,
                        codEmpresa,
                        @"
                        select
                            S.cedula,
                            S.fechaIngreso,
                            S.estadoActual,
                            isnull(max(R.proceso), 'N') as proceso,
                            isnull(count(R.id_solicitud), 0) as operaciones,
                            isnull(sum(R.saldo), 0) as saldos,
                            isnull(sum(R.cuota), 0) as cuotas,
                            isnull(dbo.fxCRDClasificacion(S.cedula, getdate()), '') as clasificacion
                        from Socios S
                        left join Reg_Creditos R
                            on S.cedula = R.cedula
                           and R.estado = 'A'
                           and R.saldo > 0
                        where S.cedula = @Cedula
                        group by S.cedula, S.fechaIngreso, S.estadoActual",
                        new CrConsultaFianzasEstadoDeudorQueryDto(),
                        new { Cedula = cedulaDeudor });

                if (estadoResponse.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        estadoResponse.Description ?? MsgEstadoDeudorError,
                        estadoResponse.Code ?? -1,
                        new CrConsultaFianzasDetalleData());
                }

                ErrorDto<List<CrConsultaFianzasMoraDto>> moraResponse =
                    DbHelper.ExecuteListQuery<CrConsultaFianzasMoraDto>(
                        _portalDb,
                        codEmpresa,
                        @"exec spCrd_Operacion_Mora_Cuotas @Operacion",
                        new { Operacion = request.operacion });

                if (moraResponse.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        moraResponse.Description ?? MsgMoraError,
                        moraResponse.Code ?? -1,
                        new CrConsultaFianzasDetalleData());
                }

                CrConsultaFianzasEstadoDeudorQueryDto estadoQuery =
                    estadoResponse.Result ?? new CrConsultaFianzasEstadoDeudorQueryDto();

                CrConsultaFianzasDetalleData data = new()
                {
                    estado_deudor = MapearEstadoDeudor(estadoQuery, cedulaDeudor),
                    mora = moraResponse.Result ?? new List<CrConsultaFianzasMoraDto>()
                };

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    $"Ocurri&oacute; un error al consultar el detalle: {ex.Message}",
                    -1,
                    new CrConsultaFianzasDetalleData());
            }
        }

        private static List<CrConsultaFianzasItemDto> PrepararLista(List<CrConsultaFianzasItemDto>? items)
        {
            List<CrConsultaFianzasItemDto> lista = items ?? new List<CrConsultaFianzasItemDto>();

            foreach (CrConsultaFianzasItemDto item in lista)
            {
                item.resaltar = DebeResaltarRegistro(item.moracta, item.proceso);
                item.mora_desc = $"[{item.moracta}] {item.moramnt:N2}";
            }

            return lista;
        }

        private static CrConsultaFianzasEstadoDeudorDto MapearEstadoDeudor(
            CrConsultaFianzasEstadoDeudorQueryDto estadoQuery,
            string cedulaDeudor)
        {
            string clasificacion = estadoQuery.clasificacion ?? string.Empty;
            string proceso = estadoQuery.proceso ?? ProcesoNormal;

            return new CrConsultaFianzasEstadoDeudorDto
            {
                cedula = string.IsNullOrWhiteSpace(estadoQuery.cedula)
                    ? cedulaDeudor
                    : estadoQuery.cedula,
                clasificacion = clasificacion,
                categoria = $"Categor&iacute;a : {clasificacion}",
                membresia = ObtenerMembresia(estadoQuery.estadoactual, estadoQuery.fechaingreso),
                cuotas = estadoQuery.cuotas,
                saldos = estadoQuery.saldos,
                operaciones = estadoQuery.operaciones,
                proceso = proceso,
                resaltar_categoria = DebeResaltarCategoria(clasificacion, proceso)
            };
        }

        private static bool DebeResaltarRegistro(int moraCta, string? proceso)
        {
            return moraCta > 0 ||
                   !string.Equals(proceso, ProcesoNormal, StringComparison.OrdinalIgnoreCase);
        }

        private static bool DebeResaltarCategoria(string? clasificacion, string? proceso)
        {
            bool clasificacionMayorAB = string.Compare(
                clasificacion ?? string.Empty,
                "B",
                StringComparison.OrdinalIgnoreCase) > 0;

            return clasificacionMayorAB ||
                   !string.Equals(proceso, ProcesoNormal, StringComparison.OrdinalIgnoreCase);
        }

        private static string ConstruirSubtitulo(string tipo, int totalCasos, decimal totalSaldos)
        {
            string tituloLista = ObtenerTituloLista(tipo);
            return $"{tituloLista} [ Casos: {totalCasos}   Saldos: {totalSaldos:N2} ]";
        }

        private static string NormalizarTipo(string? tipo)
        {
            string valor = (tipo ?? TipoTodos).Trim().ToUpperInvariant();

            return valor switch
            {
                TipoFianzas => TipoFianzas,
                TipoTraslados => TipoTraslados,
                _ => TipoTodos
            };
        }

        private static string ObtenerTituloResumen(string tipo)
        {
            return tipo switch
            {
                TipoFianzas => "Resumen de Fianzas",
                TipoTraslados => "Resumen de Traslados",
                _ => "Resumen General"
            };
        }

        private static string ObtenerTituloLista(string tipo)
        {
            return tipo switch
            {
                TipoFianzas => "Listado de Fianzas",
                TipoTraslados => "Listado de Traslados",
                _ => "Listado General"
            };
        }

        private static string ObtenerMembresia(string? estadoActual, DateTime? fechaIngreso)
        {
            if (!string.Equals(estadoActual, EstadoAsociado, StringComparison.OrdinalIgnoreCase))
            {
                return "Esta persona no es Asociado";
            }

            if (!fechaIngreso.HasValue)
            {
                return string.Empty;
            }

            return MCredito.fxMembresia(fechaIngreso.Value);
        }
    }
}