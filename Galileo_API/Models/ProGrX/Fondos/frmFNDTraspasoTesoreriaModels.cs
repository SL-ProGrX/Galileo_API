using Org.BouncyCastle.Ocsp;

namespace Galileo.Models.ProGrX.Fondos
{
    public class TesTokenConsultaParams
    {
        public required int CodEmpresa { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Estado { get; set; } = "A";
        public string Usuario { get; set; } = string.Empty;
        public int Top { get; set; } = 20;
    }

    public class TesTokenConsultaResult
    {
        public string Id_Token { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public DateTime? Fecha_Registra { get; set; }
        public DateTime? Fecha_Uso { get; set; }
        public string ItmX { get; set; } = string.Empty;
        public string IdX { get; set; } = string.Empty;
    }

    public class FndTraspasoTesoreriaFiltroParams
    {
        public required int CodEmpresa { get; set; }
        public required DateTime FechaDesde { get; set; }
        public required DateTime FechaHasta { get; set; }
        public string Estado { get; set; } = "P"; // "P" = Pendiente, otro = Generado
    }

    public class TesTokenNewParams
    {
        public required int CodEmpresa { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class TesTokenNewResult
    {
        public string Id_Token { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class FndTraspasoTesoreriaFixResult
    {
        public bool Success { get; set; }
    }

    public class FndTraspasoTesoreriaLiquidacionConsultaParams
    {
        public required int CodEmpresa { get; set; }
        public required DateTime FechaDesde { get; set; }
        public required DateTime FechaHasta { get; set; }
        public required bool Todos { get; set; }
        public string? SifParam { get; set; }
        public string Estado { get; set; } = "P"; // "P" = Pendiente, otro = Generado
        public required bool Filtros { get; set; }
        public int? BancoId { get; set; }
        public string? Oficina { get; set; }
        public string? Usuario { get; set; }
        public string? Sistema { get; set; }
        public string? TokenConsulta { get; set; }
        public string? AppProductName { get; set; }
    }

    public class FndTraspasoTesoreriaLiquidacionConsultaResult
    {
        public bool Valor { get; set; }
        public int Consec { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Cod_Plan { get; set; } = string.Empty;
        public int Cod_Contrato { get; set; }
        public decimal Total_Girar { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Oficina { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Cta_Ahorros { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public int Duplicado { get; set; }
        public DateTime? Tes_Supervision_Fecha { get; set; }
        public decimal? Pago_Tercero_Apl { get; set; }
        public string? Pago_Tercero_Tipo { get; set; }
        public string? Pago_Tercero_Id { get; set; }
        public string? Pago_Tercero_Nombre { get; set; }
        public string? Id_Token { get; set; }
    }

    public class FndTraspasoTesoreriaDuplicadosParams
    {
        public required int CodEmpresa { get; set; }
        public required DateTime FechaDesde { get; set; }
        public required DateTime FechaHasta { get; set; }
        public string? SifParam { get; set; }
        public string Estado { get; set; } = "P";
        public required bool Filtros { get; set; }
        public int? BancoId { get; set; }
        public string? Oficina { get; set; }
        public string? Usuario { get; set; }
        public string? Sistema { get; set; }
        public string? TokenConsulta { get; set; }
        public string? AppProductName { get; set; }
    }

    public class FndTraspasoTesoreriaDuplicadosResult
    {
        public int Liquidaciones { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Cta_Ahorros { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Total_Girar { get; set; }
    }

    public class FndRetLiqTesoreriaParams
    {
        public required int CodEmpresa { get; set; }
        public required int LiqNum { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class FndTraspasoTesoreriaUpdateParams
    {
        public required int CodEmpresa { get; set; }
        public required int Consec { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string RetencionCodigo { get; set; } = string.Empty;
    }

    public class FndRetLiqTesoreriaUnificadoParams
    {
        public required int CodEmpresa { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string IdToken { get; set; } = string.Empty;
        public required DateTime FechaDesde { get; set; }
        public required DateTime FechaHasta { get; set; }
    }

    public class FndTraspasoTesoreriaDetalleParams
    {
        public required int CodEmpresa { get; set; }
        public required DateTime FechaDesde { get; set; }
        public required DateTime FechaHasta { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string? SifParam { get; set; }
        public string Estado { get; set; } = "P";
        public required bool Filtros { get; set; }
        public int? BancoId { get; set; }
        public string? Oficina { get; set; }
        public string? Usuario { get; set; }
        public string? Sistema { get; set; }
        public string? TokenConsulta { get; set; }
        public string? AppProductName { get; set; }
    }

    public sealed class FndTraspasoTesoreriaProcesarLoteRequest
    {
        public required int CodEmpresa { get; set; }
        public List<int> Consecutivos { get; set; } = new();
        public string Usuario { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? RetencionCodigo { get; set; }
    }

    public sealed class FndTraspasoTesoreriaProcesoError
    {
        public int Consec { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public sealed class FndTraspasoTesoreriaProcesarLoteResult
    {
        public int Procesados { get; set; }
        public int ConErrores { get; set; }
        public List<FndTraspasoTesoreriaProcesoError> Errores { get; set; } = new();
    }

    public sealed class FndTraspasoTesoreriaProcesoIniciarRequest
    {
        public Guid ProcesoId { get; set; } =  Guid.NewGuid();
        public string Modo { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? RetencionCodigo { get; set; }
        public DateTime? FechaDesde { get; set; } 
        public DateTime? FechaHasta { get; set; }
        public bool AplicaRevision { get; set; } = false;
        public int? BancoId { get; set; }
        public string? Oficina { get; set; }
        public string? UsuarioFiltro { get; set; }
        public string? SistemaFiltro { get; set; }
        public string? TokenFiltro { get; set; }
        public string AppProductName { get; set; } = "ProGrX";
        public List<int> Consecutivos { get; set; } = new();
        public List<string> Cedulas { get; set; } = new();

        [System.Text.Json.Serialization.JsonIgnore]
        public string Usuario { get; set; } = string.Empty;
    }

    public sealed class FndTraspasoTesoreriaProcesoContinuarRequest
    {
        public Guid ProcesoId { get; set; } = Guid.NewGuid();
        public bool? ReintentarErrores { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string Usuario { get; set; } = string.Empty;
    }

    public sealed class FndTraspasoTesoreriaProcesoErrorResult
    {
        public int Consec { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public sealed class FndTraspasoTesoreriaProcesoResult
    {
        public Guid ProcesoId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public bool ProcesoRecuperado { get; set; }
        public string UsuarioOrigen { get; set; } = string.Empty;
        public int TotalRegistros { get; set; }
        public int Procesados { get; set; }
        public int ConErrores { get; set; }
        public int Pendientes { get; set; }
        public decimal Porcentaje { get; set; }
        public long TiempoTranscurridoMs { get; set; }
        public bool ProcesoFinalizado { get; set; }
        public string? ErrorMensaje { get; set; }
        public List<FndTraspasoTesoreriaProcesoErrorResult> Errores { get; set; } = new();
    }

    /// <summary>
    /// Parámetros para consultar la lista de remesas de traspaso a tesorería.
    /// </summary>
    public sealed class FndTraspasoTesoreriaRemesaParams
    {
        public required int CodEmpresa { get; set; }
        public int Top { get; set; } = 15;
    }

    /// <summary>
    /// Resultado de la consulta de remesas de traspaso a tesorería.
    /// </summary>
    public sealed class FndTraspasoTesoreriaRemesaResult
    {
        public int Cod_Remesa { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime? Fecha_Inicio { get; set; }
        public DateTime? Fecha_Corte { get; set; }
        public int Casos { get; set; }
        public decimal Monto { get; set; }
        public string Notas { get; set; } = string.Empty;
    }
}
