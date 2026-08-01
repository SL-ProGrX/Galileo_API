using Dapper;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using System.Data;
using System.Xml.Linq;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public partial class FrmTesBancosCargadoDB
    {
        /// <summary>
        /// Obtiene los depósitos para el tab Revisión de Movimientos.
        /// </summary>
        public ErrorDto<List<TesBancosCargadoRevMovDto>>
            TES_BancosCargado_RevMov_Obtener(
                int CodEmpresa,
                TesBancosCargadoRevMovRequest request)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                var parameters = new
                {
                    FechaInicio = request.FechaInicio.Date,
                    FechaFin = request.FechaFin.Date,
                    NDocumento = NormalizarFiltro(request.NDocumento),
                    request.IdBanco,
                    Tipo = NormalizarFiltro(request.Tipo),
                    Estados = NormalizarFiltro(request.Estados),
                    EstadoSocio = NormalizarFiltro(request.EstadoSocio)
                };

                return connection.Query<TesBancosCargadoRevMovDto>(
                    "spTES_W_RevMovDepositos_Obtener",
                    parameters,
                    commandType: CommandType.StoredProcedure).ToList();
            });
        }

        /// <summary>
        /// Obtiene los movimientos bancarios candidatos para conciliación.
        /// </summary>
        public ErrorDto<List<TesBancosCargadoRevMovConciliaDto>>
            TES_BancosCargado_RevMovConcilia_Obtener(
                int CodEmpresa,
                TesBancosCargadoRevMovConciliaRequest request)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                var parameters = new
                {
                    Banco = NormalizarFiltro(request.Banco),
                    Concepto = NormalizarFiltro(request.Concepto),
                    TipoMov = NormalizarFiltro(request.TipoMov),
                    Documento = NormalizarFiltro(request.Documento),
                    request.MontoDesde,
                    request.MontoHasta,
                    FechaInicio = request.FechaInicio.Date,
                    FechaFin = request.FechaFin.Date,
                    Estado = NormalizarFiltro(request.Estado)
                };

                var lista = connection.Query<TesBancosCargadoRevMovConciliaDto>(
                    "spTES_W_RevMovDepositosConcilia_Obtener",
                    parameters,
                    commandType: CommandType.StoredProcedure).ToList();

                return lista;
            });
        }

        /// <summary>
        /// Asocia las solicitudes seleccionadas con un movimiento bancario.
        /// </summary>
        public ErrorDto TES_BancosCargado_RevMovConcilia_Aplicar(
            int CodEmpresa,
            TesBancosCargadoRevMovConciliaAplicarRequest request)
        {
            if (request.Solicitudes.Count == 0)
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar al menos una solicitud.");
            }

            var solicitudesXml = ConstruirSolicitudesConciliacionXml(
                request.Solicitudes);

            var resultado = DbHelper.WithConn(
                _portalDB,
                CodEmpresa,
                connection =>
                {
                    connection.Execute(
                        "spTES_W_RevMovDepositosConcilia_Aplicar",
                        new
                        {
                            IdLineaOrigen = request.MovimientoDestino.IdLinea,
                            SolicitudesXml = solicitudesXml
                        },
                        commandType: CommandType.StoredProcedure);

                    return true;
                });

            return resultado.Code == -1
                ? DbHelper.ErrorResponse(
                    resultado.Description
                        ?? "No fue posible aplicar la asociación.")
                : DbHelper.OkResponse(
                    "La asociación se aplicó correctamente.");
        }

        /// <summary>
        /// Construye el XML ordenado de las solicitudes que serán conciliadas.
        /// </summary>
        private static string ConstruirSolicitudesConciliacionXml(
            IEnumerable<TesBancosCargadoRevMovSolicitudAplicarRequest> solicitudes)
        {
            var documento = new XElement(
                "Solicitudes",
                solicitudes.Select(
                    (solicitud, indice) =>
                        new XElement(
                            "Solicitud",
                            new XAttribute("orden", indice + 1),
                            new XAttribute(
                                "numeroSolicitud",
                                solicitud.NumeroSolicitud),
                            new XAttribute("idBanco", solicitud.IdBanco),
                            new XAttribute("monto", solicitud.Monto))));

            return documento.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Convierte los filtros de texto vacíos en null para que los
        /// procedimientos almacenados los traten como opcionales.
        /// </summary>
        private static string? NormalizarFiltro(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? null
                : valor.Trim();
        }
    }
}
