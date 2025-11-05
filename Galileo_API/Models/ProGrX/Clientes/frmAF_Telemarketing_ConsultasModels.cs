namespace PgxAPI.Models.ProGrX.Clientes
{
    public class ColocacionFiltros
    {
        public int fechaTipo { get; set; }
        public bool chkFechas { get; set; }
        public DateTime fechaInicio { get; set; }
        public DateTime fechaCorte { get; set; }
        public string? categoria { get; set; }
        public string? credito { get; set; }
        public string? destino { get; set; }
        public string? institucion { get; set; }
        public string? canal { get; set; }
        public string? producto { get; set; }
        public string? gyp { get; set; }
        public List<DropDownListaGenericaModel>? validaciones { get; set; }
        public int? mFecUltMovUpdate { get; set; }
    }

    public class AfTelemarketingColocacionData
    {
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public string? apellido1 { get; set; }
        public string? apellido2 { get; set; }
        public string? email { get; set; }
        public string? movil { get; set; }
        public string? tel_Hab { get; set; }
        public string? provincia { get; set; }
        public string? canton { get; set; }
        public string? linea { get; set; }
        public string? destino { get; set; }
        public string? actividad { get; set; }
        public string? canal { get; set; }
        public decimal monto { get; set; }
        public int plazo { get; set; }
        public string? institucion { get; set; }
        public string? departamento { get; set; }
        public DateTime? ultimo_Mov { get; set; }
        public DateTime? fechaForp { get; set; }
        public DateTime? fecha_Termina { get; set; }
        public string? ejecutivo { get; set; }
        public string? categoria { get; set; }
    }

    public class ClientesFiltros
    {
        public List<DropDownListaGenericaModel>? lineas { get; set; }
        public List<DropDownListaGenericaModel>? codigos { get; set; }
        public bool? chkAnalisis { get; set; }
        public string? usuario { get; set; }
    }

    public class AfTelemarketingClientesData
    {
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public string? apellido1 { get; set; }
        public string? apellido2 { get; set; }
        public string? email { get; set; }
        public string? movil { get; set; }
        public string? tel_Hab { get; set; }
        public string? provincia { get; set; }
        public string? canton { get; set; }
        public string? institucion { get; set; }
    }

    public class AfTelemarketingClientesDetalleData
    {
        public int id_Solicitud { get; set; }
        public string? codigo { get; set; }
        public string? linea { get; set; }
        public string? destino { get; set; }
        public string? actividad { get; set; }
        public string? canal { get; set; }
        public decimal monto { get; set; }
        public int plazo { get; set; }
        public decimal tasa { get; set; }
        public decimal cuota { get; set; }
        public decimal saldo { get; set; }
        public string? institucion { get; set; }
        public DateTime? ultimo_Mov { get; set; }
        public DateTime? fechaForp { get; set; }
        public DateTime? fecha_Termina { get; set; }
        public string? ejecutivo { get; set; }
    }

    public class ContactosFiltros
    {
        public string? fechaTipo { get; set; }
        public DateTime? fechaInicio { get; set; }
        public DateTime? fechaCorte { get; set; }
        public string? estado { get; set; }
        public List<DropDownListaGenericaModel>? valida { get; set; }
        public int mensajeTipo { get; set; }
        public string? mensaje { get; set; }
    }

    public class AfTelemarketingContactoData
    {
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public string? apellido1 { get; set; }
        public string? apellido2 { get; set; }
        public string? email { get; set; }
        public string? email_02 { get; set; }
        public string? movil { get; set; }
        public string? tel_Hab { get; set; }
        public string? provincia { get; set; }
        public string? canton { get; set; }
        public string? institucion { get; set; }
    }
}