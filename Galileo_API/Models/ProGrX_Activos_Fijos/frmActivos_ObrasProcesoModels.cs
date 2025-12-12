namespace Galileo.Models.ProGrX_Activos_Fijos
{
    public class ActivosObrasProcesoAdendumsData
    {
        public string cod_Adendum { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public decimal? monto { get; set; }
    }

    public class ActivosObrasProcesoDesembolsosData
    {
        public int? secuencia { get; set; } 
        public string cod_desembolso { get; set; } = string.Empty;
        public string cod_proveedor { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public decimal? monto { get; set; }
        public string desembolso { get; set; } = string.Empty;
        public string proveedor { get; set; } = string.Empty;

    }
    public class ActivosObrasProcesoResultadosData
    {
        public int id_resultados { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string num_placa { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public int id_adicion { get; set; }
        public string nombre { get; set; } = string.Empty; 
        public string ta { get; set; } = string.Empty; 

    }
    public class ActivosObrasData
    { 
        public string contrato { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string cod_proveedor { get; set; } = string.Empty;
        public DateTime? fecha_finiquito { get; set; }
        public string encargado { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_estimada { get; set; }
        public string ubicacion { get; set; } = string.Empty;
        public decimal? presu_original { get; set; }
        public decimal? addendums { get; set; }
        public decimal? presu_actual { get; set; }
        public decimal? desembolsado { get; set; }
        public decimal? distribuido { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string tipoobra { get; set; } = string.Empty;
        public string proveedor { get; set; } = string.Empty;
        public string cod_tipo { get; set; } = string.Empty;
    }
}