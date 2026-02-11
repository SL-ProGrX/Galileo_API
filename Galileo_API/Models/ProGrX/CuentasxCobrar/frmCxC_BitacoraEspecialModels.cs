namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class FrmCxCBitacoraEspecialModels
    {
        public class BitacoraEspeciaLista
        {
            public int total { get; set; }
            public List<BitacoraEspeciaData> lista { get; set; } = new List<BitacoraEspeciaData>();
        }
        public class BitacoraEspeciaData
        {
            public bool? Revisado { get; set; }
            public int bitacora_id { get; set; }
            public string? Usuario { get; set; } = string.Empty;
            public DateTime? fecha { get; set; }             
            public string? MovimientoDesc { get; set; } = string.Empty;
            public int Operacion { get; set; }
            public string? Cod_Concepto { get; set; } = string.Empty;
            public string? Detalle { get; set; } = string.Empty;
            public string? Cedula { get; set; } = string.Empty;
            public string? Nombre { get; set; } = string.Empty;
            public string? Notas { get; set; } = string.Empty;
            public string? Revisado_Usuario { get; set; } = string.Empty;
            public DateTime? Revisado_Fecha { get; set; }
        }

        public class BitacoraEspeciaFiltros
        {
            public string? usuario { get; set; } = string.Empty;
            public string? cedula { get; set; } = string.Empty;  
            public DateTime? fecha_inicio { get; set; }
            public DateTime? fecha_corte { get; set; }
            public bool fechasChk { get; set; } = false;
            public bool usuariosChk { get; set; } = false;
            public bool revisionChk { get; set; } = false;
            public string? lista_movimientos { get; set; } = string.Empty;
            public string? revision { get; set; } = string.Empty;
            public string? filtro { get; set; } //filtro del buscar en tablas o buscador
            public int? pagina { get; set; } = 1;//pagina de la tabla
            public int? paginacion { get; set; } = 30; //paginacion de la tabla
            public int? sortOrder { get; set; } = 0; //0: sin orden, 1: ascendente, 2: descendente
            public string? sortField { get; set; } //campo por el cual se ordena

        }
    }
}
