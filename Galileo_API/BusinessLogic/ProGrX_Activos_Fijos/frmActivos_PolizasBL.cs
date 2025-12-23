using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosPolizasBL
    {
        private readonly FrmActivosPolizasDb _db;

        public FrmActivosPolizasBL(IConfiguration config)
        {
            _db = new FrmActivosPolizasDb(config);
        }

        public ErrorDto<ActivosPolizasLista> Activos_PolizasLista_Obtener(int CodEmpresa, string jfiltros)
        {

            return _db.Activos_PolizasLista_Obtener(CodEmpresa, jfiltros);
        }

        public ErrorDto<ActivosPolizasData?> Activos_Polizas_Obtener(int CodEmpresa, string cod_poliza)
        {
            return _db.Activos_Polizas_Obtener(CodEmpresa, cod_poliza);
        }
        public ErrorDto Activos_Polizas_Guardar(int CodEmpresa, ActivosPolizasData poliza)
        {
            return _db.Activos_Polizas_Guardar(CodEmpresa, poliza);
        }
        public ErrorDto Activos_Polizas_Eliminar(int CodEmpresa, string usuario, string cod_poliza)
        {
            return _db.Activos_Polizas_Eliminar(CodEmpresa, usuario, cod_poliza);
        }

        public ErrorDto Activos_Polizas_Valida(int CodEmpresa, string cod_poliza)
        {
            return _db.Activos_PolizasExiste_Obtener(CodEmpresa, cod_poliza);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Polizas_Tipos_Listar(int CodEmpresa)
        {
            return _db.Activos_Polizas_Tipos_Listar(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Tipo_Activo_Listar(int CodEmpresa)
        {
            return _db.Activos_Tipo_Activo_Listar(CodEmpresa);
        }
        public ErrorDto<ActivosPolizasLista> Activos_Polizas_Asignacion_Listar(int CodEmpresa,string cod_poliza,string? tipo_activo,string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_Polizas_Asignacion_Listar(CodEmpresa, cod_poliza, tipo_activo, filtros);
        }
        public ErrorDto<List<ActivosPolizasAsignacionItem>> Activos_Polizas_Asignacion_Listar_Export(int CodEmpresa,string cod_poliza,string? tipo_activo,string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_Polizas_Asignacion_Listar_Export(CodEmpresa, cod_poliza, tipo_activo, filtros);
        }
        public ErrorDto Activos_Polizas_Asignar(int CodEmpresa, string usuario, string cod_poliza, List<string> placas)
        {
            return _db.Activos_Polizas_Asignar(CodEmpresa, usuario, cod_poliza, placas);
        }

        public ErrorDto Activos_Polizas_Desasignar(int CodEmpresa, string usuario, string cod_poliza, List<string> placas)
        {
            return _db.Activos_Polizas_Desasignar(CodEmpresa, usuario, cod_poliza, placas);
        }
    }
}
