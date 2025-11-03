namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AF_CR_Renuncias_TagsData
    {
        public int Cod_Renuncia { get; set; }
        public string Cedula { get; set; }
        public string Nombre { get; set; }
        public string Estado { get; set; }
        public string Tipo { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public DateTime? Vencimiento { get; set; }
        public string Causa_Desc { get; set; }
        public string Estado_Desc { get; set; }
    }

    public class AF_CR_RenunciaRecepcionAplica
    {
        public int RenunciaId { get; set; }
        public string Usuario { get; set; } = "";
        public string Notas { get; set; } = "";
        public string Equipo { get; set; } = "";
        public string Version { get; set; } = "";
    }

    public class AF_CR_RenunciaRevisionAplica : AF_CR_RenunciaRecepcionAplica
    {
        public string Estado { get; set; }        
    }

    public class AF_CR_RenunciaEtiquetas
    {
        public int Id { get; set; }
        public int Cod_Renuncia { get; set; }
        public string Cod_Etiqueta { get; set; } = "";
        public DateTime? Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = "";
        public string Observacion { get; set; } = "";
        public string Tag_Desc { get; set; } = "";
        public DateTime? Fecha_Format { get; set; }
    }

    public class AF_CR_RenunciaReversa : AF_CR_RenunciaRecepcionAplica
    {
        public string NotasReversa { get; set; } = "";
    }

}
