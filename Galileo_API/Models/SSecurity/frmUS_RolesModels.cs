namespace PgxAPI.Models
{
    public class RolesVincularDTO
    {
        public int Index { get; set; }
        public string CodRol { get; set; } = string.Empty;
        public long? CodEmpresa { get; set; }
    }

    public class RolesObtenerDTO
    {
        public string Cod_Rol { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class RolInsertarDTO
    {
        public string Cod_Rol { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;

        //public string CodRol { get; set; }
        //public string Descripcion { get; set; }
        //public bool Activo { get; set; }
        //public string Usuario {  get; set; }
    }

    public class ClientesObtenerDTO
    {
        public string Cod_Empresa { get; set; } = string.Empty;
        public string Nombre_Corto { get; set; } = string.Empty;
        public string Nombre_Largo { get; set; } = string.Empty;
    }

}
