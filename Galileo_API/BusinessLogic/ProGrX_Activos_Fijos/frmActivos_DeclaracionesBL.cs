using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessTier.ProGrX_Activos_Fijos
{
    public class FrmActivosDeclaracionesBL
    {
        private readonly FrmActivosDeclaracionesDB _db;

        public FrmActivosDeclaracionesBL(IConfiguration config)
        {
            _db = new FrmActivosDeclaracionesDB(config);
        }

        public ErrorDto<ActivosDeclaracionLista> Activos_Declaraciones_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_Declaraciones_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<ActivosDeclaracion> Activos_Declaraciones_Registro_Obtener(int CodEmpresa, int id_declara)
        {
            return _db.Activos_Declaraciones_Registro_Obtener(CodEmpresa, id_declara);
        }

        public ErrorDto<ActivosDeclaracionResult> Activos_Declaraciones_Registro_Guardar(int CodEmpresa, ActivosDeclaracionGuardarRequest data)
        {
            return _db.Activos_Declaraciones_Registro_Guardar(CodEmpresa, data);
        }

        public ErrorDto Activos_Declaraciones_Registro_Eliminar(int CodEmpresa, int id_declara, string usuario) 
        { 
           return _db.Activos_Declaraciones_Registro_Eliminar(CodEmpresa, id_declara, usuario);
        } 
        public ErrorDto Activos_Declaraciones_Registro_Cerrar(int CodEmpresa,int id_declara,string usuario)
        {
            return _db.Activos_Declaraciones_Registro_Cerrar(CodEmpresa, id_declara, usuario);
        }

        public ErrorDto Activos_Declaraciones_Registro_Procesar(int CodEmpresa, int id_declara, string usuario)
        {
           return _db.Activos_Declaraciones_Registro_Procesar(CodEmpresa, id_declara, usuario);
        }
        public ErrorDto<ActivosDeclaracion> Activos_Declaraciones_Registro_Scroll(int CodEmpresa, int scroll, int? id_declara, string usuario)
        {
            return _db.Activos_Declaraciones_Registro_Scroll(CodEmpresa, scroll, id_declara, usuario);
        }
    }
}