using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Comites
{
    public class FrmAfCdPreCalculo
    {
        public class CrdPreCalculoComiteRequest
        {
            public string ComiteId { get; set; } = string.Empty;
            public decimal AjusteAsociados { get; set; } = 0;
        }

        public class CrdPreCalculoComiteResponse
        {
            public string ComiteId { get; set; } = string.Empty;
            public string ComiteDescripcion { get; set; } = string.Empty;
            public int? CantidadAsociados { get; set; }
            public decimal AjusteAsociados { get; set; }=0;
            public decimal TotalAsociadosAjustado { get; set; } = 0;
            public string Mensaje { get; set; } = string.Empty;
        }

        public class CrdPreCalculoGridRequest
        {
            public string CodTipoActividad { get; set; } = string.Empty;
            public decimal TotalAsociadosAjustado { get; set; } = 0;
            public long Operacion { get; set; } = 0;
            public string ComiteId { get; set; } = string.Empty;
        }

        public class CrdPreCalculoActividadGridItem
        {
            public string Cod_Actividad { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public decimal Monto { get; set; } = 0;
            public string Tipo { get; set; } = string.Empty;
            public bool Asignado { get; set; } = false;
        }

        public class CrdPreCalculoGridResponse
        {
            public List<CrdPreCalculoActividadGridItem> Actividades { get; set; } = [];
            public decimal MontoTotalAsignado { get; set; } = 0;
        }

        public class CrdPreCalculoPantallaInicialResponse
        {
            public string FechaRegistro { get; set; } = string.Empty;
            public List<DropDownListaGenericaModel> TiposActividad { get; set; } = [];
        }
    }
}
