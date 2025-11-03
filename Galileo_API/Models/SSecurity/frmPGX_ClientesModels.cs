namespace PgxAPI.Models
{
    public class ClientesDataLista
    {
        public int Total { get; set; }
        public List<ClienteDTO> Lista { get; set; } = new List<ClienteDTO>();
    }

    public class ClienteDTO
    {
        public int cod_empresa { get; set; }
        public string cod_vendedor { get; set; } = string.Empty;
        public string? nombre_largo { get; set; }
        public string? nombre_corto { get; set; }
        public byte[]? logo_cliente { get; set; }
        public string? estado { get; set; }
        public string? identificacion { get; set; }
        public string? email_01 { get; set; }
        public string? email_02 { get; set; }
        public string? tel_cell { get; set; }
        public string? tel_trabajo { get; set; }
        public string? tel_auxiliar { get; set; }
        public string? web_site { get; set; }
        public string? facebook { get; set; }
        public DateTime? suscripcion_inicial { get; set; }
        public DateTime? suscripcion_vence { get; set; }
        public decimal? suscripcion_mensualidad { get; set; }
        public decimal? suscripcion_anual { get; set; }
        public string? pgx_core_server { get; set; }
        public string? pgx_core_db { get; set; }
        public string? pgx_core_user { get; set; }
        public string? pgx_core_key { get; set; }
        public string? pgx_analisis_server { get; set; }
        public string? pgx_analisis_db { get; set; }
        public string? pgx_analisis_user { get; set; }
        public string? pgx_analisis_key { get; set; }
        public string? pgx_auxiliar_server { get; set; }
        public string? pgx_auxiliar_db { get; set; }
        public string? pgx_auxiliar_user { get; set; }
        public string? pgx_auxiliar_key { get; set; }
        public string? pgx_pruebas_server { get; set; }
        public string? pgx_pruebas_db { get; set; }
        public string? pgx_pruebas_user { get; set; }
        public string? pgx_pruebas_key { get; set; }
        public string? registro_usuario { get; set; }
        public string? modifica_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? direccion { get; set; }
        public string? apto_postal { get; set; }
        public string? pais { get; set; }
        public int? provincia { get; set; }
        public int? canton { get; set; }
        public string? distrito { get; set; }
        public string? cod_pais { get; set; }
        public string? cod_pais_n1 { get; set; }
        public string? cod_pais_n2 { get; set; }
        public string? cod_pais_n3 { get; set; }
        public string? cod_clasificacion { get; set; }
        public string? tipo_id { get; set; }
        public bool pgx_pruebas_activo { get; set; }
        public string? url_app { get; set; }
        public string? url_web { get; set; }
        public string? url_logo { get; set; }
        public bool url_app_activo { get; set; }
        public bool url_web_activo { get; set; }
        public bool url_logo_activo { get; set; }
    }

    public class ServicioDTO
    {
        public string cod_servicio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal costo { get; set; }
        public int cantidad_usuarios { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class ContactoDTO
    {
        public int cod_empresa { get; set; }
        public int cod_contacto { get; set; }
        public string identificacion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string tel_cell { get; set; } = string.Empty;
        public string tel_trabajo { get; set; } = string.Empty;
        public string email_01 { get; set; } = string.Empty;
        public string email_02 { get; set; } = string.Empty;
        public bool activo { get; set; }

        public string? registro_usuario { get; set; }
    }

    public class SMTPDTO
    {
        public int cod_empresa { get; set; }
        public string smtp_id { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public bool asignado { get; set; }  // This will map to whether the checkbox is checked

        public string cod_smtp { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class ConnectionModel
    {
        public string Server { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RespuestaDTO
    {
        public bool HasError { get; set; }  // Indicates if there was an error
        public string ErrorMessage { get; set; } = string.Empty;  // Error message (if any)
        public int Id { get; set; }  // The ID of the newly created record (if successful)
    }

    public class ListaDD
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    //public class EmpresaDto
    //{
    //    public int COD_EMPRESA { get; set; }
    //    public string COD_VENDEDOR { get; set; } = string.Empty;
    //    public string NOMBRE_LARGO { get; set; } = string.Empty;
    //    public string NOMBRE_CORTO { get; set; } = string.Empty;
    //    public byte[]? LOGO_CLIENTE { get; set; }
    //    public string ESTADO { get; set; } = string.Empty;
    //    public string IDENTIFICACION { get; set; } = string.Empty;
    //    public string EMAIL_01 { get; set; } = string.Empty;
    //    public string EMAIL_02 { get; set; } = string.Empty;
    //    public string TEL_CELL { get; set; } = string.Empty;
    //    public string TEL_TRABAJO { get; set; } = string.Empty;
    //    public string TEL_AUXILIAR { get; set; } = string.Empty;
    //    public string WEB_SITE { get; set; } = string.Empty;
    //    public string FACEBOOK { get; set; } = string.Empty;
    //    public DateTime? SUSCRIPCION_INICIAL { get; set; }
    //    public DateTime? SUSCRIPCION_VENCE { get; set; }
    //    public decimal? SUSCRIPCION_MENSUALIDAD { get; set; }
    //    public decimal? SUSCRIPCION_ANUAL { get; set; }
    //    public string PGX_CORE_SERVER { get; set; } = string.Empty;
    //    public string PGX_CORE_DB { get; set; } = string.Empty;
    //    public string PGX_CORE_USER { get; set; } = string.Empty;
    //    public string PGX_CORE_KEY { get; set; } = string.Empty;    
    //    public string PGX_ANALISIS_SERVER { get; set; } = string.Empty;
    //    public string PGX_ANALISIS_DB { get; set; } = string.Empty;
    //    public string PGX_ANALISIS_USER { get; set; } = string.Empty;
    //    public string PGX_ANALISIS_KEY { get; set; } = string.Empty;
    //    public string PGX_AUXILIAR_SERVER { get; set; } = string.Empty;
    //    public string PGX_AUXILIAR_DB { get; set; } = string.Empty;
    //    public string PGX_AUXILIAR_USER { get; set; } = string.Empty;
    //    public string PGX_AUXILIAR_KEY { get; set; } = string.Empty;
    //    public string PGX_PRUEBAS_SERVER { get; set; } = string.Empty;
    //    public string PGX_PRUEBAS_DB { get; set; } = string.Empty;
    //    public string PGX_PRUEBAS_USER { get; set; } = string.Empty;
    //    public string PGX_PRUEBAS_KEY { get; set; } = string.Empty;
    //    public int pgX_PRUEBAS_ACTIVO { get; set; }
    //    public string REGISTRO_USUARIO { get; set; } = string.Empty;
    //    public DateTime? REGISTRO_FECHA { get; set; }
    //    public string DIRECCION { get; set; } = string.Empty;
    //    public string APTO_POSTAL { get; set; } = string.Empty;
    //    public string PAIS { get; set; } = string.Empty;
    //    public int? PROVINCIA { get; set; }
    //    public int? CANTON { get; set; }
    //    public int? DISTRITO { get; set; }
    //    public string COD_PAIS { get; set; } = string.Empty;
    //    public string COD_PAIS_N1 { get; set; } = string.Empty;
    //    public string COD_PAIS_N2 { get; set; } = string.Empty;                 
    //    public string COD_PAIS_N3 { get; set; } = string.Empty;         
    //    public string COD_CLIENTE { get; set; } = string.Empty; 
    //    public string TIPO_ID { get; set; } = string.Empty;
    //    public string COD_CLASIFICACION { get; set; } = string.Empty;

    //    public string URL_Web { get; set; } = string.Empty;
    //    public string URL_App { get; set; } = string.Empty;
    //    public string URL_Logo { get; set; } = string.Empty;
    //    public int URL_App_Activo { get; set; }
    //    public int URL_Web_Activo { get; set; }
    //    public int URL_Logo_Activo { get; set; }


    //}


    public class PaisesDto
    {
        public string COD_PAIS { get; set; } = string.Empty;
        public string DESCRIPCION { get; set; } = string.Empty;
        public string ZONA_HORARIA { get; set; } = string.Empty;
    }

    public class ProvinciaDto
    {
        public string COD_PAIS { get; set; } = string.Empty;
        public string COD_PAIS_N1 { get; set; } = string.Empty;  //cod provincia
        public string DESCRIPCION { get; set; } = string.Empty;

    }

    public class CantonDto : ProvinciaDto
    {
        public string COD_PAIS_N2 { get; set; } = string.Empty;  //cod canton

    }

    public class DistritoDto : CantonDto
    {
        public string COD_PAIS_N3 { get; set; } = string.Empty;  //cod distrito

    }

    public class EmpresaServiciosDto
    {
        public int Cod_Servicio { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public decimal Costo { get; set; }
        public int Cantidad_Usuarios { get; set; }
        public int Registro_Fecha { get; set; }
        public int Registro_Usuario { get; set; }

    }

    public class EmpresaContactosDto
    {
        public int cod_Contacto { get; set; }
        public int identificacion { get; set; }
        public string nombre { get; set; } = string.Empty;
        public string tel_cell { get; set; } = string.Empty;
        public string tel_trabajo { get; set; } = string.Empty;
        public string Email_01 { get; set; } = string.Empty;
        public string Email_02 { get; set; } = string.Empty;
    }

    public class EmpresaSmtpDto
    {
        public string COD_SMTP { get; set; } = string.Empty;
        public string DESCRIPCION { get; set; } = string.Empty;
        public int ASIGNADO { get; set; }
    }


}//end namespace
