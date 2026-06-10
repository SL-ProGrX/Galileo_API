using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPParametrosBL
    {
        private readonly FrmCxPParametrosDB _db;

        public FrmCxPParametrosBL(IConfiguration config)
        {
            _db = new FrmCxPParametrosDB(config);
        }

        public ErrorDto ExecParametros(int CodCliente)
        {
            return _db.ExecParametros(CodCliente);
        }

        public ErrorDto<List<ParametrosDto>> ObtenerParametros(int CodEmpresa)
        {
            return _db.ObtenerParametros(CodEmpresa);
        }

        public ErrorDto ActualizarParametros(int CodCliente, string Usuario, string Valor, string Parametro)
        {
            return _db.ActualizarDatosParametro(CodCliente, Usuario, Valor, Parametro);
        }
    }
}