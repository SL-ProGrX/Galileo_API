namespace Galileo.Models.PRES
{
    public class CntxCData
    {
        public int IdX { get; set; }
        public string ItmX { get; set; } = string.Empty;
    }

    public class PresModeloData
    {
        public string Cod_Modelo { get; set; } = string.Empty;
        public int Cod_Contabilidad { get; set; }
        public int ID_Cierre { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Estado_Desc { get; set; } = string.Empty;
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Resolucion_Usuario { get; set; } = string.Empty;
        public DateTime Resolucion_Fecha { get; set; }
        public string Resolucion_Notas { get; set; } = string.Empty;
        public bool Mostrar_Inicio { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public decimal Patrimonio_Promedio { get; set; } = 0;
    }

    public class PresModeloInsert
    {
        public string Cod_Modelo { get; set; } = string.Empty;
        public int? Cod_Contabilidad { get; set; }
        public int? ID_Cierre { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;

        public decimal Patrimonio_Promedio { get; set; } = 0;
    }

    public class PressModeloUsuarios
    {
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class PressModeloAjustes
    {
        public string Cod_Ajuste { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class PressModeloAjUsRegistro
    {
        public string CodModelo { get; set; } = string.Empty;
        public int? CodContab { get; set; }
        public string Cod_Ajuste { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string UsuarioReg { get; set; } = string.Empty;
        public bool? Activo { get; set; }
    }

    public class PresModeloCierreData
    {
        public int Inicio_Anio { get; set; } = 1900;
        public int Inicio_Mes { get; set; } = 1;
        public int Corte_Anio { get; set; } = 1900;
        public int Corte_Mes { get; set; } = 12;
    }

    public class PresModeloIndicadorData
    {
        public DateTime Corte { get; set; }
        public string Cod_Modelo { get; set; } = string.Empty;
        public int Cod_Contabilidad { get; set; }
        public decimal? Tipo_Cambio { get; set; }
        public double? Tasa_Basica_Pasiva { get; set; }
        public double? Indice_Inflacion { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
        public DateTime? Modifica_Fecha { get; set; }
        public string Modifica_Usuario { get; set; } = string.Empty;
    }

    public class PresModeloIndicadorGuardar
    {
        public DateTime Corte { get; set; }
        public decimal? Tipo_Cambio { get; set; }
        public double? Tasa_Basica_Pasiva { get; set; }
        public double? Indice_Inflacion { get; set; }
    }

    public class PresModeloIndicadoresGuardarRequest
    {
        public string Cod_Modelo { get; set; } = string.Empty;
        public int Cod_Contabilidad { get; set; } = 0;
        public string Usuario { get; set; } = string.Empty;
        public List<PresModeloIndicadorGuardar> Indicadores { get; set; } = new();
    }

    public class PresModeloCopiar
    {
        public int cod_Empresa { get; set; }
        public string cod_Modelo_Origen { get; set; } = string.Empty;
        public string cod_Modelo_Destino { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

}