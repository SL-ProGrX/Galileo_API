using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPTrasladoAsientosBL
    {
        private readonly FrmCxPTrasladoAsientosDB _db;

        public FrmCxPTrasladoAsientosBL(IConfiguration config)
        {
            _db = new FrmCxPTrasladoAsientosDB(config);
        }

        public ErrorDto<DocsPendientesTraslado> DocPendientes_Obtener(int CodEmpresa, string Inicio, string Corte)
        {
            return _db.DocPendientes_Obtener(CodEmpresa, Inicio, Corte);
        }

        public ErrorDto<List<Desbalanceado>> Desbalanceados_Obtener(int CodEmpresa, string Inicio, string Corte)
        {
            return _db.Desbalanceados_Obtener(CodEmpresa, Inicio, Corte);
        }

        public ErrorDto Reactivar(int CodEmpresa, string Inicio, string Corte)
        {
            return _db.Reactivar(CodEmpresa, Inicio, Corte);
        }

        public bool fxValidaPeriodoAsiento(int CodEmpresa, string Fecha)
        {
            return _db.fxValidaPeriodoAsiento(CodEmpresa, Fecha);
        }

        public ErrorDto CasosCero_Borrar(int CodEmpresa)
        {
            return _db.CasosCero_Borrar(CodEmpresa);
        }

        public ErrorDto AsientoIndividual_Procesar(int CodEmpresa, int cod_contabilidad, AsientoInfo data)
        {
            return _db.AsientoIndividual_Procesar(CodEmpresa, cod_contabilidad, data);
        }
    }
}