using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmFndTrasladoFondosBL
    {
        private readonly FrmFndTrasladoFondosDB _db;

        public FrmFndTrasladoFondosBL(IConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _db = new FrmFndTrasladoFondosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Operadoras_Obtener(int CodEmpresa)
        {
            return _db.Fnd_Operadoras_Obtener(CodEmpresa);
        }

        public ErrorDto<List<FndTrasladoSocioSimple>> Fnd_Traslado_Socios_Obtener(int CodEmpresa, string ordenarPor)
        {
            return _db.Fnd_Traslado_Socios_Obtener(CodEmpresa, ordenarPor);
        }

        public ErrorDto<List<FndTrasladoContratoDisponible>> Fnd_Traslado_ContratosDisponibles_Obtener(int CodEmpresa, string codOperadora, string? cedula)
        {
            return _db.Fnd_Traslado_ContratosDisponibles_Obtener(CodEmpresa, codOperadora, cedula);
        }

        public ErrorDto<FndTrasladoFondosResult> Fnd_TrasladoFondos_Ejecutar(int CodEmpresa, FndTrasladoFondosRequest request)
        {
            return _db.Fnd_TrasladoFondos_Ejecutar(CodEmpresa, request);
        }
    }
}