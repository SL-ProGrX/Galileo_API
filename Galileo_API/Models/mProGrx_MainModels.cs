namespace Galileo.Models
{
    public class ConsultaStatusResultDto
    {
        public int PERSONA_ID { get; set; }
        public int AUTORIZACION_ID { get; set; }
    }

    public class SifRegistraTagsRequestDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public string Observacion { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public string Llave_01 { get; set; } = string.Empty;
        public string Llave_02 { get; set; } = string.Empty;
        public string Llave_03 { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class MenuUsoRequestDto
    {
        public int menu_nodo { get; set; }
        public int Empresa_Id { get; set; }
        public int Modulo { get; set; }
        public int Formulario { get; set; }
        public int Tipo { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class MenuFavoritosRequestDto
    {
        public int Empresa_Id { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class MenuUsoResultDto
    {
        public int Nodo { get; set; }
        public int Cliente { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class ParametroDto
    {
        public int Cod_Parametro { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public DateTime Modifica_Fecha { get; set; }
        public string Modifica_Usuario { get; set; } = string.Empty;
        public string Acceso { get; set; } = string.Empty;
    }

    public class EmpresaEnlaceResultDto
    {
        public int Cod_Empresa { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int SysCrdPlanPago { get; set; } = 0;
        public int SysDocVersion { get; set; } = 0;
        public int SysTesVersion { get; set; } = 0;
        public int sys_ccss_ind { get; set; } = 0;

        public int? ec_visible_patrimonio { get; set; } = 0;
        public int? ec_visible_fondos { get; set; } = 0;
        public int? ec_visible_creditos { get; set; } = 0;
        public int? ec_visible_fianzas { get; set; } = 0;

        public string? estadoCuenta { get; set; } = string.Empty;

    }

    public class MenuFavoritosResultDto
    {
        public int Menu_Nodo { get; set; }
        public int? Nodo_Padre { get; set; }
        public string Nodo_Descripcion { get; set; } = string.Empty;
        public char? Tipo { get; set; }
        public string Icono { get; set; } = string.Empty;
        public char? Modo { get; set; }
        public int? Modal { get; set; }
        public int? Accesos_Dll_Id { get; set; }
        public string Accesos_Dll_Cls { get; set; } = string.Empty;
        public int? Prioridad { get; set; }
        public string Formulario { get; set; } = string.Empty;
        public int? Modulo { get; set; }
    }

    public class SifOficinasUsuarioResultDto
    {
        public string Titular { get; set; } = string.Empty;
        public string Apoyo { get; set; } = string.Empty;
        public string CodUnidad { get; set; } = string.Empty;
        public string CodCentroCosto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Inconsistencia { get; set; }
    }

    public class ParAhcr
    {
        public DateTime? cr_fecha_calculo { get; set; }
        public DateTime fechaalterna { get; set; }
    }
}
