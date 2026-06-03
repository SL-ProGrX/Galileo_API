namespace Galileo.Models.ProGrX_Procesos
{
    public class FrmCCProcesoMensualProcAddModels
    {
        public class CcPlanillaProcesosComplementariosLista
        {
            public int total { get; set; }
            public List<CcPlanillaProcesosComplementariosData> lista { get; set; } = new List<CcPlanillaProcesosComplementariosData>();
        }

        public class CcPlanillaProcesosComplementariosData
        {
            public string transaccion { get; set; } = string.Empty;              
            public string proceso { get; set; } = string.Empty;               
            public int proc_num { get; set; } = 0;
            public string ejecucion_tipo { get; set; } = string.Empty;
            public string ejecucion_orden { get; set; } = string.Empty;
            public string procedimiento { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
            public bool parametros_planillas { get; set; } = false;
            public string parametros_add { get; set; } = string.Empty;
            public bool isNew { get; set; } = false;
        }
    }
}
