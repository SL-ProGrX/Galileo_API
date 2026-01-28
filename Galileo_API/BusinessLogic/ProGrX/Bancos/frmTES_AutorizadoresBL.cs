using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos.Autorizadores;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesAutorizadoresBL
    {
        private readonly FrmTesAutorizadoresDB _Db;

        public FrmTesAutorizadoresBL(IConfiguration config)
        {
            _Db = new FrmTesAutorizadoresDB(config);
        }

        public ErrorDto<TesAutorizadoresLista> Tes_AutorizadoresUsuarioLista_Obtener(int CodEmpresa, string parametros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();
            return _Db.Tes_AutorizadoresUsuarioLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<TesAutorizadoresDto> Tes_AutorizadoresUsuarioBuscar_scroll(int CodEmpresa, string nombre, int? scroll)
        {
            return _Db.Tes_AutorizadoresUsuarioBuscar_scroll(CodEmpresa, nombre, scroll);
        }

        public ErrorDto<TesAutorizadoresDto> Tes_AutorizadoresUsuario_Obtener(int CodEmpresa, string nombre)
        {
            return _Db.Tes_AutorizadoresUsuario_Obtener(CodEmpresa, nombre);
        }

        public ErrorDto Tes_Autorizadores_Guardar(int CodEmpresa, string usuario, TesAutorizadoresDto autorizador)
        {
            return _Db.Tes_Autorizadores_Guardar(CodEmpresa, usuario, autorizador);
        }

        public ErrorDto Tes_Autorizadores_Eliminar(int CodEmpresa, string nombre, string usuario)
        {
            return _Db.Tes_Autorizadores_Eliminar(CodEmpresa, nombre, usuario);
        }
    }
}
