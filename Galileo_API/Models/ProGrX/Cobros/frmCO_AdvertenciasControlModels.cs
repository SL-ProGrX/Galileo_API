
public class CoAdvertenciasControlLista
{
    public CoAdvertenciasControlTotales totales { get; set; }
    public List<CoAdvertenciasControlData> lista { get; set; }
}
public class CoAdvertenciasControlTotales
{
    public int total { get; set; }
    public decimal montototal { get; set; }
}

public class CoAdvertenciasControlData
{
    public int linea { get; set; }
    public string cod_advertencia { get; set; } = string.Empty;
    public string cedula { get; set; } = string.Empty;
    public string nombre { get; set; } = string.Empty;
    public string estado { get; set; } = string.Empty;
    public string advtipo { get; set; } = string.Empty;
    public DateTime? fecha_vence { get; set; }
    public DateTime? registro_fecha { get; set; }
    public string registro_usuario { get; set; } = string.Empty;
    public DateTime? resolucion_fecha { get; set; }
    public string resolucion_usuario { get; set; } = string.Empty;
}


public class CoAdvertenciasControlFiltros
{
    public string? cedula { get; set; } = string.Empty;
    public string? nombre { get; set; } = string.Empty;
    public string? usuario { get; set; } = string.Empty;
    public string? estado_persona { get; set; } = string.Empty;
    public string? estado { get; set; } = string.Empty;
    public DateTime? fecha_inicio { get; set; }
    public DateTime? fecha_corte { get; set; }
    public string? lista_advertencias { get; set; } = string.Empty;
    public string? lista_estados_personas { get; set; } = string.Empty;
    public string? filtro { get; set; } //filtro del buscar en tablas o buscador
    public int? pagina { get; set; } = 1;//pagina de la tabla
    public int? paginacion { get; set; } = 30; //paginacion de la tabla
    public int? sortOrder { get; set; } = 0; //0: sin orden, 1: ascendente, 2: descendente
    public string? sortField { get; set; } //campo por el cual se ordena

}