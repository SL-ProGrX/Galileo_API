using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX.Cobros;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivDesembolsosBl
    {
        private readonly FrmVivDesembolsosDb _db;

        public FrmVivDesembolsosBl(IConfiguration config)
            => _db = new FrmVivDesembolsosDb(config);


        public ErrorDto<List<OperacionBusquedaDto>> Operaciones_Listar(int codEmpresa)
        {
            return _db.Operaciones_Listar(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Lineas_Listar(int codEmpresa)
        {
            return _db.Lineas_Listar(codEmpresa);
        }

        public ErrorDto<VivDesembolsoHeaderDto> Desembolso_Consultar(int codEmpresa, int operacion)
        {
            return _db.Desembolso_Consultar(codEmpresa, operacion);
        }

        public ErrorDto<List<VivDesembolsoDto>> Desembolsos_Listar(int codEmpresa, int operacion)
        {
            return _db.Desembolsos_Listar(codEmpresa, operacion);
        }

        public ErrorDto<List<VivDesembolsoPendienteDto>> Pendientes_Listar(int codEmpresa, int operacion)
        {
            return _db.Pendientes_Listar(codEmpresa, operacion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Bancos_Listar(int codEmpresa, string usuario)
        {
            return _db.Bancos_Listar(codEmpresa, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cuentas_Listar(int codEmpresa, string bancoId)
        {
            return _db.Cuentas_Listar(codEmpresa, bancoId);
        }

        public ErrorDto<List<ConceptoApiDto>> Conceptos_Listar(int codEmpresa)
        {
            return _db.Conceptos_Listar(codEmpresa);
        }

        public ErrorDto<bool> PermiteDesembolso(int codEmpresa,int operacion,int index)
        {
            return _db.PermiteDesembolso(codEmpresa, operacion, index);
        }
    }
}
