using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFDistribucionPoliticaBL
    {
        private readonly FrmAFDistribucionPoliticaDB _db;

        public FrmAFDistribucionPoliticaBL(IConfiguration config)
        {
            _db = new FrmAFDistribucionPoliticaDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_DistribucionPolitica_Provincias_Obtener(int CodEmpresa)
        {
            return _db.AF_DistribucionPolitica_Provincias_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_DistribucionPolitica_Cantones_Obtener(int CodEmpresa, string Provincia)
        {
            return _db.AF_DistribucionPolitica_Cantones_Obtener(CodEmpresa, Provincia);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_DistribucionPolitica_Distritos_Obtener(int CodEmpresa, string Provincia, string Canton)
        {
            return _db.AF_DistribucionPolitica_Distritos_Obtener(CodEmpresa, Provincia, Canton);
        }

        public ErrorDto AF_DistribucionPolitica_Guardar(int CodEmpresa, string Usuario, AfDistribucionesDto Info)
        {
            return _db.AF_DistribucionPolitica_Guardar(CodEmpresa, Usuario, Info);
        }
    }
}