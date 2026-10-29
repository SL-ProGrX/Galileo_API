namespace Galileo.Models.ProGrX_Nucleo
{
    public class SysIvaParametrosData
    {
        public string? codParametro { get; set; }
        public string? descripcion { get; set; }
        public string? valor { get; set; }
        public string? tipo { get; set; }
        public string? visible { get; set; }
        public string? notas { get; set; }
        public string? registroUsuario { get; set; }
        public DateTime? registroFecha { get; set; }
        public string? modificaUsuario { get; set; }
        public DateTime? modificaFecha { get; set; }
        public string? valorMask { get; set; }
        public string? cuentaDescripcion { get; set; }
    }

    public class SysIvaParametrosLista
    {
        public int total { get; set; }
        public List<SysIvaParametrosData>? lista { get; set; }
    }

    public class SysIvaParametrosUpdateRequest
    {
        public string? valor { get; set; }
    }

    public class SysIvaParametrosUpdateResponse : SysIvaParametrosData
    {
        public int? resultadoOperacion { get; set; }
    }

    public class SysIvaTipoSaveRequest
    {
        public string? Cod_IVA { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Porcentaje { get; set; }
        public string? Cod_Cuenta { get; set; }
        public string? Cod_Hacienda { get; set; }
        public int? Activo { get; set; }
    }

    public class SysIvaCuentasResumenData
    {
        public string? codigo { get; set; }
        public string? codigoMask { get; set; }
        public string? codigoAlterna { get; set; }
        public string? nombre { get; set; }
        public string? movimientos { get; set; }
        public string? divisa { get; set; }
        public int? nivel { get; set; }
    }

    public class SysIvaCuentasResumenLista
    {
        public int total { get; set; }
        public List<SysIvaCuentasResumenData>? lista { get; set; }
    }

    public sealed class ParametroIvaConsulta
    {
        public string? COD_PARAMETRO { get; set; }
        public string? DESCRIPCION { get; set; }
        public string? VALOR { get; set; }
        public string? TIPO { get; set; }
        public string? NOTAS { get; set; }
        public string? REGISTRO_USUARIO { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string? MODIFICA_USUARIO { get; set; }
        public DateTime? MODIFICA_FECHA { get; set; }
        public string? COD_CUENTA_MASK { get; set; }
        public string? CTA_DESCRIPCION { get; set; }
    }

    public sealed class TipoIvaConsultaRow
    {
        public string COD_IVA { get; set; } = string.Empty;
        public string DESCRIPCION { get; set; } = string.Empty;
        public decimal PORCENTAJE { get; set; }
        public string COD_CUENTA { get; set; } = string.Empty;
        public string? COD_HACIENDA { get; set; }
        public int? ACTIVO { get; set; }
        public string? COD_CUENTA_MASK { get; set; }
        public string? CTA_DESCRIPCION { get; set; }
    }

    public sealed class TipoIvaMovimiento
    {
        public string Movimiento { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
    }
}
