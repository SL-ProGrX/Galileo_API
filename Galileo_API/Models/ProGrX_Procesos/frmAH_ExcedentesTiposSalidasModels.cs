namespace PgxAPI.Models
{

    public class TipoSalidaDTO
    {


        public string Cod_Salida { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        public bool Activa { get; set; }

        public bool Sistema { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public string Tipo_Aplicacion_Desc { get; set; } = string.Empty;

        public int Id_Operadora { get; set; }

        public string Codigo_Plan { get; set; } = string.Empty;

        public int Banco_Cta { get; set; } = 0;

        public bool Requiere_Porcentaje { get; set; }

        public bool Permite_Reclasificar { get; set; }


    }
}
