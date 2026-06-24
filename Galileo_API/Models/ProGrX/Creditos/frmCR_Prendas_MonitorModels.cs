namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrPrendasMonitorConsultaRequest
    {
        public string Tipo_Fecha { get; set; } = string.Empty;
        public required DateTime Fecha_Inicio { get; set; }
        public required DateTime Fecha_Corte { get; set; }

        public bool? Pe_Activa { get; set; }
        public DateTime? Vence_Inicio { get; set; }
        public DateTime? Vence_Corte { get; set; }

        public string? Credito_Estado_Id { get; set; }
        public int? Id_Presentacion { get; set; }
        public int? Id_Combustible { get; set; }
        public int? Id_Modelo { get; set; }
        public string? Estado_Persona { get; set; }
        public int? Anio { get; set; }
        public int? Puertas_Numero { get; set; }

        public string? Unidad_Peso { get; set; }
        public decimal? Peso_Inicio { get; set; }
        public decimal? Peso_Corte { get; set; }

        public string? Unidad_Capacidad { get; set; }
        public decimal? Capacidad_Inicio { get; set; }
        public decimal? Capacidad_Corte { get; set; }

        public string? Unidad_Cilindraje { get; set; }
        public decimal? Cilindraje_Inicio { get; set; }
        public decimal? Cilindraje_Corte { get; set; }

        public List<string>? Tipo_Prenda { get; set; }
        public List<string>? Id_Comercio { get; set; }
        public List<string>? Id_Marca { get; set; }

        public string? Registro_Usuario { get; set; }
        public string? Actualiza_Usuario { get; set; }
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public string? Id_Principal { get; set; }
        public string? Id_Provisional { get; set; }
        public string? Chasis_Numero { get; set; }
        public string? Vin_Motor { get; set; }
        public string? Color { get; set; }
    }

    public class CrPrendasMonitorConsultaData
    {
        public long? Prenda_Id { get; set; }
        public string? Cod_Preanalisis { get; set; }
        public long? Id_Solicitud { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Tipo_Prenda_Desc { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal? Cobertura { get; set; }
        public decimal? Porc_Cobertura { get; set; }
        public string Estado_Desc { get; set; } = string.Empty;
        public string Id_Principal { get; set; } = string.Empty;
        public string Id_Provisional { get; set; } = string.Empty;
        public decimal? Avaluo { get; set; }
        public decimal? Valor_Fiscal { get; set; }
        public decimal? Valor_Mercado { get; set; }
        public decimal? Credito_Monto { get; set; }
        public decimal? Credito_Saldo { get; set; }
        public string Credito_Divisa { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime? Actualiza_Fecha { get; set; }
        public string Actualiza_Usuario { get; set; } = string.Empty;
        public string Comercializa_Desc { get; set; } = string.Empty;
        public string Marca_Desc { get; set; } = string.Empty;
        public string Modelo_Desc { get; set; } = string.Empty;
        public int? Anio { get; set; }
        public string Presentacion_Desc { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Chasis_Numero { get; set; } = string.Empty;
        public string Vin_Motor { get; set; } = string.Empty;
        public int? Puertas_Numero { get; set; }
        public decimal? Peso { get; set; }
        public decimal? Capacidad { get; set; }
        public decimal? Cilindraje { get; set; }
        public string Tomo { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
        public string Notario { get; set; } = string.Empty;
        public DateTime? Notario_Registro_Fecha { get; set; }
        public string Poliza_Mnt_Formalizacion { get; set; } = string.Empty;
        public string Poliza_Rst_Plan { get; set; } = string.Empty;
        public string Peso_Ud_Desc { get; set; } = string.Empty;
        public string Capacidad_Ud_Desc { get; set; } = string.Empty;
        public string Cilindraje_Ud_Desc { get; set; } = string.Empty;
        public string Pe_Activa { get; set; } = string.Empty;
        public string Pe_Numero { get; set; } = string.Empty;
        public DateTime? Pe_Vence { get; set; }
        public decimal? Pe_Prima { get; set; }
        public string Pe_Frecuencia { get; set; } = string.Empty;
        public string Pe_Vencida { get; set; } = string.Empty;
        public string Pe_Cedula { get; set; } = string.Empty;
        public string Pe_Nombre { get; set; } = string.Empty;
        public decimal? Pe_Cobertura { get; set; }
        public string Titular_Tercero { get; set; } = string.Empty;
        public string Titular_Nombre { get; set; } = string.Empty;
    }

    public class CrPrendasMonitorCatalogoDbItem
    {
        public string IdX { get; set; } = string.Empty;
        public string ItmX { get; set; } = string.Empty;
    }
}
