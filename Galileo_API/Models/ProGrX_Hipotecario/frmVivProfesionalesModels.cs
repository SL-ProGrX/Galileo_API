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
        public string? Identificacion { get; set; }
        public string? Nombre { get; set; }
        public string? TipoContacto { get; set; }
        public string? TipoProfesional { get; set; }
        public string? Estado { get; set; }
        public int? Cod_Banco { get; set; }
        public int? IdEmpresa { get; set; }
        public int? TipoId { get; set; }
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
}
