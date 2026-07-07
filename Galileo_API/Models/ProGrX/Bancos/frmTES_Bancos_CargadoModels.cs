namespace Galileo.Models.ProGrX.Bancos
{
    public class TesBancoCargadoConceptos
    {
        public string? cod_concepto { get; set; }
        public string? descripcion { get; set; }
        public string? cod_cuenta_mask { get; set; }
        public int dp_tramite_apl { get; set; }
        public string? cuenta_desc { get; set; }
    }

    public class TesCargadoExcelDto
    {
        public Nullable<DateTime> fecha { get; set; }
        public string? tipo { get; set; }
        public string? documento { get; set; }
        public decimal? importe { get; set; }
        public string? descripcion { get; set; }
        public decimal? saldo { get; set; }
    }

    public class DropDownListaBancosCargados
    {
        public string idx { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class TeslistaRegistroBancosDto
    {
        public int? id_linea { get; set; }
        public string estado { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public decimal importe { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string? cod_concepto { get; set; }
        public string? concepto_desc { get; set; }
        public string? cod_cuenta { get; set; }
        public string? cod_unidad { get; set; }
        public string? estado_desc { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? procesado_fecha { get; set; }
        public string procesado_usuario { get; set; } = string.Empty;
        public string concilia_nsolicitud { get; set; } = string.Empty;
        public int? auto_Registro_id { get; set; }
        public string dp_tramite_id { get; set; } = string.Empty;
    }

    public class TesFiltrosRegistroBancoDto
    {
        public string? base_ { get; set; }
        public string ndocumento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal montoInicio { get; set; }
        public DateTime fechaInicio { get; set; }
        public decimal montoCorte { get; set; }
        public DateTime fechaCorte { get; set; }
        public string? tipoMovimiento { get; set; }
        public string? estado { get; set; }
        public string cod_cuenta { get; set; } = string.Empty;

    }

    public class RegistroBancoDto
    {
        public int Linea_Id { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public int Auto_Id { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public string Centro { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public short DP_Tramite_Apl { get; set; }
    }

    public class TesBancosCargadoReclasificaConceptoModel
    {
        public List<int> Solicitudes  { get; set; } = new();
        public string CodConcepto     { get; set; } = string.Empty;
        public string? Usuario        { get; set; }
        public string? Nota           { get; set; }
    }

    public class TesBancosCargadoDetalleExcluirModel
    {
        public List<long> LineasId { get; set; } = new();
        public string? Usuario     { get; set; }
    }

    public class TesBancosCargadoDetalleRegistrarModel
    {
        public List<long> LineasId { get; set; } = new();
        public string Concepto     { get; set; } = string.Empty;
        public string Unidad       { get; set; } = string.Empty;
        public string Centro       { get; set; } = string.Empty;
        public string Cuenta       { get; set; } = string.Empty;
        public string? Usuario     { get; set; }
    }

    public class TesFiltrosDetalleMovimientoDto
    {
        public string BancoId        { get; set; } = string.Empty;
        public string Documento      { get; set; } = string.Empty;
        public string Tipo           { get; set; } = string.Empty;
        public string FechaTipo      { get; set; } = "Doc";
        public DateTime FechaInicio  { get; set; }
        public DateTime FechaCorte   { get; set; }
        public decimal MontoInicio   { get; set; }
        public decimal MontoCorte    { get; set; }
        public string Estado         { get; set; } = string.Empty;
        public string Descripcion    { get; set; } = string.Empty;
        public string CodConcepto    { get; set; } = string.Empty;
        public string CodCuenta      { get; set; } = string.Empty;
        public string CodUnidad      { get; set; } = string.Empty;
        public string CodCentroCosto { get; set; } = string.Empty;
    }
}