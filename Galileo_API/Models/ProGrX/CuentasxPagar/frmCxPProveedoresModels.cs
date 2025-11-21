namespace Galileo.Models.CxP
{
    public class ProveedorDto
    {
        public int Cod_Proveedor { get; set; }
        public string Cod_Clasificacion { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Cod_Alter { get; set; } = string.Empty;
        public string Cedjur { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Observacion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Contacto_Ventas { get; set; } = string.Empty;
        public string Contacto_Compras { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Telefono_Ext { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public string Fax_Ext { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Aptopostal { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int Credito_Plazo { get; set; }
        public decimal Credito_Monto { get; set; }
        public decimal Descuento_Porc { get; set; }
        public decimal Saldo { get; set; }
        public DateTime Ultimo_Pago { get; set; }
        public DateTime Ultima_Compra { get; set; }
        public string Ultima_Factura { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Tipo_Pago { get; set; } = string.Empty;
        public int Cod_Banco { get; set; }
        public string Cuenta_Ahorros { get; set; } = string.Empty;
        public string Nit_Codigo { get; set; } = string.Empty;
        public string Nit_Nombre { get; set; } = string.Empty;
        public string? Fusion { get; set; }
        public string Cod_Divisa { get; set; } = string.Empty;
        public decimal Saldo_Divisa_Real { get; set; }
        public string Email_02 { get; set; } = string.Empty;
        public DateTime Suspende_Fecha { get; set; }
        public string Suspende_Usuario { get; set; } = string.Empty;
        public string TipoProv { get; set; } = string.Empty;
        public string CuentaConta { get; set; } = string.Empty;
        public string Banco_Desc { get; set; } = string.Empty;
        public bool Web_Auto_Gestion { get; set; } = false;
        public bool Web_Ferias { get; set; } = false;
        public Nullable<DateTime> registro_fecha { get; set; }
        public string registro_Usuario { get; set; } = string.Empty;
        public Nullable<DateTime> fecha_vencimiento { get; set; }
        public int plazo { get; set; }
        public int convenio { get; set; }
        public string? representante_legal { get; set; }
        public string? justificacion_estado { get; set; }
        public string? user_modifica { get; set; }
        public string? criticidad { get; set; }
    }

    public class ProveedorFusionLista
    {
        public int Total { get; set; }
        public List<ProveedorFusion> Fusiones { get; set; } = new List<ProveedorFusion>();
    }

    public class ProveedorFusion
    {
        public int Cod_Proveedor { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Fusion { get; set; } = string.Empty;
    }

    public class TipoProveedor
    {
        public string Cod_Clasificacion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CuentaDesembolso
    {
        public int Id_Banco { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Desc_Corta { get; set; } = string.Empty;
        public string Cta { get; set; } = string.Empty;
        public int IdX { get; set; }
        public string ItmX { get; set; } = string.Empty;
    }

    public class Cuenta
    {
        public string Banco { get; set; } = string.Empty;
        public string Tipo_Desc { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public string Cuenta_Interna { get; set; } = string.Empty;
        public string Cuenta_Interbanca { get; set; } = string.Empty;
        public int Activa { get; set; }
        public string Destino { get; set; } = string.Empty;
        public string Registro_Fecha { get; set; } = string.Empty;
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class Autorizacion
    {
        public string Datakey { get; set; } = string.Empty;
        public int Cod_Proveedor { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class TipoSuspension
    {
        public string Cod_Suspension { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CuentaDivisa
    {
        public string Cod_Divisa { get; set; } = string.Empty;
    }

    public class SuspensionLista
    {
        public int Total { get; set; }
        public List<Suspension> Suspensiones { get; set; } = new List<Suspension>();
    }

    public class Suspension
    {
        public int? Suspension_Id { get; set; }
        public int Cod_Proveedor { get; set; }
        public string Cod_Suspension { get; set; } = string.Empty;
        public string Suspension_Desc { get; set; } = string.Empty;
        public string Registro_Fecha { get; set; } = string.Empty;
        public string Registro_Usuario { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public int Activa { get; set; }
        public string Vencimiento { get; set; } = string.Empty;
        public string Reactiva_Fecha { get; set; } = string.Empty;
        public string Reactiva_Usuario { get; set; } = string.Empty;
        public string Reactiva_Notas { get; set; } = string.Empty;
    }

    public class ProveedorUsuariosListaDatos
    {
        public string? usuario { get; set; }
        public int cod_proveedor { get; set; }
        public string? nombre { get; set; }
        public string? movil { get; set; }
        public string? email { get; set; }
        public string? clave { get; set; }
        public string? clave_renueva { get; set; }
        public bool activo { get; set; }
        public bool web_auto_gestion { get; set; }
        public bool web_ferias { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public string? activo_desc { get; set; }
        public string? portal_desc { get; set; }
        public string? ferias_desc { get; set; }
    }

    public class ProveedorEventosListaDatos
    {
        public int cod_evento { get; set; }
        public string? descripcion { get; set; }
        public Nullable<DateTime> inicio { get; set; }
        public Nullable<DateTime> corte { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public bool asignado { get; set; }
    }

    public class BitacoraProveedorDto
    {
        public int id_bitacora { get; set; }
        public string cod_proveedor { get; set; } = string.Empty;
        public int consec { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }
}