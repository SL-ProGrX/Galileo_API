using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo.Models;


namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOPrincipalBL
    {
        private readonly FrmCOPrincipalDB _db;

        public FrmCOPrincipalBL(IConfiguration config)
        {
            _db = new FrmCOPrincipalDB(config);
        }

        public ErrorDto<List<OperacionBusquedaDto>> Operaciones_Listar(int codEmpresa)
        {
            return _db.Operaciones_Listar(codEmpresa);
        }

        public ErrorDto<OperacionConsultarDto> Operacion_Consultar(int codEmpresa, int operacion)
        {
            return _db.Operacion_Consultar(codEmpresa, operacion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Deductoras_Listar(int codEmpresa, int codInstitucion)
        {
            return _db.Deductoras_Listar(codEmpresa, codInstitucion);
        }
    }
}