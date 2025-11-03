namespace PgxAPI.Models
{

    //Solicitudes:

    public class ReporteUsuariosListaSolicitudDto
    {
        public int EmpresaId { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public char Estado { get; set; }
        public string Vinculado { get; set; } = string.Empty;
        public int Contabiliza { get; set; } = 0;
    }

    public class ReporteUsuariosRolesSolicitudDto
    {
        public int EmpresaId { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public int Contabiliza { get; set; } = 0;
    }

    public class ReporteUsuariosPermisosSolicitudDto
    {
        public int EmpresaId { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public int Contabiliza { get; set; } = 0;
    }

    public class ReporteRolesPermisosSolicitudDto
    {
        public string RolId { get; set; } = string.Empty;
    }


    //Respuesta:
    public class ReporteUsuariosListaRespuestaDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int Contabiliza { get; set; } = 0;
        public int Cod_Empresa { get; set; }
        public char Estado { get; set; }
        public string Estado_Desc { get; set; } = string.Empty;
        public string Vinculacion { get; set; } = string.Empty;
        public int Limita_Acceso_Estacion { get; set; }
        public int Limita_Acceso_Horario { get; set; }
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime Salida_Fecha { get; set; }
        public string Salida_Usuario { get; set; } = string.Empty;
    }

    public class ReporteUsuariosRolesRespuestaDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Rol_Id { get; set; } = string.Empty;
        public string Rol_Descripcion { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }
    public class ReporteUsuariosPermisosRespuestaDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public string Rol_Id { get; set; } = string.Empty;
        public string Rol_Descripcion { get; set; } = string.Empty;
        public int Modulo_Id { get; set; } = 0;
        public string Modulo_Descripcion { get; set; } = string.Empty;
        public string Form_Id { get; set; } = string.Empty;
        public string Form_Descripcion { get; set; } = string.Empty;
        public int Opcion_Id { get; set; } = 0;
        public string Opcion_Descripcion { get; set; } = string.Empty;
    }

    public class ReporteRolesPermisosRespuestaDto
    {
        public string Rol_Id { get; set; } = string.Empty;
        public string Rol_Descripcion { get; set; } = string.Empty;
        public int Modulo_Id { get; set; } = 0;
        public string Modulo_Descripcion { get; set; } = string.Empty;
        public string Form_Id { get; set; } = string.Empty;
        public string Form_Descripcion { get; set; } = string.Empty;
        public int Opcion_Id { get; set; } = 0;
        public string Opcion_Descripcion { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class ReporteUsuarioVinculacionDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Estado_Desc { get; set; } = string.Empty;
        public string Vinculacion { get; set; } = string.Empty;
    }

    public class ReporteUsuarioRolesDto
    {
        public string IdX { get; set; } = string.Empty;
        public string ItmX { get; set; } = string.Empty;
    }

    /*public class ReporteUsuariosErrorDto
    {
        public int Code { get; set; }
        public string Description { get; set; } = string.Empty;
    }*/
}
