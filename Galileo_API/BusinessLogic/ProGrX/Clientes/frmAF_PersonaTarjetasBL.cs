using Newtonsoft.Json;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFPersonaTarjetasBL
    {
        private readonly FrmAFPersonaTarjetasDB _db;

        public FrmAFPersonaTarjetasBL(IConfiguration config)
        {
            _db = new FrmAFPersonaTarjetasDB(config);
        }

        public ErrorDto<List<PersonaTarjetaDto>> AF_PersonaTarjetas_Consulta(int CodEmpresa, string cedula)
        {
            return _db.AF_PersonaTarjetas_Consulta(CodEmpresa, cedula);
        }

        public ErrorDto AF_PersonaTarjetas_Registro(int CodEmpresa, PersonaTarjetaRegistroDto tarjeta)
        {
            return _db.AF_PersonaTarjetas_Registro(CodEmpresa, tarjeta);
        }

        public static ErrorDto<string> AF_PersonaTarjetas_ValidaTipo(string Tarjeta)
        {
            return FrmAFPersonaTarjetasDB.AF_PersonaTarjetas_ValidaTipo(Tarjeta);
        }
    }
}