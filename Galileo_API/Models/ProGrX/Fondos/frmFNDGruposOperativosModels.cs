namespace Galileo.Models.ProGrX.Fondos
{
    public class FndGrupoOperativoModel
    {
        public string Grupo_Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo_Grupo { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime? Fecha_Registra { get; set; }
        public string Usuario_Registra { get; set; } = string.Empty;
        public bool IsNew { get; set; } 
    }

    public class FndGruposOperativosLista
    {
        public int total { get; set; }
        public List<FndGrupoOperativoModel> lista { get; set; } = new();
    }

    public class FndGrupoOperativoValidaResult
    {
        public int Existe { get; set; }
    }

    public class FndGrupoOperativoPlanResult
    {
        public string Cod_Operadora { get; set; } = string.Empty;
        public string Cod_Plan { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime? Fecha_Registra { get; set; }
        public string Usuario_Registra { get; set; } = string.Empty;
    }

    public class FndGrupoOperativoUsuarioResult
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime? Fecha_Registra { get; set; }
        public string Usuario_Registra { get; set; } = string.Empty;
    }

    public class FndGrupoOperativoConceptoResult
    {
        public string Retencion_Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime? Fecha_Registra { get; set; }
        public string Usuario_Registra { get; set; } = string.Empty;
    }

    public class FndGrupoOperativoFiltroRequest
    {
        public string GrupoCodigo { get; set; } = string.Empty;
        public string Filtro { get; set; } = string.Empty;
    }

    public class FndGrupoOperativoAsignarPlanRequest
    {
        public int Cod_Operadora { get; set; }
        public string Plan_Codigo { get; set; } = string.Empty;
        public int Grupo_Codigo { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public bool Asignar { get; set; } 
    }

    public class FndGrupoOperativoAsignarUsuarioRequest
    {
        public string Usuario_Codigo { get; set; } = string.Empty;
        public int Grupo_Codigo { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public bool Asignar { get; set; }
    }

    public class FndGrupoOperativoAsignarConceptoRequest
    {
        public string Retencion_Codigo { get; set; } = string.Empty;
        public int Grupo_Codigo { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public bool Asignar { get; set; }
    }
}
