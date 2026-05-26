using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrGarantiasPatrimonialesListaResult
    {
        public int total { get; set; }
        public List<CrGarantiasPatrimonialesData> lista { get; set; } = new();
    }
    public class FiltrosGarantiasPatrimoniales : FiltrosLazyLoadData
    {
        public string garantia { get; set; } = string.Empty;
        public string cod_estado { get; set; } = string.Empty;
    }
    public class CrGarantiasPatrimonialesData
    {
        public int linea_id { get; set; }
        public int cod_operadora { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int membresia_inicio { get; set; }
        public int membresia_corte { get; set; }
        public decimal porcentaje { get; set; }
        public bool patrimonio { get; set; }
        public string patrimonio_descripcion { get; set; } = string.Empty;
    }

    public class CrGarantiasPatrimonialesRegistroRequest
    {
        public string garantia { get; set; } = string.Empty;
        public string cod_estado { get; set; } = string.Empty;
        public int? linea_id { get; set; }
        public int? membresia_inicio { get; set; }
        public int? membresia_corte { get; set; }
        public bool? patrimonio { get; set; }
        public int? cod_operadora { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public decimal? porcentaje { get; set; }
    }
}