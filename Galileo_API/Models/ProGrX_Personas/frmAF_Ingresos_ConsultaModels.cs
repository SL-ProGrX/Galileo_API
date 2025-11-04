namespace PgxAPI.Models.ProGrX_Personas
{
    public class IngresosConsultaFiltro
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public DateTime? Inicio { get; set; }
        public DateTime? Corte { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Promotor { get; set; } = string.Empty;
    }

    public class IngresosConsultaData
    {
        public int Id_Persona { get; set; }
        public int Id_Afiliacion { get; set; }
        public string Fecha_Ingreso { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Promotor { get; set; } = string.Empty;
        public string Tipo_Desc { get; set; } = string.Empty;
        public string TipoIdDesc { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Id_Alterno { get; set; } = string.Empty;
        public string Apellido1 { get; set; } = string.Empty;
        public string Apellido2 { get; set; } = string.Empty;
        public string Nombrev2 { get; set; } = string.Empty;
        public string Sexo { get; set; } = string.Empty;
        public string Fecha_Nacimiento { get; set; } = string.Empty;
        public string Vence_Cedula { get; set; } = string.Empty;
        public string EstadoPersonaDesc { get; set; } = string.Empty;
        public string EstadoCivilDesc { get; set; } = string.Empty;
        public string EstadoLaboralDesc { get; set; } = string.Empty;
        public int AnioServicio { get; set; }
        public string Af_Email { get; set; } = string.Empty;
        public string Email_02 { get; set; } = string.Empty;
        public string Tel_Habitacion { get; set; } = string.Empty;
        public string Tel_Trabajo { get; set; } = string.Empty;
        public string Tel_Celular { get; set; } = string.Empty;
        public string I_Beneficiario { get; set; } = string.Empty;
        public string ProvinciaDesc { get; set; } = string.Empty;
        public string CantonDesc { get; set; } = string.Empty;
        public string DistritoDesc { get; set; } = string.Empty;
        public string Dirección { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string Nacionalidad { get; set; } = string.Empty;
        public string DeductoraDesc { get; set; } = string.Empty;
        public string InstitucionDesc { get; set; } = string.Empty;
        public string UP_Desc { get; set; } = string.Empty;
        public string UT_Desc { get; set; } = string.Empty;
        public string CT_Desc { get; set; } = string.Empty;
        public string ProfesionDesc { get; set; } = string.Empty;
        public string SectorDesc { get; set; } = string.Empty;
        public string NivelAcademicoDesc { get; set; } = string.Empty;
        public string Puesto { get; set; } = string.Empty;
        public string OficinaDesc { get; set; } = string.Empty;
        public string Tra_Provincia_Desc { get; set; } = string.Empty;
        public string Tra_Canton_Desc { get; set; } = string.Empty;
        public string Tra_Distrito_Desc { get; set; } = string.Empty;
        public string Tra_Direccion { get; set; } = string.Empty;
        public string Salario_Tipo { get; set; } = string.Empty;
        public string SalarioDivisaDesc { get; set; } = string.Empty;
        public string SalarioEmbargo { get; set; } = string.Empty;
        public decimal SALARIO_DEVENGADO { get; set; }
        public decimal SALARIO_NETO { get; set; }
        public decimal SALARIO_REBAJOS { get; set; }
        public string C_ActividadDesc { get; set; } = string.Empty;
        public string PEP_Indica { get; set; } = string.Empty;
        public string PEP_CARGO { get; set; } = string.Empty;
        public int Tipo_CES { get; set; }
    }

    public class IngresosConsultaLista
    {
        public List<IngresosConsultaData> Lista { get; set; } = new List<IngresosConsultaData>();
    }
}