namespace Galileo.Models.TES
{
    public class FiltrosBusqueda
    {
        public List<DropDownListaGenericaModel<string>>? Bancos { get; set; }
        public List<DropDownListaGenericaModel<int>>? Cuentas { get; set; }
        public List<DropDownListaGenericaModel<string>>? Conceptos { get; set; }
        public List<DropDownListaGenericaModel<string>>? TiposDocumento { get; set; }
        public string? Estado { get; set; }
        public string? TipoFecha { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaCorte { get; set; }
        public string? Codigo { get; set; }
        public string? NoDocumento { get; set; }
        public string? Transferencia { get; set; }
        public string? IdAplicacion { get; set; }
        public string? Beneficiario { get; set; }
        public string? Detalle { get; set; }
        public string? Ref01 { get; set; }
        public string? Ref02 { get; set; }
        public string? Ref03 { get; set; }
        public bool ChkProtegido { get; set; }
        public string? Usuario { get; set; }
        public string? TipoUsuario { get; set; }
        //Apuntando a tabla
        public string? filtro { get; set; } //filtro del buscar en tablas o buscador
        public int? pagina { get; set; } = 1;//pagina de la tabla
        public int? paginacion { get; set; } = 30; //paginacion de la tabla
        public int? sortOrder { get; set; } = 0; //0: sin orden, 1: ascendente, 2: descendente
        public string? sortField { get; set; } //campo por el cual se ordena
    }

    public class Desembolsos
    {
        public int seleccionado { get; set; } = 0; // Corresponde al 0 inicial
        public int nsolicitud { get; set; }
        public string? doc_interno { get; set; }
        public string? doc_banco { get; set; }
        public string? tipo { get; set; }
        public decimal monto { get; set; }
        public string? estado { get; set; }
        public DateTime? fecha_emision { get; set; }
        public DateTime? fecha_anula { get; set; }
        public string? beneficiario { get; set; }
        public string? cta_ahorros { get; set; }
        public string? banco { get; set; }
        public string? codigo { get; set; }
        public string? detalle { get; set; }
        public string? ref_banco { get; set; }
        public string? unidad { get; set; }
        public string? concepto { get; set; }
        public string? tipo_beneficio { get; set; }
        public string? user_solicita { get; set; }
        public string? user_genera { get; set; }
        public string? user_anula { get; set; }
        public string? cod_divisa { get; set; }
        public string? tipo_cliente { get; set; }
        public decimal tipo_cambio { get; set; }
        public string? grupo_bancario { get; set; }
        public string? periodo { get; set; }
        public string? ref_01 { get; set; }
        public string? ref_02 { get; set; }
        public string? ref_03 { get; set; }
        public int id_desembolso { get; set; }
        public string? referencia_sinpe { get; set; }
        public string? nombre_origen { get; set; }
        public string? user_autoriza { get; set; }
        public DateTime? fecha_autoriza { get; set; }
    }

    public class DesembolsosLista
    {
        public DesembolsoTotales? totales { get; set; }
        public List<Desembolsos>? lista { get; set; }
    }

    public class DesembolsoTotales
    {
        public int total { get; set; }
        public decimal montototal { get; set; }
    }
}