
namespace Galileo.Models.ProGrX.Cobros
{
    public class CoAplFndInstitucionesData
    {
        public int cod_institucion { get; set; } = 0;
        public string descripcion { get; set; } = "";
        public string fecha_corte { get; set; } = "";
        public bool aplica_pagos { get; set; } = false;
        public bool isNew { get; set; } = false;   
    }

    public class CoAplFndInstitucionesListaResult
    {
        public int total { get; set; } = 0;
        public List<CoAplFndInstitucionesData> lista { get; set; } = new();
    }

    public class CoAplFndInstitucionesActualizarRequest
    {
        public int cod_institucion { get; set; } = 0;
        public bool aplica_pagos { get; set; } = false;
        public string usuario_sesion { get; set; } = "";
    }
}
