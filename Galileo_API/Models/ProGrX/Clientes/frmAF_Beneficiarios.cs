namespace PgxAPI.Models.ProGrX.Clientes
{
    public class PersonaBeneficiarioDTO
    {
        public int Linea_Id { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Tipo_Relacion { get; set; } = string.Empty;
        public string Relacion_Desc { get; set; } = string.Empty;
        public string Cedula_Beneficiario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public DateTime Fecha_Nac { get; set; }
        public string Cod_Parentesco { get; set; } = string.Empty;
        public string Parentesco { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
        public bool Aplica_Seguros { get; set; }
        public string Notas { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Apto_Postal { get; set; } = string.Empty;
        public string Telefono1 { get; set; } = string.Empty;
        public string Telefono2 { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Sexo { get; set; } = string.Empty;
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public bool Albacea_Check { get; set; }
        public bool Albacea_Ind { get; set; }
        public string Albacea_Cedula { get; set; } = string.Empty;
        public string Albacea_Nombre { get; set; } = string.Empty;
        public string Albacea_Movil{ get; set; } = string.Empty;
        public string Albacea_TelTra { get; set; } = string.Empty;
        public string Albacea_TelTra_Ext { get; set; } = string.Empty;
        public string Tipo_Id { get; set; } = string.Empty;
        public string Tipo_Id_R { get; set; } = string.Empty;
        public string Tipo_Id_Desc { get; set; } = string.Empty;
        public string TipoMov { get; set; } = "A";
    }


    public class Beneficiarios_CatalogoDTO
    {
        public List<DropDownListaGenericaModel> TiposIdentificacion { get; set; }
        public List<DropDownListaGenericaModel> Parentescos { get; set; }
    }
}
