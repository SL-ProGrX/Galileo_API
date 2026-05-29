using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFRemesasIngresosBL
    {
        private readonly FrmAFRemesasIngresosDB _db;

        public FrmAFRemesasIngresosBL(IConfiguration config)
        {
            _db = new FrmAFRemesasIngresosDB(config);
        }

        public ErrorDto<List<AdiRemesaIngDto>> AFI_Remesas_Obtener(int CodEmpresa)
        {
            return _db.AFI_Remesas_Obtener(CodEmpresa);
        }

        public ErrorDto AFI_Remesa_Eliminar(int codEmpresa, string codRemesa)
        {
            return _db.AFI_Remesa_Eliminar(codEmpresa, codRemesa);
        }

        public ErrorDto AFI_Remesa_Registrar(int codEmpresa, AdiRemesaIngRequestDto request)
        {
            return _db.AFI_Remesa_Registrar(codEmpresa, request);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_RemesaAbiertas_Obtener(int CodEmpresa)
        {
            return _db.AF_RemesaAbiertas_Obtener(CodEmpresa);
        }

        public ErrorDto<List<IngresosPendientesDto>> AFI_IngresosPendientes_Obtener(int codEmpresa, string codRemesa, string oficina = "")
        {
            return _db.AFI_IngresosPendientes_Obtener(codEmpresa, codRemesa, oficina);
        }

        public ErrorDto AFI_Remesa_Cerrar(int codEmpresa, int codRemesa)
        {
            return _db.AFI_Remesa_Cerrar(codEmpresa, codRemesa);
        }

        public ErrorDto AFI_Remesa_Cargar(int codEmpresa, int codRemesa, List<int> ingresosSeleccionados)
        {
            return _db.AFI_Remesa_Cargar(codEmpresa, codRemesa, ingresosSeleccionados);
        }


        public ErrorDto<List<RemesaConsultaDto>> AFI_RemesaPorCedula_Obtener(int codEmpresa, string cedula)
        {
            return _db.AFI_RemesaPorCedula_Obtener(codEmpresa, cedula);
        }
    }
}