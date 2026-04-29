namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class VivContactoDto
    {
        public int IdContacto { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class VivContactoFiltroParams
    {
        public string? TipoProfesional { get; set; }
        public string? Estado { get; set; }
    }

    public class CrdSgtBancoDto
    {
        public int Id_Banco { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Desc_Corta { get; set; } = string.Empty;
        public string Cta { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public int Idx { get; set; }
        public string ItmX { get; set; } = string.Empty;
    }

    public class CrdSgtBancoParams
    {
        public string Usuario { get; set; } = string.Empty;
        public string? Divisa { get; set; }
    }

    public class VivCuentaBancariaDto
    {
        public string Banco { get; set; } = string.Empty;
        public string TipoDesc { get; set; } = string.Empty;
        public string Cod_Divisa { get; set; } = string.Empty;
        public string Cuenta_Interna { get; set; } = string.Empty;
        public string Cuenta_Interbanca { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public string Destino { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class CrdVivContactoConsultaDto
    {
        public int IdContacto { get; set; }
        public int? IdEmpresa { get; set; }
        public string? TipoContacto { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public string? TipoProfesional { get; set; }
        public string? Telefono { get; set; }
        public string? Fax { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? Estado { get; set; }
        public DateTime? SuspensionInicio { get; set; }
        public DateTime? SuspensionCorte { get; set; }
        public string? Observacion { get; set; }
        public string? RegistroUsuario { get; set; }
        public DateTime? RegistroFecha { get; set; }
        public string? SuspendeUsuario { get; set; }
        public DateTime? SuspendeFecha { get; set; }
        public short? PagaHonorarios { get; set; }
        public short? SuspendeActivo { get; set; }
        public string? Emite { get; set; }
        public int? Cod_Banco { get; set; }
        public string? TelefonoExt { get; set; }
        public string? FaxExt { get; set; }
        public string? AptoPostal { get; set; }
        public string? TelMovil { get; set; }
        public DateTime? Modifica_Fecha { get; set; }
        public string? Modifica_Usuario { get; set; }
        public int? TipoId { get; set; }
        // Campos calculados/adicionales del SP
        public string? Apellido_1 { get; set; }
        public string? Apellido_2 { get; set; }
        public string? Nombre_1 { get; set; }
        public string? Banco_Desc { get; set; }
        public int SuspendeActual { get; set; }
        public int EmpresaId { get; set; }
        public string EmpresaNombre { get; set; } = string.Empty;
        public string TipoProfesional_Desc { get; set; } = string.Empty;
        public string Estado_Desc { get; set; } = string.Empty;
        public int TipoId_Rev { get; set; }
        public string TipoId_Desc { get; set; } = string.Empty;
    }

    public class VivContactoEmpresaDto
    {
        public int IdContacto { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public bool PagaHonorarios { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int Suspendido { get; set; }
    }

    public class CrdVivContactoAddParams
    {
        public int? IdContacto { get; set; }
        public int? TipoId { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string TipoProfesional { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string TelefonoExt { get; set; } = string.Empty;
        public string TelMovil { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public string FaxExt { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string AptoPostal { get; set; } = string.Empty;
        public short? PagaHonorarios { get; set; }
        public int? BancoId { get; set; }
        public string Emite { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class CrdVivContactoAddResult
    {
        public int IdContacto { get; set; }
        public string Movimiento { get; set; } = string.Empty;
    }
}
