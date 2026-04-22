using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo_API.DataBaseTier.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasSesionBl
    {
        readonly FrmCajasSesionDb DbfrmCajas_Sesion;

        public FrmCajasSesionBl(IConfiguration config)
        {
            DbfrmCajas_Sesion = new FrmCajasSesionDb(config);
        }

        public ErrorDto<CajasSesionDto> Cajas_Sesion_Obtener(int codEmpresa, string usuario, string identificacion)
        {
            return DbfrmCajas_Sesion.Cajas_Sesion_Obtener(codEmpresa, usuario, identificacion);
        }
        public ErrorDto Cajas_Sesion_Inicia(int codEmpresa, string caja, string usuario, int tipoId, string cedula, string nombre)
        {
            return DbfrmCajas_Sesion.Cajas_Sesion_Inicia(codEmpresa, caja, usuario, tipoId, cedula, nombre);
        }

        public ErrorDto Cajas_Sesion_Finaliza(int codEmpresa, int sesionId, string usuario)
        {
            return DbfrmCajas_Sesion.Cajas_Sesion_Finaliza(codEmpresa, sesionId, usuario);
        }

        public ErrorDto<List<CajasSesionMovimientosDto>> Cajas_Sesion_Movimientos(int codEmpresa, int sesionId)
        {
            return DbfrmCajas_Sesion.Cajas_Sesion_Movimientos(codEmpresa, sesionId);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodCliente)
        {
            return DbfrmCajas_Sesion.TiposIdentificacion_Obtener(CodCliente);
        }
    }
}