using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cajas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasUsuariosBL
    {
        private readonly FrmCajasUsuariosDB _db;
        public FrmCajasUsuariosBL(IConfiguration config)
        {
            _db = new FrmCajasUsuariosDB(config);
        }
        public ErrorDto<List<CajasUsuariosListadoUsuarioData>> Cajas_Usuarios_Lista_Obtener(int CodEmpresa, string jfiltros, bool soloAsignados)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Cajas_Usuarios_Lista_Obtener(CodEmpresa, filtros, soloAsignados);
        }

        public ErrorDto Cajas_Usuarios_Guardar(int CodEmpresa, string usuarioSesion, CajasUsuariosData usuarioCaja)
        {
            return _db.Cajas_Usuarios_Guardar(CodEmpresa, usuarioSesion, usuarioCaja);
        }

        public ErrorDto Cajas_Usuarios_Eliminar(int CodEmpresa, string usuarioSesion, string cod_caja, string usuarioCaja)
        {
            return _db.Cajas_Usuarios_Eliminar(CodEmpresa, usuarioSesion, cod_caja, usuarioCaja);
        }

        public ErrorDto<List<CajasUsuariosHistData>> Cajas_Usuarios_Historico_Obtener(int CodEmpresa, string cod_caja, string usuarioCaja)
        {
            return _db.Cajas_Usuarios_Historico_Obtener(CodEmpresa, cod_caja, usuarioCaja);
        }
        public ErrorDto<List<CajasUsuariosCajaListaData>> Cajas_Usuarios_Cajas_Lista_Obtener(int CodEmpresa,string usuarioCaja)
        {
            return _db.Cajas_Usuarios_Cajas_Lista_Obtener(CodEmpresa, usuarioCaja);
        }
    }
}