using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic
{
    public class FrmSysRaAutorizadoresBL(IConfiguration config)
    {
        private readonly FrmSysRaAutorizadoresDB _db = new FrmSysRaAutorizadoresDB(config);

        public ErrorDto<int> Frm_Sys_Ra_Autorizadores_ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            return _db.Frm_Sys_Ra_Autorizadores_ConsultaAscDesc(CodEmpresa, consecutivo, tipo);
        }

        public ErrorDto<AutorizadoresExpDto> Frm_Sys_Ra_Autorizadores_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            return _db.Frm_Sys_Ra_Autorizadores_Obtener(CodEmpresa, Cod_Proveedor);
        }


        public ErrorDto Frm_Sys_Ra_Autorizadores_Insertar(int CodCliente, AutorizadoresExpDto autorizador)
        {
            return _db.Frm_Sys_Ra_Autorizadores_Insertar(CodCliente, autorizador);
        }

        public ErrorDto Frm_Sys_Ra_Autorizadores_Actualizar(int CodEmpresa, AutorizadoresExpDto request)
        {
            return _db.Frm_Sys_Ra_Autorizadores_Actualizar(CodEmpresa, request);
        }

        public ErrorDto<List<AutorizadoresExpDto>> Frm_Sys_Ra_AutorizadoresLista_Obtener(int codEmpresa)
        {
            return _db.Frm_Sys_Ra_AutorizadoresLista_Obtener(codEmpresa);
        }

    }
}