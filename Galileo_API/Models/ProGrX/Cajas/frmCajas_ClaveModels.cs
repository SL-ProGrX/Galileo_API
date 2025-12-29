using static Galileo.Models.ProGrX.Cajas.CajasDesglocePagoRequest;

namespace PgxAPI.Models.ProGrX.Cajas
{

    public class CajasUsuarioDTO
    {
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public int periodicidad_contrasena { get; set; }
    }


}

