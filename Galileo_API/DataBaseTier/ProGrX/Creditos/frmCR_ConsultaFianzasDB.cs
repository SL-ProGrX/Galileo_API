using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrConsultaFianzasDb
    {
        private const string MsgCedulaRequerida = "Debe indicar la c&eacute;dula.";
        private const string MsgOperacionRequerida = "Debe indicar la operaci&oacute;n.";
        private const string TipoTodos = "X";
        private const string TipoFianzas = "F";
        private const string TipoTraslados = "T";

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
                return DbHelper.CreateErrorResponse(MsgCedulaRequerida, -2, new CrConsultaFianzasConsultaData());
            }

            try
            {
                string tipo = NormalizarTipo(request.tipo);
                string estado = request.canceladas ? "C" : "A";

                var listaResponse = DbHelper.ExecuteQuery<CrConsultaFianzasItemDto>(
                    _portalDb,
                    codEmpresa,
                    @"exec spCrd_Consulta_Fianzas_Rsm @Cedula, @Estado, @Tipo",
                    new
                    {
                        Cedula = request.cedula.Trim(),
                        Estado = estado,
                        Tipo = tipo
                    });

                if (listaResponse.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        listaResponse.Description ?? "No fue posible consultar las fianzas.",
                        listaResponse.Code ?? -1,
                        new CrConsultaFianzasConsultaData());
                }

                List<CrConsultaFianzasItemDto> lista = listaResponse.Result ?? new List<CrConsultaFianzasItemDto>();

                foreach (CrConsultaFianzasItemDto item in lista)
                {
                    item.resaltar = item.moracta > 0 ||
                                     !string.Equals(item.proceso, "N", StringComparison.OrdinalIgnoreCase);
                    item.mora_desc = $"[{item.moracta}] {item.moramnt:N2}";
                }

                var nombreResponse = DbHelper.ExecuteSingleQuery<string>(
                    _portalDb,
                    codEmpresa,
                    "select isnull(dbo.fxNombre(@Cedula), '')",
                    string.Empty,
                    new { Cedula = request.cedula.Trim() });

                if (nombreResponse.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        nombreResponse.Description ?? "No fue posible obtener el nombre de la persona.",
                        nombreResponse.Code ?? -1,
                        new CrConsultaFianzasConsultaData());
                }

                string tituloResumen = ObtenerTituloResumen(tipo);
                string tituloLista = ObtenerTituloLista(tipo);
                decimal totalSaldos = lista.Sum(x => x.saldo);
                decimal totalCuotas = lista.Sum(x => x.cuota);

                CrConsultaFianzasConsultaData data = new()
                {
                    cedula = request.cedula.Trim(),
                    nombre = nombreResponse.Result ?? string.Empty,
                    tipo = tipo,
                    titulo_resumen = tituloResumen,
                    titulo_lista = tituloLista,
                    subtitulo = $"{tituloLista} [ Casos: {lista.Count}   Saldos: {totalSaldos:N2} ]",
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
                return DbHelper.CreateErrorResponse(MsgCedulaRequerida, -2, new CrConsultaFianzasDetalleData());
            }

            if (request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse(MsgOperacionRequerida, -2, new CrConsultaFianzasDetalleData());
            }

            try
            {
                var estadoResponse = DbHelper.ExecuteSingleQuery<CrConsultaFianzasEstadoDeudorQueryDto>(
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
                    new { Cedula = request.cedula_deudor.Trim() });

                if (estadoResponse.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        estadoResponse.Description ?? "No fue posible consultar el estado del deudor.",
                        estadoResponse.Code ?? -1,
                        new CrConsultaFianzasDetalleData());
                }

                var moraResponse = DbHelper.ExecuteQuery<CrConsultaFianzasMoraDto>(
                    _portalDb,
                    codEmpresa,
                    @"exec spCrd_Operacion_Mora_Cuotas @Operacion",
                    new { Operacion = request.operacion });

                if (moraResponse.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        moraResponse.Description ?? "No fue posible consultar la mora de la operaci&oacute;n.",
                        moraResponse.Code ?? -1,
                        new CrConsultaFianzasDetalleData());
                }

                CrConsultaFianzasEstadoDeudorQueryDto estadoQuery =
                    estadoResponse.Result ?? new CrConsultaFianzasEstadoDeudorQueryDto();

                CrConsultaFianzasEstadoDeudorDto estado = new()
                {
                    cedula = string.IsNullOrWhiteSpace(estadoQuery.cedula)
                        ? request.cedula_deudor.Trim()
                        : estadoQuery.cedula,
                    clasificacion = estadoQuery.clasificacion ?? string.Empty,
                    categoria = $"Categor&iacute;a : {estadoQuery.clasificacion}",
                    membresia = ObtenerMembresia(estadoQuery.estadoactual, estadoQuery.fechaingreso),
                    cuotas = estadoQuery.cuotas,
                    saldos = estadoQuery.saldos,
                    operaciones = estadoQuery.operaciones,
                    proceso = estadoQuery.proceso ?? "N",
                    resaltar_categoria =
                        string.Compare(estadoQuery.clasificacion ?? string.Empty, "B", StringComparison.OrdinalIgnoreCase) > 0 ||
                        !string.Equals(estadoQuery.proceso, "N", StringComparison.OrdinalIgnoreCase)
                };

                CrConsultaFianzasDetalleData data = new()
                {
                    estado_deudor = estado,
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
            if (!string.Equals(estadoActual, "S", StringComparison.OrdinalIgnoreCase))
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