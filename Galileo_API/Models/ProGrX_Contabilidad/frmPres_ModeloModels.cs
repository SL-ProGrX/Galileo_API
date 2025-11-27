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
}