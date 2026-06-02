using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Microsoft.VisualBasic;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAHMovimientosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrx;

        public FrmAHMovimientosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrx = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene los catálogos y fechas iniciales requeridos por la consulta de movimientos a patrimonio.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Filtros iniciales del formulario.</returns>
        public ErrorDto<MovimientosPatrimonioFiltrosDto?> AH_Movimientos_Filtros_Obtener(int codEmpresa)
        {
            try
            {
                var fechaServidor = _mProGrx.fxFechaServidor(codEmpresa, 0);

                const string sqlDocumentos = @"
select
    'TODOS' as idx,
    'TODOS' as itmx
union all
select
    rtrim(Tipo_Documento) as idx,
    rtrim(Descripcion) as itmx
from sif_documentos
where Tipo_Documento in ('ND', 'NC', 'RE', 'LIQ', 'RLIQ', 'PLA', 'ING', 'CAJA', 'CAJARE')
order by itmx;";

                var documentosResponse = DbHelper.ExecuteListQuery<DocumentosTransaccionSifDto>(
                    _portalDb,
                    codEmpresa,
                    sqlDocumentos);

                if (documentosResponse.Code == -1 || documentosResponse.Result == null)
                    return DbHelper.CreateErrorResponse<MovimientosPatrimonioFiltrosDto?>(documentosResponse.Description ?? "No fue posible cargar los tipos de documento.");

                var response = new MovimientosPatrimonioFiltrosDto
                {
                    fecha_inicio = fechaServidor.Date.AddDays(-7),
                    fecha_corte = fechaServidor.Date,
                    tipos_aporte = CrearTiposAporte(),
                    tipos_documento = documentosResponse.Result
                };

                return DbHelper.CreateOkResponse<MovimientosPatrimonioFiltrosDto?>(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<MovimientosPatrimonioFiltrosDto?>(ex.Message);
            }
        }

        /// <summary>
        /// Consulta los movimientos a patrimonio según el rango de fechas y filtros indicados.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="request">Filtros de la consulta.</param>
        /// <returns>Lista de movimientos encontrados.</returns>
        public ErrorDto<List<MovimientosPatrimonioDto>> AH_Movimientos_Consulta_Obtener(
            int codEmpresa,
            MovimientosPatrimonioConsultaRequest request)
        {
            if (request == null)
                return DbHelper.CreateErrorResponse<List<MovimientosPatrimonioDto>>("La solicitud es requerida.");

            if (request.fecha_inicio == default)
                return DbHelper.CreateErrorResponse<List<MovimientosPatrimonioDto>>("La fecha inicio es requerida.");

            if (request.fecha_corte == default)
                return DbHelper.CreateErrorResponse<List<MovimientosPatrimonioDto>>("La fecha corte es requerida.");

            if (request.fecha_inicio.Date > request.fecha_corte.Date)
            {
                return DbHelper.CreateErrorResponse<List<MovimientosPatrimonioDto>>(
                    "La fecha inicio no puede ser mayor que la fecha corte.",
                    -2);
            }

            var tiposAporte = ObtenerTiposAporteConsulta(request.tipo_aporte);
            if (tiposAporte.Count == 0)
            {
                return DbHelper.CreateErrorResponse<List<MovimientosPatrimonioDto>>(
                    "El tipo de aporte indicado no es válido.",
                    -2);
            }

            const string sql = @"
select
    rtrim(isnull(convert(varchar(50), D.Id_seq), '')) as id_seq,
    rtrim(isnull(D.Tipo_Aporte, '')) as tipo_aporte,
    rtrim(isnull(D.Tipo_Aporte_Id, '')) as tipo_aporte_id,
    rtrim(isnull(D.Cedula, '')) as cedula,
    rtrim(isnull(D.Nombre, '')) as nombre,
    isnull(D.Monto, 0) as monto,
    D.Fecha as fecha,
    rtrim(isnull(D.Usuario, '')) as usuario,
    rtrim(isnull(D.Concepto, '')) as concepto,
    rtrim(isnull(D.Tipo, '')) as tipo,
    rtrim(isnull(D.TCon, '')) as tcon,
    rtrim(isnull(convert(varchar(50), D.NCon), '')) as ncon,
    rtrim(isnull(convert(varchar(50), D.Cod_Caja), '')) as cod_caja,
    isnull(D.FechaProc, 0) as fechaproc,
    rtrim(isnull(convert(varchar(50), D.Cod_Institucion), '')) as cod_institucion,
    rtrim(isnull(D.Institucion, '')) as institucion,
    rtrim(isnull(Sec.Descripcion, '')) as sectordesc
from vSIF_CtrlDoc_Pat_Detalle D
inner join Socios S
    on D.Cedula = S.CEDULA
left join AFI_SECTORES Sec
    on S.COD_SECTOR = Sec.COD_SECTOR
where 
  D.Tipo_Aporte_Id in @TiposAporte and
   D.Fecha >= @FechaInicio
  and D.Fecha < dateadd(day, 1, @FechaCorte)
  and (@TipoDocumento = '' or @TipoDocumento = 'TODOS' or D.TCon = @TipoDocumento)
  and (@Documento = '' or convert(varchar(50), D.NCon) like @DocumentoLike)
  and (@Cedula = '' or D.Cedula like @CedulaLike)
order by D.Fecha desc, D.Id_seq desc;";

            string fechInicio = MProGrXAuxiliarDB.validaFechaGlobal(request!.fecha_inicio, "yyyy-MM-dd");
            string fechFin = MProGrXAuxiliarDB.validaFechaGlobal(request!.fecha_corte, "yyyy-MM-dd");

            

            var parametros = new
            {
                TiposAporte = tiposAporte,
                FechaInicio = fechInicio,
                FechaCorte = fechFin,
                TipoDocumento = NormalizarTexto(request.tipo_documento),
                Documento = NormalizarTexto(request.tipo_documento),
                DocumentoLike = $"{NormalizarTexto(request.documento)}%",
                Cedula = NormalizarTexto(request.cedula),
                CedulaLike = $"{NormalizarTexto(request.cedula)}%"
            };

            return DbHelper.ExecuteListQuery<MovimientosPatrimonioDto>(
                _portalDb,
                codEmpresa,
                sql,
                parametros);
        }

        private static List<TipoAportePatrimonioDto> CrearTiposAporte()
        {
            return
            [
                new TipoAportePatrimonioDto { id = "TODOS", descripcion = "[TODOS]" },
                new TipoAportePatrimonioDto { id = "O", descripcion = "Obrero" },
                new TipoAportePatrimonioDto { id = "P", descripcion = "Patronal" },
                new TipoAportePatrimonioDto { id = "X", descripcion = "Custodia" },
                new TipoAportePatrimonioDto { id = "C", descripcion = "Capitalizado" }
            ];
        }

        private static List<string> ObtenerTiposAporteConsulta(string? tipoAporte)
        {
            var tipoNormalizado = NormalizarTexto(tipoAporte).ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(tipoNormalizado) || tipoNormalizado == "TODOS")
                return ["O", "P", "C", "E", "X"];

            if (tipoNormalizado == "OBRERO")
                return ["O"];

            if (tipoNormalizado == "PATRONAL")
                return ["P"];

            if (tipoNormalizado == "CUSTODIA")
                return ["X"];

            if (tipoNormalizado == "CAPITALIZADO" || tipoNormalizado == "CAPITALIZADA")
                return ["C"];

            return tipoNormalizado switch
            {
                "O" or "P" or "C" or "E" or "X" => [tipoNormalizado],
                _ => []
            };
        }

        private static string NormalizarTexto(string? valor)
            => string.IsNullOrWhiteSpace(valor) ? string.Empty : valor.Trim();
    }
}
